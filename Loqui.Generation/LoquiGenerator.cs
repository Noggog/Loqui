using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Noggog;

namespace Loqui.Generation
{
    public class LoquiGenerator
    {
        private const string CSPROJ_NAMESPACE = "http://schemas.microsoft.com/developer/msbuild/2003";
        private List<DirectoryInfo> sourceFolders = new List<DirectoryInfo>();
        private HashSet<FileInfo> sourceFiles = new HashSet<FileInfo>();
        Dictionary<ProtocolKey, ProtocolGeneration> targetData = new Dictionary<ProtocolKey, ProtocolGeneration>();
        List<FileInfo> projectsToModify = new List<FileInfo>();
        private List<DirectoryInfo> addedTargetDirs = new List<DirectoryInfo>();
        Dictionary<StringCaseAgnostic, Type> typeDict = new Dictionary<StringCaseAgnostic, Type>();
        public string DefaultNamespace;
        public List<GenerationInterface> GenerationInterfaces = new List<GenerationInterface>();
        public List<GenerationModule> GenerationModules = new List<GenerationModule>();
        public DirectoryInfo CommonGenerationFolder;
        public Dictionary<DirectoryPath, List<ObjectGeneration>> ObjectGenerationsByDir = new Dictionary<DirectoryPath, List<ObjectGeneration>>();
        public HashSet<StringCaseAgnostic> GeneratedFiles = new HashSet<StringCaseAgnostic>();
        public static string Namespace => "http://tempuri.org/LoquiSource.xsd";
        public List<string> Namespaces = new List<string>();
        public LoquiInterfaceType InterfaceTypeDefault = LoquiInterfaceType.Direct;
        public bool ProtectedDefault;
        public bool DerivativeDefault;
        public NotifyingOption NotifyingDefault = NotifyingOption.None;
        public bool RaisePropertyChangedDefault;
        public ProtocolKey ProtocolDefault;
        public MaskModule MaskModule = new MaskModule();
        public XmlTranslationModule XmlTranslation;

        public LoquiGenerator(DirectoryInfo commonGenerationFolder, bool typical = true)
        {
            this.CommonGenerationFolder = commonGenerationFolder;
            if (typical)
            {
                this.AddTypicalTypeAssociations();
                this.XmlTranslation = new XmlTranslationModule(this);
                this.Add(this.XmlTranslation);
                this.Add(MaskModule);
                this.AddSearchableFolder(this.CommonGenerationFolder);
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
            AddTypeAssociation<P2IntType>("P2Int");
            AddTypeAssociation<P2IntNullType>("P2IntNull");
            AddTypeAssociation<P3IntType>("P3Int");
            AddTypeAssociation<P3IntNullType>("P3IntNull");
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

        public void AddProtocol(ProtocolGeneration protoGen)
        {
            this.targetData[protoGen.Protocol] = protoGen;
        }

        public void AddProjectToModify(FileInfo projFile)
        {
            this.projectsToModify.Add(projFile);
        }

        public void AddSearchableFolder(DirectoryInfo dir)
        {
            AddSpecificFolders(dir);
            foreach (var d in dir.GetDirectories())
            {
                AddSearchableFolder(d);
            }
        }

        public void AddSpecificFolders(params DirectoryInfo[] dirs)
        {
            this.sourceFolders.AddRange(dirs);
        }

        protected void LoadSpecificFolders(IEnumerable<DirectoryInfo> dirs)
        {
            foreach (var dir in dirs)
            {
                addedTargetDirs.Add(dir);
                dir.Refresh();
                LoadSpecificFile(dir.EnumerateFiles().ToArray());
            }
        }

        public void AddSpecificFile(params FileInfo[] files)
        {
            this.sourceFiles.Add(files);
        }

        protected void LoadSpecificFile(IEnumerable<FileInfo> files)
        {
            foreach (var protocolGen in this.targetData.Values)
            {
                protocolGen.LoadInitialObjects(files
                    .Where((f) => ".XML".EqualsIgnoreCase(f.Extension))
                    .Select(
                    (f) =>
                    {
                        XDocument doc;
                        using (var stream = new FileStream(f.FullName, FileMode.Open))
                        {
                            doc = XDocument.Load(stream);
                        }
                        return new System.Tuple<XDocument, FileInfo>(doc, f);
                    })
                    .Where((t) =>
                    {
                        var loquiNode = t.Item1.Element(XName.Get("Loqui", LoquiGenerator.Namespace));
                        if (loquiNode == null) return false;
                        var protoNode = loquiNode.Element(XName.Get("Protocol", LoquiGenerator.Namespace));
                        string nameSpace;
                        if (protoNode == null
                            && !string.IsNullOrWhiteSpace(this.ProtocolDefault.Namespace))
                        {
                            nameSpace = this.ProtocolDefault.Namespace;
                        }
                        else
                        {
                            if (!protoNode.TryGetAttribute("Namespace", out nameSpace))
                            {
                                throw new ArgumentException();
                            }
                        }

                        return protocolGen.Protocol.Namespace.Equals(nameSpace);
                    }));
            }
        }

        public void Add(GenerationModule transl)
        {
            GenerationModules.Add(transl);
        }

        public void Add(GenerationInterface interf)
        {
            GenerationInterfaces.Add(interf);
        }

        public bool TryGetProtocol(ProtocolKey protocol, out ProtocolGeneration protoGen)
        {
            return this.targetData.TryGetValue(protocol, out protoGen);
        }

        public void Generate()
        {
            this.LoadSpecificFolders(this.sourceFolders);
            this.LoadSpecificFile(this.sourceFiles);

            foreach (var mod in this.GenerationModules)
            {
                mod.Modify(this);
            }

            ResolveIDs();

            foreach (var protoGen in this.targetData.Values)
            {
                protoGen.Generate();
            }

            foreach (var file in this.projectsToModify)
            {
                ModifyProject(file);
            }

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
                foreach (var file in dir.GetFiles())
                {
                    if (file.Name.Contains(ObjectGeneration.AUTOGENERATED)
                        && !file.Name.EndsWith(".meta")
                        && !this.GeneratedFiles.Contains(file.FullName))
                    {
                        file.Delete();
                    }
                }
            }
        }

        private void ModifyProject(FilePath projFile)
        {
            XDocument doc;
            using (var stream = new FileStream(projFile.Path, FileMode.Open))
            {
                doc = XDocument.Load(stream);
            }
            bool modified = false;
            var projNode = doc.Element(XName.Get("Project", CSPROJ_NAMESPACE));
            List<XElement> includeNodes = projNode.Elements(XName.Get("ItemGroup", CSPROJ_NAMESPACE)).ToList();
            List<XElement> compileGroupNodes = includeNodes
                .Where((group) => group.Elements().Any((e) => e.Name.LocalName.Equals("Compile")))
                .ToList();
            List<XElement> noneGroupNodes = includeNodes
                .Where((group) => group.Elements().Any((e) => e.Name.LocalName.Equals("None")))
                .ToList();
            var compileNodes = compileGroupNodes
                .SelectMany((itemGroup) => itemGroup.Elements(XName.Get("Compile", CSPROJ_NAMESPACE)))
                .ToList();
            var noneNodes = compileGroupNodes
                .SelectMany((itemGroup) => itemGroup.Elements(XName.Get("None", CSPROJ_NAMESPACE)))
                .ToList();

            XElement compileIncludeNode;
            if (compileGroupNodes.Count == 0)
            {
                compileIncludeNode = new XElement("ItemGroup", CSPROJ_NAMESPACE);
                projNode.Add(compileIncludeNode);
                compileGroupNodes.Add(compileIncludeNode);
            }
            else
            {
                compileIncludeNode = compileGroupNodes.First();
            }

            Lazy<XElement> noneIncludeNode = new Lazy<XElement>(() =>
            {
                if (noneGroupNodes.Count == 0)
                {
                    var ret = new XElement("ItemGroup", CSPROJ_NAMESPACE);
                    projNode.Add(ret);
                    noneGroupNodes.Add(ret);
                    return ret;
                }
                else
                {
                    return noneGroupNodes.First();
                }
            });

            Dictionary<FilePath, ProjItemType> generatedItems = new Dictionary<FilePath, ProjItemType>();
            generatedItems.Set(this.ObjectGenerationsByDir.SelectMany((kv) => kv.Value).Select((objGen) => objGen.GeneratedFiles).SelectMany((d) => d));
            HashSet<FilePath> sourceXMLs = new HashSet<FilePath>(this.ObjectGenerationsByDir.SelectMany(kv => kv.Value).Select((objGen) => new FilePath(objGen.SourceXMLFile.FullName)));

            // Find which objects are present
            foreach (var subNode in includeNodes.SelectMany((n) => n.Elements()))
            {
                XAttribute includeAttr = subNode.Attribute("Include");
                if (includeAttr == null) continue;
                generatedItems.Remove(Path.Combine(projFile.Directory.Path, includeAttr.Value));
            }

            // Add missing object nodes
            foreach (var objGens in generatedItems)
            {
                if (objGens.Key.Directory.IsSubfolderOf(projFile.Directory)
                    || objGens.Key.Directory.Equals(projFile.Directory))
                {
                    string filePath = objGens.Key.Path.TrimStart(projFile.Directory.Path);
                    filePath = filePath.TrimStart('\\');
                    List<XElement> nodes;
                    XElement includeNode;
                    string nodeName;
                    switch (objGens.Value)
                    {
                        case ProjItemType.None:
                            nodes = noneNodes;
                            includeNode = noneIncludeNode.Value;
                            nodeName = "None";
                            break;
                        case ProjItemType.Compile:
                            nodes = compileNodes;
                            includeNode = compileIncludeNode;
                            nodeName = "Compile";
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    var compileElem = new XElement(XName.Get(nodeName, CSPROJ_NAMESPACE),
                        new XAttribute("Include", filePath));
                    nodes.Add(compileElem);
                    includeNode.Add(compileElem);
                    modified = true;
                }
            }

            // Add dependent files underneath
            var depName = XName.Get("DependentUpon", CSPROJ_NAMESPACE);
            foreach (var subMode in includeNodes.SelectMany((n) => n.Elements()))
            {
                XAttribute includeAttr = subMode.Attribute("Include");
                if (includeAttr == null) continue;
                FilePath file = new FilePath(Path.Combine(projFile.Directory.Path, includeAttr.Value));
                if (sourceXMLs.Contains(file)) continue;
                if (!TryGetMatchingObjectGeneration(file, out ObjectGeneration objGen)) continue;
                if (file.Name.Equals(objGen.SourceXMLFile.Name)) continue;
                if (subMode.Element(depName) != null) continue;

                var depElem = new XElement(depName)
                {
                    Value = objGen.SourceXMLFile.Name
                };
                subMode.Add(depElem);
                modified = true;
            }

            // Add Protocol Definition
            HashSet<ProtocolKey> foundProtocols = new HashSet<ProtocolKey>();
            foreach (var compile in compileNodes)
            {
                XAttribute includeAttr = compile.Attribute("Include");
                if (includeAttr == null) continue;
                foreach (var proto in this.targetData)
                {
                    if (includeAttr.Value.ToLower().Contains(proto.Value.ProtocolDefinitionName.ToLower()))
                    {
                        foundProtocols.Add(proto.Key);
                    }
                }
            }
            foreach (var proto in this.targetData)
            {
                if (!foundProtocols.Add(proto.Key)) continue;
                var compileElem = new XElement(XName.Get("Compile", CSPROJ_NAMESPACE),
                    new XAttribute("Include", proto.Value.ProtocolDefinitionName + ".cs"));
                compileNodes.Add(compileElem);
                compileIncludeNode.Add(compileElem);
                modified = true;
            }

            if (!modified) return;

            using (XmlTextWriter writer = new XmlTextWriter(
                new FileStream(projFile.Path, FileMode.Create), Encoding.ASCII))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 2;
                doc.WriteTo(writer);
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