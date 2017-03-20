using Noggog;
using System;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class XmlTranslationGeneration : GenerationModule
    {
        public NoggolloquyXmlTranslationGeneration LevTranslation = new NoggolloquyXmlTranslationGeneration();
        public Dictionary<Type, XmlFieldTranslationGeneration> FieldGenerators = new Dictionary<Type, XmlFieldTranslationGeneration>();

        public override string RegionString { get { return "XML Translation"; } }

        public override IEnumerable<string> RequiredUsingStatements()
        {
            yield return "System.Xml";
            yield return "System.Xml.Linq";
            yield return "System.IO";
            yield return "Noggog.Xml";
        }

        public override IEnumerable<string> Interfaces(ObjectGeneration obj)
        {
            yield break;
        }

        public XmlTranslationGeneration()
        {
            FieldGenerators[typeof(LevType)] = new LevFieldXmlGeneration();
            FieldGenerators[typeof(BoolNullType)] = new TypedStructXmlGeneration<bool?>();
            FieldGenerators[typeof(BoolType)] = new TypedStructXmlGeneration<bool>();
            FieldGenerators[typeof(CharNullType)] = new TypedStructXmlGeneration<char?>();
            FieldGenerators[typeof(CharType)] = new TypedStructXmlGeneration<char>();
            FieldGenerators[typeof(Int8NullType)] = new TypedStructXmlGeneration<sbyte?>();
            FieldGenerators[typeof(Int8Type)] = new TypedStructXmlGeneration<sbyte>();
            FieldGenerators[typeof(Int16NullType)] = new TypedStructXmlGeneration<short?>();
            FieldGenerators[typeof(Int16Type)] = new TypedStructXmlGeneration<short>();
            FieldGenerators[typeof(Int32NullType)] = new TypedStructXmlGeneration<int?>();
            FieldGenerators[typeof(Int32Type)] = new TypedStructXmlGeneration<int>();
            FieldGenerators[typeof(Int64NullType)] = new TypedStructXmlGeneration<long?>();
            FieldGenerators[typeof(Int64Type)] = new TypedStructXmlGeneration<long>();
            FieldGenerators[typeof(UInt8NullType)] = new TypedStructXmlGeneration<byte?>();
            FieldGenerators[typeof(UInt8Type)] = new TypedStructXmlGeneration<byte>();
            FieldGenerators[typeof(UInt16NullType)] = new TypedStructXmlGeneration<ushort?>();
            FieldGenerators[typeof(UInt16Type)] = new TypedStructXmlGeneration<ushort>();
            FieldGenerators[typeof(UInt32NullType)] = new TypedStructXmlGeneration<uint?>();
            FieldGenerators[typeof(UInt32Type)] = new TypedStructXmlGeneration<uint>();
            FieldGenerators[typeof(UInt64NullType)] = new TypedStructXmlGeneration<ulong?>();
            FieldGenerators[typeof(UInt64Type)] = new TypedStructXmlGeneration<ulong>();
            FieldGenerators[typeof(StringType)] = new TypedStructXmlGeneration<string>();
            FieldGenerators[typeof(PercentType)] = new TypedStructXmlGeneration<Percent>();
            FieldGenerators[typeof(FloatType)] = new TypedStructXmlGeneration<float>();
            FieldGenerators[typeof(FloatNullType)] = new TypedStructXmlGeneration<float?>();
            FieldGenerators[typeof(UDoubleType)] = new TypedStructXmlGeneration<UDouble>();
            FieldGenerators[typeof(UDoubleNullType)] = new TypedStructXmlGeneration<UDouble?>();
            FieldGenerators[typeof(DoubleType)] = new TypedStructXmlGeneration<Double>();
            FieldGenerators[typeof(DoubleNullType)] = new TypedStructXmlGeneration<Double?>();
            FieldGenerators[typeof(RangeIntType)] = new TypedStructXmlGeneration<RangeInt>();
            FieldGenerators[typeof(RangeIntNullType)] = new TypedStructXmlGeneration<RangeInt?>();
            FieldGenerators[typeof(RangeDoubleType)] = new TypedStructXmlGeneration<RangeDouble>();
            FieldGenerators[typeof(RangeDoubleNullType)] = new TypedStructXmlGeneration<RangeDouble?>();
            FieldGenerators[typeof(EnumType)] = new EnumFieldXmlGeneration();
            FieldGenerators[typeof(ListType)] = new ListFieldXmlGeneration();
            FieldGenerators[typeof(DictType)] = new DictFieldXmlGeneration();
            FieldGenerators[typeof(Array2DType)] = new Container2DFieldXmlGeneration("Array2D");
            FieldGenerators[typeof(P2IntType)] = new StructTypeXmlGeneration("P2Int");
            FieldGenerators[typeof(P2IntNullType)] = new StructTypeXmlGeneration("P2Int?");
            FieldGenerators[typeof(P3IntType)] = new StructTypeXmlGeneration("P3Int");
            FieldGenerators[typeof(P3IntNullType)] = new StructTypeXmlGeneration("P3Int?");
            FieldGenerators[typeof(P3DoubleType)] = new P3DoubleFieldXmlGeneration();
            FieldGenerators[typeof(P3DoubleNullType)] = new P3DoubleFieldXmlGeneration();
            FieldGenerators[typeof(NonExportedObjectType)] = new NonExportedObjectXmlGeneration();
            FieldGenerators[typeof(WildcardType)] = new StructTypeXmlGeneration("Wildcard")
            {
                IsNullable = true
            };
        }

        public void AddTypeAssociation<T>(XmlFieldTranslationGeneration fieldGen)
        {
            FieldGenerators[typeof(T)] = fieldGen;
        }

        public bool TryGetFieldGen(Type fieldType, out XmlFieldTranslationGeneration gen)
        {
            return FieldGenerators.TryGetValue(fieldType, out gen);
        }

        public override void GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            GenerateRead(obj, fg);
            GenerateWrite(obj, fg);
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
                fg.AppendLine("public static " + obj.ObjectName + " CreateFromXML(XElement root)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("return CreateFromXML_Internal(root, mask: null);");
                }
                fg.AppendLine();

                fg.AppendLine($"public static {obj.ObjectName} CreateFromXML(XElement root, out {obj.GetErrorMaskItemString()} errorMask)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"var mask = new {obj.GetErrorMaskItemString()}();");
                    fg.AppendLine($"errorMask = mask;");
                    fg.AppendLine("return CreateFromXML_Internal(root, mask);");
                }
                fg.AppendLine();

                fg.AppendLine($"private static {obj.ObjectName} CreateFromXML_Internal(XElement root, {obj.GetErrorMaskItemString()} mask)");
                using (new BraceWrapper(fg))
                {
                    if (obj is ClassGeneration)
                    {
                        fg.AppendLine("var ret = new " + obj.ObjectName + "();");
                        fg.AppendLine("ret.CopyInFromXML_Internal(root, mask: mask, cmds: null);");
                        fg.AppendLine("return ret;");
                    }
                    else if (obj is StructGeneration)
                    {
                        LevTranslation.GenerateRead(param);
                    }
                }
                fg.AppendLine();
            }

            if (obj is StructGeneration) return;
            fg.AppendLine("public" + obj.FunctionOverride + "void CopyInFromXML(XElement root, NotifyingFireParameters? cmds = null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("CopyInFromXML_Internal(root, mask: null, cmds: cmds);");
            }
            fg.AppendLine();

            fg.AppendLine("public virtual void CopyInFromXML(XElement root, out " + obj.GetErrorMaskItemString() + " errorMask, NotifyingFireParameters? cmds = null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("var ret = new " + obj.GetErrorMaskItemString() + "();");
                fg.AppendLine("errorMask = ret;");
                fg.AppendLine("CopyInFromXML_Internal(root, ret, cmds: cmds);");
            }
            fg.AppendLine();

            foreach (var baseClass in obj.BaseClassTrail())
            {
                fg.AppendLine("public override void CopyInFromXML(XElement root, out " + baseClass.GetErrorMaskItemString() + " errorMask, NotifyingFireParameters? cmds = null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"var ret = new {obj.GetErrorMaskItemString()}()");
                    using (new BraceWrapper(fg) { AppendSemicolon = true })
                    {
                        fg.AppendLine($"Specific = new {obj.GetMaskString("Exception")}()");
                    }
                    fg.AppendLine("errorMask = ret;");
                    fg.AppendLine("CopyInFromXML_Internal(root, ret, cmds: cmds);");
                }
                fg.AppendLine();
            }

            fg.AppendLine("private void CopyInFromXML_Internal(XElement root, " + obj.GetErrorMaskItemString() + " mask, NotifyingFireParameters? cmds = null)");
            using (new BraceWrapper(fg))
            {
                LevTranslation.GenerateReadFunction(param, obj as ClassGeneration);
            }
            fg.AppendLine();

            fg.AppendLine("protected void CopyInFromXMLElement_Internal(XElement root, " + obj.GetErrorMaskItemString() + " mask, string name, HashSet<ushort> readIndices, NotifyingFireParameters? cmds = null)");
            using (new BraceWrapper(fg))
            {
                LevTranslation.GenerateRead(param);
            }
            fg.AppendLine();
        }

        private void GenerateWrite(ObjectGeneration obj, FileGeneration fg)
        {
            if (obj.IsTopClass)
            {
                fg.AppendLine("public void WriteXMLToStream(Stream stream)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("using (var writer = new XmlTextWriter(stream, Encoding.ASCII))");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("writer.Formatting = Formatting.Indented;");
                        fg.AppendLine("writer.Indentation = 3;");
                        fg.AppendLine("WriteXML(writer);");
                    }
                }
                fg.AppendLine();
            }

            if (obj.Abstract)
            {
                if (!obj.BaseClass?.Abstract ?? true)
                {
                    fg.AppendLine("public abstract void WriteXML(XmlWriter writer, string name);");
                    fg.AppendLine();

                    fg.AppendLine("public abstract void WriteXML(XmlWriter writer);");
                    fg.AppendLine();
                }
            }
            else
            {
                fg.AppendLine("public void WriteXML(XmlWriter writer, " + obj.GetErrorMaskItemString() + " errorMask, string name = null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("var ret = new " + obj.GetErrorMaskItemString() + "();");
                    fg.AppendLine("errorMask = ret;");
                    fg.AppendLine("WriteXML_Internal(writer, ret, name);");
                }
                fg.AppendLine();

                fg.AppendLine($"public{obj.FunctionOverride}void WriteXML(XmlWriter writer, string name)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("WriteXML_Internal(writer, null, name);");
                }
                fg.AppendLine();

                fg.AppendLine($"public{obj.FunctionOverride}void WriteXML(XmlWriter writer)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("WriteXML_Internal(writer, null, null);");
                }
                fg.AppendLine();
            }

            fg.AppendLine("private void WriteXML_Internal(XmlWriter writer, " + obj.GetErrorMaskItemString() + " errorMask, string name)");
            using (new BraceWrapper(fg))
            {
                LevTranslation.GenerateWrite(
                    new XmlWriteGenerationParameters()
                    {
                        XmlGen = this,
                        Object = obj,
                        FG = fg,
                        Field = obj,
                        Accessor = "this",
                        Name = "name",
                        ErrorMaskAccessor = "errorMask"
                    });
            }
            fg.AppendLine();

            string privacy = (obj is StructGeneration) ? "private" : "protected";
            fg.AppendLine($"{privacy} void WriteXMLFields_Internal(XmlWriter writer, " + obj.GetErrorMaskItemString() + " errorMask)");
            using (new BraceWrapper(fg))
            {
                LevTranslation.GenerateFieldWrites(
                    new XmlWriteGenerationParameters()
                    {
                        XmlGen = this,
                        Object = obj,
                        FG = fg,
                        Field = obj,
                        Accessor = "this",
                        Name = "name",
                        ErrorMaskAccessor = "errorMask"
                    });
            }
            fg.AppendLine();
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

        public override IEnumerable<string> GetWriterInterfaces()
        {
            yield return "IXmlWriter";
        }

        public override IEnumerable<string> GetReaderInterfaces()
        {
            yield return "IXmlTranslator";
        }
    }
}
