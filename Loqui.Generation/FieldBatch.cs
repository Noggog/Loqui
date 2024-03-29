using Noggog;
using System.Xml.Linq;

namespace Loqui.Generation;

public class FieldBatch
{
    protected LoquiGenerator gen;
    public string Name { get; private set; }
    public List<(XElement Node, TypeGeneration TypeGen)> Fields = new List<(XElement, TypeGeneration)>();
    public Dictionary<string, GenericDefinition> Generics = new Dictionary<string, GenericDefinition>();

    public FieldBatch(LoquiGenerator gen)
    {
        this.gen = gen;
    }

    public void Load(XElement node)
    {
        Name = node.GetAttribute("name", throwException: true);
        foreach (var generic in node.Elements(XName.Get("Generic", LoquiGenerator.Namespace)))
        {
            var gen = new GenericDefinition()
            {
                Loqui = generic.GetAttribute<bool>("isLoqui", defaultVal: false)
            };
            var genName = generic.GetAttribute("name");
            foreach (var where in generic.Elements(XName.Get("Where", LoquiGenerator.Namespace)))
            {
                gen.Add(where.Value);
            }
            Generics[genName] = gen;
        }

        XElement fieldsNode = node.Element(XName.Get("Fields", LoquiGenerator.Namespace));
        if (fieldsNode != null)
        {
            foreach (XElement fieldNode in fieldsNode.Elements())
            {
                if (fieldNode.NodeType == System.Xml.XmlNodeType.Comment) continue;

                if (!gen.TryGetTypeGeneration(fieldNode.Name.LocalName, out var typeGen))
                {
                    throw new ArgumentException("Unknown field type: " + fieldNode.Name);
                }
                Fields.Add((fieldNode, typeGen));
            }
        }
    }
}