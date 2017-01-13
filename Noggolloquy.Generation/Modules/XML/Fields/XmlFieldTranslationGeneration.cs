using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public abstract class XmlFieldTranslationGeneration
    {
        public abstract string GetElementName(object field);
        public abstract void GenerateWrite(XmlWriteGenerationParameters param);
        public virtual IEnumerable<string> GenerateCommonReadVariables(XmlReadGenerationParameters xmlGen, object field)
        {
            yield break;
        }
        public abstract void GenerateRead(XmlReadGenerationParameters param);
        public virtual void PrepSubRead(XmlReadGenerationParameters param)
        {
        }
    }
}
