using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Loqui.Presentation
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
    }
}
