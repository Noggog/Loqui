using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Noggog;

namespace Noggolloquy.Generation
{
    public class NoggolloquyGenerator
    {
        private const string CSPROJ_NAMESPACE = "http://schemas.microsoft.com/developer/msbuild/2003";
        Dictionary<ProtocolDefinition, ProtocolGeneration> targetData = new Dictionary<ProtocolDefinition, ProtocolGeneration>();
        Dictionary<ushort, ProtocolDefinition> idUsageDict = new Dictionary<ushort, ProtocolDefinition>();
        List<FileInfo> projectsToModify = new List<FileInfo>();
        private List<DirectoryInfo> addedTargetDirs = new List<DirectoryInfo>();
        Dictionary<StringCaseAgnostic, Type> typeDict = new Dictionary<StringCaseAgnostic, Type>();
        public string DefaultNamespace;
        public List<GenerationInterface> GenerationInterfaces = new List<GenerationInterface>();
        public List<GenerationModule> GenerationModules = new List<GenerationModule>();
        public DirectoryInfo CommonGenerationFolder;
        public Dictionary<StringCaseAgnostic, List<ObjectGeneration>> ObjectGenerationsByDir = new Dictionary<StringCaseAgnostic, List<ObjectGeneration>>();
        public HashSet<StringCaseAgnostic> GeneratedFiles = new HashSet<StringCaseAgnostic>();
        public static string Namespace { get { return "http://tempuri.org/NoggolloquySource.xsd"; } }

        public NoggolloquyGenerator(DirectoryInfo commonGenerationFolder)
        {
            this.CommonGenerationFolder = commonGenerationFolder;
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
            AddTypeAssociation<RangeIntType>("RangeInt");
            AddTypeAssociation<RangeIntNullType>("RangeIntNull");
            AddTypeAssociation<RangeDoubleType>("RangeDouble");
            AddTypeAssociation<RangeDoubleNullType>("RangeDoubleNull");
            AddTypeAssociation<PercentType>("Percent");
            AddTypeAssociation<FloatType>("Float");
            AddTypeAssociation<FloatNullType>("FloatNull");
            AddTypeAssociation<UDoubleType>("UDouble");
            AddTypeAssociation<UDoubleNullType>("UDoubleNull");
            AddTypeAssociation<DoubleType>("Double");
            AddTypeAssociation<DoubleNullType>("DoubleNull");
            AddTypeAssociation<LevType>("Lev");
            AddTypeAssociation<ListType>("List");
            AddTypeAssociation<DictType>("Dict");
            AddTypeAssociation<Array2DType>("Array2D");
            AddTypeAssociation<EnumType>("Enum");
            AddTypeAssociation<StringType>("String");
            AddTypeAssociation<NonExportedObjectType>("NonExportedObject");
            AddTypeAssociation<WildcardType>("Wildcard");
        }

        public void AddTypeAssociation<T>(StringCaseAgnostic key, bool overrideExisting = false)
            where T : TypeGeneration
        {
            if (!overrideExisting && typeDict.ContainsKey(key))
            {
                throw new ArgumentException("Cannot add two type associations on the same key: " + key);
            }

            typeDict[key] = typeof(T);
        }

        public void AddProtocol(ProtocolGeneration protoGen)
        {
            this.targetData[protoGen.Definition] = protoGen;
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
            foreach (var dir in dirs)
            {
                addedTargetDirs.Add(dir);
                dir.Refresh();
                AddSpecificFile(dir.EnumerateFiles().ToArray());
            }
        }

        public void AddSpecificFile(params FileInfo[] files)
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
                        var levNode = t.Item1.Element(XName.Get("Noggolloquy", NoggolloquyGenerator.Namespace));
                        if (levNode == null) return false;
                        var protoNode = levNode.Element(XName.Get("Protocol", NoggolloquyGenerator.Namespace));

                        ushort protoID, version;
                        if (!protoNode.TryGetAttribute<ushort>("ProtocolID", out protoID)
                            || !protoNode.TryGetAttribute<ushort>("Version", out version))
                        {
                            throw new ArgumentException();
                        }

                        return protocolGen.Definition.Key.Equals(
                            new ProtocolKey(protoID));
                    }));

                if (protocolGen.Empty) return;

                idUsageDict[protocolGen.Definition.Key.ProtocolID] = protocolGen.Definition;
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

        public void Generate()
        {
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
                        throw new ArgumentException($"Two objects in protocol {proto.Definition} have the same ID {obj.ID.Value}");
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
            Type t;
            if (!typeDict.TryGetValue(name, out t))
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

        private void ModifyProject(FileInfo projFile)
        {
            XDocument doc;
            using (var stream = new FileStream(projFile.FullName, FileMode.Open))
            {
                doc = XDocument.Load(stream);
            }
            bool modified = false;
            var projNode = doc.Element(XName.Get("Project", CSPROJ_NAMESPACE));
            foreach (var compile in projNode.Elements(XName.Get("ItemGroup", CSPROJ_NAMESPACE))
                .SelectMany((itemGroup) => itemGroup.Elements(XName.Get("Compile", CSPROJ_NAMESPACE))))
            {
                XAttribute includeAttr;
                if (!compile.TryGetAttribute("Include", out includeAttr)) continue;
                FileInfo file = new FileInfo(projFile.Directory.FullName + "/" + includeAttr.Value);
                ObjectGeneration objGen;
                if (!TryGetMatchingObjectGeneration(file, out objGen)) continue;
                if (file.Name.EqualsIgnoreCase(objGen.SourceXMLFile.Name)) continue;
                var depName = XName.Get("DependentUpon", CSPROJ_NAMESPACE);
                if (compile.Element(depName) != null) continue;

                var depElem = new XElement(depName);
                depElem.Value = objGen.SourceXMLFile.Name;
                compile.Add(depElem);
                modified = true;
            }

            if (!modified) return;

            using (XmlTextWriter writer = new XmlTextWriter(
                new FileStream(projFile.FullName, FileMode.Create), Encoding.ASCII))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 3;
                doc.WriteTo(writer);
            }
        }

        public bool TryGetMatchingObjectGeneration(FileInfo csFile, out ObjectGeneration objGen)
        {
            List<ObjectGeneration> objs;
            if (this.ObjectGenerationsByDir.TryGetValue(csFile.Directory.FullName, out objs))
            {
                foreach (var obj in objs)
                {
                    if (csFile.Name.Equals($"{obj.Name}.cs")
                        || csFile.Name.Equals($"{obj.Name}_{ObjectGeneration.AUTOGENERATED}.cs"))
                    {
                        objGen = obj;
                        return true;
                    }
                }
            }

            objGen = null;
            return false;
        }
    }
}