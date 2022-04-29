using Loqui.Xml;
using Noggog;
using System.Xml.Linq;

namespace Loqui.Generation;

public class LoquiXmlTranslationGeneration : XmlTranslationGeneration
{
    public override string GetTranslatorInstance(TypeGeneration typeGen, bool getter)
    {
        var loquiGen = typeGen as LoquiType;
        return $"LoquiXmlTranslation<{loquiGen.TypeName(getter)}>.Instance";
    }

    public override void GenerateWrite(
        StructuredStringBuilder sb,
        ObjectGeneration objGen,
        TypeGeneration typeGen,
        Accessor writerAccessor,
        Accessor itemAccessor,
        Accessor errorMaskAccessor,
        Accessor nameAccessor,
        Accessor translationMaskAccessor)
    {
        using (sb.CurlyBrace(doIt: !XmlMod.TranslationMaskParameter))
        {
            if (typeGen.Nullable)
            {
                sb.AppendLine($"if ({itemAccessor.Access} is {{}} {typeGen.Name}Item)");
                itemAccessor = $"{typeGen.Name}Item";
            }
            else
            {
                // We want to cache retrievals, in case it's a wrapper being written
                sb.AppendLine($"var {typeGen.Name}Item = {itemAccessor.Access};");
                itemAccessor = $"{typeGen.Name}Item";
            }

            using (sb.CurlyBrace(doIt: typeGen.Nullable))
            {
                var loquiGen = typeGen as LoquiType;
                string line;
                if (loquiGen.TargetObjectGeneration != null)
                {
                    line = $"(({XmlMod.TranslationWriteClassName(loquiGen.TargetObjectGeneration)})(({nameof(IXmlItem)}){typeGen.Name}Item).{XmlMod.TranslationWriteItemMember})";
                }
                else
                {
                    line = $"(({XmlMod.TranslationWriteInterface})(({nameof(IXmlItem)}){typeGen.Name}Item).{XmlMod.TranslationWriteItemMember})";
                }
                using (var args = sb.Args( $"{line}.Write{loquiGen.GetGenericTypes(getter: true, additionalMasks: new MaskType[] { MaskType.Normal })}"))
                {
                    args.Add($"item: {typeGen.Name}Item");
                    args.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {writerAccessor}");
                    args.Add($"name: {nameAccessor}");
                    if (typeGen.HasIndex)
                    {
                        args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                    }
                    args.Add($"errorMask: {errorMaskAccessor}");
                    if (XmlMod.TranslationMaskParameter)
                    {
                        if (typeGen.HasIndex)
                        {
                            args.Add($"translationMask: {translationMaskAccessor}?.GetSubCrystal({typeGen.IndexEnumInt})");
                        }
                        else
                        {
                            args.Add($"translationMask: {translationMaskAccessor}");
                        }
                    }
                }
            }
        }
    }

    public override bool ShouldGenerateCopyIn(TypeGeneration typeGen)
    {
        var loquiGen = typeGen as LoquiType;
        return !loquiGen.Singleton || loquiGen.SetterInterfaceType != LoquiInterfaceType.IGetter;
    }

    public override void GenerateCopyIn(
        StructuredStringBuilder sb,
        ObjectGeneration objGen,
        TypeGeneration typeGen,
        Accessor nodeAccessor,
        Accessor itemAccessor,
        Accessor errorMaskAccessor,
        Accessor translationMaskAccessor)
    {
        var loquiGen = typeGen as LoquiType;
        if (loquiGen.Singleton)
        {
            if (loquiGen.SetterInterfaceType == LoquiInterfaceType.IGetter) return;
            MaskGenerationUtility.WrapErrorFieldIndexPush(
                sb,
                () =>
                {
                    using (var args = sb.Args(
                               $"{itemAccessor.Access}.{XmlMod.CopyInFromPrefix}{XmlMod.ModuleNickname}{loquiGen.GetGenericTypes(getter: false, MaskType.Normal)}"))
                    {
                        args.Add($"node: {nodeAccessor}");
                        args.Add($"translationMask: {translationMaskAccessor}");
                        args.Add($"errorMask: {errorMaskAccessor}");
                    }
                },
                errorMaskAccessor: "errorMask",
                indexAccessor: $"{typeGen.IndexEnumInt}");
        }
        else
        {
            GenerateCopyInRet_Internal(
                sb: sb,
                objGen: objGen,
                typeGen: typeGen,
                nodeAccessor: nodeAccessor,
                itemAccessor: itemAccessor,
                translationMaskAccessor: translationMaskAccessor,
                indexAccessor: typeGen.HasIndex ? $"(int){typeGen.IndexEnumName}" : null,
                errorMaskAccessor: errorMaskAccessor);
        }
    }

    public void GenerateCopyInRet_Internal(
        StructuredStringBuilder sb,
        ObjectGeneration objGen,
        TypeGeneration typeGen,
        Accessor nodeAccessor,
        Accessor itemAccessor,
        Accessor? indexAccessor,
        Accessor errorMaskAccessor,
        Accessor translationMaskAccessor)
    {
        var loquiGen = typeGen as LoquiType;
        WrapParseCall(
            new TranslationWrapParseArgs()
            {
                FG = sb,
                TypeGen = typeGen,
                TranslatorLine = $"LoquiXmlTranslation<{loquiGen.TypeName(LoquiInterfaceType.Direct)}>.Instance",
                MaskAccessor = errorMaskAccessor,
                IndexAccessor = indexAccessor,
                ItemAccessor = itemAccessor,
                Do = indexAccessor != null,
                TranslationMaskAccessor = translationMaskAccessor == null ? "null" : $"{ translationMaskAccessor}?.GetSubCrystal({typeGen.IndexEnumInt})",
                ExtraArgs = new string[]
                {
                    $"{XmlTranslationModule.XElementLine.GetParameterName(loquiGen.TargetObjectGeneration)}: {nodeAccessor}"
                }
            });
    }

    public override XElement GenerateForXSD(
        ObjectGeneration objGen,
        XElement rootElement,
        XElement choiceElement,
        TypeGeneration typeGen,
        string nameOverride = null)
    {
        LoquiType loqui = typeGen as LoquiType;
        var targetObject = loqui.TargetObjectGeneration;
        var targetNamespace = XmlMod.ObjectNamespace(targetObject);
        var diffNamespace = !targetNamespace.Equals(XmlMod.ObjectNamespace(objGen));
        if (diffNamespace)
        {
            rootElement.Add(
                new XAttribute(XNamespace.Xmlns + $"{targetObject.Name.ToLower()}", XmlMod.ObjectNamespace(targetObject)));
        }
        FilePath xsdPath = XmlMod.ObjectXSDLocation(targetObject);
        var relativePath = xsdPath.GetRelativePathTo(objGen.TargetDir);
        var importElem = new XElement(
            XmlTranslationModule.XSDNamespace + "include",
            new XAttribute("schemaLocation", relativePath));
        if (diffNamespace
            && !rootElement.Elements().Any((e) => e.ContentEqual(importElem)))
        {
            importElem.Add(new XAttribute("namespace", XmlMod.ObjectNamespace(targetObject)));
        }
        if (!rootElement.Elements().Any((e) => e.ContentEqual(importElem)))
        {
            rootElement.AddFirst(importElem);
        }
        var elem = new XElement(
            XmlTranslationModule.XSDNamespace + "element",
            new XAttribute("name", nameOverride ?? loqui.Name));
        if (diffNamespace)
        {
            elem.Add(
                new XAttribute("type", $"{targetObject.Name.ToLower()}:{loqui.TargetObjectGeneration.Name}Type"));
        }
        else
        {
            elem.Add(
                new XAttribute("type", $"{loqui.TargetObjectGeneration.Name}Type"));
        }
        choiceElement.Add(elem);
        return elem;
    }

    public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
    {
    }
}