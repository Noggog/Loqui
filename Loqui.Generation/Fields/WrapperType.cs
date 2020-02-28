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
            return SubTypeGeneration.TypeName(getter);
        }

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            return this.SubTypeGeneration.GetRequiredNamespaces();
        }

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);

            if (!node.Elements().Any())
            {
                throw new ArgumentException("List had no elements.");
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
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
