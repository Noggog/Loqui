using Noggog;
using System.Xml.Linq;

namespace Loqui.Generation;

public class GenericDefinition
{
    public bool MustBeClass;
    public bool Loqui;
    public ObjectGeneration BaseObjectGeneration;
    private readonly HashSet<string> _whereSet = new HashSet<string>();
    private readonly List<string> _whereList = new List<string>();
    public IEnumerable<string> Wheres => _whereList;
    public string Name;
    public bool Override;
    public Variance GetterVariance = Variance.Out;
    public Variance SetterVariance = Variance.None;

    public string GetterName => VarianceName(GetterVariance);
    public string SetterName => VarianceName(SetterVariance);

    private string VarianceName(Variance variance)
    {
        switch (variance)
        {
            case Variance.None:
                return Name;
            case Variance.In:
                return $"in {Name}";
            case Variance.Out:
                return $"out {Name}";
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
        Loqui = node.GetAttribute<bool>("isLoqui", defaultVal: false);
        Name = node.GetAttribute(Constants.NAME);
        MustBeClass = node.GetAttribute<bool>(Constants.IS_CLASS);
        GetterVariance = node.GetAttribute<Variance>(Constants.GETTER_VARIANCE, GetterVariance);
        SetterVariance = node.GetAttribute<Variance>(Constants.SETTER_VARIANCE, SetterVariance);
        Override = node.GetAttribute<bool>(Constants.OVERRIDE, Override);
        var baseClass = node.Element(XName.Get(Constants.BASE_CLASS, LoquiGenerator.Namespace));
        if (baseClass != null)
        {
            Add(baseClass.Value);
        }
        foreach (var where in node.Elements(XName.Get(Constants.WHERE, LoquiGenerator.Namespace)))
        {
            Add(where.Value);
        }
    }

    public void Resolve(ObjectGeneration obj)
    {
        if (!Wheres.Any()) return;
        if (!Loqui)
        {
            var loquiElem = Wheres.FirstOrDefault((i) =>
                i.Equals(nameof(ILoquiObjectGetter))
                || i.Equals(nameof(ILoquiObject)));
            Loqui = loquiElem != null;
        }
        if (!ObjectNamedKey.TryFactory(Wheres.First(), obj.ProtoGen.Protocol, out var objGenKey)) return;
        if (!obj.ProtoGen.Gen.ObjectGenerationsByObjectNameKey.TryGetValue(objGenKey, out var baseObjGen)) return;
        BaseObjectGeneration = baseObjGen;
        Loqui = true;
    }

    public IEnumerable<string> GetWheres(LoquiInterfaceType type)
    {
        if (BaseObjectGeneration != null)
        {
            yield return BaseObjectGeneration.GetTypeName(type);
        }
        foreach (var item in _whereList.Skip(BaseObjectGeneration == null ? 0 : 1))
        {
            yield return item;
        }
    }

    public GenericDefinition Copy()
    {
        var ret = new GenericDefinition()
        {
            MustBeClass = MustBeClass
        };
        ret.Name = Name;
        ret.MustBeClass = MustBeClass;
        ret.GetterVariance = GetterVariance;
        ret.SetterVariance = SetterVariance;
        ret.BaseObjectGeneration = BaseObjectGeneration;
        ret.Loqui = Loqui;
        ret._whereSet.Add(_whereSet);
        ret._whereList.AddRange(_whereList);
        return ret;
    }
}