using Noggog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace Noggolloquy.Generation
{
    public class ProtocolGeneration
    {
        public ProtocolDefinition Definition;
        public Dictionary<Guid, ObjectGeneration> ObjectGenerationsByID = new Dictionary<Guid, ObjectGeneration>();
        public Dictionary<StringCaseAgnostic, ObjectGeneration> ObjectGenerationsByName = new Dictionary<StringCaseAgnostic, ObjectGeneration>();
        public Dictionary<StringCaseAgnostic, FieldBatch> FieldBatchesByName = new Dictionary<StringCaseAgnostic, FieldBatch>();
        public bool Empty => ObjectGenerationsByID.Count == 0;
        public NoggolloquyGenerator Gen { get; private set; }
        public DirectoryInfo DefFileLocationOverride { get; private set; }
        public DirectoryInfo DefFileLocation => this.DefFileLocationOverride ?? this.Gen.CommonGenerationFolder;
        public NoggInterfaceType InterfaceTypeDefault = NoggInterfaceType.Direct;
        public bool ProtectedDefault;
        public bool DerivativeDefault;
        public bool RaisePropertyChangedDefault = true;

        public ProtocolGeneration(
            NoggolloquyGenerator gen,
            ProtocolDefinition def,
            DirectoryInfo defFileLocation = null)
        {
            this.Definition = def;
            this.Gen = gen;
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
                XElement objNode = xmlDoc.Element(XName.Get("Noggolloquy", NoggolloquyGenerator.Namespace));

                string namespaceStr = this.Gen.DefaultNamespace;
                XElement namespaceNode = objNode.Element(XName.Get("Namespace", NoggolloquyGenerator.Namespace));
                if (namespaceNode != null)
                {
                    namespaceStr = namespaceNode.Value;
                }

                foreach (var batch in objNode.Elements(XName.Get("FieldBatch", NoggolloquyGenerator.Namespace)))
                {
                    var fieldBatch = new FieldBatch(this.Gen);
                    fieldBatch.Load(batch);
                    this.FieldBatchesByName[fieldBatch.Name] = fieldBatch;
                }

                foreach (var obj in objNode.Elements(XName.Get("Object", NoggolloquyGenerator.Namespace))
                    .And(objNode.Elements(XName.Get("Struct", NoggolloquyGenerator.Namespace))))
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

        public void Generate()
        {
            foreach (var obj in ObjectGenerationsByID.Values)
            {
                obj.Load();
            }

            foreach (var obj in ObjectGenerationsByID.Values)
            {
                obj.Resolve();
            }

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
                "Noggolloquy"
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

            fg.AppendLine("namespace Noggolloquy");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"public class ProtocolDefinition_{this.Definition.Nickname} : IProtocolRegistration");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"public readonly static ProtocolKey ProtocolKey = new ProtocolKey({this.Definition.Key.ProtocolID});");
                    fg.AppendLine($"public readonly static ProtocolDefinition Definition = new ProtocolDefinition(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"key: ProtocolKey,");
                        fg.AppendLine($"nickname: \"{this.Definition.Nickname}\");");
                    }
                    fg.AppendLine();
                    fg.AppendLine("public void Register()");
                    using (new BraceWrapper(fg))
                    {
                        foreach (var obj in this.ObjectGenerationsByID.Values
                            .OrderBy((o) => o.ID))
                        {
                            fg.AppendLine($"NoggolloquyRegistration.Register({obj.InternalNamespace}.{obj.RegistrationName}.Instance);");
                        }
                    }
                }
            }

            fg.Generate(
                new FileInfo(
                    DefFileLocation.FullName
                    + $"/ProtocolDefinition_{this.Definition.Nickname}.cs"));
        }
    }
}
