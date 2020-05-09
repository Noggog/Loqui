using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class ClassType : TypicalTypeGeneration
    {
        public bool Singleton { get; set; }
        public bool Readonly;
        public override string ProtectedName => $"_{this.Name}";
        public override bool IsClass => true;
        public override bool IsNullable => this.HasBeenSet && !this.Singleton;

        public abstract string GetNewForNonNullable();

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            this.Singleton = node.GetAttribute(Constants.SINGLETON, this.Singleton);
            this.ReadOnly = this.ReadOnly || this.Singleton;
        }

        protected override string GenerateDefaultValue()
        {
            if (this.Singleton
                && string.IsNullOrWhiteSpace(this.DefaultValue))
            {
                return GetNewForNonNullable();
            }
            return base.GenerateDefaultValue();
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            if (!this.IntegrateField) return;
            if (this.NotifyingType == NotifyingType.ReactiveUI)
            {
                if (this.HasBeenSet)
                {
                    if (!this.TrueReadOnly)
                    {
                        fg.AppendLine($"protected {this.TypeName(getter: false)} _{this.Name};");
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => this._{ this.Name};");
                            fg.AppendLine($"{SetPermissionStr}set => {this.Name}_Set(value);");
                        }
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"{this.TypeName(getter: true)} {this.ObjectGen.Interface(getter: true, this.InternalGetInterface)}.{this.Name} => this.{this.Name};");
                    }
                    else
                    {
                        fg.AppendLine($"public readonly {this.TypeName(getter: false)} {this.Name};");
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    }

                    using (var args = new FunctionWrapper(fg,
                        $"public void {this.Name}_Set"))
                    {
                        args.Add($"{this.TypeName(getter: false)} value");
                        args.Add($"bool markSet = true");
                    }
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"this.RaiseAndSetIfChanged(ref _{this.Name}, value, _hasBeenSetTracker, markSet, (int){this.ObjectCentralizationEnumName}, nameof({this.Name}), zzz);");
                    }

                    using (var args = new FunctionWrapper(fg,
                        $"public void {this.Name}_Unset"))
                    {
                    }
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"this.{this.Name}_Set({(this.HasDefault ? $"_{this.Name}_Default" : $"default({this.TypeName(getter: false)})")}, false);");
                    }
                }
                else
                {
                    fg.AppendLine($"private {this.TypeName(getter: false)} _{this.Name}{(this.Singleton ? $" = {GetNewForNonNullable()}" : string.Empty)};");
                    fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => {this.ProtectedName};");
                        if (!this.Singleton)
                        {
                            fg.AppendLine($"{SetPermissionStr}set => this.RaiseAndSetIfChanged(ref _{this.Name}, value, nameof({this.Name}));");
                        }
                        else
                        {
                            fg.AppendLine($"{SetPermissionStr}set => this.RaiseAndSetIfChanged(ref _{this.Name}, value ?? {this.GetNewForNonNullable()}, nameof({this.Name}));");
                        }
                    }
                    if (this.TypeName(getter: true) != this.TypeName(getter: false))
                    {
                        fg.AppendLine($"{this.TypeName(getter: true)} {this.ObjectGen.Interface(getter: true, this.InternalGetInterface)}.{this.Name} => this.{this.Name};");
                    }
                }
            }
            else
            {
                if (this.HasBeenSet)
                {
                    if (this.PrefersProperty)
                    {
                        if (!this.TrueReadOnly)
                        {
                            fg.AppendLine($"protected readonly IHasBeenSetItem<{base.TypeName(getter: false)}> _{this.Name};");
                            fg.AppendLine($"public IHasBeenSetItem<{this.TypeName(getter: false)}> {this.Property} => _{this.Name};");
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name}");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine($"get => this._{ this.Name}.Item;");
                                fg.AppendLine($"{SetPermissionStr}set => this._{this.Name}.Set(value);");
                            }
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"{this.TypeName(getter: false)} {this.ObjectGen.Interface(getter: true)}.{this.Name} => this.{this.Name};");
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName(getter: false)}> {this.ObjectGen.Interface(getter: true)}.{this.Property} => this.{this.Property};");
                        }
                        else
                        {
                            fg.AppendLine($"public readonly {this.TypeName(getter: false)} {this.Name};");
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"{this.TypeName(getter: false)} {this.ObjectGen.Interface(getter: true)}.{this.Name} => this.{this.Name};");
                            fg.AppendLine($"IHasBeenSetItemGetter<{this.TypeName(getter: false)}> {this.ObjectGen.Interface(getter: true)}.{this.Property} => HasBeenSetGetter.NotBeenSet_Instance;");
                        }
                    }
                    else if (this.CanBeNullable(false))
                    {
                        if (!this.TrueReadOnly)
                        {
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"protected {this.TypeName(getter: false)}? _{this.Name};");
                            fg.AppendLine($"public {this.TypeName(getter: false)}? {this.Name}");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine($"get => this._{ this.Name};");
                                fg.AppendLine($"{SetPermissionStr}set => this._{this.Name} = value;");
                            }
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            if (this.CanBeNullable(getter: true))
                            {
                                fg.AppendLine($"{this.TypeName(getter: true)}? {this.ObjectGen.Interface(getter: true, this.InternalGetInterface)}.{this.Name} => this.{this.Name};");
                            }
                            else
                            {
                                fg.AppendLine($"{this.TypeName(getter: true)} {this.ObjectGen.Interface(getter: true, this.InternalGetInterface)}.{this.Name} => this.{this.Name};");
                                fg.AppendLine($"bool {this.ObjectGen.Interface(getter: true, this.InternalGetInterface)}.{this.Name}_IsSet => this.{this.Name} != null;");
                            }
                        }
                        else
                        {
                            fg.AppendLine($"public readonly {this.TypeName(getter: false)}? {this.Name};");
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    fg.AppendLine($"private {this.TypeName(getter: false)} _{this.Name}{(this.IsNullable ? string.Empty : $" = {GetNewForNonNullable()}")};");
                    if (this.Singleton)
                    {
                        fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name} => {this.ProtectedName};");
                    }
                    else
                    {
                        fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => {this.ProtectedName};");
                            fg.AppendLine($"{SetPermissionStr}set => this._{this.Name} = value;");
                        }
                    }
                    if (this.TypeName(getter: true) != this.TypeName(getter: false))
                    {
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"{this.TypeName(getter: true)} {this.ObjectGen.Interface(getter: true, this.InternalGetInterface)}.{this.Name} => this.{this.Name};");
                    }
                }
            }
        }

        public override void GenerateClear(FileGeneration fg, Accessor identifier)
        {
            if (this.ReadOnly || !this.IntegrateField) return;
            // ToDo
            // Add internal interface support
            if (this.InternalSetInterface) return;
            if (this.NotifyingType == NotifyingType.ReactiveUI)
            {
                if (this.HasBeenSet)
                {
                    fg.AppendLine($"{identifier.DirectAccess}_Unset();");
                }
                else
                {
                    fg.AppendLine($"{identifier.DirectAccess} = {(this.HasDefault ? $"{this.ObjectGen.Name}._{this.Name}_Default" : $"default")};");
                }
                return;
            }
            if (this.HasProperty && this.PrefersProperty)
            {
                fg.AppendLine($"{identifier.PropertyAccess}.Unset();");
            }
            else if (this.HasBeenSet)
            {
                fg.AppendLine($"{identifier.DirectAccess} = default;");
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
