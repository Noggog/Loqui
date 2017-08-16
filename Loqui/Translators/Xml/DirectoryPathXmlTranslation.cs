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

        protected override DirectoryPath ParseNonNullString(string str)
        {
            return new DirectoryPath(str);
        }
    }
}
