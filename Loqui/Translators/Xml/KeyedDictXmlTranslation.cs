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

namespace Loqui.Xml
{
    public class KeyedDictXmlTranslation<K, V, Mask> : IXmlTranslation<IEnumerable<V>, MaskItem<Exception, IEnumerable<Mask>>>
    {
        public static readonly KeyedDictXmlTranslation<K, V, Mask> Instance = new KeyedDictXmlTranslation<K, V, Mask>();
        public virtual string ElementName => "Dict";

        public TryGet<IEnumerable<V>> Parse(XElement root, bool doMasks, out MaskItem<Exception, IEnumerable<Mask>> maskObj)
        {
            var valTransl = XmlTranslator<V, Mask>.Translator;
            if (valTransl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(V)}. {valTransl.Item.Reason}");
            }
            return Parse(valTransl.Item.Value, root, doMasks, out maskObj);
        }

        public TryGet<IEnumerable<V>> Parse(
            IXmlTranslation<V, Mask> valTranl,
            XElement root,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<Mask>> maskObj)
        {
            if (!root.Name.LocalName.Equals(ElementName))
            {
                var ex = new ArgumentException($"Skipping field that did not match proper type. Type: {root.Name.LocalName}, expected: {ElementName}.");
                if (!doMasks) throw ex;
                maskObj = new MaskItem<Exception, IEnumerable<Mask>>(ex, null);
                return TryGet<IEnumerable<V>>.Failure;
            }
            return TryGet<IEnumerable<V>>.Succeed(Parse_Internal(valTranl, root, doMasks, out maskObj));
        }

        private IEnumerable<V> Parse_Internal(
            IXmlTranslation<V, Mask> transl,
            XElement root,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<Mask>> maskObj)
        {
            List<Mask> maskList = null;
            var ret = new List<V>();
            foreach (var listElem in root.Elements())
            {
                var get = ParseSingleItem(listElem, transl, doMasks, out var subMaskObj);
                if (get.Succeeded)
                {
                    ret.Add(get.Value);
                }
                if (doMasks && subMaskObj != null)
                {
                    if (maskList == null)
                    {
                        maskList = new List<Mask>();
                    }
                    maskList.Add(subMaskObj);
                }
            }
            maskObj = new MaskItem<Exception, IEnumerable<Mask>>(null, maskList);
            return ret;
        }

        public virtual TryGet<V> ParseSingleItem(
            XElement root,
            IXmlTranslation<V, Mask> valTranl,
            bool doMasks,
            out Mask maskObj)
        {
            return valTranl.Parse(root, doMasks, out maskObj);
        }

        public void Write(
            XmlWriter writer,
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
                writer: writer,
                name: name,
                items: items,
                doMasks: doMasks,
                maskObj: out maskObj,
                valTransl: (V item1, bool internalDoMasks, out Mask obj) => valTransl.Item.Value.Write(writer: writer, name: null, item: item1, doMasks: internalDoMasks, maskObj: out obj));
        }

        public void Write(
            XmlWriter writer,
            string name,
            IEnumerable<V> items,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<Mask>> maskObj,
            XmlSubWriteDelegate<V, Mask> valTransl)
        {
            List<Mask> maskList = null;
            using (new ElementWrapper(writer, ElementName))
            {
                if (name != null)
                {
                    writer.WriteAttributeString(XmlConstants.NAME_ATTRIBUTE, name);
                }
                foreach (var item in items)
                {
                    WriteSingleItem(
                        writer,
                        item,
                        doMasks,
                        out var valErrMask,
                        valTransl);

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
            XmlWriter writer,
            V item,
            bool doMasks,
            out Mask valmaskItem,
            XmlSubWriteDelegate<V, Mask> valTransl)
        {
            valTransl(item, doMasks, out valmaskItem);
        }
    }
}
