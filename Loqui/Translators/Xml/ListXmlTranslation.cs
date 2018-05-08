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
    public class ListXmlTranslation<T, M> : ContainerXmlTranslation<T, M>
    {
        public static readonly ListXmlTranslation<T, M> Instance = new ListXmlTranslation<T, M>();

        public override string ElementName => "List";

        public override void WriteSingleItem<ErrMask>(XElement node, XmlSubWriteDelegate<T, ErrMask> transl, T item, bool doMasks, out ErrMask maskObj)
        {
            transl(node, item, doMasks, out maskObj);
        }

        public override TryGet<T> ParseSingleItem(XElement root, XmlSubParseDelegate<T, M> transl, bool doMasks, out M maskObj)
        {
            return transl(root, doMasks, out maskObj);
        }
    }
}
