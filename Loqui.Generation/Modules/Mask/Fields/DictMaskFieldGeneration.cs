using System;

namespace Loqui.Generation
{
    public class DictMaskFieldGeneration : MaskModuleField
    {
        public static string GetMaskString(IDictType dictType, string typeStr)
        {
            return $"MaskItem<{typeStr}, IEnumerable<{GetSubMaskString(dictType, typeStr)}>>";
        }

        public static string GetSubMaskString(IDictType dictType, string typeStr)
        {
            LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
            LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;
            string keyStr = $"{(keyLoquiType == null ? typeStr : $"MaskItem<{typeStr}, {keyLoquiType.GetMaskString(typeStr)}>")}";
            string valueStr = $"{(valueLoquiType == null ? typeStr : $"MaskItem<{typeStr}, {valueLoquiType.GetMaskString(typeStr)}>")}";

            string itemStr;
            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    itemStr = $"KeyValuePair<{keyStr}, {valueStr}>";
                    break;
                case DictMode.KeyedValue:
                    itemStr = valueStr;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return itemStr;
        }

        public static string GetErrorMaskString(IDictType dictType)
        {
            return $"MaskItem<Exception, IEnumerable<{GetSubErrorMaskString(dictType)}>>";
        }

        public static string GetSubErrorMaskString(IDictType dictType)
        {
            LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
            LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;
            string keyStr = $"{(keyLoquiType == null ? "Exception" : $"MaskItem<Exception, {keyLoquiType.Mask(MaskType.Error)}>")}";
            string valueStr = $"{(valueLoquiType == null ? "Exception" : $"MaskItem<Exception, {valueLoquiType.Mask(MaskType.Error)}>")}";

            string itemStr;
            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    itemStr = $"KeyValuePair<{keyStr}, {valueStr}>";
                    break;
                case DictMode.KeyedValue:
                    itemStr = valueStr;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return itemStr;
        }

        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            fg.AppendLine($"public {GetMaskString(field as IDictType, typeStr)} {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = new {GetErrorMaskString(field as IDictType)}(ex, null);");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = ({GetErrorMaskString(field as IDictType)})obj;");
        }

        public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
        {
            DictType dictType = field as DictType;
            LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
            LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;

            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    if (keyLoquiType == null && valueLoquiType == null)
                    {
                        fg.AppendLine($"public bool {field.Name};");
                    }
                    else if (keyLoquiType != null && valueLoquiType != null)
                    {
                        fg.AppendLine($"public MaskItem<bool, KeyValuePair<({nameof(RefCopyType)} Type, {keyLoquiType.TargetObjectGeneration.Mask(MaskType.Copy)} Mask), ({nameof(RefCopyType)} Type, {valueLoquiType.TargetObjectGeneration.Mask(MaskType.Copy)} Mask)>> {field.Name};");
                    }
                    else
                    {
                        LoquiType loqui = keyLoquiType ?? valueLoquiType;
                        fg.AppendLine($"public MaskItem<bool, ({nameof(RefCopyType)} Type, {loqui.TargetObjectGeneration.Mask(MaskType.Copy)} Mask)> {field.Name};");
                    }
                    break;
                case DictMode.KeyedValue:
                    fg.AppendLine($"public MaskItem<{nameof(CopyOption)}, {valueLoquiType.Mask(MaskType.Copy)}> {field.Name};");
                    break;
                default:
                    break;
            }
        }

        public override void GenerateForTranslationMask(FileGeneration fg, TypeGeneration field)
        {
            DictType dictType = field as DictType;
            LoquiType keyLoquiType = dictType.KeyTypeGen as LoquiType;
            LoquiType valueLoquiType = dictType.ValueTypeGen as LoquiType;

            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    if (keyLoquiType == null && valueLoquiType == null)
                    {
                        fg.AppendLine($"public bool {field.Name};");
                    }
                    else if (keyLoquiType != null && valueLoquiType != null)
                    {
                        fg.AppendLine($"public MaskItem<bool, KeyValuePair<{keyLoquiType.TargetObjectGeneration.Mask(MaskType.Translation)}, {valueLoquiType.TargetObjectGeneration.Mask(MaskType.Translation)}>> {field.Name};");
                    }
                    else
                    {
                        LoquiType loqui = keyLoquiType ?? valueLoquiType;
                        fg.AppendLine($"public MaskItem<bool, {loqui.TargetObjectGeneration.Mask(MaskType.Translation)}> {field.Name};");
                    }
                    break;
                case DictMode.KeyedValue:
                    fg.AppendLine($"public MaskItem<bool, {valueLoquiType.Mask(MaskType.Translation)}> {field.Name};");
                    break;
                default:
                    break;
            }
        }

        public override void GenerateForErrorMaskToString(FileGeneration fg, TypeGeneration field, string accessor, bool topLevel)
        {
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"{field.Name} =>\");");
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"[\");");
            fg.AppendLine($"using (new DepthWrapper(fg))");
            using (new BraceWrapper(fg))
            {
                DictType dictType = field as DictType;

                if (topLevel)
                {
                    fg.AppendLine($"if ({accessor}.Overall != null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}({accessor}.Overall.ToString());");
                    }
                }
                fg.AppendLine($"if ({accessor}.Specific != null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"foreach (var subItem in {accessor}{(topLevel ? ".Specific" : string.Empty)})");
                    using (new BraceWrapper(fg))
                    {
                        var keyFieldGen = this.Module.GetMaskModule(dictType.KeyTypeGen.GetType());
                        var valFieldGen = this.Module.GetMaskModule(dictType.ValueTypeGen.GetType());
                        fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"[\");");
                        fg.AppendLine($"using (new DepthWrapper(fg))");
                        using (new BraceWrapper(fg))
                        {
                            switch (dictType.Mode)
                            {
                                case DictMode.KeyValue:
                                    fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"Key => [\");");
                                    fg.AppendLine($"using (new DepthWrapper(fg))");
                                    using (new BraceWrapper(fg))
                                    {
                                        keyFieldGen.GenerateForErrorMaskToString(fg, dictType.KeyTypeGen, "subItem.Key", false);
                                    }
                                    fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
                                    fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"Value => [\");");
                                    fg.AppendLine($"using (new DepthWrapper(fg))");
                                    using (new BraceWrapper(fg))
                                    {
                                        valFieldGen.GenerateForErrorMaskToString(fg, dictType.ValueTypeGen, "subItem.Value", false);
                                    }
                                    fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
                                    break;
                                case DictMode.KeyedValue:
                                    keyFieldGen.GenerateForErrorMaskToString(fg, dictType.KeyTypeGen, "subItem", false);
                                    break;
                                default:
                                    break;
                            }
                        }
                        fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
                    }
                }
            }
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}(\"]\");");
        }

        public override void GenerateForAllEqual(FileGeneration fg, TypeGeneration field, Accessor accessor, bool nullCheck)
        {
            DictType dictType = field as DictType;

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
                        switch (dictType.Mode)
                        {
                            case DictMode.KeyValue:
                                if (dictType.KeyTypeGen is LoquiType loquiKey)
                                {
                                    fg.AppendLine($"if (item.Key != null)");
                                    using (new BraceWrapper(fg))
                                    {
                                        fg.AppendLine($"if (!eval(item.Key.Overall)) return false;");
                                        fg.AppendLine($"if (!item.Key.Specific?.AllEqual(eval) ?? false) return false;");
                                    }
                                }
                                else
                                {
                                    fg.AppendLine($"if (!eval(item.Key)) return false;");
                                }
                                if (dictType.ValueTypeGen is LoquiType loquiVal)
                                {
                                    fg.AppendLine($"if (item.Value != null)");
                                    using (new BraceWrapper(fg))
                                    {
                                        fg.AppendLine($"if (!eval(item.Value.Overall)) return false;");
                                        fg.AppendLine($"if (!item.Value.Specific?.AllEqual(eval) ?? false) return false;");
                                    }
                                }
                                else
                                {
                                    fg.AppendLine($"if (!eval(item.Value)) return false;");
                                }
                                break;
                            case DictMode.KeyedValue:
                                fg.AppendLine($"if (!eval(item.Overall)) return false;");
                                fg.AppendLine($"if (!item.Specific?.AllEqual(eval) ?? false) return false;");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public override void GenerateForTranslate(FileGeneration fg, TypeGeneration field, string retAccessor, string rhsAccessor)
        {
            DictType dictType = field as DictType;

            fg.AppendLine($"if ({field.Name} != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{retAccessor} = new {DictMaskFieldGeneration.GetMaskString(dictType, "R")}();");
                fg.AppendLine($"{retAccessor}.Overall = eval({rhsAccessor}.Overall);");
                fg.AppendLine($"if ({field.Name}.Specific != null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"List<{GetSubMaskString(dictType, "R")}> l = new List<{GetSubMaskString(dictType, "R")}>();");
                    fg.AppendLine($"{retAccessor}.Specific = l;");
                    fg.AppendLine($"foreach (var item in {field.Name}.Specific)");
                    using (new BraceWrapper(fg))
                    {
                        switch (dictType.Mode)
                        {
                            case DictMode.KeyValue:
                                if (dictType.KeyTypeGen is LoquiType loquiKey)
                                {
                                    fg.AppendLine($"MaskItem<R, {loquiKey.GenerateMaskString("R")}> keyVal = default(MaskItem<R, {loquiKey.GenerateMaskString("R")}>);");
                                    this.Module.GetMaskModule(loquiKey.GetType()).GenerateForTranslate(fg, loquiKey, "keyVal", "item.Key");
                                }
                                else
                                {
                                    fg.AppendLine($"R keyVal = eval(item.Key);");
                                }
                                if (dictType.ValueTypeGen is LoquiType loquiVal)
                                {
                                    fg.AppendLine($"MaskItem<R, {loquiVal.GenerateMaskString("R")}> valVal = default(MaskItem<R, {loquiVal.GenerateMaskString("R")}>);");
                                    this.Module.GetMaskModule(loquiVal.GetType()).GenerateForTranslate(fg, loquiVal, "valVal", "item.Value");
                                }
                                else
                                {
                                    fg.AppendLine($"R valVal = eval(item.Value);");
                                }
                                fg.AppendLine($"l.Add(new {GetSubMaskString(dictType, "R")}(keyVal, valVal));");
                                break;
                            case DictMode.KeyedValue:
                                var loquiType = dictType.ValueTypeGen as LoquiType;
                                fg.AppendLine($"MaskItem<R, {loquiType.GenerateMaskString("R")}> mask = default(MaskItem<R, {loquiType.GenerateMaskString("R")}>);");
                                var fieldGen = this.Module.GetMaskModule(loquiType.GetType());
                                fieldGen.GenerateForTranslate(fg, loquiType, "mask", "item");
                                fg.AppendLine($"l.Add(mask);");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public override void GenerateForErrorMaskCombine(FileGeneration fg, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor)
        {
            DictType dictType = field as DictType;
            switch (dictType.Mode)
            {
                case DictMode.KeyValue:
                    string keyStr = "Exception", valStr = "Exception";
                    if (dictType.KeyTypeGen is LoquiType keyLoqui)
                    {
                        keyStr = $"MaskItem<Exception, {keyLoqui.GenerateMaskString("Exception")}>";
                    }
                    if (dictType.ValueTypeGen is LoquiType valLoqui)
                    {
                        valStr = $"MaskItem<Exception, {valLoqui.GenerateMaskString("Exception")}>";
                    }
                    var keyValStr = $"KeyValuePair<{keyStr}, {valStr}>";
                    fg.AppendLine($"{retAccessor} = new MaskItem<Exception, IEnumerable<{keyValStr}>>({accessor}.Overall.Combine({rhsAccessor}.Overall), new List<{keyValStr}>({accessor}.Specific.And({rhsAccessor}.Specific)));");
                    break;
                case DictMode.KeyedValue:
                    var loqui = dictType.ValueTypeGen as LoquiType;
                    fg.AppendLine($"{retAccessor} = new MaskItem<Exception, IEnumerable<MaskItem<Exception, {loqui.Mask(MaskType.Error)}>>>({accessor}.Overall.Combine({rhsAccessor}.Overall), new List<MaskItem<Exception, {loqui.Mask(MaskType.Error)}>>({accessor}.Specific.And({rhsAccessor}.Specific)));");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override string GenerateBoolMaskCheck(TypeGeneration field, string maskAccessor)
        {
            return $"{maskAccessor}?.{field.Name}?.Overall ?? true";
        }

        public override void GenerateForCtor(FileGeneration fg, TypeGeneration field, string valueStr)
        {
            fg.AppendLine($"this.{field.Name} = new {GetMaskString(field as IDictType, "T")}({valueStr}, null);");
        }

        public override string GetErrorMaskTypeStr(TypeGeneration field)
        {
            return DictMaskFieldGeneration.GetErrorMaskString(field as IDictType);
        }

        public override void GenerateForClearEnumerable(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name}.Specific = null;");
        }
    }
}
