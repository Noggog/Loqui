using Loqui.Internal;
using Noggog;
using System;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class ColorXmlTranslation : PrimitiveXmlTranslation<Color>
    {
        public const string A = "A";
        public const string R = "R";
        public const string G = "G";
        public const string B = "B";
        public readonly static ColorXmlTranslation Instance = new ColorXmlTranslation();

        protected override bool ParseNonNullString(string str, out Color value, ErrorMaskBuilder errorMask)
        {
            throw new NotImplementedException();
        }

        protected override bool ParseValue(XElement root, out Color? value, ErrorMaskBuilder errorMask)
        {
            byte? r = null, g = null, b = null, a = null;
            if (root.TryGetAttribute(A, out XAttribute aAtt))
            {
                a = byte.Parse(aAtt.Value);
            }
            if (root.TryGetAttribute(R, out XAttribute rAtt))
            {
                r = byte.Parse(rAtt.Value);
            }
            if (root.TryGetAttribute(G, out XAttribute gAtt))
            {
                g = byte.Parse(gAtt.Value);
            }
            if (root.TryGetAttribute(B, out XAttribute bAtt))
            {
                b = byte.Parse(bAtt.Value);
            }
            if (!a.HasValue
                && !r.HasValue
                && !g.HasValue
                && !b.HasValue)
            {
                value = null;
                return true;
            }
            value = Color.FromArgb(alpha: a ?? 0, red: r ?? 0, green: g ?? 0, blue: b ?? 0);
            return true;
        }

        protected override void WriteValue(XElement node, Color? item)
        {
            if (!item.HasValue) return;
            node.SetAttributeValue(A, item.Value.A.ToString());
            node.SetAttributeValue(R, item.Value.R.ToString());
            node.SetAttributeValue(G, item.Value.G.ToString());
            node.SetAttributeValue(B, item.Value.B.ToString());
        }
    }
}
