using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
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
        public DictMode Mode => subDictGenerator.Mode;
        public override bool CopyNeedsTryCatch => subGenerator.CopyNeedsTryCatch;

        public TypeGeneration KeyTypeGen => subDictGenerator.KeyTypeGen;
        public TypeGeneration ValueTypeGen => subDictGenerator.ValueTypeGen;
        public string TypeTuple => subDictGenerator.TypeTuple;

        public override string Property => subGenerator.Property;
        public override string ProtectedName => subGenerator.ProtectedName;
        public override string TypeName => subGenerator.TypeName;
        public override bool IsEnumerable => true;
        public override bool IsClass => true;
        public override bool HasDefault => false;

        public override string SkipCheck(string copyMaskAccessor) => subGenerator.SkipCheck(copyMaskAccessor);

        public override string GetName(bool internalUse, bool property)
        {
            return subGenerator.GetName(internalUse, property);
        }

        public void AddMaskException(FileGeneration fg, string errorMaskMemberAccessor, string exception, bool key)
        {
            subDictGenerator.AddMaskException(fg, errorMaskMemberAccessor, exception, key);
        }

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);

            var keyedValueNode = node.Element(XName.Get("KeyedValue", LoquiGenerator.Namespace));
            if (keyedValueNode != null)
            {
                var dictType = new DictType_KeyedValue();
                dictType.SetObjectGeneration(this.ObjectGen, setDefaults: false);
                subGenerator = dictType;
                await subGenerator.Load(node, requireName);
                subDictGenerator = dictType;
            }
            else
            {
                var dictType = new DictType_Typical();
                dictType.SetObjectGeneration(this.ObjectGen, setDefaults: false);
                subGenerator = dictType;
                await subGenerator.Load(node, requireName);
                subDictGenerator = dictType;
            }
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier)
        {
            subGenerator.GenerateSetNthHasBeenSet(fg, identifier, onIdentifier);
        }

        public override void GenerateUnsetNth(FileGeneration fg, string identifier, string cmdsAccessor)
        {
            subGenerator.GenerateUnsetNth(fg, identifier, cmdsAccessor);
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

        public override void GenerateForCopy(
            FileGeneration fg,
            Accessor accessor,
            string rhsAccessorPrefix,
            string copyMaskAccessor,
            string defaultFallbackAccessor,
            string cmdsAccessor,
            bool protectedMembers)
        {
            subGenerator.GenerateForCopy(fg, accessor, rhsAccessorPrefix, copyMaskAccessor, defaultFallbackAccessor, cmdsAccessor, protectedMembers);
        }

        public override void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor, bool internalUse)
        {
            subGenerator.GenerateSetNth(fg, accessorPrefix, rhsAccessorPrefix, cmdsAccessor, internalUse);
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

        public override async Task Resolve()
        {
            await subGenerator.Resolve();
        }

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            subGenerator.GenerateForEquals(fg, accessor, rhsAccessor);
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            subGenerator.GenerateForEqualsMask(fg, accessor, rhsAccessor, retAccessor);
        }

        public override void GenerateForHash(FileGeneration fg, string hashResultAccessor)
        {
            subGenerator.GenerateForHash(fg, hashResultAccessor);
        }

        public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
        {
            subGenerator.GenerateToString(fg, name, accessor, fgAccessor);
        }

        public override void GenerateForHasBeenSetCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
        {
            subGenerator.GenerateForHasBeenSetCheck(fg, accessor, checkMaskAccessor);
        }

        public override void GenerateForHasBeenSetMaskGetter(FileGeneration fg, Accessor accessor, string retAccessor)
        {
            subGenerator.GenerateForHasBeenSetMaskGetter(fg, accessor, retAccessor);
        }

        public override bool IsNullable()
        {
            return false;
        }
    }
}
