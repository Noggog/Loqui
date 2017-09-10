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
        void SetNthException(ushort index, Exception ex);
        void SetNthMask(ushort index, object maskObj);
    }
}
