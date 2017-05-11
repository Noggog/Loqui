using Noggog.Notifying;
using System;

namespace Loqui
{
    public interface ICopyInAble
    {
        void CopyFieldsFrom(object rhs, object def, NotifyingFireParameters? cmds);
    }
}
