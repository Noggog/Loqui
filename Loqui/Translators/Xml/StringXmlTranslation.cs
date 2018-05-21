using Loqui.Internal;
using Noggog;
using Noggog.Notifying;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class StringXmlTranslation : IXmlTranslation<string, Exception>
    {
        public string ElementName => "String";
        public readonly static StringXmlTranslation Instance = new StringXmlTranslation();

        public TryGet<string> Parse(XElement root)
        {
            if (!root.Name.LocalName.Equals(ElementName))
            {
                throw new ArgumentException($"Skipping field Version that did not match proper type. Type: {root.Name.LocalName}, expected: {ElementName}.");
            }
            if (root.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute val))
            {
                return TryGet<string>.Succeed(val.Value);
            }
            return TryGet<string>.Succeed(null);
        }

        public TryGet<string> Parse(XElement root, ErrorMaskBuilder errorMask)
        {
            var ret = this.Parse(
                root,
                out var item,
                errorMask);
            return TryGet<string>.Create(
                ret,
                item);
        }

        public bool Parse(XElement root, out string item, ErrorMaskBuilder errorMask)
        {
            if (root.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute val))
            {
                item = val.Value;
                return true;
            }
            item = null;
            return false;
        }

        public void Write(XElement node, string name, string item, bool doMasks, out Exception errorMask)
        {
            try
            {
                var elem = new XElement(name);
                node.Add(elem);
                if (item != null)
                {
                    elem.SetAttributeValue(XmlConstants.VALUE_ATTRIBUTE, item);
                }
                errorMask = null;
            }
            catch (Exception ex)
            when (doMasks)
            {
                errorMask = ex;
            }
        }

        public void Write<M>(
            XElement node,
            string name,
            string item,
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
            IHasItemGetter<string> item,
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
            IHasBeenSetItemGetter<string> item,
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
