using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class EnumXmlTranslationGeneration : XmlTranslationGeneration
    {
        PrimitiveXmlTranslationGeneration<string> _subGen = new PrimitiveXmlTranslationGeneration<string>();
        public override void GenerateWrite(
            FileGeneration fg,
            TypeGeneration typeGen,
            string writerAccessor,
            string itemAccessor,
            string doMaskAccessor,
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
                args.Add($"doMasks: {doMaskAccessor}");
                args.Add($"errorMask: out {maskAccessor}");
            }
        }

        public override void GenerateCopyIn(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            string itemAccessor,
            string doMaskAccessor,
            string maskAccessor)
        {
            var eType = typeGen as EnumType;
            GenerateCopyInRet(fg, typeGen, nodeAccessor, "var tryGet = ", doMaskAccessor, maskAccessor);
            fg.AppendLine("if (tryGet.Succeeded)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{itemAccessor} = tryGet.Value{(eType.Nullable ? null : ".Value")};");
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
            var eType = typeGen as EnumType;
            using (var args = new ArgsWrapper(fg,
                $"{retAccessor}EnumXmlTranslation<{eType.NoNullTypeName}>.Instance.Parse"))
            {
                args.Add(nodeAccessor);
                args.Add($"nullable: {eType.Nullable.ToString().ToLower()}");
                args.Add($"doMasks: {doMaskAccessor}");
                args.Add($"errorMask: out {maskAccessor}");
            }
        }

        public override XElement GenerateForXSD(
            XElement rootElement, 
            XElement choiceElement,
            TypeGeneration typeGen,
            string nameOverride = null)
        {
            return _subGen.GenerateForXSD(rootElement, choiceElement, typeGen, nameOverride);
        }
    }
}
