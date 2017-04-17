using System;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class GenericDefinition
    {
        public bool MustBeClass;
        public HashSet<string> Wheres = new HashSet<string>();

        public GenericDefinition Copy()
        {
            var ret = new GenericDefinition()
            {
                MustBeClass = this.MustBeClass
            };
            ret.Wheres.Add(this.Wheres);
            return ret;
        }
    }
}
