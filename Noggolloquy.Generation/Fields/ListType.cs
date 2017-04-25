using System;

namespace Noggolloquy.Generation
{
    public class ListType : ContainerType
    {
        public override string TypeName => $"NotifyingList<{this.ItemTypeName}>";
        public override bool CopyNeedsTryCatch => true;
        public override string SetToName => $"IEnumerable<{this.ItemTypeName}>";

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

        public override string SkipAccessor(string copyMaskAccessor)
        {
            if (this.SubTypeGeneration is NoggType)
            {
                return $"{copyMaskAccessor}?.{this.Name}.Overall";
            }
            else
            {
                return $"{copyMaskAccessor}?.{this.Name}";
            }
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
            if (defaultFallbackAccessor == null)
            {
                GenerateCopy(fg, accessorPrefix, rhsAccessorPrefix, cmdsAccessor, protectedMembers);
            }
            else
            {
                fg.AppendLine($"if ({rhsAccessorPrefix}.{this.HasBeenSetAccessor})");
                using (new BraceWrapper(fg))
                {
                    if (defaultFallbackAccessor == null || !this.isNoggSingle)
                    {
                        GenerateCopy(fg, accessorPrefix, rhsAccessorPrefix, cmdsAccessor, protectedMembers);
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
                                fg.AppendLine($"switch (copyMask?.{this.Name}.Overall ?? {nameof(CopyType)}.{nameof(CopyType.Reference)})");
                                using (new BraceWrapper(fg))
                                {
                                    fg.AppendLine($"case {nameof(CopyType)}.{nameof(CopyType.Reference)}:");
                                    using (new DepthWrapper(fg))
                                    {
                                        fg.AppendLine("return s;");
                                    }
                                    fg.AppendLine($"case {nameof(CopyType)}.{nameof(CopyType.Deep)}:");
                                    using (new DepthWrapper(fg))
                                    {
                                        fg.AppendLine($"return s.Copy(copyMask?.{this.Name}.Specific, defList?[i++]);");
                                    }
                                    fg.AppendLine($"default:");
                                    using (new DepthWrapper(fg))
                                    {
                                        fg.AppendLine($"throw new NotImplementedException($\"Unknown CopyType {{copyMask?.{this.Name}.Overall}}. Cannot execute copy.\");");
                                    }
                                }
                            }
                        }
                        fg.AppendLine($"), {cmdsAccessor});");
                    }
                }
                fg.AppendLine($"else if ({defaultFallbackAccessor} == null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{accessorPrefix}.{this.Name}.Unset({cmdsAccessor}.ToUnsetParams());");
                }
                fg.AppendLine("else");
                using (new BraceWrapper(fg))
                {
                    GenerateCopy(fg, accessorPrefix, defaultFallbackAccessor, cmdsAccessor, protectedMembers);
                }
            }
        }

        public override void GenerateInterfaceSet(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor)
        {
            fg.AppendLine($"{accessorPrefix}.{this.ProtectedName}.SetTo({rhsAccessorPrefix}, {cmdsAccessor});");
        }

        private void GenerateCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdAccessor, bool protectedUse)
        {
            if (!this.isNoggSingle)
            {
                fg.AppendLine($"{accessorPrefix}.{this.GetName(protectedUse, false)}.SetTo({rhsAccessorPrefix}.{this.Name}, {cmdAccessor});");
            }
            else
            {
                fg.AppendLine($"{accessorPrefix}.{this.Name}.SetTo({rhsAccessorPrefix}.{this.Name}.Select((s) =>");
                using (new BraceWrapper(fg)
                {
                    AppendParenthesis = true,
                    AppendComma = true
                })
                {
                    fg.AppendLine($"switch (copyMask?.{this.Name}.Overall ?? {nameof(CopyType)}.{nameof(CopyType.Reference)})");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"case {nameof(CopyType)}.{nameof(CopyType.Reference)}:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine("return s;");
                        }
                        fg.AppendLine($"case {nameof(CopyType)}.{nameof(CopyType.Deep)}:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"return s.Copy(copyMask?.{this.Name}.Specific);");
                        }
                        fg.AppendLine($"default:");
                        using (new DepthWrapper(fg))
                        {
                            fg.AppendLine($"throw new NotImplementedException($\"Unknown CopyType {{copyMask?.{this.Name}.Overall}}. Cannot execute copy.\");");
                        }
                    }
                }
                fg.AppendLine($"{cmdAccessor});");
            }
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            fg.AppendLine($"{accessorPrefix}.{this.Name}.Unset({cmdAccessor}.ToUnsetParams());");
        }
    }
}
