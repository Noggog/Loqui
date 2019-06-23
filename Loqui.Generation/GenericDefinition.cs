using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Loqui.Generation
{
    public class GenericDefinition
    {
        public bool MustBeClass;
        public bool Loqui;
        public ObjectGeneration BaseObjectGeneration;
        private readonly HashSet<string> _whereSet = new HashSet<string>();
        private readonly List<string> _whereList = new List<string>();
        public IEnumerable<string> Wheres => _whereList;
        public string Name;

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

        public IEnumerable<string> GetWheres(LoquiInterfaceType type)
        {
            if (this.BaseObjectGeneration != null)
            {
                yield return this.BaseObjectGeneration.GetTypeName(type);
            }
            foreach (var item in _whereList.Skip(BaseObjectGeneration == null ? 0 : 1))
            {
                yield return item;
            }
            if (Loqui)
            {
                yield return $"ILoquiObject<{Name}>";
            }
        }

        public GenericDefinition Copy()
        {
            var ret = new GenericDefinition()
            {
                MustBeClass = this.MustBeClass
            };
            ret.BaseObjectGeneration = this.BaseObjectGeneration;
            ret.Loqui = this.Loqui;
            ret._whereSet.Add(this._whereSet);
            ret._whereList.AddRange(this._whereList);
            return ret;
        }
    }
}
