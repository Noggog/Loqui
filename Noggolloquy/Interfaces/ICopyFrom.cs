using System;
using Noggolloquy;
using Noggog.Notifying;

namespace Noggolloquy
{
    public interface ICopyFrom<L, G>
        where L : class, G
        where G : class
    {
        void CopyFieldsFrom(G rhs, G def, NotifyingFireParameters? cmds);
    }
}

namespace System
{
    public static class ICopyFromExt
    {
        public static void CopyFieldsFrom<L, G>(this ICopyFrom<L, G> c, G rhs, NotifyingFireParameters? cmds = null)
            where L : class, G
            where G : class
        {
            c.CopyFieldsFrom(rhs, def: null, cmds: cmds);
        }

        public static void CopyFieldsFrom<L, G>(this ICopyFrom<L, G> c, G rhs, G def)
            where L : class, G
            where G : class
        {
            c.CopyFieldsFrom(rhs, def: def, cmds: null);
        }
    }
}
