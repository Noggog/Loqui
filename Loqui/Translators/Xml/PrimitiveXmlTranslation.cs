using Loqui.Internal;
using Noggog;
using Noggog.Xml;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public abstract class PrimitiveXmlTranslation<T> : IXmlTranslation<T>
        where T : struct
    {
        string IXmlTranslation<T>.ElementName => ElementName;
        public virtual string ElementName => typeof(T).GetName();

        protected virtual string GetItemStr(T item)
        {
            return item!.ToString()!;
        }

        protected abstract bool Parse(string str, out T value, ErrorMaskBuilder? errorMask);

        public T Parse(
            XElement node,
            ErrorMaskBuilder? errorMask,
            T defaultVal = default)
        {
            if (this.Parse(
                node: node,
                item: out var item,
                errorMask: errorMask))
            {
                return item;
            }
            return defaultVal;
        }

        public bool Parse(
            XElement node,
            out T item,
            ErrorMaskBuilder? errorMask,
            TranslationCrystal? translationMask)
        {
            return this.Parse(
                node: node,
                item: out item,
                errorMask: errorMask);
        }

        public bool Parse(
            XElement node, 
            int fieldIndex,
            out T item,
            ErrorMaskBuilder? errorMask)
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
            item = default(T);
            return false;
        }

        public virtual bool Parse(XElement node, out T item, ErrorMaskBuilder? errorMask)
        {
            if (!node.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out var val)
                || string.IsNullOrEmpty(val.Value))
            {
                item = default;
                return true;
            }
            if (Parse(val.Value, out var nonNullVal, errorMask))
            {
                item = nonNullVal;
                return true;
            }
            item = default;
            return false;
        }

        protected virtual void WriteValue(XElement node, T? item)
        {
            node.SetAttributeValue(
                XmlConstants.VALUE_ATTRIBUTE,
                item.HasValue ? GetItemStr(item.Value) : string.Empty);
        }

        public void Write(
            XElement node,
            string? name,
            T? item)
        {
            Write_Internal(
                node,
                name,
                item,
                nullable: true);
        }

        public void Write(
            XElement node,
            string? name,
            T? item,
            ErrorMaskBuilder? errorMask)
        {
            Write_Internal(
                node,
                name,
                item,
                nullable: true);
        }

        private void Write_Internal(
            XElement node, 
            string? name, 
            T? item,
            bool nullable)
        {
            var elem = new XElement(name ?? ElementName);
            node.Add(elem);
            WriteValue(elem, item);
        }

        public void Write(
            XElement node, 
            string? name,
            T item)
        {
            Write_Internal(node, name, (T?)item, nullable: false);
        }

        public void Write(
            XElement node,
            string name,
            T? item,
            int fieldIndex,
            ErrorMaskBuilder? errorMask)
        {
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
                    this.Write(
                        node,
                        name,
                        item);
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }
        }

        void IXmlTranslation<T>.Write(XElement node, string? name, T item, ErrorMaskBuilder? errorMask, TranslationCrystal? translationMask)
        {
            this.Write(
                node: node,
                name: name,
                item: item);
        }

        bool IXmlTranslation<T>.Parse(XElement node, [MaybeNullWhen(false)]out T item, ErrorMaskBuilder? errorMask, TranslationCrystal? translationMask)
        {
            return this.Parse(
                node: node,
                item: out item,
                errorMask: errorMask);
        }
    }
}
