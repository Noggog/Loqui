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

        public bool Contains(LoquiInterfaceType interfaceType, params MaskType[] maskTypes)
        {
            MaskTypeSet set = new MaskTypeSet(interfaceType, maskTypes, false);
            return sets.Contains(set);
        }

    }
}
