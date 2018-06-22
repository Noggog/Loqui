using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class EnumXmlTranslationGeneration : XmlTranslationGeneration
    {
        public override string GetTranslatorInstance(TypeGeneration typeGen)
        {
            var eType = typeGen as EnumType;
            return $"EnumXmlTranslation<{eType.NoNullTypeName}>.Instance";
        }

        PrimitiveXmlTranslationGeneration<string> _subGen = new PrimitiveXmlTranslationGeneration<string>();
        public override void GenerateWrite(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            string writerAccessor,
            Accessor itemAccessor,
            string maskAccessor,
            string nameAccessor)
        {
            var eType = typeGen as EnumType;

            using (var args = new ArgsWrapper(fg,
                $"EnumXmlTranslation<{eType.NoNullTypeName}>.Instance.Write"))
            {
                args.Add($"node: {writerAccessor}");
                args.Add($"name: {nameAccessor}");
                args.Add($"item: {itemAccessor.PropertyOrDirectAccess}");
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                }
                args.Add($"errorMask: {maskAccessor}");
            }
        }

        public override void GenerateCopyIn(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor itemAccessor,
            string maskAccessor)
        {
            var eType = typeGen as EnumType;
            string prefix = typeGen.PrefersProperty ? null : $"{itemAccessor.DirectAccess} = ";
            using (var args = new ArgsWrapper(fg,
                $"{prefix}EnumXmlTranslation<{eType.NoNullTypeName}>.Instance.Parse{(typeGen.PrefersProperty ? "Into" : null)}"))
            {
                args.Add(nodeAccessor);
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
            var eType = typeGen as EnumType;
            using (var args = new ArgsWrapper(fg,
                $"{retAccessor.DirectAccess}EnumXmlTranslation<{eType.NoNullTypeName}>.Instance.Parse{(eType.Nullable ? null : "NonNull")}"))
            {
                args.Add(nodeAccessor);
                args.Add($"index: {indexAccessor}");
                args.Add($"errorMask: out {maskAccessor}");
            }
        }

        public override XElement GenerateForXSD(
            ObjectGeneration obj,
            XElement rootElement, 
            XElement choiceElement,
            TypeGeneration typeGen,
            string nameOverride = null)
        {
            return _subGen.GenerateForXSD(obj, rootElement, choiceElement, typeGen, nameOverride);
        }

        public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
        {
            _subGen.GenerateForCommonXSD(rootElement, typeGen);
        }
    }
}
