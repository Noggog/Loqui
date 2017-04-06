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
    public abstract class TypicalXmlTranslation_Test<T>
    {
        public abstract string ExpectedName { get; }
        public abstract IXmlTranslation<T> GetTranslation();

        public virtual XElement GetTypicalElement(T value, string name = null)
        {
            var elem = XmlUtility.GetElementNoValue(ExpectedName, name);
            elem.SetAttributeValue(XName.Get(XmlConstants.VALUE_ATTRIBUTE), StringConverter(value));
            return elem;
        }

        public XElement GetElementNoValue()
        {
            return XmlUtility.GetElementNoValue(this.ExpectedName);
        }

        public virtual string StringConverter(T item)
        {
            return item.ToString();
        }

        public virtual T GetDefault()
        {
            return default(T);
        }
        
        [Fact]
        public void ElementName()
        {
            var transl = GetTranslation();
            Assert.Equal(ExpectedName, transl.ElementName);
        }

        [Fact]
        public void Write_NodeName()
        {
            var name = "AName";
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            var ret = transl.Write(
                writer: writer.Writer,
                name: name,
                item: GetDefault(),
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(ret);
            Assert.Null(maskObj);
            XElement elem = writer.Resolve();
            var nameAttr = elem.Attribute(XName.Get(XmlConstants.NAME_ATTRIBUTE));
            Assert.NotNull(nameAttr);
            Assert.Equal(name, nameAttr.Value);
        }
    }
}
