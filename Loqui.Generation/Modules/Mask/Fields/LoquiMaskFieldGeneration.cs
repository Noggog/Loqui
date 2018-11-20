using System;
using static Loqui.Generation.LoquiType;

namespace Loqui.Generation
{
    public class LoquiMaskFieldGeneration : MaskModuleField
    {
        public override string GetErrorMaskTypeStr(TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            return $"MaskItem<Exception, {loqui.Mask(MaskType.Error)}>";
        }

        public override string GetTranslationMaskTypeStr(TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            return $"MaskItem<bool, {loqui.Mask(MaskType.Translation)}>";
        }

        public static string GetObjectErrorMask(LoquiType loqui, string accessor)
        {
            return $"new MaskItem<Exception, {loqui.Mask(MaskType.Error)}>(null, {accessor})";
        }

        public override void GenerateForField(FileGeneration fg, TypeGeneration field, string typeStr)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"public MaskItem<{typeStr}, {loqui.GenerateMaskString(typeStr)}> {field.Name} {{ get; set; }}");
        }

        public override void GenerateForErrorMask(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"public MaskItem<Exception, {loqui.Mask(MaskType.Error)}> {field.Name};");
        }

        public override void GenerateSetException(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"this.{field.Name} = new MaskItem<Exception, {loqui.Mask(MaskType.Error)}>(ex, null);");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"this.{field.Name} = (MaskItem<Exception, {loqui.Mask(MaskType.Error)}>)obj;");
        }

        public override void GenerateForCopyMask(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            if (loqui.RefType == LoquiRefType.Direct)
            {
                if (loqui.SingletonType == SingletonLevel.Singleton)
                {
                    if (loqui.InterfaceType == LoquiInterfaceType.IGetter) return;
                    fg.AppendLine($"public MaskItem<bool, {loqui.Mask(MaskType.Copy)}> {field.Name};");
                }
                else
                {
                    fg.AppendLine($"public MaskItem<{nameof(CopyOption)}, {loqui.Mask(MaskType.Copy)}> {field.Name};");
                }
            }
            else
            {
                if (loqui.TargetObjectGeneration == null)
                {
                    fg.AppendLine($"public {nameof(GetterCopyOption)} {field.Name};");
                }
                else
                {
                    fg.AppendLine($"public {nameof(CopyOption)} {field.Name};");
                }
            }
        }

        public override void GenerateForTranslationMask(FileGeneration fg, TypeGeneration field)
        {
            LoquiType loqui = field as LoquiType;
            if (loqui.RefType == LoquiRefType.Direct)
            {
                fg.AppendLine($"public MaskItem<bool, {loqui.Mask(MaskType.Translation)}> {field.Name};");
            }
            else
            {
                fg.AppendLine($"public bool {field.Name};");
            }
        }

        public bool IsUnknownGeneric(LoquiType type)
        {
            return type.RefType != LoquiRefType.Direct
                && type.TargetObjectGeneration == null;
        }

        public override void GenerateForErrorMaskToString(FileGeneration fg, TypeGeneration field, string accessor, bool topLevel)
        {
            if (!field.IntegrateField) return;
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"{accessor}?.ToString(fg);");
        }

        public override void GenerateForAllEqual(FileGeneration fg, TypeGeneration field, Accessor accessor, bool nullCheck)
        {
            LoquiType loqui = field as LoquiType;

            if (nullCheck)
            {
                fg.AppendLine($"if ({field.Name} != null)");
            }
            using (new BraceWrapper(fg, doIt: nullCheck))
            {
                fg.AppendLine($"if (!eval({accessor.DirectAccess}.Overall)) return false;");
                if (!IsUnknownGeneric(loqui))
                {
                    fg.AppendLine($"if ({accessor.DirectAccess}.Specific != null && !{accessor.DirectAccess}.Specific.AllEqual(eval)) return false;");
                }
                else
                {
                    fg.AppendLine($"throw new {nameof(NotImplementedException)}();");
                }
            }
        }

        public override void GenerateForTranslate(FileGeneration fg, TypeGeneration field, string retAccessor, string rhsAccessor)
        {
            LoquiType loqui = field as LoquiType;

            fg.AppendLine($"if ({rhsAccessor} != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{retAccessor} = new MaskItem<R, {loqui.GenerateMaskString("R")}>();");
                fg.AppendLine($"{retAccessor}.Overall = eval({rhsAccessor}.Overall);");
                if (!IsUnknownGeneric(loqui))
                {
                    fg.AppendLine($"if ({rhsAccessor}.Specific != null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{retAccessor}.Specific = {rhsAccessor}.Specific.Translate(eval);");
                    }
                }
                else
                {
                    fg.AppendLine($"throw new {nameof(NotImplementedException)}();");
                }
            }
        }

        public override void GenerateForErrorMaskCombine(FileGeneration fg, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor)
        {
            LoquiType loqui = field as LoquiType;
            if (!IsUnknownGeneric(loqui))
            {
                fg.AppendLine($"{retAccessor} = new MaskItem<Exception, {loqui.Mask(MaskType.Error)}>({accessor}.Overall.Combine({rhsAccessor}.Overall), ((IErrorMask<{loqui.Mask(MaskType.Error)}>){accessor}.Specific).Combine({rhsAccessor}.Specific));");
            }
            else
            {
                fg.AppendLine($"{retAccessor} = new MaskItem<Exception, {loqui.Mask(MaskType.Error)}>({accessor}.Overall.Combine({rhsAccessor}.Overall), Loqui.Internal.LoquiHelper.Combine({accessor}.Specific, {rhsAccessor}.Specific));");
            }
        }

        public override string GenerateBoolMaskCheck(TypeGeneration field, string maskAccessor)
        {
            return $"{maskAccessor}?.{field.Name}?.Overall ?? true";
        }

        public override void GenerateForCtor(FileGeneration fg, TypeGeneration field, string typeStr, string valueStr)
        {
            LoquiType loqui = field as LoquiType;
            fg.AppendLine($"this.{field.Name} = new MaskItem<{typeStr}, {loqui.GenerateMaskString(typeStr)}>({valueStr}, {(loqui.TargetObjectGeneration == null ? "null" : $"new {loqui.TargetObjectGeneration.GetMaskString(typeStr)}({valueStr})")});");
        }

        public override void GenerateForClearEnumerable(FileGeneration fg, TypeGeneration field)
        {
        }

        public override string GetMaskString(TypeGeneration field, string valueStr)
        {
            var loqui = field as LoquiType;
            return $"MaskItem<{valueStr}, {(loqui.TargetObjectGeneration?.GetMaskString(valueStr) ?? $"IMask<{valueStr}>")}>";
        }

        public override string GenerateForTranslationMaskCrystalization(TypeGeneration field)
        {
            return $"({field.Name}?.Overall ?? true, {field.Name}?.Specific?.GetCrystal())";
        }

        public override void GenerateForCopyMaskCtor(FileGeneration fg, TypeGeneration field, string basicValueStr, string deepCopyStr)
        {
            LoquiType loqui = field as LoquiType;
            if (loqui.RefType == LoquiRefType.Direct)
            {
                if (loqui.SingletonType == SingletonLevel.Singleton)
                {
                    if (loqui.InterfaceType == LoquiInterfaceType.IGetter) return;
                    fg.AppendLine($"this.{field.Name} = new MaskItem<bool, {loqui.Mask(MaskType.Copy)}>({basicValueStr}, default);");
                }
                else
                {
                    fg.AppendLine($"this.{field.Name} = new MaskItem<{nameof(CopyOption)}, {loqui.Mask(MaskType.Copy)}>({deepCopyStr}, default);");
                }
            }
            else
            {
                fg.AppendLine($"this.{field.Name} = {deepCopyStr};");
            }
        }
    }
}
