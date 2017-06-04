using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class UnsafeXmlTranslationGeneration : XmlTranslationGeneration
    {
        public override bool OutputsErrorMask => true;

        public override void GenerateWrite(
            FileGeneration fg,
            TypeGeneration typeGen,
            string writerAccessor,
            string itemAccessor,
            string maskAccessor,
            string nameAccessor)
        {
            fg.AppendLine($"var wildType = item.{typeGen.Name} == null ? null : item.{typeGen.Name}.GetType();");
            fg.AppendLine($"var transl = XmlTranslator.GetTranslator(wildType);");
            fg.AppendLine($"if (transl?.Item.Failed ?? true)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"throw new ArgumentException($\"Failed to get translator for {{wildType}}. {{transl?.Item.Reason}}\");");
            }
            using (var args = new ArgsWrapper(fg,
                $"transl.Item.Value.Write"))
            {
                args.Add(writerAccessor);
                args.Add(nameAccessor);
                args.Add($"{itemAccessor}");
                args.Add($"doMasks");
                args.Add($"out object sub{maskAccessor}");
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

        public override void GenerateCopyIn(FileGeneration fg, TypeGeneration typeGen, string nodeAccessor, string itemAccessor, string maskAccessor)
        {
            fg.AppendLine($"var wildType = item.{typeGen.Name} == null ? null : item.{typeGen.Name}.GetType();");
            fg.AppendLine($"var transl = XmlTranslator.GetTranslator(wildType);");
            fg.AppendLine($"if (transl?.Item.Failed ?? true)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"throw new ArgumentException($\"Failed to get translator for {{wildType}}. {{transl?.Item.Reason}}\");");
            }
            using (var args = new ArgsWrapper(fg,
                $"transl.Item.Value.Parse"))
            {
                args.Add($"{nodeAccessor}");
                args.Add($"doMasks");
                args.Add($"out object sub{maskAccessor}");
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
    }
}
