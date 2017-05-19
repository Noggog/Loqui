using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Loqui.Xml;
using Loqui.Tests.Internals;

namespace Loqui.Tests.XML
{
    public class ObjectToRefXmlTranslation_Test : TypicalXmlTranslation_Test<ObjectToRef, ObjectToRefXmlTranslation_Test>
    {
        public static readonly ObjectToRef TYPICAL_VALUE = new ObjectToRef()
        {
            KeyField = 43,
            SomeField = true
        };

        public override string ExpectedName => "Loqui.Tests.ObjectToRef";

        public override ObjectToRef TypicalValue => TYPICAL_VALUE;

        public override IXmlTranslation<ObjectToRef> GetTranslation()
        {
            return new LoquiXmlTranslation<ObjectToRef, ObjectToRef_ErrorMask>();
        }

        public override XElement GetTypicalElement(string name = null)
        {
            var ret = XmlUtility.GetElementNoValue(ExpectedName, name);
            ret.Add(
                Int16XmlTranslation_Test.Instance.GetTypicalElement(nameof(ObjectToRef.KeyField)));
            ret.Add(
                BoolXmlTranslation_Test.Instance.GetTypicalElement(nameof(ObjectToRef.SomeField)));
            return ret;
        }
    }
}
