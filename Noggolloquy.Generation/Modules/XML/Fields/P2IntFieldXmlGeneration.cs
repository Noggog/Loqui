using System;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class P2IntFieldXmlGeneration : XmlFieldTranslationGeneration
    {
        public const string Element_Name = "P2Int";

        string targetAttr;

        public P2IntFieldXmlGeneration(string targetAttr = "value")
        {
            this.targetAttr = targetAttr;
        }

        public override void GenerateWrite(XmlWriteGenerationParameters param)
        {
            param.FG.AppendLine("using (new ElementWrapper(writer, \"" + Element_Name + "\"))");
            using (new BraceWrapper(param.FG))
            {
                if (!string.IsNullOrEmpty(param.Name))
                {
                    param.FG.AppendLine("writer.WriteAttributeString(\"name\", \"" + param.Name + "\");");
                }

                param.FG.AppendLine($"writer.WriteAttributeString(\"{this.targetAttr}\", {param.Accessor}.X + \",\" + {param.Accessor}.Y);");
            }
        }

        public override IEnumerable<string> GenerateCommonReadVariables(XmlReadGenerationParameters xmlGen, object field)
        {
            yield return "XAttribute val;";
            yield return "int pointX;";
            yield return "int pointY;";
        }

        public override string GetElementName(object field)
        {
            return Element_Name;
        }

        public override void GenerateRead(XmlReadGenerationParameters param)
        {
            param.FG.AppendLine($"if (!{param.XmlNodeName}.TryGetAttribute(\"{this.targetAttr}\", out val)  || string.IsNullOrEmpty(val.Value)) break;");
            param.FG.AppendLine("string[] " + param.Name + "Split = val.Value.Split(',');");
            param.FG.AppendLine("if (" + param.Name + "Split.Length < 2)");
            using (new BraceWrapper(param.FG))
            {
                param.FG.AppendLine("mask.Warnings.Add(\"Skipped field " + param.Name + " due to malformed data: \" + val.Value);");
                param.FG.AppendLine("return;");
            }
            param.FG.AppendLine("if (!int.TryParse(" + param.Name + "Split[0], out pointX) || !int.TryParse(" + param.Name + "Split[1], out pointY))");
            using (new BraceWrapper(param.FG))
            {
                param.FG.AppendLine("mask.Warnings.Add(\"Skipped field " + param.Name + " due to malformed data: \" + val.Value);");
                param.FG.AppendLine("return;");
            }
            param.FG.AppendLine(param.Accessor + " = new Point2D(pointX, pointY);");
        }
    }
}
