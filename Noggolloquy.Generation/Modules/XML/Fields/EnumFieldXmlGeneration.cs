using System;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class EnumFieldXmlGeneration : XmlFieldTranslationGeneration
    {
        const string Element_Name = "Enum";

        public override void GenerateWrite(XmlWriteGenerationParameters param)
        {
            EnumType enu = param.Field as EnumType;
            param.FG.AppendLine($"EnumXmlTranslation<{enu.EnumName}>.Instance.Write(writer, \"{param.Name}\", {param.Accessor});");
        }

        public override string GetElementName(object field)
        {
            return Element_Name;
        }

        public override IEnumerable<string> GenerateCommonReadVariables(XmlReadGenerationParameters xmlGen, object field)
        {
            yield return "XAttribute val;";
        }

        public override void GenerateRead(XmlReadGenerationParameters param)
        {
            EnumType enu = param.Field as EnumType;
            param.FG.AppendLine($"TryGet<{enu.EnumName}> parse = EnumXmlTranslation<{enu.EnumName}>.Instance.ParseNoNull({param.XmlNodeName});");
            param.FG.AppendLine("if (parse.Succeeded)");
            using (new BraceWrapper(param.FG))
            {
                param.FG.AppendLine(param.Accessor + " = parse.Value;");
            }
            param.FG.AppendLine("else");
            using (new BraceWrapper(param.FG))
            {
                param.GenerateErrorMask("new ArgumentException(parse.Reason)");
                param.FG.AppendLine("break;");
            }
        }
    }
}
