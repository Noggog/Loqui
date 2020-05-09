using Noggog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Loqui
{
    public class MaskItem<TOverall, TSpecific> : IEquatable<MaskItem<TOverall, TSpecific>>
    {
        public TOverall Overall;
        public TSpecific Specific;

        [DebuggerStepThrough]
        public MaskItem(
            TOverall overall,
            TSpecific specific)
        {
            this.Overall = overall;
            this.Specific = specific;
        }

        public bool Equals(MaskItem<TOverall, TSpecific> other)
        {
            return EqualityComparer<TOverall>.Default.Equals(this.Overall, other.Overall)
                && EqualityComparer<TSpecific>.Default.Equals(this.Specific, other.Specific);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MaskItem<TOverall, TSpecific>)) return false;
            return Equals((MaskItem<TOverall, TSpecific>)obj);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Overall);
            hash.Add(Specific);
            return hash.ToHashCode();
        }
    }

    public class MaskItemIndexed<TOverall, TSpecific> : MaskItem<TOverall, TSpecific>, IEquatable<MaskItemIndexed<TOverall, TSpecific>>
    {
        public int Index;

        [DebuggerStepThrough]
        public MaskItemIndexed(
            int index,
            TOverall overall,
            TSpecific specific)
            : base(overall, specific)
        {
            this.Index = index;
        }

        public bool Equals(MaskItemIndexed<TOverall, TSpecific> other)
        {
            if (!base.Equals(other)) return false;
            return this.Index == other.Index;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(this.Index);
            return hash.ToHashCode();
        }
    }

    public class MaskItemIndexed<TKey, TOverall, TSpecific> : MaskItem<TOverall, TSpecific>, IEquatable<MaskItemIndexed<TKey, TOverall, TSpecific>>
    {
        public TKey Index;

        [DebuggerStepThrough]
        public MaskItemIndexed(
            TKey index,
            TOverall overall,
            TSpecific specific)
            : base(overall, specific)
        {
            this.Index = index;
        }

        public bool Equals(MaskItemIndexed<TKey, TOverall, TSpecific> other)
        {
            if (!base.Equals(other)) return false;
            return object.Equals(this.Index, other.Index);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(this.Index);
            return hash.ToHashCode();
        }
    }

    public static class MaskItemExt
    {
        public static void ToString<TOverall, TSpecific>(this MaskItem<TOverall, TSpecific> maskItem, FileGeneration fg)
        {
            if (maskItem == null) return;
            if (maskItem.Overall != null)
            {
                if (maskItem.Overall is IPrintable printable)
                {
                    printable.ToString(fg, null);
                }
                else
                {
                    fg.AppendLine(maskItem.Overall.ToString());
                }
            }
            if (maskItem.Specific != null)
            {
                if (maskItem.Specific is IPrintable printable)
                {
                    printable.ToString(fg, null);
                }
                else
                {
                    fg.AppendLine(maskItem.Specific.ToString());
                }
            }
        }

        [return: NotNullIfNotNull("item")]
        public static MaskItem<Exception, TRet>? Bubble<TSource, TRet>(this MaskItem<Exception, TSource> item)
            where TSource : TRet
        {
            if (item == null) return null;
            return new MaskItem<Exception, TRet>(item.Overall, item.Specific);
        }

        public static MaskItem<bool, M?>? Factory<M>(M mask, EqualsMaskHelper.Include include)
            where M : class, IMask<bool>
        {
            var allEq = mask.All(b => b);
            if (allEq)
            {
                switch (include)
                {
                    case EqualsMaskHelper.Include.All:
                        return new MaskItem<bool, M?>(overall: true, specific: mask);
                    case EqualsMaskHelper.Include.OnlyFailures:
                        return null;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                return new MaskItem<bool, M?>(overall: allEq, specific: mask);
            }
        }

        public static MaskItem<Exception?, TMask?>? Combine<TMask>(this MaskItem<Exception?, TMask?>? lhs, MaskItem<Exception?, TMask?>? rhs, Func<TMask, TMask, TMask> combiner)
            where TMask : class
        {
            if (rhs == null) return lhs;
            if (lhs == null) return rhs;
            var overall = ExceptionExt.Combine(lhs.Overall, rhs.Overall);
            TMask? specific;
            if (lhs.Specific == null)
            {
                specific = rhs.Specific;
            }
            else if (rhs.Specific == null)
            {
                specific = lhs.Specific;
            }
            else
            {
                specific = combiner(lhs.Specific, rhs.Specific);
            }
            if (overall == null && specific == null) return null;
            return new MaskItem<Exception?, TMask?>(overall, specific);
        }
    }
}
