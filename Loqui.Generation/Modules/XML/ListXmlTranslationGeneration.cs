using Loqui.Internal;
using System.Xml.Linq;

namespace Loqui.Generation;

public class ListXmlTranslationGeneration : XmlTranslationGeneration
{
    public virtual string TranslatorName => $"ListXmlTranslation";

    public override string GetTranslatorInstance(TypeGeneration typeGen, bool getter)
    {
        var list = typeGen as ListType;
        if (!XmlMod.TryGetTypeGeneration(list.SubTypeGeneration.GetType(), out var subTransl))
        {
            throw new ArgumentException("Unsupported type generator: " + list.SubTypeGeneration);
        }

        var subMaskStr = subTransl.MaskModule.GetMaskModule(list.SubTypeGeneration.GetType()).GetErrorMaskTypeStr(list.SubTypeGeneration);
        return $"{TranslatorName}<{list.SubTypeGeneration.TypeName(getter)}, {subMaskStr}>.Instance";
    }

    protected virtual string GetWriteAccessor(Accessor itemAccessor)
    {
        return itemAccessor.Access;
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
        var list = typeGen as ListType;
        if (!XmlMod.TryGetTypeGeneration(list.SubTypeGeneration.GetType(), out var subTransl))
        {
            throw new ArgumentException("Unsupported type generator: " + list.SubTypeGeneration);
        }

        var typeName = list.SubTypeGeneration.TypeName(getter: true, needsCovariance: true);
        if (list.SubTypeGeneration is LoquiType loqui)
        {
            typeName = loqui.TypeNameInternal(getter: true, internalInterface: true);
        }

        using (var args = sb.Args(
                   $"{TranslatorName}<{typeName}>.Instance.Write"))
        {
            args.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {writerAccessor}");
            args.Add($"name: {nameAccessor}");
            args.Add($"item: {GetWriteAccessor(itemAccessor)}");
            if (typeGen.HasIndex)
            {
                args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
            }
            else
            {
                throw new NotImplementedException();
            }
            args.Add($"errorMask: {errorMaskAccessor}");
            args.Add($"translationMask: {translationMaskAccessor}?.GetSubCrystal({typeGen.IndexEnumInt})");
            args.Add((gen) =>
            {
                var subTypeName = list.SubTypeGeneration.TypeName(getter: true, needsCovariance: true);
                if (list.SubTypeGeneration is LoquiType subLoqui)
                {
                    subTypeName = subLoqui.TypeNameInternal(getter: true, internalInterface: true);
                }
                gen.AppendLine($"transl: (XElement subNode, {subTypeName} subItem, ErrorMaskBuilder? listSubMask, {nameof(TranslationCrystal)}? listTranslMask) =>");
                using (new CurlyBrace(gen))
                {
                    subTransl.GenerateWrite(
                        sb: gen,
                        objGen: objGen,
                        typeGen: list.SubTypeGeneration,
                        writerAccessor: "subNode",
                        itemAccessor: new Accessor($"subItem"),
                        errorMaskAccessor: $"listSubMask",
                        translationMaskAccessor: "listTranslMask",
                        nameAccessor: "null");
                }
            });
            ExtraWriteArgs(itemAccessor, typeGen, args);
        }
    }

    protected virtual void ExtraWriteArgs(
        Accessor itemAccessor,
        TypeGeneration typeGen,
        Args args)
    {
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
        GenerateCopyInRet_Internal(
            sb: sb,
            objGen: objGen,
            typeGen: typeGen,
            nodeAccessor: nodeAccessor,
            itemAccessor: itemAccessor,
            ret: false,
            translationMaskAccessor: translationMaskAccessor,
            errorMaskAccessor: errorMaskAccessor);
    }

    public void GenerateCopyInRet_Internal(
        StructuredStringBuilder sb,
        ObjectGeneration objGen,
        TypeGeneration typeGen,
        Accessor nodeAccessor,
        Accessor itemAccessor,
        bool ret,
        Accessor errorMaskAccessor,
        Accessor translationMaskAccessor)
    {
        var list = typeGen as ListType;
        if (!XmlMod.TryGetTypeGeneration(list.SubTypeGeneration.GetType(), out var subTransl))
        {
            throw new ArgumentException("Unsupported type generator: " + list.SubTypeGeneration);
        }

        if (ret)
        {
            throw new NotImplementedException();
        }

        MaskGenerationUtility.WrapErrorFieldIndexPush(
            sb: sb,
            toDo: () =>
            {
                using (var args = new Function(
                           sb,
                           $"if ({TranslatorName}<{list.SubTypeGeneration.TypeName(getter: false, needsCovariance: true)}>.Instance.Parse"))
                {
                    args.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {XmlTranslationModule.XElementLine.GetParameterName(objGen)}");
                    args.Add($"enumer: out var {typeGen.Name}Item");
                    args.Add($"transl: {subTransl.GetTranslatorInstance(list.SubTypeGeneration, getter: false)}.Parse");
                    args.Add("errorMask: errorMask");
                    args.Add($"translationMask: {translationMaskAccessor})");
                }
                using (sb.CurlyBrace())
                {
                    if (typeGen.Nullable)
                    {
                        sb.AppendLine($"{itemAccessor.Access} = {typeGen.Name}Item.ToExtendedList();");
                    }
                    else
                    {
                        sb.AppendLine($"{itemAccessor.Access}.SetTo({typeGen.Name}Item);");
                    }
                }
                sb.AppendLine("else");
                using (sb.CurlyBrace())
                {
                    list.GenerateClear(sb, itemAccessor);
                }
            },
            errorMaskAccessor: errorMaskAccessor,
            indexAccessor: typeGen.IndexEnumInt);
    }

    public override XElement GenerateForXSD(
        ObjectGeneration objGen,
        XElement rootElement,
        XElement choiceElement,
        TypeGeneration typeGen,
        string nameOverride = null)
    {
        var elem = new XElement(XmlTranslationModule.XSDNamespace + "element",
            new XAttribute("name", nameOverride ?? typeGen.Name),
            new XAttribute("type", $"{typeGen.Name}Type"));
        choiceElement.Add(elem);

        var subChoice = new XElement(XmlTranslationModule.XSDNamespace + "choice",
            new XAttribute("minOccurs", 0),
            new XAttribute("maxOccurs", "unbounded"));
        rootElement.Add(
            new XElement(XmlTranslationModule.XSDNamespace + "complexType",
                new XAttribute("name", $"{typeGen.Name}Type"),
                subChoice));

        var list = typeGen as ListType;
        var xmlGen = XmlMod.GetTypeGeneration(list.SubTypeGeneration.GetType());
        var subElem = xmlGen.GenerateForXSD(
            objGen,
            rootElement,
            subChoice,
            list.SubTypeGeneration,
            nameOverride: "Item");
        return elem;
    }

    public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
    {
    }
}