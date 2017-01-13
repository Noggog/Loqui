using Noggog;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy
{
    public class Bounding2DXmlTranslation : IXmlTranslation<Bounding2D?>
    {
        public string ElementName { get { return "Bounding2D"; } }
        public readonly static Bounding2DXmlTranslation Instance = new Bounding2DXmlTranslation();

        public TryGet<Bounding2D> ParseNoNull(XElement root)
        {
            var ret = Parse(root);
            if (ret.Failed) return ret.BubbleFailure<Bounding2D>();
            if (!ret.Value.HasValue)
            {
                return TryGet<Bounding2D>.Failure("Had no values set.");
            }
            return TryGet<Bounding2D>.Success(ret.Value.Value);
        }

        public TryGet<Bounding2D?> Parse(XElement root)
        {
            int pointLeft, pointTop, pointRight, pointBottom;
            bool anyGot = false;
            XAttribute val;
            if (root.TryGetAttribute("Left", out val))
            {
                if (!int.TryParse(val.Value, out pointLeft))
                {
                    return TryGet<Bounding2D?>.Failure($"Left had malformed data: {val.Value}");
                }
                anyGot = true;
            }
            else
            {
                pointLeft = 0;
            }

            if (root.TryGetAttribute("Top", out val))
            {
                if (!int.TryParse(val.Value, out pointTop))
                {
                    return TryGet<Bounding2D?>.Failure($"Top had malformed data: {val.Value}");
                }
                anyGot = true;
            }
            else
            {
                pointTop = 0;
            }

            if (root.TryGetAttribute("Right", out val))
            {
                if (!int.TryParse(val.Value, out pointRight))
                {
                    return TryGet<Bounding2D?>.Failure($"Right had malformed data: {val.Value}");
                }
                anyGot = true;
            }
            else
            {
                pointRight = 0;
            }

            if (root.TryGetAttribute("Bottom", out val))
            {
                if (!int.TryParse(val.Value, out pointBottom))
                {
                    return TryGet<Bounding2D?>.Failure($"Bottom had malformed data: {val.Value}");
                }
                anyGot = true;
            }
            else
            {
                pointBottom = 0;
            }
            
            if (!anyGot)
            {
                return TryGet<Bounding2D?>.Success(null);
            }

            return TryGet<Bounding2D?>.Success(
                new Bounding2D(
                    xl: pointLeft,
                    xr: pointRight, 
                    yb: pointBottom, 
                    yt: pointTop));
        }

        public void Write(XmlWriter writer, string name, Bounding2D item)
        {
            this.Write(writer, name, (Bounding2D?)item);
        }

        public void Write(XmlWriter writer, string name, Bounding2D? item)
        {
            using (new ElementWrapper(writer, ElementName))
            {
                if (name != null)
                {
                    writer.WriteAttributeString("name", name);
                }

                if (item.HasValue)
                {
                    writer.WriteAttributeString("Left", item.Value.Left.ToString());
                    writer.WriteAttributeString("Top", item.Value.Top.ToString());
                    writer.WriteAttributeString("Right", item.Value.Right.ToString());
                    writer.WriteAttributeString("Bottom", item.Value.Bottom.ToString());
                }
            }
        }
    }
}
