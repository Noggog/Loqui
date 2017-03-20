using Noggog;
using System;

namespace Noggolloquy.Xml
{
    public class SingleXmlTranslation : TypicalXmlTranslation<float>
    {
        public readonly static SingleXmlTranslation Instance = new SingleXmlTranslation();

        protected override TryGet<float?> ParseNonNullString(string str)
        {
            if (float.TryParse(str, out float parsed))
            {
                return TryGet<float?>.Success(parsed);
            }
            return TryGet<float?>.Failure($"Could not convert to {ElementName}");
        }
    }
}
