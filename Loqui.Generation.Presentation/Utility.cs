using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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
