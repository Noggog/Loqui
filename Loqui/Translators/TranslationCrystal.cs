namespace Loqui.Internal;

public class TranslationCrystal
{
    public (bool On, TranslationCrystal? SubCrystal)[] Crystal;

    public TranslationCrystal((bool On, TranslationCrystal? SubCrystal)[] crystal)
    {
        Crystal = crystal;
    }

    public bool GetShouldTranslate(ushort index)
    {
        if (Crystal.Length <= index) return true;
        return Crystal[index].On;
    }

    public TranslationCrystal? GetSubCrystal(ushort index)
    {
        if (Crystal.Length <= index) return null;
        return Crystal[index].SubCrystal;
    }

    public bool CopyNothing => Crystal.All(c => !c.On && (c.SubCrystal?.CopyNothing ?? true));
}