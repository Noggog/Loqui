using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{ 
    public class TranslationGeneration
    {
        public MaskModule MaskModule;
        public virtual bool ShouldGenerateCopyIn(TypeGeneration typeGen) => typeGen.GenerateTypicalItems;
        public virtual bool ShouldGenerateWrite(TypeGeneration typeGen) => !typeGen.Derivative && typeGen.GenerateTypicalItems;
    }
}
