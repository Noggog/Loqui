using System;

namespace Loqui.Generation
{
    public class ContainerMaskFieldGeneration : MaskModuleField
    {
        public static string GetItemString(ContainerType listType, string valueStr)
        {
            LoquiType loquiType = listType.SubTypeGeneration as LoquiType;
            if (loquiType == null)
            {
                return valueStr;
            }
            else
            {
                return $"MaskItem<{valueStr}, {loquiType.RefGen.Obj.GetMaskString(valueStr)}>";
            }
        }

        public static string GetListString(ContainerType listType, string valueStr)
        {
            return $"IEnumerable<{GetItemString(listType, valueStr)}>";
        }

        public static string GetMaskString(ContainerType listType, string valueStr)
        {
            return $"MaskItem<{valueStr}, {GetListString(listType, valueStr)}>";
        }

        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string valueStr)
        {
            fg.AppendLine($"public {GetMaskString(field as ContainerType, valueStr)} {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = new {GetMaskString(field as ContainerType, "Exception")}(ex, null);");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = ({GetMaskString(field as ContainerType, "Exception")})obj;");
        }

        public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
        {
            ListType listType = field as ListType;
            if (listType.SubTypeGeneration is LoquiType loqui)
            {
                fg.AppendLine($"public MaskItem<{nameof(CopyOption)}, {loqui.RefGen.Obj.CopyMask}> {field.Name};");
            }
            else
            {
                fg.AppendLine($"public {nameof(CopyOption)} {field.Name};");
            }
        }

        public static string GetSubMaskString(TypeGeneration field, string maskStr)
        {
            ListType listType = field as ListType;
            if (listType.SubTypeGeneration is LoquiType loqui)
            {
                return $"MaskItem<{maskStr}, {loqui.RefGen.Obj.GetMaskString(maskStr)}>";
            }
            else
            {
                return maskStr;
            }
        }

        public override void GenerateForErrorMaskToString(FileGeneration fg, TypeGeneration field, string accessor, bool topLevel)
        {
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"{field.Name} =>\");");
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"[\");");
            fg.AppendLine($"using (new DepthWrapper(fg))");
            using (new BraceWrapper(fg))
            {
                ContainerType listType = field as ContainerType;
                if (topLevel)
                {
                    fg.AppendLine($"if ({accessor}.Overall != null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}({accessor}.Overall.ToString());");
                    }
                    fg.AppendLine($"if ({accessor}.Specific != null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"foreach (var subItem in {accessor}.Specific)");
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"[\");");
                            var fieldGen = this.Module.GetMaskModule(listType.SubTypeGeneration.GetType());
                            fg.AppendLine($"using (new DepthWrapper(fg))");
                            using (new BraceWrapper(fg))
                            {
                                fieldGen.GenerateForErrorMaskToString(fg, listType.SubTypeGeneration, "subItem", false);
                            }
                            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
                        }
                    }
                }
                else
                {
                    fg.AppendLine($"foreach (var subItem in {accessor})");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"[\");");
                        var fieldGen = this.Module.GetMaskModule(listType.SubTypeGeneration.GetType());
                        fg.AppendLine($"using (new DepthWrapper(fg))");
                        using (new BraceWrapper(fg))
                        {
                            fieldGen.GenerateForErrorMaskToString(fg, listType.SubTypeGeneration, "subItem", false);
                        }
                        fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
                    }
                }
            }
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
        }

        public override void GenerateForAllEqual(FileGeneration fg, TypeGeneration field)
        {
            ListType listType = field as ListType;

            fg.AppendLine($"if ({field.Name} != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"if (!eval(this.{field.Name}.Overall)) return false;");
                fg.AppendLine($"if ({field.Name}.Specific != null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"foreach (var item in {field.Name}.Specific)");
                    using (new BraceWrapper(fg))
                    {
                        if (listType.SubTypeGeneration is LoquiType loqui)
                        {
                            fg.AppendLine($"if (!eval(item.Overall)) return false;");
                            fg.AppendLine($"if (!item.Specific?.AllEqual(eval) ?? false) return false;");
                        }
                        else
                        {
                            fg.AppendLine($"if (!eval(item)) return false;");
                        }
                    }
                }
            }
        }

        public override void GenerateForTranslate(FileGeneration fg, TypeGeneration field, string retAccessor, string rhsAccessor)
        {
            ListType listType = field as ListType;

            fg.AppendLine($"if ({field.Name} != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{retAccessor} = new {ContainerMaskFieldGeneration.GetMaskString(listType, "R")}();");
                fg.AppendLine($"{retAccessor}.Overall = eval({rhsAccessor}.Overall);");
                fg.AppendLine($"if ({field.Name}.Specific != null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"List<{GetSubMaskString(listType, "R")}> l = new List<{GetSubMaskString(listType, "R")}>();");
                    fg.AppendLine($"{retAccessor}.Specific = l;");
                    fg.AppendLine($"foreach (var item in {field.Name}.Specific)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{GetSubMaskString(listType, "R")} mask = default({GetSubMaskString(listType, "R")});");
                        var fieldGen = this.Module.GetMaskModule(listType.SubTypeGeneration.GetType());
                        fieldGen.GenerateForTranslate(fg, listType.SubTypeGeneration, "mask", "item");
                        fg.AppendLine($"l.Add(mask);");
                    }
                }
            }
        }

        public override void GenerateForErrorMaskCombine(FileGeneration fg, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor)
        {
            ContainerType cont = field as ContainerType;
            fg.AppendLine($"{retAccessor} = new {GetMaskString(cont, "Exception")}({accessor}.Overall.Combine({rhsAccessor}.Overall), new List<{GetItemString(cont, "Exception")}>({accessor}.Specific.And({rhsAccessor}.Specific)));");
        }

        public override string GenerateBoolMaskCheck(TypeGeneration field, string maskAccessor)
        {
            return $"{maskAccessor}?.{field.Name}?.Overall ?? true";
        }
    }
}
