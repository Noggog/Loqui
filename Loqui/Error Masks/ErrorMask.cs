using Loqui.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public class ErrorMask : IErrorMask
    {
        public Exception? Overall { get; set; }
        public List<string>? Warnings { get; set; }
        public object[]? Errors { get; set; }
        public readonly ushort IndexSize;

        public ErrorMask(ushort numIndexes)
        {
            this.IndexSize = numIndexes;
        }

        public void SetNthException(int index, Exception ex)
        {
            if (Errors == null)
            {
                this.Errors = new object[IndexSize];
            }
        }

        public void SetNthMask(int index, object maskObj)
        {
            if (Errors == null)
            {
                this.Errors = new object[IndexSize];
            }
        }

        public void ToString(FileGeneration fg, string? name)
        {
            throw new NotImplementedException();
        }

        public static void HandleErrorMask<M, O>(
            Func<M> creator,
            int index,
            O errMaskObj)
            where M : IErrorMask
        {
            if (errMaskObj == null) return;
            var mask = creator();
            mask.SetNthMask(index, errMaskObj);
        }

        public static void HandleErrorMaskAddition<M, O>(
            Func<M> creator,
            int index,
            O errMaskObj)
            where M : IErrorMask
        {
            if (errMaskObj == null) return;
            var mask = creator();
            var nthMask = mask.GetNthMask(index);
            MaskItem<Exception?, IEnumerable<MaskItem<Exception?, O>>>? maskItem = nthMask as MaskItem<Exception?, IEnumerable<MaskItem<Exception?, O>>>;
            ICollection<MaskItem<Exception?, O>>? coll = maskItem?.Specific as ICollection<MaskItem<Exception?, O>>;
            if (maskItem == null)
            {
                coll = new List<MaskItem<Exception?, O>>();
                maskItem = new MaskItem<Exception?, IEnumerable<MaskItem<Exception?, O>>>(null, coll);
                mask.SetNthMask(index, maskItem);
            }
            else if (coll == null)
            {
                coll = new List<MaskItem<Exception?, O>>();
                maskItem.Specific = coll;
            }
            coll.Add(new MaskItem<Exception?, O>(null, errMaskObj));
        }

        public static void HandleErrorMaskAddition<M, O>(
            M mask,
            int index,
            O errMaskObj)
            where M : IErrorMask
        {
            if (errMaskObj == null) return;
            var nthMask = mask.GetNthMask(index);
            MaskItem<Exception?, IEnumerable<MaskItem<Exception?, O>>>? maskItem = nthMask as MaskItem<Exception?, IEnumerable<MaskItem<Exception?, O>>>;
            ICollection<MaskItem<Exception?, O>>? coll = maskItem?.Specific as ICollection<MaskItem<Exception?, O>>;
            if (maskItem == null)
            {
                coll = new List<MaskItem<Exception?, O>>();
                maskItem = new MaskItem<Exception?, IEnumerable<MaskItem<Exception?, O>>>(null, coll);
                mask.SetNthMask(index, maskItem);
            }
            else if (coll == null)
            {
                coll = new List<MaskItem<Exception?, O>>();
                maskItem.Specific = coll;
            }
            coll.Add(new MaskItem<Exception?, O>(null, errMaskObj));
        }
        
        public bool IsInError()
        {
            if (Overall != null) return true;
            if (Errors?.Length > 0) return true;
            return false;
        }

        public object? GetNthMask(int index)
        {
            if (Errors == null) return null;
            return Errors[index];
        }
    }

    public static class ErrorMaskExt
    {
        public static TMask? Combine<TMask>(this TMask? lhs, TMask? rhs)
            where TMask : class, IErrorMask<TMask>
        {
            if (lhs == null) return rhs;
            if (rhs == null) return lhs;
            return lhs.Combine(rhs);
        }
    }
}
