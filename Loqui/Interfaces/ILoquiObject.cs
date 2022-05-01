using Noggog;
using Noggog.StructuredStrings;

namespace Loqui;

public interface ILoquiObject
{
    ILoquiRegistration Registration { get; }
}

public interface ILoquiObject<out T> : ILoquiObjectGetter
{
}

public interface ILoquiReflectionGetter : ILoquiObject
{
    object? GetNthObject(ushort index);
    bool GetNthObjectHasBeenSet(ushort index);
}

public interface ILoquiObjectGetter : ILoquiObject, IPrintable
{
}

public interface IEqualsMask
{
    IMask<bool> GetEqualsMask(object rhs, EqualsMaskHelper.Include include = EqualsMaskHelper.Include.OnlyFailures);
}

public interface ILoquiObjectSetter : ILoquiObjectGetter, IClearable
{
}

public interface ILoquiObjectSetter<T> : ILoquiObjectSetter, ILoquiObject<T>
{
}

public interface ILoquiReflectionSetter : ILoquiReflectionGetter, ILoquiObjectSetter
{
    void SetNthObjectHasBeenSet(ushort index, bool on);
    void UnsetNthObject(ushort index);
    void SetNthObject(ushort index, object? o);
}

public static class ILoquiObjectExt
{
    public static void CopyFieldsIn(
        ILoquiReflectionSetter obj,
        ILoquiReflectionGetter rhs,
        ILoquiReflectionGetter def,
        bool skipProtected)
    {
        for (ushort i = 0; i < obj.Registration.FieldCount; i++)
        {
            if (skipProtected && obj.Registration.IsProtected(i)) continue;
            if (obj.Registration.IsNthDerivative(i)) continue;
            if (rhs.GetNthObjectHasBeenSet(i))
            {
                obj.SetNthObject(i, rhs.GetNthObject(i));
            }
            else
            {
                if (def != null && def.GetNthObjectHasBeenSet(i))
                {
                    obj.SetNthObject(i, def.GetNthObject(i));
                }
                else
                {
                    obj.UnsetNthObject(i);
                }
            }
        }
    }

    public static void CopyFieldsIn(
        ILoquiReflectionSetter obj,
        IEnumerable<KeyValuePair<ushort, object>> fields,
        ILoquiReflectionGetter def,
        bool skipProtected)
    {
        if (fields == null || !fields.Any()) return;
        HashSet<ushort> readFields = new HashSet<ushort>();
        foreach (var field in fields)
        {
            readFields.Add(field.Key);
            if (skipProtected && obj.Registration.IsProtected(field.Key)) continue;
            obj.SetNthObject(field.Key, field.Value);
        }

        for (ushort i = 0; i < obj.Registration.FieldCount; i++)
        {
            if (obj.Registration.IsNthDerivative(i)) continue;
            if (readFields.Contains(i)) continue;
            if (def != null && def.GetNthObjectHasBeenSet(i))
            {
                obj.SetNthObject(i, def.GetNthObject(i));
            }
            else
            {
                obj.UnsetNthObject(i);
            }
        }
    }

    public static void CopyFieldsIn(
        ILoquiReflectionSetter obj,
        IEnumerable<KeyValuePair<ushort, object>> fields,
        ILoquiReflectionGetter def,
        Func<IErrorMask> errorMaskGetter,
        bool skipProtected)
    {
        if (!fields.Any()) return;
        try
        {
            HashSet<ushort> readFields = new HashSet<ushort>();
            foreach (var field in fields)
            {
                readFields.Add(field.Key);
                if (skipProtected && obj.Registration.IsProtected(field.Key)) continue;
                try
                {
                    obj.SetNthObject(field.Key, field.Value);
                }
                catch (Exception ex)
                {
                    errorMaskGetter().SetNthException(field.Key, ex);
                }
            }

            for (ushort i = 0; i < obj.Registration.FieldCount; i++)
            {
                if (obj.Registration.IsNthDerivative(i)) continue;
                if (readFields.Contains(i)) continue;
                try
                {
                    if (def != null && def.GetNthObjectHasBeenSet(i))
                    {
                        obj.SetNthObject(i, def.GetNthObject(i));
                    }
                    else
                    {
                        obj.UnsetNthObject(i);
                    }
                }
                catch (Exception ex)
                {
                    errorMaskGetter().SetNthException(i, ex);
                }
            }
        }
        catch (Exception ex)
        {
            errorMaskGetter().Overall = ex;
        }
    }
}