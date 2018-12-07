using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation.Presentation
{
    public static class Utility
    {
        public static void AddToLoquiGenerator(LoquiGenerator gen)
        {
            gen.AddTypeAssociation<ColorType>("Color");
            gen.XmlTranslation.AddTypeAssociation<ColorType>(new PrimitiveXmlTranslationGeneration<Color>());
        }
    }
}
