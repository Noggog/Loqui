using Noggog;
using System.Xml.Linq;

namespace Loqui.Generation;

public abstract class XmlTranslationGeneration : TranslationGeneration
{
    public XmlTranslationModule XmlMod;

    public delegate TryGet<string> ParamTest(
        ObjectGeneration objGen,
        TypeGeneration typeGen);

    public abstract void GenerateWrite(
        FileGeneration fg,
        ObjectGeneration objGen,
        TypeGeneration typeGen,
        Accessor writerAccessor,
        Accessor itemAccessor,
        Accessor errorMaskAccessor,
        Accessor nameAccessor,
        Accessor translationMaskAccessor);

    public abstract void GenerateCopyIn(
        FileGeneration fg,
        ObjectGeneration objGen,
        TypeGeneration typeGen,
        Accessor nodeAccessor,
        Accessor itemAccessor,
        Accessor errorMaskAccessor,
        Accessor translationMaskAccessor);

    public abstract string GetTranslatorInstance(TypeGeneration typeGen, bool getter);

    public abstract XElement GenerateForXSD(
        ObjectGeneration objGen,
        XElement rootElement,
        XElement choiceElement,
        TypeGeneration typeGen,
        string nameOverride);

    public abstract void GenerateForCommonXSD(
        XElement rootElement,
        TypeGeneration typeGen);
}