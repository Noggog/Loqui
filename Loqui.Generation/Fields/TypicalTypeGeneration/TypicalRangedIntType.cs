using Noggog;
using System.Xml.Linq;

namespace Loqui.Generation;

public class TypicalRangedIntType<T> : TypicalWholeNumberTypeGeneration
{
    string defaultFrom, defaultTo;
        
    public override Type Type(bool getter) => typeof(T);

    public override async Task Load(XElement node, bool requireName = true)
    {
        await base.Load(node, requireName);

        if (!HasDefault) return;

        string[] split = DefaultValue.Split('-');
        if (split.Length != 2)
        {
            throw new ArgumentException("Range field was not properly split with -");
        }

        defaultFrom = split[0];
        defaultTo = split[1];
    }

    protected override string GenerateDefaultValue() => $"new {Type(getter: false).GetName().TrimStringFromEnd("?")}({defaultFrom}, {defaultTo})";
}