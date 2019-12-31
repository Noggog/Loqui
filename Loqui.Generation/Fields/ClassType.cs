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
        public SingletonLevel Singleton;
        public bool Readonly;
        public override string ProtectedName => $"_{this.Name}";
        public override bool IsClass => true;

        public abstract string GetNewForNonNullable();

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            this.Singleton = node.GetAttribute<SingletonLevel>(Constants.NULLABLE, this.Singleton);
            this.ReadOnly = this.ReadOnly || this.Singleton == SingletonLevel.Singleton;
        }

        protected override string GenerateDefaultValue()
        {
            if (this.Singleton != SingletonLevel.None
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
                        fg.AppendLine($"public bool {this.HasBeenSetAccessor(new Accessor(this.Name))}");
                        using (new BraceWrapper(fg))
                        {
                            if (this.ObjectCentralized)
                            {
                                fg.AppendLine($"get => _hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}];");
                                fg.AppendLine($"{SetPermissionStr}set => this.RaiseAndSetIfChanged(_hasBeenSetTracker, value, (int){this.ObjectCentralizationEnumName}, nameof({this.HasBeenSetAccessor(new Accessor(this.Name))}));");
                            }
                        }
                        fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                        fg.AppendLine($"bool {this.ObjectGen.Interface(getter: true, this.InternalGetInterface)}.{this.Name}_IsSet => {this.HasBeenSetAccessor(new Accessor(this.Name))};");
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
                        fg.AppendLine($"this.RaiseAndSetIfChanged(ref _{this.Name}, value, _hasBeenSetTracker, markSet, (int){this.ObjectCentralizationEnumName}, nameof({this.Name}), nameof({this.HasBeenSetAccessor(new Accessor(this.Name))}));");
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
                    fg.AppendLine($"private {this.TypeName(getter: false)} _{this.Name}{(this.Singleton == SingletonLevel.None ? string.Empty : $" = {GetNewForNonNullable()}")};");
                    fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name}");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"get => {this.ProtectedName};");
                        if (this.Singleton == SingletonLevel.None)
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
                    else
                    {
                        if (!this.TrueReadOnly)
                        {
                            fg.AppendLine($"public bool {this.HasBeenSetAccessor(new Accessor(this.Name))}");
                            using (new BraceWrapper(fg))
                            {
                                if (this.ObjectCentralized)
                                {
                                    fg.AppendLine($"get => _hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}];");
                                    fg.AppendLine($"{SetPermissionStr}set => _hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}] = value;");
                                }
                            }
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"bool {this.ObjectGen.Interface(getter: true, this.InternalGetInterface)}.{this.Name}_IsSet => {this.HasBeenSetAccessor(new Accessor(this.Name))};");
                            fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                            fg.AppendLine($"protected {this.TypeName(getter: false)} _{this.Name};");
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
                            fg.AppendLine($"_{this.Name} = value;");
                            fg.AppendLine($"_hasBeenSetTracker[(int){this.ObjectCentralizationEnumName}] = markSet;");
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
                }
                else
                {
                    fg.AppendLine($"[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
                    fg.AppendLine($"private {this.TypeName(getter: false)} _{this.Name}{(this.Singleton == SingletonLevel.None ? string.Empty : $" = {GetNewForNonNullable()}")};");
                    if (this.Singleton == SingletonLevel.Singleton)
                    {
                        fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name} =>  {this.ProtectedName};");
                    }
                    else
                    {
                        fg.AppendLine($"public {this.TypeName(getter: false)} {this.Name}");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"get => {this.ProtectedName};");
                            if (this.Singleton == SingletonLevel.None)
                            {
                                fg.AppendLine($"{SetPermissionStr}set => this._{this.Name} = value;");
                            }
                            else
                            {
                                fg.AppendLine($"{SetPermissionStr}set => this.{this.ProtectedName} = value ?? {this.GetNewForNonNullable()};");
                            }
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

        public override bool IsNullable()
        {
            return this.Singleton == SingletonLevel.None;
        }
    }
}
