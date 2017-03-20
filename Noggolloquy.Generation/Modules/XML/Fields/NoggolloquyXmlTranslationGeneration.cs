using System;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class NoggolloquyXmlTranslationGeneration : XmlFieldTranslationGeneration
    {
        protected virtual void WriteAttributes(XmlWriteGenerationParameters param)
        {
            if (!string.IsNullOrEmpty(param.Name))
            {
                param.FG.AppendLine("if (!string.IsNullOrEmpty(" + param.Name + "))");
                using (new BraceWrapper(param.FG))
                {
                    param.FG.AppendLine("writer.WriteAttributeString(\"name\", " + param.Name + ");");
                }
            }
        }

        public override void GenerateWrite(XmlWriteGenerationParameters param)
        {
            ObjectGeneration obj = param.Field as ObjectGeneration;
            param.FG.AppendLine("using (new ElementWrapper(writer, " + obj.ObjectName + ".NoggolloquyName))");
            using (new BraceWrapper(param.FG))
            {
                WriteAttributes(param);

                param.FG.AppendLine("WriteXMLFields_Internal(writer, errorMask);");
            }
        }

        public void GenerateFieldWrites(XmlWriteGenerationParameters param)
        {
            ObjectGeneration obj = param.Field as ObjectGeneration;

            if (obj.HasBaseObject)
            {
                param.FG.AppendLine("base.WriteXMLFields_Internal(writer, errorMask);");
            }

            foreach (var f in obj.Fields)
            {
                if (f.Derivative) continue;

                XmlFieldTranslationGeneration fieldGen;
                if (!param.XmlGen.TryGetFieldGen(f.GetType(), out fieldGen))
                {
                    throw new ArgumentException("Unknown type for XML generation");
                }

                param.FG.AppendLine("try");
                using (new BraceWrapper(param.FG))
                {
                    if (f.Notifying)
                    {
                        param.FG.AppendLine("if (" + param.Accessor + "." + f.HasBeenSetAccessor + ")");
                    }
                    using (new BraceWrapper(param.FG, f.Notifying))
                    {
                        fieldGen.GenerateWrite(
                            new XmlWriteGenerationParameters()
                            {
                                XmlGen = param.XmlGen,
                                Object = param.Object,
                                FG = param.FG,
                                Field = f,
                                Accessor = param.Accessor + "." + f.Name,
                                Name = f.Name
                            });
                    }
                }
                obj.GenerateExceptionCatcher(param.FG, f, "XML Write To", param.ErrorMaskAccessor);
                param.FG.AppendLine();
            }
        }

        public void GenerateReadFunction(XmlReadGenerationParameters param, ClassGeneration obj)
        {
            param.FG.AppendLine("HashSet<ushort> readIndices = new HashSet<ushort>();");
            param.FG.AppendLine("foreach (var elem in " + param.XmlNodeName + ".Elements())");
            using (new BraceWrapper(param.FG))
            {
                param.FG.AppendLine("XAttribute name;");
                param.FG.AppendLine("if (!elem.TryGetAttribute(\"name\", out name))");
                using (new BraceWrapper(param.FG))
                {
                    param.FG.AppendLine("mask.Warnings.Add(\"Skipping field that did not have name\");");
                    param.FG.AppendLine("continue;");
                }
                param.FG.AppendLine();

                param.FG.AppendLine("CopyInFromXMLElement_Internal(elem, mask, name.Value, readIndices, cmds);");
            }
            param.FG.AppendLine();

            param.FG.AppendLine("for (ushort i = 0; i < this.FieldCount; i++)");
            using (new BraceWrapper(param.FG))
            {
                param.FG.AppendLine("if (" + obj.ExtCommonName(obj.GenericTypes) + ".IsNthDerivative(i)) continue;");
                param.FG.AppendLine("if (!readIndices.Contains(i))");
                using (new BraceWrapper(param.FG))
                {
                    param.FG.AppendLine("this.SetNthObjectHasBeenSet_Internal(i, false, this);");
                }
            }
        }

        public void GenerateReadFunction(XmlReadGenerationParameters param, StructGeneration obj)
        {
            foreach (var field in obj.Fields)
            {
                param.FG.AppendLine($"{field.TypeName} imported_{field.Name} = default({field.TypeName});");
            }

            param.FG.AppendLine("foreach (var elem in " + param.XmlNodeName + ".Elements())");
            using (new BraceWrapper(param.FG))
            {
                param.FG.AppendLine("XAttribute name;");
                param.FG.AppendLine("if (!elem.TryGetAttribute(\"name\", out name))");
                using (new BraceWrapper(param.FG))
                {
                    param.FG.AppendLine("mask.Warnings.Add(\"Skipping field that did not have name\");");
                    param.FG.AppendLine("continue;");
                }
                param.FG.AppendLine();

                GenerateRead(param, obj);
            }
            param.FG.AppendLine();

            param.FG.AppendLine($"return new {obj.ObjectName}(");
            List<string> lines = new List<string>();
            foreach (var field in obj.Fields)
            {
                lines.Add($"{field.Name}: imported_{field.Name}");
            }

            for (int i = 0; i < lines.Count; i++)
            {
                using (new DepthWrapper(param.FG))
                {
                    using (new LineWrapper(param.FG))
                    {
                        param.FG.Append(lines[i]);
                        if (i != lines.Count - 1)
                        {
                            param.FG.Append(",");
                        }
                        else
                        {
                            param.FG.Append(");");
                        }
                    }
                }
            }
        }

        public override void GenerateRead(XmlReadGenerationParameters param)
        {
            ObjectGeneration obj = param.Obj as ObjectGeneration;

            HashSet<string> commonVariables = new HashSet<string>();
            foreach (var f in obj.Fields)
            {
                XmlFieldTranslationGeneration fieldGen;
                if (!param.XmlGen.TryGetFieldGen(f.GetType(), out fieldGen))
                {
                    throw new NotImplementedException();
                }

                if (f.Imports)
                {
                    var subParam = param.Copy();
                    subParam.Field = f;
                    commonVariables.Add(fieldGen.GenerateCommonReadVariables(subParam, f));
                }
            }
            foreach (var commonVar in commonVariables)
            {
                param.FG.AppendLine(commonVar);
            }

            ClassGeneration classObj = param.Obj as ClassGeneration;
            if (classObj != null)
            {
                GenerateRead(param, classObj);
            }
            else
            {
                GenerateReadFunction(param, param.Obj as StructGeneration);
            }
        }

        private void GenerateRead(XmlReadGenerationParameters param, ClassGeneration obj)
        {
            param.FG.AppendLine("switch (name)");
            using (new BraceWrapper(param.FG))
            {
                for (int i = 0; i < obj.Fields.Count; i++)
                {
                    TypeGeneration f = obj.Fields[i];
                    if (!param.XmlGen.TryGetFieldGen(f.GetType(), out XmlFieldTranslationGeneration fieldGen))
                    {
                        throw new NotImplementedException();
                    }

                    param.FG.AppendLine("case \"" + f.Name + "\":");
                    using (new DepthWrapper(param.FG))
                    {
                        if (!f.Imports)
                        { 
                            param.FG.AppendLine("mask.Warnings.Add(\"Skipping field " + f.Name + " that was not listed for import.\");");
                            param.FG.AppendLine("break;");
                            continue;
                        }

                        param.FG.AppendLine("try");
                        using (new BraceWrapper(param.FG))
                        {
                            fieldGen.GenerateRead(
                                new XmlReadGenerationParameters()
                                {
                                    XmlGen = param.XmlGen,
                                    FG = param.FG,
                                    Obj = param.Obj,
                                    Field = f,
                                    GenerateErrorMask = (err) => f.SetMaskException(param.FG, $"mask.{f.Name}", err),
                                    Accessor = f.ProtectedName,
                                    MaskAccessor = $"mask",
                                    Name = f.Name,
                                    XmlNodeName = param.XmlNodeName
                                });
                            param.FG.AppendLine("readIndices.Add(" + i + ");");
                        }
                        obj.GenerateExceptionCatcher(param.FG, f, "XML Copy In", param.MaskAccessor);
                        param.FG.AppendLine("break;");
                    }
                }

                GenerateDefaultSwitch(param, obj);
            }
        }

        private void GenerateDefaultSwitch(XmlReadGenerationParameters param, ObjectGeneration obj)
        {
            param.FG.AppendLine("default:");
            using (new DepthWrapper(param.FG))
            {
                if (obj.HasBaseObject)
                {
                    param.FG.AppendLine("base.CopyInFromXMLElement_Internal(" + param.XmlNodeName + ", mask, name, readIndices);");
                }
                else
                {
                    param.FG.AppendLine("//Deleted field");
                    param.FG.AppendLine("mask.Warnings.Add(\"Skipping field that did not exist anymore with name: \" + name);");
                }
                param.FG.AppendLine("break;");
            }
        }

        private void GenerateRead(XmlReadGenerationParameters param, StructGeneration obj)
        {
            param.FG.AppendLine("switch (name.Value)");
            using (new BraceWrapper(param.FG))
            {
                for (int i = 0; i < obj.Fields.Count; i++)
                {
                    TypeGeneration f = obj.Fields[i];
                    XmlFieldTranslationGeneration fieldGen;
                    if (!param.XmlGen.TryGetFieldGen(f.GetType(), out fieldGen))
                    {
                        throw new NotImplementedException();
                    }

                    param.FG.AppendLine("case \"" + f.Name + "\":");
                    using (new DepthWrapper(param.FG))
                    {
                        if (!f.Imports)
                        {
                            param.FG.AppendLine("mask.Warnings.Add(\"Skipping field " + f.Name + " that was not listed for import.\");");
                            param.FG.AppendLine("break;");
                            continue;
                        }

                        param.FG.AppendLine("try");
                        using (new BraceWrapper(param.FG))
                        {
                            param.FG.AppendLine("if (!" + param.XmlNodeName + ".Name.LocalName.Equals(\"" + fieldGen.GetElementName(f) + "\"))");
                            using (new BraceWrapper(param.FG))
                            {
                                param.FG.AppendLine("mask.Warnings.Add(\"Skipping field " + f.Name + " that did not match proper type. Type: \" + " + param.XmlNodeName + ".Name.LocalName + \", expected: " + fieldGen.GetElementName(f) + ".\");");
                                param.FG.AppendLine("break;");
                            }

                            fieldGen.GenerateRead(
                                new XmlReadGenerationParameters()
                                {
                                    XmlGen = param.XmlGen,
                                    FG = param.FG,
                                    Obj = param.Obj,
                                    Field = f,
                                    Accessor = "imported_" + f.Name,
                                    GenerateErrorMask = (err) => param.FG.AppendLine($"mask.{f.Name} = {err};"),
                                    MaskAccessor = $"mask.{f.Name}",
                                    Name = f.Name,
                                    XmlNodeName = param.XmlNodeName
                                });
                        }
                        obj.GenerateExceptionCatcher(param.FG, f, "XML Copy In", param.MaskAccessor);
                        param.FG.AppendLine("break;");
                    }
                }

                GenerateDefaultSwitch(param, obj);
            }
        }

        public override string GetElementName(object field)
        {
            ObjectGeneration objGen = field as ObjectGeneration;
            return objGen.Name;
        }
    }
}
