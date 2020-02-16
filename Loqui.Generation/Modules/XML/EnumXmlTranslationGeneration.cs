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
        public override string GetTranslatorInstance(TypeGeneration typeGen, bool getter)
        {
            var eType = typeGen as EnumType;
            return $"EnumXmlTranslation<{eType.NoNullTypeName}>.Instance";
        }

        PrimitiveXmlTranslationGeneration<string> _subGen = new PrimitiveXmlTranslationGeneration<string>();
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
            var eType = typeGen as EnumType;

            using (var args = new ArgsWrapper(fg,
                $"EnumXmlTranslation<{eType.NoNullTypeName}>.Instance.Write"))
            {
                args.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {writerAccessor}");
                args.Add($"name: {nameAccessor}");
                args.Add($"item: {itemAccessor.DirectAccess}");
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                }
                args.Add($"errorMask: {errorMaskAccessor}");
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
            var eType = typeGen as EnumType;
            TranslationGeneration.WrapParseCall(
                new TranslationWrapParseArgs()
                {
                    FG = fg,
                    TypeGen = typeGen,
                    TranslatorLine = $"EnumXmlTranslation<{eType.NoNullTypeName}>.Instance",
                    MaskAccessor = errorMaskAccessor,
                    ItemAccessor = itemAccessor,
                    TranslationMaskAccessor = null,
                    IndexAccessor = typeGen.IndexEnumInt,
                    ExtraArgs = new string[]
                    {
                        $"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {nodeAccessor}"
                    }
                });
        }

        public override XElement GenerateForXSD(
            ObjectGeneration obj,
            XElement rootElement,
            XElement choiceElement,
            TypeGeneration typeGen,
            string nameOverride = null)
        {
            _subGen.XmlMod = this.XmlMod;
            return _subGen.GenerateForXSD(obj, rootElement, choiceElement, typeGen, nameOverride);
        }

        public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
        {
            _subGen.XmlMod = this.XmlMod;
            _subGen.GenerateForCommonXSD(rootElement, typeGen);
        }
    }
}
