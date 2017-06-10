using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class DictXmlTranslationGeneration : XmlTranslationGeneration
    { 
        public override bool OutputsErrorMask => true;
        private readonly XmlTranslationModule _mod;

        public DictXmlTranslationGeneration(XmlTranslationModule mod)
        {
            this._mod = mod;
        }

        public override void GenerateWrite(
            FileGeneration fg,
            TypeGeneration typeGen,
            string writerAccessor,
            string itemAccessor,
            string maskAccessor,
            string nameAccessor)
        {
            var dictType = typeGen as DictType;
            if (!_mod.TypeGenerations.TryGetValue(dictType.KeyTypeGen.GetType(), out var keyTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + dictType.KeyTypeGen);
            }

            var valTypeGen = typeGen as DictType;
            if (!_mod.TypeGenerations.TryGetValue(dictType.ValueTypeGen.GetType(), out var valTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + dictType.ValueTypeGen);
            }

            using (var args = new ArgsWrapper(fg,
                $"DictXmlTranslation<{dictType.KeyTypeGen.TypeName}, {dictType.ValueTypeGen.TypeName}>.Instance.Write"))
            {
                args.Add($"writer: {writerAccessor}");
                args.Add($"name: {nameAccessor}");
                args.Add($"items: {itemAccessor}");
                args.Add($"doMasks: doMasks");
                args.Add("maskList: out var errorMaskObj");
                args.Add((gen) =>
                {
                    gen.AppendLine($"keyTransl: ({dictType.KeyTypeGen.TypeName} subItem, out object subMask) =>");
                    using (new BraceWrapper(gen))
                    {
                        keyTransl.GenerateWrite(
                            fg: gen,
                            typeGen: dictType.KeyTypeGen,
                            writerAccessor: "writer",
                            itemAccessor: $"subItem",
                            maskAccessor: $"subMask",
                            nameAccessor: "null");
                        if (!keyTransl.OutputsErrorMask)
                        {
                            gen.AppendLine($"subMask = null;");
                        }
                    }
                });
                args.Add((gen) =>
                {
                    gen.AppendLine($"valTransl: ({dictType.ValueTypeGen.TypeName} subItem, out object subMask) =>");
                    using (new BraceWrapper(gen))
                    {
                        valTransl.GenerateWrite(
                            fg: gen,
                            typeGen: dictType.ValueTypeGen,
                            writerAccessor: "writer",
                            itemAccessor: $"subItem",
                            maskAccessor: $"subMask",
                            nameAccessor: "null");
                        if (!valTransl.OutputsErrorMask)
                        {
                            gen.AppendLine($"subMask = null;");
                        }
                    }
                });
            }

            fg.AppendLine($"if (errorMaskObj != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{maskAccessor}().SetNthMask((ushort){typeGen.IndexEnumName}, errorMaskObj);");
            }
        }

        public override void GenerateCopyIn(FileGeneration fg, TypeGeneration typeGen, string nodeAccessor, string itemAccessor, string maskAccessor)
        {
            fg.AppendLine($"throw new NotImplementedException();");
        }
    }
}
