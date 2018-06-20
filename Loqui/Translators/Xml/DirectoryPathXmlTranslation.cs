using Loqui.Internal;
using Noggog;
using Noggog.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public class DirectoryPathXmlTranslation : PrimitiveXmlTranslation<DirectoryPath>
    {
        public readonly static DirectoryPathXmlTranslation Instance = new DirectoryPathXmlTranslation();

        protected override string GetItemStr(DirectoryPath item)
        {
            return item.Path;
        }

        protected override bool ParseNonNullString(string str, out DirectoryPath value, ErrorMaskBuilder errorMask)
        {
            value = new DirectoryPath(str);
            return true;
        }
    }
}
