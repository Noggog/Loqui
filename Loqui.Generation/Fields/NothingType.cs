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
        public override string TypeName(bool getter, bool needsCovariance = false) => null;
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
            this.NotifyingProperty.OnNext((NotifyingType.None, true));
            this.NullableProperty.OnNext((false, true));
        }

        public override void GenerateClear(FileGeneration fg, Accessor accessorPrefix)
        {
        }

        public override void GenerateForClass(FileGeneration fg)
        {
        }

        public override void GenerateForCopy(FileGeneration fg, Accessor accessor, Accessor rhs, Accessor copyMaskAccessor, bool protectedMembers, bool deepCopy)
        {
        }

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, Accessor maskAccessor)
        {
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
        }

        public override void GenerateForNullableCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
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

        public override void GenerateSetNth(FileGeneration fg, Accessor accessor, Accessor rhs, bool internalUse)
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

        public override string SkipCheck(Accessor copyMaskAccessor, bool deepCopy)
        {
            return null;
        }

        public override string GetDuplicate(Accessor accessor)
        {
            throw new NotImplementedException();
        }
    }
}
