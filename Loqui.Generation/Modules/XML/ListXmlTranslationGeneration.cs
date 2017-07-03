using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class ListXmlTranslationGeneration : XmlTranslationGeneration
    {
        public override void GenerateWrite(
            FileGeneration fg,
            TypeGeneration typeGen,
            string writerAccessor,
            string itemAccessor,
            string doMaskAccessor,
            string maskAccessor,
            string nameAccessor)
        {
            var list = typeGen as ListType;
            if (!XmlMod.TypeGenerations.TryGetValue(list.SubTypeGeneration.GetType(), out var subTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + list.SubTypeGeneration);
            }

            var subMaskStr = subTransl.MaskModule.GetMaskModule(list.SubTypeGeneration.GetType()).GetErrorMaskTypeStr(list.SubTypeGeneration);
            using (var args = new ArgsWrapper(fg,
                $"ListXmlTranslation<{list.SubTypeGeneration.TypeName}, {subMaskStr}>.Instance.Write"))
            {
                args.Add($"writer: {writerAccessor}");
                args.Add($"name: {nameAccessor}");
                args.Add($"item: {itemAccessor}");
                args.Add($"doMasks: {doMaskAccessor}");
                args.Add($"maskObj: out {maskAccessor}");
                args.Add((gen) =>
                {
                    gen.AppendLine($"transl: ({list.SubTypeGeneration.TypeName} subItem, bool listDoMasks, out {subMaskStr} listSubMask) =>");
                    using (new BraceWrapper(gen))
                    {
                        subTransl.GenerateWrite(
                            fg: gen, 
                            typeGen: list.SubTypeGeneration, 
                            writerAccessor: "writer", 
                            itemAccessor: $"subItem", 
                            doMaskAccessor: doMaskAccessor,
                            maskAccessor: $"listSubMask",
                            nameAccessor: "\"Item\"");
                    }
                });
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
            GenerateCopyInRet(fg, typeGen, nodeAccessor, "var listTryGet = ", doMaskAccessor, maskAccessor);
            fg.AppendLine($"if (listTryGet.Succeeded)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{itemAccessor}.SetTo(listTryGet.Value);");
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
            var list = typeGen as ListType;
            if (!XmlMod.TypeGenerations.TryGetValue(list.SubTypeGeneration.GetType(), out var subTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + list.SubTypeGeneration);
            }
            var subMaskStr = subTransl.MaskModule.GetMaskModule(list.SubTypeGeneration.GetType()).GetErrorMaskTypeStr(list.SubTypeGeneration);
            using (var args = new ArgsWrapper(fg,
                $"{retAccessor}ListXmlTranslation<{list.SubTypeGeneration.TypeName}, {subMaskStr}>.Instance.Parse"))
            {
                args.Add($"root: root");
                args.Add($"doMasks: {doMaskAccessor}");
                args.Add($"maskObj: out {maskAccessor}");
                args.Add((gen) =>
                {
                    gen.AppendLine($"transl: (XElement r, bool listDoMasks, out {typeGen.ProtoGen.Gen.MaskModule.GetMaskModule(list.SubTypeGeneration.GetType()).GetErrorMaskTypeStr(list.SubTypeGeneration)} listSubMask) =>");
                    using (new BraceWrapper(gen))
                    {
                        var xmlGen = XmlMod.TypeGenerations[list.SubTypeGeneration.GetType()];
                        xmlGen.GenerateCopyInRet(gen, list.SubTypeGeneration, "r", "return ", "listDoMasks", "listSubMask");
                    }
                });
            }
        }
    }
}
