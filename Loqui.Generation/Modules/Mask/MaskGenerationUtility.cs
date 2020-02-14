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
            Accessor errorMaskAccessor,
            Accessor indexAccessor,
            bool doIt = true)
        {
            if (!doIt)
            {
                toDo();
                return;
            }
            fg.AppendLine("try");
            using (new BraceWrapper(fg))
            {
                if (!string.IsNullOrWhiteSpace(indexAccessor.DirectAccess))
                {
                    fg.AppendLine($"{errorMaskAccessor}?.PushIndex({indexAccessor});");
                }
                toDo();
            }
            fg.AppendLine("catch (Exception ex)");
            fg.AppendLine($"when ({errorMaskAccessor} != null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{errorMaskAccessor}.ReportException(ex);");
            }
            fg.AppendLine("finally");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{errorMaskAccessor}?.PopIndex();");
            }
        }
    }
}
