using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loqui.Generation
{
    public class MaskTypeSet : IEquatable<MaskTypeSet>
    {
        private LoquiInterfaceType _loquiInterface;
        private readonly HashSet<MaskType> _maskTypeSet;
        public MaskType[] MaskTypes => _maskTypeSet.ToArray();
        public bool AcceptingAll { get; }
        public bool IsMainSet { get; }
        public CommonGenerics CommonGen { get; }

        public MaskTypeSet(LoquiInterfaceType interfaceType, IEnumerable<MaskType> types, bool acceptAll, CommonGenerics commonGen)
        {
            this.CommonGen = commonGen;
            this._maskTypeSet = new HashSet<MaskType>(types);
            this.AcceptingAll = acceptAll;
            this._loquiInterface = interfaceType;
            this.IsMainSet = interfaceType == LoquiInterfaceType.IGetter && (this._maskTypeSet.Count == 0 || (this._maskTypeSet.Count == 1 && this._maskTypeSet.Contains(MaskType.Normal)));
        }

        public bool Applicable(LoquiInterfaceType interfaceType, CommonGenerics commonGen, params MaskType[] maskTypes)
        {
            if (this.AcceptingAll) return true;
            if (commonGen != this.CommonGen) return false;
            if (interfaceType != this._loquiInterface) return false;
            if (maskTypes?.Length == 0)
            {
                return this._maskTypeSet.Count == 1 && this._maskTypeSet.Contains(MaskType.Normal);
            }
            if (maskTypes.Length != this._maskTypeSet.Count) return false;
            foreach (var maskType in maskTypes)
            {
                if (!this._maskTypeSet.Contains(maskType)) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return _loquiInterface.GetHashCode()
                .CombineHashCode(_maskTypeSet.Select(m => m.GetHashCode()));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MaskTypeSet rhs)) return false;
            return Equals(rhs);
        }

        public bool Equals(MaskTypeSet other)
        {
            if (this._loquiInterface != other._loquiInterface) return false;
            if (this._maskTypeSet.Count != other._maskTypeSet.Count) return false;
            foreach (var maskItem in _maskTypeSet)
            {
                if (!other._maskTypeSet.Contains(maskItem)) return false;
            }
            return true;
        }
    }
}
