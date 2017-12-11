using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class PrimitiveXmlTranslationGeneration<T> : XmlTranslationGeneration
    {
        private string typeName;
        private bool? nullable;
        public bool Nullable => nullable ?? false || typeof(T).GetName().EndsWith("?");
        public bool CanBeNotNullable = true;

        public PrimitiveXmlTranslationGeneration(string typeName = null, bool? nullable = null)
        {
            this.nullable = nullable;
            this.typeName = typeName ?? typeof(T).GetName().Replace("?", string.Empty);
        }

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
            using (var args = new ArgsWrapper(fg,
                $"{this.typeName}XmlTranslation.Instance.Write"))
            {
                args.Add($"writer: {writerAccessor}");
                args.Add($"name: {nameAccessor}");
                args.Add($"item: {itemAccessor.PropertyOrDirectAccess}");
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
                foreach (var arg in AdditionWriteParameters(
                    fg: fg,
                    objGen: objGen,
                    typeGen: typeGen,
                    writerAccessor: writerAccessor,
                    itemAccessor: itemAccessor,
                    doMaskAccessor: doMaskAccessor,
                    maskAccessor: maskAccessor))
                {
                    args.Add(arg);
                }
            }
        }

        protected virtual IEnumerable<string> AdditionWriteParameters(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            string writerAccessor,
            Accessor itemAccessor,
            string doMaskAccessor,
            string maskAccessor)
        {
            yield break;
        }

        public override void GenerateCopyIn(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor itemAccessor,
            string doMaskAccessor,
            string maskAccessor)
        {
            var pType = typeGen as PrimitiveType;
            using (var args = new ArgsWrapper(fg,
                $"var tryGet = {this.typeName}XmlTranslation.Instance.Parse{(this.Nullable ? null : "NonNull")}"))
            {
                args.Add(nodeAccessor);
                args.Add($"doMasks: {doMaskAccessor}");
                args.Add($"errorMask: out {maskAccessor}");
            }
            if (itemAccessor.PropertyAccess != null)
            {
                fg.AppendLine($"{itemAccessor.PropertyAccess}.{nameof(HasBeenSetItemExt.SetIfSucceeded)}(tryGet);");
            }
            else
            {
                fg.AppendLine("if (tryGet.Succeeded)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{itemAccessor.DirectAccess} = tryGet.Value;");
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
            using (var args = new ArgsWrapper(fg,
                $"{retAccessor}{this.typeName}XmlTranslation.Instance.Parse",
                (this.Nullable ? string.Empty : $".Bubble((o) => o.Value)")))
            {
                args.Add(nodeAccessor);
                if (CanBeNotNullable)
                {
                    args.Add($"nullable: {Nullable.ToString().ToLower()}");
                }
                args.Add($"doMasks: {doMaskAccessor}");
                args.Add($"errorMask: out {maskAccessor}");
            }
        }

        public override XElement GenerateForXSD(
            XElement rootElement,
            XElement choiceElement,
            TypeGeneration typeGen,
            string nameOverride = null)
        {
            var elem = new XElement(XmlTranslationModule.XSDNamespace + "element");
            elem.Add(new XAttribute("name", nameOverride ?? typeGen.Name));
            elem.Add(new XAttribute("type", $"{typeName}Type"));
            choiceElement.Add(elem);

            if (rootElement.Elements().Any((e) => e.Attribute("name")?.Value.Equals($"{typeName}Type") ?? false)) return elem;

            rootElement.Add(
                new XElement(XmlTranslationModule.XSDNamespace + "complexType",
                    new XAttribute("name", $"{typeName}Type"),
                    new XElement(XmlTranslationModule.XSDNamespace + "attribute",
                        new XAttribute("name", "value"),
                        new XAttribute("use", this.Nullable ? "optional" : "required"))));
            return elem;
        }
    }
}
