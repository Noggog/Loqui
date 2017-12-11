using Noggog;
using Noggog.Notifying;
using Noggog.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class WildcardXmlTranslation : IXmlTranslation<object, object>
    {
        public static readonly WildcardXmlTranslation Instance = new WildcardXmlTranslation();

        public string ElementName => null;

        public IXmlTranslation<object, object> GetTranslator(Type t)
        {
            return XmlTranslator.Instance.GetTranslator(t).Item.Value;
        }

        public bool Validate(Type t)
        {
            return XmlTranslator.Instance.Validate(t);
        }

        public TryGet<Object> Parse(XElement root, bool doMasks, out object maskObj)
        {
            if (!root.TryGetAttribute(XmlConstants.TYPE_ATTRIBUTE, out var nameAttr))
            {
                var ex = new ArgumentException($"Could not get name attribute for XML Translator.");
                if (doMasks)
                {
                    maskObj = ex;
                    return TryGet<Object>.Failure;
                }
                else
                {
                    throw ex;
                }
            }

            var itemNode = root.Element("Item");
            if (itemNode == null)
            {
                var ex = new ArgumentException($"Could not get item node.");
                if (doMasks)
                {
                    maskObj = ex;
                    return TryGet<Object>.Failure;
                }
                else
                {
                    throw ex;
                }
            }

            if (!XmlTranslator.Instance.TranslateElementName(nameAttr.Value, out INotifyingItemGetter<Type> t))
            {
                var ex = new ArgumentException($"Could not match Element type {nameAttr.Value} to an XML Translator.");
                if (doMasks)
                {
                    maskObj = ex;
                    return TryGet<Object>.Failure;
                }
                else
                {
                    throw ex;
                }
            }
            var xml = GetTranslator(t.Item);
            return xml.Parse(itemNode, doMasks, out maskObj);
        }

        public void Write(XmlWriter writer, string name, object item, bool doMasks, out object maskObj)
        {
            var xml = GetTranslator(item?.GetType());
            using (new ElementWrapper(writer, name))
            {
                writer.WriteAttributeString(XmlConstants.TYPE_ATTRIBUTE, xml.ElementName);
                xml.Write(writer, "Item", item, doMasks, out maskObj);
            }
        }

        public void Write<M>(
            XmlWriter writer,
            string name,
            IHasItem<object> item,
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
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                subMask);
        }

        public void Write<M>(
            XmlWriter writer,
            string name,
            object item,
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
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                subMask);
        }

        public void Write<M>(
            XmlWriter writer,
            string name,
            IHasBeenSetItemGetter<object> item,
            int fieldIndex,
            Func<M> errorMask)
            where M : IErrorMask
        {
            if (!item.HasBeenSet) return;
            this.Write(
                writer: writer,
                name: name,
                item: item,
                fieldIndex: fieldIndex,
                errorMask: errorMask);
        }
    }
}
