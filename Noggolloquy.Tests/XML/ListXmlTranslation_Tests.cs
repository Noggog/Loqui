using Noggolloquy.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Noggolloquy.Tests.XML
{
    public class ListXmlTranslation_Tests
    {
        public string ExpectedName => "List";

        public ListXmlTranslation<bool> GetTranslation()
        {
            return new ListXmlTranslation<bool>();
        }

        #region Element Name
        [Fact]
        public void ElementName()
        {
            var transl = GetTranslation();
            Assert.Equal(ExpectedName, transl.ElementName);
        }
        #endregion

        #region Single Items
        [Fact]
        public void ReimportSingleItem()
        {
            var transl = GetTranslation();
            var writer = XmlUtility.GetWriteBundle();
            var writeResp = transl.WriteSingleItem(
                writer: writer.Writer,
                item: true,
                doMasks: false,
                maskObj: out object maskObj);
            Assert.True(writeResp);
            var readResp = transl.ParseSingleItem(
                writer.Resolve(),
                doMasks: false,
                maskObj: out object readMaskObj);
            Assert.True(readResp.Succeeded);
            Assert.Equal(true, readResp.Value);
        }
        #endregion
    }
}
