using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class AttributeDeclaration
    {
        public LoquiInterfaceType[] Types { get; private set; }
        public string Attribute { get; private set; }
        public ObjectGeneration AssociatedObject;

        public AttributeDeclaration(string attributeStr, LoquiInterfaceType[] types)
        {
            Attribute = attributeStr;
            Types = types;
        }
    }

    public class AttributeCollection : IEnumerable<AttributeDeclaration>
    {
        private readonly HashSet<AttributeDeclaration> _attributes = new();

        public void Add(string attributeStr, params LoquiInterfaceType[] types)
        {
            _attributes.Add(new AttributeDeclaration(attributeStr, types));
        }

        public IEnumerable<string> Get(LoquiInterfaceType type)
        {
            return _attributes.Where(x => x.Types.Contains(type)).Select(x => x.Attribute);
        }

        public IEnumerator<AttributeDeclaration> GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
