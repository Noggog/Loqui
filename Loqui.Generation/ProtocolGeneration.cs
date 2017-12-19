using Noggog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Text;

namespace Loqui.Generation
{
    public class ProtocolGeneration
    {
        private const string CSPROJ_NAMESPACE = "http://schemas.microsoft.com/developer/msbuild/2003";
        public ProtocolKey Protocol;
        public Dictionary<Guid, ObjectGeneration> ObjectGenerationsByID = new Dictionary<Guid, ObjectGeneration>();
        public Dictionary<StringCaseAgnostic, ObjectGeneration> ObjectGenerationsByName = new Dictionary<StringCaseAgnostic, ObjectGeneration>();
        public Dictionary<StringCaseAgnostic, FieldBatch> FieldBatchesByName = new Dictionary<StringCaseAgnostic, FieldBatch>();
        public bool Empty => ObjectGenerationsByID.Count == 0;
        public LoquiGenerator Gen { get; private set; }
        public DirectoryInfo GenerationFolder { get; private set; }
        public DirectoryInfo DefFileLocation => this.GenerationFolder;
        public LoquiInterfaceType InterfaceTypeDefault = LoquiInterfaceType.Direct;
        public bool ProtectedDefault;
        public bool DerivativeDefault;
        public string DefaultNamespace;
        public bool NotifyingDefault;
        public bool HasBeenSetDefault;
        public bool RaisePropertyChangedDefault = true;
        public string ProtocolDefinitionName => $"ProtocolDefinition_{this.Protocol.Namespace}";
        private HashSet<DirectoryPath> sourceFolders = new HashSet<DirectoryPath>();
        List<FileInfo> projectsToModify = new List<FileInfo>();

        public ProtocolGeneration(
            LoquiGenerator gen,
            ProtocolKey protocol,
            DirectoryInfo defSearchableFolder)
        {
            this.Protocol = protocol;
            this.Gen = gen;
            this.NotifyingDefault = gen.NotifyingDefault;
            this.HasBeenSetDefault = gen.HasBeenSetDefault;
            this.GenerationFolder = defSearchableFolder;
            this.InterfaceTypeDefault = gen.InterfaceTypeDefault;
            this.ProtectedDefault = gen.ProtectedDefault;
            this.DerivativeDefault = gen.DerivativeDefault;
            this.RaisePropertyChangedDefault = gen.RaisePropertyChangedDefault;
            this.AddSearchableFolder(defSearchableFolder);
        }

        private async Task LoadInitialObjects(IEnumerable<System.Tuple<XDocument, FileInfo>> xmlDocs)
        {
            List<ObjectGeneration> unassignedObjects = new List<ObjectGeneration>();

            // Parse IDs
            foreach (var xmlDocTuple in xmlDocs)
            {
                var xmlDoc = xmlDocTuple.Item1;
                XElement objNode = xmlDoc.Element(XName.Get("Loqui", LoquiGenerator.Namespace));

                string namespaceStr = this.DefaultNamespace ?? this.Gen.DefaultNamespace;
                XElement namespaceNode = objNode.Element(XName.Get("Namespace", LoquiGenerator.Namespace));
                if (namespaceNode != null)
                {
                    namespaceStr = namespaceNode.Value;
                }

                foreach (var batch in objNode.Elements(XName.Get("FieldBatch", LoquiGenerator.Namespace)))
                {
                    var fieldBatch = new FieldBatch(this.Gen);
                    fieldBatch.Load(batch);
                    this.FieldBatchesByName[fieldBatch.Name] = fieldBatch;
                }

                foreach (var obj in objNode.Elements(XName.Get("Object", LoquiGenerator.Namespace))
                    .And(objNode.Elements(XName.Get("Struct", LoquiGenerator.Namespace))))
                {
                    if (obj.GetAttribute<DisabledLevel>("disable", DisabledLevel.Enabled) == DisabledLevel.OmitEntirely) continue;
                    ObjectGeneration objGen;
                    if (obj.Name.LocalName.Equals("Object"))
                    {
                        objGen = new ClassGeneration(Gen, this, xmlDocTuple.Item2);
                    }
                    else
                    {
                        objGen = new StructGeneration(Gen, this, xmlDocTuple.Item2);
                    }
                    objGen.Node = obj;
                    if (!string.IsNullOrWhiteSpace(namespaceStr))
                    {
                        objGen.Namespace = namespaceStr;
                    }

                    var guid = obj.GetAttribute("GUID");
                    if (string.IsNullOrWhiteSpace(guid))
                    {
                        objGen.GUID = Guid.NewGuid();
                    }
                    else
                    {
                        objGen.GUID = new Guid(guid);
                    }

                    if (obj.TryGetAttribute<ushort>("ID", out ushort id))
                    {
                        objGen.ID = id;
                    }

                    if (this.ObjectGenerationsByID.ContainsKey(objGen.GUID))
                    {
                        throw new ArgumentException($"Two objects in the same protocol cannot have the same ID: {objGen.GUID}");
                    }
                    this.ObjectGenerationsByID.Add(objGen.GUID, objGen);

                    var nameNode = obj.Attribute("name");
                    if (nameNode == null)
                    {
                        throw new ArgumentException("Object must have a name");
                    }

                    string name = nameNode.Value;
                    if (this.ObjectGenerationsByName.ContainsKey(name))
                    {
                        throw new ArgumentException($"Two objects in the same protocol cannot have the same name: {name}");
                    }
                    objGen.Name = name;

                    foreach (var interf in Gen.GenerationInterfaces)
                    {
                        if (obj.GetAttribute(interf.KeyString, false))
                        {
                            objGen.GenerationInterfaces.Add(interf);
                        }
                    }

                    this.ObjectGenerationsByName.Add(name, objGen);
                    this.Gen.ObjectGenerationsByDir.TryCreateValue(objGen.TargetDir.FullName).Add(objGen);
                    this.Gen.ObjectGenerationsByObjectNameKey[new ObjectNamedKey(this.Protocol, objGen.Name)] = objGen;
                }
            }
        }

        public async Task Generate()
        {
            await Task.WhenAll(
                this.ObjectGenerationsByID.Values
                    .SelectMany((obj) => this.Gen.GenerationModules
                        .Select((m) => m.PreLoad(obj))));

            await Task.WhenAll(
                ObjectGenerationsByID.Values
                    .Select((obj) => TaskExt.DoThenComplete(obj.LoadingCompleteTask, obj.Load)));
            
            await Task.WhenAll(
                this.ObjectGenerationsByID.Values
                    .Select((obj) => obj.Resolve()));

            await Task.WhenAll(this.ObjectGenerationsByID.Values
                .Select(async (obj) =>
                {
                    await obj.Generate();
                    obj.RegenerateAndStampSourceXML();
                }));

            GenerateDefFile();

            foreach (var file in this.projectsToModify)
            {
                ModifyProject(file);
            }
        }

        private void GenerateDefFile()
        {
            HashSet<string> namespaces = new HashSet<string>
            {
                "Loqui"
            };
            foreach (var obj in this.ObjectGenerationsByID.Values)
            {
                namespaces.Add(obj.Namespace);
            }

            FileGeneration fg = new FileGeneration();
            foreach (var nameS in namespaces)
            {
                fg.AppendLine($"using {nameS};");
            }
            fg.AppendLine();

            fg.AppendLine("namespace Loqui");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"public class {this.ProtocolDefinitionName} : IProtocolRegistration");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"public readonly static ProtocolKey ProtocolKey = new ProtocolKey(\"{this.Protocol.Namespace}\");");
                    fg.AppendLine("public void Register()");
                    using (new BraceWrapper(fg))
                    {
                        foreach (var obj in this.ObjectGenerationsByID.Values
                            .OrderBy((o) => o.ID))
                        {
                            fg.AppendLine($"LoquiRegistration.Register({obj.InternalNamespace}.{obj.RegistrationName}.Instance);");
                        }
                    }
                }
            }

            fg.Generate(
                new FileInfo(
                    DefFileLocation.FullName
                    + $"/{this.ProtocolDefinitionName}.cs"));
        }

        public void AddSearchableFolder(DirectoryInfo dir)
        {
            if (dir == null) return;
            AddSpecificFolders(dir);
            foreach (var d in dir.GetDirectories())
            {
                AddSearchableFolder(d);
            }
        }

        public void AddSpecificFolders(params DirectoryPath[] dirs)
        {
            this.sourceFolders.Add(dirs);
        }

        public async Task LoadInitialObjects()
        {
            await this.LoadInitialObjects(this.sourceFolders.SelectMany((dir) => dir.EnumerateFileInfos())
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
                    string protoNamespace;
                    if (protoNode == null
                        && !string.IsNullOrWhiteSpace(this.Gen.ProtocolDefault.Namespace))
                    {
                        protoNamespace = this.Gen.ProtocolDefault.Namespace;
                    }
                    else
                    {
                        protoNamespace = protoNode.GetAttribute("Namespace");
                    }

                    return protoNamespace == null || this.Protocol.Namespace.Equals(protoNamespace);
                }));
        }

        public void AddProjectToModify(FileInfo projFile)
        {
            this.projectsToModify.Add(projFile);
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
            generatedItems.Set(this.ObjectGenerationsByID.Select((kv) => kv.Value).SelectMany((objGen) => objGen.GeneratedFiles));
            HashSet<FilePath> sourceXMLs = new HashSet<FilePath>(this.ObjectGenerationsByID.Select(kv => new FilePath(kv.Value.SourceXMLFile.FullName)));

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
                if (!this.Gen.TryGetMatchingObjectGeneration(file, out ObjectGeneration objGen)) continue;
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
            bool found = false;
            foreach (var compile in compileNodes)
            {
                XAttribute includeAttr = compile.Attribute("Include");
                if (includeAttr == null) continue;
                if (includeAttr.Value.ToLower().Contains(this.ProtocolDefinitionName.ToLower()))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                var compileElem = new XElement(XName.Get("Compile", CSPROJ_NAMESPACE),
                    new XAttribute("Include", this.ProtocolDefinitionName + ".cs"));
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
    }
}
