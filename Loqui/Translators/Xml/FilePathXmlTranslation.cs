using Loqui.Internal;
using Noggog;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class FilePathXmlTranslation : PrimitiveXmlTranslation<FilePath>
    {
        public readonly static FilePathXmlTranslation Instance = new FilePathXmlTranslation();

        protected override string GetItemStr(FilePath item)
        {
            return item.Path;
        }

        protected override bool Parse(string str, out FilePath value, ErrorMaskBuilder? errorMask)
        {
            value = new FilePath(str);
            return true;
        }
    }
}
