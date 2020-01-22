using Loqui.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public interface ITranslationMask
    {
        TranslationCrystal? GetCrystal();
    }

    public class TranslationMaskStub : ITranslationMask
    {
        public TranslationCrystal? GetCrystal() => null;
    }
}
