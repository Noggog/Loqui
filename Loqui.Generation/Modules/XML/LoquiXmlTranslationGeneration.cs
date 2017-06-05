using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class LoquiXmlTranslationGeneration : XmlTranslationGeneration
    {
        public override bool OutputsErrorMask => true;
        UnsafeXmlTranslationGeneration unsafeXml = new UnsafeXmlTranslationGeneration();

        public override void GenerateWrite(
            FileGeneration fg,
            TypeGeneration typeGen,
            string writerAccessor,
            string itemAccessor,
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
                    args.Add($"doMasks: doMasks");
                    args.Add($"errorMask: out {loquiGen.ErrorMaskItemString} sub{maskAccessor}");
                }

                if (typeGen.Name != null)
                {
                    fg.AppendLine($"if (sub{maskAccessor} != null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{maskAccessor}().SetNthMask((ushort){typeGen.IndexEnumName}, new MaskItem<Exception, {loquiGen.TargetObjectGeneration.ErrorMask}>(null, sub{maskAccessor}));");
                    }
                }
                else
                {
                    fg.AppendLine($"{maskAccessor} = sub{maskAccessor};");
                }
            }
            else
            {
                unsafeXml.GenerateWrite(
                    fg: fg, 
                    typeGen: typeGen, 
                    writerAccessor: writerAccessor,
                    itemAccessor: itemAccessor,
                    maskAccessor: maskAccessor,
                    nameAccessor: nameAccessor);
            }
        }

        public override bool ShouldGenerateCopyIn(TypeGeneration typeGen)
        {
            var loquiGen = typeGen as LoquiType;
            return loquiGen.SingletonType != LoquiType.SingletonLevel.Singleton || loquiGen.InterfaceType != LoquiInterfaceType.IGetter;
        }

        public override void GenerateCopyIn(FileGeneration fg, TypeGeneration typeGen, string nodeAccessor, string itemAccessor, string maskAccessor)
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
                        args.Add($"doMasks: doMasks");
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
                        args.Add("doErrorMask: doMasks");
                        args.Add($"errorMask: out {loquiGen.ErrorMaskItemString} copyMask");
                    }
                    fg.AppendLine($"var sub{maskAccessor} = {loquiGen.ErrorMaskItemString}.Combine(createMask, copyMask);");
                }
                else
                {
                    fg.AppendLine($"{loquiGen.TargetObjectGeneration.ErrorMask} suberrorMask;");
                    fg.AppendLine($"if (typeName.Equals(\"{loquiGen.TargetObjectGeneration.FullName}\"))");
                    using (new BraceWrapper(fg))
                    {
                        using (var args = new ArgsWrapper(fg,
                            $"{itemAccessor} = ({typeGen.TypeName}){loquiGen.TargetObjectGeneration.Name}.Create_XML"))
                        {
                            args.Add($"root: {nodeAccessor}");
                            args.Add($"doMasks: doMasks");
                            args.Add($"errorMask: out sub{maskAccessor}");
                        }
                    }
                    fg.AppendLine("else");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"var register = LoquiRegistration.GetRegisterByFullName(typeName);");
                        using (var args = new ArgsWrapper(fg,
                            $"XmlTranslator.GetTranslator(register.ClassType).Item.Value.Parse"))
                        {
                            args.Add("root: root");
                            args.Add("doMasks: doMasks");
                            args.Add("maskObj: out var subErrorMaskObj");
                        }
                        fg.AppendLine($"suberrorMask = ({loquiGen.TargetObjectGeneration.ErrorMask})subErrorMaskObj;");

                    }
                }

                if (typeGen.Name != null)
                {
                    fg.AppendLine($"if (sub{maskAccessor} != null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{maskAccessor}().SetNthMask((ushort){typeGen.IndexEnumName}, {LoquiMaskFieldGeneration.GetObjectErrorMask(loquiGen, $"sub{maskAccessor}")});");
                    }
                }
                else
                {
                    fg.AppendLine($"{maskAccessor} = sub{maskAccessor};");
                }
            }
            else
            {
                unsafeXml.GenerateCopyIn(
                    fg: fg,
                    typeGen: typeGen,
                    nodeAccessor: nodeAccessor,
                    itemAccessor: itemAccessor,
                    maskAccessor: maskAccessor);
            }
        }
    }
}
