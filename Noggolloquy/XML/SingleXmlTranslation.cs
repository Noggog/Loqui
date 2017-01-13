using Noggog;
using System;

namespace Noggolloquy
{
    public class SingleXmlTranslation : TypicalXmlTranslation<float>
    {
        public readonly static SingleXmlTranslation Instance = new SingleXmlTranslation();

        public override TryGet<float?> ParseNonNullString(string str)
        {
            float parsed;
            if (float.TryParse(str, out parsed))
            {
                return TryGet<float?>.Success(parsed);
            }
            return TryGet<float?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
