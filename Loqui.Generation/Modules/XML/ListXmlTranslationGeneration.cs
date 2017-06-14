using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class ListXmlTranslationGeneration : XmlTranslationGeneration
    {
        public override bool OutputsErrorMask => true;
        private readonly XmlTranslationModule _mod;

        public ListXmlTranslationGeneration(XmlTranslationModule mod)
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
            var list = typeGen as ListType;
            if (!_mod.TypeGenerations.TryGetValue(list.SubTypeGeneration.GetType(), out var subTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + list.SubTypeGeneration);
            }

            using (var args = new ArgsWrapper(fg,
                $"ListXmlTranslation<{list.SubTypeGeneration.TypeName}>.Instance.Write"))
            {
                args.Add($"writer: {writerAccessor}");
                args.Add($"name: {nameAccessor}");
                args.Add($"item: {itemAccessor}");
                args.Add($"doMasks: doMasks");
                args.Add("maskObj: out object errorMaskObj");
                args.Add((gen) =>
                {
                    gen.AppendLine($"transl: ({list.SubTypeGeneration.TypeName} subItem, out object subMask) =>");
                    using (new BraceWrapper(gen))
                    {
                        subTransl.GenerateWrite(
                            fg: gen, 
                            typeGen: list.SubTypeGeneration, 
                            writerAccessor: "writer", 
                            itemAccessor: $"subItem", 
                            maskAccessor: $"subMask",
                            nameAccessor: "null");
                        if (!subTransl.OutputsErrorMask)
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
            GenerateCopyInRet(fg, typeGen, nodeAccessor, "var listTryGet = ", maskAccessor);
            fg.AppendLine($"if (suberrorMask != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"errorMask().SetNthMask((ushort){typeGen.IndexEnumName}, suberrorMask);");
            }
            fg.AppendLine($"if (listTryGet.Succeeded)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{itemAccessor}.SetTo(listTryGet.Value);");
            }
        }

        public override void GenerateCopyInRet(FileGeneration fg, TypeGeneration typeGen, string nodeAccessor, string retAccessor, string maskAccessor)
        {
            ListType list = typeGen as ListType;
            using (var args = new ArgsWrapper(fg,
                $"{retAccessor}ListXmlTranslation<{list.SubTypeGeneration.TypeName}>.Instance.Parse"))
            {
                args.Add($"root: root");
                args.Add($"doMasks: doMasks");
                args.Add($"maskObj: out var suberrorMask");
                args.Add((gen) =>
                {
                    gen.AppendLine($"transl: (XElement r, out {typeGen.ProtoGen.Gen.MaskModule.GetMaskModule(list.SubTypeGeneration.GetType()).GetErrorMaskTypeStr(list.SubTypeGeneration)} subsubErr) =>");
                    using (new BraceWrapper(gen))
                    {
                        var xmlGen = _mod.TypeGenerations[list.SubTypeGeneration.GetType()];
                        if (!xmlGen.OutputsErrorMask)
                        {
                            gen.AppendLine("subsubErr = null;");
                        }
                        xmlGen.GenerateCopyInRet(gen, list.SubTypeGeneration, "r", "return ", "subsubErr");
                    }
                });
            }
        }
    }
}
