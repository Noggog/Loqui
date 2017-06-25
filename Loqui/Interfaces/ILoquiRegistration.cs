using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public interface ILoquiRegistration
    {
        ProtocolDefinition ProtocolDefinition { get; }

        ObjectKey ObjectKey { get; }

        string GUID { get; }

        int FieldCount { get; }

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

        Type CommonType { get; }

        string FullName { get; }

        string Name { get; }

        byte GenericCount { get; }

        Type GenericRegistrationType { get; }
    }
}
