using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public abstract class XmlTranslationGeneration
    {
        public abstract bool OutputsErrorMask { get; }
        public abstract void GenerateWrite(
            FileGeneration fg, 
            TypeGeneration typeGen, 
            string writerAccessor, 
            string itemAccessor, 
            string maskAccessor,
            string nameAccessor);
    }
}
