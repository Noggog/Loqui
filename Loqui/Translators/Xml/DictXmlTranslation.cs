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
    public class DictXmlTranslation<K, V, KMask, VMask> : IXmlTranslation<IEnumerable<KeyValuePair<K, V>>, MaskItem<Exception, IEnumerable<KeyValuePair<KMask, VMask>>>>
    {
        public static readonly DictXmlTranslation<K, V, KMask, VMask> Instance = new DictXmlTranslation<K, V, KMask, VMask>();
        public virtual string ElementName => "Dict";

        public TryGet<IEnumerable<KeyValuePair<K, V>>> Parse(XElement root, bool doMasks, out MaskItem<Exception, IEnumerable<KeyValuePair<KMask, VMask>>> maskObj)
        {
            var keyTransl = XmlTranslator<K, KMask>.Translator;
            if (keyTransl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(K)}. {keyTransl.Item.Reason}");
            }
            var valTransl = XmlTranslator<V, VMask>.Translator;
            if (valTransl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(V)}. {valTransl.Item.Reason}");
            }
            return Parse(keyTransl.Item.Value, valTransl.Item.Value, root, doMasks, out maskObj);
        }

        public TryGet<IEnumerable<KeyValuePair<K, V>>> Parse(
            IXmlTranslation<K, KMask> keyTranl,
            IXmlTranslation<V, VMask> valTranl,
            XElement root,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<KeyValuePair<KMask, VMask>>> maskObj)
        {
            if (!root.Name.LocalName.Equals(ElementName))
            {
                var ex = new ArgumentException($"Skipping field that did not match proper type. Type: {root.Name.LocalName}, expected: {ElementName}.");
                if (!doMasks) throw ex;
                maskObj = new MaskItem<Exception, IEnumerable<KeyValuePair<KMask, VMask>>>(ex, null);
                return TryGet<IEnumerable<KeyValuePair<K, V>>>.Failure;
            }
            return TryGet<IEnumerable<KeyValuePair<K, V>>>.Succeed(Parse_Internal(keyTranl, valTranl, root, doMasks, out maskObj));
        }

        private IEnumerable<KeyValuePair<K, V>> Parse_Internal(
            IXmlTranslation<K, KMask> keyTransl,
            IXmlTranslation<V, VMask> valTransl,
            XElement root,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<KeyValuePair<KMask, VMask>>> maskObj)
        {
            List<KeyValuePair<KMask, VMask>> maskList = null;
            var ret = new List<KeyValuePair<K, V>>();
            foreach (var listElem in root.Elements())
            {
                var get = ParseSingleItem(listElem, keyTransl, valTransl, doMasks, out var subMaskObj);
                if (get.Succeeded)
                {
                    ret.Add(get.Value);
                }
                if (doMasks && subMaskObj != null)
                {
                    if (maskList == null)
                    {
                        maskList = new List<KeyValuePair<KMask, VMask>>();
                    }
                    maskList.Add(subMaskObj.Value);
                }
            }
            maskObj = new MaskItem<Exception, IEnumerable<KeyValuePair<KMask, VMask>>>(null, maskList);
            return ret;
        }

        public virtual TryGet<KeyValuePair<K, V>> ParseSingleItem(
            XElement root,
            IXmlTranslation<K, KMask> keyTranl,
            IXmlTranslation<V, VMask> valTranl,
            bool doMasks,
            out KeyValuePair<KMask, VMask>? maskObj)
        {
            var keyElem = root.Element(XName.Get("Key"));
            if (keyElem == null)
            {
                maskObj = null;
                return TryGet<KeyValuePair<K, V>>.Failure;
            }
            
            if (keyElem.Elements().Count() != 1)
            {
                maskObj = null;
                return TryGet<KeyValuePair<K, V>>.Failure;
            }

            var keyParse = keyTranl.Parse(keyElem.Elements().First(), doMasks, out var keyMaskObj);
            if (!keyParse.Succeeded)
            {
                maskObj = null;
                return keyParse.BubbleFailure<KeyValuePair<K, V>>();
            }

            var valElem = root.Element(XName.Get("Value"));
            if (valElem == null)
            {
                maskObj = null;
                return TryGet<KeyValuePair<K, V>>.Failure;
            }

            if (valElem.Elements().Count() != 1)
            {
                maskObj = null;
                return TryGet<KeyValuePair<K, V>>.Failure;
            }

            var valParse = valTranl.Parse(valElem.Elements().First(), doMasks, out var valMaskObj);
            if (!valParse.Succeeded)
            {
                maskObj = null;
                return valParse.BubbleFailure<KeyValuePair<K, V>>();
            }

            maskObj = null;
            return TryGet<KeyValuePair<K, V>>.Succeed(new KeyValuePair<K, V>(keyParse.Value, valParse.Value));
        }

        public void Write(
            XmlWriter writer,
            string name,
            IEnumerable<KeyValuePair<K, V>> items,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<KeyValuePair<KMask, VMask>>> maskObj)
        {
            var keyTransl = XmlTranslator<K, KMask>.Translator;
            if (keyTransl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(K)}. {keyTransl.Item.Reason}");
            }
            var valTransl = XmlTranslator<V, VMask>.Translator;
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
                keyTransl: (K item1, out KMask obj) => keyTransl.Item.Value.Write(writer: writer, name: null, item: item1, doMasks: doMasks, maskObj: out obj),
                valTransl: (V item1, out VMask obj) => valTransl.Item.Value.Write(writer: writer, name: null, item: item1, doMasks: doMasks, maskObj: out obj));
        }

        public void Write(
            XmlWriter writer,
            string name,
            IEnumerable<KeyValuePair<K, V>> items,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<KeyValuePair<KMask, VMask>>> maskObj,
            XmlSubWriteDelegate<K, KMask> keyTransl,
            XmlSubWriteDelegate<V, VMask> valTransl)
        {
            List<KeyValuePair<KMask, VMask>> maskList = null;
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
                        out var keyErrMask,
                        out var valErrMask,
                        keyTransl,
                        valTransl);

                    if (!doMasks) continue;
                    if (keyErrMask != null
                        || valErrMask != null)
                    {
                        if (maskList == null)
                        {
                            maskList = new List<KeyValuePair<KMask, VMask>>();
                        }
                        maskList.Add(
                            new KeyValuePair<KMask, VMask>(
                                keyErrMask,
                                valErrMask));
                    }
                }
            }
            if (maskList != null)
            {
                maskObj = new MaskItem<Exception, IEnumerable<KeyValuePair<KMask, VMask>>>(null, maskList);
            }
            else
            {
                maskObj = null;
            }
        }

        public void WriteSingleItem(
            XmlWriter writer,
            KeyValuePair<K, V> item,
            bool doMasks,
            out KMask keymaskItem,
            out VMask valmaskItem,
            XmlSubWriteDelegate<K, KMask> keyTransl,
            XmlSubWriteDelegate<V, VMask> valTransl)
        {
            using (new ElementWrapper(writer, "Item"))
            {
                using (new ElementWrapper(writer, "Key"))
                {
                    keyTransl(item.Key, out keymaskItem);
                }
                using (new ElementWrapper(writer, "Value"))
                {
                    valTransl(item.Value, out valmaskItem);
                }
            }
        }
    }
}
