using Loqui.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public interface IXmlWriteTranslator
    {
        void Write(
            XElement node,
            object item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            string name);
    }
    
    public interface IXmlItem
    {
        object XmlWriteTranslator { get; }
    }
}
