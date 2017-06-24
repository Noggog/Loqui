using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class UnsafeMaskFieldGeneration : TypicalMaskFieldGeneration
    {
        public override string GetErrorMaskTypeStr(TypeGeneration field)
        {
            return "object";
        }

        public override void GenerateForErrorMask(FileGeneration fg, TypeGeneration field)
        {
            GenerateForField(fg, field, "object");
        }

        public override void GenerateForErrorMaskCombine(FileGeneration fg, TypeGeneration field, string accessor, string retAccessor, string rhsAccessor)
        {
            fg.AppendLine($"{retAccessor} = {accessor} ?? {rhsAccessor};");
        }

        public override void GenerateSetMask(FileGeneration fg, TypeGeneration field)
        {
            fg.AppendLine($"this.{field.Name} = obj;");
        }
    }
}
