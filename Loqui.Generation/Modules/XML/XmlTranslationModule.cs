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

namespace Loqui.Generation
{
    public class XmlTranslationModule : TranslationModule<XmlTranslationGeneration>
    {
        public override string ModuleNickname => "XML";
        public override string Namespace => "Loqui.Generation";
        public readonly static XNamespace XSDNamespace = "http://www.w3.org/2001/XMLSchema";
        public bool ShouldGenerateXSD = true;

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
            this._typeGenerations[typeof(P2IntNullType)] = new PrimitiveXmlTranslationGeneration<P2Int?>();
            this._typeGenerations[typeof(P2IntType)] = new PrimitiveXmlTranslationGeneration<P2Int>();
            this._typeGenerations[typeof(P3IntNullType)] = new PrimitiveXmlTranslationGeneration<P3Int?>();
            this._typeGenerations[typeof(P3IntType)] = new PrimitiveXmlTranslationGeneration<P3Int>();
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
                    majorAPI: new string[] { "XmlWriter writer" },
                    customAPI: null,
                    optionalAPI: new string[] { "string name = null" }),
                readerAPI: new MethodAPI("XElement root"));
            this.MinorAPIs.Add(
                new TranslationModuleAPI(
                    writerAPI: new MethodAPI(
                        majorAPI: new string[] { "string path" },
                        customAPI: null,
                        optionalAPI: new string[] { "string name = null" }),
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
                        majorAPI: new string[] { "Stream stream" },
                        customAPI: null,
                        optionalAPI: new string[] { "string name = null" }),
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

        public override IEnumerable<string> RequiredUsingStatements()
        {
            yield return "System.Xml";
            yield return "System.Xml.Linq";
            yield return "System.IO";
            yield return "Noggog.Xml";
            yield return "Loqui.Xml";
            foreach (var item in base.RequiredUsingStatements())
            {
                yield return item;
            }
        }

        private void ConvertFromStreamOut(FileGeneration fg, InternalTranslation internalToDo)
        {
            fg.AppendLine("using (var writer = new XmlTextWriter(stream, Encoding.ASCII))");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"writer.Formatting = Formatting.Indented;");
                fg.AppendLine($"writer.Indentation = 3;");
                internalToDo("writer", "name");
            }
        }

        private void ConvertFromStreamIn(FileGeneration fg, InternalTranslation internalToDo)
        {
            fg.AppendLine($"var root = XDocument.Load(stream).Root;");
            internalToDo("root");
        }

        private void ConvertFromPathOut(FileGeneration fg, InternalTranslation internalToDo)
        {
            fg.AppendLine("using (var writer = new XmlTextWriter(path, Encoding.ASCII))");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"writer.Formatting = Formatting.Indented;");
                fg.AppendLine($"writer.Indentation = 3;");
                internalToDo("writer", "name");
            }
        }

        private void ConvertFromPathIn(FileGeneration fg, InternalTranslation internalToDo)
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
            if (!obj.Abstract)
            {
                using (var args = new FunctionWrapper(fg,
                    $"private static {obj.ObjectName} Create_{ModuleNickname}_Internal{obj.Mask_GenericClause(MaskType.Error)}",
                    wheres: obj.GenericTypes_ErrorMaskWheres))
                {
                    args.Add("XElement root");
                    args.Add($"Func<{obj.Mask(MaskType.Error)}> errorMask");
                }
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"var ret = new {obj.Name}{obj.GenericTypes}();");
                    fg.AppendLine("try");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("foreach (var elem in root.Elements())");
                        using (new BraceWrapper(fg))
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"Fill_{ModuleNickname}_Internal{obj.Mask_GenericClause(MaskType.Error)}"))
                            {
                                args.Add("item: ret");
                                args.Add("root: elem");
                                args.Add("name: elem.Name.LocalName");
                                args.Add("errorMask: errorMask");
                            }
                        }
                    }
                    fg.AppendLine("catch (Exception ex)");
                    fg.AppendLine("when (errorMask != null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("errorMask().Overall = ex;");
                    }
                    fg.AppendLine("return ret;");
                }
                fg.AppendLine();
            }

            using (var args = new FunctionWrapper(fg,
                $"protected static void Fill_{ModuleNickname}_Internal{obj.Mask_GenericClause(MaskType.Error)}",
                wheres: obj.GenericTypes_ErrorMaskWheres))
            {
                args.Add($"{obj.ObjectName} item");
                args.Add("XElement root");
                args.Add("string name");
                args.Add($"Func<{obj.Mask(MaskType.Error)}> errorMask");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("switch (name)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in obj.IterateFields())
                    {
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
                                    itemAccessor: new Accessor(field, "item.", protectedAccess: true),
                                    doMaskAccessor: "errorMask != null",
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
                                $"{obj.BaseClassName}.Fill_{ModuleNickname}_Internal{obj.BaseMask_GenericClause(MaskType.Error)}"))
                            {
                                args.Add("item: item");
                                args.Add("root: root");
                                args.Add("name: name");
                                args.Add("errorMask: errorMask");
                            }
                        }
                        fg.AppendLine("break;");
                    }
                }
            }
            fg.AppendLine();
        }

        public override async Task Generate(ObjectGeneration obj)
        {
            GenerateXSD(obj);
        }

        public void GenerateXSD(ObjectGeneration obj)
        {
            if (!ShouldGenerateXSD) return;

            var itemNamespace = $"{obj.ProtoGen.Protocol.Namespace}/{obj.Name}.xsd";

            XElement root = new XElement(XSDNamespace + "schema",
                new XAttribute("id", obj.Name),
                new XAttribute("targetNamespace", itemNamespace),
                new XAttribute("elementFormDefault", "qualified"),
                new XAttribute("xmlns", itemNamespace),
                new XAttribute(XNamespace.Xmlns + "mstns", itemNamespace),
                new XAttribute(XNamespace.Xmlns + "xs", XSDNamespace.NamespaceName));

            root.Add(
                new XElement(XSDNamespace + "element",
                    new XAttribute("name", obj.Name),
                    new XAttribute("type", $"{obj.Name}Type")));

            var typeElement = new XElement(XSDNamespace + "complexType",
                new XAttribute("name", $"{obj.Name}Type"));
            var choiceElement = new XElement(XSDNamespace + "choice",
                new XAttribute("minOccurs", 0),
                new XAttribute("maxOccurs", "unbounded"));
            typeElement.Add(choiceElement);
            root.Add(typeElement);
            foreach (var field in obj.IterateFields())
            {
                if (!this.TryGetTypeGeneration(field.GetType(), out var xmlGen))
                {
                    throw new ArgumentException("Unsupported type generator: " + field.GetType());
                }
                var elem = xmlGen.GenerateForXSD(
                    root,
                    choiceElement,
                    field,
                    nameOverride: null);
                elem.Add(new XAttribute("minOccurs", 0));
                elem.Add(new XAttribute("maxOccurs", 1));
            }

            var outputPath = Path.Combine(obj.TargetDir.FullName, $"{obj.Name}.xsd");
            var outputFile = new FileInfo(outputPath);
            obj.GeneratedFiles[Path.GetFullPath(outputPath)] = ProjItemType.None;
            using (var writer = new XmlTextWriter(outputPath, Encoding.ASCII))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 3;
                XDocument doc = new XDocument(root);
                doc.WriteTo(writer);
            }
        }

        protected override void GenerateCopyInSnippet(ObjectGeneration obj, FileGeneration fg, bool usingErrorMask)
        {
            using (var args = new ArgsWrapper(fg,
                $"LoquiXmlTranslation<{obj.ObjectName}, {(usingErrorMask ? obj.Mask(MaskType.Error) : obj.Mask_GenericAssumed(MaskType.Error))}>.Instance.CopyIn"))
            using (new DepthWrapper(fg))
            {
                foreach (var item in this.MainAPI.ReaderPassArgs)
                {
                    args.Add(item);
                }
                args.Add($"item: this");
                args.Add($"skipProtected: true");
                if (usingErrorMask)
                {
                    args.Add($"doMasks: true");
                    args.Add($"mask: out errorMask");
                }
                else
                {
                    args.Add($"doMasks: false");
                    args.Add($"mask: out var errorMask");
                }
                args.Add($"cmds: cmds");
            }
        }

        protected override void GenerateCreateSnippet(ObjectGeneration obj, FileGeneration fg)
        {
            fg.AppendLine($"{obj.Mask(MaskType.Error)} errMaskRet = null;");
            using (var args = new ArgsWrapper(fg,
                $"var ret = Create_{ModuleNickname}_Internal{ObjectGeneration.GenerateGenericClause(obj.GenericTypes_Nickname(MaskType.Error))}"))
            {
                args.Add("root: root");
                args.Add($"errorMask: doMasks ? () => errMaskRet ?? (errMaskRet = new {obj.Mask(MaskType.Error)}()) : default(Func<{obj.Mask(MaskType.Error)}>)");
            }
            fg.AppendLine($"return (ret, errMaskRet);");
        }

        protected override void GenerateWriteSnippet(ObjectGeneration obj, FileGeneration fg)
        {
            fg.AppendLine($"using (new ElementWrapper(writer, name ?? \"{obj.FullName}\"))");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("if (name != null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"writer.WriteAttributeString(\"{XmlConstants.TYPE_ATTRIBUTE}\", \"{obj.FullName}\");");
                }

                foreach (var field in obj.IterateFieldIndices())
                {
                    if (!this.TryGetTypeGeneration(field.Field.GetType(), out var generator))
                    {
                        throw new ArgumentException("Unsupported type generator: " + field.Field);
                    }

                    if (!generator.ShouldGenerateWrite(field.Field)) continue;

                    if (field.Field.HasBeenSet)
                    {
                        fg.AppendLine($"if (item.{field.Field.HasBeenSetAccessor})");
                    }
                    using (new BraceWrapper(fg, doIt: field.Field.HasBeenSet))
                    {
                        var maskType = this.Gen.MaskModule.GetMaskModule(field.Field.GetType()).GetErrorMaskTypeStr(field.Field);
                        generator.GenerateWrite(
                            fg: fg,
                            objGen: obj,
                            typeGen: field.Field,
                            writerAccessor: "writer",
                            itemAccessor: new Accessor(field.Field, "item."),
                            doMaskAccessor: "errorMask != null",
                            maskAccessor: $"errorMask",
                            nameAccessor: $"nameof(item.{field.Field.Name})");
                    }
                }
            }
        }
    }
}
