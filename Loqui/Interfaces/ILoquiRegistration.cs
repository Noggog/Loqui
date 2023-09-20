using Noggog;

namespace Loqui;

public interface ILoquiRegistration
{
    ProtocolKey ProtocolKey { get; }

    ushort AdditionalFieldCount { get; }

    ushort FieldCount { get; }

    string GetNthName(ushort index);

    bool GetNthIsLoqui(ushort index);

    bool GetNthIsEnumerable(ushort index);

    bool GetNthIsSingleton(ushort index);

    bool IsNthDerivative(ushort index);

    Type GetNthType(ushort index);

    ushort? GetNameIndex(StringCaseAgnostic name);

    bool IsProtected(ushort index);

    Type MaskType { get; }

    Type ErrorMaskType { get; }

    Type ClassType { get; }

    Type GetterType { get; }

    Type SetterType { get; }

    Type? InternalGetterType { get; }

    Type? InternalSetterType { get; }

    string FullName { get; }

    string Name { get; }

    string Namespace { get; }

    byte GenericCount { get; }

    Type? GenericRegistrationType { get; }
}