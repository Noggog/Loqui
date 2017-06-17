using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class DictXmlTranslationGeneration : XmlTranslationGeneration
    { 
        public override void GenerateWrite(
            FileGeneration fg,
            TypeGeneration typeGen,
            string writerAccessor,
            string itemAccessor,
            string maskAccessor,
            string nameAccessor)
        {
            fg.AppendLine($"throw new NotImplementedException();");
            return; 

            var dictType = typeGen as DictType;
            if (!XmlMod.TypeGenerations.TryGetValue(dictType.KeyTypeGen.GetType(), out var keyTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + dictType.KeyTypeGen);
            }

            var valTypeGen = typeGen as DictType;
            if (!XmlMod.TypeGenerations.TryGetValue(dictType.ValueTypeGen.GetType(), out var valTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + dictType.ValueTypeGen);
            }

            var keyMask = this.MaskModule.GetMaskModule(dictType.KeyTypeGen.GetType()).GetErrorMaskTypeStr(dictType.KeyTypeGen);
            var valMask = this.MaskModule.GetMaskModule(dictType.ValueTypeGen.GetType()).GetErrorMaskTypeStr(dictType.ValueTypeGen);

            using (var args = new ArgsWrapper(fg,
                $"DictXmlTranslation<{dictType.KeyTypeGen.TypeName}, {dictType.ValueTypeGen.TypeName}, {keyMask}, {valMask}>.Instance.Write"))
            {
                args.Add($"writer: {writerAccessor}");
                args.Add($"name: {nameAccessor}");
                args.Add($"items: {itemAccessor}");
                args.Add($"doMasks: doMasks");
                args.Add($"maskObj: out {maskAccessor}");
                args.Add((gen) =>
                {
                    gen.AppendLine($"keyTransl: ({dictType.KeyTypeGen.TypeName} subItem, out {keyMask} dictSubMask) =>");
                    using (new BraceWrapper(gen))
                    {
                        keyTransl.GenerateWrite(
                            fg: gen,
                            typeGen: dictType.KeyTypeGen,
                            writerAccessor: "writer",
                            itemAccessor: $"subItem",
                            maskAccessor: $"dictSubMask",
                            nameAccessor: "null");
                    }
                });
                args.Add((gen) =>
                {
                    gen.AppendLine($"valTransl: ({dictType.ValueTypeGen.TypeName} subItem, out {valMask} dictSubMask) =>");
                    using (new BraceWrapper(gen))
                    {
                        valTransl.GenerateWrite(
                            fg: gen,
                            typeGen: dictType.ValueTypeGen,
                            writerAccessor: "writer",
                            itemAccessor: $"subItem",
                            maskAccessor: $"dictSubMask",
                            nameAccessor: "null");
                    }
                });
            }
        }

        public override void GenerateCopyIn(FileGeneration fg, TypeGeneration typeGen, string nodeAccessor, string itemAccessor, string maskAccessor)
        {
            fg.AppendLine($"throw new NotImplementedException();");
        }

        public override void GenerateCopyInRet(FileGeneration fg, TypeGeneration typeGen, string nodeAccessor, string retAccessor, string maskAccessor)
        {
            fg.AppendLine($"throw new NotImplementedException();");
        }
    }
}
