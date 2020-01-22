using Loqui.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public interface IErrorMask : IPrintable
    {
        Exception Overall { get; set; }
        List<string> Warnings { get; }
        void SetNthException(int index, Exception ex);
        void SetNthMask(int index, object maskObj);
        object? GetNthMask(int index);
        bool IsInError();
    }

    public interface IErrorMask<M> : IErrorMask
        where M : IErrorMask<M>
    {
        M Combine(M rhs);
    }
}
