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
            string doMaskAccessor,
            string maskAccessor,
            string nameAccessor)
        {
            var dictType = typeGen as DictType;
            if (!XmlMod.TypeGenerations.TryGetValue(dictType.ValueTypeGen.GetType(), out var valTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + dictType.ValueTypeGen);
            }

            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    if (!XmlMod.TypeGenerations.TryGetValue(dictType.KeyTypeGen.GetType(), out var keyTransl))
                    {
                        throw new ArgumentException("Unsupported type generator: " + dictType.KeyTypeGen);
                    }

                    var keyMask = this.MaskModule.GetMaskModule(dictType.KeyTypeGen.GetType()).GetErrorMaskTypeStr(dictType.KeyTypeGen);
                    var valMask = this.MaskModule.GetMaskModule(dictType.ValueTypeGen.GetType()).GetErrorMaskTypeStr(dictType.ValueTypeGen);

                    using (var args = new ArgsWrapper(fg,
                        $"DictXmlTranslation<{dictType.KeyTypeGen.TypeName}, {dictType.ValueTypeGen.TypeName}, {keyMask}, {valMask}>.Instance.Write"))
                    {
                        args.Add($"writer: {writerAccessor}");
                        args.Add($"name: {nameAccessor}");
                        args.Add($"items: {itemAccessor}");
                        args.Add($"doMasks: {doMaskAccessor}");
                        args.Add($"maskObj: out {maskAccessor}");
                        args.Add((gen) =>
                        {
                            gen.AppendLine($"keyTransl: ({dictType.KeyTypeGen.TypeName} subItem, bool dictDoMask, out {keyMask} dictSubMask) =>");
                            using (new BraceWrapper(gen))
                            {
                                keyTransl.GenerateWrite(
                                    fg: gen,
                                    typeGen: dictType.KeyTypeGen,
                                    writerAccessor: "writer",
                                    itemAccessor: $"subItem",
                                    doMaskAccessor: "dictDoMask",
                                    maskAccessor: $"dictSubMask",
                                    nameAccessor: "null");
                            }
                        });
                        args.Add((gen) =>
                        {
                            gen.AppendLine($"valTransl: ({dictType.ValueTypeGen.TypeName} subItem, bool dictDoMask, out {valMask} dictSubMask) =>");
                            using (new BraceWrapper(gen))
                            {
                                valTransl.GenerateWrite(
                                    fg: gen,
                                    typeGen: dictType.ValueTypeGen,
                                    writerAccessor: "writer",
                                    itemAccessor: $"subItem",
                                    doMaskAccessor: "dictDoMask",
                                    maskAccessor: $"dictSubMask",
                                    nameAccessor: "null");
                            }
                        });
                    }
                    break;
                case DictMode.KeyedValue:
                    var mask = this.MaskModule.GetMaskModule(dictType.ValueTypeGen.GetType()).GetErrorMaskTypeStr(dictType.ValueTypeGen);

                    using (var args = new ArgsWrapper(fg,
                        $"KeyedDictXmlTranslation<{dictType.KeyTypeGen.TypeName}, {dictType.ValueTypeGen.TypeName}, {mask}>.Instance.Write"))
                    {
                        args.Add($"writer: {writerAccessor}");
                        args.Add($"name: {nameAccessor}");
                        args.Add($"items: {itemAccessor}.Values");
                        args.Add($"doMasks: {doMaskAccessor}");
                        args.Add($"maskObj: out {maskAccessor}");
                        args.Add((gen) =>
                        {
                            gen.AppendLine($"valTransl: ({dictType.ValueTypeGen.TypeName} subItem, bool dictDoMask, out {mask} dictSubMask) =>");
                            using (new BraceWrapper(gen))
                            {
                                valTransl.GenerateWrite(
                                    fg: gen,
                                    typeGen: dictType.ValueTypeGen,
                                    writerAccessor: "writer",
                                    itemAccessor: $"subItem",
                                    doMaskAccessor: "dictDoMask",
                                    maskAccessor: $"dictSubMask",
                                    nameAccessor: "null");
                            }
                        });
                    }
                    break;
                default:
                    throw new NotImplementedException();
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
            fg.AppendLine($"throw new NotImplementedException();");
        }

        public override void GenerateCopyInRet(
            FileGeneration fg,
            TypeGeneration typeGen, 
            string nodeAccessor,
            string retAccessor,
            string doMaskAccessor,
            string maskAccessor)
        {
            fg.AppendLine($"throw new NotImplementedException();");
        }
    }
}
