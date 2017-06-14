using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class EnumXmlTranslationGeneration : XmlTranslationGeneration
    {
        public override bool OutputsErrorMask => false;

        public override void GenerateWrite(
            FileGeneration fg,
            TypeGeneration typeGen,
            string writerAccessor,
            string itemAccessor,
            string maskAccessor,
            string nameAccessor)
        {
            var eType = typeGen as EnumType;
            using (var args = new ArgsWrapper(fg,
                $"EnumXmlTranslation<{eType.NoNullTypeName}>.Instance.Write"))
            {
                args.Add(writerAccessor);
                args.Add(nameAccessor);
                args.Add($"{itemAccessor}");
            }
        }

        public override void GenerateCopyIn(FileGeneration fg, TypeGeneration typeGen, string nodeAccessor, string itemAccessor, string maskAccessor)
        {
            var eType = typeGen as EnumType;
            using (var args = new ArgsWrapper(fg,
                $"var tryGet = EnumXmlTranslation<{eType.NoNullTypeName}>.Instance.Parse"))
            {
                args.Add(nodeAccessor);
                args.Add($"nullable: {eType.Nullable.ToString().ToLower()}");
            }
            fg.AppendLine("if (tryGet.Succeeded)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{itemAccessor} = tryGet.Value{(eType.Nullable ? null : ".Value")};");
            }
        }

        public override void GenerateCopyInRet(FileGeneration fg, TypeGeneration typeGen, string nodeAccessor, string retAccessor, string maskAccessor)
        {
            var eType = typeGen as EnumType;
            using (var args = new ArgsWrapper(fg,
                $"{retAccessor}EnumXmlTranslation<{eType.NoNullTypeName}>.Instance.Parse"))
            {
                args.Add(nodeAccessor);
                args.Add($"nullable: {eType.Nullable.ToString().ToLower()}");
            }
        }
    }
}
