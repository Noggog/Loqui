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
        public virtual string TranslatorName => "DictXmlTranslation";

        public override string GetTranslatorInstance(TypeGeneration typeGen)
        {
            var dictType = typeGen as DictType;
            var keyMask = this.MaskModule.GetMaskModule(dictType.KeyTypeGen.GetType()).GetErrorMaskTypeStr(dictType.KeyTypeGen);
            var valMask = this.MaskModule.GetMaskModule(dictType.ValueTypeGen.GetType()).GetErrorMaskTypeStr(dictType.ValueTypeGen);
            return $"{TranslatorName}<{dictType.KeyTypeGen.TypeName}, {dictType.ValueTypeGen.TypeName}, {keyMask}, {valMask}>.Instance";
        }

        public override void GenerateWrite(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            string writerAccessor,
            Accessor itemAccessor,
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
                            throw new NotImplementedException();
                        }
                        args.Add((gen) =>
                        {
                            gen.AppendLine($"keyTransl: ({dictType.KeyTypeGen.TypeName} subItem, ErrorMaskBuilder dictSubMask) =>");
                            using (new BraceWrapper(gen))
                            {
                                keyTransl.GenerateWrite(
                                    fg: gen,
                                    objGen: objGen,
                                    typeGen: dictType.KeyTypeGen,
                                    writerAccessor: "writer",
                                    itemAccessor: new Accessor($"subItem"),
                                    maskAccessor: $"dictSubMask",
                                    nameAccessor: "\"Item\"");
                            }
                        });
                        args.Add((gen) =>
                        {
                            gen.AppendLine($"valTransl: ({dictType.ValueTypeGen.TypeName} subItem, ErrorMaskBuilder dictSubMask) =>");
                            using (new BraceWrapper(gen))
                            {
                                valTransl.GenerateWrite(
                                    fg: gen,
                                    objGen: objGen,
                                    typeGen: dictType.ValueTypeGen,
                                    writerAccessor: "writer",
                                    itemAccessor: new Accessor($"subItem"),
                                    maskAccessor: $"dictSubMask",
                                    nameAccessor: "\"Item\"");
                            }
                        });
                    }
                    break;
                case DictMode.KeyedValue:
                    MaskGenerationUtility.WrapErrorFieldIndexPush(
                        fg: fg,
                        toDo: () =>
                        {
                            using (var args = new ArgsWrapper(
                                fg,
                                $"KeyedDictXmlTranslation<{dictType.KeyTypeGen.TypeName}, {dictType.ValueTypeGen.TypeName}>.Instance.Write"))
                            {
                                args.Add($"node: {writerAccessor}");
                                args.Add($"name: {nameAccessor}");
                                args.Add($"items: {itemAccessor.DirectAccess}.Values");
                                args.Add("errorMask: errorMask");
                                args.Add((gen) =>
                                {
                                    gen.AppendLine($"valTransl: (XElement subNode, {dictType.ValueTypeGen.TypeName} subItem, ErrorMaskBuilder dictSubMask) =>");
                                    using (new BraceWrapper(gen))
                                    {
                                        valTransl.GenerateWrite(
                                            fg: gen,
                                            objGen: objGen,
                                            typeGen: dictType.ValueTypeGen,
                                            writerAccessor: "subNode",
                                            itemAccessor: new Accessor($"subItem"),
                                            maskAccessor: $"dictSubMask",
                                            nameAccessor: "\"Item\"");
                                    }
                                });
                            }
                        },
                        maskAccessor: maskAccessor,
                        indexAccessor: typeGen.IndexEnumInt);
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
            string maskAccessor)
        {
            GenerateCopyIn_Internal(
                fg: fg,
                typeGen: typeGen,
                nodeAccessor: nodeAccessor,
                itemAccessor: itemAccessor,
                ret: false,
                indexAccessor: $"(int){typeGen.IndexEnumName}",
                maskAccessor: maskAccessor);
        }

        private void GenerateCopyIn_Internal(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor itemAccessor,
            bool ret,
            string indexAccessor,
            string maskAccessor)
        {
            var dictType = typeGen as IDictType;
            TypeGeneration keyTypeGen = dictType.KeyTypeGen;
            if (keyTypeGen == null)
            {
                if (!this.XmlMod.Gen.TryGetTypeGeneration(dictType.KeyTypeGen.TypeName, out keyTypeGen))
                {
                    throw new ArgumentException($"Could not find dictionary key type for {dictType.KeyTypeGen.TypeName}");
                }
            }

            if (!XmlMod.TryGetTypeGeneration(dictType.ValueTypeGen.GetType(), out var valSubTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + dictType.ValueTypeGen);
            }
            var valSubMaskStr = valSubTransl.MaskModule.GetMaskModule(dictType.ValueTypeGen.GetType()).GetErrorMaskTypeStr(dictType.ValueTypeGen);

            string prefix;
            if (!ret)
            {
                prefix = null;
            }
            else
            {
                prefix = itemAccessor.DirectAccess;
            }

            string funcStr;
            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    funcStr = $"{prefix}DictXmlTranslation<{dictType.KeyTypeGen.TypeName}, {dictType.ValueTypeGen.TypeName}>.Instance.Parse{(ret ? null : "Into")}";
                    break;
                case DictMode.KeyedValue:
                    funcStr = $"{prefix}KeyedDictXmlTranslation<{dictType.KeyTypeGen.TypeName}, {dictType.ValueTypeGen.TypeName}>.Instance.Parse{(ret ? null : "Into")}";
                    break;
                default:
                    throw new NotImplementedException();
            }

            using (var args = new ArgsWrapper(fg, funcStr))
            {
                args.Add($"root: root");
                if (!ret)
                {
                    args.Add($"item: {itemAccessor.DirectAccess}");
                }
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                    args.Add($"errorMask: {maskAccessor}");
                }
                else
                {
                    throw new NotImplementedException();
                }
                if (dictType.Mode != DictMode.KeyedValue)
                {
                    args.Add((gen) =>
                    {
                        gen.AppendLine($"keyTransl: (XElement r, int dictIndex, ErrorMaskBuilder dictErrMask) =>");
                        using (new BraceWrapper(gen))
                        {
                            var xmlGen = XmlMod.GetTypeGeneration(keyTypeGen.GetType());
                            xmlGen.GenerateCopyInRet(
                                fg: gen,
                                typeGen: keyTypeGen,
                                nodeAccessor: "r",
                                retAccessor: new Accessor("return "),
                                indexAccessor: "dictIndex",
                                maskAccessor: "dictErrMask");
                        }
                    });
                }
                switch (dictType.Mode)
                {
                    case DictMode.KeyValue:
                        args.Add((gen) =>
                        {
                            var xmlGen = XmlMod.GetTypeGeneration(dictType.ValueTypeGen.GetType());
                            gen.AppendLine($"valTransl: (XElement r, int dictIndex, ErrorMaskBuilder dictErrMask) =>");
                            using (new BraceWrapper(gen))
                            {
                                xmlGen.GenerateCopyInRet(
                                    fg: gen,
                                    typeGen: dictType.ValueTypeGen,
                                    nodeAccessor: "r",
                                    retAccessor: new Accessor("return "),
                                    indexAccessor: "dictIndex",
                                    maskAccessor: "dictErrMask");
                            }
                        });
                        break;
                    case DictMode.KeyedValue:
                        args.Add($"valTransl: {valSubTransl.GetTranslatorInstance(dictType.ValueTypeGen)}.Parse");
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
        }

        public override void GenerateCopyInRet(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor retAccessor,
            string indexAccessor,
            string maskAccessor)
        {
            this.GenerateCopyIn_Internal(
                fg: fg,
                typeGen: typeGen,
                nodeAccessor: nodeAccessor,
                ret: true,
                itemAccessor: retAccessor,
                indexAccessor: indexAccessor,
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
