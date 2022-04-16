using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Noggog;

namespace Loqui.Generation
{
    public class ArrayType : ListType
    {
        public override bool CopyNeedsTryCatch => false;
        public override bool IsClass => false;
        public override bool HasDefault => true;
        public override string TypeName(bool getter, bool needsCovariance = false)
        {
            if (getter)
            {
                return $"ReadOnlyMemorySlice<{this.ItemTypeName(getter)}>{this.NullChar}";
            }
            else
            {
                return $"{this.ItemTypeName(getter)}[]";
            }
        }

        public int? FixedSize;

        public override async Task Load(XElement node, bool requireName = true)
        {
            this.FixedSize = node.GetAttribute(Constants.FIXED_SIZE, default(int?));
            await base.Load(node, requireName);
        }

        protected override string GetActualItemClass(bool ctor = false)
        {
            if (this.NotifyingType == NotifyingType.ReactiveUI)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (!ctor)
                {
                    return $"{this.ItemTypeName(getter: false)}[]";
                }
                if (this.Nullable)
                {
                    return $"default";
                }
                else if (this.FixedSize.HasValue)
                {
                    if (this.SubTypeGeneration is LoquiType loqui)
                    {
                        return $"ArrayExt.Create({this.FixedSize}, (i) => new {loqui.TargetObjectGeneration.ObjectName}())";
                    }
                    else if (this.SubTypeGeneration.IsNullable
                        || this.SubTypeGeneration.CanBeNullable(getter: false))
                    {
                        return $"new {this.ItemTypeName(getter: false)}[{this.FixedSize}]";
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    return $"new {this.ItemTypeName(getter: false)}[0]";
                }
            }
        }

        public override string ListTypeName(bool getter, bool internalInterface)
        {
            string itemTypeName = this.ItemTypeName(getter: getter);
            if (this.SubTypeGeneration is LoquiType loqui)
            {
                itemTypeName = loqui.TypeNameInternal(getter: getter, internalInterface: internalInterface);
            }
            if (getter)
            {
                return $"ReadOnlyMemorySlice<{itemTypeName}{this.SubTypeGeneration.NullChar}>";
            }
            else
            {
                return $"{itemTypeName}{this.SubTypeGeneration.NullChar}[]";
            }
        }

        public override void GenerateClear(FileGeneration fg, Accessor accessorPrefix)
        {
            if (this.Nullable)
            {
                fg.AppendLine($"{accessorPrefix} = null;");
            }
            else
            {
                if (this.FixedSize.HasValue)
                {
                    if (this.SubTypeGeneration.IsNullable
                        && this.SubTypeGeneration.Nullable)
                    {
                        fg.AppendLine($"{accessorPrefix.Access}.ResetToNull();");
                    }
                    else if (this.SubTypeGeneration is StringType)
                    {
                        fg.AppendLine($"Array.Fill({accessorPrefix.Access}, string.Empty);");
                    }
                    else if (this.SubTypeGeneration is LoquiType loqui)
                    {
                        fg.AppendLine($"{accessorPrefix.Access}.Fill(() => new {loqui.TargetObjectGeneration.ObjectName}());");
                    }
                    else
                    {
                        fg.AppendLine($"{accessorPrefix.Access}.Reset();");
                    }
                }
                else
                {
                    fg.AppendLine($"{accessorPrefix.Access}.Clear();");
                }
            }
        }

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
        {
            fg.AppendLine($"if ({this.GetTranslationIfAccessor(maskAccessor)})");
            using (new BraceWrapper(fg))
            {
                if (this.SubTypeGeneration.IsIEquatable)
                {
                    if (this.Nullable)
                    {
                        fg.AppendLine($"if (!{nameof(ObjectExt)}.{nameof(ObjectExt.NullSame)}({accessor}, {rhsAccessor})) return false;");
                    }
                    fg.AppendLine($"if (!MemoryExtensions.SequenceEqual<{SubTypeGeneration.TypeName(getter: true)}>({accessor.Access}{(Nullable ? "!.Value" : null)}.Span!, {rhsAccessor.Access}{(Nullable ? "!.Value" : null)}.Span!)) return false;");
                }
                else
                {
                    fg.AppendLine($"if (!{accessor}.{nameof(EnumerableExt.SequenceEqualNullable)}({rhsAccessor})) return false;");
                }
            }
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            string funcStr;
            var loqui = this.SubTypeGeneration as LoquiType;
            if (loqui != null)
            {
                funcStr = $"(loqLhs, loqRhs) => loqLhs.{(loqui.TargetObjectGeneration == null ? nameof(IEqualsMask.GetEqualsMask) : "GetEqualsMask")}(loqRhs, include)";
            }
            else
            {
                funcStr = $"(l, r) => {this.SubTypeGeneration.GenerateEqualsSnippet(new Accessor("l"), new Accessor("r"))}";
            }
            using (var args = new ArgsWrapper(fg,
                $"ret.{this.Name} = {nameof(EqualsMaskHelper)}.SpanEqualsHelper<{SubTypeGeneration.TypeName(getter: true)}{SubTypeGeneration.NullChar}{(loqui == null ? null : $", {loqui.GetMaskString("bool")}")}>"))
            {
                args.Add($"item.{this.Name}");
                args.Add($"rhs.{this.Name}");
                args.Add(funcStr);
                args.Add($"include");
            }
        }

        public override void GenerateForCopy(FileGeneration fg, Accessor accessor, Accessor rhs, Accessor copyMaskAccessor, bool protectedMembers, bool deepCopy)
        {
            if (FixedSize.HasValue && SubTypeGeneration is not LoquiType)
            {
                if (!this.AlwaysCopy)
                {
                    fg.AppendLine($"if ({(deepCopy ? this.GetTranslationIfAccessor(copyMaskAccessor) : this.SkipCheck(copyMaskAccessor, deepCopy))})");
                }
                using (new BraceWrapper(fg, doIt: !AlwaysCopy))
                {
                    if (Nullable)
                    {
                        fg.AppendLine($"{accessor} = {rhs}?.ToArray();");
                    }
                    else
                    {
                        fg.AppendLine($"{rhs}.Span.CopyTo({accessor}.AsSpan());");
                    }
                }
            }
            else
            {
                base.GenerateForCopy(fg, accessor, rhs, copyMaskAccessor, protectedMembers, deepCopy);
            }
        }

        public override void WrapSet(FileGeneration fg, Accessor accessor, Action<FileGeneration> a)
        {
            if (FixedSize.HasValue && SubTypeGeneration is not LoquiType)
            {
                fg.AppendLine($"{accessor} = ");
                using (new DepthWrapper(fg))
                {
                    a(fg);
                    fg.AppendLine($"{NullChar}.ToArray();");
                }
            }
            else
            {
                base.WrapSet(fg, accessor, a);
            }
        }
    }
}
