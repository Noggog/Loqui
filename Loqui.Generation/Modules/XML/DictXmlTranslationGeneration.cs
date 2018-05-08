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
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            string writerAccessor,
            Accessor itemAccessor,
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
                        args.Add($"items: {itemAccessor.DirectAccess}");
                        if (typeGen.HasIndex)
                        {
                            args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                            args.Add($"errorMask: {maskAccessor}");
                        }
                        else
                        {
                            args.Add($"doMasks: {doMaskAccessor}");
                            args.Add($"errorMask: out {maskAccessor}");
                        }
                        args.Add((gen) =>
                        {
                            gen.AppendLine($"keyTransl: ({dictType.KeyTypeGen.TypeName} subItem, bool dictDoMask, out {keyMask} dictSubMask) =>");
                            using (new BraceWrapper(gen))
                            {
                                keyTransl.GenerateWrite(
                                    fg: gen,
                                    objGen: objGen,
                                    typeGen: dictType.KeyTypeGen,
                                    writerAccessor: "writer",
                                    itemAccessor: new Accessor($"subItem"),
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
                                    objGen: objGen,
                                    typeGen: dictType.ValueTypeGen,
                                    writerAccessor: "writer",
                                    itemAccessor: new Accessor($"subItem"),
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
                        args.Add($"node: {writerAccessor}");
                        args.Add($"name: {nameAccessor}");
                        args.Add($"items: {itemAccessor.DirectAccess}.Values");
                        if (typeGen.HasIndex)
                        {
                            args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                            args.Add($"errorMask: {maskAccessor}");
                        }
                        else
                        {
                            args.Add($"doMasks: {doMaskAccessor}");
                            args.Add($"errorMask: out {maskAccessor}");
                        }
                        args.Add((gen) =>
                        {
                            gen.AppendLine($"valTransl: (XElement subNode, {dictType.ValueTypeGen.TypeName} subItem, bool dictDoMask, out {mask} dictSubMask) =>");
                            using (new BraceWrapper(gen))
                            {
                                valTransl.GenerateWrite(
                                    fg: gen,
                                    objGen: objGen,
                                    typeGen: dictType.ValueTypeGen,
                                    writerAccessor: "subNode",
                                    itemAccessor: new Accessor($"subItem"),
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
            GenerateCopyIn_Internal(
                fg: fg,
                typeGen: typeGen,
                nodeAccessor: nodeAccessor,
                itemAccessor: itemAccessor,
                ret: false,
                doMaskAccessor: doMaskAccessor,
                maskAccessor: maskAccessor);
        }

        private void GenerateCopyIn_Internal(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor itemAccessor,
            bool ret,
            string doMaskAccessor,
            string maskAccessor)
        {
            var dictType = typeGen as IDictType;
            XmlTranslationGeneration keySubTransl;
            TypeGeneration keyTypeGen = dictType.KeyTypeGen;
            if (keyTypeGen == null)
            {
                if (!this.XmlMod.Gen.TryGetTypeGeneration(dictType.KeyTypeGen.TypeName, out keyTypeGen))
                {
                    throw new ArgumentException($"Could not find dictionary key type for {dictType.KeyTypeGen.TypeName}");
                }
            }
            if (!XmlMod.TryGetTypeGeneration(keyTypeGen.GetType(), out keySubTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + keyTypeGen);
            }
            var keySubMaskStr = keySubTransl.MaskModule.GetMaskModule(keyTypeGen.GetType()).GetErrorMaskTypeStr(keyTypeGen);

            if (!XmlMod.TryGetTypeGeneration(dictType.ValueTypeGen.GetType(), out var valSubTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + dictType.ValueTypeGen);
            }
            var valSubMaskStr = valSubTransl.MaskModule.GetMaskModule(dictType.ValueTypeGen.GetType()).GetErrorMaskTypeStr(dictType.ValueTypeGen);

            string prefix, suffix;
            bool isProperty = itemAccessor?.PropertyAccess != null;
            if (!ret)
            {
                suffix = isProperty ? ")" : null;
                prefix = isProperty ? $"{itemAccessor.PropertyAccess}.{nameof(HasBeenSetItemExt.SetIfSucceeded)}(" : $"var {typeGen.Name}dict = ";
            }
            else
            {
                suffix = null;
                prefix = itemAccessor.DirectAccess;
            }

            string funcStr;
            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    funcStr = $"{prefix}DictXmlTranslation<{dictType.KeyTypeGen.TypeName}, {dictType.ValueTypeGen.TypeName}, {keySubMaskStr}, {valSubMaskStr}>.Instance.Parse";
                    break;
                case DictMode.KeyedValue:
                    funcStr = $"{prefix}KeyedDictXmlTranslation<{dictType.KeyTypeGen.TypeName}, {dictType.ValueTypeGen.TypeName}, {valSubMaskStr}>.Instance.Parse";
                    break;
                default:
                    throw new NotImplementedException();
            }

            using (var args = new ArgsWrapper(fg, funcStr,
                suffixLine: suffix))
            {
                args.Add($"root: root");
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                    args.Add($"errorMask: {maskAccessor}");
                }
                else
                {
                    args.Add($"doMasks: {doMaskAccessor}");
                    args.Add($"errorMask: out {maskAccessor}");
                }
                if (dictType.Mode != DictMode.KeyedValue)
                {
                    args.Add((gen) =>
                    {
                        gen.AppendLine($"keyTransl: (XElement r, bool dictDoMasks, out {typeGen.ProtoGen.Gen.MaskModule.GetMaskModule(keyTypeGen.GetType()).GetErrorMaskTypeStr(keyTypeGen)} dictSubMask) =>");
                        using (new BraceWrapper(gen))
                        {
                            var xmlGen = XmlMod.GetTypeGeneration(keyTypeGen.GetType());
                            xmlGen.GenerateCopyInRet(gen, keyTypeGen, "r", new Accessor("return "), "dictDoMasks", "dictSubMask");
                        }
                    });
                }
                args.Add((gen) =>
                {
                    gen.AppendLine($"valTransl: (XElement r, bool dictDoMasks, out {typeGen.ProtoGen.Gen.MaskModule.GetMaskModule(dictType.ValueTypeGen.GetType()).GetErrorMaskTypeStr(dictType.ValueTypeGen)} dictSubMask) =>");
                    using (new BraceWrapper(gen))
                    {
                        var xmlGen = XmlMod.GetTypeGeneration(dictType.ValueTypeGen.GetType());
                        xmlGen.GenerateCopyInRet(gen, dictType.ValueTypeGen, "r", new Accessor("return "), "dictDoMasks", "dictSubMask");
                    }
                });
            }

            if (!ret && !isProperty)
            {
                fg.AppendLine($"if ({typeGen.Name}dict.Succeeded)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{itemAccessor.DirectAccess}.SetTo({typeGen.Name}dict.Value);");
                }
            }
        }

        public override void GenerateCopyInRet(
            FileGeneration fg,
            TypeGeneration typeGen, 
            string nodeAccessor,
            Accessor retAccessor,
            string doMaskAccessor,
            string maskAccessor)
        {
            this.GenerateCopyIn_Internal(
                fg: fg,
                typeGen: typeGen,
                nodeAccessor: nodeAccessor,
                ret: true,
                doMaskAccessor: doMaskAccessor,
                itemAccessor: retAccessor,
                maskAccessor: maskAccessor);
        }

        public override XElement GenerateForXSD(
            ObjectGeneration obj,
            XElement rootElement,
            XElement choiceElement, 
            TypeGeneration typeGen,
            string nameOverride = null)
        {
            return null;
        }

        public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
        {
        }
    }
}
