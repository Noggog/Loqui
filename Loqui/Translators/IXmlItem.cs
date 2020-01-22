using Loqui.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using Noggog;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace Loqui.Xml
{
    public interface IXmlItem
    {
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        object XmlWriteTranslator { get; }
        void WriteToXml(
            XElement node,
            ErrorMaskBuilder? errorMask = null,
            TranslationCrystal? translationMask = null,
            string name = "");
    }

    public static class IXmlItemExt
    {
        public static void WriteToXml(
            this IXmlItem item,
            string path,
            ErrorMaskBuilder? errorMask = null,
            TranslationCrystal? translationMask = null,
            string name = "")
        {
            var node = new XElement("topnode");
            item.WriteToXml(
                name: name,
                node: node,
                errorMask: errorMask,
                translationMask: translationMask);
            node.Elements().First().SaveIfChanged(path);
        }

        public static void WriteToXml(
            this IXmlItem item,
            Stream stream,
            ErrorMaskBuilder? errorMask = null,
            TranslationCrystal? translationMask = null,
            string name = "")
        {
            var node = new XElement("topnode");
            item.WriteToXml(
                name: name,
                node: node,
                errorMask: errorMask,
                translationMask: translationMask);
            node.Elements().First().Save(stream);
        }
    }
}
