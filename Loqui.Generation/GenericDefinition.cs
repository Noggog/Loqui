using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Loqui.Generation
{
    public class GenericDefinition
    {
        public bool MustBeClass;
        public ObjectGeneration BaseObjectGeneration;
        private readonly HashSet<string> _whereSet = new HashSet<string>();
        private readonly List<string> _whereList = new List<string>();
        public IEnumerable<string> Wheres => _whereList;

        public void Add(string where)
        {
            if (_whereSet.Add(where))
            {
                _whereList.Add(where);
            }
        }

        public void Add(IEnumerable<string> wheres)
        {
            foreach (var where in wheres)
            {
                Add(where);
            }
        }

        public GenericDefinition Copy()
        {
            var ret = new GenericDefinition()
            {
                MustBeClass = this.MustBeClass
            };
            ret._whereSet.Add(this._whereSet);
            ret._whereList.AddRange(this._whereList);
            return ret;
        }
    }
}
