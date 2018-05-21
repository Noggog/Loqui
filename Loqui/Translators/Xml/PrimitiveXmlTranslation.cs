using Loqui.Internal;
using Noggog;
using Noggog.Notifying;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public abstract class PrimitiveXmlTranslation<T> : IXmlTranslation<T, Exception>, IXmlTranslation<T?, Exception>
        where T : struct
    {
        string IXmlTranslation<T?, Exception>.ElementName => NullableName;
        string IXmlTranslation<T, Exception>.ElementName => ElementName;
        public static readonly string RAW_NULLABLE_NAME = typeof(T?).GetName().Replace('?', 'N');
        public static readonly string RAW_ELEMENT_NAME = typeof(T).GetName().Replace("?", string.Empty);
        public virtual string NullableName => RAW_NULLABLE_NAME;
        public virtual string ElementName => RAW_ELEMENT_NAME;

        protected virtual string GetItemStr(T item)
        {
            return item.ToStringSafe();
        }

        protected abstract T ParseNonNullString(string str);

        public bool Parse(XElement root, out T item, ErrorMaskBuilder errorMask)
        {
            if (this.Parse(root, nullable: false, item: out var nullItem, errorMask: errorMask))
            {
                item = nullItem.Value;
                return true;
            }
            item = default(T);
            return false;
        }

        public bool Parse(XElement root, out T? item, ErrorMaskBuilder errorMask)
        {
            return this.Parse(root, nullable: true, item: out item, errorMask: errorMask);
        }

        protected virtual T? ParseValue(XElement root)
        {
            if (!root.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute val)
                || string.IsNullOrEmpty(val.Value))
            {
                return null;
            }
            return ParseNonNullString(val.Value);
        }
        
        public bool Parse(XElement root, bool nullable, out T? item, ErrorMaskBuilder errorMask)
        {
            try
            {
                item = ParseValue(root);
                if (!nullable && !item.HasValue)
                {
                    throw new ArgumentException("Value was unexpectedly null.");
                }
                return true;
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
                item = null;
                return false;
            }
        }

        protected virtual void WriteValue(XElement node, T? item)
        {
            node.SetAttributeValue(
                XmlConstants.VALUE_ATTRIBUTE,
                item.HasValue ? GetItemStr(item.Value) : string.Empty);
        }

        public void Write(XElement node, string name, T? item, bool doMasks, out Exception errorMask)
        {
            errorMask = Write_Internal(node, name, item, doMasks, nullable: true);
        }

        private Exception Write_Internal(XElement node, string name, T? item, bool doMasks, bool nullable)
        {
            Exception errorMask;
            try
            {
                var elem = new XElement(name);
                node.Add(elem);
                WriteValue(elem, item);
                errorMask = null;
            }
            catch (Exception ex)
            when (doMasks)
            {
                errorMask = ex;
            }

            return errorMask;
        }

        public void Write(XElement node, string name, T item, bool doMasks, out Exception errorMask)
        {
            errorMask = Write_Internal(node, name, (T?)item, doMasks, nullable: false);
        }

        public void Write<M>(
            XElement node,
            string name,
            T? item,
            int fieldIndex,
            Func<M> errorMask)
            where M : IErrorMask
        {
            this.Write(
                node,
                name,
                item,
                errorMask != null,
                out var subMask);
            ErrorMask.HandleException(
                errorMask,
                fieldIndex,
                subMask);
        }

        public void Write<M>(
            XElement node,
            string name,
            IHasItemGetter<T> item,
            int fieldIndex,
            Func<M> errorMask)
            where M : IErrorMask
        {
            this.Write(
                node,
                name,
                item.Item,
                errorMask != null,
                out var subMask);
            ErrorMask.HandleException(
                errorMask,
                fieldIndex,
                subMask);
        }

        public void Write<M>(
            XElement node,
            string name,
            IHasItemGetter<T?> item,
            int fieldIndex,
            Func<M> errorMask)
            where M : IErrorMask
        {
            this.Write(
                node,
                name,
                item.Item,
                errorMask != null,
                out var subMask);
            ErrorMask.HandleException(
                errorMask,
                fieldIndex,
                subMask);
        }

        public void Write<M>(
            XElement node,
            string name,
            IHasBeenSetItemGetter<T?> item,
            int fieldIndex,
            Func<M> errorMask)
            where M : IErrorMask
        {
            if (!item.HasBeenSet) return;
            this.Write(
                node,
                name,
                item.Item,
                fieldIndex,
                errorMask);
        }

        public void Write<M>(
            XElement node,
            string name,
            IHasBeenSetItemGetter<T> item,
            int fieldIndex,
            Func<M> errorMask)
            where M : IErrorMask
        {
            if (!item.HasBeenSet) return;
            this.Write(
                node,
                name,
                item.Item,
                fieldIndex,
                errorMask);
        }
    }
}
