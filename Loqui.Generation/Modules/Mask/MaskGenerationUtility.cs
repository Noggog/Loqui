namespace Loqui.Generation;

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
        if (!string.IsNullOrWhiteSpace(indexAccessor.Access))
        {
            fg.AppendLine($"{errorMaskAccessor}?.PushIndex({indexAccessor});");
        }
        fg.AppendLine("try");
        using (new BraceWrapper(fg))
        {
            toDo();
        }
        GenerateExceptionCatcher(fg, errorMaskAccessor);
    }

    public static void GenerateExceptionCatcher(FileGeneration fg, Accessor errorMaskAccessor)
    {
        fg.AppendLine("catch (Exception ex)");
        fg.AppendLine($"when ({errorMaskAccessor} != null)");
        using (new BraceWrapper(fg))
        {
            fg.AppendLine($"{errorMaskAccessor}.ReportException(ex);");
        }
        fg.AppendLine("finally");
        using (new BraceWrapper(fg))
        {
            fg.AppendLine("errorMask?.PopIndex();");
        }
    }
}