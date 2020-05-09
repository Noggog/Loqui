using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Loqui.Internal;

namespace Loqui.Xml
{
    public class ListXmlTranslation<T> : ContainerXmlTranslation<T>
    {
        public static readonly ListXmlTranslation<T> Instance = new ListXmlTranslation<T>();

        public override string ElementName => "List";

        public override void WriteSingleItem(
            XElement node,
            XmlSubWriteDelegate<T> transl,
            T item,
            ErrorMaskBuilder? errorMask,
            TranslationCrystal? translMask)
        {
            transl(
                node, 
                item, 
                errorMask,
                translMask);
        }
    }
}
