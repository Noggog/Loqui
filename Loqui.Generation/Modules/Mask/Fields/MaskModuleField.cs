using System;

namespace Loqui.Generation
{
    public abstract class MaskModuleField
    {
        public MaskModule Module;
        public abstract string GetErrorMaskTypeStr(TypeGeneration field);
        public abstract void GenerateForField(FileGeneration fg, TypeGeneration field, string valueStr);
        public virtual void GenerateForErrorMask(FileGeneration fg, TypeGeneration field)
        {
            GenerateForField(fg, field, "Exception");
        }
        public virtual void GenerateForErrorMaskToString(FileGeneration fg, TypeGeneration field, string accessor, bool topLevel)
        {
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}($\"{field.Name} => {{{accessor}.ToStringSafe()}}\");");
        }
        public abstract void GenerateSetException(FileGeneration fg, TypeGeneration field);
        public abstract void GenerateSetMask(FileGeneration fg, TypeGeneration field);
        public abstract void GenerateForCopyMask(FileGeneration fg, TypeGeneration field);
        public abstract void GenerateForAllEqual(FileGeneration fg, TypeGeneration field);
        public abstract void GenerateForTranslate(FileGeneration fg, TypeGeneration field, string retAccessor, string rhsAccessor);
        public abstract void GenerateForErrorMaskCombine(FileGeneration fg, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor);
        public abstract string GenerateBoolMaskCheck(TypeGeneration field, string maskAccessor);
        public abstract void GenerateForCtor(FileGeneration fg, TypeGeneration field, string valueStr);
    }
}
