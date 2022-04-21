using Noggog;
using Noggog.Printing;
using System.Collections;

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
    public static string PrintPretty(ILoquiReflectionGetter obj)
    {
        return PrintPretty(obj, new DepthPrinter());
    }

    public static string PrintPretty(ILoquiReflectionGetter obj, DepthPrinter depthPrinter)
    {
        depthPrinter.AddLine(obj.Registration.Name + "=>");
        return PrintPrettyInternal(obj, depthPrinter);
    }

    private static string PrintLoquiName(ILoquiReflectionGetter obj, ushort i)
    {
        return $"{obj.Registration.GetNthName(i)}({obj.Registration.GetNthType(i).Name}) => ";
    }

    private static string PrintPrettyInternal(this ILoquiReflectionGetter loqui, DepthPrinter depthPrinter)
    {
        depthPrinter.AddLine("[");
        using (depthPrinter.IncrementDepth())
        {
            for (ushort i = 0; i < loqui.Registration.FieldCount; i++)
            {
                var obj = loqui.GetNthObject(i);
                if (loqui.Registration.GetNthIsEnumerable(i))
                {
                    if (obj is IEnumerable listObj)
                    {
                        bool hasItems = listObj.Any();
                        depthPrinter.AddLine(loqui.Registration.GetNthName(i) + " => " + (hasItems ? string.Empty : "[ ]"));
                        if (hasItems)
                        {
                            depthPrinter.AddLine("[");
                            using (depthPrinter.IncrementDepth())
                            {
                                foreach (var listItem in listObj)
                                {
                                    if (loqui.Registration.GetNthIsLoqui(i))
                                    {
                                        if (listItem is ILoquiReflectionGetter subLoqui)
                                        {
                                            depthPrinter.AddLine(PrintLoquiName(loqui, i));
                                            PrintPrettyInternal(subLoqui, depthPrinter);
                                        }
                                        continue;
                                    }

                                    depthPrinter.AddLine(obj.ToString() + ",");
                                }
                            }
                            depthPrinter.AddLine("]");
                        }
                    }
                    continue;
                }

                if (loqui.Registration.GetNthIsLoqui(i))
                {
                    if (obj is ILoquiReflectionGetter subLoqui)
                    {
                        depthPrinter.AddLine(PrintLoquiName(loqui, i));
                        PrintPrettyInternal(subLoqui, depthPrinter);
                    }
                    continue;
                }

                depthPrinter.AddLine(loqui.Registration.GetNthName(i) + ": " + obj?.ToString());
            }
        }
        depthPrinter.AddLine("]");
        return depthPrinter.ToString();
    }

    private static void PrintItem(ILoquiObjectGetter loqui, ushort i, object o, DepthPrinter dp)
    {
    }

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