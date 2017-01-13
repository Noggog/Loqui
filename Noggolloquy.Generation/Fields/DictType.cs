using System;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public interface IDictType
    {
        TypeGeneration KeyTypeGen { get; }
        TypeGeneration ValueTypeGen { get; }
        string TypeTuple { get; }
        DictMode Mode { get; }
        void AddMaskException(FileGeneration fg, string errorMaskMemberAccessor, string exception, bool key);
    }

    public enum DictMode
    {
        KeyValue,
        KeyedValue
    }

    public class DictType : TypeGeneration, IDictType
    {
        private TypeGeneration subGenerator;
        private IDictType subDictGenerator;
        public DictMode Mode { get { return subDictGenerator.Mode; } }

        public TypeGeneration KeyTypeGen { get { return subDictGenerator.KeyTypeGen; } }
        public TypeGeneration ValueTypeGen { get { return subDictGenerator.ValueTypeGen; } }
        public string TypeTuple { get { return subDictGenerator.TypeTuple; } }

        public override string Property { get { return subGenerator.Property; } }
        public override string ProtectedName { get { return subGenerator.ProtectedName; } }
        public override bool Imports { get { return subGenerator.Imports; } }
        public override string TypeName { get { return subGenerator.TypeName; } }

        public override string GetPropertyString(bool internalUse)
        {
            return subGenerator.GetPropertyString(internalUse);
        }

        public void AddMaskException(FileGeneration fg, string errorMaskMemberAccessor, string exception, bool key)
        {
            subDictGenerator.AddMaskException(fg, errorMaskMemberAccessor, exception, key);
        }

        public override void Load(XElement node, bool requireName = true)
        {
            base.Load(node, requireName);

            var keyedValueNode = node.Element(XName.Get("KeyedValue", NoggolloquyGenerator.Namespace));
            if (keyedValueNode != null)
            {
                var dictType = new DictType_KeyedValue();
                dictType.ObjectGen = this.ObjectGen;
                subGenerator = dictType;
                subGenerator.Load(node, requireName);
                subDictGenerator = dictType;
            }
            else
            {
                var dictType = new DictType_Typical();
                dictType.ObjectGen = this.ObjectGen;
                subGenerator = dictType;
                subGenerator.Load(node, requireName);
                subDictGenerator = dictType;
            }
        }

        public override void SetMaskException(FileGeneration fg, string errorMaskMemberAccessor, string exception)
        {
            subGenerator.SetMaskException(fg, errorMaskMemberAccessor, exception);
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier, bool internalUse)
        {
            subGenerator.GenerateSetNthHasBeenSet(fg, identifier, onIdentifier, internalUse);
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            subGenerator.GenerateForClass(fg);
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            subGenerator.GenerateForInterface(fg);
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
        {
            subGenerator.GenerateForGetterInterface(fg);
        }

        public override void GenerateForCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdsAccessor)
        {
            subGenerator.GenerateForCopy(fg, accessorPrefix, rhsAccessorPrefix, defaultFallbackAccessor, cmdsAccessor);
        }

        public override void GenerateForSetTo(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string defaultFallbackAccessor, string cmdsAccessor)
        {
            subGenerator.GenerateForSetTo(fg, accessorPrefix, rhsAccessorPrefix, defaultFallbackAccessor, cmdsAccessor);
        }

        public override void GenerateGetNth(FileGeneration fg, string identifier)
        {
            subGenerator.GenerateGetNth(fg, identifier);
        }

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            subGenerator.GenerateClear(fg, accessorPrefix, cmdAccessor);
        }

        public override string GenerateACopy(string rhsAccessor)
        {
            return subGenerator.GenerateACopy(rhsAccessor);
        }
    }
}
