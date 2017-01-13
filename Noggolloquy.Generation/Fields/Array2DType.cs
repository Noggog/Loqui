using System;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public class Array2DType : ContainerType
    {
        string dim;

        public override string TypeName
        {
            get
            {
                return "INotifyingContainer2D<" + this.ItemTypeName + ">";
            }
        }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            dim = node.GetAttribute<string>("dimension");
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            fg.AppendLine("private readonly INotifyingContainer2D<" + ItemTypeName + "> _" + this.Name + " = new NotifyingArray2D<" + ItemTypeName + $">(new Dimension2D({dim});");
            fg.AppendLine("public INotifyingContainer2D<" + ItemTypeName + "> " + this.Name + " { get { return _" + this.Name + "; } }");
            GenerateInterfaceMembers(fg, "_" + this.Name);
        }

        public void GenerateInterfaceMembers(FileGeneration fg, string member)
        {
            using (new RegionWrapper(fg, "Interface Members"))
            {
                // Get nth
                fg.AppendLine("public " + ItemTypeName + " GetNth" + this.Name + "(int index)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("return " + member + "[index];");
                }
                if (this.isLevSingle)
                {
                    fg.AppendLine(GetterTypeName + $" {this.ObjectGen.Getter_InterfaceStr}.GetNth{this.Name} (int index)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("return " + member + "[index];");
                    }
                }

                fg.AppendLine("INotifyingContainer2D" + (this.Protected ? "Getter" : string.Empty) + "<" + this.ItemTypeName + "> " + this.ObjectGen.InterfaceStr + "." + this.GetPropertyString(false) + " { get { return " + member + "; } }");
                fg.AppendLine("INotifyingContainer2DGetter<" + this.ItemTypeName + "> " + this.ObjectGen.Getter_InterfaceStr + "." + this.GetPropertyString(false) + " { get { return " + member + "; } }");
            }
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            fg.AppendLine($"{accessorPrefix}.{this.Name}.Unset({cmdAccessor}.ToUnsetParams());");
        }

        public override void GenerateForCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdsAccessor)
        {
            fg.AppendLine(accessorPrefix + "." + this.Name + ".SetTo(" + rhsAccessorPrefix + "." + this.Name + (this.isLevSingle ? ".Select((s) => s.Copy())" : string.Empty) + ", " + cmdsAccessor + ");");
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            fg.AppendLine("INotifyingContainer2DGetter<" + ItemTypeName + "> " + this.GetPropertyString(false) + " { get; }");
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            fg.AppendLine("new INotifyingContainer2D" + (this.Protected ? "Getter" : string.Empty) + "<" + ItemTypeName + "> " + this.GetPropertyString(false) + " { get; }");
        }

        public override void GenerateForSetTo(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdsAccessor)
        {
            GenerateForCopy(fg, accessorPrefix, rhsAccessorPrefix, defaultFallbackAccessor, cmdsAccessor);
        }

        public override void GenerateGetNth(FileGeneration fg, string identifier)
        {
            fg.AppendLine("return " + identifier + "." + this.GetPropertyString(false) + "; ");
        }
    }
}
