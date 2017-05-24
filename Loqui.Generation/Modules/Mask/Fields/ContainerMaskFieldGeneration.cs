using System;

namespace Loqui.Generation
{
    public class ContainerMaskFieldGeneration : MaskModuleField
    {
        public static string GetListString(ContainerType listType, string valueStr)
        {
            LoquiType loquiType = listType.SubTypeGeneration as LoquiType;
            string listStr;
            if (loquiType == null)
            {
                listStr = $"IEnumerable<{valueStr}>";
            }
            else
            {
                listStr = $"IEnumerable<MaskItem<{valueStr}, {loquiType.RefGen.Obj.GetMaskString(valueStr)}>>";
            }
            return listStr;
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

        public override void GenerateForErrorMaskToString(FileGeneration fg, TypeGeneration field, string accessor, bool topLevel)
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

        public override void GenerateForAllEqual(FileGeneration fg, TypeGeneration field)
        {
            ListType listType = field as ListType;

            fg.AppendLine($"if ({field.Name} != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"if (!object.Equals(this.{field.Name}.Overall, t)) return false;");
                fg.AppendLine($"if ({field.Name}.Specific != null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"foreach (var item in {field.Name}.Specific)");
                    using (new BraceWrapper(fg))
                    {
                        if (listType.SubTypeGeneration is LoquiType loqui)
                        {
                            fg.AppendLine($"if (!object.Equals(item.Overall, t)) return false;");
                            fg.AppendLine($"if (!item.Specific?.AllEqual(t) ?? false) return false;");
                        }
                        else
                        {
                            fg.AppendLine($"if (!object.Equals(item, t)) return false;");
                        }
                    }
                }
            }
        }
    }
}
