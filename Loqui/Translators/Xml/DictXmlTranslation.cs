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
    public class DictXmlTranslation<K, V, KMask, VMask> : IXmlTranslation<IEnumerable<KeyValuePair<K, V>>, MaskItem<Exception, IEnumerable<KeyValuePair<KMask, VMask>>>>
    {
        public static readonly DictXmlTranslation<K, V, KMask, VMask> Instance = new DictXmlTranslation<K, V, KMask, VMask>();
        public virtual string ElementName => "Dict";

        public bool Parse(
            XElement root,
            out IEnumerable<KeyValuePair<K, V>> item,
            ErrorMaskBuilder errorMask)
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
            return Parse(
                keyTransl: keyTransl.Item.Value.Parse,
                valTransl: valTransl.Item.Value.Parse,
                root: root,
                item: out item,
                errorMask: errorMask);
        }

        public bool Parse(
            XmlSubParseDelegate<K> keyTransl,
            XmlSubParseDelegate<V> valTransl,
            XElement root,
            int fieldIndex,
            out IEnumerable<KeyValuePair<K, V>> item,
            ErrorMaskBuilder errorMask)
        {
            using (errorMask.PushIndex(fieldIndex))
            {
                return this.Parse(
                    root: root,
                    keyTransl: keyTransl,
                    valTransl: valTransl,
                    item: out item,
                    errorMask: errorMask);
            }
        }

        private bool Parse(
            XmlSubParseDelegate<K> keyTransl,
            XmlSubParseDelegate<V> valTransl,
            XElement root,
            out IEnumerable<KeyValuePair<K, V>> item,
            ErrorMaskBuilder errorMask)
        {
            try
            {
                var ret = new List<KeyValuePair<K, V>>();
                int i = 0;
                foreach (var listElem in root.Elements())
                {
                    using (errorMask.PushIndex(i++))
                    {
                        if (ParseSingleItem(
                            listElem,
                            keyTransl,
                            valTransl,
                            out var subItem,
                            errorMask))
                        {
                            ret.Add(subItem);
                        }
                    }
                }
                item = ret;
                return true;
            }
            catch (Exception ex)
            when (errorMask != null)
            {
                errorMask.ReportException(ex);
            }
            item = null;
            return false;
        }

        private bool ParseKey(
            XElement root,
            XmlSubParseDelegate<K> keyTransl,
            out K item,
            ErrorMaskBuilder errorMask)
        {
            var keyElem = root.Element(XName.Get("Key"));
            if (keyElem == null)
            {
                errorMask.ReportExceptionOrThrow(
                    new ArgumentException("Key field did not exist"));
                item = default(K);
                return false;
            }

            var keyCount = keyElem.Elements().Count();
            if (keyCount != 1)
            {
                errorMask.ReportExceptionOrThrow(
                    new ArgumentException($"Key field has unexpected count: {keyCount}"));
                item = default(K);
                return false;
            }

            return keyTransl(
                keyElem.Elements().First(),
                out item, 
                errorMask);
        }

        private bool ParseValue(
            XElement root,
            XmlSubParseDelegate<V> valTransl,
            out V item,
            ErrorMaskBuilder errorMask)
        {
            var valElem = root.Element(XName.Get("Value"));
            if (valElem == null)
            {
                errorMask.ReportExceptionOrThrow(
                    new ArgumentException("Value field did not exist"));
                item = default(V);
                return false;
            }

            var keyCount = valElem.Elements().Count();
            if (keyCount != 1)
            {
                errorMask.ReportExceptionOrThrow(
                    new ArgumentException($"Value field has unexpected count: {keyCount}"));
                item = default(V);
                return false;
            }

            return valTransl(
                valElem.Elements().First(),
                out item,
                errorMask);
        }

        public virtual bool ParseSingleItem(
            XElement root,
            XmlSubParseDelegate<K> keyTransl,
            XmlSubParseDelegate<V> valTransl,
            out KeyValuePair<K, V> item,
            ErrorMaskBuilder errorMask)
        {
            bool gotKey = false;
            K key = default(K);
            using (errorMask.PushIndex(0))
            {
                try
                {
                    gotKey = ParseKey(
                        root: root,
                        keyTransl: keyTransl,
                        item: out key,
                        errorMask: errorMask);
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }

            using (errorMask.PushIndex(1))
            {
                try
                {
                    if (ParseValue(
                        root: root,
                        valTransl: valTransl,
                        item: out var val,
                        errorMask: errorMask)
                        && gotKey)
                    {
                        item = new KeyValuePair<K, V>(
                            key,
                            val);
                        return true;
                    }
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }

            item = default(KeyValuePair<K, V>);
            return false;
        }

        public void Write(
            XElement node,
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
                node: node,
                name: name,
                items: items,
                doMasks: doMasks,
                maskObj: out maskObj,
                keyTransl: (XElement n, K item1, bool internalDoMasks, out KMask obj) => keyTransl.Item.Value.Write(node: n, name: "Item", item: item1, doMasks: internalDoMasks, maskObj: out obj),
                valTransl: (XElement n, V item1, bool internalDoMasks, out VMask obj) => valTransl.Item.Value.Write(node: n, name: "Item", item: item1, doMasks: internalDoMasks, maskObj: out obj));
        }

        public void Write(
            XElement node,
            string name,
            IEnumerable<KeyValuePair<K, V>> items,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<KeyValuePair<KMask, VMask>>> maskObj,
            XmlSubWriteDelegate<K, KMask> keyTransl,
            XmlSubWriteDelegate<V, VMask> valTransl)
        {
            List<KeyValuePair<KMask, VMask>> maskList = null;
            var elem = new XElement(name);
            node.Add(elem);
            foreach (var item in items)
            {
                WriteSingleItem(
                    node: elem,
                    item: item,
                    doMasks: doMasks,
                    keymaskItem: out var keyErrMask,
                    valmaskItem: out var valErrMask,
                    keyTransl: keyTransl,
                    valTransl: valTransl);

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
            XElement node,
            KeyValuePair<K, V> item,
            bool doMasks,
            out KMask keymaskItem,
            out VMask valmaskItem,
            XmlSubWriteDelegate<K, KMask> keyTransl,
            XmlSubWriteDelegate<V, VMask> valTransl)
        {
            var itemElem = new XElement("Item");
            node.Add(itemElem);
            var keyElem = new XElement("Key");
            itemElem.Add(keyElem);
            keyTransl(keyElem, item.Key, doMasks: false, maskObj: out keymaskItem);
            var valElem = new XElement("Value");
            itemElem.Add(valElem);
            valTransl(valElem, item.Value, doMasks: false, maskObj: out valmaskItem);
        }

        public void Write<Mask>(
            XElement node,
            string name,
            IEnumerable<KeyValuePair<K, V>> items,
            int fieldIndex,
            Func<Mask> errorMask,
            XmlSubWriteDelegate<K, KMask> keyTransl,
            XmlSubWriteDelegate<V, VMask> valTransl)
            where Mask : IErrorMask
        {
            this.Write(
                node: node,
                name: name,
                items: items,
                doMasks: errorMask != null,
                maskObj: out var subMask,
                keyTransl: keyTransl,
                valTransl: valTransl);
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                subMask);
        }

        public void Write<Mask>(
            XElement node,
            string name,
            IHasItem<IEnumerable<KeyValuePair<K, V>>> item,
            int fieldIndex,
            Func<Mask> errorMask,
            XmlSubWriteDelegate<K, KMask> keyTransl,
            XmlSubWriteDelegate<V, VMask> valTransl)
            where Mask : IErrorMask
        {
            this.Write(
                node: node,
                name: name,
                items: item.Item,
                doMasks: errorMask != null,
                maskObj: out var subMask,
                keyTransl: keyTransl,
                valTransl: valTransl);
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                subMask);
        }
    }
}
