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

        #region Element Name
        [Fact]
        public void ElementName()
        {
            var transl = GetTranslation();
            Assert.Equal(ExpectedName, transl.ElementName);
        }
        #endregion
    }
}
