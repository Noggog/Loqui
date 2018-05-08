using Noggog;
using System;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class ColorXmlTranslation : PrimitiveXmlTranslation<Color>
    {
        public const string R = "R";
        public const string G = "G";
        public const string B = "B";
        public readonly static ColorXmlTranslation Instance = new ColorXmlTranslation();

        protected override Color ParseNonNullString(string str)
        {
            throw new NotImplementedException();
        }

        protected override Color? ParseValue(XElement root)
        {
            byte? r = null, g = null, b = null;
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
            if (!r.HasValue
                && !g.HasValue
                && !b.HasValue)
            {
                return null;
            }
            return Color.FromRgb(r ?? 0, g ?? 0, b ?? 0);
        }

        protected override void WriteValue(XElement node, Color? item)
        {
            if (!item.HasValue) return;
            node.SetAttributeValue(R, item.Value.R.ToString());
            node.SetAttributeValue(G, item.Value.G.ToString());
            node.SetAttributeValue(B, item.Value.B.ToString());
        }
    }
}
