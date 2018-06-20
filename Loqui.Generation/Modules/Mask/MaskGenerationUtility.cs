using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public static class MaskGenerationUtility
    {
        public static void WrapErrorFieldIndexPush(
            FileGeneration fg, 
            Action toDo, 
            string maskAccessor,
            string indexAccessor)
        {
            fg.AppendLine("try");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{maskAccessor}?.PushIndex({indexAccessor});");
                toDo();
            }
            fg.AppendLine("catch (Exception ex)");
            fg.AppendLine($"when ({maskAccessor} != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{maskAccessor}.ReportException(ex);");
            }
            fg.AppendLine("finally");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{maskAccessor}?.PopIndex();");
            }
        }
    }
}
