using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class ContainerType : TypeGeneration
    {
        protected bool singleType;
        public TypeGeneration SingleTypeGen;
        protected bool isLoquiSingle;
        protected LoquiType LoquiTypeSingleton => SingleTypeGen as LoquiType;

        public override string Property => $"{this.Name}";
        public override string ProtectedName => $"{this.ProtectedProperty}";

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

        public TypeGeneration SubTypeGeneration => SingleTypeGen;

        public string GetterTypeName => (this.isLoquiSingle ? LoquiTypeSingleton.TypeName : ItemTypeName);

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
                    SingleTypeGen = typeGen.Value;
                    singleType = true;
                    isLoquiSingle = SingleTypeGen as LoquiType != null;
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

            if (SingleTypeGen is ContainerType
                || SingleTypeGen is DictType)
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            return this.SingleTypeGen.GetRequiredNamespaces();
        }

        public void AddMaskException(FileGeneration fg, string errorMaskAccessor, string exception)
        {
            fg.AppendLine($"{errorMaskAccessor}?.{this.Name}.Specific.Value.Add({exception});");
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier)
        {
            if (!this.Protected)
            {
                fg.AppendLine($"{identifier}.{this.GetName(internalUse: false)}.HasBeenSet = {onIdentifier};");
            }
            fg.AppendLine("break;");
        }

        public override void GenerateUnsetNth(FileGeneration fg, string identifier, string cmdsAccessor)
        {
            if (!this.Protected)
            {
                fg.AppendLine($"{identifier}.{this.GetName(false)}.Unset({cmdsAccessor});");
            }
            fg.AppendLine("break;");
        }

        public override string GetName(bool internalUse, bool property = true)
        {
            if (internalUse)
            {
                return $"_{this.Name}";
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

        public override void GenerateForEquals(FileGeneration fg, string rhsAccessor)
        {
            fg.AppendLine($"if (!{this.Name}.SequenceEqual({rhsAccessor}.{this.Name})) return false;");
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            if (this.Bare)
            {
                this.GenerateForEqualsMaskCheck(fg, $"item.{this.Name}", $"rhs.{this.Name}", $"ret.{this.Name}");
            }
            else
            {
                fg.AppendLine($"if (item.{this.HasBeenSetAccessor} == rhs.{this.HasBeenSetAccessor})");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"if (item.{this.HasBeenSetAccessor})");
                    using (new BraceWrapper(fg))
                    {
                        this.GenerateForEqualsMaskCheck(fg, $"item.{this.Name}", $"rhs.{this.Name}", $"ret.{this.Name}");
                    }
                    fg.AppendLine($"else");
                    using (new BraceWrapper(fg))
                    {
                        this.GenerateForEqualsMask(fg, $"ret.{this.Name}", true);
                    }
                }
                fg.AppendLine($"else");
                using (new BraceWrapper(fg))
                {
                    this.GenerateForEqualsMask(fg, $"ret.{this.Name}", false);
                }
            }
        }

        public void GenerateForEqualsMaskCheck(FileGeneration fg, string accessor, string rhsAccessor, string retAccessor)
        {
            fg.AppendLine($"{retAccessor} = new {ContainerMaskFieldGeneration.GetMaskString(this, "bool")}();");
            if (isLoquiSingle)
            {
                var maskGen = this.ObjectGen.ProtoGen.Gen.MaskModule.GetMaskModule(SubTypeGeneration.GetType());
                var maskStr = maskGen.GetMaskString(SubTypeGeneration, "bool");
                fg.AppendLine($"{retAccessor}.Specific = {accessor}.SelectAgainst<{this.SubTypeGeneration.TypeName}, {maskStr}>({rhsAccessor}, ((l, r) =>");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{maskStr} itemRet;");
                    LoquiTypeSingleton.GenerateForEqualsMask(fg, new Accessor("l"), new Accessor("r"), "itemRet");
                    fg.AppendLine("return itemRet;");
                }
                fg.AppendLine($"), out {retAccessor}.Overall);");
                fg.AppendLine($"{retAccessor}.Overall = {retAccessor}.Overall && {retAccessor}.Specific.All((b) => {SubTypeGeneration.EqualsMaskAccessor("b")});");
            }
            else
            {
                fg.AppendLine($"{retAccessor}.Specific = {accessor}.SelectAgainst<{this.SubTypeGeneration.TypeName}, bool>({rhsAccessor}, ((l, r) => object.Equals(l, r)), out {retAccessor}.Overall);");
                fg.AppendLine($"{retAccessor}.Overall = {retAccessor}.Overall && {retAccessor}.Specific.All((b) => b);");
            }
        }

        public void GenerateForEqualsMask(FileGeneration fg, string retAccessor, bool on)
        {
            fg.AppendLine($"{retAccessor} = new {ContainerMaskFieldGeneration.GetMaskString(this, "bool")}();");
            fg.AppendLine($"{retAccessor}.Overall = {(on ? "true" : "false")};");
        }

        public override void GenerateForHash(FileGeneration fg, string hashResultAccessor)
        {
            fg.AppendLine($"{hashResultAccessor} = HashHelper.GetHashCode({this.Name}).CombineHashCode({hashResultAccessor});");
        }
    }
}
