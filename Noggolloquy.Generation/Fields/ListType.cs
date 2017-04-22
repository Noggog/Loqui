using System;

namespace Noggolloquy.Generation
{
    public class ListType : ContainerType
    {
        public override string TypeName => $"NotifyingList<{this.ItemTypeName}>";

        public override void GenerateForClass(FileGeneration fg)
        {
            fg.AppendLine($"private readonly INotifyingList<{ItemTypeName}> _{this.Name} = new NotifyingList<{ItemTypeName}>();");
            fg.AppendLine($"public INotifyingList{(this.Protected ? "Getter" : string.Empty)}<{ItemTypeName}> {this.Name} => _{this.Name};");
            GenerateInterfaceMembers(fg, $"_{this.Name}");
        }

        public void GenerateInterfaceMembers(FileGeneration fg, string member)
        {
            using (new RegionWrapper(fg, "Interface Members"))
            {
                // Get nth
                if (!this.ReadOnly)
                {
                    fg.AppendLine($"INotifyingList{(this.Protected ? "Getter" : string.Empty)}<{this.ItemTypeName}> {this.ObjectGen.InterfaceStr}.{this.Name} => {member};");
                }
                fg.AppendLine($"INotifyingListGetter<{this.ItemTypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => {member};");
            }
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            if (!this.ReadOnly)
            {
                fg.AppendLine($"new INotifyingList{(this.Protected ? "Getter" : string.Empty)}<{ItemTypeName}> {this.Name} {{ get; }}");
            }
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            fg.AppendLine($"INotifyingListGetter<{ItemTypeName}> {this.Name} {{ get; }}");
        }

        public override void GenerateGetNth(FileGeneration fg, string identifier)
        {
            fg.AppendLine($"return {identifier}.{this.Name};");
        }

        private void GenerateHasBeenSetCopy()
        {

        }

        public override void GenerateForCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdAccessor, bool protectedUse)
        {
            if (defaultFallbackAccessor == null)
            {
                GenerateCopy(fg, accessorPrefix, rhsAccessorPrefix, cmdAccessor, protectedUse);
            }
            else
            {
                fg.AppendLine($"if ({rhsAccessorPrefix}.{this.HasBeenSetAccessor})");
                using (new BraceWrapper(fg))
                {
                    if (defaultFallbackAccessor == null || !this.isNoggSingle)
                    {
                        GenerateCopy(fg, accessorPrefix, rhsAccessorPrefix, cmdAccessor, protectedUse);
                    }
                    else
                    {
                        fg.AppendLine("int i = 0;");
                        fg.AppendLine($"List<{this.ItemTypeName}> defList = {defaultFallbackAccessor}?.{this.Name}.ToList();");
                        fg.AppendLine($"{accessorPrefix}.{this.Name}.SetTo(");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"{rhsAccessorPrefix}.{this.Name}.Select((s) =>");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine("return s.Copy(defList?[i++]);");
                            }
                        }
                        fg.AppendLine($"), {cmdAccessor});");
                    }
                }
                fg.AppendLine("else");
                using (new BraceWrapper(fg))
                {
                    if (defaultFallbackAccessor != null)
                    {
                        fg.AppendLine($"if ({defaultFallbackAccessor} == null)");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"{accessorPrefix}.{this.Name}.Unset({cmdAccessor}.ToUnsetParams());");
                        }
                        fg.AppendLine("else");
                        using (new BraceWrapper(fg))
                        {
                            GenerateCopy(fg, accessorPrefix, defaultFallbackAccessor, cmdAccessor, protectedUse);
                        }
                    }
                    else
                    {
                        GenerateCopy(fg, accessorPrefix, defaultFallbackAccessor, cmdAccessor, protectedUse);
                    }
                }
            }
        }

        public override void GenerateInterfaceSet(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor)
        {
            fg.AppendLine($"{accessorPrefix}.{this.ProtectedName}.SetTo(({rhsAccessorPrefix}){(this.isNoggSingle ? ".Select((s) => s.Copy())" : string.Empty)}, {cmdsAccessor});");
        }

        private void GenerateCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdAccessor, bool protectedUse)
        {
            fg.AppendLine($"{accessorPrefix}.{this.GetName(protectedUse, false)}.SetTo({rhsAccessorPrefix}.{this.Name}{(this.isNoggSingle ? ".Select((s) => s.Copy())" : string.Empty)}, {cmdAccessor});");
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            fg.AppendLine($"{accessorPrefix}.{this.Name}.Unset({cmdAccessor}.ToUnsetParams());");
        }
    }
}
