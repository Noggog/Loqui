using System.Xml.Linq;

namespace Loqui.Generation;

public class EnumXmlTranslationGeneration : XmlTranslationGeneration
{
    public override string GetTranslatorInstance(TypeGeneration typeGen, bool getter)
    {
        var eType = typeGen as EnumType;
        return $"EnumXmlTranslation<{eType.NoNullTypeName}>.Instance";
    }

    PrimitiveXmlTranslationGeneration<string> _subGen = new PrimitiveXmlTranslationGeneration<string>();
    public override void GenerateWrite(
        StructuredStringBuilder sb,
        ObjectGeneration objGen,
        TypeGeneration typeGen,
        Accessor writerAccessor,
        Accessor itemAccessor,
        Accessor errorMaskAccessor,
        Accessor nameAccessor,
        Accessor translationMaskAccessor)
    {
        var eType = typeGen as EnumType;

        using (var args = sb.Args(
                   $"EnumXmlTranslation<{eType.NoNullTypeName}>.Instance.Write"))
        {
            args.Add($"{XmlTranslationModule.XElementLine.GetParameterName(objGen)}: {writerAccessor}");
            args.Add($"name: {nameAccessor}");
            args.Add($"item: {itemAccessor.Access}");
            if (typeGen.HasIndex)
            {
                args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
            }
            args.Add($"errorMask: {errorMaskAccessor}");
        }
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
        var eType = typeGen as EnumType;
        WrapParseCall(
            new TranslationWrapParseArgs()
            {
                FG = sb,
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
        _subGen.XmlMod = XmlMod;
        return _subGen.GenerateForXSD(obj, rootElement, choiceElement, typeGen, nameOverride);
    }

    public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
    {
        _subGen.XmlMod = XmlMod;
        _subGen.GenerateForCommonXSD(rootElement, typeGen);
    }
}