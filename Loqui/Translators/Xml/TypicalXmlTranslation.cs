using Noggog;
using Noggog.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public abstract class TypicalXmlTranslation<T> : IXmlTranslation<T, Exception>
        where T : class
    {
        string IXmlTranslation<T, Exception>.ElementName => ElementName;
        public static readonly string RAW_ELEMENT_NAME = typeof(T).GetName().Replace("?", string.Empty);
        public virtual string ElementName => RAW_ELEMENT_NAME;

        protected virtual string GetItemStr(T item)
        {
            return item.ToStringSafe();
        }

        protected abstract T ParseNonNullString(string str);
        
        public TryGet<T> ParseNonNull(XElement root, bool doMasks, out Exception errorMask)
        {
            var parse = this.Parse(root, nullable: false, doMasks: doMasks, errorMask: out errorMask);
            if (parse.Failed) return parse.BubbleFailure<T>();
            return TryGet<T>.Succeed(parse.Value);
        }

        protected virtual T ParseValue(XElement root)
        {
            if (!root.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute val)
                || string.IsNullOrEmpty(val.Value))
            {
                return null;
            }
            return ParseNonNullString(val.Value);
        }

        public TryGet<T> Parse(XElement root, bool nullable, bool doMasks, out Exception errorMask)
        {
            try
            {
                var parse = ParseValue(root);
                if (!nullable && parse == null)
                {
                    throw new ArgumentException("Value was unexpectedly null.");
                }
                errorMask = null;
                return TryGet<T>.Succeed(parse);
            }
            catch (Exception ex)
            {
                if (doMasks)
                {
                    errorMask = ex;
                    return TryGet<T>.Failure;
                }
                throw;
            }
        }

        protected virtual void WriteValue(XmlWriter writer, string name, T item)
        {
            writer.WriteAttributeString(
                XmlConstants.VALUE_ATTRIBUTE,
                item != null ? GetItemStr(item) : string.Empty);
        }

        TryGet<T> IXmlTranslation<T, Exception>.Parse(XElement root, bool doMasks, out Exception errorMask)
        {
            return Parse(root, nullable: true, doMasks: doMasks, errorMask: out errorMask);
        }

        private Exception Write_Internal(XmlWriter writer, string name, T item, bool doMasks, bool nullable)
        {
            Exception errorMask;
            try
            {
                using (new ElementWrapper(writer, name))
                {
                    WriteValue(writer, name, item);
                }
                errorMask = null;
            }
            catch (Exception ex)
            when (doMasks)
            {
                errorMask = ex;
            }

            return errorMask;
        }

        public void Write(XmlWriter writer, string name, T item, bool doMasks, out Exception errorMask)
        {
            errorMask = Write_Internal(writer, name, item, doMasks, nullable: false);
        }
    }
}
