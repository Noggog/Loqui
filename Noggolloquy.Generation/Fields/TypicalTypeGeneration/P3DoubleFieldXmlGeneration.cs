using Noggog;
using System;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class P3DoubleFieldXmlGeneration : XmlFieldTranslationGeneration
    {
        public static readonly string Element_Name = typeof(P3Double).Name;

        public override void GenerateWrite(XmlWriteGenerationParameters param)
        {
            param.FG.AppendLine("using (new ElementWrapper(writer, \"" + Element_Name + "\"))");
            using (new BraceWrapper(param.FG))
            {
                if (!string.IsNullOrEmpty(param.Name))
                {
                    param.FG.AppendLine("writer.WriteAttributeString(\"name\", \"" + param.Name + "\");");
                }

                param.FG.AppendLine("writer.WriteAttributeString(\"value\", " + param.Accessor + ".X + \",\" + " + param.Accessor + ".Y + \",\" + " + param.Accessor + ".Z);");
            }
        }

        public override IEnumerable<string> GenerateCommonReadVariables(XmlReadGenerationParameters xmlGen, object field)
        {
            yield return "XAttribute val;";
            yield return "double pointDX;";
            yield return "double pointDY;";
            yield return "double pointDZ;";
        }

        public override string GetElementName(object field)
        {
            return Element_Name;
        }

        public override void GenerateRead(XmlReadGenerationParameters param)
        {
            param.FG.AppendLine("if (!" + param.XmlNodeName + ".TryGetAttribute(\"value\", out val)  || string.IsNullOrEmpty(val.Value)) break;");
            param.FG.AppendLine("string[] " + param.Name + "Split = val.Value.Split(',');");
            param.FG.AppendLine("if (" + param.Name + "Split.Length < 3)");
            using (new BraceWrapper(param.FG))
            {
                param.FG.AppendLine("mask.Warnings.Add(\"Skipped field " + param.Name + " due to malformed data: \" + val.Value);");
                param.FG.AppendLine("return;");
            }
            param.FG.AppendLine("if (!double.TryParse(" + param.Name + "Split[0], out pointDX) || !double.TryParse(" + param.Name + "Split[1], out pointDY) || !double.TryParse(" + param.Name + "Split[2], out pointDZ))");
            using (new BraceWrapper(param.FG))
            {
                param.FG.AppendLine("mask.Warnings.Add(\"Skipped field " + param.Name + " due to malformed data: \" + val.Value);");
                param.FG.AppendLine("return;");
            }
            param.FG.AppendLine(param.Accessor + " = new " + Element_Name + "(pointDX, pointDY, pointDZ);");
        }
    }
}
