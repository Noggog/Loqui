using Noggog;
using System;
using System.Linq;
using System.Collections.Generic;
using Loqui.Xml;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using Loqui.Internal;

namespace Loqui.Generation
{
    public class XmlTranslationModule : TranslationModule<XmlTranslationGeneration>
    {
        public override string ModuleNickname => "Xml";
        public override string Namespace => "Loqui.Generation";
        public readonly static XNamespace XSDNamespace = "http://www.w3.org/2001/XMLSchema";
        public bool ShouldGenerateXSD = true;
        public FilePath ObjectXSDLocation(ObjectGeneration obj) => new FilePath(Path.Combine(obj.TargetDir.FullName, this.ObjectXSDName(obj)));
        public string ObjectXSDName(ObjectGeneration obj) => $"{obj.Name}.xsd";
        public FilePath CommonXSDLocation(ProtocolGeneration proto) => new FilePath(Path.Combine(proto.GenerationFolder.FullName, "Common.xsd"));
        public string ObjectNamespace(ObjectGeneration obj) => $"{obj.ProtoGen.Protocol.Namespace}";
        public string ObjectType(ObjectGeneration obj) => $"{obj.Name}Type";

        public XmlTranslationModule(LoquiGenerator gen)
            : base(gen)
        {
            this._typeGenerations[typeof(LoquiType)] = new LoquiXmlTranslationGeneration();
            this._typeGenerations[typeof(BoolNullType)] = new PrimitiveXmlTranslationGeneration<bool?>();
            this._typeGenerations[typeof(BoolType)] = new PrimitiveXmlTranslationGeneration<bool>();
            this._typeGenerations[typeof(CharNullType)] = new PrimitiveXmlTranslationGeneration<char?>();
            this._typeGenerations[typeof(CharType)] = new PrimitiveXmlTranslationGeneration<char>();
            this._typeGenerations[typeof(DateTimeNullType)] = new PrimitiveXmlTranslationGeneration<DateTime?>();
            this._typeGenerations[typeof(DateTimeType)] = new PrimitiveXmlTranslationGeneration<DateTime>();
            this._typeGenerations[typeof(DoubleNullType)] = new PrimitiveXmlTranslationGeneration<double?>();
            this._typeGenerations[typeof(DoubleType)] = new PrimitiveXmlTranslationGeneration<double>();
            this._typeGenerations[typeof(EnumType)] = new EnumXmlTranslationGeneration();
            this._typeGenerations[typeof(EnumNullType)] = new EnumXmlTranslationGeneration();
            this._typeGenerations[typeof(FloatNullType)] = new PrimitiveXmlTranslationGeneration<float?>("Float");
            this._typeGenerations[typeof(FloatType)] = new PrimitiveXmlTranslationGeneration<float>("Float");
            this._typeGenerations[typeof(Int8NullType)] = new PrimitiveXmlTranslationGeneration<sbyte?>("Int8");
            this._typeGenerations[typeof(Int8Type)] = new PrimitiveXmlTranslationGeneration<sbyte>("Int8");
            this._typeGenerations[typeof(Int16NullType)] = new PrimitiveXmlTranslationGeneration<short?>();
            this._typeGenerations[typeof(Int16Type)] = new PrimitiveXmlTranslationGeneration<short>();
            this._typeGenerations[typeof(Int32NullType)] = new PrimitiveXmlTranslationGeneration<int?>();
            this._typeGenerations[typeof(Int32Type)] = new PrimitiveXmlTranslationGeneration<int>();
            this._typeGenerations[typeof(Int64NullType)] = new PrimitiveXmlTranslationGeneration<long?>();
            this._typeGenerations[typeof(Int64Type)] = new PrimitiveXmlTranslationGeneration<long>();
            this._typeGenerations[typeof(P2Int32NullType)] = new PrimitiveXmlTranslationGeneration<P2Int?>();
            this._typeGenerations[typeof(P2Int32Type)] = new PrimitiveXmlTranslationGeneration<P2Int>();
            this._typeGenerations[typeof(P2Int16NullType)] = new PrimitiveXmlTranslationGeneration<P2Int16?>();
            this._typeGenerations[typeof(P2Int16Type)] = new PrimitiveXmlTranslationGeneration<P2Int16>();
            this._typeGenerations[typeof(P2FloatNullType)] = new PrimitiveXmlTranslationGeneration<P2Float?>();
            this._typeGenerations[typeof(P2FloatType)] = new PrimitiveXmlTranslationGeneration<P2Float>();
            this._typeGenerations[typeof(P3FloatNullType)] = new PrimitiveXmlTranslationGeneration<P3Float?>();
            this._typeGenerations[typeof(P3FloatType)] = new PrimitiveXmlTranslationGeneration<P3Float>();
            this._typeGenerations[typeof(P3IntNullType)] = new PrimitiveXmlTranslationGeneration<P3Int?>();
            this._typeGenerations[typeof(P3IntType)] = new PrimitiveXmlTranslationGeneration<P3Int>();
            this._typeGenerations[typeof(P3UInt16NullType)] = new PrimitiveXmlTranslationGeneration<P3UInt16?>();
            this._typeGenerations[typeof(P3UInt16Type)] = new PrimitiveXmlTranslationGeneration<P3UInt16>();
            this._typeGenerations[typeof(P3DoubleNullType)] = new PrimitiveXmlTranslationGeneration<P3Double?>();
            this._typeGenerations[typeof(P3DoubleType)] = new PrimitiveXmlTranslationGeneration<P3Double>();
            this._typeGenerations[typeof(PercentNullType)] = new PrimitiveXmlTranslationGeneration<Percent?>();
            this._typeGenerations[typeof(PercentType)] = new PrimitiveXmlTranslationGeneration<Percent>();
            this._typeGenerations[typeof(RangeDoubleNullType)] = new PrimitiveXmlTranslationGeneration<RangeDouble?>();
            this._typeGenerations[typeof(RangeDoubleType)] = new PrimitiveXmlTranslationGeneration<RangeDouble>();
            this._typeGenerations[typeof(StringType)] = new PrimitiveXmlTranslationGeneration<string>(nullable: true) { CanBeNotNullable = false };
            this._typeGenerations[typeof(TypicalRangedIntType<RangeInt8?>)] = new PrimitiveXmlTranslationGeneration<RangeInt8?>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeInt8>)] = new PrimitiveXmlTranslationGeneration<RangeInt8>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeInt16?>)] = new PrimitiveXmlTranslationGeneration<RangeInt16?>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeInt16>)] = new PrimitiveXmlTranslationGeneration<RangeInt16>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeInt32?>)] = new PrimitiveXmlTranslationGeneration<RangeInt32?>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeInt32>)] = new PrimitiveXmlTranslationGeneration<RangeInt32>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeInt64?>)] = new PrimitiveXmlTranslationGeneration<RangeInt64?>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeInt64>)] = new PrimitiveXmlTranslationGeneration<RangeInt64>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeUInt8?>)] = new PrimitiveXmlTranslationGeneration<RangeUInt8?>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeUInt8>)] = new PrimitiveXmlTranslationGeneration<RangeUInt8>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeUInt16?>)] = new PrimitiveXmlTranslationGeneration<RangeUInt16?>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeUInt16>)] = new PrimitiveXmlTranslationGeneration<RangeUInt16>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeUInt32?>)] = new PrimitiveXmlTranslationGeneration<RangeUInt32?>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeUInt32>)] = new PrimitiveXmlTranslationGeneration<RangeUInt32>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeUInt64?>)] = new PrimitiveXmlTranslationGeneration<RangeUInt64?>();
            this._typeGenerations[typeof(TypicalRangedIntType<RangeUInt64>)] = new PrimitiveXmlTranslationGeneration<RangeUInt64>();
            this._typeGenerations[typeof(UDoubleNullType)] = new PrimitiveXmlTranslationGeneration<UDouble?>();
            this._typeGenerations[typeof(UDoubleType)] = new PrimitiveXmlTranslationGeneration<UDouble>();
            this._typeGenerations[typeof(UInt8NullType)] = new PrimitiveXmlTranslationGeneration<byte?>();
            this._typeGenerations[typeof(UInt8Type)] = new PrimitiveXmlTranslationGeneration<byte>();
            this._typeGenerations[typeof(UInt16NullType)] = new PrimitiveXmlTranslationGeneration<ushort?>();
            this._typeGenerations[typeof(UInt16Type)] = new PrimitiveXmlTranslationGeneration<ushort>();
            this._typeGenerations[typeof(UInt32NullType)] = new PrimitiveXmlTranslationGeneration<uint?>();
            this._typeGenerations[typeof(UInt32Type)] = new PrimitiveXmlTranslationGeneration<uint>();
            this._typeGenerations[typeof(UInt64NullType)] = new PrimitiveXmlTranslationGeneration<ulong?>();
            this._typeGenerations[typeof(UInt64Type)] = new PrimitiveXmlTranslationGeneration<ulong>();
            this._typeGenerations[typeof(FilePathType)] = new PrimitiveXmlTranslationGeneration<FilePath>();
            this._typeGenerations[typeof(FilePathNullType)] = new PrimitiveXmlTranslationGeneration<FilePath?>();
            this._typeGenerations[typeof(DirectoryPathType)] = new PrimitiveXmlTranslationGeneration<DirectoryPath>();
            this._typeGenerations[typeof(DirectoryPathNullType)] = new PrimitiveXmlTranslationGeneration<DirectoryPath?>();
            this._typeGenerations[typeof(UnsafeType)] = new UnsafeXmlTranslationGeneration();
            this._typeGenerations[typeof(WildcardType)] = new UnsafeXmlTranslationGeneration();
            this._typeGenerations[typeof(ListType)] = new ListXmlTranslationGeneration();
            this._typeGenerations[typeof(DictType)] = new DictXmlTranslationGeneration();
            this._typeGenerations[typeof(ByteArrayType)] = new PrimitiveXmlTranslationGeneration<byte[]>(typeName: "ByteArray", nullable: true);
            this._typeGenerations[typeof(NothingType)] = new NothingXmlTranslationGeneration();
            this.MainAPI = new TranslationModuleAPI(
                writerAPI: new MethodAPI(
                    majorAPI: new APILine[] { "XElement node" },
                    customAPI: null,
                    optionalAPI: new APILine[] { "string name = null" }),
                readerAPI: new MethodAPI("XElement root"));
            this.MinorAPIs.Add(
                new TranslationModuleAPI(
                    writerAPI: new MethodAPI(
                        majorAPI: new APILine[] { "string path" },
                        customAPI: null,
                        optionalAPI: new APILine[] { "string name = null" }),
                    readerAPI: new MethodAPI("string path"))
                {
                    Funnel = new TranslationFunnel(
                        this.MainAPI,
                        ConvertFromPathOut,
                        ConvertFromPathIn)
                });
            this.MinorAPIs.Add(
                new TranslationModuleAPI(
                    writerAPI: new MethodAPI(
                        majorAPI: new APILine[] { "Stream stream" },
                        customAPI: null,
                        optionalAPI: new APILine[] { "string name = null" }),
                    readerAPI: new MethodAPI("Stream stream"))
                {
                    Funnel = new TranslationFunnel(
                        this.MainAPI,
                        ConvertFromStreamOut,
                        ConvertFromStreamIn)
                });
        }

        public override async Task PostLoad(ObjectGeneration obj)
        {
            foreach (var gen in _typeGenerations.Values)
            {
                gen.XmlMod = this;
                gen.MaskModule = this.Gen.MaskModule;
            }
        }

        public override IEnumerable<string> RequiredUsingStatements(ObjectGeneration obj)
        {
            yield return "System.Xml";
            yield return "System.Xml.Linq";
            yield return "System.IO";
            yield return "Noggog.Xml";
            yield return "Loqui.Xml";
            yield return "Loqui.Internal";
            foreach (var item in base.RequiredUsingStatements(obj))
            {
                yield return item;
            }
        }

        private void ConvertFromStreamOut(ObjectGeneration obj, FileGeneration fg, InternalTranslation internalToDo)
        {
            fg.AppendLine("XElement topNode = new XElement(\"topnode\");");
            internalToDo("topNode", "name");
            fg.AppendLine($"topNode.Elements().First().Save(stream);");
        }

        private void ConvertFromStreamIn(ObjectGeneration obj, FileGeneration fg, InternalTranslation internalToDo)
        {
            fg.AppendLine($"var root = XDocument.Load(stream).Root;");
            internalToDo("root");
        }

        private void ConvertFromPathOut(ObjectGeneration obj, FileGeneration fg, InternalTranslation internalToDo)
        {
            fg.AppendLine("XElement topNode = new XElement(\"topnode\");");
            internalToDo("topNode", "name");
            fg.AppendLine($"topNode.Elements().First().Save(path);");
        }

        private void ConvertFromPathIn(ObjectGeneration obj, FileGeneration fg, InternalTranslation internalToDo)
        {
            fg.AppendLine($"var root = XDocument.Load(path).Root;");
            internalToDo("root");
        }

        public override async Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            await base.GenerateInClass(obj, fg);
            GenerateCreate_InternalFunctions(obj, fg);
        }

        private void GenerateCreate_InternalFunctions(ObjectGeneration obj, FileGeneration fg)
        {
            using (var args = new FunctionWrapper(fg,
                $"protected static void Fill_{ModuleNickname}_Internal"))
            {
                args.Add($"{obj.ObjectName} item");
                args.Add("XElement root");
                args.Add("string name");
                args.Add($"ErrorMaskBuilder errorMask");
                args.Add($"{nameof(TranslationCrystal)} translationMask");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("switch (name)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in obj.IterateFields())
                    {
                        if (field.Derivative) continue;
                        if (!this.TryGetTypeGeneration(field.GetType(), out var generator))
                        {
                            throw new ArgumentException("Unsupported type generator: " + field);
                        }

                        fg.AppendLine($"case \"{field.Name}\":");
                        using (new DepthWrapper(fg))
                        {
                            if (generator.ShouldGenerateCopyIn(field))
                            {
                                generator.GenerateCopyIn(
                                    fg: fg,
                                    typeGen: field,
                                    nodeAccessor: "root",
                                    itemAccessor: new Accessor(field, "item."),
                                    translationMaskAccessor: "translationMask",
                                    maskAccessor: $"errorMask");
                            }
                            fg.AppendLine("break;");
                        }
                    }

                    fg.AppendLine("default:");
                    using (new DepthWrapper(fg))
                    {
                        if (obj.HasBaseObject)
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"{obj.BaseClassName}.Fill_{ModuleNickname}_Internal{obj.GetBaseMask_GenericTypes(MaskType.Error)}"))
                            {
                                args.Add("item: item");
                                args.Add("root: root");
                                args.Add("name: name");
                                args.Add("errorMask: errorMask");
                                if (this.TranslationMaskParameter)
                                {
                                    args.Add($"translationMask: translationMask");
                                }
                            }
                        }
                        fg.AppendLine("break;");
                    }
                }
            }
            fg.AppendLine();
        }

        public override async Task MiscellaneousGenerationActions(ObjectGeneration obj)
        {
            GenerateXSDForObj(obj);
        }

        public void GenerateXSDForObj(ObjectGeneration obj)
        {
            if (!ShouldGenerateXSD) return;

            var itemNamespace = ObjectNamespace(obj);

            XElement root = new XElement(XSDNamespace + "schema",
                new XAttribute("id", obj.Name),
                new XAttribute("targetNamespace", itemNamespace),
                new XAttribute("elementFormDefault", "qualified"),
                new XAttribute("xmlns", itemNamespace),
                new XAttribute(XNamespace.Xmlns + "xs", XSDNamespace.NamespaceName));

            if (obj.HasBaseObject)
            {
                FilePath xsdPath = this.ObjectXSDLocation(obj.BaseClass);
                var relativePath = xsdPath.GetRelativePathTo(obj.TargetDir);
                root.Add(
                    new XElement(
                        XmlTranslationModule.XSDNamespace + "include",
                        new XAttribute("schemaLocation", relativePath)));
            }

            root.Add(
                new XElement(XSDNamespace + "element",
                    new XAttribute("name", obj.Name),
                    new XAttribute("type", ObjectType(obj))));

            var typeElement = new XElement(XSDNamespace + "complexType",
                new XAttribute("name", $"{obj.Name}Type"));
            var choiceElement = new XElement(XSDNamespace + "choice",
                new XAttribute("minOccurs", 0),
                new XAttribute("maxOccurs", "unbounded"));
            if (obj.HasBaseObject)
            {
                typeElement.Add(
                    new XElement(
                        XSDNamespace + "complexContent",
                        new XElement(
                            XSDNamespace + "extension",
                            new XAttribute("base", this.ObjectType(obj.BaseClass)),
                            choiceElement)));
            }
            else
            {
                typeElement.Add(choiceElement);
            }
            root.Add(typeElement);
            foreach (var field in obj.IterateFields())
            {
                if (!this.TryGetTypeGeneration(field.GetType(), out var xmlGen))
                {
                    throw new ArgumentException("Unsupported type generator: " + field.GetType());
                }
                var elem = xmlGen.GenerateForXSD(
                    obj,
                    root,
                    choiceElement,
                    field,
                    nameOverride: null);
                elem.Add(new XAttribute("minOccurs", 0));
                elem.Add(new XAttribute("maxOccurs", 1));
            }

            var outputPath = Path.Combine(obj.TargetDir.FullName, $"{obj.Name}.xsd");
            obj.GeneratedFiles[Path.GetFullPath(outputPath)] = ProjItemType.None;
            using (var writer = new XmlTextWriter(outputPath, Encoding.ASCII))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 3;
                XDocument doc = new XDocument(root);
                doc.WriteTo(writer);
            }
        }

        public override async Task FinalizeGeneration(ProtocolGeneration proto)
        {
            GenerateCommonXSDForProto(proto);
            await base.FinalizeGeneration(proto);
        }

        public void GenerateCommonXSDForProto(ProtocolGeneration protoGen)
        {
            if (!this.ShouldGenerateXSD) return;
            var nameSpace = protoGen.Protocol.Namespace;
            XElement root = new XElement(XSDNamespace + "schema",
                new XAttribute("targetNamespace", nameSpace),
                new XAttribute("elementFormDefault", "qualified"),
                new XAttribute("xmlns", nameSpace),
                new XAttribute(XNamespace.Xmlns + "xs", XSDNamespace.NamespaceName));

            foreach (var obj in protoGen.ObjectGenerationsByID.Values)
            {
                foreach (var field in obj.IterateFields())
                {
                    if (!this.TryGetTypeGeneration(field.GetType(), out var xmlGen))
                    {
                        throw new ArgumentException("Unsupported type generator: " + field.GetType());
                    }
                    xmlGen.GenerateForCommonXSD(
                        root,
                        field);
                }
            }

            var outputPath = this.CommonXSDLocation(protoGen);
            protoGen.GeneratedFiles[outputPath.Path] = ProjItemType.None;
            using (var writer = new XmlTextWriter(outputPath.Path, Encoding.ASCII))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 3;
                XDocument doc = new XDocument(root);
                doc.WriteTo(writer);
            }
        }

        protected override async Task GenerateCreateSnippet(ObjectGeneration obj, FileGeneration fg)
        {
            fg.AppendLine($"var ret = new {obj.Name}{obj.GetGenericTypes(MaskType.Normal)}();");
            fg.AppendLine("try");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("foreach (var elem in root.Elements())");
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg,
                        $"Fill_{ModuleNickname}_Internal"))
                    {
                        args.Add("item: ret");
                        args.Add("root: elem");
                        args.Add("name: elem.Name.LocalName");
                        args.Add("errorMask: errorMask");
                        if (this.TranslationMaskParameter)
                        {
                            args.Add("translationMask: translationMask");
                        }
                    }
                }
            }
            fg.AppendLine("catch (Exception ex)");
            fg.AppendLine("when (errorMask != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("errorMask.ReportException(ex);");
            }
            fg.AppendLine("return ret;");
        }

        protected override void GenerateWriteSnippet(ObjectGeneration obj, FileGeneration fg)
        {

            fg.AppendLine($"var elem = new XElement(name ?? \"{obj.FullName}\");");
            fg.AppendLine("node.Add(elem);");
            fg.AppendLine("if (name != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"elem.SetAttributeValue(\"{XmlConstants.TYPE_ATTRIBUTE}\", \"{obj.FullName}\");");
            }

            foreach (var field in obj.IterateFieldIndices())
            {
                if (!this.TryGetTypeGeneration(field.Field.GetType(), out var generator))
                {
                    throw new ArgumentException("Unsupported type generator: " + field.Field);
                }

                if (!generator.ShouldGenerateWrite(field.Field)) continue;

                List<string> conditions = new List<string>();
                if (field.Field.HasBeenSet)
                {
                    conditions.Add($"item.{field.Field.HasBeenSetAccessor}");
                }
                if (this.TranslationMaskParameter)
                {
                    conditions.Add(generator.GetTranslationIfAccessor(field.Field, "translationMask"));
                }
                if (conditions.Count > 0)
                {
                    using (var args = new IfWrapper(fg, ANDs: true))
                    {
                        foreach (var item in conditions)
                        {
                            args.Add(item);
                        }
                    }
                }
                using (new BraceWrapper(fg, doIt: conditions.Count > 0))
                {
                    var maskType = this.Gen.MaskModule.GetMaskModule(field.Field.GetType()).GetErrorMaskTypeStr(field.Field);
                    generator.GenerateWrite(
                        fg: fg,
                        objGen: obj,
                        typeGen: field.Field,
                        writerAccessor: "elem",
                        itemAccessor: new Accessor(field.Field, "item."),
                        maskAccessor: $"errorMask",
                        translationMaskAccessor: "translationMask",
                        nameAccessor: $"nameof(item.{field.Field.Name})");
                }
            }
        }
    }
}
