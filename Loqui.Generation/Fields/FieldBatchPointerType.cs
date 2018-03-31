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
        public string ProtocolID { get; private set; }
        public override bool IsEnumerable => throw new ArgumentException();

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

        public override void GenerateSetNth(FileGeneration fg, string accessorPrefix, string rhsAccessorPrefix, string cmdsAccessor, bool internalUse)
        {
            throw new NotImplementedException();
        }

        public override void GenerateSetNthHasBeenSet(FileGeneration fg, string identifier, string onIdentifier)
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

        public override async Task Load(XElement node, bool requireName = true)
        {
            this._derivative = false;
            this.BatchName = node.GetAttribute<string>("name", throwException: true);
            this.ProtocolID = node.GetAttribute("protocol", throwException: false);
        }

        public override async Task Resolve()
        {
            await base.Resolve();
            var protoID = this.ProtocolID ?? this.ObjectGen.ProtoGen.Protocol.Namespace;
            if (!this.ObjectGen.ProtoGen.Gen.TryGetProtocol(new ProtocolKey(protoID), out var protoGen))
            {
                throw new ArgumentException($"Protocol did not exist {protoID}.");
            }
            if (!protoGen.FieldBatchesByName.TryGetValue(this.BatchName, out var batch))
            {
                throw new ArgumentException($"Field batch did not exist {this.BatchName} in protocol {protoGen.Protocol.Namespace}");
            }
            var index = this.ObjectGen.IterateFields().ToList().IndexOf(this);
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
                var typeGen = await this.ObjectGen.LoadField(field.Node, requireName: true);
                if (typeGen.Succeeded)
                {
                    this.ObjectGen.Fields.Insert(index++, typeGen.Value);
                    await typeGen.Value.Resolve();
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

        public override void GenerateForEqualsMask(FileGeneration fg, Accessor accessor, Accessor rhsAccessor, string retAccessor)
        {
            throw new NotImplementedException();
        }

        public override void GenerateToString(FileGeneration fg, string name, Accessor accessor, string fgAccessor)
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

        public override bool IsNullable()
        {
            throw new NotImplementedException();
        }
    }
}
