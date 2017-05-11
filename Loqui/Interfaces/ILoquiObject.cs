using Noggog;
using Noggog.Notifying;
using Noggog.Printing;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Loqui
{
    public interface ILoquiObject
    {
        ILoquiRegistration Registration { get; }
    }

    public interface ILoquiObjectGetter : ILoquiObject
    {
        object GetNthObject(ushort index);
        bool GetNthObjectHasBeenSet(ushort index);
    }

    public interface ILoquiObjectSetter : ILoquiObjectGetter, IClearable
    {
        void SetNthObjectHasBeenSet(ushort index, bool on);
        void UnsetNthObject(ushort index, NotifyingUnsetParameters? cmds);
        void SetNthObject(ushort index, object o, NotifyingFireParameters? cmds);
    }

    public interface ILoquiClass<L, G>
        where L : class, G
        where G : class
    {
    }

    public static class ILoquiObjectExt
    {
        public static string PrintPretty(ILoquiObjectGetter obj)
        {
            return PrintPretty(obj, new DepthPrinter());
        }

        public static string PrintPretty(ILoquiObjectGetter obj, DepthPrinter depthPrinter)
        {
            depthPrinter.AddLine(obj.Registration.Name + "=>");
            return PrintPrettyInternal(obj, depthPrinter);
        }

        private static string PrintLoquiName(ILoquiObjectGetter obj, ushort i)
        {
            return $"{obj.Registration.GetNthName(i)}({obj.Registration.GetNthType(i).Name}) => ";
        }

        private static string PrintPrettyInternal(this ILoquiObjectGetter loqui, DepthPrinter depthPrinter)
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
                                            if (listItem is ILoquiObjectGetter subLoqui)
                                            {
                                                depthPrinter.AddLine(PrintLoquiName(loqui, i));
                                                PrintPrettyInternal(subLoqui, depthPrinter);
                                            }
                                            continue;
                                        }

                                        depthPrinter.AddLine(obj.ToStringSafe() + ",");
                                    }
                                }
                                depthPrinter.AddLine("]");
                            }
                        }
                        continue;
                    }

                    if (loqui.Registration.GetNthIsLoqui(i))
                    {
                        if (obj is ILoquiObjectGetter subLoqui)
                        {
                            depthPrinter.AddLine(PrintLoquiName(loqui, i));
                            PrintPrettyInternal(subLoqui, depthPrinter);
                        }
                        continue;
                    }

                    depthPrinter.AddLine(loqui.Registration.GetNthName(i) + ": " + obj.ToStringSafe());
                }
            }
            depthPrinter.AddLine("]");
            return depthPrinter.ToString();
        }

        private static void PrintItem(ILoquiObjectGetter loqui, ushort i, object o, DepthPrinter dp)
        {
        }

        public static void CopyFieldsIn(
            ILoquiObjectSetter obj,
            ILoquiObjectGetter rhs,
            ILoquiObjectGetter def,
            bool skipProtected,
            NotifyingFireParameters? cmds = null)
        {
            for (ushort i = 0; i < obj.Registration.FieldCount; i++)
            {
                if (skipProtected && obj.Registration.IsProtected(i)) continue;
                if (obj.Registration.IsNthDerivative(i)) continue;
                if (rhs.GetNthObjectHasBeenSet(i))
                {
                    obj.SetNthObject(i, rhs.GetNthObject(i), cmds);
                }
                else
                {
                    if (def != null && def.GetNthObjectHasBeenSet(i))
                    {
                        obj.SetNthObject(i, def.GetNthObject(i), cmds);
                    }
                    else
                    {
                        obj.UnsetNthObject(i, cmds.ToUnsetParams());
                    }
                }
            }
        }

        public static void CopyFieldsIn(
            ILoquiObjectSetter obj,
            IEnumerable<KeyValuePair<ushort, object>> fields,
            ILoquiObjectGetter def,
            bool skipProtected,
            NotifyingFireParameters? cmds = null)
        {
            if (fields == null || !fields.Any()) return;
            HashSet<ushort> readFields = new HashSet<ushort>();
            foreach (var field in fields)
            {
                readFields.Add(field.Key);
                if (skipProtected && obj.Registration.IsProtected(field.Key)) continue;
                obj.SetNthObject(field.Key, field.Value, cmds);
            }

            for (ushort i = 0; i < obj.Registration.FieldCount; i++)
            {
                if (obj.Registration.IsNthDerivative(i)) continue;
                if (readFields.Contains(i)) continue;
                if (def != null && def.GetNthObjectHasBeenSet(i))
                {
                    obj.SetNthObject(i, def.GetNthObject(i), cmds);
                }
                else
                {
                    obj.UnsetNthObject(i, cmds.ToUnsetParams());
                }
            }
        }

        public static void CopyFieldsIn(
            ILoquiObjectSetter obj,
            IEnumerable<KeyValuePair<ushort, object>> fields,
            ILoquiObjectGetter def,
            Func<IErrorMask> errorMaskGetter,
            bool skipProtected,
            NotifyingFireParameters? cmds = null)
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
                        obj.SetNthObject(field.Key, field.Value, cmds);
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
                            obj.SetNthObject(i, def.GetNthObject(i), cmds);
                        }
                        else
                        {
                            obj.UnsetNthObject(i, cmds.ToUnsetParams());
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
}
