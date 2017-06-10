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
            UnsafeType unsafeType = typeGen as UnsafeType;
            fg.AppendLine($"if (!XmlTranslator.TranslateElementName(root.Name.LocalName, out var type))");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"throw new ArgumentException($\"Failed to get translator for {{root.Name.LocalName}}.\");");
            }
            fg.AppendLine($"var transl = XmlTranslator.GetTranslator(type.Item);");
            fg.AppendLine($"if (transl?.Item.Failed ?? true)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"throw new ArgumentException($\"Failed to get translator for {{type.Item}}. {{transl?.Item.Reason}}\");");
            }
            using (var args = new ArgsWrapper(fg,
                $"var tryGet = transl.Item.Value.Parse"))
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
            fg.AppendLine($"if (tryGet.Succeeded)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{itemAccessor} = ({typeGen.TypeName})tryGet.Value;");
            }
        }
    }
}
