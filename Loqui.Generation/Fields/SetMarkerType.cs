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

        public override string TypeName => nameof(SetMarkerType);

        public override string ProtectedName => throw new NotImplementedException();

        public override bool CopyNeedsTryCatch => throw new NotImplementedException();

        public List<TypeGeneration> SubFields = new List<TypeGeneration>();

        public override bool IntegrateField => false;

        public IEnumerable<(int Index, TypeGeneration Field)> IterateFields(
            bool nonIntegrated = false,
            ExpandSets expandSets = ExpandSets.True)
        {
            for (int i = 0; i < this.SubFields.Count; i++)
            {
                yield return (i, this.SubFields[i]);
            }
        }

        public override async Task Load(XElement node, bool requireName = true)
        {
            var fieldsNode = node.Element(XName.Get(Constants.FIELDS, LoquiGenerator.Namespace));
            if (fieldsNode != null)
            {
                foreach (var fieldNode in fieldsNode.Elements())
                {
                    var typeGen = await this.ObjectGen.LoadField(fieldNode, true);
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

        public override void GenerateClear(FileGeneration fg, string accessorPrefix, string cmdAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForClass(FileGeneration fg)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForCopy(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string copyMaskAccessor, string defaultFallbackAccessor, string cmdsAccessor, bool protectedMembers)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForEquals(FileGeneration fg, string rhsAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForEqualsMask(FileGeneration fg, string accessor, string rhsAccessor, string retAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForGetterInterface(FileGeneration fg)
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

        public override void GenerateForHasBeenSetCheck(FileGeneration fg, string accessor, string checkMaskAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForHasBeenSetMaskGetter(FileGeneration fg, string accessor, string retAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForHash(FileGeneration fg, string hashResultAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForInterface(FileGeneration fg)
        {
            throw new NotImplementedException();
        }

        public override void GenerateGetNth(FileGeneration fg, string identifier)
        {
            throw new NotImplementedException();
        }

        public override void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor, bool internalUse)
        {
            throw new NotImplementedException();
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier)
        {
            throw new NotImplementedException();
        }

        public override void GenerateToString(FileGeneration fg, string name, string accessor, string fgAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateUnsetNth(FileGeneration fg, string identifier, string cmdsAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateGetNthObjectHasBeenSet(FileGeneration fg)
        {
            throw new NotImplementedException();
        }

        public override string SkipCheck(string copyMaskAccessor)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
