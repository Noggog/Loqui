using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Loqui
{
    public static class ColorExt
    {
        public static bool ColorOnlyEquals(this Color color, Color rhs)
        {
            return color.A == rhs.A
                && color.R == rhs.R
                && color.G == rhs.G
                && color.B == rhs.B;
        }

        public static bool ColorOnlyEquals(this Color? color, Color? rhs)
        {
            if (color.HasValue && rhs.HasValue)
            {
                return ColorOnlyEquals(color.Value, rhs.Value);
            }
            else
            {
                return !color.HasValue && !rhs.HasValue;
            }
        }
    }
}
