using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class Accessor
    {
        public string DirectAccess;
        public string PropertyAccess;
        public string PropertyOrDirectAccess => this.PropertyAccess ?? this.DirectAccess;

        public Accessor()
        {
        }

        public Accessor(string direct)
        {
            this.DirectAccess = direct;
        }

        public Accessor(
            TypeGeneration typeGen,
            string accessor,
            bool protectedAccess = false)
        {
            this.DirectAccess = $"{accessor}{(protectedAccess ? typeGen.ProtectedName : typeGen.Name)}";
            if (typeGen.HasProperty)
            {
                this.PropertyAccess = $"{accessor}{(protectedAccess ? typeGen.ProtectedProperty : typeGen.Property)}";
            }
        }
    }
}
