using Noggog.Notifying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class TypeGeneration
    {
        public ObjectGeneration ObjectGen { get; private set; }
        public ProtocolGeneration ProtoGen => ObjectGen.ProtoGen;
        public bool KeyField { get; protected set; }
        public abstract string TypeName { get; }
        public virtual string SetToName => TypeName;
        public virtual string Name { get; set; }
        public virtual string Property => $"{this.Name}_Property";
        public virtual string ProtectedProperty => $"_{this.Name}";
        public virtual string PropertyOrName => $"{(this.Bare ? Name : Property)}";
        public string IndexEnumName => $"{this.ObjectGen.FieldIndexName}.{this.Name}";
        public bool HasIndex => !string.IsNullOrWhiteSpace(this.Name);
        public abstract string ProtectedName { get; }
        public string HasBeenSetAccessor => $"{this.Property}.HasBeenSet";
        protected bool _derivative;
        public virtual bool Derivative => this._derivative;
        public virtual bool IntegrateField { get; set; } = true;
        public bool RaisePropertyChanged;
        public bool Protected;
        private bool _copy;
        public virtual bool Copy => _copy;
        public bool TrueReadOnly => this.ObjectGen is StructGeneration;
        public bool GenerateClassMembers = true;
        public abstract bool IsEnumerable { get; }
        public readonly HasBeenSetItem<bool> NotifyingProperty = new HasBeenSetItem<bool>();
        public bool Notifying => NotifyingProperty.Item;
        public readonly HasBeenSetItem<bool> HasBeenSetProperty = new HasBeenSetItem<bool>();
        public bool HasBeenSet => HasBeenSetProperty.Item;
        public bool Bare => !this.Notifying && !this.HasBeenSet;
        public Dictionary<object, object> CustomData = new Dictionary<object, object>();

        public TypeGeneration()
        {
        }

        public void SetObjectGeneration(ObjectGeneration obj)
        {
            this.ObjectGen = obj;
            this.RaisePropertyChanged = this.ObjectGen.RaisePropertyChangedDefault;
            this.NotifyingProperty.SetIfNotSet(this.ObjectGen.NotifyingDefault, markAsSet: false);
            this.HasBeenSetProperty.SetIfNotSet(this.ObjectGen.HasBeenSetDefault, markAsSet: false);
            this._derivative = this.ObjectGen.DerivativeDefault;
            this.Protected = this.ObjectGen.ProtectedDefault;
        }

        public virtual async Task Load(XElement node, bool requireName = true)
        {
            LoadTypeGenerationFromNode(node, requireName);
        }

        protected void LoadTypeGenerationFromNode(XElement node, bool requireName = true)
        {
            node.TransferAttribute<bool>(Constants.HIDDEN_FIELD, i => this.IntegrateField = !i);
            Name = node.GetAttribute<string>(Constants.NAME);
            node.TransferAttribute<bool>(Constants.KEY_FIELD, i => this.KeyField = i);
            node.TransferAttribute<bool>(Constants.DERIVATIVE, i => this._derivative = i);
            this.Protected = node.GetAttribute<bool>(Constants.PROTECTED, this.ObjectGen.ProtectedDefault || Derivative);
            this._copy = node.GetAttribute<bool>(Constants.COPY, !this.Protected);
            node.TransferAttribute<bool>(Constants.GENERATE_CLASS_MEMBERS, i => this.GenerateClassMembers = i);
            node.TransferAttribute<bool>(Constants.RAISE_PROPERTY_CHANGED, i => this.RaisePropertyChanged = i);
            node.TransferAttribute<bool>(Constants.NOTIFYING, i => this.NotifyingProperty.Item = i);
            node.TransferAttribute<bool>(Constants.HAS_BEEN_SET, i => this.HasBeenSetProperty.Item = i);
            if (requireName && Name == null)
            {
                throw new ArgumentException("Type field needs a name.");
            }
        }

        public abstract bool IsNullable();

        public void FinalizeField()
        {
            if (this._derivative && !Protected)
            {
                throw new ArgumentException("Cannot mark field as non-readonly if also derivative.  Being derivative implied being readonly.");
            }
        }

        public virtual IEnumerable<string> GetRequiredNamespaces()
        {
            yield break;
        }

        public virtual void GenerateForCtor(FileGeneration fg)
        {

        }

        public abstract void GenerateForClass(FileGeneration fg);

        public abstract void GenerateForInterface(FileGeneration fg);

        public abstract void GenerateForGetterInterface(FileGeneration fg);

        public abstract bool CopyNeedsTryCatch { get; }

        public abstract string SkipCheck(string copyMaskAccessor);

        public abstract void GenerateForCopy(
            FileGeneration fg,
            string accessorPrefix,
            string rhsAccessorPrefix,
            string copyMaskAccessor,
            string defaultFallbackAccessor,
            string cmdsAccessor,
            bool protectedMembers);

        public abstract string GenerateACopy(string rhsAccessor);

        public abstract void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor, bool internalUse);

        public abstract void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier);

        public abstract void GenerateUnsetNth(FileGeneration fg, string identifier, string cmdsAccessor);

        public abstract void GenerateGetNth(FileGeneration fg, string identifier);

        public abstract void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor);

        public abstract void GenerateForEquals(FileGeneration fg, string rhsAccessor);

        public abstract void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor);

        public abstract void GenerateForHash(FileGeneration fg, string hashResultAccessor);

        public virtual void GenerateForInterfaceExt(FileGeneration fg) { }

        public virtual void GenerateForStaticCtor(FileGeneration fg) { }

        public virtual void GenerateGetNameIndex(FileGeneration fg)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"return (ushort){this.ObjectGen.FieldIndexName}.{this.Name};");
        }

        public virtual void GenerateGetNthName(FileGeneration fg)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"return \"{this.Name}\";");
        }

        public virtual void GenerateGetNthType(FileGeneration fg)
        {
            if (!this.IntegrateField) return;
            fg.AppendLine($"return typeof({this.TypeName});");
        }

        public abstract void GenerateToString(FileGeneration fg, string name, string accessor, string fgAccessor);

        public abstract void GenerateForHasBeenSetCheck(FileGeneration fg, string accessor, string checkMaskAccessor);

        public abstract void GenerateForHasBeenSetMaskGetter(FileGeneration fg, string accessor, string retAccessor);

        public virtual void GenerateGetNthObjectHasBeenSet(FileGeneration fg)
        {
            if (!this.IntegrateField) return;
            if (this.HasBeenSet)
            {
                fg.AppendLine($"return obj.{this.HasBeenSetAccessor};");
            }
            else
            {
                fg.AppendLine("return true;");
            }
        }

        public virtual string GetName(bool internalUse, bool property)
        {
            if (internalUse)
            {
                if (property)
                {
                    return this.ProtectedProperty;
                }
                else
                {
                    return this.ProtectedName;
                }
            }
            else
            {
                if (property)
                {
                    return this.Property;
                }
                else
                {
                    return this.Name;
                }
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(this.Name)) return base.ToString();
            return $"{base.ToString()}: {this.Name}";
        }

        public virtual async Task Resolve()
        {
            this.ObjectGen.RequiredNamespaces.Add(this.GetRequiredNamespaces());
        }
    }
}
