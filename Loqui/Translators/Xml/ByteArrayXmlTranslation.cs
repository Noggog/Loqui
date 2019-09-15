using Loqui.Internal;
using Noggog;
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

        protected virtual string GetItemStr(ReadOnlySpan<byte> item)
        {
            return item.ToHexString();
        }

        protected override void WriteValue(XElement node, string name, byte[] item)
        {
            if (item == null) return;
            base.WriteValue(node, name, item);
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

        public void Write(
            XElement node,
            string name,
            ReadOnlySpan<byte> item,
            int fieldIndex,
            ErrorMaskBuilder errorMask)
        {
            using (errorMask?.PushIndex(fieldIndex))
            {
                try
                {
                    this.Write(
                        node: node,
                        name: name,
                        item: item);
                }
                catch (Exception ex)
                when (errorMask != null)
                {
                    errorMask.ReportException(ex);
                }
            }
        }

        protected virtual void WriteValue(XElement node, string name, ReadOnlySpan<byte> item)
        {
            node.SetAttributeValue(
                XmlConstants.VALUE_ATTRIBUTE,
                item != null ? GetItemStr(item) : string.Empty);
        }

        private void Write(
            XElement node,
            string name,
            ReadOnlySpan<byte> item)
        {
            var elem = new XElement(name);
            node.Add(elem);
            WriteValue(elem, name, item);
        }
    }
}
