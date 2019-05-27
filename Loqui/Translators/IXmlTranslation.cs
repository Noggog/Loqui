using Loqui.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public interface IXmlTranslator
    {
        void Write_Xml(
            XElement node,
            object item,
            ErrorMaskBuilder errorMask,
            TranslationCrystal translationMask,
            string name);
    }

    public interface IXmlItem
    {
        IXmlTranslator XmlTranslator { get; }
    }
}
