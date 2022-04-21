namespace Loqui.Translators;

public interface ITranslationCaster<T>
{
    ITranslation<T> Source { get; }
}