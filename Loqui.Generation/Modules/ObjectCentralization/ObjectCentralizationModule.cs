using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class ObjectCentralizationModule : GenerationModule
    {
        public override IEnumerable<string> RequiredUsingStatements(ObjectGeneration obj)
        {
            foreach (var e in base.RequiredUsingStatements(obj))
            {
                yield return e;
            }
            yield return "Loqui.Internal";
            yield return "System.Collections.Specialized";
        }

        public Dictionary<string, List<TypeGeneration>> GetContainedTypes(ObjectGeneration obj)
        {
            Dictionary<string, List<TypeGeneration>> containedTypes = new Dictionary<string, List<TypeGeneration>>();

            foreach (var field in obj.IterateFields())
            {
                if (field.Notifying != NotifyingType.ObjectCentralized) continue;
                if (field.IsEnumerable) continue;
                containedTypes.TryCreateValue(field.TypeName).Add(field);
            }
            return containedTypes;
        }

        public override IEnumerable<string> Interfaces(ObjectGeneration obj)
        {
            foreach (var ret in base.Interfaces(obj))
            {
                yield return ret;
            }
            foreach (var type in GetContainedTypes(obj))
            {
                yield return $"IPropertySupporter<{type.Key}>";
            }
        }

        public async override Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            if (!GetContainedTypes(obj).Any()) return;
            fg.AppendLine($"protected BitArray _hasBeenSetTracker = new BitArray({obj.IterateFields().Count()});");
            foreach (var type in GetContainedTypes(obj))
            {
                using (new RegionWrapper(fg, $"IPropertySupporter {type}")) ;
                {
                    GenerateForType(fg, obj, type.Key, type.Value);
                }
            }
        }

        private static void GenerateForType(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            if (!obj.BaseClassTrail().Any((b) => b.IterateFields().Any((f) => f.TypeName.Equals(type))))
            {
                fg.AppendLine($"protected ObjectCentralizationSubscriptions<{type}> _{Utility.MemberNameSafety(type)}_subscriptions;");
            }
            GenerateGet(fg, obj, type, fields);
            GenerateSet(fg, obj, type, fields);
            GenerateGetHasBeenSet(fg, obj, type, fields);
            GenerateSetHasBeenSet(fg, obj, type, fields);
            GenerateUnset(fg, obj, type, fields);
            GenerateUnsubscribe(fg, obj, type, fields);
            GenerateSetCurrentAsDefault(fg, obj, type, fields);
            GenerateGetDefault(fg, obj, type, fields);
        }

        private static void GenerateGet(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"{type} IPropertySupporter<{type}>.Get"))
            {
                args.Add("int index");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"switch (({obj.FieldIndexName})index)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in fields)
                    {
                        fg.AppendLine($"case {field.IndexEnumName}:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"return {field.Name};");
                        }
                    }
                    fg.AppendLine("default:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"throw new ArgumentException($\"Unknown index for field type {type}: {{index}}\");");
                    }
                }
            }
        }

        private static void GenerateGetHasBeenSet(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"bool IPropertySupporter<{type}>.GetHasBeenSet"))
            {
                args.Add("int index");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"switch (({obj.FieldIndexName})index)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in fields)
                    {
                        fg.AppendLine($"case {field.IndexEnumName}:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine("return _hasBeenSetTracker[index];");
                        }
                    }
                    fg.AppendLine("default:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"throw new ArgumentException($\"Unknown index for field type {type}: {{index}}\");");
                    }
                }
            }
        }

        private static void GenerateSetHasBeenSet(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"void IPropertySupporter<{type}>.SetHasBeenSet"))
            {
                args.Add("int index");
                args.Add("bool on");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"switch (({obj.FieldIndexName})index)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in fields)
                    {
                        fg.AppendLine($"case {field.IndexEnumName}:");
                    }
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine("_hasBeenSetTracker[index] = on;");
                        fg.AppendLine($"break;");
                    }
                    fg.AppendLine("default:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"throw new ArgumentException($\"Unknown index for field type {type}: {{index}}\");");
                    }
                }
            }
        }

        private static void GenerateSet(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"void IPropertySupporter<{type}>.Set"))
            {
                args.Add("int index");
                args.Add($"{type} item");
                args.Add($"bool hasBeenSet");
                args.Add($"NotifyingFireParameters cmds");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"switch (({obj.FieldIndexName})index)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in fields)
                    {
                        fg.AppendLine($"case {field.IndexEnumName}:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"Set{field.Name}(item, hasBeenSet, cmds);");
                            fg.AppendLine($"break;");
                        }
                    }
                    fg.AppendLine("default:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"throw new ArgumentException($\"Unknown index for field type {type}: {{index}}\");");
                    }
                }
            }
        }

        private static void GenerateUnset(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"void IPropertySupporter<{type}>.Unset"))
            {
                args.Add("int index");
                args.Add("NotifyingUnsetParameters cmds");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"switch (({obj.FieldIndexName})index)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in fields)
                    {
                        fg.AppendLine($"case {field.IndexEnumName}:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine("_hasBeenSetTracker[index] = false;");
                            fg.AppendLine($"{field.Name} = default({type});");
                            fg.AppendLine("break;");
                        }
                    }
                    fg.AppendLine("default:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"throw new ArgumentException($\"Unknown index for field type {type}: {{index}}\");");
                    }
                }
            }
        }

        private static void GenerateUnsubscribe(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"void IPropertySupporter<{type}>.Unsubscribe"))
            {
                args.Add("int index");
                args.Add("object owner");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"_{Utility.MemberNameSafety(type)}_subscriptions.Unsubscribe(index, owner);");
            }
        }

        private static void GenerateSetCurrentAsDefault(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"void IPropertySupporter<{type}>.SetCurrentAsDefault"))
            {
                args.Add("int index");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"throw new NotImplementedException();");
            }
        }

        private static void GenerateGetDefault(
            FileGeneration fg,
            ObjectGeneration obj,
            string type,
            IEnumerable<TypeGeneration> fields)
        {
            using (var args = new FunctionWrapper(fg,
                $"{type} IPropertySupporter<{type}>.DefaultValue"))
            {
                args.Add("int index");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"switch (({obj.FieldIndexName})index)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in fields.Where((f) => !f.HasDefault))
                    {
                        fg.AppendLine($"case {field.IndexEnumName}:");
                    }
                    if (fields.Any((f) => !f.HasDefault))
                    {
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"return default({type});");
                        }
                    }
                    foreach (var field in fields.Where((f) => f.HasDefault))
                    {
                        fg.AppendLine($"case {field.IndexEnumName}:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"return _{field.Name}_Default;");
                        }
                    }
                    fg.AppendLine("default:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"throw new ArgumentException($\"Unknown index for field type {type}: {{index}}\");");
                    }
                }
            }
        }
    }
}
