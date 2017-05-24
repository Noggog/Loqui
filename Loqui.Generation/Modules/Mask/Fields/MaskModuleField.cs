using System;

namespace Loqui.Generation
{
    public abstract class MaskModuleField
    {
        public MaskModule Module;
        public abstract void GenerateForField(FileGeneration fg, TypeGeneration field, string valueStr);
        public virtual void GenerateForErrorMask(FileGeneration fg, TypeGeneration field)
        {
            GenerateForField(fg, field, "Exception");
        }
        public virtual void GenerateForErrorMaskToString(FileGeneration fg, TypeGeneration field, string accessor, bool topLevel)
        {
            fg.AppendLine($"fg.{nameof(FileGeneration.AppendLine)}({accessor}.ToString());");
        }
        public abstract void GenerateSetException(FileGeneration fg, TypeGeneration field);
        public abstract void GenerateSetMask(FileGeneration fg, TypeGeneration field);
        public abstract void GenerateForCopyMask(FileGeneration fg, TypeGeneration field);
        public abstract void GenerateForAllEqual(FileGeneration fg, TypeGeneration field);
    }
}
