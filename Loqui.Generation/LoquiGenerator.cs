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
        Dictionary<ProtocolKey, ProtocolGeneration> _protocols = new Dictionary<ProtocolKey, ProtocolGeneration>();
        public IReadOnlyDictionary<ProtocolKey, ProtocolGeneration> Protocols => _protocols;
        private HashSet<DirectoryPath> addedTargetDirs = new HashSet<DirectoryPath>();
        Dictionary<string, Type> nameToTypeDict = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        Dictionary<Type, Type> typeDict = new Dictionary<Type, Type>();
        public string DefaultNamespace;
        public List<GenerationInterface> GenerationInterfaces = new List<GenerationInterface>();
        public List<IGenerationModule> GenerationModules = new List<IGenerationModule>();
        public Dictionary<DirectoryPath, List<ObjectGeneration>> ObjectGenerationsByDir = new Dictionary<DirectoryPath, List<ObjectGeneration>>();
        public IEnumerable<ObjectGeneration> ObjectGenerations => this.ObjectGenerationsByDir.Values.SelectMany((v) => v);
        public Dictionary<ObjectNamedKey, ObjectGeneration> ObjectGenerationsByObjectNameKey = new Dictionary<ObjectNamedKey, ObjectGeneration>();
        public HashSet<FilePath> GeneratedFiles = new HashSet<FilePath>();
        public static string Namespace => "http://tempuri.org/LoquiSource.xsd";
        public List<string> Namespaces = new List<string>();
        public LoquiInterfaceType SetterInterfaceTypeDefault = LoquiInterfaceType.Direct;
        public LoquiInterfaceType GetterInterfaceTypeDefault = LoquiInterfaceType.IGetter;
        public List<string> Interfaces = new List<string>();
        public PermissionLevel SetPermissionDefault;
        public RxBaseOption RxBaseOptionDefault;
        public bool DerivativeDefault;
        public NotifyingType NotifyingDefault;
        public bool NullableDefault;
        public bool ToStringDefault = true;
        public bool NthReflectionDefault = false;
        public ProtocolKey ProtocolDefault;
        public MaskModule MaskModule = new MaskModule();

        public LoquiGenerator(DirectoryInfo commonGenerationFolder = null, bool typical = true)
        {
            if (typical)
            {
                this.AddTypicalTypeAssociations();
                this.Add(MaskModule);
                this.Add(new ReactiveModule());
            }
        }

        public void AddTypicalTypeAssociations()
        {
            AddTypeAssociation<Int8Type>("Int8");
            AddTypeAssociation<Int16Type>("Int16");
            AddTypeAssociation<Int32Type>("Int32");
            AddTypeAssociation<Int64Type>("Int64");
            AddTypeAssociation<UInt8Type>("UInt8");
            AddTypeAssociation<UInt16Type>("UInt16");
            AddTypeAssociation<UInt32Type>("UInt32");
            AddTypeAssociation<UInt64Type>("UInt64");
            AddTypeAssociation<P2Int32Type>("P2Int32");
            AddTypeAssociation<P2Int16Type>("P2Int16");
            AddTypeAssociation<P3UInt16Type>("P3UInt16");
            AddTypeAssociation<P3IntType>("P3Int32");
            AddTypeAssociation<P3Int16Type>("P3Int16");
            AddTypeAssociation<P2FloatType>("P2Float");
            AddTypeAssociation<P3FloatType>("P3Float");
            AddTypeAssociation<P3DoubleType>("P3Double");
            AddTypeAssociation<BoolType>("Bool");
            AddTypeAssociation<CharType>("Char");
            AddTypeAssociation<TypicalRangedIntType<RangeInt8>>("RangeInt8");
            AddTypeAssociation<TypicalRangedIntType<RangeInt16>>("RangeInt16");
            AddTypeAssociation<TypicalRangedIntType<RangeInt32>>("RangeInt32");
            AddTypeAssociation<TypicalRangedIntType<RangeInt64>>("RangeInt64");
            AddTypeAssociation<TypicalRangedIntType<RangeUInt8>>("RangeUInt8");
            AddTypeAssociation<TypicalRangedIntType<RangeUInt16>>("RangeUInt16");
            AddTypeAssociation<TypicalRangedIntType<RangeUInt32>>("RangeUInt32");
            AddTypeAssociation<TypicalRangedIntType<RangeUInt64>>("RangeUInt64");
            AddTypeAssociation<RangeDoubleType>("RangeDouble");
            AddTypeAssociation<PercentType>("Percent");
            AddTypeAssociation<FloatType>("Float");
            AddTypeAssociation<UDoubleType>("UDouble");
            AddTypeAssociation<DoubleType>("Double");
            AddTypeAssociation<LoquiType>("Ref");
            AddTypeAssociation<LoquiType>("RefDirect");
            AddTypeAssociation<ListType>("RefList");
            AddTypeAssociation<ListType>("List");
            AddTypeAssociation<ArrayType>("Array");
            AddTypeAssociation<DictType>("Dict");
            AddTypeAssociation<EnumType>("Enum");
            AddTypeAssociation<StringType>("String");
            AddTypeAssociation<FilePathType>("FilePath");
            AddTypeAssociation<DirectoryPathType>("DirectoryPath");
            AddTypeAssociation<FieldBatchPointerType>("FieldBatch");
            AddTypeAssociation<DateTimeType>("DateTime");
            AddTypeAssociation<ByteArrayType>("ByteArray");
            AddTypeAssociation<NothingType>("Nothing");
            AddTypeAssociation<ColorType>("Color");
        }

        public void AddTypeAssociation<T>(string key, bool overrideExisting = false)
            where T : TypeGeneration
        {
            if (!overrideExisting && nameToTypeDict.ContainsKey(key))
            {
                throw new ArgumentException($"Cannot add two type associations on the same key: {key}");
            }

            nameToTypeDict[key] = typeof(T);
            typeDict[typeof(T)] = typeof(T);
        }

        public void ReplaceTypeAssociation<Target, Replacement>()
            where Target : TypeGeneration
            where Replacement : TypeGeneration
        {
            Type t = typeof(Target);
            var matching = this.nameToTypeDict.Where((kv) => kv.Value.Equals(t)).ToList();
            if (!matching.Any())
            {
                throw new ArgumentException($"No matching types of type {t}");
            }
            foreach (var item in matching)
            {
                AddTypeAssociation<Replacement>(item.Key, overrideExisting: true);
            }
            typeDict[typeof(Target)] = typeof(Replacement);
            foreach (var module in this.GenerationModules
                .WhereCastable<IGenerationModule, ITranslationModule>())
            {
                module.ReplaceTypeAssociation<Target, Replacement>();
            }
        }

        public ProtocolGeneration AddProtocol(ProtocolGeneration protoGen)
        {
            this._protocols[protoGen.Protocol] = protoGen;
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
            return this._protocols.TryGetValue(protocol, out protoGen);
        }

        public async Task Generate()
        {
            await Task.WhenAll(this._protocols.Values.Select((p) => p.LoadInitialObjects()));


            await Task.WhenAll(this.GenerationModules.Select((m) => m.Modify(this)));

            ResolveIDs();

            await Task.WhenAll(
                this._protocols.Values
                    .Select((protoGen) => protoGen.Generate()));

            DeleteOldAutogenerated();
        }

        private void ResolveIDs()
        {
            foreach (var proto in this._protocols.Values)
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

        public bool TryGetTypeGeneration(string name, out TypeGeneration gen)
        {
            if (!nameToTypeDict.TryGetValue(name, out Type t)
                || !typeDict.TryGetValue(t, out var outT))
            {
                gen = null;
                return false;
            }
            gen = Activator.CreateInstance(outT) as TypeGeneration;
            return true;
        }

        public bool TryGetTypeGeneration<T>(out T gen)
            where T : TypeGeneration
        {
            if (!typeDict.TryGetValue(typeof(T), out Type t))
            {
                gen = null;
                return false;
            }

            gen = Activator.CreateInstance(t) as T;
            return true;
        }

        public T GetTypeGeneration<T>()
            where T : TypeGeneration
        {
            var t = typeDict[typeof(T)];

            return Activator.CreateInstance(t) as T;
        }

        private void DeleteOldAutogenerated()
        {
            foreach (var dir in addedTargetDirs)
            {
                foreach (var file in dir.EnumerateFiles())
                {
                    if (file.Name.String.Contains(Constants.AutogeneratedMarkerString)
                        && file.Name.Extension != ".meta"
                        && !this.GeneratedFiles.Contains(file))
                    {
                        file.Delete();
                    }
                }
            }
        }

        public bool TryGetMatchingObjectGeneration(FilePath file, out ObjectGeneration objGen)
        {
            if (file.Directory.TryGet(out var dir) 
                && this.ObjectGenerationsByDir.TryGetValue(dir, out List<ObjectGeneration> objs))
            {
                objGen = objs.Where((obj) => file.Name.String.StartsWith(obj.Name))
                    .OrderByDescending((obj) => obj.Name.Length)
                    .FirstOrDefault();
                return objGen != null;
            }

            objGen = null;
            return false;
        }
    }
}
