using Loqui.Internal;
using Noggog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            return item.ToString();
        }

        protected abstract T Parse(string str);

        public virtual bool Parse(XElement node, [MaybeNullWhen(false)]out T item)
        {
            if (!node.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute? attr)
                || string.IsNullOrEmpty(attr.Value))
            {
                item = default!;
                return false;
            }
            item = Parse(attr.Value);
            return true;
        }

        public T? ParseNullable(
            XElement node,
            ErrorMaskBuilder? errorMask,
            T? defaultVal = default)
        {
            if (this.Parse(
                node: node,
                item: out var item))
            {
                return item;
            }
            return defaultVal;
        }

        public bool Parse(
            XElement node,
            int fieldIndex,
            [MaybeNullWhen(false)] out T item,
            ErrorMaskBuilder? errorMask)
        {
            using (errorMask.PushIndex(fieldIndex))
            {
                try
                {
                    return Parse(node, out item!);
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }
            item = default!;
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
            node.Add(elem);
            WriteValue(elem, name, item);
        }

        public void Write(
            XElement node,
            string name,
            T item,
            int fieldIndex,
            ErrorMaskBuilder? errorMask)
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

        public void Write(XElement node, string name, T item, ErrorMaskBuilder? errorMask, TranslationCrystal? translationMask)
        {
            this.Write(
                node: node,
                item: item,
                name: name);
        }

        public bool Parse(XElement node, [MaybeNullWhen(false)] out T item, ErrorMaskBuilder? errorMask, TranslationCrystal? translationMask)
        {
            return this.Parse(
                node: node,
                item: out item!);
        }
    }
}
