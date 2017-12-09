using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class UnsafeType : PrimitiveType
    {
        private string _typeName;
        public override string TypeName => _typeName;
        public override Type Type => throw new NotImplementedException();

        public override async Task Load(XElement root, bool requireName = true)
        {
            await base.Load(root, requireName);
            var typeNode = root.Element(XName.Get(Constants.TARGET_TYPE, LoquiGenerator.Namespace));
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

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            if (this.Bare)
            {
                fg.AppendLine($"{retAccessor} = object.Equals({accessor.DirectAccess}, {rhsAccessor.DirectAccess});");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = {accessor.PropertyAccess}.Equals({rhsAccessor.PropertyAccess}, (l, r) => object.Equals(l, r));");
            }
        }
    }
}
