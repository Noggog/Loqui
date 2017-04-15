using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noggolloquy
{
    public interface INoggolloquyRegistration
    {
        ProtocolDefinition ProtocolDefinition { get; }

        ObjectKey ObjectKey { get; }

        string GUID { get; }

        int FieldCount { get; }

        string GetNthName(ushort index);

        bool GetNthIsNoggolloquy(ushort index);

        bool GetNthIsEnumerable(ushort index);

        bool GetNthIsSingleton(ushort index);

        bool IsNthDerivative(ushort index);

        ushort? GetNameIndex(StringCaseAgnostic name);

        bool IsReadOnly(ushort index);

        Type MaskType { get; }

        Type ErrorMaskType { get; }

        Type ClassType { get; }

        string FullName { get; }

        string Name { get; }
    }
}
