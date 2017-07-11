using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class ByteArrayType : PrimitiveType
    {
        public override Type Type => typeof(byte[]);

        public override void GenerateForEquals(FileGeneration fg, string rhsAccessor)
        {
            fg.AppendLine($"if (!{this.Name}.EqualsFast({rhsAccessor}.{this.Name})) return false;");
        }
        
        public override void GenerateForEqualsMask(FileGeneration fg, string accessor, string rhsAccessor, string retAccessor)
        {
            if (this.Notifying == NotifyingOption.None)
            {
                fg.AppendLine($"{retAccessor} = {accessor}.EqualsFast({rhsAccessor});");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = {accessor}.Equals({rhsAccessor}, (l, r) => l.EqualsFast(r));");
            }
        }
    }
}
