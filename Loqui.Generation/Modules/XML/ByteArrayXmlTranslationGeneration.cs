using System;
using System.Collections.Generic;
using System.Text;

namespace Loqui.Generation
{
    public class ByteArrayXmlTranslationGeneration : PrimitiveXmlTranslationGeneration<byte[]>
    {
        public ByteArrayXmlTranslationGeneration()
            : base(typeName: "ByteArray", nullable: true)
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
            var byteArray = typeGen as ByteArrayType;

            List<string> extraArgs = new List<string>();
            extraArgs.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {nodeAccessor}");
            foreach (var writeParam in this.AdditionalCopyInParams)
            {
                var get = writeParam(
                    objGen: objGen,
                    typeGen: typeGen);
                if (get.Failed) continue;
                extraArgs.Add(get.Value);
            }

            if (byteArray.Length != null)
            {
                extraArgs.Add($"fallbackLength: {byteArray.Length}");
            }

            TranslationGeneration.WrapParseCall(
                new TranslationWrapParseArgs()
                {
                    FG = fg,
                    TypeGen = typeGen,
                    TranslatorLine = $"{this.TypeName(typeGen)}XmlTranslation.Instance",
                    MaskAccessor = errorMaskAccessor,
                    ItemAccessor = itemAccessor,
                    TranslationMaskAccessor = null,
                    IndexAccessor = new Accessor(typeGen.IndexEnumInt),
                    ExtraArgs = extraArgs.ToArray()
                });
        }
    }
}
