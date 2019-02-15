using CSharpExt.Rx;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Loqui
{
    public class MaskItem<TOverall, TSpecific> : IEquatable<MaskItem<TOverall, TSpecific>>
    {
        public TOverall Overall;
        public TSpecific Specific;

        public MaskItem()
        {
        }

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
            return object.Equals(this.Overall, other.Overall)
                && object.Equals(this.Specific, other.Specific);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MaskItem<TOverall, TSpecific>)) return false;
            return Equals((MaskItem<TOverall, TSpecific>)obj);
        }

        public override int GetHashCode()
        {
            return HashHelper.GetHashCode(this.Overall)
                .CombineHashCode(this.Specific);
        }

        public static MaskItem<TOverall, TSpecific> WrapValue(TSpecific val)
        {
            if (val == null) return null;
            return new MaskItem<TOverall, TSpecific>(default(TOverall), val);
        }
    }

    public class MaskItemIndexed<TOverall, TSpecific> : MaskItem<TOverall, TSpecific>, IEquatable<MaskItemIndexed<TOverall, TSpecific>>
    {
        public int Index;

        public MaskItemIndexed(int index)
            : base()
        {
            this.Index = index;
        }

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
            return HashHelper.CombineHashCode(
                base.GetHashCode(),
                this.Index.GetHashCode());
        }
    }

    public class MaskItemIndexed<TKey, TOverall, TSpecific> : MaskItem<TOverall, TSpecific>, IEquatable<MaskItemIndexed<TKey, TOverall, TSpecific>>
    {
        public TKey Index;

        public MaskItemIndexed(TKey index)
            : base()
        {
            this.Index = index;
        }

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
            return HashHelper.CombineHashCode(
                base.GetHashCode(),
                HashHelper.GetHashCode(this.Index));
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
                    printable.ToString(fg);
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
                    printable.ToString(fg);
                }
                else
                {
                    fg.AppendLine(maskItem.Specific.ToString());
                }
            }
        }

        public static MaskItem<Exception, TRet> Bubble<TSource, TRet>(this MaskItem<Exception, TSource> item)
            where TSource : TRet
        {
            if (item == null) return null;
            return new MaskItem<Exception, TRet>(item.Overall, item.Specific);
        }

        public static MaskItem<bool, M> Factory<M>(M mask, EqualsMaskHelper.Include include)
            where M : IMask<bool>
        {
            var allEq = mask.AllEqual(b => b);
            if (allEq)
            {
                switch (include)
                {
                    case EqualsMaskHelper.Include.All:
                        return new MaskItem<bool, M>()
                        {
                            Overall = false,
                            Specific = mask,
                        };
                    case EqualsMaskHelper.Include.OnlyFailures:
                        return null;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                return new MaskItem<bool, M>()
                {
                    Overall = allEq,
                    Specific = mask
                };
            }
        }
    }
}
