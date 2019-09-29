using Loqui.Internal;
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

        public override string GetTranslatorInstance(TypeGeneration typeGen, bool getter)
        {
            var dictType = typeGen as DictType;
            return $"{TranslatorName}<{dictType.KeyTypeGen.TypeName(getter)}, {dictType.ValueTypeGen.TypeName(getter)}>.Instance";
        }

        public override void GenerateWrite(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            Accessor writerAccessor,
            Accessor itemAccessor,
            Accessor errorMaskAccessor,
            Accessor nameAccessor,
            Accessor translationMaskAccessor)
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

                    using (var args = new ArgsWrapper(fg,
                        $"DictXmlTranslation<{dictType.KeyTypeGen.TypeName(getter: true)}, {dictType.ValueTypeGen.TypeName(getter: true)}>.Instance.Write"))
                    {
                        args.Add($"writer: {writerAccessor}");
                        args.Add($"name: {nameAccessor}");
                        args.Add($"items: {itemAccessor.DirectAccess}");
                        if (typeGen.HasIndex)
                        {
                            args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                            args.Add($"errorMask: {errorMaskAccessor}");
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                        args.Add($"translationMask: {translationMaskAccessor}");
                        args.Add((gen) =>
                        {
                            gen.AppendLine($"keyTransl: ({dictType.KeyTypeGen.TypeName(getter: true)} subItem, ErrorMaskBuilder dictSubMask, {nameof(TranslationCrystal)} dictSubTranslMask) =>");
                            using (new BraceWrapper(gen))
                            {
                                keyTransl.GenerateWrite(
                                    fg: gen,
                                    objGen: objGen,
                                    typeGen: dictType.KeyTypeGen,
                                    writerAccessor: "writer",
                                    itemAccessor: $"subItem",
                                    errorMaskAccessor: $"dictSubMask",
                                    translationMaskAccessor: "dictSubTranslMask",
                                    nameAccessor: "\"Item\"");
                            }
                        });
                        args.Add((gen) =>
                        {
                            gen.AppendLine($"valTransl: ({dictType.ValueTypeGen.TypeName(getter: true)} subItem, ErrorMaskBuilder dictSubMask, {nameof(TranslationCrystal)} dictSubTranslMask) =>");
                            using (new BraceWrapper(gen))
                            {
                                valTransl.GenerateWrite(
                                    fg: gen,
                                    objGen: objGen,
                                    typeGen: dictType.ValueTypeGen,
                                    writerAccessor: "writer",
                                    itemAccessor: $"subItem",
                                    errorMaskAccessor: $"dictSubMask",
                                    translationMaskAccessor: "dictSubTranslMask",
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
                                $"KeyedDictXmlTranslation<{dictType.KeyTypeGen.TypeName(getter: true)}, {dictType.ValueTypeGen.TypeName(getter: true)}>.Instance.Write"))
                            {
                                args.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {writerAccessor}");
                                args.Add($"name: {nameAccessor}");
                                args.Add($"items: {itemAccessor.DirectAccess}.Items");
                                args.Add($"translationMask: {translationMaskAccessor}");
                                args.Add("errorMask: errorMask");
                                args.Add((gen) =>
                                {
                                    gen.AppendLine($"valTransl: (XElement subNode, {dictType.ValueTypeGen.TypeName(getter: true)} subItem, ErrorMaskBuilder dictSubMask, {nameof(TranslationCrystal)} dictTranslMask) =>");
                                    using (new BraceWrapper(gen))
                                    {
                                        valTransl.GenerateWrite(
                                            fg: gen,
                                            objGen: objGen,
                                            typeGen: dictType.ValueTypeGen,
                                            writerAccessor: "subNode",
                                            itemAccessor: new Accessor($"subItem"),
                                            errorMaskAccessor: $"dictSubMask",
                                            translationMaskAccessor: "dictTranslMask",
                                            nameAccessor: "null");
                                    }
                                });
                            }
                        },
                        errorMaskAccessor: errorMaskAccessor,
                        indexAccessor: typeGen.IndexEnumInt);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void GenerateCopyIn(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            Accessor nodeAccessor,
            Accessor itemAccessor,
            Accessor errorMaskAccessor,
            Accessor translationMaskAccessor)
        {
            GenerateCopyIn_Internal(
                fg: fg,
                objGen: objGen,
                typeGen: typeGen,
                nodeAccessor: nodeAccessor,
                itemAccessor: itemAccessor,
                ret: false,
                translationMaskAccessor: translationMaskAccessor,
                errorMaskAccessor: errorMaskAccessor);
        }

        private void GenerateCopyIn_Internal(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            Accessor nodeAccessor,
            Accessor itemAccessor,
            bool ret,
            Accessor errorMaskAccessor,
            Accessor translationMaskAccessor)
        {
            var dictType = typeGen as IDictType;
            TypeGeneration keyTypeGen = dictType.KeyTypeGen;
            if (keyTypeGen == null)
            {
                if (!this.XmlMod.Gen.TryGetTypeGeneration(dictType.KeyTypeGen.TypeName(getter: false), out keyTypeGen))
                {
                    throw new ArgumentException($"Could not find dictionary key type for {dictType.KeyTypeGen.TypeName(getter: false)}");
                }
            }

            if (!XmlMod.TryGetTypeGeneration(dictType.ValueTypeGen.GetType(), out var valSubTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + dictType.ValueTypeGen);
            }
            var valSubMaskStr = valSubTransl.MaskModule.GetMaskModule(dictType.ValueTypeGen.GetType()).GetTranslationMaskTypeStr(dictType.ValueTypeGen);

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
                    funcStr = $"{prefix}DictXmlTranslation<{dictType.KeyTypeGen.TypeName(getter: false)}, {dictType.ValueTypeGen.TypeName(getter: false)}>.Instance.Parse{(ret ? null : "Into")}";
                    break;
                case DictMode.KeyedValue:
                    funcStr = $"{prefix}KeyedDictXmlTranslation<{dictType.KeyTypeGen.TypeName(getter: false)}, {dictType.ValueTypeGen.TypeName(getter: false)}>.Instance.Parse{(ret ? null : "Into")}";
                    break;
                default:
                    throw new NotImplementedException();
            }

            using (var args = new ArgsWrapper(fg, funcStr))
            {
                args.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {XmlTranslationModule.XElementLine.GetParameterName(objGen)}");
                if (!ret)
                {
                    args.Add($"item: {itemAccessor.DirectAccess}");
                }
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                    args.Add($"errorMask: {errorMaskAccessor}");
                }
                else
                {
                    throw new NotImplementedException();
                }
                args.Add($"translationMask: {translationMaskAccessor}");
                if (dictType.Mode != DictMode.KeyedValue)
                {
                    throw new NotImplementedException();
                    //args.Add((gen) =>
                    //{
                    //    gen.AppendLine($"keyTransl: (XElement r, ErrorMaskBuilder dictErrMask, {valSubMaskStr} dictTranslMask) =>");
                    //    using (new BraceWrapper(gen))
                    //    {
                    //        var xmlGen = XmlMod.GetTypeGeneration(keyTypeGen.GetType());
                    //        xmlGen.GenerateCopyInRet(
                    //            fg: gen,
                    //            objGen: objGen,
                    //            typeGen: keyTypeGen,
                    //            nodeAccessor: "r",
                    //            retAccessor: new Accessor("return "),
                    //            translationMaskAccessor: "dictTranslMask",
                    //            errorMaskAccessor: "dictErrMask");
                    //    }
                    //});
                }
                switch (dictType.Mode)
                {
                    case DictMode.KeyValue:
                        throw new NotImplementedException();
                        //args.Add((gen) =>
                        //{
                        //    var xmlGen = XmlMod.GetTypeGeneration(dictType.ValueTypeGen.GetType());
                        //    gen.AppendLine($"valTransl: (XElement r, ErrorMaskBuilder dictErrMask, {valSubMaskStr} dictTranslMask) =>");
                        //    using (new BraceWrapper(gen))
                        //    {
                        //        xmlGen.GenerateCopyInRet(
                        //            fg: gen,
                        //            objGen: objGen,
                        //            typeGen: dictType.ValueTypeGen,
                        //            nodeAccessor: "r",
                        //            retAccessor: new Accessor("return "),
                        //            translationMaskAccessor: "dictTranslMask",
                        //            errorMaskAccessor: "dictErrMask");
                        //    }
                        //});
                        break;
                    case DictMode.KeyedValue:
                        args.Add($"valTransl: {valSubTransl.GetTranslatorInstance(dictType.ValueTypeGen, getter: false)}.Parse");
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
        }

        public override void GenerateCopyInRet(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            Accessor nodeAccessor,
            Accessor retAccessor,
            Accessor outItemAccessor,
            Accessor errorMaskAccessor,
            Accessor translationMaskAccessor)
        {
            this.GenerateCopyIn_Internal(
                fg: fg,
                objGen: objGen,
                typeGen: typeGen,
                nodeAccessor: nodeAccessor,
                ret: true,
                itemAccessor: retAccessor,
                translationMaskAccessor: translationMaskAccessor,
                errorMaskAccessor: errorMaskAccessor);
        }

        public override XElement GenerateForXSD(
            ObjectGeneration obj,
            XElement rootElement,
            XElement choiceElement,
            TypeGeneration typeGen,
            string nameOverride = null)
        {
            if (!(typeGen is DictType dict))
            {
                throw new ArgumentException();
            }
            if (dict.Mode == DictMode.KeyValue)
            {
                throw new NotImplementedException();
            }
            var elem = new XElement(XmlTranslationModule.XSDNamespace + "element",
                new XAttribute("name", nameOverride ?? typeGen.Name),
                new XAttribute("type", $"{typeGen.Name}Type"));
            choiceElement.Add(elem);

            var subChoice = new XElement(XmlTranslationModule.XSDNamespace + "choice",
                new XAttribute("minOccurs", 0),
                new XAttribute("maxOccurs", "unbounded"));
            rootElement.Add(
                new XElement(XmlTranslationModule.XSDNamespace + "complexType",
                    new XAttribute("name", $"{typeGen.Name}Type"),
                    subChoice));
            
            var xmlGen = XmlMod.GetTypeGeneration(dict.ValueTypeGen.GetType());
            var subElem = xmlGen.GenerateForXSD(
                obj,
                rootElement,
                subChoice,
                dict.ValueTypeGen,
                nameOverride: "Item");
            return elem;
        }

        public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
        {
        }
    }
}
