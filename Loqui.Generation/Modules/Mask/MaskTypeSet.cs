namespace Loqui.Generation;

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
        CommonGen = commonGen;
        _maskTypeSet = new HashSet<MaskType>(types);
        AcceptingAll = acceptAll;
        _loquiInterface = interfaceType;
        IsMainSet = interfaceType == LoquiInterfaceType.IGetter && (_maskTypeSet.Count == 0 || (_maskTypeSet.Count == 1 && _maskTypeSet.Contains(MaskType.Normal)));
    }

    public bool Applicable(LoquiInterfaceType interfaceType, CommonGenerics commonGen, params MaskType[] maskTypes)
    {
        if (AcceptingAll) return true;
        if (commonGen != CommonGen) return false;
        if (interfaceType != _loquiInterface) return false;
        if (maskTypes?.Length == 0)
        {
            return _maskTypeSet.Count == 1 && _maskTypeSet.Contains(MaskType.Normal);
        }
        if (maskTypes.Length != _maskTypeSet.Count) return false;
        foreach (var maskType in maskTypes)
        {
            if (!_maskTypeSet.Contains(maskType)) return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_loquiInterface);
        hash.Add(_maskTypeSet.Select(m => m.GetHashCode()));
        return hash.ToHashCode();
    }

    public override bool Equals(object obj)
    {
        if (!(obj is MaskTypeSet rhs)) return false;
        return Equals(rhs);
    }

    public bool Equals(MaskTypeSet other)
    {
        if (_loquiInterface != other._loquiInterface) return false;
        if (_maskTypeSet.Count != other._maskTypeSet.Count) return false;
        foreach (var maskItem in _maskTypeSet)
        {
            if (!other._maskTypeSet.Contains(maskItem)) return false;
        }
        return true;
    }
}