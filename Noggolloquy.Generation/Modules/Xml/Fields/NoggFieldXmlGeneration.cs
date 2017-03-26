using System;

namespace Noggolloquy.Generation
{
    public class NoggFieldXmlGeneration : XmlFieldTranslationGeneration
    {
        public override void GenerateWrite(XmlWriteGenerationParameters param)
        {
            var noggType = param.Field as NoggType;
            if (noggType.RefType == NoggType.NoggRefType.Generic
                || noggType.RefGen.Obj is ClassGeneration)
            {
                param.FG.AppendLine(param.Accessor + "?.WriteXML(writer" + (param.Name == null ? string.Empty : ", \"" + param.Name + "\"") + ");");
            }
            else if (noggType.RefGen.Obj is StructGeneration)
            {
                param.FG.AppendLine(param.Accessor + ".WriteXML(writer" + (param.Name == null ? string.Empty : ", \"" + param.Name + "\"") + ");");
            }
        }

        public override void GenerateRead(XmlReadGenerationParameters param)
        {
            NoggType noggType = param.Field as NoggType;
            if (noggType.RefType == NoggType.NoggRefType.Generic)
            {
                param.FG.AppendLine("throw new NotImplementedException();");
            }
            else if (noggType.RefGen.Obj is ClassGeneration)
            {
                if (noggType.SingletonMember)
                {
                    param.FG.AppendLine($"if (mask != null)");
                    using (new BraceWrapper(param.FG))
                    {
                        param.FG.AppendLine($"{param.Accessor}.CopyInFromXML({param.XmlNodeName}, out {noggType.GenerateErrorMaskItemString()} errorMask);");
                        param.FG.AppendLine($"{param.MaskAccessor}.{noggType.Name} = new MaskItem<Exception, {noggType.RefGen.Obj.GetErrorMaskItemString()}>(null, errorMask);");
                    }
                    param.FG.AppendLine("else");
                    using (new BraceWrapper(param.FG))
                    {
                        param.FG.AppendLine($"{param.Accessor}.CopyInFromXML({param.XmlNodeName});");
                    }
                }
                else
                {
                    param.FG.AppendLine($"{param.Name}Obj = new {noggType.TypeName}({GetCtorParameters(param)});");
                    param.FG.AppendLine($"if (mask != null)");
                    using (new BraceWrapper(param.FG))
                    {
                        param.FG.AppendLine($"{param.Name}Obj.CopyInFromXML({param.XmlNodeName}, out {noggType.GenerateErrorMaskItemString()} errorMask);");
                        param.GenerateErrorMask($"errorMask");
                    }
                    param.FG.AppendLine("else");
                    using (new BraceWrapper(param.FG))
                    {
                        param.FG.AppendLine($"{param.Name}Obj.CopyInFromXML({param.XmlNodeName});");
                    }
                    param.FG.AppendLine(param.Accessor + " = " + param.Name + "Obj;");
                }
            }
            else if (noggType.RefGen.Obj is StructGeneration)
            {
                param.FG.AppendLine($"if (mask != null)");
                using (new BraceWrapper(param.FG))
                {
                    param.FG.AppendLine($"{param.Accessor} = {noggType.RefGen.ObjectName}.CreateFromXML(root, out {noggType.GenerateErrorMaskItemString()} errorMask);");
                    param.GenerateErrorMask($"new MaskItem<Exception, {noggType.GenerateErrorMaskItemString()}>(null, errorMask)");
                }
                param.FG.AppendLine("else");
                using (new BraceWrapper(param.FG))
                {
                    param.FG.AppendLine($"{param.Accessor} = {noggType.RefGen.ObjectName}.CreateFromXML(root);");
                }
            }
        }

        protected virtual string GetCtorParameters(XmlReadGenerationParameters param)
        {
            return string.Empty;
        }

        public override string GetElementName(object field)
        {
            NoggType nogg = field as NoggType;
            return nogg.TypeName;
        }
    }
}
