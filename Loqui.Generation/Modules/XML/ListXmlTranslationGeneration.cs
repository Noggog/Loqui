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
            if (!XmlMod.TryGetTypeGeneration(list.SubTypeGeneration.GetType(), out var subTransl))
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
            if (!XmlMod.TryGetTypeGeneration(list.SubTypeGeneration.GetType(), out var subTransl))
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
                        var xmlGen = XmlMod.GetTypeGeneration(list.SubTypeGeneration.GetType());
                        xmlGen.GenerateCopyInRet(gen, list.SubTypeGeneration, "r", "return ", "listDoMasks", "listSubMask");
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
                 rootElement,
                 subChoice,
                 list.SubTypeGeneration,
                 nameOverride: "Item");
            return elem;
        }
    }
}
