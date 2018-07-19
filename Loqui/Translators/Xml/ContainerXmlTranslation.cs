using Loqui.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noggog;
using System.Xml;
using System.Xml.Linq;
using Noggog.Notifying;
using Noggog.Xml;
using Loqui.Internal;

namespace Loqui.Xml
{
    public abstract class ContainerXmlTranslation<T> : IXmlTranslation<IEnumerable<T>>
    {
        public abstract string ElementName { get; }

        public void ParseInto(
            XElement root,
            int fieldIndex,
            INotifyingList<T> item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                if (Parse(
                    root,
                    out var enumer,
                    errorMask,
                    translationMask))
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
            out IEnumerable<T> enumer,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            var transl = XmlTranslator<T>.Translator;
            if (transl.Item.Failed)
            {
                errorMask.ReportExceptionOrThrow(
                    new ArgumentException($"No XML Translator available for {typeof(T)}. {transl.Item.Reason}"));
                enumer = null;
                return false;
            }
            return Parse(
                root,
                out enumer,
                errorMask: errorMask,
                transl: transl.Item.Value.Parse,
                translationMask: translationMask);
        }

        public void ParseInto(
            XElement root,
            int fieldIndex,
            INotifyingList<T> item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            XmlSubParseDelegate<T> transl)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                if (Parse(
                    root,
                    out var enumer,
                    errorMask,
                    translationMask,
                    transl))
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
            out IEnumerable<T> enumer,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            XmlSubParseDelegate<T> transl)
        {
            var ret = new List<T>();
            int i = 0;
            var subCrystal = translationMask.GetSubCrystal(0);
            foreach (var listElem in root.Elements())
            {
                try
                {
                    errorMask?.PushIndex(i++);
                    if (transl(listElem, out var subItem, errorMask, subCrystal))
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

        public void Write(
            XElement node,
            string name,
            IEnumerable<T> item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask)
        {
            var transl = XmlTranslator<T>.Translator;
            if (transl.Item.Failed)
            {
                errorMask.ReportExceptionOrThrow(
                    new ArgumentException($"No XML Translator available for {typeof(T)}. {transl.Item.Reason}"));
            }
            this.Write(
                node: node,
                name: name,
                item: item,
                errorMask: errorMask,
                translationMask: translationMask,
                transl: (XElement n, T item1, ErrorMaskBuilder errorMask2, TranslationCrystal transCrystal2) => 
                    transl.Item.Value.Write(
                        node: n, 
                        name: "Item", 
                        item: item1,
                        errorMask: errorMask2,
                        translationMask: transCrystal2));
        }

        public void Write(
            XElement node,
            string name,
            IEnumerable<T> item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            XmlSubWriteDelegate<T> transl)
        {
            var elem = new XElement(name);
            node.Add(elem);
            int i = 0;
            var subCrystal = translationMask.GetSubCrystal(0);
            foreach (var listObj in item)
            {
                try
                {
                    errorMask?.PushIndex(i++);
                    WriteSingleItem(elem, transl, listObj, errorMask, subCrystal);
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

        public abstract void WriteSingleItem(
            XElement node,
            XmlSubWriteDelegate<T> transl,
            T item, 
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask);

        public void Write(
            XElement node,
            string name,
            IEnumerable<T> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            XmlSubWriteDelegate<T> transl)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                this.Write(
                    node: node,
                    name: name,
                    item: item,
                    errorMask: errorMask,
                    translationMask: translationMask,
                    transl: transl);
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
            IHasItem<IEnumerable<T>> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            XmlSubWriteDelegate<T> transl)
        {
            try
            {
                errorMask?.PushIndex(fieldIndex);
                this.Write(
                    node,
                    name,
                    item.Item,
                    errorMask,
                    translationMask,
                    transl);
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
