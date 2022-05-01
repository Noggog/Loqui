using Noggog.StructuredStrings;

namespace Loqui.Generation;

public class XmlWriteGenerationParameters
{
    public XmlTranslationModule XmlGen;
    public ObjectGeneration Object;
    public StructuredStringBuilder FG;
    public object Field;
    public string Accessor;
    public string Name;
    public string ErrorMaskAccessor;
}