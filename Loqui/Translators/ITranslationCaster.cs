using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Translators
{
    public interface ITranslationCaster<T>
    {
        ITranslation<T> Source { get; }
    }
}
