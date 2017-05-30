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
                        fg.AppendLine($"{maskAccessor}().SetNthMask((ushort){typeGen.IndexEnumName}, sub{maskAccessor});");
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

        public override void GenerateCopyIn(FileGeneration fg, TypeGeneration typeGen, string nodeAccessor, string itemAccessor, string maskAccessor)
        {
            var loquiGen = typeGen as LoquiType;
            if (loquiGen.TargetObjectGeneration != null)
            {
                if (loquiGen.SingletonType == LoquiType.SingletonLevel.Singleton)
                {
                    using (var args = new ArgsWrapper(fg,
                        $"var tmp = {loquiGen.TargetObjectGeneration.Name}.Create_XML"))
                    {
                        args.Add($"root: {nodeAccessor}");
                        args.Add($"doMasks: doMasks");
                        args.Add($"errorMask: out {loquiGen.ErrorMaskItemString} createMask");
                    }
                    using (var args = new ArgsWrapper(fg,
                        $"{itemAccessor}.CopyFieldsFrom"))
                    {
                        args.Add("rhs: tmp");
                        args.Add("def: null");
                        args.Add("doErrorMask: doMasks");
                        args.Add($"errorMask: out {loquiGen.ErrorMaskItemString} copyMask");
                    }
                }
                else
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{itemAccessor} = {loquiGen.TargetObjectGeneration.Name}.Create_XML"))
                    {
                        args.Add($"root: {nodeAccessor}");
                        args.Add($"doMasks: doMasks");
                        args.Add($"errorMask: out {loquiGen.ErrorMaskItemString} sub{maskAccessor}");
                    }
                }

                if (typeGen.Name != null)
                {
                    fg.AppendLine($"if (sub{maskAccessor} != null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{maskAccessor}().SetNthMask((ushort){typeGen.IndexEnumName}, sub{maskAccessor});");
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
