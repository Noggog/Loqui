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
    public class DictXmlTranslation<K, V> : IXmlTranslation<IEnumerable<KeyValuePair<K, V>>>
    {
        public virtual string ElementName => "Dict";

        public TryGet<IEnumerable<KeyValuePair<K, V>>> Parse(XElement root, bool doMasks, out object maskObj)
        {
            var keyTransl = XmlTranslator<K>.Translator;
            if (keyTransl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(K)}. {keyTransl.Item.Reason}");
            }
            var valTransl = XmlTranslator<V>.Translator;
            if (valTransl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(V)}. {valTransl.Item.Reason}");
            }
            return Parse(keyTransl.Item.Value, valTransl.Item.Value, root, doMasks, out maskObj);
        }

        public TryGet<IEnumerable<KeyValuePair<K, V>>> Parse(
            IXmlTranslation<K> keyTranl,
            IXmlTranslation<V> valTranl,
            XElement root,
            bool doMasks,
            out object maskObj)
        {
            if (!root.Name.LocalName.Equals(ElementName))
            {
                var ex = new ArgumentException($"Skipping field that did not match proper type. Type: {root.Name.LocalName}, expected: {ElementName}.");
                if (!doMasks) throw ex;
                maskObj = ex;
                return TryGet<IEnumerable<KeyValuePair<K, V>>>.Failure;
            }
            return TryGet<IEnumerable<KeyValuePair<K, V>>>.Succeed(Parse_Internal(keyTranl, valTranl, root, doMasks, out maskObj));
        }

        private IEnumerable<KeyValuePair<K, V>> Parse_Internal(
            IXmlTranslation<K> keyTransl,
            IXmlTranslation<V> valTransl,
            XElement root,
            bool doMasks,
            out object maskObj)
        {
            List<MaskItem<Exception, object>> maskList = null;
            var ret = new List<KeyValuePair<K, V>>();
            foreach (var listElem in root.Elements())
            {
                try
                {
                    var get = ParseSingleItem(listElem, keyTransl, valTransl, doMasks, out object subMaskObj);
                    if (get.Succeeded)
                    {
                        ret.Add(get.Value);
                    }
                    if (doMasks && subMaskObj != null)
                    {
                        if (maskList == null)
                        {
                            maskList = new List<MaskItem<Exception, object>>();
                        }
                        maskList.Add(new MaskItem<Exception, object>(null, subMaskObj));
                    }
                }
                catch (Exception ex)
                {
                    if (!doMasks) throw;
                    if (maskList == null)
                    {
                        maskList = new List<MaskItem<Exception, object>>();
                    }
                    maskList.Add(new MaskItem<Exception, object>(ex, null));
                }
            }
            maskObj = maskList;
            return ret;
        }

        public virtual TryGet<KeyValuePair<K, V>> ParseSingleItem(
            XElement root,
            IXmlTranslation<K> keyTranl,
            IXmlTranslation<V> valTranl,
            bool doMasks,
            out object maskObj)
        {
            if (!root.Name.LocalName.Equals("Item"))
            {
                maskObj = null;
                if (!doMasks) throw new ArgumentException($"Unknown item type listed: {root.Name}");
                return TryGet<KeyValuePair<K, V>>.Failure;
            }

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
                return keyParse.BubbleFailure<KeyValuePair<K, V>>();
            }

            maskObj = null;
            return TryGet<KeyValuePair<K, V>>.Succeed(new KeyValuePair<K, V>(keyParse.Value, valParse.Value));
        }

        public void Write(
            XmlWriter writer,
            string name,
            IEnumerable<KeyValuePair<K, V>> item,
            bool doMasks,
            out object maskObj)
        {
            var keyTransl = XmlTranslator<K>.Translator;
            if (keyTransl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(K)}. {keyTransl.Item.Reason}");
            }
            var valTransl = XmlTranslator<V>.Translator;
            if (valTransl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(V)}. {valTransl.Item.Reason}");
            }
            this.Write<object, object>(
                writer: writer,
                name: name,
                items: item,
                doMasks: doMasks,
                maskList: out var maskList,
                keyTransl: (K item1, out object obj) => keyTransl.Item.Value.Write(writer: writer, name: null, item: item1, doMasks: doMasks, maskObj: out obj),
                valTransl: (V item1, out object obj) => valTransl.Item.Value.Write(writer: writer, name: null, item: item1, doMasks: doMasks, maskObj: out obj));
            maskObj = maskList;
        }

        public void Write<KMask, VMask>(
            XmlWriter writer,
            string name,
            IEnumerable<KeyValuePair<K, V>> items,
            bool doMasks,
            out List<KeyValuePair<MaskItem<Exception, KMask>, MaskItem<Exception, VMask>>> maskList,
            XmlSubWriteDelegate<K, KMask> keyTransl,
            XmlSubWriteDelegate<V, VMask> valTransl)
        {
            maskList = null;
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
                            maskList = new List<KeyValuePair<MaskItem<Exception, KMask>, MaskItem<Exception, VMask>>>();
                        }
                        maskList.Add(
                            new KeyValuePair<MaskItem<Exception, KMask>, MaskItem<Exception, VMask>>(
                                keyErrMask,
                                valErrMask));
                    }
                }
            }
        }

        public void WriteSingleItem<KMask, VMask>(
            XmlWriter writer,
            KeyValuePair<K, V> item,
            bool doMasks,
            out MaskItem<Exception, KMask> keymaskItem,
            out MaskItem<Exception, VMask> valmaskItem,
            XmlSubWriteDelegate<K, KMask> keyTransl,
            XmlSubWriteDelegate<V, VMask> valTransl)
        {
            using (new ElementWrapper(writer, "Item"))
            {
                try
                {
                    using (new ElementWrapper(writer, "Key"))
                    {
                        keyTransl(item.Key, out KMask keyMask);
                        if (keyMask != null)
                        {
                            keymaskItem = new MaskItem<Exception, KMask>(null, keyMask);
                        }
                        else
                        {
                            keymaskItem = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!doMasks) throw;
                    keymaskItem = new MaskItem<Exception, KMask>(ex, default(KMask));
                }
                try
                {
                    using (new ElementWrapper(writer, "Value"))
                    {
                        valTransl(item.Value, out VMask valMask);
                        if (valMask != null)
                        {
                            valmaskItem = new MaskItem<Exception, VMask>(null, valMask);
                        }
                        else
                        {
                            valmaskItem = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!doMasks) throw;
                    valmaskItem = new MaskItem<Exception, VMask>(ex, default(VMask));
                }
            }
        }
    }
}
