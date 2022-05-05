using Noggog.StructuredStrings;
using Noggog.StructuredStrings.CSharp;

namespace Loqui.Generation;

public static class MaskGenerationUtility
{
    public static void WrapErrorFieldIndexPush(
        StructuredStringBuilder sb,
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
        if (!string.IsNullOrWhiteSpace(indexAccessor.Access))
        {
            sb.AppendLine($"{errorMaskAccessor}?.PushIndex({indexAccessor});");
        }
        sb.AppendLine("try");
        using (sb.CurlyBrace())
        {
            toDo();
        }
        GenerateExceptionCatcher(sb, errorMaskAccessor);
    }

    public static void GenerateExceptionCatcher(StructuredStringBuilder sb, Accessor errorMaskAccessor)
    {
        sb.AppendLine("catch (Exception ex)");
        sb.AppendLine($"when ({errorMaskAccessor} != null)");
        using (sb.CurlyBrace())
        {
            sb.AppendLine($"{errorMaskAccessor}.ReportException(ex);");
        }
        sb.AppendLine("finally");
        using (sb.CurlyBrace())
        {
            sb.AppendLine("errorMask?.PopIndex();");
        }
    }
}