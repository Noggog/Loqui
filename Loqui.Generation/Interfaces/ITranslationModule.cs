using Noggog.StructuredStrings;

namespace Loqui.Generation;

public interface ITranslationModule
{
    string Namespace { get; }
    bool DoErrorMasks { get; }
    Task GenerateTranslationInterfaceImplementation(ObjectGeneration obj, StructuredStringBuilder sb, Context context);
    bool DoTranslationInterface(ObjectGeneration obj);
    void ReplaceTypeAssociation<Target, Replacement>()
        where Target : TypeGeneration
        where Replacement : TypeGeneration;
}