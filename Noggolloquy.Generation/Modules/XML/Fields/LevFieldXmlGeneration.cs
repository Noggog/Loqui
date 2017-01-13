using System;

namespace Noggolloquy.Generation
{
    public class LevFieldXmlGeneration : XmlFieldTranslationGeneration
    {
        public override void GenerateWrite(XmlWriteGenerationParameters param)
        {
            var levType = param.Field as LevType;
            if (levType.RefType == LevType.LevRefType.Generic
                || levType.RefGen.Obj is ClassGeneration)
            {
                param.FG.AppendLine(param.Accessor + "?.WriteXML(writer" + (param.Name == null ? string.Empty : ", \"" + param.Name + "\"") + ");");
            }
            else if (levType.RefGen.Obj is StructGeneration)
            {
                param.FG.AppendLine(param.Accessor + ".WriteXML(writer" + (param.Name == null ? string.Empty : ", \"" + param.Name + "\"") + ");");
            }
        }

        public override void GenerateRead(XmlReadGenerationParameters param)
        {
            LevType levType = param.Field as LevType;
            param.FG.AppendLine($"{levType.GenerateMaskItemString("Exception")} {levType.RefGen?.Obj.Name}_ErrorMask = null;");
            if (levType.RefType == LevType.LevRefType.Generic)
            {
                param.FG.AppendLine("throw new NotImplementedException();");
            }
            else if (levType.RefGen.Obj is ClassGeneration)
            {
                if (levType.SingletonMember)
                {
                    param.FG.AppendLine($"if (mask != null)");
                    using (new BraceWrapper(param.FG))
                    {
                        param.FG.AppendLine($"{param.Accessor}.CopyInFromXML({param.XmlNodeName}, out {levType.RefGen.Obj.Name}_ErrorMask);");
                        param.FG.AppendLine($"{param.MaskAccessor}.{levType.Name} = new MaskItem<Exception, {levType.RefGen.Obj.GetMaskString("Exception")}>(null, {levType.RefGen.Obj.Name}_ErrorMask);");
                    }
                    param.FG.AppendLine("else");
                    using (new BraceWrapper(param.FG))
                    {
                        param.FG.AppendLine($"{param.Accessor}.CopyInFromXML({param.XmlNodeName});");
                    }
                }
                else
                {
                    param.FG.AppendLine($"{param.Name}Obj = new {levType.TypeName}({GetCtorParameters(param)});");
                    param.FG.AppendLine($"if (mask != null)");
                    using (new BraceWrapper(param.FG))
                    {
                        param.FG.AppendLine($"{param.Name}Obj.CopyInFromXML({param.XmlNodeName}, out {levType.RefGen.Obj.Name}_ErrorMask);");
                        param.GenerateErrorMask($"{levType.RefGen.Obj.Name}_ErrorMask");
                    }
                    param.FG.AppendLine("else");
                    using (new BraceWrapper(param.FG))
                    {
                        param.FG.AppendLine($"{param.Name}Obj.CopyInFromXML({param.XmlNodeName});");
                    }
                    param.FG.AppendLine(param.Accessor + " = " + param.Name + "Obj;");
                }
            }
            else if (levType.RefGen.Obj is StructGeneration)
            {
                param.FG.AppendLine($"if (mask != null)");
                using (new BraceWrapper(param.FG))
                {
                    param.FG.AppendLine($"{param.Accessor} = {levType.RefGen.ObjectName}.CreateFromXML(root, out {levType.RefGen.Obj.Name}_ErrorMask);");
                    param.GenerateErrorMask($"new MaskItem<Exception, {levType.RefGen.Obj.GetMaskString("Exception")}>(null, {levType.RefGen.Obj.Name}_ErrorMask)");
                }
                param.FG.AppendLine("else");
                using (new BraceWrapper(param.FG))
                {
                    param.FG.AppendLine($"{param.Accessor} = {levType.RefGen.ObjectName}.CreateFromXML(root);");
                }
            }
        }

        protected virtual string GetCtorParameters(XmlReadGenerationParameters param)
        {
            return string.Empty;
        }

        public override string GetElementName(object field)
        {
            LevType lev = field as LevType;
            return lev.TypeName;
        }
    }
}
