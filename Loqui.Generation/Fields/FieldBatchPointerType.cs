using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class FieldBatchPointerType : TypeGeneration
    {
        public string BatchName { get; private set; }
        public ushort? ProtocolID { get; private set; }

        #region Type Generation Abstract
        public override string TypeName => throw new NotImplementedException();

        public override string ProtectedName => throw new NotImplementedException();

        public override bool CopyNeedsTryCatch => throw new NotImplementedException();

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

        public override void GenerateForGetterInterface(FileGeneration fg)
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

        public override void GenerateInterfaceSet(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier, bool internalUse)
        {
            throw new NotImplementedException();
        }

        public override void GenerateUnsetNth(FileGeneration fg, string identifier, string cmdsAccessor)
        {
            throw new NotImplementedException();
        }

        public override string SkipCheck(string copyMaskAccessor)
        {
            throw new NotImplementedException();
        }
        #endregion

        public override void Load(XElement node, bool requireName = true)
        {
            this.BatchName = node.GetAttribute<string>("name", throwException: true);
            this.ProtocolID = node.GetAttribute<ushort?>("protocolID", throwException: false);
        }

        public override void Resolve()
        {
            base.Resolve();
            var protoID = this.ProtocolID ?? this.ObjectGen.ProtoGen.Definition.Key.ProtocolID;
            if (!this.ObjectGen.ProtoGen.Gen.TryGetProtocol(protoID, out var protoGen))
            {
                throw new ArgumentException($"Protocol did not exist {protoID}.");
            }
            if (!protoGen.Value.FieldBatchesByName.TryGetValue(this.BatchName, out var batch))
            {
                throw new ArgumentException($"Field batch did not exist {this.BatchName} in protocol {protoGen.Key}");
            }
            var index = this.ObjectGen.Fields.IndexOf(this);
            if (index == -1)
            {
                throw new ArgumentException("Could not find self in object's field list.");
            }
            foreach (var generic in batch.Generics)
            {
                this.ObjectGen.Generics[generic.Key] = generic.Value;
            }
            foreach (var field in batch.Fields)
            {
                if (this.ObjectGen.LoadField(field.Node, true, out var typeGen))
                {
                    this.ObjectGen.Fields.Insert(index++, typeGen);
                    typeGen.Resolve();
                }
            }
            if (!this.ObjectGen.Fields.Remove(this))
            {
                throw new ArgumentException("Could not remove self from object's field list.");
            }
        }

        public override void GenerateForEquals(FileGeneration fg, string rhsAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForHash(FileGeneration fg, string hashResultAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateForEqualsMask(FileGeneration fg, string accessor, string rhsAccessor, string retAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateToString(FileGeneration fg, string name, string accessor, string fgAccessor)
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
    }
}
