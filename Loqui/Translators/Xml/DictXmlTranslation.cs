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
            return Parse(
                keyTransl: (XElement r, bool internalDoMasks, out KMask obj) => keyTransl.Item.Value.Parse(root: r, doMasks: internalDoMasks, maskObj: out obj),
                valTransl: (XElement r, bool internalDoMasks, out VMask obj) => valTransl.Item.Value.Parse(root: r, doMasks: internalDoMasks, maskObj: out obj),
                root: root,
                doMasks: doMasks,
                maskObj: out maskObj);
        }

        public TryGet<IEnumerable<KeyValuePair<K, V>>> Parse(
            XmlSubParseDelegate<K, KMask> keyTransl,
            XmlSubParseDelegate<V, VMask> valTransl,
            XElement root,
            bool doMasks,
            out MaskItem<Exception, IEnumerable<KeyValuePair<KMask, VMask>>> maskObj)
        {
            return TryGet<IEnumerable<KeyValuePair<K, V>>>.Succeed(Parse_Internal(keyTransl, valTransl, root, doMasks, out maskObj));
        }

        public TryGet<IEnumerable<KeyValuePair<K, V>>> Parse<Mask>(
            XmlSubParseDelegate<K, KMask> keyTransl,
            XmlSubParseDelegate<V, VMask> valTransl,
            XElement root,
            int fieldIndex,
            Func<Mask> errorMask)
            where Mask : IErrorMask
        {
            var ret = this.Parse(
                root: root,
                doMasks: errorMask != null,
                keyTransl: keyTransl,
                valTransl: valTransl,
                maskObj: out var ex);
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                ex);
            return ret;
        }

        private IEnumerable<KeyValuePair<K, V>> Parse_Internal(
            XmlSubParseDelegate<K, KMask> keyTransl,
            XmlSubParseDelegate<V, VMask> valTransl,
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
            maskObj = maskList == null ? null : new MaskItem<Exception, IEnumerable<KeyValuePair<KMask, VMask>>>(null, maskList);
            return ret;
        }

        public virtual TryGet<KeyValuePair<K, V>> ParseSingleItem(
            XElement root,
            XmlSubParseDelegate<K, KMask> keyTransl,
            XmlSubParseDelegate<V, VMask> valTransl,
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

            var keyParse = keyTransl(keyElem.Elements().First(), doMasks, out var keyMaskObj);
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

            var valParse = valTransl(valElem.Elements().First(), doMasks, out var valMaskObj);
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
                keyTransl: (K item1, bool internalDoMasks, out KMask obj) => keyTransl.Item.Value.Write(writer: writer, name: "Item", item: item1, doMasks: internalDoMasks, maskObj: out obj),
                valTransl: (V item1, bool internalDoMasks, out VMask obj) => valTransl.Item.Value.Write(writer: writer, name: "Item", item: item1, doMasks: internalDoMasks, maskObj: out obj));
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
            using (new ElementWrapper(writer, name))
            {
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
                    keyTransl(item.Key, doMasks, out keymaskItem);
                }
                using (new ElementWrapper(writer, "Value"))
                {
                    valTransl(item.Value, doMasks, out valmaskItem);
                }
            }
        }

        public void Write<Mask>(
            XmlWriter writer,
            string name,
            IEnumerable<KeyValuePair<K, V>> items,
            int fieldIndex,
            Func<Mask> errorMask,
            XmlSubWriteDelegate<K, KMask> keyTransl,
            XmlSubWriteDelegate<V, VMask> valTransl)
            where Mask : IErrorMask
        {
            this.Write(
                writer,
                name,
                items,
                errorMask != null,
                out var subMask,
                keyTransl,
                valTransl);
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                subMask);
        }

        public void Write<Mask>(
            XmlWriter writer,
            string name,
            IHasItem<IEnumerable<KeyValuePair<K, V>>> item,
            int fieldIndex,
            Func<Mask> errorMask,
            XmlSubWriteDelegate<K, KMask> keyTransl,
            XmlSubWriteDelegate<V, VMask> valTransl)
            where Mask : IErrorMask
        {
            this.Write(
                writer,
                name,
                item.Item,
                errorMask != null,
                out var subMask,
                keyTransl,
                valTransl);
            ErrorMask.HandleErrorMask(
                errorMask,
                fieldIndex,
                subMask);
        }
    }
}
