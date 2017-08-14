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
            TypeGeneration typeGen,
            string writerAccessor,
            string itemAccessor,
            string doMaskAccessor,
            string maskAccessor,
            string nameAccessor)
        {
            var loquiGen = typeGen as LoquiType;
            if (loquiGen.TargetObjectGeneration != null)
            {
                using (var args = new ArgsWrapper(fg,
                    $"{loquiGen.TargetObjectGeneration.ExtCommonName}.Write_XML"))
                {
                    args.Add($"writer: {writerAccessor}");
                    args.Add($"item: {itemAccessor}");
                    args.Add($"name: {nameAccessor}");
                    args.Add($"doMasks: {doMaskAccessor}");
                    args.Add($"errorMask: out {loquiGen.ErrorMaskItemString} loquiMask");
                }
                fg.AppendLine($"{maskAccessor} = loquiMask == null ? null : new MaskItem<Exception, {loquiGen.ErrorMaskItemString}>(null, loquiMask);");
            }
            else
            {
                UnsafeXmlTranslationGeneration unsafeXml = new UnsafeXmlTranslationGeneration()
                {
                    ErrMaskString = $"MaskItem<Exception, {loquiGen.ErrorMaskItemString}>"
                };
                unsafeXml.GenerateWrite(
                    fg: fg,
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
            return loquiGen.SingletonType != LoquiType.SingletonLevel.Singleton || loquiGen.InterfaceType != LoquiInterfaceType.IGetter;
        }

        public override void GenerateCopyIn(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            string itemAccessor,
            string doMaskAccessor,
            string maskAccessor)
        {
            var loquiGen = typeGen as LoquiType;
            if (loquiGen.TargetObjectGeneration != null)
            {
                if (loquiGen.SingletonType == LoquiType.SingletonLevel.Singleton)
                {
                    if (loquiGen.InterfaceType == LoquiInterfaceType.IGetter) return;
                    using (var args = new ArgsWrapper(fg,
                        $"var tmp = {loquiGen.TargetObjectGeneration.Name}.Create_XML"))
                    {
                        args.Add($"root: {nodeAccessor}");
                        args.Add($"doMasks: {doMaskAccessor}");
                        args.Add($"errorMask: out {loquiGen.ErrorMaskItemString} createMask");
                    }
                    using (var args = new ArgsWrapper(fg,
                        $"{loquiGen.TargetObjectGeneration.ExtCommonName}.CopyFieldsFrom"))
                    {
                        args.Add($"item: {itemAccessor}");
                        args.Add("rhs: tmp");
                        args.Add("def: null");
                        args.Add("cmds: null");
                        args.Add("copyMask: null");
                        args.Add($"doErrorMask: {doMaskAccessor}");
                        args.Add($"errorMask: out {loquiGen.ErrorMaskItemString} copyMask");
                    }
                    fg.AppendLine($"var loquiMask = {loquiGen.ErrorMaskItemString}.Combine(createMask, copyMask);");
                    fg.AppendLine($"{maskAccessor} = loquiMask == null ? null : new MaskItem<Exception, {loquiGen.ErrorMaskItemString}>(null, loquiMask);");
                }
                else
                {
                    GenerateCopyInRet(fg, typeGen, nodeAccessor, null, doMaskAccessor, maskAccessor);
                    fg.AppendLine("if (tryGet.Succeeded)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{itemAccessor} = tryGet.Value;");
                    }
                }
            }
            else
            {
                UnsafeXmlTranslationGeneration unsafeXml = new UnsafeXmlTranslationGeneration()
                {
                    ErrMaskString = $"MaskItem<Exception, {loquiGen.ErrorMaskItemString}>"
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
                fg.AppendLine($"{loquiGen.TargetObjectGeneration.ErrorMask} loquiMask;");
                fg.AppendLine($"TryGet<{typeGen.TypeName}> tryGet;");
                fg.AppendLine($"var typeStr = {nodeAccessor}.GetAttribute(XmlConstants.{nameof(XmlConstants.TYPE_ATTRIBUTE)});");
                fg.AppendLine($"if (typeStr != null");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"&& typeStr.Equals(\"{loquiGen.TargetObjectGeneration.FullName}\"))");
                }
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg,
                        $"tryGet = TryGet<{typeGen.TypeName}>.Succeed(({typeGen.TypeName}){loquiGen.TargetObjectGeneration.Name}.Create_XML"))
                    {
                        args.Add($"root: {nodeAccessor}");
                        args.Add($"doMasks: {doMaskAccessor}");
                        args.Add($"errorMask: out loquiMask)");
                    }
                }
                fg.AppendLine("else");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"var register = LoquiRegistration.GetRegisterByFullName(typeStr ?? {nodeAccessor}.Name.LocalName);");
                    fg.AppendLine("if (register == null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"var ex = new ArgumentException($\"Unknown Loqui type: {{{nodeAccessor}.Name.LocalName}}\");");
                        fg.AppendLine($"if (!{doMaskAccessor}) throw ex;");
                        using (var args = new ArgsWrapper(fg,
                            $"{maskAccessor} = new MaskItem<Exception, {loquiGen.ErrorMaskItemString}>"))
                        {
                            args.Add($"ex");
                            args.Add("null");
                        }
                        if (retAccessor != null)
                        {
                            fg.AppendLine($"{retAccessor}TryGet<{loquiGen.ObjectTypeName}>.Fail(null);");
                        }
                        else
                        {
                            fg.AppendLine("break;");
                        }
                    }
                    using (var args = new ArgsWrapper(fg,
                        $"tryGet = XmlTranslator.Instance.GetTranslator(register.ClassType).Item.Value.Parse",
                        $".Bubble((o) => ({typeGen.TypeName})o)"))
                    {
                        args.Add("root: root");
                        args.Add($"doMasks: {doMaskAccessor}");
                        args.Add("maskObj: out var subErrorMaskObj");
                    }
                    fg.AppendLine($"loquiMask = ({loquiGen.TargetObjectGeneration.ErrorMask})subErrorMaskObj;");
                }
                fg.AppendLine($"{maskAccessor} = loquiMask == null ? null : new MaskItem<Exception, {loquiGen.ErrorMaskItemString}>(null, loquiMask);");
                if (retAccessor != null)
                {
                    fg.AppendLine($"{retAccessor}tryGet;");
                }
            }
            else
            {
                UnsafeXmlTranslationGeneration unsafeXml = new UnsafeXmlTranslationGeneration()
                {
                    ErrMaskString = $"MaskItem<Exception, {loquiGen.ErrorMaskItemString}>"
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
