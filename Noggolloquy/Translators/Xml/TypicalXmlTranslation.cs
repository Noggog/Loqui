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
        string IXmlTranslation<T?>.ElementName => NullableName;
        string IXmlTranslation<T>.ElementName => ElementName;
        public static readonly string RAW_NULLABLE_NAME = typeof(T?).GetName().Replace('?', 'N');
        public static readonly string RAW_ELEMENT_NAME = typeof(T).GetName().Replace("?", string.Empty);
        public virtual string NullableName => RAW_NULLABLE_NAME;
        public virtual string ElementName => RAW_ELEMENT_NAME;

        protected virtual string GetItemStr(T item)
        {
            return item.ToStringSafe();
        }

        protected abstract T ParseNonNullString(string str);

        public TryGet<T?> Parse(XElement root, bool doMasks, out object maskObj)
        {
            return Parse_Internal(root, nullable: true, doMasks: doMasks, maskObj: out maskObj);
        }

        protected virtual TryGet<T?> ParseValue(XElement root, bool nullable, bool doMasks, out object maskObj)
        {
            maskObj = null;
            if (!root.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute val)
                || string.IsNullOrEmpty(val.Value))
            {
                return TryGet<T?>.Succeed(null);
            }
            return TryGet<T?>.Succeed(ParseNonNullString(val.Value));
        }

        private TryGet<T?> Parse_Internal(XElement root, bool nullable, bool doMasks, out object maskObj)
        {
            if (!root.Name.LocalName.Equals(nullable ? NullableName : ElementName))
            {
                var ex = new ArgumentException($"Skipping field Version that did not match proper type. Type: {root.Name.LocalName}, expected: {(nullable ? NullableName : ElementName)}.");
                if (doMasks)
                {
                    maskObj = ex;
                    return TryGet<T?>.Failure;
                }
                else
                {
                    throw ex;
                }
            }
            return ParseValue(root, nullable, doMasks, out maskObj);
        }

        protected virtual bool WriteValue(XmlWriter writer, string name, T? item, bool doMasks, out object maskObj)
        {
            maskObj = null;
            if (item.HasValue)
            {
                writer.WriteAttributeString(XmlConstants.VALUE_ATTRIBUTE, GetItemStr(item.Value));
            }
            else
            {
                writer.WriteAttributeString(XmlConstants.VALUE_ATTRIBUTE, string.Empty);
            }
            return true;
        }

        public bool Write(XmlWriter writer, string name, T? item, bool doMasks, out object maskObj)
        {
            using (new ElementWrapper(writer, NullableName))
            {
                if (name != null)
                {
                    writer.WriteAttributeString(XmlConstants.NAME_ATTRIBUTE, name);
                }

                return WriteValue(writer, name, item, doMasks, out maskObj);
            }
        }

        public bool Write(XmlWriter writer, string name, T item, bool doMasks, out object maskObj)
        {
            using (new ElementWrapper(writer, ElementName))
            {
                if (name != null)
                {
                    writer.WriteAttributeString(XmlConstants.NAME_ATTRIBUTE, name);
                }

                return WriteValue(writer, name, item, doMasks, out maskObj);
            }
        }

        TryGet<T> IXmlTranslation<T>.Parse(XElement root, bool doMasks, out object maskObj)
        {
            var parse = this.Parse_Internal(root, nullable: false, doMasks: doMasks, maskObj: out maskObj);
            if (parse.Failed) return parse.BubbleFailure<T>();
            if (parse.Value.HasValue) return TryGet<T>.Succeed(parse.Value.Value);
            var ex = new ArgumentException("Value was unexpectedly null.");
            if (doMasks)
            {
                maskObj = ex;
                return TryGet<T>.Failure;
            }
            else
            {
                throw ex;
            }
        }
    }
}
