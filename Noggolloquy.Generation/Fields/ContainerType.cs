using System;
using System.Linq;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public abstract class ContainerType : TypeGeneration
    {
        protected bool singleType;
        public TypeGeneration SingleTypeGen;
        protected bool isLevSingle;
        protected LevType LevTypeSingleton { get { return SingleTypeGen as LevType; } }

        public override string Property { get { return $"{this.Name}"; } }
        public override string ProtectedName { get { return $"{this.ProtectedProperty}"; } }

        public override bool Imports
        {
            get
            {
                if (!base.Imports) return false;
                if (!SingleTypeGen.Imports) return false;
                return true;
            }
        }

        public virtual string ItemTypeName
        {
            get
            {
                if (singleType)
                {
                    return SingleTypeGen.TypeName;
                }
                throw new NotImplementedException();
            }
        }

        public TypeGeneration SubTypeGeneration { get { return SingleTypeGen; } }

        public string GetterTypeName
        {
            get { return (this.isLevSingle ? LevTypeSingleton.Getter : ItemTypeName); }
        }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);

            if (!node.Elements().Any())
            {
                throw new ArgumentException("List had no elements.");
            }

            if (node.Elements().Any()
                && ObjectGen.LoadField(
                    node.Elements().First(),
                    false,
                    out SingleTypeGen))
            {
                singleType = true;
                isLevSingle = SingleTypeGen as LevType != null;
            }
            else
            {
                throw new NotImplementedException();
            }

            if (this.Derivative)
            {
                throw new NotImplementedException();
            }
        }

        public override void SetMaskException(FileGeneration fg, string errorMaskAccessor, string exception)
        {
            fg.AppendLine($"{errorMaskAccessor} = {exception};");
        }

        public void AddMaskException(FileGeneration fg, string errorMaskAccessor, string exception)
        {
            fg.AppendLine($"{errorMaskAccessor}?.{this.Name}.Value.Add({exception});");
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier, bool internalUse)
        {
            fg.AppendLine(identifier + "." + this.GetPropertyString(internalUse) + ".Unset();");
        }

        public override string GetPropertyString(bool internalUse)
        {
            if (internalUse)
            {
                return "_" + this.Name;
            }
            else
            {
                return this.Name;
            }
        }

        public override string GenerateACopy(string rhsAccessor)
        {
            throw new NotImplementedException();
        }
    }
}
