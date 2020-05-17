using System;
using System.Collections.Generic;
using System.Text;

namespace Loqui.Generation
{
    public class PercentXmlTranslationGeneration : PrimitiveXmlTranslationGeneration<double>
    {
        public override string TypeName(TypeGeneration typeGen)
        {
            return "Percent";
        }
    }
}
