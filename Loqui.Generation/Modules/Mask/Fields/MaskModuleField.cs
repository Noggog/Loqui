using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Loqui.Generation;

public abstract class MaskModuleField
{
    public MaskModule Module;
    public virtual string IndexStr => throw new NotImplementedException();
    public abstract string GetErrorMaskTypeStr(TypeGeneration field);
    public abstract string GetMaskTypeStr(TypeGeneration field, string typeStr);
    public abstract string GetTranslationMaskTypeStr(TypeGeneration field);
    public abstract void GenerateForField(StructuredStringBuilder sb, TypeGeneration field, string valueStr);
    public virtual void GenerateForErrorMask(StructuredStringBuilder sb, TypeGeneration field)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"public {GetErrorMaskTypeStr(field)}? {field.Name};");
    }
    public virtual void GenerateMaskToString(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool topLevel, bool printMask)
    {
        if (!field.IntegrateField) return;
        if (printMask)
        {
            sb.AppendLine($"if ({GenerateBoolMaskCheck(field, "printMask")})");
        }
        using (sb.CurlyBrace(printMask))
        {
            sb.AppendLine($"sb.{nameof(StructuredStringBuilder.AppendItem)}({accessor}{(string.IsNullOrWhiteSpace(field.Name) ? null : $", \"{field.Name}\"")});");
        }
    }
    public abstract void GenerateSetException(StructuredStringBuilder sb, TypeGeneration field);
    public abstract void GenerateSetMask(StructuredStringBuilder sb, TypeGeneration field);
    public abstract void GenerateForCopyMask(StructuredStringBuilder sb, TypeGeneration field);
    public abstract void GenerateForCopyMaskCtor(StructuredStringBuilder sb, TypeGeneration field, string basicValueStr, string deepCopyStr);
    public abstract void GenerateForTranslationMask(StructuredStringBuilder sb, TypeGeneration field);
    public abstract string GenerateForTranslationMaskCrystalization(TypeGeneration field);
    public abstract void GenerateForTranslationMaskSet(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, string onAccessor);
    public abstract void GenerateForAll(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed);
    public abstract void GenerateForAny(StructuredStringBuilder sb, TypeGeneration field, Accessor accessor, bool nullCheck, bool indexed);
    public virtual void GenerateForEqual(StructuredStringBuilder sb, TypeGeneration field, string rhsAccessor)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"if (!object.Equals(this.{field.Name}, {rhsAccessor})) return false;");
    }
    public virtual void GenerateForHashCode(StructuredStringBuilder sb, TypeGeneration field, string rhsAccessor)
    {
        if (!field.IntegrateField) return;
        sb.AppendLine($"hash.Add(this.{field.Name});");
    }
    public abstract void GenerateForTranslate(StructuredStringBuilder sb, TypeGeneration field, string retAccessor, string rhsAccessor, bool indexed);
    public abstract void GenerateForClearEnumerable(StructuredStringBuilder sb, TypeGeneration field);
    public abstract void GenerateForErrorMaskCombine(StructuredStringBuilder sb, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor);
    public abstract string GenerateBoolMaskCheck(TypeGeneration field, string boolMaskAccessor);
    public abstract void GenerateForCtor(StructuredStringBuilder sb, TypeGeneration field, string typeStr, string valueStr);
    public virtual string GetMaskString(TypeGeneration field, string valueStr, string? indexed)
    {
        if (indexed != null)
        {
            return $"({indexed} Index, {valueStr} Value)";
        }
        else
        {
            return valueStr;
        }
    }
}