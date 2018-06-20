using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class UnsafeXmlTranslationGeneration : XmlTranslationGeneration
    {
        public string ErrMaskString = "object";

        public override string GetTranslatorInstance(TypeGeneration typeGen)
        {
            return $"WildcardXmlTranslation.Instance";
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
                $"WildcardXmlTranslation.Instance.Write"))
            {
                args.Add($"node: {writerAccessor}");
                args.Add($"name: {nameAccessor}");
                args.Add($"item: {itemAccessor.DirectAccess}");
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                    args.Add($"errorMask: {maskAccessor}");
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public override void GenerateCopyIn(
            FileGeneration fg, 
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor itemAccessor,
            string maskAccessor)
        {
            UnsafeType unsafeType = typeGen as UnsafeType;
            var isProperty = itemAccessor.PropertyAccess != null;
            string prefix = isProperty ? null : $"{itemAccessor.DirectAccess} = ";
            using (var args = new ArgsWrapper(fg,
                $"{prefix}WildcardXmlTranslation.Instance.Parse{(isProperty ? "Into" : null)}",
                suffixLine: $".Bubble<{typeGen.TypeName}>(i => ({typeGen.TypeName})i){(isProperty ? null : $".GetOrDefault({itemAccessor.DirectAccess})")}"))
            {
                args.Add($"root: {nodeAccessor}");
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                    if (isProperty)
                    {
                        args.Add($"item: {itemAccessor.PropertyAccess}");
                    }
                    args.Add($"errorMask: {maskAccessor}");
                }
                else
                {
                    throw new NotImplementedException();
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
            UnsafeType unsafeType = typeGen as UnsafeType;
            bool isProperty = retAccessor?.PropertyAccess != null;
            using (var args = new ArgsWrapper(fg,
                $"{retAccessor.DirectAccess}WildcardXmlTranslation.Instance.Parse{(isProperty ? "Into" : null)}"))
            {
                args.Add($"root: {nodeAccessor}");
                if (isProperty)
                {
                    args.Add($"item: {retAccessor.PropertyAccess}");
                }
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
            return null;
        }

        public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
        {
        }
    }
}
