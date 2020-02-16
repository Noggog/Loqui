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

        public override string GetTranslatorInstance(TypeGeneration typeGen, bool getter)
        {
            return $"WildcardXmlTranslation.Instance";
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
            using (var args = new ArgsWrapper(fg,
                $"WildcardXmlTranslation.Instance.Write"))
            {
                args.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {writerAccessor}");
                args.Add($"name: {nameAccessor}");
                args.Add($"item: {itemAccessor.DirectAccess}");
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
            UnsafeType unsafeType = typeGen as UnsafeType;
            string prefix = typeGen.PrefersProperty ? null : $"{itemAccessor.DirectAccess} = ";
            using (var args = new ArgsWrapper(fg,
                $"{prefix}WildcardXmlTranslation.Instance.Parse{(typeGen.PrefersProperty ? "Into" : null)}",
                suffixLine: $".Bubble<{typeGen.TypeName(getter: true)}>(i => ({typeGen.TypeName(getter: true)})i){(typeGen.PrefersProperty ? null : $".GetOrDefault({itemAccessor.DirectAccess})")}"))
            {
                args.Add($"root: {nodeAccessor}");
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                    if (typeGen.PrefersProperty)
                    {
                        args.Add($"item: {itemAccessor.PropertyAccess}");
                    }
                    args.Add($"errorMask: {errorMaskAccessor}");
                }
                else
                {
                    throw new NotImplementedException();
                }
                args.Add($"translationMask: {translationMaskAccessor}");
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
