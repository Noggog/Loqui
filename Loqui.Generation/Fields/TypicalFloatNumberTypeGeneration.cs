using System.Xml.Linq;

namespace Loqui.Generation;

public abstract class TypicalFloatNumberTypeGeneration : TypicalRangedTypeGeneration
{
    public override async Task Load(XElement node, bool requireName = true)
    {
        await base.Load(node, requireName);
        if (!HasRange) return;

        float minFloat, maxFloat;
        if (string.IsNullOrWhiteSpace(Min) && string.IsNullOrWhiteSpace(Max))
        {
            throw new ArgumentException($"Value was not convertable to range: {Min}-{Max}");
        }

        if (string.IsNullOrWhiteSpace(Min))
        {
            minFloat = float.MinValue;
            Min = "float.MinValue";
        }
        else if (!float.TryParse(Min, out minFloat))
        {
            throw new ArgumentException($"Value was not convertable to float: {Min}");
        }

        if (string.IsNullOrWhiteSpace(Max))
        {
            maxFloat = float.MaxValue;
            Max = "float.MaxValue";
        }
        else if (!float.TryParse(Max, out maxFloat))
        {
            throw new ArgumentException($"Value was not convertable to float: {Max}");
        }

        if (minFloat > maxFloat)
        {
            throw new ArgumentException($"Min {minFloat} was greater than max {maxFloat}");
        }

        if (!minFloat.Equals(float.MinValue)
            && !Min.EndsWith("f"))
        {
            Min += "f";
        }

        if (!maxFloat.Equals(float.MaxValue)
            && !Max.EndsWith("f"))
        {
            Max += "f";
        }
    }
}