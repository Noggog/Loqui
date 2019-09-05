using System;
using System.Collections.Generic;
using System.Text;

namespace Loqui.Generation
{
    public class MaskTypeSetCollection
    {
        private List<MaskTypeSet> sets = new List<MaskTypeSet>();

        public void Add(MaskTypeSet set)
        {
            this.sets.Add(set);
        }

        public bool Contains(LoquiInterfaceType interfaceType, CommonGenerics commonGen, params MaskType[] maskTypes)
        {
            MaskTypeSet set = new MaskTypeSet(interfaceType, maskTypes, acceptAll: false, commonGen: commonGen);
            return sets.Contains(set);
        }

    }
}
