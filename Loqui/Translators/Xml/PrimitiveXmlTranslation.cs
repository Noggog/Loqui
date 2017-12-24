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

        public TryGet<T?> Parse(XElement root, bool doMasks, out Exception errorMask)
        {
            return Parse(root, nullable: true, doMasks: doMasks, errorMask: out errorMask);
        }

        public TryGet<T> ParseNonNull(XElement root, bool doMasks, out Exception errorMask)
        {
            var parse = this.Parse(root, nullable: false, doMasks: doMasks, errorMask: out errorMask);
            if (parse.Failed) return parse.BubbleFailure<T>();
            return TryGet<T>.Succeed(parse.Value.Value);
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
        
        public TryGet<T?> Parse(XElement root, bool nullable, bool doMasks, out Exception errorMask)
        {
            try
            {
                var parse = ParseValue(root);
                if (!nullable && !parse.HasValue)
                {
                    throw new ArgumentException("Value was unexpectedly null.");
                }
                errorMask = null;
                return TryGet<T?>.Succeed(parse);
            }
            catch (Exception ex)
            {
                if (doMasks)
                {
                    errorMask = ex;
                    return TryGet<T?>.Failure;
                }
                throw;
            }
        }

        public TryGet<T?> Parse<M>(XElement root, int fieldIndex, Func<M> errorMask)
            where M : IErrorMask
        {
            var ret = this.Parse(
                root: root,
                doMasks: errorMask != null,
                errorMask: out Exception ex);
            ErrorMask.HandleException(
                errorMask,
                fieldIndex,
                ex);
            return ret;
        }

        public TryGet<T?> Parse<M>(XElement root, bool nullable, int fieldIndex, Func<M> errorMask)
            where M : IErrorMask
        {
            var ret = this.Parse(
                root: root,
                nullable: nullable,
                doMasks: errorMask != null,
                errorMask: out Exception ex);
            ErrorMask.HandleException(
                errorMask,
                fieldIndex,
                ex);
            return ret;
        }

        public TryGet<T> ParseNonNull<M>(XElement root, int fieldIndex, Func<M> errorMask)
            where M : IErrorMask
        {
            var ret = this.ParseNonNull(
                root: root,
                doMasks: errorMask != null,
                errorMask: out Exception ex);
            ErrorMask.HandleException(
                errorMask,
                fieldIndex,
                ex);
            return ret;
        }

        protected virtual void WriteValue(XmlWriter writer, string name, T? item)
        {
            writer.WriteAttributeString(
                XmlConstants.VALUE_ATTRIBUTE,
                item.HasValue ? GetItemStr(item.Value) : string.Empty);
        }

        TryGet<T> IXmlTranslation<T, Exception>.Parse(XElement root, bool doMasks, out Exception errorMask)
        {
            return ParseNonNull(root, doMasks, out errorMask);
        }

        public void Write(XmlWriter writer, string name, T? item, bool doMasks, out Exception errorMask)
        {
            errorMask = Write_Internal(writer, name, item, doMasks, nullable: true);
        }

        private Exception Write_Internal(XmlWriter writer, string name, T? item, bool doMasks, bool nullable)
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
            errorMask = Write_Internal(writer, name, (T?)item, doMasks, nullable: false);
        }

        public void Write<M>(
            XmlWriter writer,
            string name,
            T? item,
            int fieldIndex,
            Func<M> errorMask)
            where M : IErrorMask
        {
            this.Write(
                writer,
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
            XmlWriter writer,
            string name,
            IHasItemGetter<T> item,
            int fieldIndex,
            Func<M> errorMask)
            where M : IErrorMask
        {
            this.Write(
                writer,
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
            XmlWriter writer,
            string name,
            IHasItemGetter<T?> item,
            int fieldIndex,
            Func<M> errorMask)
            where M : IErrorMask
        {
            this.Write(
                writer,
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
            XmlWriter writer,
            string name,
            IHasBeenSetItemGetter<T?> item,
            int fieldIndex,
            Func<M> errorMask)
            where M : IErrorMask
        {
            if (!item.HasBeenSet) return;
            this.Write(
                writer,
                name,
                item.Item,
                fieldIndex,
                errorMask);
        }

        public void Write<M>(
            XmlWriter writer,
            string name,
            IHasBeenSetItemGetter<T> item,
            int fieldIndex,
            Func<M> errorMask)
            where M : IErrorMask
        {
            if (!item.HasBeenSet) return;
            this.Write(
                writer,
                name,
                item.Item,
                fieldIndex,
                errorMask);
        }
    }
}
