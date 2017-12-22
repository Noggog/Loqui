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
        public override async Task Load(XElement node, bool requireName = true)
        {
            LoadTypeGenerationFromNode(node, requireName);
            SingleTypeGen = new LoquiType();
            SingleTypeGen.SetObjectGeneration(this.ObjectGen, setDefaults: false);
            await SingleTypeGen.Load(node, false);
            SingleTypeGen.Name = null;
            singleType = true;
            isLoquiSingle = SingleTypeGen as LoquiType != null;
            this.MaxValue = node.GetAttribute<int?>("maxSize", null);
        }
    }
}
