using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class NothingType : TypeGeneration
    {
        public override bool IntegrateField => false;
        public override string TypeName(bool getter) => null;
        public override string ProtectedName => null;
        public override bool CopyNeedsTryCatch => false;
        public override bool IsEnumerable => false;
        public override bool Namable => false;
        public override bool IsClass => throw new ArgumentException();
        public override bool HasDefault => throw new ArgumentException();

        public override string GenerateACopy(string rhsAccessor)
        {
            return null;
        }

        public override async Task Load(XElement node, bool requireName = true)
        {
            await base.Load(node, requireName);
            this.NotifyingProperty.OnNext(NotifyingType.None);
            this.HasBeenSetProperty.OnNext(false);
        }

        public override void GenerateClear(FileGeneration fg, Accessor accessorPrefix)
        {
        }

        public override void GenerateForClass(FileGeneration fg)
        {
        }

        public override void GenerateForCopy(FileGeneration fg, Accessor accessor, string rhsAccessorPrefix, string copyMaskAccessor, string defaultFallbackAccessor, bool protectedMembers)
        {
        }

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
        }

        public override void GenerateForHasBeenSetCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
        {
        }

        public override void GenerateForHasBeenSetMaskGetter(FileGeneration fg, Accessor accessor, string retAccessor)
        {
        }

        public override void GenerateForHash(FileGeneration fg, Accessor accessor, string hashResultAccessor)
        {
        }

        public override void GenerateForInterface(FileGeneration fg, bool getter, bool internalInterface)
        {
        }

        public override void GenerateGetNth(FileGeneration fg, Accessor identifier)
        {
        }

        public override void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, bool internalUse)
        {
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, Accessor identifier, string onIdentifier)
        {
        }

        public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
        {
        }

        public override void GenerateUnsetNth(FileGeneration fg, Accessor identifier)
        {
        }

        public override string ToString()
        {
            return "Nothing";
        }

        public override string SkipCheck(string copyMaskAccessor)
        {
            return null;
        }

        public override bool IsNullable()
        {
            throw new ArgumentException();
        }
    }
}
