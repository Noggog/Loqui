using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class ListType : ContainerType
    {
        public override string TypeName(bool getter) => Interface(getter, internalInterface: true);
        public override bool CopyNeedsTryCatch => true;
        public override bool IsEnumerable => true;
        public override bool IsClass => true;
        public override bool HasDefault => false;

        public override IEnumerable<string> GetRequiredNamespaces()
        {
            yield return "DynamicData";
            yield return "CSharpExt.Rx";
        }

        public virtual string Interface(bool getter, bool internalInterface)
        {
            string itemTypeName = this.ItemTypeName(getter: getter);
            if (this.SingleTypeGen is LoquiType loqui)
            {
                itemTypeName = loqui.TypeName(getter: getter, internalInterface: internalInterface);
            }
            if (this.ReadOnly || getter)
            {
                if (this.Notifying)
                {
                    if (this.HasBeenSet)
                    {
                        return $"IObservableSetList<{itemTypeName}>";
                    }
                    else
                    {
                        return $"IObservableList<{itemTypeName}>";
                    }
                }
                else
                {
                    if (this.HasBeenSet)
                    {
                        return $"IReadOnlySetList<{itemTypeName}>";
                    }
                    else
                    {
                        return $"IReadOnlyList<{itemTypeName}>";
                    }
                }
            }
            else
            {
                if (this.Notifying)
                {
                    if (this.HasBeenSet)
                    {
                        return $"ISourceSetList<{itemTypeName}>";
                    }
                    else
                    {
                        return $"ISourceList<{itemTypeName}>";
                    }
                }
                else
                {
                    if (this.HasBeenSet)
                    {
                        return $"ISetList<{itemTypeName}>";
                    }
                    else
                    {
                        return $"IExtendedList<{itemTypeName}>";
                    }
                }
            }
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            fg.AppendLine($"private readonly {GetActualItemClass()} _{this.Name} = new {GetActualItemClass(ctor: true)};");
            fg.AppendLine($"public {this.Interface(getter: false, internalInterface: true)} {this.Name} => _{this.Name};");
            GenerateInterfaceMembers(fg, $"_{this.Name}");
        }

        protected virtual string GetActualItemClass(bool ctor = false)
        {
            if (this.NotifyingType == NotifyingType.ReactiveUI)
            {
                if (this.HasBeenSet)
                {
                    return $"SourceSetList<{ItemTypeName(getter: false)}>{(ctor ? "()" : null)}";
                }
                else
                {
                    return $"SourceList<{ItemTypeName(getter: false)}>{(ctor ? "()" : null)}";
                }
            }
            else
            {
                if (this.HasBeenSet)
                {
                    return $"SetList<{ItemTypeName(getter: false)}>{(ctor ? "()" : null)}";
                }
                else
                {
                    return $"ExtendedList<{ItemTypeName(getter: false)}>{(ctor ? "()" : null)}";
                }
            }
        }

        public void GenerateInterfaceMembers(FileGeneration fg, string member)
        {
            using (new RegionWrapper(fg, "Interface Members"))
            {
                if (!this.ReadOnly)
                {
                    fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    fg.AppendLine($"{Interface(getter: false, internalInterface: true)} {this.ObjectGen.Interface(internalInterface: this.InternalGetInterface)}.{this.Name} => {member};");
                }
                fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                fg.AppendLine($"{Interface(getter: true, internalInterface: true)} {this.ObjectGen.Interface(getter: true, internalInterface: this.InternalGetInterface)}.{this.Name} => {member};");
            }
        }


        public override void GenerateForInterface(FileGeneration fg, bool getter, bool internalInterface)
        {
            if (!ApplicableInterfaceField(getter: getter, internalInterface: internalInterface)) return;
            if (getter)
            {
                fg.AppendLine($"{Interface(getter: true, internalInterface: true)} {this.Name} {{ get; }}");
            }
            else
            {
                if (!this.ReadOnly)
                {
                    fg.AppendLine($"new {Interface(getter: false, internalInterface: true)} {this.Name} {{ get; }}");
                }
            }
        }

        public override string HasBeenSetAccessor(Accessor accessor = null)
        {
            if (accessor == null)
            {
                return $"{this.Property}.HasBeenSet";
            }
            else
            {
                return $"{accessor.PropertyAccess}.HasBeenSet";
            }
        }

        public override void GenerateGetNth(FileGeneration fg, Accessor identifier)
        {
            fg.AppendLine($"return {identifier.DirectAccess};");
        }

        private void GenerateHasBeenSetCopy()
        {

        }

        public override string SkipCheck(string copyMaskAccessor, bool deepCopy)
        {
            var loqui = this.SubTypeGeneration as LoquiType;
            if (!deepCopy
                && loqui != null
                && loqui.SupportsMask(MaskType.Copy))
            {
                return $"{copyMaskAccessor}?.{this.Name}.Overall != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
            }
            else if (deepCopy
                && loqui != null
                && loqui.SupportsMask(MaskType.Translation))
            {
                return $"{copyMaskAccessor}?.{this.Name}.Overall ?? true";
            }
            else
            {
                if (deepCopy)
                {
                    return $"{copyMaskAccessor}?.{this.Name} ?? true";
                }
                else
                {
                    return $"{copyMaskAccessor}?.{this.Name} != {nameof(CopyOption)}.{nameof(CopyOption.Skip)}";
                }
            }
        }

        public override void GenerateForCopy(
            FileGeneration fg,
            Accessor accessor,
            string rhsAccessorPrefix,
            string copyMaskAccessor,
            string defaultFallbackAccessor,
            bool protectedMembers,
            bool deepCopy)
        {
            if (this.isLoquiSingle)
            {
                if (deepCopy)
                {
                    LoquiType loqui = this.SubTypeGeneration as LoquiType;
                    using (var args = new ArgsWrapper(fg,
                        $"{accessor.PropertyOrDirectAccess}.SetToWithDefault"))
                    {
                        args.Add($"rhs: {rhsAccessorPrefix}.{this.Name}");
                        args.Add($"def: {defaultFallbackAccessor}?.{this.Name}");
                        args.Add((gen) =>
                        {
                            gen.AppendLine("converter: (r, d) =>");
                            using (new BraceWrapper(gen))
                            {
                                loqui.GenerateTypicalMakeCopy(
                                    gen,
                                    retAccessor: $"return ",
                                    rhsAccessor: new Accessor("r"),
                                    defAccessor: new Accessor("d"),
                                    copyMaskAccessor: copyMaskAccessor,
                                    deepCopy: deepCopy);
                            }
                        });
                    }
                }
                else
                {
                    LoquiType loqui = this.SubTypeGeneration as LoquiType;
                    using (var args = new ArgsWrapper(fg,
                        $"{accessor.PropertyOrDirectAccess}.SetToWithDefault<{this.SubTypeGeneration.TypeName(getter: false)}, {this.SubTypeGeneration.TypeName(getter: false)}>"))
                    {
                        args.Add($"rhs: {rhsAccessorPrefix}.{this.Name}");
                        args.Add($"def: {defaultFallbackAccessor}?.{this.Name}");
                        args.Add((gen) =>
                        {
                            gen.AppendLine("converter: (r, d) =>");
                            using (new BraceWrapper(gen))
                            {
                                var supportsCopy = loqui.SupportsMask(MaskType.Copy);
                                var accessorStr = $"copyMask?.{this.Name}{(supportsCopy ? ".Overall" : string.Empty)}";
                                gen.AppendLine($"switch ({accessorStr} ?? {nameof(CopyOption)}.{nameof(CopyOption.Reference)})");
                                using (new BraceWrapper(gen))
                                {
                                    gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.Reference)}:");
                                    using (new DepthWrapper(gen))
                                    {
                                        gen.AppendLine($"return ({loqui.TypeName()})r;");
                                    }
                                    gen.AppendLine($"case {nameof(CopyOption)}.{nameof(CopyOption.MakeCopy)}:");
                                    using (new DepthWrapper(gen))
                                    {
                                        loqui.GenerateTypicalMakeCopy(
                                            gen,
                                            retAccessor: $"return ",
                                            rhsAccessor: new Accessor("r"),
                                            defAccessor: new Accessor("d"),
                                            copyMaskAccessor: copyMaskAccessor,
                                            deepCopy: deepCopy);
                                    }
                                    gen.AppendLine($"default:");
                                    using (new DepthWrapper(gen))
                                    {
                                        gen.AppendLine($"throw new NotImplementedException($\"Unknown {nameof(CopyOption)} {{{accessorStr}}}. Cannot execute copy.\");");
                                    }
                                }
                            }
                        });
                    }
                }
            }
            else
            {
                FileGeneration subFg = new FileGeneration();
                this.SingleTypeGen.GenerateCopySetToConverter(subFg);
                if (subFg.Empty)
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{accessor.PropertyOrDirectAccess}.SetTo"))
                    {
                        args.Add($"rhs.{this.Name}");
                    }
                }
                else
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{accessor.PropertyOrDirectAccess}.SetToWithDefault"))
                    {
                        args.Add($"rhs.{this.Name}");
                        args.Add($"def?.{this.Name}");
                        args.Add(subFg.ToArray());
                    }
                }
            }
        }

        public override void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, bool internalUse)
        {
            fg.AppendLine($"{accessorPrefix}.{this.ProtectedName}.SetTo({rhsAccessorPrefix});");
            fg.AppendLine($"break;");
        }

        private void GenerateCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, bool protectedUse)
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
        }

        public override void GenerateClear(FileGeneration fg, Accessor accessorPrefix)
        {
            if (this.HasBeenSet)
            {
                fg.AppendLine($"{accessorPrefix.PropertyAccess}.Unset();");
            }
            else
            {
                fg.AppendLine($"{accessorPrefix.PropertyAccess}.Clear();");
            }
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
            if (this.HasBeenSet)
            {
                fg.AppendLine($"if ({checkMaskAccessor}.Overall.HasValue && {checkMaskAccessor}.Overall.Value != {accessor.DirectAccess}.HasBeenSet) return false;");
            }
        }

        public override void GenerateForHasBeenSetMaskGetter(FileGeneration fg, Accessor accessor, string retAccessor)
        {
            if (this.SubTypeGeneration is LoquiType loqui)
            {
                string maskGetter = loqui.TargetObjectGeneration == null ? nameof(ILoquiObjectGetter.GetHasBeenSetIMask) : "GetHasBeenSetMask";
                fg.AppendLine($"{retAccessor} = new {ContainerMaskFieldGeneration.GetMaskString(this, "bool")}({(this.HasBeenSet ? $"{accessor.PropertyOrDirectAccess}.HasBeenSet" : "true")}, {accessor.PropertyOrDirectAccess}.WithIndex().Select((i) => new MaskItemIndexed<bool, {loqui.GetMaskString("bool")}>(i.Index, true, i.Item.{maskGetter}())));");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = new MaskItem<bool, IEnumerable<(int, bool)>>({(this.HasBeenSet ? $"{accessor.PropertyOrDirectAccess}.HasBeenSet" : "true")}, null);");
            }
        }

        public override bool IsNullable()
        {
            return false;
        }
    }
}
