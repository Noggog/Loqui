using Noggolloquy.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Noggolloquy.Tests.XML
{
    public class BoolXmlTranslation_Test
    {
        public IXmlTranslation<bool> GetTranslation()
        {
            return new BooleanXmlTranslation();
        }

        public XElement GetTypicalElement(bool value, string name = null)
        {
            var elem = new XElement(XName.Get("Boolean"));
            if (!string.IsNullOrWhiteSpace(name))
            {
                elem.SetAttributeValue(XName.Get("name"), name);
            }
            elem.SetAttributeValue(XName.Get("value"), value ? "True" : "False");
            return elem;
        }

        [Fact]
        public void Parse_True()
        {
            var transl = GetTranslation();
            var elem = GetTypicalElement(true);
            var ret = transl.Parse(
                elem,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(ret.Succeeded);
            Assert.Equal(true, ret.Value);
        }

        [Fact]
        public void Parse_False()
        {
            var transl = GetTranslation();
            var elem = GetTypicalElement(false);
            var ret = transl.Parse(
                elem,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(ret.Succeeded);
            Assert.Equal(false, ret.Value);
        }

        [Fact]
        public void Parse_BadElementName_Mask()
        {
            var transl = GetTranslation();
            var elem = XmlUtility.GetBadlyNamedElement();
            var ret = transl.Parse(
                elem,
                doMasks: true,
                maskObj: out object maskObj);
            Assert.True(ret.Failed);
            Assert.NotNull(maskObj);
            Assert.IsType(typeof(ArgumentException), maskObj);
        }

        [Fact]
        public void Parse_BadElementName_NoMask()
        {
            var transl = GetTranslation();
            var elem = XmlUtility.GetBadlyNamedElement();
            Assert.Throws(
                typeof(ArgumentException),
                () => transl.Parse(
                    elem,
                    doMasks: false,
                    maskObj: out object maskObj));
        }
    }
}
