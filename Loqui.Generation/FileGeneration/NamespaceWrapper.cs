using System;

namespace Loqui.Generation
{
    public class NamespaceWrapper : IDisposable
    {
        FileGeneration fg;
        bool doThings;

        public NamespaceWrapper(FileGeneration fg, string str)
        {
            this.fg = fg;
            doThings = !string.IsNullOrWhiteSpace(str);
            if (doThings)
            {
                fg.AppendLine($"namespace {str}");
                fg.AppendLine("{");
                fg.Depth++;
            }
        }

        public void Dispose()
        {
            if (doThings)
            {
                fg.Depth--;
                fg.AppendLine("}");
            }
        }
    }
}
