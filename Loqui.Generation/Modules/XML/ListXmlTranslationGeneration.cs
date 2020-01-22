using Loqui.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class ListXmlTranslationGeneration : XmlTranslationGeneration
    {
        public virtual string TranslatorName => $"ListXmlTranslation";

        public override string GetTranslatorInstance(TypeGeneration typeGen, bool getter)
        {
            var list = typeGen as ListType;
            if (!XmlMod.TryGetTypeGeneration(list.SubTypeGeneration.GetType(), out var subTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + list.SubTypeGeneration);
            }

            var subMaskStr = subTransl.MaskModule.GetMaskModule(list.SubTypeGeneration.GetType()).GetErrorMaskTypeStr(list.SubTypeGeneration);
            return $"{TranslatorName}<{list.SubTypeGeneration.TypeName(getter)}, {subMaskStr}>.Instance";
        }

        protected virtual string GetWriteAccessor(Accessor itemAccessor)
        {
            return itemAccessor.DirectAccess;
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
            var list = typeGen as ListType;
            if (!XmlMod.TryGetTypeGeneration(list.SubTypeGeneration.GetType(), out var subTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + list.SubTypeGeneration);
            }

            var typeName = list.SubTypeGeneration.TypeName(getter: true);
            if (list.SubTypeGeneration is LoquiType loqui)
            {
                typeName = loqui.TypeName(getter: true, internalInterface: true);
            }

            using (var args = new ArgsWrapper(fg,
                $"{TranslatorName}<{typeName}>.Instance.Write"))
            {
                args.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {writerAccessor}");
                args.Add($"name: {nameAccessor}");
                args.Add($"item: {GetWriteAccessor(itemAccessor)}");
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                }
                else
                {
                    throw new NotImplementedException();
                }
                args.Add($"errorMask: {errorMaskAccessor}");
                args.Add($"translationMask: {translationMaskAccessor}?.GetSubCrystal({typeGen.IndexEnumInt})");
                args.Add((gen) =>
                {
                    var subTypeName = list.SubTypeGeneration.TypeName(getter: true);
                    if (list.SubTypeGeneration is LoquiType subLoqui)
                    {
                        subTypeName = subLoqui.TypeName(getter: true, internalInterface: true);
                    }
                    gen.AppendLine($"transl: (XElement subNode, {subTypeName} subItem, ErrorMaskBuilder listSubMask, {nameof(TranslationCrystal)} listTranslMask) =>");
                    using (new BraceWrapper(gen))
                    {
                        subTransl.GenerateWrite(
                            fg: gen,
                            objGen: objGen,
                            typeGen: list.SubTypeGeneration,
                            writerAccessor: "subNode",
                            itemAccessor: new Accessor($"subItem"),
                            errorMaskAccessor: $"listSubMask",
                            translationMaskAccessor: "listTranslMask",
                            nameAccessor: "null");
                    }
                });
                ExtraWriteArgs(itemAccessor, typeGen, args);
            }
        }

        protected virtual void ExtraWriteArgs(
            Accessor itemAccessor,
            TypeGeneration typeGen,
            ArgsWrapper args)
        {
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
            GenerateCopyInRet_Internal(
                fg: fg,
                objGen: objGen,
                typeGen: typeGen,
                nodeAccessor: nodeAccessor,
                itemAccessor: itemAccessor,
                ret: false,
                translationMaskAccessor: translationMaskAccessor,
                errorMaskAccessor: errorMaskAccessor);
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
            GenerateCopyInRet_Internal(
                fg: fg,
                objGen: objGen,
                typeGen: typeGen,
                nodeAccessor: nodeAccessor,
                itemAccessor: retAccessor,
                ret: true,
                errorMaskAccessor: errorMaskAccessor,
                translationMaskAccessor: translationMaskAccessor);
        }

        public void GenerateCopyInRet_Internal(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            Accessor nodeAccessor,
            Accessor itemAccessor,
            bool ret,
            Accessor errorMaskAccessor,
            Accessor translationMaskAccessor)
        {
            var list = typeGen as ListType;
            if (!XmlMod.TryGetTypeGeneration(list.SubTypeGeneration.GetType(), out var subTransl))
            {
                throw new ArgumentException("Unsupported type generator: " + list.SubTypeGeneration);
            }

            if (ret)
            {
                throw new NotImplementedException();
            }

            MaskGenerationUtility.WrapErrorFieldIndexPush(
                fg: fg,
                toDo: () =>
                {
                    using (var args = new FunctionWrapper(
                        fg,
                        $"if ({TranslatorName}<{list.SubTypeGeneration.TypeName(getter: false)}>.Instance.Parse"))
                    {
                        args.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {XmlTranslationModule.XElementLine.GetParameterName(objGen)}");
                        args.Add($"enumer: out var {typeGen.Name}Item");
                        if (subTransl.AdditionalCopyInParams.Any((p) => p(objGen, typeGen).Succeeded))
                        {
                            args.Add((gen) =>
                            {
                                gen.AppendLine($"transl: (XElement subNode, out {list.SubTypeGeneration.TypeName(getter: false)} listSubItem, ErrorMaskBuilder listErrMask, TranslationCrystal listTranslMask) =>");
                                using (new BraceWrapper(gen))
                                {
                                    subTransl.GenerateCopyInRet(
                                        fg: gen,
                                        objGen: objGen,
                                        typeGen: list.SubTypeGeneration,
                                        nodeAccessor: "subNode",
                                        outItemAccessor: "listSubItem",
                                        translationMaskAccessor: "listTranslMask",
                                        retAccessor: "return ",
                                        errorMaskAccessor: "listErrMask");
                                }
                            });
                        }
                        else
                        {
                            args.Add($"transl: {subTransl.GetTranslatorInstance(list.SubTypeGeneration, getter: false)}.Parse");
                        }
                        args.Add("errorMask: errorMask");
                        args.Add($"translationMask: {translationMaskAccessor})");
                    }
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{itemAccessor.DirectAccess}.SetTo({typeGen.Name}Item);");
                    }
                    fg.AppendLine("else");
                    using (new BraceWrapper(fg))
                    {
                        list.GenerateClear(fg, itemAccessor);
                    }
                },
                errorMaskAccessor: errorMaskAccessor,
                indexAccessor: typeGen.IndexEnumInt);
        }

        public override XElement GenerateForXSD(
            ObjectGeneration objGen,
            XElement rootElement,
            XElement choiceElement,
            TypeGeneration typeGen,
            string nameOverride = null)
        {
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

            var list = typeGen as ListType;
            var xmlGen = XmlMod.GetTypeGeneration(list.SubTypeGeneration.GetType());
            var subElem = xmlGen.GenerateForXSD(
                objGen,
                rootElement,
                subChoice,
                list.SubTypeGeneration,
                nameOverride: "Item");
            return elem;
        }

        public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
        {
        }
    }
}
