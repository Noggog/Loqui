using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noggolloquy
{
    public interface IErrorMask
    {
        Exception Overall { get; set; }
        List<string> Warnings { get; }
        void SetNthException(ushort index, Exception ex);
        void SetNthMask(ushort index, object maskObj);
    }
}
