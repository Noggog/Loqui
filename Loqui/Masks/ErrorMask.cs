using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public class ErrorMask : IErrorMask
    {
        public Exception Overall { get; set; }
        public List<string> Warnings { get; set; }
        public object[] Errors { get; set; }
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

        public void ToString(FileGeneration fg)
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

        public static void HandleException<M>(
            Func<M> creator,
            int index,
            Exception ex)
            where M : IErrorMask
        {
            if (ex == null) return;
            if (creator == null)
            {
                throw ex;
            }
            var mask = creator();
            mask.SetNthException(index, ex);
        }

        public bool IsInError()
        {
            if (Overall != null) return true;
            if (Errors.Length > 0) return true;
            return false;
        }
    }
}
