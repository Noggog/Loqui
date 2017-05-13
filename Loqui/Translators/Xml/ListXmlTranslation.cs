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
    public class ListXmlTranslation<T> : ContainerXmlTranslation<T>
    {
        public static readonly ListXmlTranslation<T> Instance = new ListXmlTranslation<T>();

        public override string ElementName => "List";

        public override void WriteSingleItem<ErrMask>(XmlWriter writer, XmlSubWriteDelegate<T, ErrMask> transl, T item, bool doMasks, out ErrMask maskObj)
        {
            transl(item, out maskObj);
        }

        public override TryGet<T> ParseSingleItem(XElement root, IXmlTranslation<T> transl, bool doMasks, out object maskObj)
        {
            return transl.Parse(root, doMasks, out maskObj);
        }
    }
}
