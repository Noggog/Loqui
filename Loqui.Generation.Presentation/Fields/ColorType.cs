using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Loqui.Generation
{
    public class ColorType : PrimitiveType
    {
        public override Type Type => typeof(Color);

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            yield return "System.Windows.Media";
        }
    }
}
