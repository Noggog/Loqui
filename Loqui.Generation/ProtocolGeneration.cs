using Noggog;
using System.Xml.Linq;
using System.Xml;
using System.Text;
using Noggog.IO;
using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Loqui.Generation;

public class ProtocolGeneration
{
    public ProtocolKey Protocol;
    public readonly Dictionary<string, ObjectGeneration> ObjectGenerationsByName = new(StringComparer.OrdinalIgnoreCase);
    public readonly Dictionary<string, FieldBatch> FieldBatchesByName = new(StringComparer.OrdinalIgnoreCase);
    public bool Empty => ObjectGenerationsByName.Count == 0;
    public LoquiGenerator Gen { get; private set; }
    public DirectoryInfo GenerationFolder { get; private set; }
    public DirectoryInfo DefFileLocation => GenerationFolder;
    public LoquiInterfaceType SetterInterfaceTypeDefault;
    public LoquiInterfaceType GetterInterfaceTypeDefault;
    public AccessModifier SetPermissionDefault;
    public RxBaseOption RxBaseOptionDefault;
    public bool DerivativeDefault;
    public bool ToStringDefault;
    public bool NthReflectionDefault;
    public string DefaultNamespace;
    public bool NullableDefault;
    public bool DoGeneration = true;
    public readonly List<string> Interfaces = new();
    public string ProtocolDefinitionName => $"ProtocolDefinition_{Protocol.Namespace}";
    private readonly HashSet<DirectoryPath> sourceFolders = new();
    private readonly List<FilePath> projectsToModify = new();
    public readonly Dictionary<FilePath, ProjItemType> GeneratedFiles = new();

    public ProtocolGeneration(
        LoquiGenerator gen,
        ProtocolKey protocol,
        DirectoryInfo defSearchableFolder)
    {
        Protocol = protocol;
        Gen = gen;
        NullableDefault = gen.NullableDefault;
        GenerationFolder = defSearchableFolder;
        SetterInterfaceTypeDefault = gen.SetterInterfaceTypeDefault;
        GetterInterfaceTypeDefault = gen.GetterInterfaceTypeDefault;
        SetPermissionDefault = gen.SetPermissionDefault;
        RxBaseOptionDefault = gen.RxBaseOptionDefault;
        NthReflectionDefault = gen.NthReflectionDefault;
        ToStringDefault = gen.ToStringDefault;
        DerivativeDefault = gen.DerivativeDefault;
        AddSearchableFolder(defSearchableFolder);
    }

    private async Task LoadInitialObjects(IEnumerable<(XDocument Doc, FilePath Path)> xmlDocs)
    {
        List<ObjectGeneration> unassignedObjects = new List<ObjectGeneration>();

        // Parse IDs
        foreach (var xmlDocTuple in xmlDocs)
        {
            var xmlDoc = xmlDocTuple.Item1;
            XElement objNode = xmlDoc.Element(XName.Get("Loqui", LoquiGenerator.Namespace));

            string namespaceStr = DefaultNamespace ?? Gen.DefaultNamespace;
            XElement namespaceNode = objNode.Element(XName.Get("Namespace", LoquiGenerator.Namespace));
            if (namespaceNode != null)
            {
                namespaceStr = namespaceNode.Value;
            }

            foreach (var batch in objNode.Elements(XName.Get("FieldBatch", LoquiGenerator.Namespace)))
            {
                var fieldBatch = new FieldBatch(Gen);
                fieldBatch.Load(batch);
                FieldBatchesByName[fieldBatch.Name] = fieldBatch;
            }

            foreach (var obj in objNode.Elements(XName.Get("Object", LoquiGenerator.Namespace))
                         .And(objNode.Elements(XName.Get("Struct", LoquiGenerator.Namespace))))
            {
                if (obj.GetAttribute<DisabledLevel>("disable", DisabledLevel.Enabled) == DisabledLevel.OmitEntirely) continue;
                var objGen = GetGeneration(Gen, this, xmlDocTuple.Path, classGen: obj.Name.LocalName.Equals("Object"));
                objGen.Node = obj;
                if (!string.IsNullOrWhiteSpace(namespaceStr))
                {
                    objGen.Namespace = namespaceStr;
                }

                var nameNode = obj.Attribute("name");
                if (nameNode == null)
                {
                    throw new ArgumentException("Object must have a name");
                }

                string name = nameNode.Value;
                if (ObjectGenerationsByName.ContainsKey(name))
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

                ObjectGenerationsByName.Add(name, objGen);
                Gen.ObjectGenerationsByDir.GetOrAdd(objGen.TargetDir.Path).Add(objGen);
                Gen.ObjectGenerationsByObjectNameKey[new ObjectNamedKey(Protocol, objGen.Name)] = objGen;
            }
        }
    }

    public async Task Generate()
    {
        await Task.WhenAll(Gen.GenerationModules.Select(
            (mod) => mod.PrepareGeneration(this)));

        await Task.WhenAll(
            ObjectGenerationsByName.Values
                .SelectMany((obj) => Gen.GenerationModules
                    .Select((m) => m.PreLoad(obj))));

        await Task.WhenAll(
            ObjectGenerationsByName.Values
                .Select(async (obj) =>
                {
                    try
                    {
                        await TaskExt.DoThenComplete(obj.LoadingCompleteTask, obj.Load);
                    }
                    catch (Exception ex)
                    {
                        MarkFailure(ex);
                        throw;
                    }
                }));

        await Task.WhenAll(
            ObjectGenerationsByName.Values
                .Select((obj) => obj.Resolve()));

        await Task.WhenAll(
            ObjectGenerationsByName.Values
                .Select(async (obj) =>
                {
                    try
                    {
                        await Task.WhenAll(
                            Gen.GenerationModules.Select((m) => m.PostLoad(obj)));
                    }
                    catch (Exception ex)
                    {
                        MarkFailure(ex);
                        throw;
                    }
                }));

        if (!DoGeneration) return;

        await Task.WhenAll(ObjectGenerationsByName.Values
            .Select(async (obj) =>
            {
                await obj.Generate();
            }));

        GenerateDefFile();

        await Task.WhenAll(Gen.GenerationModules.Select(
            (mod) => mod.FinalizeGeneration(this)));

        foreach (var file in projectsToModify)
        {
            ModifyProject(file);
        }
    }

    private void MarkFailure(Exception ex)
    {
        foreach (var obj in ObjectGenerationsByName.Values)
        {
            obj.MarkFailure(ex);
        }
    }

    private void GenerateDefFile()
    {
        var namespaces = new HashSet<string>();
        foreach (var obj in ObjectGenerationsByName.Values)
        {
            namespaces.Add(obj.Namespace);
        }

        StructuredStringBuilder sb = new StructuredStringBuilder();
        foreach (var nameS in namespaces)
        {
            sb.AppendLine($"using {nameS};");
        }
        sb.AppendLine();

        sb.AppendLine("namespace Loqui;");
        sb.AppendLine();

        sb.AppendLine($"internal class {ProtocolDefinitionName} : IProtocolRegistration");
        using (sb.CurlyBrace())
        {
            sb.AppendLine($"public static readonly ProtocolKey ProtocolKey = new(\"{Protocol.Namespace}\");");
            sb.AppendLine("void IProtocolRegistration.Register() => Register();");
            sb.AppendLine("public static void Register()");
            using (sb.CurlyBrace())
            {
                sb.AppendLine("LoquiRegistration.Register(");
                using (sb.IncreaseDepth())
                {
                    using (var comma = sb.CommaCollection())
                    {
                        foreach (var obj in ObjectGenerationsByName.Values
                                     .OrderBy((o) => o.Name))
                        {
                            comma.Add($"{obj.RegistrationName}.Instance");
                        }
                    }
                }
                sb.AppendLine(");");
            }
        }

        ExportStringToFile export = new();
        export.ExportToFile( 
            new FileInfo(
                DefFileLocation.FullName
                + $"/{ProtocolDefinitionName}.cs"),
            sb.GetString());
    }

    public void AddSearchableFolder(DirectoryInfo dir)
    {
        if (dir == null || !dir.Exists) return;
        AddSpecificFolders(dir);
        foreach (var d in dir.GetDirectories())
        {
            AddSearchableFolder(d);
        }
    }

    public void AddSpecificFolders(params DirectoryPath[] dirs)
    {
        sourceFolders.Add(dirs);
    }

    public async Task LoadInitialObjects()
    {
        await LoadInitialObjects(sourceFolders.SelectMany((dir) => dir.EnumerateFiles())
            .Where((f) => ".XML".Equals(f.Extension, StringComparison.CurrentCultureIgnoreCase))
            .Select(
                (f) =>
                {
                    XDocument doc;
                    using (var stream = new FileStream(f.Path, FileMode.Open))
                    {
                        doc = XDocument.Load(stream);
                    }
                    return (Doc: doc, File: f);
                })
            .Where((t) =>
            {
                var loquiNode = t.Doc.Element(XName.Get("Loqui", LoquiGenerator.Namespace));
                if (loquiNode == null) return false;
                var protoNode = loquiNode.Element(XName.Get("Protocol", LoquiGenerator.Namespace));
                string protoNamespace;
                if (protoNode == null
                    && !string.IsNullOrWhiteSpace(Gen.ProtocolDefault.Namespace))
                {
                    protoNamespace = Gen.ProtocolDefault.Namespace;
                }
                else
                {
                    protoNamespace = protoNode.GetAttribute("Namespace");
                }

                return protoNamespace == null || Protocol.Namespace.Equals(protoNamespace);
            }));
    }

    public void AddProjectToModify(FileInfo projFile)
    {
        projectsToModify.Add(projFile);
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
        generatedItems.Set(ObjectGenerationsByName.Select((kv) => kv.Value).SelectMany((objGen) => objGen.GeneratedFiles));
        generatedItems.Set(GeneratedFiles);
        HashSet<FilePath> sourceXMLs = new HashSet<FilePath>(ObjectGenerationsByName.Select(kv => new FilePath(kv.Value.SourceXMLFile.Path)));

        // Find which objects are present
        foreach (var subNode in includeNodes.SelectMany((n) => n.Elements()))
        {
            XAttribute includeAttr = subNode.Attribute("Include");
            if (includeAttr == null) continue;
            generatedItems.Remove(Path.Combine(projFile.Directory.Value.Path, includeAttr.Value));
        }

        // Add missing object nodes
        foreach (var objGens in generatedItems)
        {
            if (objGens.Key.Directory.Value.IsUnderneath(projFile.Directory.Value)
                || objGens.Key.Directory.Equals(projFile.Directory))
            {
                string filePath = objGens.Key.Path.TrimStart(projFile.Directory.Value.Path);
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
            FilePath file = new FilePath(Path.Combine(projFile.Directory.Value.Path, includeAttr.Value));
            if (sourceXMLs.Contains(file)) continue;
            if (!Gen.TryGetMatchingObjectGeneration(file, out ObjectGeneration objGen)) continue;
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
            if (includeAttr.Value.ToLower().Contains(ProtocolDefinitionName.ToLower()))
            {
                found = true;
                break;
            }
        }
        if (!found)
        {
            var defFile = new FilePath($"{DefFileLocation.FullName}/{ProtocolDefinitionName}.cs");
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
        return $"ProtocolGeneration ({Protocol.Namespace})";
    }

    public virtual ObjectGeneration GetGeneration(
        LoquiGenerator gen,
        ProtocolGeneration protoGen,
        FilePath sourceFile,
        bool classGen)
    {
        if (classGen)
        {
            return new ClassGeneration(gen, protoGen, sourceFile);
        }
        return new StructGeneration(gen, protoGen, sourceFile);
    }
}