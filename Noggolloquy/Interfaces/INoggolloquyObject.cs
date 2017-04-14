using Noggog;
using Noggog.Notifying;
using Noggog.Printing;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Noggolloquy
{
    public interface INoggolloquyObjectGetter : ICopyable
    {
        string NoggolloquyFullName { get; }

        string NoggolloquyName { get; }

        ProtocolDefinition Noggolloquy_ProtocolDefinition { get; }

        ObjectKey Noggolloquy_ObjectKey { get; }

        string Noggolloquy_GUID { get; }

        int FieldCount { get; }

        object GetNthObject(ushort index);

        bool GetNthObjectHasBeenSet(ushort index);

        Type GetNthType(ushort index);

        string GetNthName(ushort index);

        bool GetNthIsNoggolloquy(ushort index);

        bool GetNthIsEnumerable(ushort index);

        bool GetNthIsSingleton(ushort index);

        bool IsNthDerivative(ushort index);

        ushort? GetNameIndex(StringCaseAgnostic name);

        bool IsReadOnly(ushort index);

        Type GetMaskType();

        Type GetErrorMaskType();
    }

    public interface INoggolloquyObjectSetter : INoggolloquyObjectGetter
    {
        void SetNthObjectHasBeenSet(ushort index, bool on);
        void UnsetNthObject(ushort index, NotifyingUnsetParameters? cmds);
        void SetNthObject(ushort index, object o, NotifyingFireParameters? cmds);
    }

    public interface INoggolloquyClass<L, G> : ICopyFrom<L, G>, IClearable
        where L : class, G
        where G : class
    {
    }

    public static class INoggolloquyObjectExt
    {
        public static bool HasFieldWithName(this INoggolloquyObjectGetter obj, StringCaseAgnostic name)
        {
            return -1 != obj.GetNameIndex(name);
        }

        public static IEnumerable EnumerateFields(this INoggolloquyObjectGetter obj)
        {
            for (ushort i = 0; i < obj.FieldCount; i++)
            {
                yield return obj.GetNthObject(i);
            }
        }

        public static string PrintPretty(this INoggolloquyObjectGetter obj)
        {
            return obj.PrintPretty(new DepthPrinter());
        }

        public static string PrintPretty(this INoggolloquyObjectGetter obj, DepthPrinter depthPrinter)
        {
            depthPrinter.AddLine(obj.NoggolloquyName + "=>");
            return PrintPrettyInternal(obj, depthPrinter);
        }

        private static string PrintNoggName(INoggolloquyObjectGetter obj, ushort i)
        {
            return obj.GetNthName(i) + "(" + obj.GetNthType(i).Name + ")" + " => ";
        }

        private static string PrintPrettyInternal(this INoggolloquyObjectGetter nogg, DepthPrinter depthPrinter)
        {
            depthPrinter.AddLine("[");
            using (depthPrinter.IncrementDepth())
            {
                for (ushort i = 0; i < nogg.FieldCount; i++)
                {
                    var obj = nogg.GetNthObject(i);
                    if (nogg.GetNthIsEnumerable(i))
                    {
                        if (obj is IEnumerable listObj)
                        {
                            bool hasItems = listObj.Any();
                            depthPrinter.AddLine(nogg.GetNthName(i) + " => " + (hasItems ? string.Empty : "[ ]"));
                            if (hasItems)
                            {
                                depthPrinter.AddLine("[");
                                using (depthPrinter.IncrementDepth())
                                {
                                    foreach (var listItem in listObj)
                                    {
                                        if (nogg.GetNthIsNoggolloquy(i))
                                        {
                                            if (listItem is INoggolloquyObjectGetter subNogg)
                                            {
                                                depthPrinter.AddLine(PrintNoggName(nogg, i));
                                                PrintPrettyInternal(subNogg, depthPrinter);
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

                    if (nogg.GetNthIsNoggolloquy(i))
                    {
                        if (obj is INoggolloquyObjectGetter subNogg)
                        {
                            depthPrinter.AddLine(PrintNoggName(nogg, i));
                            PrintPrettyInternal(subNogg, depthPrinter);
                        }
                        continue;
                    }

                    depthPrinter.AddLine(nogg.GetNthName(i) + ": " + obj.ToStringSafe());
                }
            }
            depthPrinter.AddLine("]");
            return depthPrinter.ToString();
        }

        private static void PrintItem(INoggolloquyObjectGetter nogg, ushort i, object o, DepthPrinter dp)
        {
        }

        public static void CopyFieldsIn(
            this INoggolloquyObjectSetter obj,
            IEnumerable<KeyValuePair<ushort, object>> fields,
            INoggolloquyObjectGetter def,
            bool skipReadonly,
            NotifyingFireParameters? cmds = null)
        {
            if (fields == null || !fields.Any()) return;
            HashSet<ushort> readFields = new HashSet<ushort>();
            foreach (var field in fields)
            {
                readFields.Add(field.Key);
                if (skipReadonly && obj.IsReadOnly(field.Key)) continue;
                obj.SetNthObject(field.Key, field.Value, cmds);
            }

            for (ushort i = 0; i < obj.FieldCount; i++)
            {
                if (obj.IsNthDerivative(i)) continue;
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
            this INoggolloquyObjectSetter obj,
            IEnumerable<KeyValuePair<ushort, object>> fields,
            INoggolloquyObjectGetter def,
            Func<IErrorMask> errorMaskGetter,
            bool skipReadonly,
            NotifyingFireParameters? cmds = null)
        {
            if (!fields.Any()) return;
            try
            {
                HashSet<ushort> readFields = new HashSet<ushort>();
                foreach (var field in fields)
                {
                    readFields.Add(field.Key);
                    if (skipReadonly && obj.IsReadOnly(field.Key)) continue;
                    try
                    {
                        obj.SetNthObject(field.Key, field.Value, cmds);
                    }
                    catch (Exception ex)
                    {
                        errorMaskGetter().SetNthException(field.Key, ex);
                    }
                }

                for (ushort i = 0; i < obj.FieldCount; i++)
                {
                    if (obj.IsNthDerivative(i)) continue;
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
