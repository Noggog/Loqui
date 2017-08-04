using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class LoquiListType : ListType
    {
        public override void Load(XElement node, bool requireName = true)
        {
            LoadTypeGenerationFromNode(node, requireName);
            SingleTypeGen = new LoquiType()
            {
                ObjectGen = this.ObjectGen,
                ProtoGen = this.ProtoGen
            };
            SingleTypeGen.Load(node, false);
            singleType = true;
            isLoquiSingle = SingleTypeGen as LoquiType != null;
        }
    }
}
