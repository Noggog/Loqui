using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class SetMarkerType : TypeGeneration
    {
        public enum ExpandSets { False, FalseAndInclude, True, TrueAndInclude }

        public override string TypeName(bool getter) => nameof(SetMarkerType);
        public override string ProtectedName => throw new ArgumentException();
        public override bool CopyNeedsTryCatch => throw new ArgumentException();
        public List<TypeGeneration> SubFields = new List<TypeGeneration>();
        public override bool IntegrateField => false;
        public override bool IsEnumerable => throw new ArgumentException();
        public override bool IsClass => throw new ArgumentException();
        public override bool HasDefault => throw new ArgumentException();

        public IEnumerable<(int Index, TypeGeneration Field)> IterateFields(
            bool nonIntegrated = false,
            ExpandSets expandSets = ExpandSets.True)
        {
            int i = 0;
            foreach (var field in this.SubFields)
            {
                if (!field.IntegrateField && !nonIntegrated) continue;
                yield return (i++, field);
            }
        }

        public override async Task Load(XElement node, bool requireName = true)
        {
            var fieldsNode = node.Element(XName.Get(Constants.FIELDS, LoquiGenerator.Namespace));
            if (fieldsNode != null)
            {
                foreach (var fieldNode in fieldsNode.Elements())
                {
                    var typeGen = await this.ObjectGen.LoadField(fieldNode, requireName: true);
                    if (typeGen.Succeeded)
                    {
                        this.SubFields.Add(typeGen.Value);
                    }
                }
            }
        }

        #region Abstract
        public override string GenerateACopy(string rhsAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateClear(FileGeneration fg, Accessor accessorPrefix)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForCopy(FileGeneration fg, Accessor accessor, string rhsAccessorPrefix, string copyMaskAccessor, bool protectedMembers, bool deepCopy)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForEquals(FileGeneration fg, Accessor accessor, Accessor rhsAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateGetNameIndex(FileGeneration fg)
        {
            throw new NotImplementedException();
        }

        public override void GenerateGetNthName(FileGeneration fg)
        {
            throw new NotImplementedException();
        }

        public override void GenerateGetNthType(FileGeneration fg)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForHasBeenSetCheck(FileGeneration fg, Accessor accessor, string checkMaskAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForHasBeenSetMaskGetter(FileGeneration fg, Accessor accessor, string retAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForHash(FileGeneration fg, Accessor accessor, string hashResultAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForInterface(FileGeneration fg, bool getter, bool internalInterface)
        {
            throw new NotImplementedException();
        }

        public override void GenerateGetNth(FileGeneration fg, Accessor identifier)
        {
            throw new NotImplementedException();
        }

        public override void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, bool internalUse)
        {
            throw new NotImplementedException();
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, Accessor identifier, string onIdentifier)
        {
            throw new NotImplementedException();
        }

        public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateUnsetNth(FileGeneration fg, Accessor identifier)
        {
            throw new NotImplementedException();
        }

        public override void GenerateGetNthObjectHasBeenSet(FileGeneration fg)
        {
            throw new NotImplementedException();
        }

        public override string SkipCheck(string copyMaskAccessor, bool deepCopy)
        {
            throw new NotImplementedException();
        }

        public override bool IsNullable()
        {
            throw new NotImplementedException();
        }

        public override string GetDuplicate(Accessor accessor)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
