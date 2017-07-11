using System;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class UnsafeType : PrimitiveType
    {
        private string _typeName;
        public override string TypeName => _typeName;
        public override Type Type => throw new NotImplementedException();

        public override void Load(XElement root, bool requireName = true)
        {
            base.Load(root, requireName);
            var typeNode = root.Element(XName.Get("TargetType", LoquiGenerator.Namespace));
            if (typeNode == null)
            {
                throw new ArgumentException("Needed to define target type.");
            }
            
            _typeName = typeNode.Value;
            if (_typeName == null)
            {
                throw new ArgumentException();
            }
        }

        public override void GenerateForEquals(FileGeneration fg, string rhsAccessor)
        {
            fg.AppendLine($"if (!object.Equals({this.Name}, {rhsAccessor}.{this.Name})) return false;");
        }

        public override void GenerateForEqualsMask(FileGeneration fg, string accessor, string rhsAccessor, string retAccessor)
        {
            if (this.Notifying == NotifyingOption.None)
            {
                fg.AppendLine($"{retAccessor} = object.Equals({accessor}, {rhsAccessor});");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = {accessor}.Equals({rhsAccessor}, (l, r) => object.Equals(l, r));");
            }
        }
    }
}
