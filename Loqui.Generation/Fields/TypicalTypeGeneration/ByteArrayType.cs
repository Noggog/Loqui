using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class ByteArrayType : ClassType
    {
        public int? Length;
        public override Type Type(bool getter) => getter ? typeof(ReadOnlyMemorySlice<byte>) : typeof(MemorySlice<byte>);
        public override bool IsEnumerable => true;
        public override bool IsReference => true;

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
        {
            if (this.Nullable)
            {
                return $"{(negate ? "!" : null)}{nameof(MemorySliceExt)}.Equal({accessor.Access}, {rhsAccessor.Access})";
            }
            else
            {
                return $"{(negate ? "!" : null)}MemoryExtensions.SequenceEqual({accessor.Access}.Span, {rhsAccessor.Access}.Span)";
            }
        }

        public override string GetNewForNonNullable()
        {
            return $"new byte[{this.Length ?? 0}]";
        }

        public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
        {
            if (!this.IntegrateField) return;
            // ToDo
            // Add Internal interface support
            if (this.InternalGetInterface) return;
            fg.AppendLine($"{fgAccessor}.AppendLine($\"{name} => {{{nameof(SpanExt)}.{nameof(SpanExt.ToHexString)}({accessor.Access})}}\");");
        }

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            this.Length = node.GetAttribute<int?>("byteLength", null);

            if (this.Length != null
                && this.Nullable)
            {
                throw new ArgumentException($"Cannot have a byte array with a length that is nullable.  Doesn't apply. {this.ObjectGen.Name} {this.Name}");
            }
        }
        
        public override void GenerateForHash(FileGeneration fg, Accessor accessor, string hashResultAccessor)
        {
            if (!this.IntegrateField) return;
            if (this.Nullable)
            {
                fg.AppendLine($"if ({accessor} is {{}} {this.Name}Item)");
                accessor = $"{this.Name}Item";
            }
            using (new BraceWrapper(fg, doIt: this.Nullable))
            {
                fg.AppendLine($"{hashResultAccessor}.Add({accessor});");
            }
        }

        public override void GenerateForCopy(
            FileGeneration fg,
            Accessor accessor,
            Accessor rhs, 
            Accessor copyMaskAccessor,
            bool protectedMembers,
            bool deepCopy)
        {
            if (!this.AlwaysCopy)
            {
                fg.AppendLine($"if ({(deepCopy ? this.GetTranslationIfAccessor(copyMaskAccessor) : this.SkipCheck(copyMaskAccessor, deepCopy))})");
            }
            using (new BraceWrapper(fg, doIt: !AlwaysCopy))
            {
                MaskGenerationUtility.WrapErrorFieldIndexPush(
                    fg,
                    () =>
                    {
                        if (this.Nullable)
                        {
                            fg.AppendLine($"if(rhs.{this.Name} is {{}} {this.Name}rhs)");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine($"{accessor.Access} = {this.Name}rhs{(deepCopy ? null : ".Value")}.ToArray();");
                            }
                            fg.AppendLine("else");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine($"{accessor.Access} = default;");
                            }
                        }
                        else
                        {
                            fg.AppendLine($"{accessor.Access} = {rhs}.ToArray();");
                        }
                    },
                    errorMaskAccessor: "errorMask",
                    indexAccessor: this.HasIndex ? this.IndexEnumInt : default(Accessor),
                    doIt: this.CopyNeedsTryCatch);
            }
        }

        public override void GenerateClear(FileGeneration fg, Accessor identifier)
        {
            if (this.ReadOnly || !this.IntegrateField) return;
            // ToDo
            // Add internal interface support
            if (this.InternalSetInterface) return;
            if (!this.Enabled) return;
            if (this.Nullable)
            {
                fg.AppendLine($"{identifier.Access} = default;");
            }
            else if (this.Length.HasValue)
            {
                fg.AppendLine($"{identifier.Access} = new byte[{Length.Value}];");
            }
            else
            {
                fg.AppendLine($"{identifier.Access} = new byte[0];");
            }
        }

        public override string GetDuplicate(Accessor accessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateCopySetToConverter(FileGeneration fg)
        {
            using (new DepthWrapper(fg))
            {
                fg.AppendLine(".Select(b => new MemorySlice<byte>(b.ToArray()))");
            }
        }
    }
}
