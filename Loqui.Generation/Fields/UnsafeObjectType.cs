using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class UnsafeType : PrimitiveType
    {
        private string _typeName;
        public override string TypeName(bool getter = false, bool needsCovariance = false) => _typeName;
        public override Type Type(bool getter) => throw new NotImplementedException();

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

        public override string GenerateEqualsSnippet(Accessor accessor, Accessor rhsAccessor, bool negate)
        {
            return $"{(negate ? "!" : null)}object.Equals({accessor.DirectAccess}, {rhsAccessor.DirectAccess})";
        }
    }
}
