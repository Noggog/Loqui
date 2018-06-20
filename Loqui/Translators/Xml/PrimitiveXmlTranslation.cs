using Loqui.Internal;
using Noggog;
using Noggog.Notifying;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public abstract class PrimitiveXmlTranslation<T> : IXmlTranslation<T>, IXmlTranslation<T?>
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

        protected abstract bool ParseNonNullString(string str, out T value, ErrorMaskBuilder errorMask);

        public void ParseInto(XElement root, int fieldIndex, IHasItem<T> item, ErrorMaskBuilder errorMask)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                if (Parse(root, out T val, errorMask))
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
            finally
            {
                errorMask?.PopIndex();
            }
        }

        public void ParseInto(XElement root, int fieldIndex, IHasItem<T?> item, ErrorMaskBuilder errorMask)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                if (Parse(root, out T? val, errorMask))
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
            finally
            {
                errorMask?.PopIndex();
            }
        }

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

        public bool Parse(XElement root, int fieldIndex, out T item, ErrorMaskBuilder errorMask)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                return Parse(root, out item, errorMask);
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
            }
            finally
            {
                errorMask?.PopIndex();
            }
            item = default(T);
            return false;
        }

        public bool Parse(XElement root, int fieldIndex, out T? item, ErrorMaskBuilder errorMask)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                return Parse(root, out item, errorMask);
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
            }
            finally
            {
                errorMask?.PopIndex();
            }
            item = null;
            return false;
        }

        protected virtual bool ParseValue(XElement root, out T? value, ErrorMaskBuilder errorMask)
        {
            if (!root.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute val)
                || string.IsNullOrEmpty(val.Value))
            {
                value = null;
                return true;
            }
            if (ParseNonNullString(val.Value, out var nonNullVal, errorMask))
            {
                value = nonNullVal;
                return true;
            }
            value = null;
            return false;
        }

        public bool Parse(XElement root, bool nullable, out T? item, ErrorMaskBuilder errorMask)
        {
            if (!ParseValue(root, out item, errorMask))
            {
                return false;
            }
            if (!nullable && !item.HasValue)
            {
                errorMask.ReportExceptionOrThrow(new ArgumentException("Value was unexpectedly null."));
            }
            return true;
        }

        protected virtual void WriteValue(XElement node, T? item)
        {
            node.SetAttributeValue(
                XmlConstants.VALUE_ATTRIBUTE,
                item.HasValue ? GetItemStr(item.Value) : string.Empty);
        }

        public void Write(XElement node, string name, T? item, ErrorMaskBuilder errorMask)
        {
            Write_Internal(node, name, item, errorMask, nullable: true);
        }

        private void Write_Internal(XElement node, string name, T? item, ErrorMaskBuilder errorMask, bool nullable)
        {
            var elem = new XElement(name);
            node.Add(elem);
            WriteValue(elem, item);
            errorMask = null;
        }

        public void Write(XElement node, string name, T item, ErrorMaskBuilder errorMask)
        {
            Write_Internal(node, name, (T?)item, errorMask, nullable: false);
        }

        public void Write(
            XElement node,
            string name,
            T? item,
            int fieldIndex,
            ErrorMaskBuilder errorMask)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                this.Write(
                    node,
                    name,
                    item,
                    errorMask);
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
            }
            finally
            {
                errorMask?.PopIndex();
            }
        }

        public void Write(
            XElement node,
            string name,
            IHasItemGetter<T> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                this.Write(
                    node,
                    name,
                    item.Item,
                    errorMask);
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
            }
            finally
            {
                errorMask?.PopIndex();
            }
        }

        public void Write(
            XElement node,
            string name,
            IHasItemGetter<T?> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                this.Write(
                    node,
                    name,
                    item.Item,
                    errorMask);
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
            }
            finally
            {
                errorMask?.PopIndex();
            }
        }

        public void Write(
            XElement node,
            string name,
            IHasBeenSetItemGetter<T?> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask)
        {
            if (!item.HasBeenSet) return;
            this.Write(
                node,
                name,
                item.Item,
                fieldIndex,
                errorMask);
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
                node,
                name,
                item.Item,
                fieldIndex,
                errorMask);
        }
    }
}
