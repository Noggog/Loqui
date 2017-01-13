using Noggog;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy
{
    public class Dimension2DXmlTranslation : IXmlTranslation<Dimension2D?>
    {
        public string ElementName { get { return "Dimension2D"; } }
        public readonly static Dimension2DXmlTranslation Instance = new Dimension2DXmlTranslation();

        public TryGet<Dimension2D> ParseNoNull(XElement root)
        {
            var ret = Parse(root);
            if (ret.Failed) return ret.BubbleFailure<Dimension2D>();
            if (!ret.Value.HasValue)
            {
                return TryGet<Dimension2D>.Failure("Had no values set.");
            }
            return TryGet<Dimension2D>.Success(ret.Value.Value);
        }

        public TryGet<Dimension2D?> Parse(XElement root)
        {
            int width, height;
            bool anyGot = false;
            XAttribute val;
            if (root.TryGetAttribute("Width", out val))
            {
                if (!int.TryParse(val.Value, out width))
                {
                    return TryGet<Dimension2D?>.Failure($"Width had malformed data: {val.Value}");
                }
                anyGot = true;
            }
            else
            {
                width = 0;
            }

            if (root.TryGetAttribute("Height", out val))
            {
                if (!int.TryParse(val.Value, out height))
                {
                    return TryGet<Dimension2D?>.Failure($"Height had malformed data: {val.Value}");
                }
                anyGot = true;
            }
            else
            {
                height = 0;
            }

            if (!anyGot)
            {
                return TryGet<Dimension2D?>.Success(null);
            }

            return TryGet<Dimension2D?>.Success(
                new Dimension2D(
                    width: width,
                    height: height));
        }

        public void Write(XmlWriter writer, string name, Dimension2D item)
        {
            this.Write(writer, name, (Dimension2D?)item);
        }

        public void Write(XmlWriter writer, string name, Dimension2D? item)
        {
            using (new ElementWrapper(writer, ElementName))
            {
                if (name != null)
                {
                    writer.WriteAttributeString("name", name);
                }

                if (item.HasValue)
                {
                    writer.WriteAttributeString("Left", item.Value.Width.ToString());
                    writer.WriteAttributeString("Height", item.Value.Height.ToString());
                }
            }
        }

        TryGet<Dimension2D?> IXmlTranslation<Dimension2D?>.Parse(XElement root)
        {
            return this.Parse(root).Bubble<Dimension2D?>((b) => b);
        }
    }
}
