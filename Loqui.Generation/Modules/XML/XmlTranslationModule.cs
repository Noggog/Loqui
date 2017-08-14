using Noggog;
using System;
using System.Linq;
using System.Collections.Generic;
using Loqui.Xml;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Text;

namespace Loqui.Generation
{
    public class XmlTranslationModule : TranslationModule<XmlTranslationGeneration>
    {
        public override string ModuleNickname => "XML";
        public override string Namespace => "Loqui.Generation";
        public readonly static XNamespace XSDNamespace = "http://www.w3.org/2001/XMLSchema";

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
            this._typeGenerations[typeof(UnsafeType)] = new UnsafeXmlTranslationGeneration();
            this._typeGenerations[typeof(WildcardType)] = new UnsafeXmlTranslationGeneration();
            this._typeGenerations[typeof(ListType)] = new ListXmlTranslationGeneration();
            this._typeGenerations[typeof(DictType)] = new DictXmlTranslationGeneration();
            this._typeGenerations[typeof(ByteArrayType)] = new PrimitiveXmlTranslationGeneration<byte[]>(typeName: "ByteArray", nullable: true);
        }

        public override void Load()
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
        }

        public override IEnumerable<string> Interfaces(ObjectGeneration obj)
        {
            yield break;
        }

        public override void GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            GenerateRead(obj, fg);
            if (obj.IsTopClass)
            {
                fg.AppendLine($"public void Write_{ModuleNickname}(Stream stream)");
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{obj.ExtCommonName}.Write_{ModuleNickname}"))
                    {
                        args.Add("this");
                        args.Add("stream");
                    }
                }
                fg.AppendLine();

                fg.AppendLine($"public void Write_{ModuleNickname}(string path)");
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{obj.ExtCommonName}.Write_{ModuleNickname}"))
                    {
                        args.Add("this");
                        args.Add("path");
                    }
                }
                fg.AppendLine();
            }

            fg.AppendLine($"public void Write_{ModuleNickname}(Stream stream, out {obj.ErrorMask} errorMask)");
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"{obj.ExtCommonName}.Write_{ModuleNickname}"))
                {
                    args.Add("this");
                    args.Add("stream");
                    args.Add("out errorMask");
                }
            }
            fg.AppendLine();

            fg.AppendLine($"public void Write_{ModuleNickname}(string path, out {obj.ErrorMask} errorMask)");
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"{obj.ExtCommonName}.Write_{ModuleNickname}"))
                {
                    args.Add("this");
                    args.Add("path");
                    args.Add("out errorMask");
                }
            }
            fg.AppendLine();

            fg.AppendLine($"public void Write_{ModuleNickname}(XmlWriter writer, out {obj.ErrorMask} errorMask, string name = null)");
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"{obj.ExtCommonName}.Write_{ModuleNickname}"))
                {
                    args.Add($"writer: writer");
                    args.Add($"name: name");
                    args.Add($"item: this");
                    args.Add($"doMasks: true");
                    args.Add($"errorMask: out errorMask");
                }
            }
            fg.AppendLine();

            if (obj.Abstract)
            {
                if (!obj.BaseClass?.Abstract ?? true)
                {
                    fg.AppendLine($"public abstract void Write_{ModuleNickname}(XmlWriter writer, string name = null);");
                    fg.AppendLine();
                }
            }
            else if (obj.IsTopClass
                || (!obj.Abstract && (obj.BaseClass?.Abstract ?? true)))
            {
                fg.AppendLine($"public{obj.FunctionOverride}void Write_{ModuleNickname}(XmlWriter writer, string name = null)");
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{obj.ExtCommonName}.Write_{ModuleNickname}"))
                    {
                        args.Add($"writer: writer");
                        args.Add($"name: name");
                        args.Add($"item: this");
                        args.Add($"doMasks: false");
                        args.Add($"errorMask: out {obj.ErrorMask} errorMask");
                    }
                }
                fg.AppendLine();
            }
        }

        private void GenerateRead(ObjectGeneration obj, FileGeneration fg)
        {
            if (!obj.Abstract)
            {
                GenerateXmlCreate(obj, fg);
            }

            if (obj is StructGeneration) return;
            using (var args = new FunctionWrapper(fg,
                $"public{obj.FunctionOverride}void CopyIn_{ModuleNickname}"))
            {
                args.Add("XElement root");
                args.Add("NotifyingFireParameters? cmds = null");
            }
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"LoquiXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.CopyIn"))
                using (new DepthWrapper(fg))
                {
                    args.Add($"root: root");
                    args.Add($"item: this");
                    args.Add($"skipProtected: true");
                    args.Add($"doMasks: false");
                    args.Add($"mask: out {obj.ErrorMask} errorMask");
                    args.Add($"cmds: cmds");
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public virtual void CopyIn_{ModuleNickname}"))
            {
                args.Add("XElement root");
                args.Add($"out {obj.ErrorMask} errorMask");
                args.Add("NotifyingFireParameters? cmds = null");
            }
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"LoquiXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.CopyIn"))
                {
                    args.Add($"root: root");
                    args.Add($"item: this");
                    args.Add($"skipProtected: true");
                    args.Add($"doMasks: true");
                    args.Add($"mask: out errorMask");
                    args.Add($"cmds: cmds");
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public{obj.FunctionOverride}void CopyIn_{ModuleNickname}"))
            {
                args.Add("string path");
                args.Add("NotifyingFireParameters? cmds = null");
            }
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"LoquiXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.CopyIn"))
                using (new DepthWrapper(fg))
                {
                    args.Add($"root: XDocument.Load(path).Root");
                    args.Add($"item: this");
                    args.Add($"skipProtected: true");
                    args.Add($"doMasks: false");
                    args.Add($"mask: out {obj.ErrorMask} errorMask");
                    args.Add($"cmds: cmds");
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public virtual void CopyIn_{ModuleNickname}"))
            {
                args.Add($"string path");
                args.Add($"out {obj.ErrorMask} errorMask");
                args.Add($"NotifyingFireParameters? cmds = null");
            }
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                $"LoquiXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.CopyIn"))
                {
                    args.Add($"root: XDocument.Load(path).Root");
                    args.Add($"item: this");
                    args.Add($"skipProtected: true");
                    args.Add($"doMasks: true");
                    args.Add($"mask: out errorMask");
                    args.Add($"cmds: cmds");
                }
            }
            fg.AppendLine();

            foreach (var baseClass in obj.BaseClassTrail())
            {
                using (var args = new FunctionWrapper(fg,
                    $"public override void CopyIn_{ModuleNickname}"))
                {
                    args.Add($"XElement root");
                    args.Add($"out {baseClass.ErrorMask} errorMask");
                    args.Add($"NotifyingFireParameters? cmds = null");
                }
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg,
                        $"CopyIn_{ModuleNickname}"))
                    {
                        args.Add($"root");
                        args.Add($"out {obj.ErrorMask} errMask");
                        args.Add($"cmds: cmds");
                    }
                    fg.AppendLine("errorMask = errMask;");
                }
                fg.AppendLine();
            }
        }

        public override void Modify(ObjectGeneration obj)
        {
        }

        public override void Modify(LoquiGenerator gen)
        {
        }

        public override void GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public override void Generate(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public override IEnumerable<string> GetWriterInterfaces(ObjectGeneration obj)
        {
            yield break;
        }

        public override IEnumerable<string> GetReaderInterfaces(ObjectGeneration obj)
        {
            yield break;
        }

        public override void GenerateInCommonExt(ObjectGeneration obj, FileGeneration fg)
        {
            using (new RegionWrapper(fg, $"{ModuleNickname} Write"))
            {
                CommonXmlWrite(obj, fg);
            }
        }

        private void GenerateXmlCreate(ObjectGeneration obj, FileGeneration fg)
        {
            fg.AppendLine($"public{obj.NewOverride}static {obj.ObjectName} Create_{ModuleNickname}(Stream stream)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"using (var reader = new StreamReader(stream))");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"return Create_{ModuleNickname}(XElement.Parse(reader.ReadToEnd()));");
                }
            }
            fg.AppendLine();

            fg.AppendLine($"public{obj.NewOverride}static {obj.ObjectName} Create_{ModuleNickname}(XElement root)");
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"return Create_{ModuleNickname}"))
                {
                    args.Add("root: root");
                    args.Add("doMasks: false");
                    args.Add("errorMask: out var errorMask");
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public static {obj.ObjectName} Create_{ModuleNickname}"))
            {
                args.Add("XElement root");
                args.Add($"out {obj.ErrorMask} errorMask");
            }
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"return Create_{ModuleNickname}"))
                {
                    args.Add("root: root");
                    args.Add("doMasks: true");
                    args.Add("errorMask: out errorMask");
                }
            }
            fg.AppendLine();

            fg.AppendLine($"public{obj.NewOverride}static {obj.ObjectName} Create_{ModuleNickname}(string path)");
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                $"return Create_{ModuleNickname}"))
                {
                    args.Add("root: XDocument.Load(path).Root");
                    args.Add("doMasks: false");
                    args.Add("errorMask: out var errorMask");
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public static {obj.ObjectName} Create_{ModuleNickname}"))
            {
                args.Add("string path");
                args.Add($"out {obj.ErrorMask} errorMask");
            }
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                $"return Create_{ModuleNickname}"))
                {
                    args.Add("root: XDocument.Load(path).Root");
                    args.Add("doMasks: true");
                    args.Add("errorMask: out errorMask");
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public static {obj.ObjectName} Create_{ModuleNickname}"))
            {
                args.Add("XElement root");
                args.Add("bool doMasks");
                args.Add($"out {obj.ErrorMask} errorMask");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{obj.ErrorMask} errMaskRet = null;");
                using (var args = new ArgsWrapper(fg,
                    $"var ret = Create_{ModuleNickname}_Internal"))
                {
                    args.Add("root: root");
                    args.Add("doMasks: doMasks");
                    args.Add($"errorMask: doMasks ? () => errMaskRet ?? (errMaskRet = new {obj.ErrorMask}()) : default(Func<{obj.ErrorMask}>)");
                }
                fg.AppendLine($"errorMask = errMaskRet;");
                fg.AppendLine($"return ret;");
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"private static {obj.ObjectName} Create_{ModuleNickname}_Internal"))
            {
                args.Add("XElement root");
                args.Add("bool doMasks");
                args.Add($"Func<{obj.ErrorMask}> errorMask");
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
                            $"Fill_{ModuleNickname}_Internal"))
                        {
                            args.Add("item: ret");
                            args.Add("root: elem");
                            args.Add("name: elem.Name.LocalName");
                            args.Add("doMasks: doMasks");
                            args.Add("errorMask: errorMask");
                        }
                    }
                }
                fg.AppendLine("catch (Exception ex)");
                fg.AppendLine("when (doMasks)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("errorMask().Overall = ex;");
                }
                fg.AppendLine("return ret;");
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"protected static void Fill_{ModuleNickname}_Internal"))
            {
                args.Add($"{obj.ObjectName} item");
                args.Add("XElement root");
                args.Add("string name");
                args.Add("bool doMasks");
                args.Add($"Func<{obj.ErrorMask}> errorMask");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("switch (name)");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in obj.IterateFields())
                    {
                        if (!this.TryGetTypeGeneration(field.Field.GetType(), out var generator))
                        {
                            throw new ArgumentException("Unsupported type generator: " + field.Field);
                        }

                        fg.AppendLine($"case \"{field.Field.Name}\":");
                        using (new DepthWrapper(fg))
                        {
                            if (generator.ShouldGenerateCopyIn(field.Field))
                            {
                                using (new BraceWrapper(fg))
                                {
                                    var maskType = this.Gen.MaskModule.GetMaskModule(field.Field.GetType()).GetErrorMaskTypeStr(field.Field);
                                    fg.AppendLine($"{maskType} subMask;");
                                    generator.GenerateCopyIn(
                                        fg: fg,
                                        typeGen: field.Field,
                                        nodeAccessor: "root",
                                        itemAccessor: $"item.{field.Field.ProtectedName}",
                                        doMaskAccessor: "doMasks",
                                        maskAccessor: $"subMask");
                                    fg.AppendLine("if (doMasks && subMask != null)");
                                    using (new BraceWrapper(fg))
                                    {
                                        fg.AppendLine($"errorMask().{field.Field.Name} = subMask;");
                                    }
                                }
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
                                $"{obj.BaseClassName}.Fill_{ModuleNickname}_Internal"))
                            {
                                args.Add("item: item");
                                args.Add("root: root");
                                args.Add("name: name");
                                args.Add("doMasks: doMasks");
                                args.Add("errorMask: errorMask");
                            }
                        }
                        fg.AppendLine("break;");
                    }
                }
            }
            fg.AppendLine();
        }

        private void CommonXmlWrite(ObjectGeneration obj, FileGeneration fg)
        {
            using (var args = new FunctionWrapper(fg,
                $"public static void Write_{ModuleNickname}{obj.GenericTypes}",
                obj.GenerateWhereClauses().ToArray()))
            {
                args.Add($"{obj.Getter_InterfaceStr} item");
                args.Add($"Stream stream");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("using (var writer = new XmlTextWriter(stream, Encoding.ASCII))");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("writer.Formatting = Formatting.Indented;");
                    fg.AppendLine("writer.Indentation = 3;");
                    fg.AppendLine($"Write_{ModuleNickname}(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"writer: writer,");
                        fg.AppendLine($"name: null,");
                        fg.AppendLine($"item: item,");
                        fg.AppendLine($"doMasks: false,");
                        fg.AppendLine($"errorMask: out {obj.ErrorMask} errorMask);");
                    }
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public static void Write_{ModuleNickname}{obj.GenericTypes}",
                obj.GenerateWhereClauses().ToArray()))
            {
                args.Add($"{obj.Getter_InterfaceStr} item");
                args.Add($"Stream stream");
                args.Add($"out {obj.ErrorMask} errorMask");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("using (var writer = new XmlTextWriter(stream, Encoding.ASCII))");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("writer.Formatting = Formatting.Indented;");
                    fg.AppendLine("writer.Indentation = 3;");
                    fg.AppendLine($"Write_{ModuleNickname}(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"writer: writer,");
                        fg.AppendLine($"name: null,");
                        fg.AppendLine($"item: item,");
                        fg.AppendLine($"doMasks: true,");
                        fg.AppendLine($"errorMask: out errorMask);");
                    }
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public static void Write_{ModuleNickname}{obj.GenericTypes}",
                obj.GenerateWhereClauses().ToArray()))
            {
                args.Add($"{obj.Getter_InterfaceStr} item");
                args.Add($"string path");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("using (var writer = new XmlTextWriter(path, Encoding.ASCII))");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("writer.Formatting = Formatting.Indented;");
                    fg.AppendLine("writer.Indentation = 3;");
                    fg.AppendLine($"Write_{ModuleNickname}(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"writer: writer,");
                        fg.AppendLine($"name: null,");
                        fg.AppendLine($"item: item,");
                        fg.AppendLine($"doMasks: false,");
                        fg.AppendLine($"errorMask: out {obj.ErrorMask} errorMask);");
                    }
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public static void Write_{ModuleNickname}{obj.GenericTypes}",
                obj.GenerateWhereClauses().ToArray()))
            {
                args.Add($"{obj.Getter_InterfaceStr} item");
                args.Add($"string path");
                args.Add($"out {obj.ErrorMask} errorMask");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("using (var writer = new XmlTextWriter(path, Encoding.ASCII))");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("writer.Formatting = Formatting.Indented;");
                    fg.AppendLine("writer.Indentation = 3;");
                    fg.AppendLine($"Write_{ModuleNickname}(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"writer: writer,");
                        fg.AppendLine($"name: null,");
                        fg.AppendLine($"item: item,");
                        fg.AppendLine($"doMasks: true,");
                        fg.AppendLine($"errorMask: out errorMask);");
                    }
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public static void Write_{ModuleNickname}{obj.GenericTypes}",
                obj.GenerateWhereClauses().ToArray()))
            {
                args.Add($"XmlWriter writer");
                args.Add($"string name");
                args.Add($"{obj.Getter_InterfaceStr} item");
                args.Add($"bool doMasks");
                args.Add($"out {obj.ErrorMask} errorMask");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{obj.ErrorMask} errMaskRet = null;");
                using (var args = new ArgsWrapper(fg,
                    $"Write_{ModuleNickname}_Internal"))
                {
                    args.Add("writer: writer");
                    args.Add("name: name");
                    args.Add("item: item");
                    args.Add("doMasks: doMasks");
                    args.Add($"errorMask: doMasks ? () => errMaskRet ?? (errMaskRet = new {obj.ErrorMask}()) : default(Func<{obj.ErrorMask}>)");
                }
                fg.AppendLine($"errorMask = errMaskRet;");
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"private static void Write_{ModuleNickname}_Internal{obj.GenericTypes}",
                obj.GenerateWhereClauses().ToArray()))
            {
                args.Add($"XmlWriter writer");
                args.Add($"string name");
                args.Add($"{obj.Getter_InterfaceStr} item");
                args.Add($"bool doMasks");
                args.Add($"Func<{obj.ErrorMask}> errorMask");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("try");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"using (new ElementWrapper(writer, name ?? \"{obj.FullName}\"))");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("if (name != null)");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"writer.WriteAttributeString(\"{XmlConstants.TYPE_ATTRIBUTE}\", \"{obj.FullName}\");");
                        }

                        foreach (var field in obj.IterateFields())
                        {
                            if (field.Field.Derivative) continue;

                            if (!this.TryGetTypeGeneration(field.Field.GetType(), out var generator))
                            {
                                throw new ArgumentException("Unsupported type generator: " + field.Field);
                            }

                            if (field.Field.Notifying != NotifyingOption.None)
                            {
                                fg.AppendLine($"if (item.{field.Field.HasBeenSetAccessor})");
                            }
                            using (new BraceWrapper(fg))
                            {
                                var maskType = this.Gen.MaskModule.GetMaskModule(field.Field.GetType()).GetErrorMaskTypeStr(field.Field);
                                fg.AppendLine($"{maskType} subMask;");
                                generator.GenerateWrite(
                                    fg: fg,
                                    typeGen: field.Field,
                                    writerAccessor: "writer",
                                    itemAccessor: $"item.{field.Field.Name}",
                                    doMaskAccessor: "doMasks",
                                    maskAccessor: $"subMask",
                                    nameAccessor: $"nameof(item.{field.Field.Name})");
                                fg.AppendLine("if (doMasks && subMask != null)");
                                using (new BraceWrapper(fg))
                                {
                                    fg.AppendLine($"errorMask().{field.Field.Name} = subMask;");
                                }
                            }
                        }
                    }
                }
                fg.AppendLine("catch (Exception ex)");
                fg.AppendLine("when (doMasks)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("errorMask().Overall = ex;");
                }
            }
        }

        public override void Generate(ObjectGeneration obj)
        {
            GenerateXSD(obj);
        }

        public void GenerateXSD(ObjectGeneration obj)
        {
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
            foreach (var field in obj.Fields)
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
            using (var writer = new XmlTextWriter (outputPath, Encoding.ASCII))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 3;
                XDocument doc = new XDocument(root);
                doc.WriteTo(writer);
            }
        }
    }
}
