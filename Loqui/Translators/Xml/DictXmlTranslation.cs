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
    public class DictXmlTranslation<K, V> : IXmlTranslation<IEnumerable<KeyValuePair<K, V>>>
    {
        public const int KEY_ERR_INDEX = 0;
        public const int VAL_ERR_INDEX = 1;
        public static readonly DictXmlTranslation<K, V> Instance = new DictXmlTranslation<K, V>();
        public virtual string ElementName => "Dict";

        public bool Parse(
            XElement root,
            out IEnumerable<KeyValuePair<K, V>> item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
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
            return Parse(
                keyTransl: keyTransl.Item.Value.Parse,
                valTransl: valTransl.Item.Value.Parse,
                root: root,
                item: out item,
                errorMask: errorMask,
                translationMask: translationMask);
        }

        public bool Parse(
            XmlSubParseDelegate<K> keyTransl,
            XmlSubParseDelegate<V> valTransl,
            XElement root,
            out IEnumerable<KeyValuePair<K, V>> item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            var ret = new List<KeyValuePair<K, V>>();
            int i = 0;
            foreach (var listElem in root.Elements())
            {
                errorMask?.PushIndex(i++);
                if (ParseSingleItem(
                    listElem,
                    keyTransl,
                    valTransl,
                    out var subItem,
                    errorMask: errorMask,
                    translationMask: translationMask))
                {
                    ret.Add(subItem);
                }
                errorMask?.PopIndex();
            }
            item = ret;
            return true;
        }

        private bool ParseKey(
            XElement root,
            XmlSubParseDelegate<K> keyTransl,
            out K item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
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
                errorMask: errorMask, 
                translationMask: translationMask);
        }

        private bool ParseValue(
            XElement root,
            XmlSubParseDelegate<V> valTransl,
            out V item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
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
                errorMask: errorMask,
                translationMask: translationMask);
        }

        public virtual bool ParseSingleItem(
            XElement root,
            XmlSubParseDelegate<K> keyTransl,
            XmlSubParseDelegate<V> valTransl,
            out KeyValuePair<K, V> item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            bool gotKey = false;
            K key = default(K);

            try
            {
                errorMask?.PushIndex(KEY_ERR_INDEX);
                gotKey = ParseKey(
                    root: root,
                    keyTransl: keyTransl,
                    item: out key,
                    errorMask: errorMask,
                    translationMask: translationMask);
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

            try
            {
                errorMask?.PushIndex(VAL_ERR_INDEX);
                if (ParseValue(
                        root: root,
                        valTransl: valTransl,
                        item: out var val,
                        errorMask: errorMask,
                        translationMask: translationMask)
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
            finally
            {
                errorMask?.PopIndex();
            }

            item = default(KeyValuePair<K, V>);
            return false;
        }

        public void Write(
            XElement node,
            string name,
            IEnumerable<KeyValuePair<K, V>> items,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
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
            this.Write(
                node: node,
                name: name,
                items: items,
                errorMask: errorMask,
                translationMask: translationMask,
                keyTransl: (XElement n, K item1, ErrorMaskBuilder errorMask2, TranslationCrystal transl2)
                    => keyTransl.Item.Value.Write(
                        node: n, 
                        name: "Item",
                        item: item1,
                        errorMask: errorMask2,
                        translationMask: transl2),
                valTransl: (XElement n, V item1, ErrorMaskBuilder errorMask2, TranslationCrystal transl2) 
                    => valTransl.Item.Value.Write(
                        node: n,
                        name: "Item", 
                        item: item1, 
                        errorMask: errorMask2,
                        translationMask: transl2));
        }

        public void Write(
            XElement node,
            string name,
            IEnumerable<KeyValuePair<K, V>> items,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            XmlSubWriteDelegate<K> keyTransl,
            XmlSubWriteDelegate<V> valTransl)
        {
            var elem = new XElement(name);
            node.Add(elem);
            int i = 0;
            var keyTranslMask = translationMask?.GetSubCrystal(0);
            var valTranslMask = translationMask?.GetSubCrystal(1);
            foreach (var item in items)
            {
                try
                {
                    errorMask?.PushIndex(i++);
                    WriteSingleItem(
                        node: elem,
                        item: item,
                        errorMask: errorMask,
                        translationMaskKey: keyTranslMask,
                        translationMaskVal: valTranslMask,
                        keyTransl: keyTransl,
                        valTransl: valTransl);
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
        }

        public void WriteSingleItem(
            XElement node,
            KeyValuePair<K, V> item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMaskKey,
            TranslationCrystal translationMaskVal,
            XmlSubWriteDelegate<K> keyTransl,
            XmlSubWriteDelegate<V> valTransl)
        {
            var itemElem = new XElement("Item");
            node.Add(itemElem);
            var keyElem = new XElement("Key");
            itemElem.Add(keyElem);
            keyTransl(
                keyElem, 
                item.Key,
                errorMask: errorMask,
                translationMask: translationMaskKey);
            var valElem = new XElement("Value");
            itemElem.Add(valElem);
            valTransl(
                valElem, 
                item.Value,
                errorMask: errorMask,
                translationMask: translationMaskVal);
        }

        public void Write<Mask>(
            XElement node,
            string name,
            IEnumerable<KeyValuePair<K, V>> items,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            XmlSubWriteDelegate<K> keyTransl,
            XmlSubWriteDelegate<V> valTransl)
            where Mask : IErrorMask
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                this.Write(
                    node: node,
                    name: name,
                    items: items,
                    errorMask: errorMask,
                    translationMask: translationMask,
                    keyTransl: keyTransl,
                    valTransl: valTransl);
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

        public void Write<Mask>(
            XElement node,
            string name,
            IHasItem<IEnumerable<KeyValuePair<K, V>>> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            XmlSubWriteDelegate<K> keyTransl,
            XmlSubWriteDelegate<V> valTransl)
            where Mask : IErrorMask
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                this.Write(
                    node: node,
                    name: name,
                    items: item.Item,
                    errorMask: errorMask,
                    translationMask: translationMask,
                    keyTransl: keyTransl,
                    valTransl: valTransl);
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
    }
}
