using System;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class UnsafeType : TypicalGeneration
    {
        private string _typeName;
        public override string TypeName => _typeName;

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
    }
}
