using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public interface IMask<T>
    {
        bool AllEqual(Func<T, bool> eval);
    }
}
