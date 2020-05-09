using Noggog;
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
            SubTypeGeneration = this.ObjectGen.ProtoGen.Gen.GetTypeGeneration<LoquiType>();
            SubTypeGeneration.SetObjectGeneration(this.ObjectGen, setDefaults: false);
            await SubTypeGeneration.Load(node, false);
            SubTypeGeneration.Name = null;
            isLoquiSingle = SubTypeGeneration as LoquiType != null;
        }
    }
}
