using Loqui.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noggog;
using System.Xml;
using System.Xml.Linq;
using Noggog.Xml;
using Loqui;
using Noggog.Notifying;
using Loqui.Internal;

namespace Loqui.Xml
{
    public class KeyedDictXmlTranslation<K, V, Mask> : IXmlTranslation<IEnumerable<V>, MaskItem<Exception, IEnumerable<Mask>>>
    {
        public static readonly KeyedDictXmlTranslation<K, V, Mask> Instance = new KeyedDictXmlTranslation<K, V, Mask>();
        public virtual string ElementName => "Dict";

        public bool Parse(XElement root, out IEnumerable<V> enumer, ErrorMaskBuilder errorMask)
        {
            var valTransl = XmlTranslator<V, Mask>.Translator;
            if (valTransl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(V)}. {valTransl.Item.Reason}");
            }
            return Parse(
                root: root,
                enumer: out enumer,
                errorMask: errorMask,
                valTransl: valTransl.Item.Value.Parse);
        }

        public bool Parse(
            XmlSubParseDelegate<V> valTransl,
            XElement root,
            out IEnumerable<V> enumer,
            ErrorMaskBuilder errorMask)
        {
            try
            {
                var ret = new List<V>();
                int i = 0;
                foreach (var listElem in root.Elements())
                {
                    using (errorMask.PushIndex(i++))
                    {
                        if (ParseSingleItem(listElem, valTransl, out var subItem, errorMask))
                        {
                            ret.Add(subItem);
                        }
                    }
                }
                enumer = ret;
                return true;
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
                enumer = null;
                return false;
            }
        }

        public TryGet<IEnumerable<V>> Parse(
            XmlSubParseDelegate<V> valTransl,
            XElement root,
            int fieldIndex,
            ErrorMaskBuilder errorMask)
        {
            using (errorMask.PushIndex(fieldIndex))
            {
                var ret = this.Parse(
                    root: root,
                    valTransl: valTransl,
                    errorMask: errorMask,
                    enumer: out var item);
                return TryGet<IEnumerable<V>>.Create(
                    ret,
                    item);
            }
        }

        public virtual bool ParseSingleItem(
            XElement root,
            XmlSubParseDelegate<V> valTransl,
            out V item,
            ErrorMaskBuilder errorMask)
        {
            return valTransl(root, out item, errorMask);
        }

        public void Write(
            XElement node,
            string name,
            IEnumerable<V> items,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<Mask>> maskObj)
        {
            var valTransl = XmlTranslator<V, Mask>.Translator;
            if (valTransl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(V)}. {valTransl.Item.Reason}");
            }
            this.Write(
                node: node,
                name: name,
                items: items,
                doMasks: doMasks,
                maskObj: out maskObj,
                valTransl: (XElement n, V item1, bool internalDoMasks, out Mask obj) => valTransl.Item.Value.Write(node: n, name: "Item", item: item1, doMasks: internalDoMasks, maskObj: out obj));
        }

        public void Write(
            XElement node,
            string name,
            IEnumerable<V> items,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<Mask>> maskObj,
            XmlSubWriteDelegate<V, Mask> valTransl)
        {
            List<Mask> maskList = null;
            var elem = new XElement(name);
            node.Add(elem);
            foreach (var item in items)
            {
                WriteSingleItem(
                    node: node,
                    item: item,
                    doMasks: doMasks,
                    valmaskItem: out var valErrMask,
                    valTransl: valTransl);

                if (!doMasks) continue;
                if (valErrMask != null)
                {
                    if (maskList == null)
                    {
                        maskList = new List<Mask>();
                    }
                    maskList.Add(valErrMask);
                }
            }
            if (maskList != null)
            {
                maskObj = new MaskItem<Exception, IEnumerable<Mask>>(null, maskList);
            }
            else
            {
                maskObj = null;
            }
        }

        public void WriteSingleItem(
            XElement node,
            V item,
            bool doMasks,
            out Mask valmaskItem,
            XmlSubWriteDelegate<V, Mask> valTransl)
        {
            valTransl(node, item, doMasks, out valmaskItem);
        }

        public void Write<M>(
            XElement node,
            string name,
            IEnumerable<V> items,
            int fieldIndex,
            Func<M> errorMask,
            XmlSubWriteDelegate<V, Mask> valTransl)
            where M : IErrorMask
        {
            this.Write(
                node: node,
                name: name,
                items: items,
                doMasks: errorMask != null,
                maskObj: out var subMask,
                valTransl: valTransl);
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                subMask);
        }

        public void Write<M>(
            XElement node,
            string name,
            IHasItem<IEnumerable<V>> item,
            int fieldIndex,
            Func<M> errorMask,
            XmlSubWriteDelegate<V, Mask> valTransl)
            where M : IErrorMask
        {
            this.Write(
                node: node,
                name: name,
                items: item.Item,
                doMasks: errorMask != null,
                maskObj: out var subMask,
                valTransl: valTransl);
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                subMask);
        }
    }
}
