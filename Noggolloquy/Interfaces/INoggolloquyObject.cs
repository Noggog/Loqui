using Noggog;
using Noggog.Printing;
using System;
using System.Collections;

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

        Type GetNthType(ushort index);

        string GetNthName(ushort index);

        bool GetNthIsNoggolloquy(ushort index);

        bool GetNthIsEnumerable(ushort index);

        bool IsNthDerivative(ushort index);

        ushort? GetNameIndex(StringCaseAgnostic name);

        bool IsReadOnly(ushort index);
    }

    public interface INoggolloquyClass<L, G> : ICopyFrom<L, G>, IClearable
        where L : class, G
        where G : class
    {
    }
    
    public static class INoggolloquyObjectGetterExt
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

        private static string PrintLevName(INoggolloquyObjectGetter obj, ushort i)
        {
            return obj.GetNthName(i) + "(" + obj.GetNthType(i).Name + ")" + " => ";
        }

        private static string PrintPrettyInternal(this INoggolloquyObjectGetter lev, DepthPrinter depthPrinter)
        {
            depthPrinter.AddLine("[");
            using (depthPrinter.IncrementDepth())
            {
                for (ushort i = 0; i < lev.FieldCount; i++)
                {
                    var obj = lev.GetNthObject(i);
                    if (lev.GetNthIsEnumerable(i))
                    {
                        var listObj = obj as IEnumerable;
                        if (listObj != null)
                        {
                            bool hasItems = listObj.Any();
                            depthPrinter.AddLine(lev.GetNthName(i) + " => " + (hasItems ? string.Empty : "[ ]"));
                            if (hasItems)
                            {
                                depthPrinter.AddLine("[");
                                using (depthPrinter.IncrementDepth())
                                {
                                    foreach (var listItem in listObj)
                                    {
                                        if (lev.GetNthIsNoggolloquy(i))
                                        {
                                            INoggolloquyObjectGetter subLev = listItem as INoggolloquyObjectGetter;
                                            if (subLev != null)
                                            {
                                                depthPrinter.AddLine(PrintLevName(lev, i));
                                                PrintPrettyInternal(subLev, depthPrinter);
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

                    if (lev.GetNthIsNoggolloquy(i))
                    {
                        INoggolloquyObjectGetter subLev = obj as INoggolloquyObjectGetter;
                        if (subLev != null)
                        {
                            depthPrinter.AddLine(PrintLevName(lev, i));
                            PrintPrettyInternal(subLev, depthPrinter);
                        }
                        continue;
                    }

                    depthPrinter.AddLine(lev.GetNthName(i) + ": " + obj.ToStringSafe());
                }
            }
            depthPrinter.AddLine("]");
            return depthPrinter.ToString();
        }

        private static void PrintItem(INoggolloquyObjectGetter lev, ushort i, object o, DepthPrinter dp)
        {
        }
    }
}
