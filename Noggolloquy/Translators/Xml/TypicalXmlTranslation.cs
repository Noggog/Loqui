using Noggog;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy.Xml
{
    public abstract class TypicalXmlTranslation<T> : IXmlTranslation<T>, IXmlTranslation<Nullable<T>>
        where T : struct
    {
        string IXmlTranslation<T?>.ElementName => NullLessName;
        string IXmlTranslation<T>.ElementName => ElementName;
        public static readonly string ElementName = typeof(T).GetName().Replace('?', 'N');
        public static readonly string NullLessName = typeof(T).GetName().Replace("?", string.Empty);
        public static readonly bool IsNullable;

        static TypicalXmlTranslation()
        {
            IsNullable = !NullLessName.Equals(typeof(T).GetName());
        }

        protected virtual string GetItemStr(T? item)
        {
            return item.ToStringSafe();
        }

        protected abstract T ParseNonNullString(string str);

        public TryGet<T?> Parse(XElement root, bool doMasks, out object maskObj)
        {
            maskObj = null;
            if (!root.Name.LocalName.Equals(ElementName))
            {
                return TryGet<T?>.Failure($"Skipping field Version that did not match proper type. Type: {root.Name.LocalName}, expected: {ElementName}.");
            }
            if (!root.TryGetAttribute("value", out XAttribute val)
                || string.IsNullOrEmpty(val.Value))
            {
                return TryGet<T?>.Success(null);
            }
            return TryGet<T?>.Success(ParseNonNullString(val.Value));
        }

        public bool Write(XmlWriter writer, string name, T? item, bool doMasks, out object maskObj)
        {
            maskObj = null;
            using (new ElementWrapper(writer, ElementName))
            {
                if (name != null)
                {
                    writer.WriteAttributeString("name", name);
                }

                writer.WriteAttributeString("value", GetItemStr(item.Value));
            }
            return true;
        }

        public bool Write(XmlWriter writer, string name, T item, bool doMasks, out object maskObj)
        {
            maskObj = null;
            using (new ElementWrapper(writer, ElementName))
            {
                if (name != null)
                {
                    writer.WriteAttributeString("name", name);
                }

                writer.WriteAttributeString("value", GetItemStr(item));
            }
            return true;
        }

        TryGet<T> IXmlTranslation<T>.Parse(XElement root, bool doMasks, out object maskObj)
        {
            var parse = this.Parse(root, doMasks, out maskObj);
            if (parse.Failed) return parse.BubbleFailure<T>();
            if (parse.Value.HasValue) return TryGet<T>.Success(parse.Value.Value);
            throw new ArgumentException("Value was unexpectedly null.");
        }
    }
}
