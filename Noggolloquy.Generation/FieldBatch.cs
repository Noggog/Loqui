using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public class FieldBatch
    {
        protected NoggolloquyGenerator gen;
        public string Name { get; private set; }
        public List<(XElement Node, TypeGeneration TypeGen)> Fields = new List<(XElement, TypeGeneration)>();
        public Dictionary<string, GenericDefinition> Generics = new Dictionary<string, GenericDefinition>();

        public FieldBatch(NoggolloquyGenerator gen)
        {
            this.gen = gen;
        }

        public void Load(XElement node)
        {
            this.Name = node.GetAttribute("name", throwException: true);
            foreach (var generic in node.Elements(XName.Get("Generic", NoggolloquyGenerator.Namespace)))
            {
                var gen = new GenericDefinition();
                var genName = generic.GetAttribute("name");
                foreach (var where in generic.Elements(XName.Get("Where", NoggolloquyGenerator.Namespace)))
                {
                    gen.Add(where.Value);
                }
                this.Generics[genName] = gen;
            }

            XElement fieldsNode = node.Element(XName.Get("Fields", NoggolloquyGenerator.Namespace));
            if (fieldsNode != null)
            {
                foreach (XElement fieldNode in fieldsNode.Elements())
                {
                    if (fieldNode.NodeType == System.Xml.XmlNodeType.Comment) continue;

                    if (!gen.TryGetTypeGeneration(fieldNode.Name.LocalName, out var typeGen))
                    {
                        throw new ArgumentException("Unknown field type: " + fieldNode.Name);
                    }
                    this.Fields.Add((fieldNode, typeGen));
                }
            }
        }
    }
}
