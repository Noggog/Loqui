using System;
using Loqui;
using Noggog.Notifying;

namespace Loqui
{
    public interface IClearable
    {
        void Clear(NotifyingUnsetParameters cmds);
    }
}

namespace System
{
    public static class IClearableExt
    {
        public static void Clear(this IClearable c)
        {
            c.Clear(cmds: null);
        }
    }
}