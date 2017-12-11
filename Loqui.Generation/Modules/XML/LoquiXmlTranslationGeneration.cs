using Loqui.Xml;
using System;
using System.Collections.Generic;
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
                    $"LoquiXmlTranslation<{loquiGen.TypeName}, {loquiGen.MaskItemString(MaskType.Error)}>.Instance.Write"))
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
                    ErrMaskString = $"MaskItem<Exception, {loquiGen.MaskItemString(MaskType.Error)}>"
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
                        $"var tmp = {loquiGen.TargetObjectGeneration.Name}{loquiGen.GenericTypes}.Create_XML"))
                    {
                        args.Add($"root: {nodeAccessor}");
                        args.Add($"doMasks: {doMaskAccessor}");
                        args.Add($"errorMask: out {loquiGen.MaskItemString(MaskType.Error)} createMask");
                    }
                    using (var args = new ArgsWrapper(fg,
                        $"{loquiGen.TargetObjectGeneration.ExtCommonName}.CopyFieldsFrom"))
                    {
                        args.Add($"item: {itemAccessor.DirectAccess}");
                        args.Add("rhs: tmp");
                        args.Add("def: null");
                        args.Add("cmds: null");
                        args.Add("copyMask: null");
                        args.Add($"doMasks: {doMaskAccessor}");
                        args.Add($"errorMask: out {loquiGen.MaskItemString(MaskType.Error)} copyMask");
                    }
                    fg.AppendLine($"var loquiMask = {loquiGen.MaskItemString(MaskType.Error)}.Combine(createMask, copyMask);");
                    fg.AppendLine($"{maskAccessor} = loquiMask == null ? null : new MaskItem<Exception, {loquiGen.MaskItemString(MaskType.Error)}>(null, loquiMask);");
                }
                else
                {
                    GenerateCopyInRet(
                        fg: fg, 
                        typeGen: typeGen, 
                        nodeAccessor: nodeAccessor, 
                        retAccessor: "var tryGet = ",
                        doMaskAccessor: doMaskAccessor, 
                        maskAccessor: maskAccessor);
                    if (itemAccessor.PropertyAccess != null)
                    {
                        fg.AppendLine($"{itemAccessor.PropertyAccess}.{nameof(HasBeenSetItemExt.SetIfSucceeded)}(tryGet);");
                    }
                    else
                    {
                        fg.AppendLine("if (tryGet.Succeeded)");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"{itemAccessor.DirectAccess} = tryGet.Value;");
                        }
                    }
                }
            }
            else
            {
                UnsafeXmlTranslationGeneration unsafeXml = new UnsafeXmlTranslationGeneration()
                {
                    ErrMaskString = $"MaskItem<Exception, {loquiGen.MaskItemString(MaskType.Error)}>"
                };
                unsafeXml.GenerateCopyIn(
                    fg: fg,
                    typeGen: typeGen,
                    nodeAccessor: nodeAccessor,
                    itemAccessor: itemAccessor,
                    doMaskAccessor: doMaskAccessor,
                    maskAccessor: "var unsafeMask");
                fg.AppendLine($"{maskAccessor} = unsafeMask == null ? null : new MaskItem<Exception, object>(null, unsafeMask);");
            }
        }

        public override void GenerateCopyInRet(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            string retAccessor,
            string doMaskAccessor,
            string maskAccessor)
        {
            var loquiGen = typeGen as LoquiType;
            if (loquiGen.TargetObjectGeneration != null)
            {
                using (var args = new ArgsWrapper(fg,
                    $"{retAccessor}LoquiXmlTranslation<{loquiGen.ObjectTypeName}{loquiGen.GenericTypes}, {loquiGen.MaskItemString(MaskType.Error)}>.Instance.Parse"))
                {
                    args.Add($"root: {nodeAccessor}");
                    args.Add($"doMasks: {doMaskAccessor}");
                    args.Add($"errorMask: out {maskAccessor}");
                }
            }
            else
            {
                UnsafeXmlTranslationGeneration unsafeXml = new UnsafeXmlTranslationGeneration()
                {
                    ErrMaskString = $"MaskItem<Exception, {loquiGen.MaskItemString(MaskType.Error)}>"
                };
                unsafeXml.GenerateCopyInRet(
                    fg: fg,
                    typeGen: typeGen,
                    nodeAccessor: nodeAccessor,
                    retAccessor: retAccessor,
                    doMaskAccessor: doMaskAccessor,
                    maskAccessor: maskAccessor);
            }
        }

        public override XElement GenerateForXSD(
            XElement rootElement,
            XElement choiceElement,
            TypeGeneration typeGen,
            string nameOverride = null)
        {
            return null;
        }
    }
}
