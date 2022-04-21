using Loqui.Internal;

namespace Loqui;

public interface ITranslationMask
{
    TranslationCrystal? GetCrystal();
}

public class TranslationMaskStub : ITranslationMask
{
    public TranslationCrystal? GetCrystal() => null;
}