using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class UnsafeXmlTranslationGeneration : XmlTranslationGeneration
    {
        public string ErrMaskString = "object";

        public override void GenerateWrite(
            FileGeneration fg,
            TypeGeneration typeGen,
            string writerAccessor,
            string itemAccessor,
            string doMaskAccessor,
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
                args.Add($"{doMaskAccessor}");
                args.Add($"out object unsafeErrMask");
            }
            fg.AppendLine($"{maskAccessor} = ({ErrMaskString})unsafeErrMask;");
        }

        public override void GenerateCopyIn(
            FileGeneration fg, 
            TypeGeneration typeGen,
            string nodeAccessor,
            string itemAccessor,
            string doMaskAccessor,
            string maskAccessor)
        {
            GenerateCopyInRet(fg, typeGen, nodeAccessor, $"var tryGet = ", doMaskAccessor, maskAccessor);
            fg.AppendLine($"if (tryGet.Succeeded)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{itemAccessor} = ({typeGen.TypeName})tryGet.Value;");
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
                $"{retAccessor}transl.Item.Value.Parse"))
            {
                args.Add($"{nodeAccessor}");
                args.Add($"{doMaskAccessor}");
                args.Add($"out {maskAccessor}");
            }
        }
    }
}
