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
        public ProtocolKey Protocol;
        public Dictionary<Guid, ObjectGeneration> ObjectGenerationsByID = new Dictionary<Guid, ObjectGeneration>();
        public Dictionary<StringCaseAgnostic, ObjectGeneration> ObjectGenerationsByName = new Dictionary<StringCaseAgnostic, ObjectGeneration>();
        public Dictionary<StringCaseAgnostic, FieldBatch> FieldBatchesByName = new Dictionary<StringCaseAgnostic, FieldBatch>();
        public bool Empty => ObjectGenerationsByID.Count == 0;
        public LoquiGenerator Gen { get; private set; }
        public DirectoryInfo GenerationFolder { get; private set; }
        public DirectoryInfo DefFileLocation => this.GenerationFolder;
        public LoquiInterfaceType SetterInterfaceTypeDefault;
        public LoquiInterfaceType GetterInterfaceTypeDefault;
        public PermissionLevel SetPermissionDefault;
        public RxBaseOption RxBaseOptionDefault;
        public bool DerivativeDefault;
        public bool ToStringDefault;
        public bool NthReflectionDefault;
        public string DefaultNamespace;
        public NotifyingType NotifyingDefault;
        public bool HasBeenSetDefault;
        public bool ObjectCentralizedDefault;
        public List<string> Interfaces = new List<string>();
        public string ProtocolDefinitionName => $"ProtocolDefinition_{this.Protocol.Namespace}";
        private HashSet<DirectoryPath> sourceFolders = new HashSet<DirectoryPath>();
        List<FilePath> projectsToModify = new List<FilePath>();
        public Dictionary<FilePath, ProjItemType> GeneratedFiles = new Dictionary<FilePath, ProjItemType>();

        public ProtocolGeneration(
            LoquiGenerator gen,
            ProtocolKey protocol,
            DirectoryInfo defSearchableFolder)
        {
            this.Protocol = protocol;
            this.Gen = gen;
            this.NotifyingDefault = gen.NotifyingDefault;
            this.HasBeenSetDefault = gen.HasBeenSetDefault;
            this.ObjectCentralizedDefault = gen.ObjectCentralizedDefault;
            this.GenerationFolder = defSearchableFolder;
            this.SetterInterfaceTypeDefault = gen.SetterInterfaceTypeDefault;
            this.GetterInterfaceTypeDefault = gen.GetterInterfaceTypeDefault;
            this.SetPermissionDefault = gen.SetPermissionDefault;
            this.RxBaseOptionDefault = gen.RxBaseOptionDefault;
            this.NthReflectionDefault = gen.NthReflectionDefault;
            this.ToStringDefault = gen.ToStringDefault;
            this.DerivativeDefault = gen.DerivativeDefault;
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
                    .Select(async (obj) =>
                    {
                        try
                        {
                            await TaskExt.DoThenComplete(obj.LoadingCompleteTask, obj.Load)
                                .TimeoutButContinue(4000, () => System.Console.WriteLine($"{obj.Name} loading taking a long time."));
                        }
                        catch (Exception ex)
                        {
                            MarkFailure(ex);
                            throw;
                        }
                    }));

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

            await Task.WhenAll(this.Gen.GenerationModules.Select(
                (mod) => mod.FinalizeGeneration(this)));

            foreach (var file in this.projectsToModify)
            {
                ModifyProject(file);
            }
        }

        private void MarkFailure(Exception ex)
        {
            foreach (var obj in this.ObjectGenerationsByID.Values)
            {
                obj.MarkFailure(ex);
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
                    fg.AppendLine("void IProtocolRegistration.Register() => Register();");
                    fg.AppendLine("public static void Register()");
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
                .Where((f) => ".XML".Equals(f.Extension, StringComparison.CurrentCultureIgnoreCase))
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
            var nameSpace = doc.Root.Name.Namespace.NamespaceName;
            var projNode = doc.Element(XName.Get("Project", nameSpace));
            List<XElement> includeNodes = projNode.Elements(XName.Get("ItemGroup", nameSpace)).ToList();
            List<XElement> compileGroupNodes = includeNodes
                .Where((group) => group.Elements().Any((e) => e.Name.LocalName.Equals("Compile")))
                .ToList();
            List<XElement> noneGroupNodes = includeNodes
                .Where((group) => group.Elements().Any((e) => e.Name.LocalName.Equals("None")))
                .ToList();
            var compileNodes = compileGroupNodes
                .SelectMany((itemGroup) => itemGroup.Elements(XName.Get("Compile", nameSpace)))
                .ToList();
            var noneNodes = compileGroupNodes
                .SelectMany((itemGroup) => itemGroup.Elements(XName.Get("None", nameSpace)))
                .ToList();

            XElement compileIncludeNode;
            if (compileGroupNodes.Count == 0)
            {
                compileIncludeNode = new XElement("ItemGroup", nameSpace);
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
                    var ret = new XElement("ItemGroup", nameSpace);
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
            generatedItems.Set(GeneratedFiles);
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
                    var compileElem = new XElement(XName.Get(nodeName, nameSpace),
                        new XAttribute("Include", filePath));
                    nodes.Add(compileElem);
                    includeNode.Add(compileElem);
                    modified = true;
                }
            }

            // Add dependent files underneath
            var depName = XName.Get("DependentUpon", nameSpace);
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
                var defFile = new FilePath($"{DefFileLocation.FullName}/{this.ProtocolDefinitionName}.cs");
                var relativePath = defFile.GetRelativePathTo(projFile);
                var compileElem = new XElement(XName.Get("Compile", nameSpace),
                    new XAttribute("Include", relativePath));
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

        public override string ToString()
        {
            return $"ProtocolGeneration ({this.Protocol.Namespace})";
        }
    }
}
