using Noggog;
using Loqui.Xml;
using System.Xml.Linq;
using System.Xml;
using System.Text;
using Loqui.Internal;
using System.Drawing;

namespace Loqui.Generation;

public class XmlTranslationModule : TranslationModule<XmlTranslationGeneration>
{
    public override string ModuleNickname => "Xml";
    public override string Namespace => "Loqui.Generation";
    public readonly static XNamespace XSDNamespace = "http://www.w3.org/2001/XMLSchema";
    public bool ShouldGenerateXSD = true;
    public FilePath ObjectXSDLocation(ObjectGeneration obj) => new FilePath(Path.Combine(obj.TargetDir.Path, ObjectXSDName(obj)));
    public string ObjectXSDName(ObjectGeneration obj) => $"{obj.Name}.xsd";
    public FilePath CommonXSDLocation(ProtocolGeneration proto) => new FilePath(Path.Combine(proto.GenerationFolder.FullName, "Common.xsd"));
    public string ObjectNamespace(ObjectGeneration obj) => $"{obj.ProtoGen.Protocol.Namespace}";
    public string ObjectType(ObjectGeneration obj) => $"{obj.Name}Type";
    public readonly static APILine PathLine = new APILine("Path", "string path");
    public readonly static APILine NameLine = new APILine("Name", "string? name = null");
    public readonly static APILine XElementLine = new APILine("XElement", "XElement node");
    public override bool GenerateAbstractCreates => true;

    public XmlTranslationModule(LoquiGenerator gen)
        : base(gen)
    {
        _typeGenerations[typeof(LoquiType)] = new LoquiXmlTranslationGeneration();
        _typeGenerations[typeof(BoolNullType)] = new PrimitiveXmlTranslationGeneration<bool?>();
        _typeGenerations[typeof(BoolType)] = new PrimitiveXmlTranslationGeneration<bool>();
        _typeGenerations[typeof(CharNullType)] = new PrimitiveXmlTranslationGeneration<char?>();
        _typeGenerations[typeof(CharType)] = new PrimitiveXmlTranslationGeneration<char>();
        _typeGenerations[typeof(DateTimeNullType)] = new PrimitiveXmlTranslationGeneration<DateTime?>();
        _typeGenerations[typeof(DateTimeType)] = new PrimitiveXmlTranslationGeneration<DateTime>();
        _typeGenerations[typeof(DoubleNullType)] = new PrimitiveXmlTranslationGeneration<double?>();
        _typeGenerations[typeof(DoubleType)] = new PrimitiveXmlTranslationGeneration<double>();
        _typeGenerations[typeof(EnumType)] = new EnumXmlTranslationGeneration();
        _typeGenerations[typeof(EnumNullType)] = new EnumXmlTranslationGeneration();
        _typeGenerations[typeof(FloatNullType)] = new PrimitiveXmlTranslationGeneration<float?>("Float");
        _typeGenerations[typeof(FloatType)] = new PrimitiveXmlTranslationGeneration<float>("Float");
        _typeGenerations[typeof(Int8NullType)] = new PrimitiveXmlTranslationGeneration<sbyte?>("Int8");
        _typeGenerations[typeof(Int8Type)] = new PrimitiveXmlTranslationGeneration<sbyte>("Int8");
        _typeGenerations[typeof(Int16NullType)] = new PrimitiveXmlTranslationGeneration<short?>();
        _typeGenerations[typeof(Int16Type)] = new PrimitiveXmlTranslationGeneration<short>();
        _typeGenerations[typeof(Int32NullType)] = new PrimitiveXmlTranslationGeneration<int?>();
        _typeGenerations[typeof(Int32Type)] = new PrimitiveXmlTranslationGeneration<int>();
        _typeGenerations[typeof(Int64NullType)] = new PrimitiveXmlTranslationGeneration<long?>();
        _typeGenerations[typeof(Int64Type)] = new PrimitiveXmlTranslationGeneration<long>();
        _typeGenerations[typeof(P2Int32NullType)] = new PrimitiveXmlTranslationGeneration<P2Int?>();
        _typeGenerations[typeof(P2Int32Type)] = new PrimitiveXmlTranslationGeneration<P2Int>();
        _typeGenerations[typeof(P2Int16NullType)] = new PrimitiveXmlTranslationGeneration<P2Int16?>();
        _typeGenerations[typeof(P2Int16Type)] = new PrimitiveXmlTranslationGeneration<P2Int16>();
        _typeGenerations[typeof(P2FloatNullType)] = new PrimitiveXmlTranslationGeneration<P2Float?>();
        _typeGenerations[typeof(P2FloatType)] = new PrimitiveXmlTranslationGeneration<P2Float>();
        _typeGenerations[typeof(P3FloatNullType)] = new PrimitiveXmlTranslationGeneration<P3Float?>();
        _typeGenerations[typeof(P3FloatType)] = new PrimitiveXmlTranslationGeneration<P3Float>();
        _typeGenerations[typeof(P3IntNullType)] = new PrimitiveXmlTranslationGeneration<P3Int?>();
        _typeGenerations[typeof(P3IntType)] = new PrimitiveXmlTranslationGeneration<P3Int>();
        _typeGenerations[typeof(P3UInt8NullType)] = new PrimitiveXmlTranslationGeneration<P3UInt8?>();
        _typeGenerations[typeof(P3UInt8Type)] = new PrimitiveXmlTranslationGeneration<P3UInt8>();
        _typeGenerations[typeof(P3UInt16NullType)] = new PrimitiveXmlTranslationGeneration<P3UInt16?>();
        _typeGenerations[typeof(P3UInt16Type)] = new PrimitiveXmlTranslationGeneration<P3UInt16>();
        _typeGenerations[typeof(P3Int16NullType)] = new PrimitiveXmlTranslationGeneration<P3Int16?>();
        _typeGenerations[typeof(P3Int16Type)] = new PrimitiveXmlTranslationGeneration<P3Int16>();
        _typeGenerations[typeof(P3DoubleNullType)] = new PrimitiveXmlTranslationGeneration<P3Double?>();
        _typeGenerations[typeof(P3DoubleType)] = new PrimitiveXmlTranslationGeneration<P3Double>();
        _typeGenerations[typeof(PercentNullType)] = new PrimitiveXmlTranslationGeneration<Percent?>();
        _typeGenerations[typeof(PercentType)] = new PercentXmlTranslationGeneration();
        _typeGenerations[typeof(RangeDoubleNullType)] = new PrimitiveXmlTranslationGeneration<RangeDouble?>();
        _typeGenerations[typeof(RangeDoubleType)] = new PrimitiveXmlTranslationGeneration<RangeDouble>();
        _typeGenerations[typeof(StringType)] = new PrimitiveXmlTranslationGeneration<string>(nullable: true) { CanBeNotNullable = false };
        _typeGenerations[typeof(TypicalRangedIntType<RangeInt8?>)] = new PrimitiveXmlTranslationGeneration<RangeInt8?>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeInt8>)] = new PrimitiveXmlTranslationGeneration<RangeInt8>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeInt16?>)] = new PrimitiveXmlTranslationGeneration<RangeInt16?>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeInt16>)] = new PrimitiveXmlTranslationGeneration<RangeInt16>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeInt32?>)] = new PrimitiveXmlTranslationGeneration<RangeInt32?>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeInt32>)] = new PrimitiveXmlTranslationGeneration<RangeInt32>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeInt64?>)] = new PrimitiveXmlTranslationGeneration<RangeInt64?>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeInt64>)] = new PrimitiveXmlTranslationGeneration<RangeInt64>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeUInt8?>)] = new PrimitiveXmlTranslationGeneration<RangeUInt8?>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeUInt8>)] = new PrimitiveXmlTranslationGeneration<RangeUInt8>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeUInt16?>)] = new PrimitiveXmlTranslationGeneration<RangeUInt16?>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeUInt16>)] = new PrimitiveXmlTranslationGeneration<RangeUInt16>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeUInt32?>)] = new PrimitiveXmlTranslationGeneration<RangeUInt32?>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeUInt32>)] = new PrimitiveXmlTranslationGeneration<RangeUInt32>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeUInt64?>)] = new PrimitiveXmlTranslationGeneration<RangeUInt64?>();
        _typeGenerations[typeof(TypicalRangedIntType<RangeUInt64>)] = new PrimitiveXmlTranslationGeneration<RangeUInt64>();
        _typeGenerations[typeof(UDoubleNullType)] = new PrimitiveXmlTranslationGeneration<UDouble?>();
        _typeGenerations[typeof(UDoubleType)] = new PrimitiveXmlTranslationGeneration<UDouble>();
        _typeGenerations[typeof(UInt8NullType)] = new PrimitiveXmlTranslationGeneration<byte?>();
        _typeGenerations[typeof(UInt8Type)] = new PrimitiveXmlTranslationGeneration<byte>();
        _typeGenerations[typeof(UInt16NullType)] = new PrimitiveXmlTranslationGeneration<ushort?>();
        _typeGenerations[typeof(UInt16Type)] = new PrimitiveXmlTranslationGeneration<ushort>();
        _typeGenerations[typeof(UInt32NullType)] = new PrimitiveXmlTranslationGeneration<uint?>();
        _typeGenerations[typeof(UInt32Type)] = new PrimitiveXmlTranslationGeneration<uint>();
        _typeGenerations[typeof(UInt64NullType)] = new PrimitiveXmlTranslationGeneration<ulong?>();
        _typeGenerations[typeof(UInt64Type)] = new PrimitiveXmlTranslationGeneration<ulong>();
        _typeGenerations[typeof(FilePathType)] = new PrimitiveXmlTranslationGeneration<FilePath>();
        _typeGenerations[typeof(FilePathNullType)] = new PrimitiveXmlTranslationGeneration<FilePath?>();
        _typeGenerations[typeof(DirectoryPathType)] = new PrimitiveXmlTranslationGeneration<DirectoryPath>();
        _typeGenerations[typeof(DirectoryPathNullType)] = new PrimitiveXmlTranslationGeneration<DirectoryPath?>();
        _typeGenerations[typeof(ListType)] = new ListXmlTranslationGeneration();
        _typeGenerations[typeof(DictType)] = new DictXmlTranslationGeneration();
        _typeGenerations[typeof(ByteArrayType)] = new ByteArrayXmlTranslationGeneration();
        _typeGenerations[typeof(NothingType)] = new NothingXmlTranslationGeneration();
        _typeGenerations[typeof(ColorType)] = new PrimitiveXmlTranslationGeneration<Color>();
        MainAPI = new TranslationModuleAPI(
            writerAPI: new MethodAPI(
                majorAPI: new APILine[] { XElementLine },
                customAPI: null,
                optionalAPI: new APILine[] { NameLine }),
            readerAPI: new MethodAPI(
                majorAPI: new APILine[] { XElementLine },
                customAPI: null,
                optionalAPI: null));
        MinorAPIs.Add(
            new TranslationModuleAPI(
                writerAPI: new MethodAPI(
                    majorAPI: new APILine[] { PathLine },
                    customAPI: null,
                    optionalAPI: new APILine[] { NameLine }),
                readerAPI: new MethodAPI(
                    majorAPI: new APILine[] { PathLine },
                    customAPI: null,
                    optionalAPI: null))
            {
                Funnel = new TranslationFunnel(
                    MainAPI,
                    ConvertFromPathOut,
                    ConvertFromPathIn)
            });
        var stream = new APILine("Stream", "Stream stream");
        MinorAPIs.Add(
            new TranslationModuleAPI(
                writerAPI: new MethodAPI(
                    majorAPI: new APILine[] { stream },
                    customAPI: null,
                    optionalAPI: new APILine[] { NameLine }),
                readerAPI: new MethodAPI(
                    majorAPI: new APILine[] { stream },
                    customAPI: null,
                    optionalAPI: null))
            {
                Funnel = new TranslationFunnel(
                    MainAPI,
                    ConvertFromStreamOut,
                    ConvertFromStreamIn)
            });
    }

    public override async Task LoadWrapup(ObjectGeneration obj)
    {
        await base.LoadWrapup(obj);
        lock (_typeGenerations)
        {
            foreach (var gen in _typeGenerations.Values)
            {
                gen.XmlMod = this;
                gen.MaskModule = Gen.MaskModule;
            }
        }
    }

    public override async IAsyncEnumerable<string> RequiredUsingStatements(ObjectGeneration obj)
    {
        yield return "System.Xml";
        yield return "System.Xml.Linq";
        yield return "System.IO";
        yield return "Noggog.Xml";
        yield return "Loqui.Xml";
        yield return "Loqui.Internal";
        await foreach (var item in base.RequiredUsingStatements(obj))
        {
            yield return item;
        }
    }

    private void ConvertFromStreamOut(ObjectGeneration obj, StructuredStringBuilder sb, InternalTranslation internalToDo)
    {
        sb.AppendLine($"var {XElementLine.GetParameterName(obj)} = new XElement(\"topnode\");");
        internalToDo(MainAPI.WriterAPI.IterateAPI(obj, TranslationDirection.Writer).Select(a => a.API).ToArray());
        sb.AppendLine($"{XElementLine.GetParameterName(obj)}.Elements().First().Save(stream);");
    }

    private void ConvertFromStreamIn(ObjectGeneration obj, StructuredStringBuilder sb, InternalTranslation internalToDo)
    {
        sb.AppendLine($"var {XElementLine.GetParameterName(obj)} = XDocument.Load(stream).Root;");
        internalToDo(MainAPI.ReaderAPI.IterateAPI(obj, TranslationDirection.Reader).Select(a => a.API).ToArray());
    }

    private void ConvertFromPathOut(ObjectGeneration obj, StructuredStringBuilder sb, InternalTranslation internalToDo)
    {
        sb.AppendLine($"var {XElementLine.GetParameterName(obj)} = new XElement(\"topnode\");");
        internalToDo(MainAPI.WriterAPI.IterateAPI(obj, TranslationDirection.Writer).Select(a => a.API).ToArray());
        sb.AppendLine($"{XElementLine.GetParameterName(obj)}.Elements().First().SaveIfChanged(path);");
    }

    private void ConvertFromPathIn(ObjectGeneration obj, StructuredStringBuilder sb, InternalTranslation internalToDo)
    {
        sb.AppendLine($"var {XElementLine.GetParameterName(obj)} = XDocument.Load(path).Root;");
        internalToDo(MainAPI.ReaderAPI.IterateAPI(obj, TranslationDirection.Reader).Select(a => a.API).ToArray());
    }

    protected virtual void FillPrivateElement(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        if (obj.IterateFields(includeBaseClass: true).Any(f => f.ReadOnly))
        {
            using (var args = new FunctionWrapper(sb,
                       $"protected static void FillPrivateElement{ModuleNickname}"))
            {
                args.Add($"{obj.ObjectName} item");
                args.Add($"XElement {XElementLine.GetParameterName(obj)}");
                args.Add("string name");
                args.Add($"ErrorMaskBuilder errorMask");
                args.Add($"{nameof(TranslationCrystal)} translationMask");
            }
            using (sb.CurlyBrace())
            {
                sb.AppendLine("switch (name)");
                using (sb.CurlyBrace())
                {
                    foreach (var field in obj.IterateFields())
                    {
                        if (field.Derivative) continue;
                        if (!field.ReadOnly) continue;
                        if (!TryGetTypeGeneration(field.GetType(), out var generator))
                        {
                            throw new ArgumentException("Unsupported type generator: " + field);
                        }

                        sb.AppendLine($"case \"{field.Name}\":");
                        using (new DepthWrapper(sb))
                        {
                            if (generator.ShouldGenerateCopyIn(field))
                            {
                                List<string> conditions = new List<string>();
                                if (TranslationMaskParameter)
                                {
                                    conditions.Add(field.GetTranslationIfAccessor("translationMask"));
                                }
                                if (conditions.Count > 0)
                                {
                                    using (var args = new IfWrapper(sb, ANDs: true))
                                    {
                                        foreach (var item in conditions)
                                        {
                                            args.Add(item);
                                        }
                                    }
                                }
                                using (sb.CurlyBrace(doIt: conditions.Count > 0))
                                {
                                    generator.GenerateCopyIn(
                                        sb: sb,
                                        objGen: obj,
                                        typeGen: field,
                                        nodeAccessor: XElementLine.GetParameterName(obj).Result,
                                        itemAccessor: Accessor.FromType(field, "item"),
                                        translationMaskAccessor: "translationMask",
                                        errorMaskAccessor: $"errorMask");
                                }
                            }
                            sb.AppendLine("break;");
                        }
                    }

                    sb.AppendLine("default:");
                    using (new DepthWrapper(sb))
                    {
                        if (obj.HasLoquiBaseObject)
                        {
                            using (var args = new ArgsWrapper(sb,
                                       $"{obj.BaseClassName}.FillPrivateElement_" +
                                       $"{ModuleNickname}{obj.GetBaseMask_GenericTypes(MaskType.Error)}"))
                            {
                                args.Add("item: item");
                                args.Add($"{XElementLine.GetParameterName(obj)}: {XElementLine.GetParameterName(obj)}");
                                args.Add("name: name");
                                args.Add("errorMask: errorMask");
                                if (TranslationMaskParameter)
                                {
                                    args.Add($"translationMask: translationMask");
                                }
                            }
                        }
                        sb.AppendLine("break;");
                    }
                }
            }
            sb.AppendLine();
        }
    }

    public override async Task GenerateInTranslationWriteClass(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        GenerateWriteToNode(obj, sb);

        await base.GenerateInTranslationWriteClass(obj, sb);
    }

    public override async Task GenerateInTranslationCreateClass(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        using (var args = new FunctionWrapper(sb,
                   $"public static void FillPublic{ModuleNickname}"))
        {
            args.Add($"{obj.Interface(getter: false, internalInterface: true)} item");
            args.Add($"XElement {XElementLine.GetParameterName(obj)}");
            foreach (var item in MainAPI.ReaderAPI.CustomAPI)
            {
                if (!item.API.TryResolve(obj, TranslationDirection.Reader, out var line)) continue;
                args.Add(line.Result);
            }
            args.Add($"ErrorMaskBuilder? errorMask");
            args.Add($"{nameof(TranslationCrystal)}? translationMask");
        }
        using (sb.CurlyBrace())
        {
            sb.AppendLine("try");
            using (sb.CurlyBrace())
            {
                sb.AppendLine($"foreach (var elem in {XElementLine.GetParameterName(obj)}.Elements())");
                using (sb.CurlyBrace())
                {
                    using (var args = new ArgsWrapper(sb,
                               $"{TranslationCreateClass(obj)}.FillPublicElement{ModuleNickname}"))
                    {
                        args.Add("item: item");
                        args.Add($"{XElementLine.GetParameterName(obj)}: elem");
                        args.Add("name: elem.Name.LocalName");
                        args.Add("errorMask: errorMask");
                        if (TranslationMaskParameter)
                        {
                            args.Add("translationMask: translationMask");
                        }
                        foreach (var item in MainAPI.ReaderAPI.CustomAPI)
                        {
                            if (!item.API.TryGetPassthrough(obj, TranslationDirection.Reader, out var passthrough)) continue;
                            args.Add(passthrough);
                        }
                    }
                }
            }
            sb.AppendLine("catch (Exception ex)");
            sb.AppendLine("when (errorMask != null)");
            using (sb.CurlyBrace())
            {
                sb.AppendLine("errorMask.ReportException(ex);");
            }
        }
        sb.AppendLine();

        FillPublicElement(obj, sb);
        await base.GenerateInTranslationCreateClass(obj, sb);
    }

    public virtual void GenerateWriteToNode(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        using (var args = new FunctionWrapper(sb,
                   $"public static void WriteToNode{ModuleNickname}{obj.GetGenericTypes(MaskType.Normal)}"))
        {
            args.Add($"{obj.Interface(internalInterface: true, getter: true)} item");
            args.Add($"XElement {XElementLine.GetParameterName(obj)}");
            args.Add($"ErrorMaskBuilder? errorMask");
            args.Add($"{nameof(TranslationCrystal)}? translationMask");
        }
        using (sb.CurlyBrace())
        {
            if (obj.HasLoquiBaseObject)
            {
                using (var args = new ArgsWrapper(sb,
                           $"{TranslationWriteClass(obj.BaseClass)}.WriteToNode{ModuleNickname}"))
                {
                    args.Add($"item: item");
                    args.Add($"{XElementLine.GetParameterName(obj)}: {XElementLine.GetParameterName(obj)}");
                    args.Add($"errorMask: errorMask");
                    args.Add($"translationMask: translationMask");
                }
            }
            foreach (var field in obj.IterateFieldIndices())
            {
                if (!TryGetTypeGeneration(field.Field.GetType(), out var generator))
                {
                    throw new ArgumentException("Unsupported type generator: " + field.Field);
                }

                if (!generator.ShouldGenerateWrite(field.Field)) continue;

                List<string> conditions = new List<string>();
                if (field.Field.Nullable)
                {
                    conditions.Add($"{field.Field.NullableAccessor(getter: true, accessor: Accessor.FromType(field.Field, "item"))}");
                }
                if (TranslationMaskParameter)
                {
                    conditions.Add(field.Field.GetTranslationIfAccessor("translationMask"));
                }
                if (conditions.Count > 0)
                {
                    using (var args = new IfWrapper(sb, ANDs: true))
                    {
                        foreach (var item in conditions)
                        {
                            args.Add(item);
                        }
                    }
                }
                using (sb.CurlyBrace(doIt: conditions.Count > 0))
                {
                    var maskType = Gen.MaskModule.GetMaskModule(field.Field.GetType()).GetErrorMaskTypeStr(field.Field);
                    generator.GenerateWrite(
                        sb: sb,
                        objGen: obj,
                        typeGen: field.Field,
                        writerAccessor: $"{XElementLine.GetParameterName(obj)}",
                        itemAccessor: Accessor.FromType(field.Field, "item"),
                        errorMaskAccessor: $"errorMask",
                        translationMaskAccessor: "translationMask",
                        nameAccessor: $"nameof(item.{field.Field.Name})");
                }
            }
        }
        sb.AppendLine();
    }

    protected virtual void FillPublicElement(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        using (var args = new FunctionWrapper(sb,
                   $"public static void FillPublicElement{ModuleNickname}"))
        {
            args.Add($"{obj.Interface(getter: false)} item");
            args.Add($"XElement {XElementLine.GetParameterName(obj)}");
            args.Add("string name");
            args.Add($"ErrorMaskBuilder? errorMask");
            args.Add($"{nameof(TranslationCrystal)}? translationMask");
        }
        using (sb.CurlyBrace())
        {
            sb.AppendLine("switch (name)");
            using (sb.CurlyBrace())
            {
                foreach (var field in obj.IterateFields())
                {
                    if (field.Derivative) continue;
                    if (field.ReadOnly) continue;
                    if (!TryGetTypeGeneration(field.GetType(), out var generator))
                    {
                        throw new ArgumentException("Unsupported type generator: " + field);
                    }

                    sb.AppendLine($"case \"{field.Name}\":");
                    using (new DepthWrapper(sb))
                    {
                        if (generator.ShouldGenerateCopyIn(field))
                        {
                            List<string> conditions = new List<string>();
                            if (TranslationMaskParameter)
                            {
                                conditions.Add(field.GetTranslationIfAccessor("translationMask"));
                            }
                            if (conditions.Count > 0)
                            {
                                using (var args = new IfWrapper(sb, ANDs: true))
                                {
                                    foreach (var item in conditions)
                                    {
                                        args.Add(item);
                                    }
                                }
                            }
                            using (sb.CurlyBrace(doIt: conditions.Count > 0))
                            {
                                generator.GenerateCopyIn(
                                    sb: sb,
                                    objGen: obj,
                                    typeGen: field,
                                    nodeAccessor: XElementLine.GetParameterName(obj).Result,
                                    itemAccessor: Accessor.FromType(field, "item"),
                                    translationMaskAccessor: "translationMask",
                                    errorMaskAccessor: $"errorMask");
                            }
                        }
                        sb.AppendLine("break;");
                    }
                }

                sb.AppendLine("default:");
                using (new DepthWrapper(sb))
                {
                    if (obj.HasLoquiBaseObject)
                    {
                        using (var args = new ArgsWrapper(sb,
                                   $"{obj.BaseClass.CommonClassName(LoquiInterfaceType.ISetter)}.FillPublicElement{ModuleNickname}{obj.GetBaseMask_GenericTypes(MaskType.Error)}"))
                        {
                            args.Add("item: item");
                            args.Add($"{XElementLine.GetParameterName(obj)}: {XElementLine.GetParameterName(obj)}");
                            args.Add("name: name");
                            args.Add("errorMask: errorMask");
                            if (TranslationMaskParameter)
                            {
                                args.Add($"translationMask: translationMask");
                            }
                        }
                    }
                    sb.AppendLine("break;");
                }
            }
        }
        sb.AppendLine();
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

        if (obj.HasLoquiBaseObject)
        {
            FilePath xsdPath = ObjectXSDLocation(obj.BaseClass);
            var relativePath = xsdPath.GetRelativePathTo(obj.TargetDir);
            root.Add(
                new XElement(
                    XSDNamespace + "include",
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
        if (obj.HasLoquiBaseObject)
        {
            typeElement.Add(
                new XElement(
                    XSDNamespace + "complexContent",
                    new XElement(
                        XSDNamespace + "extension",
                        new XAttribute("base", ObjectType(obj.BaseClass)),
                        choiceElement)));
        }
        else
        {
            typeElement.Add(choiceElement);
        }
        root.Add(typeElement);
        foreach (var field in obj.IterateFields())
        {
            if (!TryGetTypeGeneration(field.GetType(), out var xmlGen))
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

        var outputPath = Path.Combine(obj.TargetDir.Path, $"{obj.Name}.xsd");
        obj.GeneratedFiles[Path.GetFullPath(outputPath)] = ProjItemType.None;
        using (var writer = new XmlTextWriter(outputPath, Encoding.Default))
        {
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 3;
            XDocument doc = new XDocument(root);
            doc.WriteTo(writer);
        }
        using var streamWriter = File.AppendText(outputPath);
        streamWriter.Write(Environment.NewLine);
    }

    public override async Task FinalizeGeneration(ProtocolGeneration proto)
    {
        GenerateCommonXSDForProto(proto);
        await base.FinalizeGeneration(proto);
    }

    public void GenerateCommonXSDForProto(ProtocolGeneration protoGen)
    {
        if (!ShouldGenerateXSD) return;
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
                if (!TryGetTypeGeneration(field.GetType(), out var xmlGen))
                {
                    throw new ArgumentException("Unsupported type generator: " + field.GetType());
                }
                xmlGen.GenerateForCommonXSD(
                    root,
                    field);
            }
        }

        var outputPath = CommonXSDLocation(protoGen);
        protoGen.GeneratedFiles[outputPath.Path] = ProjItemType.None;
        using (var writer = new XmlTextWriter(outputPath.Path, Encoding.Default))
        {
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 3;
            XDocument doc = new XDocument(root);
            doc.WriteTo(writer);
        }
        using var streamWriter = File.AppendText(outputPath.Path);
        streamWriter.Write(Environment.NewLine);
    }

    protected virtual async Task PreCreateLoop(ObjectGeneration obj, StructuredStringBuilder sb)
    {
    }

    protected virtual async Task PostCreateLoop(ObjectGeneration obj, StructuredStringBuilder sb)
    {
    }

    protected override async Task GenerateNewSnippet(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        if (obj.Abstract)
        {
            sb.AppendLine($"if (!LoquiXmlTranslation.Instance.TryCreate<{obj.Name}>(node, out var ret, errorMask, translationMask))");
            using (sb.CurlyBrace())
            {
                sb.AppendLine($"throw new ArgumentException($\"Unknown {obj.Name} subclass: {{node.Name.LocalName}}\");");
            }
        }
        else
        {
            sb.AppendLine($"var ret = new {obj.Name}{obj.GetGenericTypes(MaskType.Normal)}();");
        }
    }

    protected override async Task GenerateCopyInSnippet(ObjectGeneration obj, StructuredStringBuilder sb, Accessor accessor)
    {
        sb.AppendLine("try");
        using (sb.CurlyBrace())
        {
            await PreCreateLoop(obj, sb);
            sb.AppendLine($"foreach (var elem in {XElementLine.GetParameterName(obj)}.Elements())");
            using (sb.CurlyBrace())
            {
                if (obj.IterateFields(includeBaseClass: true).Any(f => f.ReadOnly))
                {
                    using (var args = new ArgsWrapper(sb,
                               $"FillPrivateElement{ModuleNickname}"))
                    {
                        args.Add($"item: {accessor}");
                        args.Add($"{XElementLine.GetParameterName(obj)}: elem");
                        args.Add("name: elem.Name.LocalName");
                        foreach (var item in MainAPI.ReaderAPI.CustomAPI)
                        {
                            if (!item.API.TryGetPassthrough(obj, TranslationDirection.Reader, out var passthrough)) continue;
                            args.Add(passthrough);
                        }
                        args.Add("errorMask: errorMask");
                        if (TranslationMaskParameter)
                        {
                            args.Add("translationMask: translationMask");
                        }
                    }
                }
                using (var args = new ArgsWrapper(sb,
                           $"{TranslationCreateClass(obj)}.FillPublicElement{ModuleNickname}"))
                {
                    args.Add($"item: {accessor}");
                    args.Add($"{XElementLine.GetParameterName(obj)}: elem");
                    args.Add("name: elem.Name.LocalName");
                    foreach (var item in MainAPI.ReaderAPI.CustomAPI)
                    {
                        if (!item.API.TryGetPassthrough(obj, TranslationDirection.Reader, out var passthrough)) continue;
                        args.Add(passthrough);
                    }
                    args.Add("errorMask: errorMask");
                    if (TranslationMaskParameter)
                    {
                        args.Add("translationMask: translationMask");
                    }
                }
            }
            await PostCreateLoop(obj, sb);
        }
        sb.AppendLine("catch (Exception ex)");
        sb.AppendLine("when (errorMask != null)");
        using (sb.CurlyBrace())
        {
            sb.AppendLine("errorMask.ReportException(ex);");
        }
    }

    protected override async Task GenerateWriteSnippet(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        sb.AppendLine($"var elem = new XElement(name ?? \"{obj.FullName}\");");
        sb.AppendLine($"{XElementLine.GetParameterName(obj)}.Add(elem);");
        sb.AppendLine("if (name != null)");
        using (sb.CurlyBrace())
        {
            sb.AppendLine($"elem.SetAttributeValue(\"{XmlConstants.TYPE_ATTRIBUTE}\", \"{obj.FullName}\");");
        }
        using (var args = new ArgsWrapper(sb,
                   $"WriteToNode{ModuleNickname}"))
        {
            args.Add($"item: item");
            args.Add($"{XElementLine.GetParameterName(obj)}: elem");
            foreach (var item in MainAPI.ReaderAPI.CustomAPI)
            {
                if (!item.API.TryGetPassthrough(obj, TranslationDirection.Reader, out var passthrough)) continue;
                args.Add(passthrough);
            }
            args.Add($"errorMask: errorMask");
            args.Add($"translationMask: translationMask");
        }
    }

    public override async Task GenerateInCommon(ObjectGeneration obj, StructuredStringBuilder sb, MaskTypeSet maskTypes)
    {
        if (maskTypes.Applicable(LoquiInterfaceType.ISetter, CommonGenerics.Class, MaskType.Normal))
        {
            FillPrivateElement(obj, sb);
        }
        await base.GenerateInCommon(obj, sb, maskTypes);
    }

    public override void ReplaceTypeAssociation<Target, Replacement>()
    {
        if (!_typeGenerations.TryGetValue(typeof(Target), out var gen))
        {
            throw new ArgumentException();
        }
        _typeGenerations[typeof(Replacement)] = gen;
    }
}