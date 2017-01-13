using System;

namespace Noggolloquy.Generation
{
    public class NonExportedObjectXmlGeneration : XmlFieldTranslationGeneration
    {
        public override void GenerateRead(XmlReadGenerationParameters param)
        {
        }

        public override void GenerateWrite(XmlWriteGenerationParameters param)
        {
        }

        public override string GetElementName(object field)
        {
            return "NonExportedObject";
        }
    }
}
