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

        protected virtual T ParseValue(XElement root)
        {
            if (!root.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute val)
                || string.IsNullOrEmpty(val.Value))
            {
                return null;
            }
            return ParseNonNullString(val.Value);
        }

        public void ParseInto(
            XElement root,
            int fieldIndex,
            IHasItem<T> item,
            ErrorMaskBuilder errorMask)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                if (Parse(root, out var val, errorMask))
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

        public bool Parse(
            XElement root,
            bool nullable,
            out T item,
            ErrorMaskBuilder errorMask)
        {
            item = ParseValue(root);
            if (!nullable && item == null)
            {
                errorMask.ReportExceptionOrThrow(
                    new ArgumentException("Value was unexpectedly null."));
            }
            return true;
        }

        public bool Parse(
            XElement root,
            out T item,
            ErrorMaskBuilder errorMask)
        {
            return this.Parse(
                root: root,
                item: out item,
                errorMask: errorMask,
                nullable: false);
        }

        public bool Parse(
            XElement root,
            int fieldIndex,
            out T item,
            ErrorMaskBuilder errorMask)
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

        protected virtual void WriteValue(XElement node, string name, T item)
        {
            node.SetAttributeValue(
                XmlConstants.VALUE_ATTRIBUTE,
                item != null ? GetItemStr(item) : string.Empty);
        }

        public void Write(XElement node, string name, T item, ErrorMaskBuilder errorMask)
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
            try
            {
                errorMask?.PushIndex(fieldIndex);
                this.Write(
                    node: node,
                    name: name,
                    item: item,
                    errorMask: errorMask);
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
                    node: node,
                    name: name,
                    item: item.Item,
                    errorMask: errorMask);
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
    }
}
