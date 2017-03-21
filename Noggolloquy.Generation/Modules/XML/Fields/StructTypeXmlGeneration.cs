using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class StructTypeXmlGeneration : XmlFieldTranslationGeneration
    {
        public string TypeName { get; private set; }
        public string ElementName { get { return TypeName.Replace('?', 'N'); } }
        public string NullLessName { get { return TypeName.Replace("?", string.Empty); } }
        private bool? nullable;
        public bool IsNullable
        {
            get { return this.nullable.HasValue ? nullable.Value : !this.NullLessName.Equals(TypeName); }
            set { this.nullable = value; }
        }

        public StructTypeXmlGeneration(string typeName)
        {
            this.TypeName = typeName;
        }

        public string OutsourceClassName
        {
            get
            {
                return $"{NullLessName}XmlTranslation";
            }
        }

        public override string GetElementName(object field)
        {
            return this.NullLessName;
        }

        public override void GenerateWrite(XmlWriteGenerationParameters param)
        {
            param.FG.AppendLine($"{OutsourceClassName}.Instance.Write(writer, \"{param.Name}\", {param.Accessor});");
        }

        public override IEnumerable<string> GenerateCommonReadVariables(XmlReadGenerationParameters xmlGen, object field)
        {
            yield break;
        }

        public override void GenerateRead(XmlReadGenerationParameters param)
        { 
            param.FG.AppendLine($"{param.Accessor} = {OutsourceClassName}.Instance.Parse{(IsNullable ? string.Empty : "NoNull")}({param.XmlNodeName}).EvaluateOrThrow();");
        }
    }
}
