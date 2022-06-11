using Noggog.StructuredStrings;

namespace Loqui.Generation;

public class ByteArrayXmlTranslationGeneration : PrimitiveXmlTranslationGeneration<byte[]>
{
    public ByteArrayXmlTranslationGeneration()
        : base(typeName: "ByteArray", nullable: true)
    {
    }

    public override void GenerateCopyIn(
        StructuredStringBuilder sb,
        ObjectGeneration objGen,
        TypeGeneration typeGen,
        Accessor nodeAccessor,
        Accessor itemAccessor,
        Accessor errorMaskAccessor,
        Accessor translationMaskAccessor)
    {
        var byteArray = typeGen as ByteArrayType;

        List<string> extraArgs = new List<string>();
        extraArgs.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen, Context.Backend)}: {nodeAccessor}");

        if (byteArray.Length != null)
        {
            extraArgs.Add($"fallbackLength: {byteArray.Length}");
        }
        else if (!byteArray.Nullable)
        {
            extraArgs.Add($"fallbackLength: 0");
        }

        WrapParseCall(
            new TranslationWrapParseArgs()
            {
                FG = sb,
                TypeGen = typeGen,
                TranslatorLine = $"{TypeName(typeGen)}XmlTranslation.Instance",
                MaskAccessor = errorMaskAccessor,
                ItemAccessor = itemAccessor,
                TranslationMaskAccessor = null,
                IndexAccessor = new Accessor(typeGen.IndexEnumInt),
                ExtraArgs = extraArgs.ToArray()
            });
    }
}