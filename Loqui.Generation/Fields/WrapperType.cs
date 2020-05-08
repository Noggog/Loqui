using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class WrapperType : TypeGeneration
    {
        protected bool isLoquiSingle;
        public TypeGeneration SubTypeGeneration;

        public virtual string ItemTypeName(bool getter)
        {
            return SubTypeGeneration.TypeName(getter, needsCovariance: true);
        }

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            return this.SubTypeGeneration.GetRequiredNamespaces();
        }

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);

            var fieldsNode = node.Elements().FirstOrDefault(f => f.Name.LocalName.Equals("Fields"));
            if (fieldsNode != null)
            {
                node = fieldsNode;
            }

            if (!node.Elements().Any())
            {
                throw new ArgumentException("Wrapper had no elements.");
            }
            if (node.Elements().Any())
            {
                var typeGen = await ObjectGen.LoadField(
                    node.Elements().First(),
                    requireName: false,
                    setDefaults: false);
                if (typeGen.Succeeded)
                {
                    this.SubTypeGeneration = typeGen.Value;
                    isLoquiSingle = this.SubTypeGeneration as LoquiType != null;
                }
                else
                {
                    throw new NotImplementedException();
                }
                this.SubTypeGeneration.Parent = this;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
