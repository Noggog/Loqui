using System;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public class NonExportedObjectType : TypicalGeneration
    {
        public override bool Derivative
        {
            get
            {
                return true;
            }
        }

        private string _typeName;
        public override string TypeName { get { return _typeName; } }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            _typeName = node.Element(XName.Get("TargetType", NoggolloquyGenerator.Namespace))?.Value;
            if (_typeName == null)
            {
                throw new ArgumentException();
            }
        }
    }
}
