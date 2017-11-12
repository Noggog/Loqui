using Noggog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Threading.Tasks;

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
        public DirectoryInfo DefFileLocationOverride { get; private set; }
        public DirectoryInfo DefFileLocation => this.DefFileLocationOverride ?? this.Gen.CommonGenerationFolder;
        public LoquiInterfaceType InterfaceTypeDefault = LoquiInterfaceType.Direct;
        public bool ProtectedDefault;
        public bool DerivativeDefault;
        public NotifyingOption NotifyingDefault;
        public bool RaisePropertyChangedDefault = true;
        public string ProtocolDefinitionName => $"ProtocolDefinition_{this.Protocol.Namespace}";

        public ProtocolGeneration(
            LoquiGenerator gen,
            ProtocolKey protocol,
            DirectoryInfo defFileLocation = null)
        {
            this.Protocol = protocol;
            this.Gen = gen;
            this.NotifyingDefault = gen.NotifyingDefault;
            this.DefFileLocationOverride = defFileLocation;
            this.InterfaceTypeDefault = gen.InterfaceTypeDefault;
            this.ProtectedDefault = gen.ProtectedDefault;
            this.DerivativeDefault = gen.DerivativeDefault;
            this.RaisePropertyChangedDefault = gen.RaisePropertyChangedDefault;
        }

        public void LoadInitialObjects(IEnumerable<System.Tuple<XDocument, FileInfo>> xmlDocs)
        {
            List<ObjectGeneration> unassignedObjects = new List<ObjectGeneration>();

            // Parse IDs
            foreach (var xmlDocTuple in xmlDocs)
            {
                var xmlDoc = xmlDocTuple.Item1;
                XElement objNode = xmlDoc.Element(XName.Get("Loqui", LoquiGenerator.Namespace));

                string namespaceStr = this.Gen.DefaultNamespace;
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
                }
            }
        }

        public async Task Generate()
        {
            foreach (var obj in ObjectGenerationsByID.Values)
            {
                foreach (var mods in this.Gen.GenerationModules)
                {
                    mods.PreLoad(obj);
                }
            }

            foreach (var obj in ObjectGenerationsByID.Values)
            {
                obj.Load();
            }


            await Task.WhenAll(this.ObjectGenerationsByID.Values.Select((obj) => obj.Resolve()));

            foreach (var obj in ObjectGenerationsByID.Values)
            {
                obj.Generate();
                obj.RegenerateAndStampSourceXML();
            }

            GenerateDefFile();
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
    }
}
