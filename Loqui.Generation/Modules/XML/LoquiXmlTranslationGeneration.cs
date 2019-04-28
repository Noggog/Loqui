using Loqui.Xml;
using Noggog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class LoquiXmlTranslationGeneration : XmlTranslationGeneration
    {
        public override string GetTranslatorInstance(TypeGeneration typeGen)
        {
            var loquiGen = typeGen as LoquiType;
            return $"LoquiXmlTranslation<{loquiGen.TypeName}>.Instance";
        }

        public override void GenerateWrite(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            Accessor writerAccessor,
            Accessor itemAccessor,
            Accessor errorMaskAccessor,
            Accessor nameAccessor,
            Accessor translationMaskAccessor)
        {
            var loquiGen = typeGen as LoquiType;
            string callString;
            if (loquiGen.RefType == LoquiType.LoquiRefType.Interface)
            {
                callString = $"LoquiXmlTranslation.Instance.Write<{loquiGen.TypeName}>";
            }
            else
            {
                callString = $"LoquiXmlTranslation<{loquiGen.TypeName}>.Instance.Write";
            }
            using (var args = new ArgsWrapper(fg,
                callString))
            {
                args.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {writerAccessor}");
                args.Add($"item: {itemAccessor.DirectAccess}");
                args.Add($"name: {nameAccessor}");
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                }
                args.Add($"errorMask: {errorMaskAccessor}");
                if (this.XmlMod.TranslationMaskParameter)
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

        public override bool ShouldGenerateCopyIn(TypeGeneration typeGen)
        {
            var loquiGen = typeGen as LoquiType;
            return loquiGen.SingletonType != SingletonLevel.Singleton || loquiGen.InterfaceType != LoquiInterfaceType.IGetter;
        }

        public override void GenerateCopyIn(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            Accessor nodeAccessor,
            Accessor itemAccessor,
            Accessor errorMaskAccessor,
            Accessor translationMaskAccessor)
        {
            var loquiGen = typeGen as LoquiType;
            if (loquiGen.SingletonType == SingletonLevel.Singleton)
            {
                if (loquiGen.InterfaceType == LoquiInterfaceType.IGetter) return;
                MaskGenerationUtility.WrapErrorFieldIndexPush(
                    fg,
                    () =>
                    {
                        using (var args = new ArgsWrapper(fg,
                            $"{itemAccessor.DirectAccess}.CopyFieldsFrom{loquiGen.GetGenericTypes(MaskType.Copy)}"))
                        {
                            args.Add((gen) =>
                            {
                                using (var subArgs = new FunctionWrapper(gen,
                                    $"rhs: {loquiGen.TargetObjectGeneration.Name}{loquiGen.GenericTypes}.Create_Xml"))
                                {
                                    subArgs.Add($"{XmlTranslationModule.XElementLine.GetParameterName(loquiGen.TargetObjectGeneration)}: {nodeAccessor}");
                                    foreach (var item in this.XmlMod.MainAPI.ReaderAPI.CustomAPI)
                                    {
                                        if (!item.TryGetPassthrough(objGen, loquiGen.TargetObjectGeneration, out var passthrough)) continue;
                                        subArgs.Add(passthrough);
                                    }
                                    subArgs.Add($"errorMask: {errorMaskAccessor}");
                                    subArgs.Add($"translationMask: {translationMaskAccessor}");
                                }
                            });
                            args.Add("def: null");
                            args.Add("copyMask: null");
                            args.Add($"errorMask: {errorMaskAccessor}");
                        }
                    },
                    errorMaskAccessor: "errorMask",
                    indexAccessor: $"{typeGen.IndexEnumInt}");
            }
            else
            {
                if (!typeGen.HasIndex)
                {
                    throw new NotImplementedException();
                }
                GenerateCopyInRet_Internal(
                    fg: fg,
                    typeGen: typeGen,
                    nodeAccessor: nodeAccessor,
                    itemAccessor: itemAccessor,
                    ret: false,
                    translationMaskAccessor: translationMaskAccessor,
                    indexAccessor: $"(int){typeGen.IndexEnumName}",
                    errorMaskAccessor: errorMaskAccessor);
            }
        }

        public void GenerateCopyInRet_Internal(
            FileGeneration fg,
            TypeGeneration typeGen,
            Accessor nodeAccessor,
            Accessor itemAccessor,
            bool ret,
            Accessor indexAccessor,
            Accessor errorMaskAccessor,
            Accessor translationMaskAccessor)
        {
            var loquiGen = typeGen as LoquiType;
            TranslationGeneration.WrapParseCall(
                new TranslationWrapParseArgs()
                {
                    FG = fg,
                    TypeGen = typeGen,
                    TranslatorLine = $"LoquiXmlTranslation<{loquiGen.ObjectTypeName}{loquiGen.GenericTypes}>.Instance",
                    MaskAccessor = errorMaskAccessor,
                    IndexAccessor = indexAccessor,
                    ItemAccessor = itemAccessor,
                    TranslationMaskAccessor = $"{translationMaskAccessor}?.GetSubCrystal({typeGen.IndexEnumInt})",
                    ExtraArgs = new string[]
                    {
                        $"{XmlTranslationModule.XElementLine.GetParameterName(loquiGen.TargetObjectGeneration)}: {nodeAccessor}"
                    }
                });
        }

        public override void GenerateCopyInRet(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            Accessor nodeAccessor,
            Accessor retAccessor,
            Accessor outItemAccessor,
            Accessor errorMaskAccessor,
            Accessor translationMaskAccessor)
        {
            GenerateCopyInRet_Internal(
                fg: fg,
                typeGen: typeGen,
                nodeAccessor: nodeAccessor,
                itemAccessor: retAccessor,
                ret: true,
                indexAccessor: null,
                errorMaskAccessor: errorMaskAccessor,
                translationMaskAccessor: translationMaskAccessor);
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
            var targetNamespace = this.XmlMod.ObjectNamespace(targetObject);
            var diffNamespace = !targetNamespace.Equals(this.XmlMod.ObjectNamespace(objGen));
            if (diffNamespace)
            {
                rootElement.Add(
                    new XAttribute(XNamespace.Xmlns + $"{targetObject.Name.ToLower()}", this.XmlMod.ObjectNamespace(targetObject)));
            }
            FilePath xsdPath = this.XmlMod.ObjectXSDLocation(targetObject);
            var relativePath = xsdPath.GetRelativePathTo(objGen.TargetDir);
            var importElem = new XElement(
                XmlTranslationModule.XSDNamespace + "include",
                new XAttribute("schemaLocation", relativePath));
            if (diffNamespace
                && !rootElement.Elements().Any((e) => e.ContentEqual(importElem)))
            {
                importElem.Add(new XAttribute("namespace", this.XmlMod.ObjectNamespace(targetObject)));
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
}
