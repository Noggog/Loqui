using Noggog;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy
{
    public abstract class TypicalXmlTranslation<T> : IXmlTranslation<Nullable<T>>
        where T : struct
    {
        string IXmlTranslation<T?>.ElementName { get { return ElementName; } }
        public static readonly string ElementName = typeof(T).GetName().Replace('?', 'N');
        public static readonly string NullLessName = typeof(T).GetName().Replace("?", string.Empty);
        public static readonly bool IsNullable;
        
        static TypicalXmlTranslation()
        {
            IsNullable = !NullLessName.Equals(typeof(T).GetName());
        }

        public virtual string GetItemStr(T? item)
        {
            return item.ToStringSafe();
        }

        public abstract TryGet<T?> ParseNonNullString(string str);

        public TryGet<T> ParseNoNull(XElement root)
        {
            XAttribute val;
            if (!root.TryGetAttribute("value", out val)
                || string.IsNullOrEmpty(val.Value))
            {
                return TryGet<T>.Failure(reason: "No value set.");
            }

            return ParseNonNullString(val.Value).Bubble<T>((i) => i ?? default(T));
        }

        public TryGet<T?> Parse(XElement root)
        {
            XAttribute val;
            if (!root.TryGetAttribute("value", out val)
                || string.IsNullOrEmpty(val.Value))
            {
                return TryGet<T?>.Success(null);
            }
            return ParseNonNullString(val.Value);
        }

        public virtual void Write(XmlWriter writer, string name, T? item)
        {
            using (new ElementWrapper(writer, ElementName))
            {
                if (name != null)
                {
                    writer.WriteAttributeString("name", name);
                }

                writer.WriteAttributeString("value", GetItemStr(item.Value));
            }
        }
    }
}
