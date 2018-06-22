using Loqui.Internal;
using Noggog;
using Noggog.Notifying;
using Noggog.Xml;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class WildcardXmlTranslation : IXmlTranslation<object>
    {
        public static readonly WildcardXmlTranslation Instance = new WildcardXmlTranslation();

        public string ElementName => null;

        public IXmlTranslation<object> GetTranslator(Type t)
        {
            return XmlTranslator.Instance.GetTranslator(t).Item.Value;
        }

        public bool Validate(Type t)
        {
            return XmlTranslator.Instance.Validate(t);
        }

        public bool Parse(
            XElement root,
            out object item,
            ErrorMaskBuilder errorMask)
        {
            if (!root.TryGetAttribute(XmlConstants.TYPE_ATTRIBUTE, out var nameAttr))
            {
                errorMask.ReportExceptionOrThrow(
                    new ArgumentException($"Could not get name attribute for XML Translator."));
            }

            var itemNode = root.Element("Item");
            if (itemNode == null)
            {
                errorMask.ReportExceptionOrThrow(
                    new ArgumentException($"Could not get item node."));
            }

            if (!XmlTranslator.Instance.TranslateElementName(nameAttr.Value, out INotifyingItemGetter<Type> t))
            {
                errorMask.ReportExceptionOrThrow(
                    new ArgumentException($"Could not match Element type {nameAttr.Value} to an XML Translator."));
            }
            var xml = GetTranslator(t.Item);
            return xml.Parse(itemNode, out item, errorMask);
        }

        public void Write(XElement node, string name, object item, ErrorMaskBuilder errorMask)
        {
            var xml = GetTranslator(item?.GetType());
            var elem = new XElement(name);
            elem.SetAttributeValue(XmlConstants.TYPE_ATTRIBUTE, xml.ElementName);
            node.Add(elem);
            xml.Write(elem, "Item", item, errorMask);
        }

        public void Write(
            XElement node,
            string name,
            IHasItem<object> item,
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
            object item,
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

        public void Write<M>(
            XElement node,
            string name,
            IHasBeenSetItemGetter<object> item,
            int fieldIndex,
            Func<M> errorMask)
            where M : IErrorMask
        {
            if (!item.HasBeenSet) return;
            this.Write(
                node: node,
                name: name,
                item: item,
                fieldIndex: fieldIndex,
                errorMask: errorMask);
        }
    }
}
