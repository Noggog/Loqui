using Noggog;
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
        private string _typeName;
        public virtual string TypeName => _typeName;
        private bool? nullable;
        public bool Nullable => nullable ?? false || typeof(T).GetName().EndsWith("?");
        public bool CanBeNotNullable = true;

        public PrimitiveXmlTranslationGeneration(string typeName = null, bool? nullable = null)
        {
            this.nullable = nullable;
            this._typeName = typeName ?? typeof(T).GetName().Replace("?", string.Empty);
        }

        protected virtual string ItemWriteAccess(Accessor itemAccessor)
        {
            return itemAccessor.PropertyOrDirectAccess;
        }

        public override string GetTranslatorInstance(TypeGeneration typeGen)
        {
            return $"{this.TypeName}XmlTranslation.Instance";
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
            using (var args = new ArgsWrapper(fg,
                $"{this.TypeName}XmlTranslation.Instance.Write"))
            {
                args.Add($"node: {writerAccessor}");
                args.Add($"name: {nameAccessor}");
                args.Add($"item: {ItemWriteAccess(itemAccessor)}");
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                }
                args.Add($"errorMask: {maskAccessor}");
                foreach (var arg in AdditionWriteParameters(
                    fg: fg,
                    objGen: objGen,
                    typeGen: typeGen,
                    writerAccessor: writerAccessor,
                    itemAccessor: itemAccessor,
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
            string maskAccessor)
        {
            yield break;
        }

        public override void GenerateCopyIn(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor itemAccessor,
            string maskAccessor)
        {
            var pType = typeGen as PrimitiveType;
            using (var args = new ArgsWrapper(fg,
                $"{this.TypeName}XmlTranslation.Instance.Parse{(typeGen.PrefersProperty ? "Into" : null)}"))
            {
                args.Add(nodeAccessor);
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                    if (isProperty)
                    {
                        args.Add($"item: {itemAccessor.PropertyAccess}");
                    }
                    else
                    {
                        args.Add($"item: out {itemAccessor.DirectAccess}");
                    }
                    args.Add($"errorMask: {maskAccessor}");
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            TranslationGenerationSnippets.DirectTryGetSetting(fg, itemAccessor, typeGen);
        }

        public override void GenerateCopyInRet(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor retAccessor,
            string indexAccessor,
            string maskAccessor)
        {
            using (var args = new ArgsWrapper(fg,
                $"{retAccessor.DirectAccess}{this.TypeName}XmlTranslation.Instance.Parse",
                (this.Nullable ? string.Empty : $".Bubble((o) => o.Value)")))
            {
                args.Add(nodeAccessor);
                if (CanBeNotNullable)
                {
                    args.Add($"nullable: {Nullable.ToString().ToLower()}");
                }
                args.Add($"index: {indexAccessor}");
                args.Add($"errorMask: {maskAccessor}");
            }
        }

        public override XElement GenerateForXSD(
            ObjectGeneration obj,
            XElement rootElement,
            XElement choiceElement,
            TypeGeneration typeGen,
            string nameOverride = null)
        {
            var common = this.XmlMod.CommonXSDLocation(obj.ProtoGen);
            var relativePath = common.GetRelativePathTo(this.XmlMod.ObjectXSDLocation(obj));
            var includeElem = new XElement(
                XmlTranslationModule.XSDNamespace + "include",
                new XAttribute("schemaLocation", relativePath));
            if (!rootElement.Elements().Any((e) => e.ContentEqual(includeElem)))
            {
                rootElement.AddFirst(includeElem);
            }

            var elem = new XElement(XmlTranslationModule.XSDNamespace + "element");
            elem.Add(new XAttribute("name", nameOverride ?? typeGen.Name));
            elem.Add(new XAttribute("type", $"ValueType"));
            choiceElement.Add(elem);
            return elem;
        }

        public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
        {
            var nodeName = this.Nullable ? "NullableValueType" : "ValueType";
            if (rootElement.Elements().Any((e) => e.Attribute("name")?.Value.Equals(nodeName) ?? false)) return;

            rootElement.Add(
                new XElement(XmlTranslationModule.XSDNamespace + "complexType",
                    new XAttribute("name", nodeName),
                    new XElement(XmlTranslationModule.XSDNamespace + "attribute",
                        new XAttribute("name", "value"),
                        new XAttribute("use", this.Nullable ? "optional" : "required"))));
        }
    }
}
