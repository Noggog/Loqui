using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class ByteArrayXmlTranslation : TypicalXmlTranslation<byte[]>
    {
        public readonly static ByteArrayXmlTranslation Instance = new ByteArrayXmlTranslation();

        protected override string GetItemStr(byte[] item)
        {
            return item.ToHexString();
        }

        protected override void WriteValue(XmlWriter writer, string name, byte[] item)
        {
            if (item == null) return;
            base.WriteValue(writer, name, item);
        }

        protected override Byte[] ParseValue(XElement root)
        {
            if (!root.TryGetAttribute(XmlConstants.VALUE_ATTRIBUTE, out XAttribute val)
                || val.Value == null)
            {
                return null;
            }
            return ParseNonNullString(val.Value);
        }

        protected override byte[] ParseNonNullString(string str)
        {
            if (str.Length % 2 != 0)
            {
                throw new ArgumentException($"Unexpected byte array length: {str.Length}.  Should be even.");
            }
            int i = 0;
            try
            {
                byte[] ret = new byte[str.Length / 2];
                for (; i < str.Length; i += 2)
                {
                    ret[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
                }
                return ret;
            }
            catch (FormatException)
            {
                throw new ArgumentException("Malformed byte: " + str.Substring(i, 2));
            }
        }
    }
}
