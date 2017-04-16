using Noggog.Notifying;
using System;

namespace Noggolloquy
{
    public interface ICopyInAble
    {
        void CopyFieldsFrom(object rhs, object def, NotifyingFireParameters? cmds);
    }
}
