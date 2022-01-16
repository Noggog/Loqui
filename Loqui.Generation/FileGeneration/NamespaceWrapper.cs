using System;

namespace Loqui.Generation;

public class NamespaceWrapper : IDisposable
{
    private readonly FileGeneration _fg;
    private readonly bool _doThings;
    private readonly bool _fileScoped;

    public NamespaceWrapper(FileGeneration fg, string str, bool fileScoped = true)
    {
        _fileScoped = fileScoped;
        _fg = fg;
        _doThings = !string.IsNullOrWhiteSpace(str);
        if (_doThings)
        {
            fg.AppendLine($"namespace {str}{(_fileScoped ? ";" : null)}");
            if (!_fileScoped)
            {
                fg.AppendLine("{");
                fg.Depth++;
            }
        }
    }

    public void Dispose()
    {
        if (_doThings && !_fileScoped)
        {
            _fg.Depth--;
            _fg.AppendLine("}");
        }
    }
}