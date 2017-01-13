using System;

namespace Noggolloquy.Generation
{
    public class XmlReadGenerationParameters
    {
        public XmlTranslationGeneration XmlGen;
        public ObjectGeneration Obj;
        public FileGeneration FG;
        public TypeGeneration Field;
        public string Accessor;
        public string MaskAccessor;
        public Action<string> GenerateErrorMask;
        public string Name;
        public string XmlNodeName;

        public XmlReadGenerationParameters Copy()
        {
            return new XmlReadGenerationParameters()
            {
                XmlGen = this.XmlGen,
                Obj = this.Obj,
                FG = this.FG,
                Field = this.Field,
                GenerateErrorMask = this.GenerateErrorMask,
                Accessor = this.Accessor,
                MaskAccessor = this.MaskAccessor,
                Name = this.Name,
                XmlNodeName = this.XmlNodeName
            };
        }
    }
}
