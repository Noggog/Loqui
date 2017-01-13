using System;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class DictFieldXmlGeneration : XmlFieldTranslationGeneration
    {
        public virtual string ElementName { get { return "Dict"; } }

        public override IEnumerable<string> GenerateCommonReadVariables(XmlReadGenerationParameters param, object field)
        {
            IDictType dictType = field as IDictType;

            foreach (var val in base.GenerateCommonReadVariables(param, field))
            {
                yield return val;
            }

            XmlFieldTranslationGeneration fieldGen;
            if (param.XmlGen.FieldGenerators.TryGetValue(
                dictType.KeyTypeGen.GetType(),
                out fieldGen))
            {
                var keyFieldParam = param.Copy();
                keyFieldParam.Field = dictType.KeyTypeGen;
                foreach (var val in fieldGen.GenerateCommonReadVariables(keyFieldParam, dictType.KeyTypeGen))
                {
                    yield return val;
                }
            }

            if (param.XmlGen.FieldGenerators.TryGetValue(
                dictType.ValueTypeGen.GetType(),
                out fieldGen))
            {
                var valFieldParam = param.Copy();
                valFieldParam.Field = dictType.ValueTypeGen;
                foreach (var val in fieldGen.GenerateCommonReadVariables(valFieldParam, dictType.ValueTypeGen))
                {
                    yield return val;
                }
            }
        }

        public override void GenerateWrite(XmlWriteGenerationParameters param)
        {
            DictType dictType = param.Field as DictType;

            param.FG.AppendLine("using (new ElementWrapper(writer, \"" + ElementName + "\"))");
            using (new BraceWrapper(param.FG))
            {
                if (!string.IsNullOrEmpty(param.Name))
                {
                    param.FG.AppendLine("writer.WriteAttributeString(\"name\", \"" + param.Name + "\");");
                }

                switch (dictType.Mode)
                {
                    case DictMode.KeyValue:
                        param.FG.AppendLine($"foreach (var dictObj in {param.Accessor})");
                        break;
                    case DictMode.KeyedValue:
                        param.FG.AppendLine($"foreach (var dictObj in ((IEnumerable<{dictType.ValueTypeGen.TypeName}>){param.Accessor}))");
                        break;
                    default:
                        throw new NotImplementedException();
                }

                using (new BraceWrapper(param.FG))
                {
                    switch (dictType.Mode)
                    {
                        case DictMode.KeyValue:
                            GenerateKeyValueWrite(dictType, param);
                            break;
                        case DictMode.KeyedValue:
                            GenerateKeyedValueWrite(dictType, param);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        private void GenerateKeyValueWrite(DictType dictType, XmlWriteGenerationParameters param)
        {
            param.FG.AppendLine("using (new ElementWrapper(writer, \"Item\"))");
            using (new BraceWrapper(param.FG))
            {
                XmlFieldTranslationGeneration keyGen;
                if (!param.XmlGen.TryGetFieldGen(dictType.KeyTypeGen.GetType(), out keyGen))
                {
                    throw new ArgumentException();
                }

                param.FG.AppendLine("using (new ElementWrapper(writer, \"Key\"))");
                using (new BraceWrapper(param.FG))
                {
                    keyGen.GenerateWrite(
                        new XmlWriteGenerationParameters()
                        {
                            XmlGen = param.XmlGen,
                            Object = param.Object,
                            FG = param.FG,
                            Field = dictType.KeyTypeGen,
                            Accessor = "dictObj.Key",
                            Name = null
                        });
                }

                XmlFieldTranslationGeneration valGen;
                if (!param.XmlGen.TryGetFieldGen(dictType.ValueTypeGen.GetType(), out valGen))
                {
                    throw new ArgumentException();
                }

                param.FG.AppendLine("using (new ElementWrapper(writer, \"Value\"))");
                using (new BraceWrapper(param.FG))
                {
                    valGen.GenerateWrite(
                        new XmlWriteGenerationParameters()
                        {
                            XmlGen = param.XmlGen,
                            Object = param.Object,
                            FG = param.FG,
                            Field = dictType.ValueTypeGen,
                            Accessor = "dictObj.Value",
                            Name = null
                        });
                }
            }
        }

        private void GenerateKeyedValueWrite(DictType dictType, XmlWriteGenerationParameters param)
        {
            XmlFieldTranslationGeneration valGen;
            if (!param.XmlGen.TryGetFieldGen(dictType.ValueTypeGen.GetType(), out valGen))
            {
                throw new ArgumentException();
            }

            valGen.GenerateWrite(
                new XmlWriteGenerationParameters()
                {
                    XmlGen = param.XmlGen,
                    Object = param.Object,
                    FG = param.FG,
                    Field = dictType.ValueTypeGen,
                    Accessor = "dictObj",
                    Name = null
                });
        }

        public override void GenerateRead(XmlReadGenerationParameters param)
        {
            DictType dictType = param.Field as DictType;

            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    GenerateKeyValueRead(dictType, param);
                    break;
                case DictMode.KeyedValue:
                    GenerateKeyedValueRead(dictType, param);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void GenerateKeyValueRead(DictType dictType, XmlReadGenerationParameters param)
        {
            param.FG.AppendLine($"foreach (var dictElem in {param.XmlNodeName}.Elements(\"Item\"))");
            using (new BraceWrapper(param.FG))
            {
                param.FG.AppendLine("var keyElem = dictElem.Element(\"Key\");");
                param.FG.AppendLine("var valElem = dictElem.Element(\"Value\");");
                param.FG.AppendLine("if (keyElem == null || valElem == null)");
                using (new BraceWrapper(param.FG))
                {
                    param.FG.AppendLine("throw new ArgumentException(\"Key or value was not present.\");");
                }
                param.FG.AppendLine();

                XmlFieldTranslationGeneration keyGen;
                if (!param.XmlGen.TryGetFieldGen(dictType.KeyTypeGen.GetType(), out keyGen))
                {
                    throw new ArgumentException();
                }
                param.FG.AppendLine(dictType.KeyTypeGen.TypeName + " keyItem;");
                keyGen.PrepSubRead(
                    new XmlReadGenerationParameters()
                    {
                        XmlGen = param.XmlGen,
                        FG = param.FG,
                        Obj = param.Obj,
                        Field = dictType.KeyTypeGen,
                        Accessor = "keyItem",
                        XmlNodeName = "dictElem"
                    });
                keyGen.GenerateRead(
                    new XmlReadGenerationParameters()
                    {
                        XmlGen = param.XmlGen,
                        FG = param.FG,
                        Obj = param.Obj,
                        Field = dictType.KeyTypeGen,
                        GenerateErrorMask = (err) => dictType.AddMaskException(param.FG, param.MaskAccessor, err, true),
                        Accessor = "keyItem",
                        Name = dictType.Name + "Key",
                        XmlNodeName = "dictElem"
                    });

                XmlFieldTranslationGeneration valGen;
                if (!param.XmlGen.TryGetFieldGen(dictType.ValueTypeGen.GetType(), out valGen))
                {
                    throw new ArgumentException();
                }

                param.FG.AppendLine(dictType.ValueTypeGen.TypeName + " valItem;");
                valGen.PrepSubRead(
                    new XmlReadGenerationParameters()
                    {
                        XmlGen = param.XmlGen,
                        FG = param.FG,
                        Obj = param.Obj,
                        Field = dictType.ValueTypeGen,
                        Accessor = "valItem",
                        XmlNodeName = "dictElem"
                    });

                valGen.GenerateRead(
                    new XmlReadGenerationParameters()
                    {
                        XmlGen = param.XmlGen,
                        FG = param.FG,
                        Obj = param.Obj,
                        Field = dictType.ValueTypeGen,
                        GenerateErrorMask = (err) => dictType.AddMaskException(param.FG, param.MaskAccessor, err, false),
                        Accessor = "valItem",
                        Name = dictType.Name + "Val",
                        XmlNodeName = "dictElem"
                    });

                param.FG.AppendLine("this." + dictType.Name + ".Set(keyItem, valItem, cmds);");
            }
        }


        private void GenerateKeyedValueRead(DictType dictType, XmlReadGenerationParameters param)
        {
            param.FG.AppendLine($"foreach (var dictElem in {param.XmlNodeName}.Elements())");
            using (new BraceWrapper(param.FG))
            {
                XmlFieldTranslationGeneration valGen;
                if (!param.XmlGen.TryGetFieldGen(dictType.ValueTypeGen.GetType(), out valGen))
                {
                    throw new ArgumentException();
                }

                param.FG.AppendLine(dictType.ValueTypeGen.TypeName + " valItem;");
                valGen.PrepSubRead(
                    new XmlReadGenerationParameters()
                    {
                        XmlGen = param.XmlGen,
                        FG = param.FG,
                        Obj = param.Obj,
                        Field = dictType.ValueTypeGen,
                        Accessor = "valItem",
                        XmlNodeName = "dictElem"
                    });

                valGen.GenerateRead(
                    new XmlReadGenerationParameters()
                    {
                        XmlGen = param.XmlGen,
                        FG = param.FG,
                        Obj = param.Obj,
                        Field = dictType.ValueTypeGen,
                        GenerateErrorMask = (err) => dictType.AddMaskException(param.FG, param.MaskAccessor, err, false),
                        Accessor = "valItem",
                        Name = dictType.Name + "Val",
                        XmlNodeName = "dictElem"
                    });

                param.FG.AppendLine("this." + dictType.Name + ".Set(valItem, cmds);");
            }
        }

        public override string GetElementName(object field)
        {
            return this.ElementName;
        }
    }
}
