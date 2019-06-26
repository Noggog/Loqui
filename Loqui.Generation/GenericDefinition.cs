using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

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
        public Variance GetterVariance = Variance.Out;
        public Variance SetterVariance = Variance.None;

        public string GetterName => VarianceName(this.GetterVariance);
        public string SetterName => VarianceName(this.SetterVariance);

        private string VarianceName(Variance variance)
        {
            switch (variance)
            {
                case Variance.None:
                    return this.Name;
                case Variance.In:
                    return $"in {this.Name}";
                case Variance.Out:
                    return $"out {this.Name}";
                default:
                    throw new NotImplementedException();
            }
        }

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

        public void Load(XElement node)
        {
            this.Loqui = node.GetAttribute<bool>("isLoqui", defaultVal: false);
            this.Name = node.GetAttribute(Constants.NAME);
            this.MustBeClass = node.GetAttribute<bool>(Constants.IS_CLASS);
            this.GetterVariance = node.GetAttribute<Variance>(Constants.GETTER_VARIANCE, this.GetterVariance);
            this.SetterVariance = node.GetAttribute<Variance>(Constants.SETTER_VARIANCE, this.SetterVariance);
            var baseClass = node.Element(XName.Get(Constants.BASE_CLASS, LoquiGenerator.Namespace));
            if (baseClass != null)
            {
                this.Add(baseClass.Value);
            }
            foreach (var where in node.Elements(XName.Get(Constants.WHERE, LoquiGenerator.Namespace)))
            {
                this.Add(where.Value);
            }
        }

        public void Resolve(ObjectGeneration obj)
        {
            if (!this.Wheres.Any()) return;
            if (!this.Loqui)
            {
                var loquiElem = this.Wheres.FirstOrDefault((i) =>
                    i.Equals(nameof(ILoquiObjectGetter))
                    || i.Equals(nameof(ILoquiObject)));
                this.Loqui = loquiElem != null;
            }
            if (!ObjectNamedKey.TryFactory(this.Wheres.First(), obj.ProtoGen.Protocol, out var objGenKey)) return;
            if (!obj.ProtoGen.Gen.ObjectGenerationsByObjectNameKey.TryGetValue(objGenKey, out var baseObjGen)) return;
            this.BaseObjectGeneration = baseObjGen;
            this.Loqui = true;
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
