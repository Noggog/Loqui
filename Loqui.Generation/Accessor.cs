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
        public bool DirectIsAssignment = true;

        public Accessor()
        {
        }

        public Accessor(string direct)
        {
            this.DirectAccess = direct;
        }

        public string Assign(string rhs)
        {
            return $"{DirectAccess}{AssignmentOperator}{rhs}";
        }

        public string AssignmentOperator => DirectIsAssignment ? " = " : ": ";

        public static Accessor ConstructorParam(string path)
        {
            return new Accessor(path)
            {
                DirectIsAssignment = false,
            };
        }

        public static Accessor FromType(
            TypeGeneration typeGen,
            string accessor,
            bool protectedAccess = false,
            bool nullable = false)
        {
            string process(string name)
            {
                if (string.IsNullOrWhiteSpace(name)) return accessor;
                return $"{accessor}{(nullable ? "?" : null)}.{name}";
            }
            Accessor ret = new Accessor();
            ret.DirectAccess = process(protectedAccess ? typeGen.ProtectedName : typeGen.Name);
            if (typeGen.HasProperty)
            {
                ret.PropertyAccess = process(protectedAccess ? typeGen.ProtectedProperty : typeGen.Property);
            }
            return ret;
        }

        public override string ToString()
        {
            return this.DirectAccess;
        }

        public static implicit operator Accessor(string str)
        {
            if (str == null) return null;
            return new Accessor(str);
        }
    }
}
