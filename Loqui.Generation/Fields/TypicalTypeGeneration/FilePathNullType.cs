using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class FilePathNullType : StringType
    {
        public override Type Type => typeof(FilePath?);
    }
}
