using System;

namespace Loqui.Generation
{
    public class ContainerMaskFieldGeneration : MaskModuleField
    {
        public static string GetItemString(ContainerType listType, string valueStr)
        {
            var maskType = listType.ObjectGen.ProtoGen.Gen.MaskModule.GetMaskModule(listType.SubTypeGeneration.GetType());
            return maskType.GetMaskString(listType.SubTypeGeneration, valueStr, indexed: true);
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
            fg.AppendLine($"public {GetMaskString(field as ContainerType, valueStr)}? {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = new {GetErrorMaskTypeStr(field)}(ex, null);");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = ({GetErrorMaskTypeStr(field)})obj;");
        }

        public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
        {
            ListType listType = field as ListType;
            if (listType.SubTypeGeneration is LoquiType loqui
                && loqui.SupportsMask(MaskType.Copy))
            {
                fg.AppendLine($"public MaskItem<{nameof(CopyOption)}, {loqui.Mask(MaskType.Copy)}?> {field.Name};");
            }
            else
            {
                fg.AppendLine($"public {nameof(CopyOption)} {field.Name};");
            }
        }

        public override void GenerateForCopyMaskCtor(FileGeneration fg, TypeGeneration field, string basicValueStr, string deepCopyStr)
        {
            ListType listType = field as ListType;
            if (listType.SubTypeGeneration is LoquiType loqui
                && loqui.SupportsMask(MaskType.Copy))
            {
                fg.AppendLine($"this.{field.Name} = new MaskItem<{nameof(CopyOption)}, {loqui.Mask(MaskType.Copy)}?>({deepCopyStr}, default);");
            }
            else
            {
                fg.AppendLine($"this.{field.Name} = {deepCopyStr};");
            }
        }

        public override void GenerateForTranslationMask(FileGeneration fg, TypeGeneration field)
        {
            ListType listType = field as ListType;
            if (listType.SubTypeGeneration is LoquiType loqui
                && loqui.SupportsMask(MaskType.Translation))
            {
                fg.AppendLine($"public MaskItem<bool, {loqui.Mask(MaskType.Translation)}?> {field.Name};");
            }
            else
            {
                fg.AppendLine($"public bool {field.Name};");
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
                    fg.AppendLine($"if ({accessor} != null)");
                    using (new BraceWrapper(fg))
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

        public override void GenerateForAllEqual(FileGeneration fg, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed)
        {
            ListType listType = field as ListType;

            if (nullCheck)
            {
                fg.AppendLine($"if ({accessor.DirectAccess} != null)");
            }
            using (new BraceWrapper(fg, doIt: nullCheck))
            {
                fg.AppendLine($"if (!eval({accessor.DirectAccess}.Overall)) return false;");
                fg.AppendLine($"if ({accessor.DirectAccess}.Specific != null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"foreach (var item in {accessor.DirectAccess}.Specific)");
                    using (new BraceWrapper(fg))
                    {
                        var subMask = this.Module.GetMaskModule(listType.SubTypeGeneration.GetType());
                        subMask.GenerateForAllEqual(fg, listType.SubTypeGeneration, new Accessor("item"), nullCheck: false, indexed: true);
                    }
                }
            }
        }

        public override void GenerateForTranslate(FileGeneration fg, TypeGeneration field, string retAccessor, string rhsAccessor, bool indexed)
        {
            ListType listType = field as ListType;

            fg.AppendLine($"if ({field.Name} != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{retAccessor} = new {ContainerMaskFieldGeneration.GetMaskString(listType, "R")}(eval({rhsAccessor}.Overall), Enumerable.Empty<{GetItemString(listType, "R")}>());");
                fg.AppendLine($"if ({field.Name}.Specific != null)");
                using (new BraceWrapper(fg))
                {
                    if (listType.SubTypeGeneration is LoquiType loqui)
                    {
                        var submaskString = $"MaskItemIndexed<R, {loqui.GetMaskString("R")}?>";
                        fg.AppendLine($"var l = new List<{submaskString}>();");
                        fg.AppendLine($"{retAccessor}.Specific = l;");
                        fg.AppendLine($"foreach (var item in {field.Name}.Specific.WithIndex())");
                        using (new BraceWrapper(fg))
                        {
                            var fieldGen = this.Module.GetMaskModule(listType.SubTypeGeneration.GetType());
                            fieldGen.GenerateForTranslate(fg, listType.SubTypeGeneration,
                                retAccessor: $"{submaskString}? mask",
                                rhsAccessor: $"item.Item{(indexed ? ".Value" : null)}",
                                indexed: true);
                            fg.AppendLine("if (mask == null) continue;");
                            fg.AppendLine($"l.Add(mask);");
                        }
                    }
                    else
                    {
                        fg.AppendLine($"var l = new List<(int Index, R Item)>();");
                        fg.AppendLine($"{retAccessor}.Specific = l;");
                        fg.AppendLine($"foreach (var item in {field.Name}.Specific.WithIndex())");
                        using (new BraceWrapper(fg))
                        {
                            var fieldGen = this.Module.GetMaskModule(listType.SubTypeGeneration.GetType());
                            fieldGen.GenerateForTranslate(fg, listType.SubTypeGeneration,
                                retAccessor: $"R mask",
                                rhsAccessor: $"item.Item{(indexed ? ".Value" : null)}",
                                indexed: true);
                            fg.AppendLine($"l.Add((item.Index, mask));");
                        }
                    }
                }
            }
        }
            
        public override void GenerateForErrorMaskCombine(FileGeneration fg, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor)
        {
            ContainerType cont = field as ContainerType;
            LoquiType loquiType = cont.SubTypeGeneration as LoquiType;
            string itemStr;
            if (loquiType == null)
            {
                itemStr = GetItemString(cont, "Exception");
            }
            else
            {
                itemStr = $"MaskItem<Exception?, {loquiType.Mask(MaskType.Error)}?>";
            }
            fg.AppendLine($"{retAccessor} = new MaskItem<Exception?, IEnumerable<{itemStr}>?>(ExceptionExt.Combine({accessor}?.Overall, {rhsAccessor}?.Overall), ExceptionExt.Combine({accessor}?.Specific, {rhsAccessor}?.Specific));");
        }

        public override string GenerateBoolMaskCheck(TypeGeneration field, string boolMaskAccessor)
        {
            return $"{boolMaskAccessor}?.{field.Name}?.Overall ?? true";
        }

        public override void GenerateForCtor(FileGeneration fg, TypeGeneration field, string typeStr, string valueStr)
        {
            fg.AppendLine($"this.{field.Name} = new {GetMaskString(field as ContainerType, typeStr)}({valueStr}, Enumerable.Empty<{GetItemString(field as ContainerType, typeStr)}>());");
        }

        public override string GetErrorMaskTypeStr(TypeGeneration field)
        {
            var contType = field as ContainerType;
            LoquiType loquiType = contType.SubTypeGeneration as LoquiType;
            string itemStr;
            if (loquiType == null)
            {
                itemStr = GetItemString(contType, "Exception");
            }
            else
            {
                itemStr = $"MaskItem<Exception?, {loquiType.Mask(MaskType.Error)}?>";
            }
            return $"MaskItem<Exception?, IEnumerable<{itemStr}>?>";
        }

        public override string GetTranslationMaskTypeStr(TypeGeneration field)
        {
            var contType = field as ContainerType;
            LoquiType loquiType = contType.SubTypeGeneration as LoquiType;
            string itemStr;
            if (loquiType == null)
            {
                itemStr = GetItemString(contType, "bool");
            }
            else
            {
                itemStr = $"MaskItem<bool, {loquiType.Mask(MaskType.Translation)}>";
            }
            return $"MaskItem<bool, IEnumerable<{itemStr}>>";
        }

        public override void GenerateForClearEnumerable(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name}.Specific = null;");
        }

        public override string GenerateForTranslationMaskCrystalization(TypeGeneration field)
        {
            var contType = field as ContainerType;
            if (contType.SubTypeGeneration is LoquiType loquiType
                && loquiType.SupportsMask(MaskType.Translation))
            {
                return $"({field.Name}?.Overall ?? true, {field.Name}?.Specific?.GetCrystal())";
            }
            else
            {
                return $"({field.Name}, null)";
            }
        }

        public override void GenerateForTranslationMaskSet(FileGeneration fg, TypeGeneration field, Accessor accessor, string onAccessor)
        {
            ListType listType = field as ListType;
            if (listType.SubTypeGeneration is LoquiType loqui
                && loqui.SupportsMask(MaskType.Translation))
            {
                fg.AppendLine($"{accessor.DirectAccess} = new MaskItem<bool, {loqui.Mask(MaskType.Translation)}?>({onAccessor}, null);");
            }
            else
            {
                fg.AppendLine($"{accessor.DirectAccess} = {onAccessor};");
            }
        }
    }
}
