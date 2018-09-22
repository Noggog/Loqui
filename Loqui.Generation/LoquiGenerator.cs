using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Noggog;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class LoquiGenerator
    {
        Dictionary<ProtocolKey, ProtocolGeneration> targetData = new Dictionary<ProtocolKey, ProtocolGeneration>();
        private HashSet<DirectoryPath> addedTargetDirs = new HashSet<DirectoryPath>();
        Dictionary<StringCaseAgnostic, Type> typeDict = new Dictionary<StringCaseAgnostic, Type>();
        public string DefaultNamespace;
        public List<GenerationInterface> GenerationInterfaces = new List<GenerationInterface>();
        public List<IGenerationModule> GenerationModules = new List<IGenerationModule>();
        public Dictionary<DirectoryPath, List<ObjectGeneration>> ObjectGenerationsByDir = new Dictionary<DirectoryPath, List<ObjectGeneration>>();
        public IEnumerable<ObjectGeneration> ObjectGenerations => this.ObjectGenerationsByDir.Values.SelectMany((v) => v);
        public Dictionary<ObjectNamedKey, ObjectGeneration> ObjectGenerationsByObjectNameKey = new Dictionary<ObjectNamedKey, ObjectGeneration>();
        public HashSet<FilePath> GeneratedFiles = new HashSet<FilePath>();
        public static string Namespace => "http://tempuri.org/LoquiSource.xsd";
        public List<string> Namespaces = new List<string>();
        public LoquiInterfaceType InterfaceTypeDefault = LoquiInterfaceType.Direct;
        public List<string> Interfaces = new List<string>();
        public bool ProtectedDefault;
        public bool DerivativeDefault;
        public NotifyingType NotifyingDefault;
        public bool HasBeenSetDefault;
        public bool ToStringDefault = true;
        public bool ObjectCentralizedDefault = true;
        public bool RaisePropertyChangedDefault;
        public ProtocolKey ProtocolDefault;
        public MaskModule MaskModule = new MaskModule();
        public ObjectCentralizationModule ObjectCentralizationModule = new ObjectCentralizationModule();
        public XmlTranslationModule XmlTranslation;

        public LoquiGenerator(DirectoryInfo commonGenerationFolder = null, bool typical = true)
        {
            if (typical)
            {
                this.AddTypicalTypeAssociations();
                this.XmlTranslation = new XmlTranslationModule(this);
                this.Add(this.XmlTranslation);
                this.Add(MaskModule);
                this.Add(ObjectCentralizationModule);
            }
        }

        public void AddTypicalTypeAssociations()
        {
            AddTypeAssociation<Int8Type>("Int8");
            AddTypeAssociation<Int8NullType>("Int8Null");
            AddTypeAssociation<Int16Type>("Int16");
            AddTypeAssociation<Int16NullType>("Int16Null");
            AddTypeAssociation<Int32Type>("Int32");
            AddTypeAssociation<Int32NullType>("Int32Null");
            AddTypeAssociation<Int64Type>("Int64");
            AddTypeAssociation<Int64NullType>("Int64Null");
            AddTypeAssociation<UInt8Type>("UInt8");
            AddTypeAssociation<UInt8NullType>("UInt8Null");
            AddTypeAssociation<UInt16Type>("UInt16");
            AddTypeAssociation<UInt16NullType>("UInt16Null");
            AddTypeAssociation<UInt32Type>("UInt32");
            AddTypeAssociation<UInt32NullType>("UInt32Null");
            AddTypeAssociation<UInt64Type>("UInt64");
            AddTypeAssociation<UInt64NullType>("UInt64Null");
            AddTypeAssociation<P2Int32Type>("P2Int32");
            AddTypeAssociation<P2Int32NullType>("P2Int32Null");
            AddTypeAssociation<P2Int16Type>("P2Int16");
            AddTypeAssociation<P2Int16NullType>("P2Int16Null");
            AddTypeAssociation<P3UInt16Type>("P3UInt16");
            AddTypeAssociation<P3UInt16NullType>("P3UInt16Null");
            AddTypeAssociation<P3IntType>("P3Int32");
            AddTypeAssociation<P3IntNullType>("P3Int32Null");
            AddTypeAssociation<P2FloatType>("P2Float");
            AddTypeAssociation<P2FloatNullType>("P2FloatNull");
            AddTypeAssociation<P3FloatType>("P3Float");
            AddTypeAssociation<P3FloatNullType>("P3FloatNull");
            AddTypeAssociation<P3DoubleType>("P3Double");
            AddTypeAssociation<P3DoubleNullType>("P3DoubleNull");
            AddTypeAssociation<BoolType>("Bool");
            AddTypeAssociation<BoolNullType>("BoolNull");
            AddTypeAssociation<CharType>("Char");
            AddTypeAssociation<CharNullType>("CharNull");
            AddTypeAssociation<TypicalRangedIntType<RangeInt8>>("RangeInt8");
            AddTypeAssociation<TypicalRangedIntType<RangeInt8?>>("RangeInt8Null");
            AddTypeAssociation<TypicalRangedIntType<RangeInt16>>("RangeInt16");
            AddTypeAssociation<TypicalRangedIntType<RangeInt16?>>("RangeInt16Null");
            AddTypeAssociation<TypicalRangedIntType<RangeInt32>>("RangeInt32");
            AddTypeAssociation<TypicalRangedIntType<RangeInt32?>>("RangeInt32Null");
            AddTypeAssociation<TypicalRangedIntType<RangeInt64>>("RangeInt64");
            AddTypeAssociation<TypicalRangedIntType<RangeInt64?>>("RangeInt64Null");
            AddTypeAssociation<TypicalRangedIntType<RangeUInt8>>("RangeUInt8");
            AddTypeAssociation<TypicalRangedIntType<RangeUInt8?>>("RangeUInt8Null");
            AddTypeAssociation<TypicalRangedIntType<RangeUInt16>>("RangeUInt16");
            AddTypeAssociation<TypicalRangedIntType<RangeUInt16?>>("RangeUInt16Null");
            AddTypeAssociation<TypicalRangedIntType<RangeUInt32>>("RangeUInt32");
            AddTypeAssociation<TypicalRangedIntType<RangeUInt32?>>("RangeUInt32Null");
            AddTypeAssociation<TypicalRangedIntType<RangeUInt64>>("RangeUInt64");
            AddTypeAssociation<TypicalRangedIntType<RangeUInt64?>>("RangeUInt64Null");
            AddTypeAssociation<RangeDoubleType>("RangeDouble");
            AddTypeAssociation<RangeDoubleNullType>("RangeDoubleNull");
            AddTypeAssociation<PercentType>("Percent");
            AddTypeAssociation<PercentNullType>("PercentNull");
            AddTypeAssociation<FloatType>("Float");
            AddTypeAssociation<FloatNullType>("FloatNull");
            AddTypeAssociation<UDoubleType>("UDouble");
            AddTypeAssociation<UDoubleNullType>("UDoubleNull");
            AddTypeAssociation<DoubleType>("Double");
            AddTypeAssociation<DoubleNullType>("DoubleNull");
            AddTypeAssociation<LoquiType>("Ref");
            AddTypeAssociation<LoquiType>("RefDirect");
            AddTypeAssociation<LoquiListType>("RefList");
            AddTypeAssociation<ListType>("List");
            AddTypeAssociation<DictType>("Dict");
            AddTypeAssociation<EnumType>("Enum");
            AddTypeAssociation<EnumNullType>("EnumNull");
            AddTypeAssociation<StringType>("String");
            AddTypeAssociation<FilePathType>("FilePath");
            AddTypeAssociation<DirectoryPathType>("DirectoryPath");
            AddTypeAssociation<FilePathNullType>("FilePathNull");
            AddTypeAssociation<DirectoryPathNullType>("DirectoryPathNull");
            AddTypeAssociation<UnsafeType>("UnsafeObject");
            AddTypeAssociation<WildcardType>("Wildcard");
            AddTypeAssociation<FieldBatchPointerType>("FieldBatch");
            AddTypeAssociation<DateTimeType>("DateTime");
            AddTypeAssociation<DateTimeNullType>("DateTimeNull");
            AddTypeAssociation<ByteArrayType>("ByteArray");
            AddTypeAssociation<NothingType>("Nothing");
        }

        public void AddTypeAssociation<T>(StringCaseAgnostic key, bool overrideExisting = false)
            where T : TypeGeneration
        {
            if (!overrideExisting && typeDict.ContainsKey(key))
            {
                throw new ArgumentException($"Cannot add two type associations on the same key: {key}");
            }

            typeDict[key] = typeof(T);
        }

        public void ReplaceTypeAssociation<Target, Replacement>()
            where Target : TypeGeneration
            where Replacement : TypeGeneration
        {
            Type t = typeof(Target);
            var matching = this.typeDict.Where((kv) => kv.Value.Equals(t)).ToList();
            if (!matching.Any())
            {
                throw new ArgumentException($"No matching types of type {t}");
            }
            foreach (var item in matching)
            {
                AddTypeAssociation<Replacement>(item.Key, overrideExisting: true);
            }
        }

        public ProtocolGeneration AddProtocol(ProtocolGeneration protoGen)
        {
            this.targetData[protoGen.Protocol] = protoGen;
            return protoGen;
        }

        public T Add<T>(T transl)
            where T : GenerationModule
        {
            GenerationModules.Add(transl);
            return transl;
        }

        public void Add(GenerationInterface interf)
        {
            GenerationInterfaces.Add(interf);
        }

        public bool TryGetProtocol(ProtocolKey protocol, out ProtocolGeneration protoGen)
        {
            return this.targetData.TryGetValue(protocol, out protoGen);
        }

        public async Task Generate()
        {
            await Task.WhenAll(this.targetData.Values.Select((p) => p.LoadInitialObjects()));


            await Task.WhenAll(this.GenerationModules.Select((m) => m.Modify(this)));

            ResolveIDs();

            await Task.WhenAll(
                this.targetData.Values
                    .Select((protoGen) => protoGen.Generate()
                        .TimeoutButContinue(
                            4000,
                            () => System.Console.WriteLine($"{protoGen} generation taking a long time."))));

            DeleteOldAutogenerated();
        }

        private void ResolveIDs()
        {
            foreach (var proto in this.targetData.Values)
            {
                HashSet<ushort> usedIDs = new HashSet<ushort>();
                foreach (var obj in proto.ObjectGenerationsByID.Values)
                {
                    if (!obj.ID.HasValue) continue;
                    if (!usedIDs.Add(obj.ID.Value))
                    {
                        throw new ArgumentException($"Two objects in protocol {proto.Protocol.Namespace} have the same ID {obj.ID.Value}");
                    }
                }

                ushort max = (ushort)(usedIDs.Count > 0 ? (usedIDs.Max() + 1) : 1);

                foreach (var obj in proto.ObjectGenerationsByID.Values)
                {
                    if (!obj.ID.HasValue)
                    {
                        obj.ID = max++;
                    }
                }
            }
        }

        public bool TryGetTypeGeneration(StringCaseAgnostic name, out TypeGeneration gen)
        {
            if (!typeDict.TryGetValue(name, out Type t))
            {
                gen = null;
                return false;
            }

            gen = Activator.CreateInstance(t) as TypeGeneration;
            return true;
        }

        private void DeleteOldAutogenerated()
        {
            foreach (var dir in addedTargetDirs)
            {
                foreach (var file in dir.EnumerateFiles())
                {
                    if (file.Name.Contains(ObjectGeneration.AUTOGENERATED)
                        && !file.Name.EndsWith(".meta")
                        && !this.GeneratedFiles.Contains(file))
                    {
                        file.Delete();
                    }
                }
            }
        }

        public bool TryGetMatchingObjectGeneration(FilePath file, out ObjectGeneration objGen)
        {
            if (this.ObjectGenerationsByDir.TryGetValue(file.Directory, out List<ObjectGeneration> objs))
            {
                objGen = objs.Where((obj) => file.Name.StartsWith(obj.Name))
                    .OrderByDescending((obj) => obj.Name.Length)
                    .FirstOrDefault();
                return objGen != null;
            }

            objGen = null;
            return false;
        }
    }
}