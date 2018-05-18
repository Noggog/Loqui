using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class ListType : ContainerType
    {
        public override string TypeName => $"NotifyingList<{this.ItemTypeName}>";
        public override bool CopyNeedsTryCatch => true;
        public override string SetToName => $"IEnumerable<{this.ItemTypeName}>";
        public override bool IsEnumerable => true;
        public int? MaxValue;

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            this.MaxValue = node.GetAttribute<int?>("maxSize", null);
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            if (MaxValue.HasValue)
            {
                fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                fg.AppendLine($"private readonly INotifyingList<{ItemTypeName}> _{this.Name} = new NotifyingListBounded<{ItemTypeName}>(max: {MaxValue});");
            }
            else
            {
                fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                fg.AppendLine($"private readonly INotifyingList<{ItemTypeName}> _{this.Name} = new NotifyingList<{ItemTypeName}>();");
            }
            fg.AppendLine($"public INotifyingList{(this.ReadOnly ? "Getter" : string.Empty)}<{ItemTypeName}> {this.Name} => _{this.Name};");
            if (this.ReadOnly)
            {
                fg.AppendLine($"public IEnumerable<{ItemTypeName}> {this.Name}Enumerable => _{this.Name};");
            }
            else
            {
                fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                fg.AppendLine($"public IEnumerable<{ItemTypeName}> {this.Name}Enumerable");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"get => _{this.Name};");
                    fg.AppendLine($"set => _{this.Name}.SetTo(value);");
                }
            }
            GenerateInterfaceMembers(fg, $"_{this.Name}");
        }

        public void GenerateInterfaceMembers(FileGeneration fg, string member)
        {
            using (new RegionWrapper(fg, "Interface Members"))
            {
                // Get nth
                if (!this.ReadOnly)
                {
                    fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    fg.AppendLine($"INotifyingList{(this.ReadOnly ? "Getter" : string.Empty)}<{this.ItemTypeName}> {this.ObjectGen.InterfaceStr}.{this.Name} => {member};");
                }
                fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                fg.AppendLine($"INotifyingListGetter<{this.ItemTypeName}> {this.ObjectGen.Getter_InterfaceStr}.{this.Name} => {member};");
            }
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            if (!this.ReadOnly)
            {
                fg.AppendLine($"new INotifyingList{(this.ReadOnly ? "Getter" : string.Empty)}<{ItemTypeName}> {this.Name} {{ get; }}");
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

        public override string SkipCheck(string copyMaskAccessor)
        {
            if (this.SubTypeGeneration is LoquiType)
            {
                return $"{copyMaskAccessor}?.{this.Name}.Overall != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
            }
            else
            {
                return $"{copyMaskAccessor}?.{this.Name} != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
            }
        }

        public override void GenerateForCopy(
            FileGeneration fg,
            Accessor accessor,
            string rhsAccessorPrefix,
            string copyMaskAccessor,
            string defaultFallbackAccessor,
            string cmdsAccessor,
            bool protectedMembers)
        {
            if (this.isLoquiSingle)
            {
                LoquiType loqui = this.SubTypeGeneration as LoquiType;
                using (var args = new ArgsWrapper(fg,
                    $"{accessor.PropertyOrDirectAccess}.SetToWithDefault"))
                {
                    args.Add($"rhs: rhs.{this.Name}");
                    args.Add($"def: def?.{this.Name}");
                    args.Add($"cmds: cmds");
                    args.Add((gen) =>
                    {
                        gen.AppendLine("converter: (r, d) =>");
                        using (new BraceWrapper(gen))
                        {

                            gen.AppendLine($"switch (copyMask?.{this.Name}.Overall ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                            using (new BraceWrapper(gen))
                            {
                                gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine("return r;");
                                }
                                gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                                using (new DepthWrapper(gen))
                                {
                                    loqui.GenerateTypicalMakeCopy(
                                        gen,
                                        copyMaskAccessor: copyMaskAccessor);
                                }
                                gen.AppendLine($"default:");
                                using (new DepthWrapper(gen))
                                {
                                    gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{copyMask?.{this.Name}.Overall}}. Cannot execute copy.\");");
                                }
                            }
                        }
                    });
                }
            }
            else
            {
                using (var args = new ArgsWrapper(fg,
                    $"{accessor.PropertyOrDirectAccess}.SetToWithDefault"))
                {
                    args.Add($"rhs.{this.Name}");
                    args.Add($"def?.{this.Name}");
                    args.Add($"cmds");
                }
            }
        }

        public override void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor, bool internalUse)
        {
            fg.AppendLine($"{accessorPrefix}.{this.ProtectedName}.SetTo({rhsAccessorPrefix}, {cmdsAccessor});");
            fg.AppendLine($"break;");
        }

        private void GenerateCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdAccessor, bool protectedUse)
        {
            fg.AppendLine($"{accessorPrefix}.{this.Name}.SetTo({rhsAccessorPrefix}.{this.Name}.Select((s) =>");
            using (new BraceWrapper(fg)
            {
                AppendParenthesis = true,
                AppendComma = true
            })
            {
                fg.AppendLine($"switch (copyMask?.{this.Name}.Overall ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine("return s;");
                    }
                    fg.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"return s.Copy(copyMask?.{this.Name}.Specific);");
                    }
                    fg.AppendLine($"default:");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{copyMask?.{this.Name}.Overall}}. Cannot execute copy.\");");
                    }
                }
            }
            fg.AppendLine($"{cmdAccessor});");
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            fg.AppendLine($"{accessorPrefix}.{this.Name}.Unset({cmdAccessor}.ToUnsetParams());");
        }

        public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
        {
            fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"{name} =>\");");
            fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"[\");");
            fg.AppendLine($"using (new DepthWrapper({fgAccessor}))");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"foreach (var subItem in {accessor.DirectAccess})");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"[\");");
                    fg.AppendLine($"using (new DepthWrapper({fgAccessor}))");
                    using (new BraceWrapper(fg))
                    {
                        this.SubTypeGeneration.GenerateToString(fg, "Item", new Accessor("subItem"), fgAccessor);
                    }
                    fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"]\");");
                }
            }
            fg.AppendLine($"{fgAccessor}.{nameof(FileGeneration.AppendLine)}(\"]\");");
        }

        public override void GenerateForHasBeenSetCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
        {
            fg.AppendLine($"if ({checkMaskAccessor}.Overall.HasValue && {checkMaskAccessor}.Overall.Value != {accessor.DirectAccess}.HasBeenSet) return false;");
        }

        public override void GenerateForHasBeenSetMaskGetter(FileGeneration fg, Accessor accessor, string retAccessor)
        {
            if (this.SubTypeGeneration is LoquiType loqui)
            {
                fg.AppendLine($"{retAccessor} = new {ContainerMaskFieldGeneration.GetMaskString(this, "bool")}({accessor.PropertyOrDirectAccess}.HasBeenSet, {accessor.PropertyOrDirectAccess}.Select((i) => new MaskItem<bool, {loqui.GetMaskString("bool")}>(true, i.GetHasBeenSetMask())));");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = new MaskItem<bool, IEnumerable<bool>>({accessor.PropertyOrDirectAccess}.HasBeenSet, null);");
            }
        }

        public override bool IsNullable()
        {
            return false;
        }
    }
}
