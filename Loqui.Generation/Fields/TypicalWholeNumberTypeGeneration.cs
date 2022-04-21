using System.Xml.Linq;

namespace Loqui.Generation;

public abstract class TypicalWholeNumberTypeGeneration : TypicalRangedTypeGeneration
{
    public override async Task Load(XElement node, bool requireName = true)
    {
        await base.Load(node, requireName);

        if (!HasRange) return;

        int min, max;
        if (string.IsNullOrWhiteSpace(Min) && string.IsNullOrWhiteSpace(Max))
        {
            throw new ArgumentException($"Value was not convertable to range: {Min}-{Max}");
        }

        if (string.IsNullOrWhiteSpace(Min))
        {
            min = int.MinValue;
            Min = $"{TypeName(getter: false)}.MinValue";
        }
        else if (!int.TryParse(Min, out min))
        {
            throw new ArgumentException($"Value was not convertable to int: {Min}");
        }

        if (string.IsNullOrWhiteSpace(Max))
        {
            max = int.MaxValue;
            Max = $"{TypeName(getter: false)}.MaxValue";
        }
        else if (!int.TryParse(Max, out max))
        {
            throw new ArgumentException($"Value was not convertable to int: {Max}");
        }

        if (min > max)
        {
            throw new ArgumentException($"Min {min} was greater than max {max}");
        }
    }
}