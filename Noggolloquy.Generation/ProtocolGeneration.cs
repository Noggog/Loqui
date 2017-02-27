using Noggog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Noggolloquy.Generation
{
    public class ProtocolGeneration
    {
        public ProtocolDefinition Definition;
        public Dictionary<Guid, ObjectGeneration> ObjectGenerationsByID = new Dictionary<Guid, ObjectGeneration>();
        public Dictionary<StringCaseAgnostic, ObjectGeneration> ObjectGenerationsByName = new Dictionary<StringCaseAgnostic, ObjectGeneration>();
        public bool Empty { get { return ObjectGenerationsByID.Count == 0; } }
        public NoggolloquyGenerator Gen { get; private set; }
        public DirectoryInfo DefFileLocationOverride { get; private set; }
        public DirectoryInfo DefFileLocation
        {
            get
            {
                if (this.DefFileLocationOverride == null)
                {
                    return this.Gen.CommonGenerationFolder;
                }
                return this.DefFileLocationOverride;
            }
        }

        public ProtocolGeneration(
            NoggolloquyGenerator gen,
            ProtocolDefinition def,
            DirectoryInfo defFileLocation = null)
        {
            this.Definition = def;
            this.Gen = gen;
            this.DefFileLocationOverride = defFileLocation;
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

                foreach (var obj in objNode.Elements(XName.Get("Object", NoggolloquyGenerator.Namespace))
                    .And(objNode.Elements(XName.Get("Struct", NoggolloquyGenerator.Namespace))))
                {
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

                    ushort id;
                    if (obj.TryGetAttribute<ushort>("ID", out id))
                    {
                        objGen.ID = id;
                    }

                    if (this.ObjectGenerationsByID.ContainsKey(objGen.GUID))
                    {
                        throw new ArgumentException("Two objects in the same protocol cannot have the same ID: " + objGen.GUID);
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
                        throw new ArgumentException("Two objects in the same protocol cannot have the same name: " + name);
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
            GenerateCommonInterfaceFile();
        }

        private void GenerateDefFile()
        {
            HashSet<string> namespaces = new HashSet<string>();
            namespaces.Add("Noggolloquy");
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
                    fg.AppendLine($"public readonly ProtocolKey ProtocolKey = new ProtocolKey({this.Definition.Key.ProtocolID});");
                    fg.AppendLine();
                    fg.AppendLine("public void Register()");
                    using (new BraceWrapper(fg))
                    {
                        foreach (var obj in this.ObjectGenerationsByID.Values)
                        {
                            fg.AppendLine("NoggolloquyRegistration.Register(");
                            using (new DepthWrapper(fg))
                            {
                                fg.AppendLine($"new ObjectKey(ProtocolKey, {obj.ID}, {obj.Version}),");
                                fg.AppendLine($"new NoggolloquyTypeRegister()");
                                using (new BraceWrapper(fg) { AppendParenthesis = true, AppendSemicolon = true })
                                {
                                    fg.AppendLine($"Class = typeof({obj.Name}{obj.EmptyGenerics}),");
                                    fg.AppendLine($"FullName = \"{obj.Name}\",");
                                    fg.AppendLine($"GenericCount = {obj.Generics.Count},");
                                    fg.AppendLine($"ObjectKey = new ObjectKey(ProtocolKey, {obj.ID}, {obj.Version})");
                                }
                            }
                        }
                    }
                }
            }

            fg.Generate(
                new FileInfo(
                    DefFileLocation.FullName
                    + $"/ProtocolDefinition_{this.Definition.Nickname}.cs"));
        }

        private void GenerateCommonInterfaceFile()
        {
            FileGeneration fg = new FileGeneration();
            HashSet<string> namespaces = new HashSet<string>();
            namespaces.Add("Noggolloquy");
            foreach (var obj in this.ObjectGenerationsByID.Values)
            {
                namespaces.Add(obj.Namespace);
            }
            foreach (var nameS in namespaces)
            {
                fg.AppendLine($"using {nameS};");
            }
            fg.AppendLine();

            fg.AppendLine($"namespace {this.Gen.DefaultNamespace}");
            using (new BraceWrapper(fg))
            {
                HashSet<string> interfaces = new HashSet<string>();
                interfaces.Add("INoggolloquyObjectGetter");
                interfaces.Add("ICopyable");
                foreach (var module in this.Gen.GenerationModules)
                {
                    interfaces.Add(module.GetWriterInterfaces());
                }
                fg.AppendLine($"public interface INoggolloquyWriterSerializer : {string.Join(", ", interfaces)}");
                using (new BraceWrapper(fg))
                {
                }
                fg.AppendLine();
                interfaces.Clear();


                interfaces.Add("ICopyInAble");
                foreach (var module in this.Gen.GenerationModules)
                {
                    interfaces.Add(module.GetReaderInterfaces());
                }
                fg.AppendLine($"public interface INoggolloquyReaderSerializer : {string.Join(", ", interfaces)}");
                using (new BraceWrapper(fg))
                {
                }
                fg.AppendLine();
                interfaces.Clear();

                interfaces.Add("INoggolloquyWriterSerializer");
                interfaces.Add("INoggolloquyReaderSerializer");
                foreach (var module in this.Gen.GenerationModules)
                {
                    interfaces.Add(module.GetReaderInterfaces());
                }
                fg.AppendLine($"public interface INoggolloquySerializer : {string.Join(", ", interfaces)}");
                using (new BraceWrapper(fg))
                {
                }
                interfaces.Clear();
            }
            fg.Generate(
                new FileInfo(
                    DefFileLocation.FullName
                    + $"/INoggolloquySerializer.cs"));
        }
    }
}
