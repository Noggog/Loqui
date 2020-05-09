using System;

namespace Loqui.Generation
{
    public abstract class MaskModuleField
    {
        public MaskModule Module;
        public abstract string GetErrorMaskTypeStr(TypeGeneration field);
        public abstract string GetMaskTypeStr(TypeGeneration field, string typeStr);
        public abstract string GetTranslationMaskTypeStr(TypeGeneration field);
        public abstract void GenerateForField(FileGeneration fg, TypeGeneration field, string valueStr);
        public virtual void GenerateForErrorMask(FileGeneration fg, TypeGeneration field)
        {
            if (!field.IntegrateField) return;
            fg.AppendLine($"public {GetErrorMaskTypeStr(field)}? {field.Name};");
        }
        public virtual void GenerateMaskToString(FileGeneration fg, TypeGeneration field, Accessor accessor, bool topLevel, bool printMask)
        {
            if (!field.IntegrateField) return;
            if (printMask)
            {
                fg.AppendLine($"if ({GenerateBoolMaskCheck(field, "printMask")})");
            }
            using (new BraceWrapper(fg, printMask))
            {
                fg.AppendLine($"fg.{nameof(FileGeneration.AppendItem)}({accessor}{(string.IsNullOrWhiteSpace(field.Name) ? null : $", \"{field.Name}\"")});");
            }
        }
        public abstract void GenerateSetException(FileGeneration fg, TypeGeneration field);
        public abstract void GenerateSetMask(FileGeneration fg, TypeGeneration field);
        public abstract void GenerateForCopyMask(FileGeneration fg, TypeGeneration field);
        public abstract void GenerateForCopyMaskCtor(FileGeneration fg, TypeGeneration field, string basicValueStr, string deepCopyStr);
        public abstract void GenerateForTranslationMask(FileGeneration fg, TypeGeneration field);
        public abstract string GenerateForTranslationMaskCrystalization(TypeGeneration field);
        public abstract void GenerateForTranslationMaskSet(FileGeneration fg, TypeGeneration field, Accessor accessor, string onAccessor);
        public abstract void GenerateForAll(FileGeneration fg, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed);
        public abstract void GenerateForAny(FileGeneration fg, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed);
        public virtual void GenerateForEqual(FileGeneration fg, TypeGeneration field, string rhsAccessor)
        {
            if (!field.IntegrateField) return;
            fg.AppendLine($"if (!object.Equals(this.{field.Name}, {rhsAccessor})) return false;");
        }
        public virtual void GenerateForHashCode(FileGeneration fg, TypeGeneration field, string rhsAccessor)
        {
            if (!field.IntegrateField) return;
            fg.AppendLine($"hash.Add(this.{field.Name});");
        }
        public abstract void GenerateForTranslate(FileGeneration fg, TypeGeneration field, string retAccessor, string rhsAccessor, bool indexed);
        public abstract void GenerateForClearEnumerable(FileGeneration fg, TypeGeneration field);
        public abstract void GenerateForErrorMaskCombine(FileGeneration fg, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor);
        public abstract string GenerateBoolMaskCheck(TypeGeneration field, string boolMaskAccessor);
        public abstract void GenerateForCtor(FileGeneration fg, TypeGeneration field, string typeStr, string valueStr);
        public virtual string GetMaskString(TypeGeneration field, string valueStr, bool indexed)
        {
            if (indexed)
            {
                return $"(int Index, {valueStr} Value)";
            }
            else
            {
                return valueStr;
            }
        }
    }
}
