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
        public override Type Type(bool getter) => getter ? typeof(ReadOnlySpan<byte>) : typeof(byte[]);
        public override bool IsEnumerable => true;
        public override bool IsReference => true;
        public override bool CanBeNullable(bool getter) => !getter;

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate = false)
        {
            return $"{(negate ? "!" : null)}MemoryExtensions.SequenceEqual({accessor.DirectAccess}, {rhsAccessor.DirectAccess})";
        }

        public override string GetNewForNonNullable()
        {
            return $"new byte[{this.Length.Value}]";
        }

        public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
        {
            if (!this.IntegrateField) return;
            // ToDo
            // Add Internal interface support
            if (this.InternalGetInterface) return;
            fg.AppendLine($"{fgAccessor}.AppendLine($\"{name} => {{{nameof(SpanExt)}.{nameof(SpanExt.ToHexString)}({accessor.DirectAccess})}}\");");
        }

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            this.Length = node.GetAttribute<int?>("byteLength", null);
            if (this.Length == null
                && this.Singleton != SingletonLevel.None)
            {
                throw new ArgumentException($"Cannot have a byte array with an undefined length that is not nullable. {this.ObjectGen.Name} {this.Name}");
            }

            if (this.Length != null
                && this.Singleton == SingletonLevel.None)
            {
                throw new ArgumentException($"Cannot have a byte array with a length that is nullable.  Doesn't apply. {this.ObjectGen.Name} {this.Name}");
            }
        }
        
        public override void GenerateForHash(FileGeneration fg, Accessor accessor, string hashResultAccessor)
        {
            if (!this.IntegrateField) return;
            if (this.HasBeenSet)
            {
                fg.AppendLine($"if ({accessor}_IsSet)");
            }
            using (new BraceWrapper(fg, doIt: this.HasBeenSet))
            {
                fg.AppendLine($"{hashResultAccessor} = HashHelper.GetHashCode({accessor}).CombineHashCode({hashResultAccessor});");
            }
        }

        public override void GenerateForCopy(
            FileGeneration fg,
            Accessor accessor,
            string rhsAccessorPrefix, 
            string copyMaskAccessor,
            bool protectedMembers,
            bool getter)
        {
            if (this.HasBeenSet)
            {
                fg.AppendLine($"if({this.HasBeenSetAccessor(getter, Accessor.FromType(this, $"{rhsAccessorPrefix}."))})");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{accessor.DirectAccess} = {rhsAccessorPrefix}.{this.Name}{(getter ? null : ".Value")}.ToArray();");
                }
                fg.AppendLine("else");
                using (new BraceWrapper(fg))
                {
                    if (this.HasProperty && this.PrefersProperty)
                    {
                        fg.AppendLine($"{accessor.PropertyAccess}.Unset();");
                    }
                    else
                    {
                        fg.AppendLine($"{accessor.DirectAccess} = default;");
                    }
                }
            }
            else
            {
                fg.AppendLine($"{accessor.DirectAccess} = {rhsAccessorPrefix}.{this.GetName(internalUse: false, property: false)}.ToArray();");
            }
        }

        public override void GenerateClear(FileGeneration fg, Accessor identifier)
        {
            if (this.ReadOnly || !this.IntegrateField) return;
            // ToDo
            // Add internal interface support
            if (this.InternalSetInterface) return;
            if (this.HasBeenSet)
            {
                fg.AppendLine($"{identifier.DirectAccess} = default;");
            }
            else if (this.Length.HasValue)
            {
                fg.AppendLine($"{identifier.DirectAccess} = new byte[{Length.Value}];");
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override string GetDuplicate(Accessor accessor)
        {
            throw new NotImplementedException();
        }
    }
}
