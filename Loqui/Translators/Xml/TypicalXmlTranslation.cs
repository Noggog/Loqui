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

        public TryGet<T> Parse(XElement root, bool doMasks, out Exception errorMask)
        {
            return Parse(root, nullable: true, doMasks: doMasks, errorMask: out errorMask);
        }

        public TryGet<T> Parse<Mask>(
            XElement root,
            int fieldIndex,
            Func<Mask> errorMask)
            where Mask : IErrorMask
        {
            var ret = this.Parse(
                root: root,
                doMasks: errorMask != null,
                errorMask: out var ex);
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                ex);
            return ret;
        }

        protected virtual void WriteValue(XElement node, string name, T item)
        {
            node.SetAttributeValue(
                XmlConstants.VALUE_ATTRIBUTE,
                item != null ? GetItemStr(item) : string.Empty);
        }

        TryGet<T> IXmlTranslation<T, Exception>.Parse(XElement root, bool doMasks, out Exception errorMask)
        {
            return Parse(root, nullable: true, doMasks: doMasks, errorMask: out errorMask);
        }

        private Exception Write_Internal(XElement node, string name, T item, bool doMasks, bool nullable)
        {
            Exception errorMask;
            try
            {
                var elem = new XElement(name);
                WriteValue(elem, name, item);
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
            errorMask = Write_Internal(node, name, item, doMasks, nullable: false);
        }

        public void Write<M>(
            XElement node,
            string name,
            T item,
            int fieldIndex,
            Func<M> errorMask)
            where M : IErrorMask
        {
            this.Write(
                node: node,
                name: name,
                item: item,
                doMasks: errorMask != null,
                errorMask: out var subMask);
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
                node: node,
                name: name,
                item: item.Item,
                doMasks: errorMask != null,
                errorMask: out var subMask);
            ErrorMask.HandleException(
                errorMask,
                fieldIndex,
                subMask);
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
                node: node,
                name: name,
                item: item.Item,
                fieldIndex: fieldIndex,
                errorMask: errorMask);
        }
    }
}
