using System.Xml.Linq;

namespace Loqui.Generation;

public class NothingXmlTranslationGeneration : XmlTranslationGeneration
{
    public override string GetTranslatorInstance(TypeGeneration typeGen, bool getter)
    {
        throw new NotImplementedException();
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
    }

    public override XElement GenerateForXSD(ObjectGeneration obj, XElement rootElement, XElement choiceElement, TypeGeneration typeGen, string nameOverride)
    {
        return null;
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
    }

    public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
    {
    }
}