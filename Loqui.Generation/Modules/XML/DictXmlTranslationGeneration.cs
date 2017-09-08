using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
            if (!XmlMod.TryGetTypeGeneration(dictType.ValueTypeGen.GetType(), out var valTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + dictType.ValueTypeGen);
            }

            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    if (!XmlMod.TryGetTypeGeneration(dictType.KeyTypeGen.GetType(), out var keyTransl))
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
                                    nameAccessor: "\"Item\"");
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
                                    nameAccessor: "\"Item\"");
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
                                    nameAccessor: "\"Item\"");
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
            Accessor itemAccessor,
            string doMaskAccessor,
            string maskAccessor)
        {
            GenerateCopyInRet(fg, typeGen, nodeAccessor, "var dictTryGet = ", doMaskAccessor, maskAccessor);
            if (itemAccessor.PropertyAccess != null)
            {
                fg.AppendLine($"{itemAccessor.PropertyAccess}.{nameof(INotifyingCollectionExt.SetIfSucceeded)}(dictTryGet);");
            }
            else
            {
                fg.AppendLine($"if (dictTryGet.Succeeded)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{itemAccessor.DirectAccess}.SetTo(dictTryGet.Value, cmds: null);");
                }
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
            var dictType = typeGen as IDictType;
            if (!XmlMod.TryGetTypeGeneration(dictType.KeyTypeGen.GetType(), out var keySubTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + dictType.KeyTypeGen);
            }
            var keySubMaskStr = keySubTransl.MaskModule.GetMaskModule(dictType.KeyTypeGen.GetType()).GetErrorMaskTypeStr(dictType.KeyTypeGen);

            if (!XmlMod.TryGetTypeGeneration(dictType.ValueTypeGen.GetType(), out var valSubTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + dictType.ValueTypeGen);
            }
            var valSubMaskStr = valSubTransl.MaskModule.GetMaskModule(dictType.ValueTypeGen.GetType()).GetErrorMaskTypeStr(dictType.ValueTypeGen);

            string funcStr;
            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    funcStr = $"{retAccessor}DictXmlTranslation<{dictType.KeyTypeGen.TypeName}, {dictType.ValueTypeGen.TypeName}, {keySubMaskStr}, {valSubMaskStr}>.Instance.Parse";
                    break;
                case DictMode.KeyedValue:
                    funcStr = $"{retAccessor}KeyedDictXmlTranslation<{dictType.KeyTypeGen.TypeName}, {dictType.ValueTypeGen.TypeName}, {valSubMaskStr}>.Instance.Parse";
                    break;
                default:
                    throw new NotImplementedException();
            }

            using (var args = new ArgsWrapper(fg, funcStr))
            {
                args.Add($"root: root");
                args.Add($"doMasks: {doMaskAccessor}");
                args.Add($"maskObj: out {maskAccessor}");
                if (dictType.Mode != DictMode.KeyedValue)
                {
                    args.Add((gen) =>
                    {
                        gen.AppendLine($"keyTransl: (XElement r, bool dictDoMasks, out {typeGen.ProtoGen.Gen.MaskModule.GetMaskModule(dictType.KeyTypeGen.GetType()).GetErrorMaskTypeStr(dictType.KeyTypeGen)} dictSubMask) =>");
                        using (new BraceWrapper(gen))
                        {
                            var xmlGen = XmlMod.GetTypeGeneration(dictType.KeyTypeGen.GetType());
                            xmlGen.GenerateCopyInRet(gen, dictType.KeyTypeGen, "r", "return ", "dictDoMasks", "dictSubMask");
                        }
                    });
                }
                args.Add((gen) =>
                {
                    gen.AppendLine($"valTransl: (XElement r, bool dictDoMasks, out {typeGen.ProtoGen.Gen.MaskModule.GetMaskModule(dictType.ValueTypeGen.GetType()).GetErrorMaskTypeStr(dictType.ValueTypeGen)} dictSubMask) =>");
                    using (new BraceWrapper(gen))
                    {
                        var xmlGen = XmlMod.GetTypeGeneration(dictType.ValueTypeGen.GetType());
                        xmlGen.GenerateCopyInRet(gen, dictType.ValueTypeGen, "r", "return ", "dictDoMasks", "dictSubMask");
                    }
                });
            }
        }

        public override XElement GenerateForXSD(
            XElement rootElement,
            XElement choiceElement, 
            TypeGeneration typeGen,
            string nameOverride = null)
        {
            return null;
        }
    }
}
