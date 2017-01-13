using System;
using Noggolloquy;
using Noggog.Notifying;

namespace Noggolloquy
{
    public interface IClearable
    {
        void Clear(NotifyingUnsetParameters? cmds);
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