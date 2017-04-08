using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Noggolloquy.Generation
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
                    this.Notifying = false;
                }
                this._ObjectGen = value;
            }
        }
        public ProtocolGeneration ProtoGen;
        public bool KeyField { get; protected set; }
        public abstract string TypeName { get; }
        public virtual string Name { get; set; }
        public virtual string Property => $"{this.Name}_Property";
        public virtual string ProtectedProperty => $"_{this.Name}";
        public abstract string ProtectedName { get; }
        public string HasBeenSetAccessor => this.Property + ".HasBeenSet";
        protected bool _derivative;
        public virtual bool Derivative => this._derivative;
        private bool _imports;
        public virtual bool Imports => _imports && !Derivative;
        private bool _protected;
        public virtual bool Protected => _protected;
        public bool ReadOnly;
        private bool _copy;
        public virtual bool Copy => _copy;
        public bool TrueReadOnly => this.ObjectGen is StructGeneration;
        public bool GenerateClassMembers;
        public bool Notifying;

        public virtual void Load(XElement node, bool requireName = true)
        {
            KeyField = node.GetAttribute<bool>("keyField", false);
            Name = node.GetAttribute<string>("name");
            this._derivative = node.GetAttribute<bool>("derivative");
            this.ReadOnly = Derivative || node.GetAttribute<bool>("readOnly");
            this._protected = this.Derivative || this.ReadOnly;
            this._imports = node.GetAttribute<bool>("export", true);
            this._copy = node.GetAttribute<bool>("copy", !this.ReadOnly);
            this.GenerateClassMembers = node.GetAttribute<bool>("generateClassMembers", true);
            this.Notifying = node.GetAttribute<bool>("notifying", this.ObjectGen.NotifyingDefault);
            if (requireName && Name == null)
            {
                throw new ArgumentException("Type field needs a name.");
            }
        }

        public virtual IEnumerable<string> GetRequiredNamespaces()
        {
            yield break;
        }

        public abstract void GenerateForClass(FileGeneration fg);

        public abstract void GenerateForInterface(FileGeneration fg);

        public abstract void GenerateForGetterInterface(FileGeneration fg);

        public abstract void GenerateForCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdsAccessor);

        public abstract string GenerateACopy(string rhsAccessor);

        public abstract void GenerateForSetTo(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdsAccessor);

        public abstract void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier, bool internalUse);

        public abstract void GenerateGetNth(FileGeneration fg, string identifier);

        public abstract void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor);

        public abstract void SetMaskException(FileGeneration fg, string errorMaskMemberAccessor, string exception);

        public virtual void GenerateForInterfaceExt(FileGeneration fg)
        {

        }

        public virtual string GetPropertyString(bool internalUse)
        {
            if (internalUse)
            {
                return this.ProtectedProperty;
            }
            else
            {
                return this.Property;
            }
        }
    }
}
