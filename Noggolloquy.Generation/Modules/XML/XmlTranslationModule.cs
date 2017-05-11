using Noggog;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class XmlTranslationModule : GenerationModule
    {
        public Dictionary<Type, XmlTranslationGeneration> TypeGenerations = new Dictionary<Type, XmlTranslationGeneration>();

        public override string RegionString => "XML Translation";

        public XmlTranslationModule()
        {
            this.TypeGenerations[typeof(NoggType)] = new NoggXmlTranslationGeneration();
            this.TypeGenerations[typeof(BoolNullType)] = new PrimitiveXmlTranslationGeneration<bool>();
            this.TypeGenerations[typeof(BoolType)] = new PrimitiveXmlTranslationGeneration<bool>();
            this.TypeGenerations[typeof(CharNullType)] = new PrimitiveXmlTranslationGeneration<char>();
            this.TypeGenerations[typeof(CharType)] = new PrimitiveXmlTranslationGeneration<char>();
            this.TypeGenerations[typeof(DateTimeNullType)] = new PrimitiveXmlTranslationGeneration<DateTime>();
            this.TypeGenerations[typeof(DateTimeType)] = new PrimitiveXmlTranslationGeneration<DateTime>();
            this.TypeGenerations[typeof(DoubleNullType)] = new PrimitiveXmlTranslationGeneration<double>();
            this.TypeGenerations[typeof(DoubleType)] = new PrimitiveXmlTranslationGeneration<double>();
            this.TypeGenerations[typeof(EnumType)] = new EnumXmlTranslationGeneration();
            this.TypeGenerations[typeof(FloatNullType)] = new PrimitiveXmlTranslationGeneration<float>("Float");
            this.TypeGenerations[typeof(FloatType)] = new PrimitiveXmlTranslationGeneration<float>("Float");
            this.TypeGenerations[typeof(Int8NullType)] = new PrimitiveXmlTranslationGeneration<sbyte>("Int8");
            this.TypeGenerations[typeof(Int8Type)] = new PrimitiveXmlTranslationGeneration<sbyte>("Int8");
            this.TypeGenerations[typeof(Int16NullType)] = new PrimitiveXmlTranslationGeneration<short>();
            this.TypeGenerations[typeof(Int16Type)] = new PrimitiveXmlTranslationGeneration<short>();
            this.TypeGenerations[typeof(Int32NullType)] = new PrimitiveXmlTranslationGeneration<int>();
            this.TypeGenerations[typeof(Int32Type)] = new PrimitiveXmlTranslationGeneration<int>();
            this.TypeGenerations[typeof(Int64NullType)] = new PrimitiveXmlTranslationGeneration<long>();
            this.TypeGenerations[typeof(Int64Type)] = new PrimitiveXmlTranslationGeneration<long>();
            this.TypeGenerations[typeof(P2IntNullType)] = new PrimitiveXmlTranslationGeneration<P2Int>();
            this.TypeGenerations[typeof(P2IntType)] = new PrimitiveXmlTranslationGeneration<P2Int>();
            this.TypeGenerations[typeof(P3IntNullType)] = new PrimitiveXmlTranslationGeneration<P3Int>();
            this.TypeGenerations[typeof(P3IntType)] = new PrimitiveXmlTranslationGeneration<P3Int>();
            this.TypeGenerations[typeof(P3DoubleNullType)] = new PrimitiveXmlTranslationGeneration<P3Double>();
            this.TypeGenerations[typeof(P3DoubleType)] = new PrimitiveXmlTranslationGeneration<P3Double>();
            this.TypeGenerations[typeof(PercentNullType)] = new PrimitiveXmlTranslationGeneration<Percent>();
            this.TypeGenerations[typeof(PercentType)] = new PrimitiveXmlTranslationGeneration<Percent>();
            this.TypeGenerations[typeof(RangeDoubleNullType)] = new PrimitiveXmlTranslationGeneration<RangeDouble>();
            this.TypeGenerations[typeof(RangeDoubleType)] = new PrimitiveXmlTranslationGeneration<RangeDouble>();
            this.TypeGenerations[typeof(StringType)] = new PrimitiveXmlTranslationGeneration<string>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeInt8?>)] = new PrimitiveXmlTranslationGeneration<RangeInt8>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeInt8>)] = new PrimitiveXmlTranslationGeneration<RangeInt8>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeInt16?>)] = new PrimitiveXmlTranslationGeneration<RangeInt16>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeInt16>)] = new PrimitiveXmlTranslationGeneration<RangeInt16>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeInt32?>)] = new PrimitiveXmlTranslationGeneration<RangeInt32>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeInt32>)] = new PrimitiveXmlTranslationGeneration<RangeInt32>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeInt64?>)] = new PrimitiveXmlTranslationGeneration<RangeInt64>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeInt64>)] = new PrimitiveXmlTranslationGeneration<RangeInt64>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeUInt8?>)] = new PrimitiveXmlTranslationGeneration<RangeUInt8>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeUInt8>)] = new PrimitiveXmlTranslationGeneration<RangeUInt8>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeUInt16?>)] = new PrimitiveXmlTranslationGeneration<RangeUInt16>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeUInt16>)] = new PrimitiveXmlTranslationGeneration<RangeUInt16>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeUInt32?>)] = new PrimitiveXmlTranslationGeneration<RangeUInt32>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeUInt32>)] = new PrimitiveXmlTranslationGeneration<RangeUInt32>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeUInt64?>)] = new PrimitiveXmlTranslationGeneration<RangeUInt64>();
            this.TypeGenerations[typeof(TypicalRangedIntType<RangeUInt64>)] = new PrimitiveXmlTranslationGeneration<RangeUInt64>();
            this.TypeGenerations[typeof(UDoubleNullType)] = new PrimitiveXmlTranslationGeneration<UDouble>();
            this.TypeGenerations[typeof(UDoubleType)] = new PrimitiveXmlTranslationGeneration<UDouble>();
            this.TypeGenerations[typeof(UInt8NullType)] = new PrimitiveXmlTranslationGeneration<byte>();
            this.TypeGenerations[typeof(UInt8Type)] = new PrimitiveXmlTranslationGeneration<byte>();
            this.TypeGenerations[typeof(UInt16NullType)] = new PrimitiveXmlTranslationGeneration<ushort>();
            this.TypeGenerations[typeof(UInt16Type)] = new PrimitiveXmlTranslationGeneration<ushort>();
            this.TypeGenerations[typeof(UInt32NullType)] = new PrimitiveXmlTranslationGeneration<uint>();
            this.TypeGenerations[typeof(UInt32Type)] = new PrimitiveXmlTranslationGeneration<uint>();
            this.TypeGenerations[typeof(UInt64NullType)] = new PrimitiveXmlTranslationGeneration<ulong>();
            this.TypeGenerations[typeof(UInt64Type)] = new PrimitiveXmlTranslationGeneration<ulong>();
            this.TypeGenerations[typeof(UnsafeType)] = new UnsafeXmlTranslationGeneration();
            this.TypeGenerations[typeof(WildcardType)] = new UnsafeXmlTranslationGeneration();
            this.TypeGenerations[typeof(ListType)] = new ListXmlTranslationGeneration(this);
            this.TypeGenerations[typeof(DictType)] = new UnsafeXmlTranslationGeneration();
        }

        public override IEnumerable<string> RequiredUsingStatements()
        {
            yield return "System.Xml";
            yield return "System.Xml.Linq";
            yield return "System.IO";
            yield return "Noggog.Xml";
            yield return "Noggolloquy.Xml";
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
                fg.AppendLine("public void Write_XML(Stream stream)");
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{obj.ExtCommonName}.Write_XML"))
                    {
                        args.Add("this");
                        args.Add("stream");
                    }
                }
                fg.AppendLine();
            }

            fg.AppendLine($"public void Write_XML(Stream stream, out {obj.ErrorMask} errorMask)");
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"{obj.ExtCommonName}.Write_XML"))
                {
                    args.Add("this");
                    args.Add("stream");
                    args.Add("out errorMask");
                }
            }
            fg.AppendLine();

            fg.AppendLine($"public void Write_XML(XmlWriter writer, out {obj.ErrorMask} errorMask, string name = null)");
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"{obj.ExtCommonName}.Write_XML"))
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
                    fg.AppendLine("public abstract void Write_XML(XmlWriter writer, string name = null);");
                    fg.AppendLine();
                }
            }
            else if (obj.IsTopClass
                || (!obj.Abstract && (obj.BaseClass?.Abstract ?? true)))
            {
                fg.AppendLine($"public{obj.FunctionOverride}void Write_XML(XmlWriter writer, string name = null)");
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{obj.ExtCommonName}.Write_XML"))
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
            var param = new XmlReadGenerationParameters()
            {
                Obj = obj,
                Accessor = "this",
                FG = fg,
                Field = null,
                Name = "Root",
                XmlNodeName = "root",
                XmlGen = this,
                MaskAccessor = "mask"
            };

            if (!obj.Abstract)
            {
                fg.AppendLine($"public{obj.NewOverride}static {obj.ObjectName} Create_XML(Stream stream)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"using (var reader = new StreamReader(stream))");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("return Create_XML(XElement.Parse(reader.ReadToEnd()));");
                    }
                }
                fg.AppendLine();

                fg.AppendLine($"public{obj.NewOverride}static {obj.ObjectName} Create_XML(XElement root)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"var ret = new {obj.ObjectName}();");
                    fg.AppendLine($"NoggXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.CopyIn(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"root: root,");
                        fg.AppendLine($"item: ret,");
                        fg.AppendLine($"skipProtected: false,");
                        fg.AppendLine($"doMasks: false,");
                        fg.AppendLine($"mask: out {obj.ErrorMask} errorMask,");
                        fg.AppendLine($"cmds: null);");
                    }
                    fg.AppendLine("return ret;");
                }
                fg.AppendLine();

                fg.AppendLine($"public static {obj.ObjectName} Create_XML(XElement root, out {obj.ErrorMask} errorMask)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"var ret = new {obj.ObjectName}();");
                    fg.AppendLine($"NoggXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.CopyIn(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"root: root,");
                        fg.AppendLine($"item: ret,");
                        fg.AppendLine($"skipProtected: false,");
                        fg.AppendLine($"doMasks: true,");
                        fg.AppendLine($"mask: out errorMask,");
                        fg.AppendLine($"cmds: null);");
                    }
                    fg.AppendLine("return ret;");
                }
                fg.AppendLine();
            }

            if (obj is StructGeneration) return;
            fg.AppendLine("public" + obj.FunctionOverride + "void CopyIn_XML(XElement root, NotifyingFireParameters? cmds = null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"NoggXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.CopyIn(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"root: root,");
                    fg.AppendLine($"item: this,");
                    fg.AppendLine($"skipProtected: true,");
                    fg.AppendLine($"doMasks: false,");
                    fg.AppendLine($"mask: out {obj.ErrorMask} errorMask,");
                    fg.AppendLine($"cmds: cmds);");
                }
            }
            fg.AppendLine();

            fg.AppendLine($"public virtual void CopyIn_XML(XElement root, out {obj.ErrorMask} errorMask, NotifyingFireParameters? cmds = null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"NoggXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.CopyIn(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"root: root,");
                    fg.AppendLine($"item: this,");
                    fg.AppendLine($"skipProtected: true,");
                    fg.AppendLine($"doMasks: true,");
                    fg.AppendLine($"mask: out errorMask,");
                    fg.AppendLine($"cmds: cmds);");
                }
            }
            fg.AppendLine();

            foreach (var baseClass in obj.BaseClassTrail())
            {
                fg.AppendLine($"public override void CopyIn_XML(XElement root, out {baseClass.ErrorMask} errorMask, NotifyingFireParameters? cmds = null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"CopyIn_XML(root, out {obj.ErrorMask} errMask, cmds: cmds);");
                    fg.AppendLine("errorMask = errMask;");
                }
                fg.AppendLine();
            }
        }

        public override void Modify(ObjectGeneration obj)
        {
        }

        public override void Modify(NoggolloquyGenerator gen)
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
            if (obj.IsTopClass)
            {
                using (var args = new FunctionWrapper(fg,
                    $"public static void Write_XML{obj.GenericTypes}",
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
                        fg.AppendLine($"Write_XML(");
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
            }

            using (var args = new FunctionWrapper(fg,
                $"public static void Write_XML{obj.GenericTypes}",
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
                    fg.AppendLine($"Write_XML(");
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
                $"public static void Write_XML{obj.GenericTypes}",
                obj.GenerateWhereClauses().ToArray()))
            {
                args.Add($"{obj.Getter_InterfaceStr} item");
                args.Add($"XmlWriter writer");
                args.Add($"out {obj.ErrorMask} errorMask");
                args.Add($"string name = null");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"Write_XML(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"writer: writer,");
                    fg.AppendLine($"name: name,");
                    fg.AppendLine($"item: item,");
                    fg.AppendLine($"doMasks: true,");
                    fg.AppendLine($"errorMask: out errorMask);");
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public static void Write_XML{obj.GenericTypes}",
                obj.GenerateWhereClauses().ToArray()))
            {
                args.Add($"{obj.Getter_InterfaceStr} item");
                args.Add($"XmlWriter writer");
                args.Add($"string name");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"Write_XML(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"writer: writer,");
                    fg.AppendLine($"name: name,");
                    fg.AppendLine($"item: item,");
                    fg.AppendLine($"doMasks: false,");
                    fg.AppendLine($"errorMask: out {obj.ErrorMask} errorMask);");
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public static void Write_XML{obj.GenericTypes}",
                obj.GenerateWhereClauses().ToArray()))
            {
                args.Add($"{obj.Getter_InterfaceStr} item");
                args.Add($"XmlWriter writer");
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"Write_XML(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"writer: writer,");
                    fg.AppendLine($"name: null,");
                    fg.AppendLine($"item: item,");
                    fg.AppendLine($"doMasks: false,");
                    fg.AppendLine($"errorMask: out {obj.ErrorMask} errorMask);");
                }
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public static void Write_XML{obj.GenericTypes}",
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
                    $"Write_XML_Internal"))
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
                $"private static void Write_XML_Internal{obj.GenericTypes}",
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
                    fg.AppendLine($"using (new ElementWrapper(writer, nameof({obj.ObjectName})))");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"if (!string.IsNullOrEmpty(name))");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"writer.WriteAttributeString(\"name\", name);");
                        }

                        foreach (var field in obj.IterateFields())
                        {
                            if (field.Field.Derivative) continue;

                            if (!this.TypeGenerations.TryGetValue(field.Field.GetType(), out var generator))
                            {
                                throw new ArgumentException("Unsupported type generator: " + field.Field);
                            }

                            fg.AppendLine("try");
                            using (new BraceWrapper(fg))
                            {
                                switch (field.Field.Notifying)
                                {
                                    case NotifyingOption.None:
                                        generator.GenerateWrite(fg, field.Field, "writer", "item", "errorMask", $"nameof(item.{field.Field.Name})");
                                        break;
                                    case NotifyingOption.HasBeenSet:
                                    case NotifyingOption.Notifying:
                                        fg.AppendLine($"if (item.{field.Field.Property}.HasBeenSet)");
                                        using (new BraceWrapper(fg))
                                        {
                                            generator.GenerateWrite(fg, field.Field, "writer", "item", "errorMask", $"nameof(item.{field.Field.Name})");
                                        }
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                            }
                            fg.AppendLine("catch (Exception ex)");
                            using (new BraceWrapper(fg))
                            {
                                fg.AppendLine("if (!doMasks) throw;");
                                fg.AppendLine($"errorMask().SetNthException((ushort){field.Field.IndexEnumName}, ex);");
                            }
                        }
                    }
                }
                fg.AppendLine("catch (Exception ex)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("if (!doMasks) throw;");
                    fg.AppendLine("errorMask().Overall = ex;");
                }
            }
        }
    }
}
