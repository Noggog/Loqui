using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class TypeGeneration
    {
        private ObjectGeneration _ObjectGen;
        public ObjectGeneration ObjectGen
        {
            get { return _ObjectGen; }
            set
            {
                if (value is StructGeneration)
                {
                    this.Notifying = NotifyingOption.None;
                }
                this._ObjectGen = value;
            }
        }
        public ProtocolGeneration ProtoGen;
        public bool KeyField { get; protected set; }
        public abstract string TypeName { get; }
        public virtual string SetToName => TypeName;
        public virtual string Name { get; set; }
        public virtual string Property => $"{this.Name}_Property";
        public virtual string ProtectedProperty => $"_{this.Name}";
        public virtual string PropertyOrName => $"{this.Name}{(this.Notifying == NotifyingOption.None ? string.Empty : "_Property")}";
        public string IndexEnumName => $"{this.ObjectGen.EnumName}.{this.Name}";
        public abstract string ProtectedName { get; }
        public string HasBeenSetAccessor => this.Property + ".HasBeenSet";
        protected bool _derivative;
        public virtual bool Derivative => this._derivative;
        public bool RaisePropertyChanged;
        private bool _imports;
        public virtual bool Imports => _imports && !Derivative;
        public bool Protected;
        private bool _copy;
        public virtual bool Copy => _copy;
        public bool TrueReadOnly => this.ObjectGen is StructGeneration;
        public bool GenerateClassMembers;
        public NotifyingOption Notifying;

        public virtual void Load(XElement node, bool requireName = true)
        {
            KeyField = node.GetAttribute<bool>("keyField", false);
            Name = node.GetAttribute<string>("name");
            this._derivative = node.GetAttribute<bool>("derivative", this.ObjectGen.DerivativeDefault);
            this.Protected = node.GetAttribute<bool>("protected", this.ObjectGen.ProtectedDefault || Derivative);
            if (this._derivative && !Protected)
            {
                throw new ArgumentException("Cannot mark field as non-readonly if also derivative.  Being derivative implied being readonly.");
            }
            this._imports = node.GetAttribute<bool>("export", true);
            this._copy = node.GetAttribute<bool>("copy", !this.Protected);
            this.GenerateClassMembers = node.GetAttribute<bool>("generateClassMembers", true);
            this.RaisePropertyChanged = node.GetAttribute<bool>("raisePropertyChanged", this.ObjectGen.RaisePropertyChangedDefault);
            this.Notifying = node.GetAttribute<NotifyingOption>("notifying", this.ObjectGen.NotifyingDefault);
            if (requireName && Name == null)
            {
                throw new ArgumentException("Type field needs a name.");
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

        public abstract void GenerateInterfaceSet(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor);

        public abstract void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier, bool internalUse);

        public abstract void GenerateUnsetNth(FileGeneration fg, string identifier, string cmdsAccessor);

        public abstract void GenerateGetNth(FileGeneration fg, string identifier);

        public abstract void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor);

        public abstract void GenerateForEquals(FileGeneration fg, string rhsAccessor);

        public abstract void GenerateForEqualsMask(FileGeneration fg, string accessor, string rhsAccessor, string retAccessor);

        public abstract void GenerateForHash(FileGeneration fg, string hashResultAccessor);

        public virtual void GenerateForInterfaceExt(FileGeneration fg) { }

        public abstract void GenerateToString(FileGeneration fg, string accessor, string fgAccessor);

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

        public virtual void Resolve()
        {
            this.ObjectGen.RequiredNamespaces.Add(this.GetRequiredNamespaces());
        }
    }
}
