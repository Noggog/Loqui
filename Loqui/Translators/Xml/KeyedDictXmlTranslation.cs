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
    public class KeyedDictXmlTranslation<K, V> : IXmlTranslation<IEnumerable<V>>
    {
        public static readonly KeyedDictXmlTranslation<K, V> Instance = new KeyedDictXmlTranslation<K, V>();
        public virtual string ElementName => "Dict";

        public bool Parse(
            XElement root, 
            out IEnumerable<V> enumer,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            var valTransl = XmlTranslator<V>.Translator;
            if (valTransl.Item.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(V)}. {valTransl.Item.Reason}");
            }
            return Parse(
                root: root,
                enumer: out enumer,
                errorMask: errorMask,
                translationMask: translationMask,
                valTransl: valTransl.Item.Value.Parse);
        }

        public void ParseInto(
            XElement root,
            INotifyingKeyedCollection<K, V> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                if (Parse(
                    root: root,
                    enumer: out var enumer,
                    errorMask: errorMask,
                    translationMask: translationMask))
                {
                    item.SetTo(enumer);
                }
                else
                {
                    item.Unset();
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
        }

        public bool Parse(
            XElement root,
            XmlSubParseDelegate<V> valTransl,
            out IEnumerable<V> enumer,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            var ret = new List<V>();
            int i = 0;
            foreach (var listElem in root.Elements())
            {
                try
                {
                    errorMask?.PushIndex(i++);
                    if (ParseSingleItem(
                        listElem, 
                        valTransl, 
                        out var subItem, 
                        errorMask: errorMask,
                        translationMask: translationMask))
                    {
                        ret.Add(subItem);
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
            }
            enumer = ret;
            return true;
        }

        public void ParseInto(
            XElement root,
            INotifyingKeyedCollection<K, V> item,
            int fieldIndex,
            XmlSubParseDelegate<V> valTransl,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                if (Parse(
                    root: root,
                    valTransl: valTransl,
                    enumer: out var enumer,
                    errorMask: errorMask,
                    translationMask: translationMask))
                {
                    item.SetTo(enumer);
                }
                else
                {
                    item.Unset();
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
        }

        public virtual bool ParseSingleItem(
            XElement root,
            XmlSubParseDelegate<V> valTransl,
            out V item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            return valTransl(
                root, 
                out item, 
                errorMask,
                translationMask);
        }

        public void Write(
            XElement node,
            string name,
            IEnumerable<V> items,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
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
                valTransl: (XElement n, V item1, ErrorMaskBuilder errorMask2, TranslationCrystal translationMask2) 
                    => valTransl.Item.Value.Write(
                        node: n, 
                        name: "Item",
                        item: item1, 
                        errorMask: errorMask2, 
                        translationMask: translationMask2));
        }

        public void Write(
            XElement node,
            string name,
            IEnumerable<V> items,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            XmlSubWriteDelegate<V> valTransl)
        {
            var elem = new XElement(name);
            node.Add(elem);
            int i = 0;
            foreach (var item in items)
            {
                try
                {
                    errorMask?.PushIndex(i++);
                    WriteSingleItem(
                        node: node,
                        item: item,
                        errorMask: errorMask,
                        translationMask: translationMask,
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
            V item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            XmlSubWriteDelegate<V> valTransl)
        {
            valTransl(
                node,
                item, 
                errorMask: errorMask,
                translationMask: translationMask);
        }

        public void Write(
            XElement node,
            string name,
            IEnumerable<V> items,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            XmlSubWriteDelegate<V> valTransl)
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

        public void Write(
            XElement node,
            string name,
            IHasItem<IEnumerable<V>> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            XmlSubWriteDelegate<V> valTransl)
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
