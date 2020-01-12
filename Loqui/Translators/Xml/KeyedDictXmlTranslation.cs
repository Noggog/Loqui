using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noggog;
using System.Xml.Linq;
using Loqui.Internal;
using DynamicData;

namespace Loqui.Xml
{
    public class KeyedDictXmlTranslation<K, V> : IXmlTranslation<IEnumerable<V>>
    {
        public static readonly KeyedDictXmlTranslation<K, V> Instance = new KeyedDictXmlTranslation<K, V>();
        public virtual string ElementName => "Dict";

        public bool Parse(
            XElement node, 
            out IEnumerable<V> enumer,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            var valTransl = XmlTranslator<V>.Translator;
            if (valTransl.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(V)}. {valTransl.Reason}");
            }
            return Parse(
                node: node,
                enumer: out enumer,
                errorMask: errorMask,
                translationMask: translationMask,
                valTransl: valTransl.Value.Parse);
        }

        public void ParseInto(
            XElement node,
            ICache<V, K> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
                    if (Parse(
                        node: node,
                        enumer: out var enumer,
                        errorMask: errorMask,
                        translationMask: translationMask))
                    {
                        item.SetTo(enumer);
                    }
                    else
                    {
                        item.Clear();
                    }
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }
        }

        public void ParseInto(
            XElement node,
            ISourceCache<V, K> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
                    if (Parse(
                        node: node,
                        enumer: out var enumer,
                        errorMask: errorMask,
                        translationMask: translationMask))
                    {
                        item.SetTo(enumer);
                    }
                    else
                    {
                        item.Clear();
                    }
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }
        }

        public bool Parse(
            XElement node,
            XmlSubParseDelegate<V> valTransl,
            out IEnumerable<V> enumer,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            var ret = new List<V>();
            int i = 0;
            foreach (var listElem in node.Elements())
            {
                using (errorMask?.PushIndex(i++))
                {
                    try
                    {
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
                }
            }
            enumer = ret;
            return true;
        }

        public void ParseInto(
            XElement node,
            ICache<V, K> item,
            int fieldIndex,
            XmlSubParseDelegate<V> valTransl,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
                    if (Parse(
                        node: node,
                        valTransl: valTransl,
                        enumer: out var enumer,
                        errorMask: errorMask,
                        translationMask: translationMask))
                    {
                        item.SetTo(enumer);
                    }
                    else
                    {
                        item.Clear();
                    }
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }
        }

        public void ParseInto(
            XElement node,
            ISourceCache<V, K> item,
            int fieldIndex,
            XmlSubParseDelegate<V> valTransl,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
                    if (Parse(
                        node: node,
                        valTransl: valTransl,
                        enumer: out var enumer,
                        errorMask: errorMask,
                        translationMask: translationMask))
                    {
                        item.SetTo(enumer);
                    }
                    else
                    {
                        item.Clear();
                    }
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }
        }

        public virtual bool ParseSingleItem(
            XElement node,
            XmlSubParseDelegate<V> valTransl,
            out V item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            return valTransl(
                node, 
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
            if (valTransl.Failed)
            {
                throw new ArgumentException($"No XML Translator available for {typeof(V)}. {valTransl.Reason}");
            }
            this.Write(
                node: node,
                name: name,
                items: items,
                errorMask: errorMask,
                translationMask: translationMask,
                valTransl: (XElement n, V item1, ErrorMaskBuilder errorMask2, TranslationCrystal translationMask2) 
                    => valTransl.Value.Write(
                        node: n, 
                        name: null,
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
                using (errorMask?.PushIndex(i++))
                {
                    try
                    {
                        WriteSingleItem(
                            node: elem,
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
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
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
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
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
            }
        }
    }
}
