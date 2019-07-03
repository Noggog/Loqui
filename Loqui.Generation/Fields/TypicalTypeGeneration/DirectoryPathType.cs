using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class DirectoryPathType : StringType
    {
        public override Type Type(bool getter) => typeof(DirectoryPath);
        public override bool IsReference => false;
    }
}
