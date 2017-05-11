using System;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public class Array2DType : ContainerType
    {
        string dim;

        public override string TypeName => $"INotifyingContainer2D<{this.ItemTypeName}>";
        public override bool CopyNeedsTryCatch => true;
        public override string SkipCheck(string copyMaskAccessor) => $"{copyMaskAccessor}?.{this.Name}.Overall != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);
            dim = node.GetAttribute<string>("dimension");
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            fg.AppendLine($"private readonly INotifyingContainer2D<{ItemTypeName}> _{this.Name} = new NotifyingArray2D<{ItemTypeName}>(new Dimension2D({dim});");
            fg.AppendLine($"public INotifyingContainer2D<{ItemTypeName}> {this.Name} => _{this.Name};");
            GenerateInterfaceMembers(fg, $"_{this.Name}");
        }

        public void GenerateInterfaceMembers(FileGeneration fg, string member)
        {
            using (new RegionWrapper(fg, "Interface Members"))
            {
                // Get nth
                fg.AppendLine($"public {ItemTypeName} GetNth{this.Name}(int index)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"return {member}[index];");
                }
                if (this.isNoggSingle)
                {
                    fg.AppendLine($"{GetterTypeName} {this.ObjectGen.Getter_InterfaceStr}.GetNth{this.Name}(int index)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"return {member}[index];");
                    }
                }

                fg.AppendLine($"INotifyingContainer2D{(this.Protected ? "Getter" : string.Empty)}<{this.ItemTypeName}> {this.ObjectGen.InterfaceStr}.{this.GetName(false, true)} => {member};");
                fg.AppendLine($"INotifyingContainer2DGetter<{this.ItemTypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.GetName(false, true)} => {member};");
            }
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            fg.AppendLine($"{accessorPrefix}.{this.Name}.Unset({cmdAccessor}.ToUnsetParams());");
        }

        public override void GenerateForCopy(
            FileGeneration fg,
            string accessorPrefix,
            string rhsAccessorPrefix,
            string copyMaskAccessor,
            string defaultFallbackAccessor,
            string cmdsAccessor,
            bool protectedMembers)
        {
            fg.AppendLine($"{accessorPrefix}.{this.GetName(protectedMembers, false)}.SetTo({rhsAccessorPrefix}.{this.Name}{(this.isNoggSingle ? ".Select((s) => s.Copy())" : string.Empty)}, {cmdsAccessor});");
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            fg.AppendLine($"INotifyingContainer2DGetter<{ItemTypeName}> {this.GetName(false, true)} {{ get; }}");
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            fg.AppendLine($"new INotifyingContainer2D{(this.Protected ? "Getter" : string.Empty)}<{ItemTypeName}> {this.GetName(false, true)} {{ get; }}");
        }

        public override void GenerateInterfaceSet(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor)
        {
            GenerateForCopy(fg, accessorPrefix, rhsAccessorPrefix, null, null, cmdsAccessor, false);
            fg.AppendLine($"break;");
        }

        public override void GenerateGetNth(FileGeneration fg, string identifier)
        {
            fg.AppendLine($"return {identifier}.{this.GetName(false, true)};");
        }
    }
}
