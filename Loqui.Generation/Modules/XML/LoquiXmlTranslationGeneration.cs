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
        public override void GenerateWrite(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            string writerAccessor,
            Accessor itemAccessor,
            string doMaskAccessor,
            string maskAccessor,
            string nameAccessor)
        {
            var loquiGen = typeGen as LoquiType;
            if (loquiGen.TargetObjectGeneration != null)
            {
                using (var args = new ArgsWrapper(fg,
                    $"LoquiXmlTranslation<{loquiGen.TypeName}, {loquiGen.Mask(MaskType.Error)}>.Instance.Write"))
                {
                    args.Add($"writer: {writerAccessor}");
                    args.Add($"item: {itemAccessor.PropertyOrDirectAccess}");
                    args.Add($"name: {nameAccessor}");
                    if (typeGen.HasIndex)
                    {
                        args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                        args.Add($"errorMask: {maskAccessor}");
                    }
                    else
                    {
                        args.Add($"doMasks: {doMaskAccessor}");
                        args.Add($"errorMask: out {maskAccessor}");
                    }
                }
            }
            else
            {
                UnsafeXmlTranslationGeneration unsafeXml = new UnsafeXmlTranslationGeneration()
                {
                    ErrMaskString = $"MaskItem<Exception, {loquiGen.Mask(MaskType.Error)}>"
                };
                unsafeXml.GenerateWrite(
                    fg: fg,
                    objGen: objGen,
                    typeGen: typeGen,
                    writerAccessor: writerAccessor,
                    itemAccessor: itemAccessor,
                    doMaskAccessor: doMaskAccessor,
                    maskAccessor: maskAccessor,
                    nameAccessor: nameAccessor);
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
            string doMaskAccessor,
            string maskAccessor)
        {
            var loquiGen = typeGen as LoquiType;
            if (loquiGen.TargetObjectGeneration != null)
            {
                if (loquiGen.SingletonType == SingletonLevel.Singleton)
                {
                    if (loquiGen.InterfaceType == LoquiInterfaceType.IGetter) return;
                    using (var args = new ArgsWrapper(fg,
                        $"{loquiGen.TargetObjectGeneration.ExtCommonName}.CopyFieldsFrom{loquiGen.GetGenericTypes(MaskType.Normal, MaskType.Error, MaskType.Copy)}"))
                    {
                        args.Add($"item: {itemAccessor.DirectAccess}");
                        args.Add((gen) =>
                        {
                            using (var subArgs = new FunctionWrapper(gen,
                                $"rhs: {loquiGen.TargetObjectGeneration.Name}{loquiGen.GenericTypes}.Create_XML"))
                            {
                                subArgs.Add($"root: {nodeAccessor}");
                                subArgs.Add($"doMasks: {doMaskAccessor}");
                                subArgs.Add($"errorMask: out {loquiGen.Mask(MaskType.Error)} {typeGen.Name}createMask");
                            }
                        });
                        args.Add("def: null");
                        args.Add("cmds: null");
                        args.Add("copyMask: null");
                        args.Add($"doMasks: {doMaskAccessor}");
                        args.Add($"errorMask: out {loquiGen.Mask(MaskType.Error)} {typeGen.Name}copyMask");
                    }
                    using (var args = new ArgsWrapper(fg,
                        "ErrorMask.HandleErrorMask"))
                    {
                        args.Add(maskAccessor);
                        args.Add($"index: (int){typeGen.IndexEnumName}");
                        args.Add($"errMaskObj: MaskItem<Exception, {loquiGen.Mask(MaskType.Error)}>.WrapValue({loquiGen.Mask(MaskType.Error)}.Combine({typeGen.Name}createMask, {typeGen.Name}copyMask))");
                    }
                }
                else
                {
                    GenerateCopyInRet_Internal(
                        fg: fg, 
                        typeGen: typeGen, 
                        nodeAccessor: nodeAccessor, 
                        itemAccessor: itemAccessor,
                        ret: false,
                        doMaskAccessor: doMaskAccessor, 
                        maskAccessor: maskAccessor);
                }
            }
            else
            {
                UnsafeXmlTranslationGeneration unsafeXml = new UnsafeXmlTranslationGeneration()
                {
                    ErrMaskString = $"MaskItem<Exception, {loquiGen.Mask(MaskType.Error)}>"
                };
                var isProperty = itemAccessor.PropertyAccess != null;
                unsafeXml.GenerateCopyInRet(
                    fg: fg,
                    typeGen: typeGen,
                    nodeAccessor: nodeAccessor,
                    retAccessor: new Accessor($"var {typeGen.Name}unsafe = "),
                    doMaskAccessor: doMaskAccessor,
                    maskAccessor: $"var {typeGen.Name}unsafeMask");
                using (var args = new ArgsWrapper(fg,
                    "ErrorMask.HandleErrorMask"))
                {
                    args.Add(maskAccessor);
                    args.Add($"index: (int){typeGen.IndexEnumName}");
                    args.Add($"errMaskObj: MaskItem<Exception, {loquiGen.Mask(MaskType.Error)}>.WrapValue({typeGen.Name}unsafeMask)");
                }
                if (isProperty)
                {
                    fg.AppendLine($"{itemAccessor.PropertyAccess}.{nameof(HasBeenSetItemExt.SetIfSucceeded)}({typeGen.Name}unsafe.Bubble<{loquiGen.GenericDef.Name}>((o) => ({loquiGen.GenericDef.Name})o));");
                }
                else
                {
                    fg.AppendLine($"{itemAccessor.DirectAccess} = {typeGen.Name}unsafe;");
                }
            }
        }

        public void GenerateCopyInRet_Internal(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor itemAccessor,
            bool ret,
            string doMaskAccessor,
            string maskAccessor)
        {
            string prefix, suffix;
            bool isProperty = itemAccessor?.PropertyAccess != null;
            if (!ret)
            {
                suffix = isProperty ? ")" : $".GetOrDefault({itemAccessor.DirectAccess})";
                prefix = isProperty ? $"{itemAccessor.PropertyAccess}.{nameof(HasBeenSetItemExt.SetIfSucceeded)}(" : $"{itemAccessor.DirectAccess} = ";
            }
            else
            {
                suffix = null;
                prefix = itemAccessor.DirectAccess;
            }

            var loquiGen = typeGen as LoquiType;
            if (loquiGen.TargetObjectGeneration != null)
            {
                using (var args = new ArgsWrapper(fg,
                    $"{prefix}LoquiXmlTranslation<{loquiGen.ObjectTypeName}{loquiGen.GenericTypes}, {loquiGen.Mask(MaskType.Error)}>.Instance.Parse",
                    suffixLine: suffix))
                {
                    args.Add($"root: {nodeAccessor}");
                    if (typeGen.HasIndex)
                    {
                        args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                        args.Add($"errorMask: {maskAccessor}");
                    }
                    else
                    {
                        args.Add($"doMasks: {doMaskAccessor}");
                        args.Add($"errorMask: out {maskAccessor}");
                    }
                }
            }
            else
            {
                UnsafeXmlTranslationGeneration unsafeXml = new UnsafeXmlTranslationGeneration()
                {
                    ErrMaskString = $"MaskItem<Exception, {loquiGen.Mask(MaskType.Error)}>"
                };
                unsafeXml.GenerateCopyInRet(
                    fg: fg,
                    typeGen: typeGen,
                    nodeAccessor: nodeAccessor,
                    retAccessor: itemAccessor,
                    doMaskAccessor: doMaskAccessor,
                    maskAccessor: maskAccessor);
            }
        }

        public override void GenerateCopyInRet(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor retAccessor,
            string doMaskAccessor,
            string maskAccessor)
        {
            GenerateCopyInRet_Internal(
                fg: fg,
                typeGen: typeGen,
                nodeAccessor: nodeAccessor,
                itemAccessor: retAccessor,
                ret: true,
                doMaskAccessor: doMaskAccessor,
                maskAccessor: maskAccessor);
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
