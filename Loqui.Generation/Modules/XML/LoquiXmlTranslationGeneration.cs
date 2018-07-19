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
            string writerAccessor,
            Accessor itemAccessor,
            string maskAccessor,
            string nameAccessor,
            string translationMaskAccessor)
        {
            var loquiGen = typeGen as LoquiType;
            using (var args = new ArgsWrapper(fg,
                $"LoquiXmlTranslation<{loquiGen.TypeName}>.Instance.Write"))
            {
                args.Add($"node: {writerAccessor}");
                args.Add($"item: {itemAccessor.PropertyOrDirectAccess}");
                args.Add($"name: {nameAccessor}");
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                }
                args.Add($"errorMask: {maskAccessor}");
                args.Add($"translationMask: {translationMaskAccessor}");
            }
        }

        public override bool ShouldGenerateCopyIn(TypeGeneration typeGen)
        {
            var loquiGen = typeGen as LoquiType;
            return loquiGen.SingletonType != SingletonLevel.Singleton || loquiGen.InterfaceType != LoquiInterfaceType.IGetter;
        }

        public override void GenerateCopyIn(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor itemAccessor,
            string maskAccessor,
            string translationMaskAccessor)
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
                                    subArgs.Add($"root: {nodeAccessor}");
                                    subArgs.Add($"errorMask: {maskAccessor}");
                                    subArgs.Add($"translationMask: {translationMaskAccessor}");
                                }
                            });
                            args.Add("def: null");
                            args.Add("cmds: null");
                            args.Add("copyMask: null");
                            args.Add($"errorMask: {maskAccessor}");
                        }
                    },
                    maskAccessor: "errorMask",
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
                    maskAccessor: maskAccessor);
            }
        }

        public void GenerateCopyInRet_Internal(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor itemAccessor,
            bool ret,
            string indexAccessor,
            string maskAccessor,
            string translationMaskAccessor)
        {
            var loquiGen = typeGen as LoquiType;
            TranslationGeneration.WrapParseCall(
                fg: fg,
                typeGen: typeGen,
                translatorLine: $"LoquiXmlTranslation<{loquiGen.ObjectTypeName}{loquiGen.GenericTypes}>.Instance",
                maskAccessor: maskAccessor,
                indexAccessor: indexAccessor,
                itemAccessor: itemAccessor,
                translationMaskAccessor: $"{translationMaskAccessor}?.GetSubCrystal({typeGen.IndexEnumInt})",
                unsetCall: null,
                extraargs: new string[]
                {
                    $"root: {nodeAccessor}",
                });
        }

        public override void GenerateCopyInRet(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor retAccessor,
            string indexAccessor,
            string maskAccessor,
            string translationMaskAccessor)
        {
            GenerateCopyInRet_Internal(
                fg: fg,
                typeGen: typeGen,
                nodeAccessor: nodeAccessor,
                itemAccessor: retAccessor,
                ret: true,
                indexAccessor: indexAccessor,
                maskAccessor: maskAccessor,
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
            rootElement.AddFirst(importElem);
            var elem = new XElement(
                XmlTranslationModule.XSDNamespace + "element",
                new XAttribute("name", loqui.TargetObjectGeneration.Name));
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
            return null;
        }

        public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
        {
        }
    }
}
