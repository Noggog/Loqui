using Loqui.Internal;
using Noggog;
using Noggog.Notifying;
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
    public abstract class TypicalXmlTranslation<T> : IXmlTranslation<T>
        where T : class
    {
        string IXmlTranslation<T>.ElementName => ElementName;
        public static readonly string RAW_ELEMENT_NAME = typeof(T).GetName().Replace("?", string.Empty);
        public virtual string ElementName => RAW_ELEMENT_NAME;

        protected virtual string GetItemStr(T item)
        {
            return item.ToStringSafe();
        }

        protected abstract T ParseNonNullString(string str);

        protected virtual T ParseValue(XElement node)
        {
            if (!node.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute val)
                || string.IsNullOrEmpty(val.Value))
            {
                return null;
            }
            return ParseNonNullString(val.Value);
        }

        public void ParseInto(
            XElement node,
            int fieldIndex,
            IHasItem<T> item,
            ErrorMaskBuilder errorMask)
        {
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
                    if (Parse(node, out var val, errorMask))
                    {
                        item.Item = val;
                    }
                    else
                    {
                        item.Unset();
                    }
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }
        }

        public bool Parse(
            XElement node,
            bool nullable,
            out T item,
            ErrorMaskBuilder errorMask)
        {
            item = ParseValue(node);
            if (!nullable && item == null)
            {
                errorMask.ReportExceptionOrThrow(
                    new ArgumentException("Value was unexpectedly null."));
            }
            return true;
        }

        public bool Parse(
            XElement node,
            out T item,
            ErrorMaskBuilder errorMask)
        {
            return this.Parse(
                node: node,
                item: out item,
                errorMask: errorMask,
                nullable: false);
        }

        public bool Parse(
            XElement node,
            int fieldIndex,
            out T item,
            ErrorMaskBuilder errorMask)
        {
            using (errorMask.PushIndex(fieldIndex))
            {
                try
                {
                    return Parse(node, out item, errorMask);
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }
            item = null;
            return false;
        }

        protected virtual void WriteValue(XElement node, string name, T item)
        {
            node.SetAttributeValue(
                XmlConstants.VALUE_ATTRIBUTE,
                item != null ? GetItemStr(item) : string.Empty);
        }

        private void Write(
            XElement node,
            string name,
            T item)
        {
            var elem = new XElement(name);
            WriteValue(elem, name, item);
        }

        public void Write(
            XElement node,
            string name,
            T item,
            int fieldIndex,
            ErrorMaskBuilder errorMask)
        {
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
                    this.Write(
                        node: node,
                        name: name,
                        item: item);
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }
        }

        public void Write(
            XElement node,
            string name,
            IHasItemGetter<T> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask)
        {
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
                    this.Write(
                        node: node,
                        name: name,
                        item: item.Item);
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }
        }

        public void Write(
            XElement node,
            string name,
            IHasBeenSetItemGetter<T> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask)
        {
            if (!item.HasBeenSet) return;
            this.Write(
                node: node,
                name: name,
                item: item.Item,
                fieldIndex: fieldIndex,
                errorMask: errorMask);
        }

        public void Write(XElement node, string name, T item, ErrorMaskBuilder errorMask, TranslationCrystal translationMask)
        {
            this.Write(
                node: node,
                item: item,
                name: name);
        }

        public bool Parse(XElement node, out T item, ErrorMaskBuilder errorMask, TranslationCrystal translationMask)
        {
            return this.Parse(
                node: node,
                item: out item,
                errorMask: errorMask);
        }
    }
}
