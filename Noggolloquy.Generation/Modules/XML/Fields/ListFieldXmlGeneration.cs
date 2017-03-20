using System;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class ListFieldXmlGeneration : XmlFieldTranslationGeneration
    {
        public virtual string ElementName { get { return "List"; } }

        public override IEnumerable<string> GenerateCommonReadVariables(XmlReadGenerationParameters param, object field)
        {
            ContainerType listType = field as ContainerType;

            foreach (var val in base.GenerateCommonReadVariables(param, field))
            {
                yield return val;
            }

            if (param.XmlGen.FieldGenerators.TryGetValue(
                listType.SubTypeGeneration.GetType(),
                out XmlFieldTranslationGeneration fieldGen))
            {
                var subParam = param.Copy();
                subParam.Field = listType.SubTypeGeneration;
                foreach (var val in fieldGen.GenerateCommonReadVariables(subParam, listType.SubTypeGeneration))
                {
                    yield return val;
                }
            }
        }

        public override void GenerateWrite(XmlWriteGenerationParameters param)
        {
            ContainerType listType = param.Field as ContainerType;

            if (!param.XmlGen.TryGetFieldGen(listType.SubTypeGeneration.GetType(), out XmlFieldTranslationGeneration subGen))
            {
                throw new ArgumentException();
            }

            param.FG.AppendLine("using (new ElementWrapper(writer, \"" + ElementName + "\"))");
            using (new BraceWrapper(param.FG))
            {
                if (!string.IsNullOrEmpty(param.Name))
                {
                    param.FG.AppendLine("writer.WriteAttributeString(\"name\", \"" + param.Name + "\");");
                }

                param.FG.AppendLine("foreach (var listObj in " + param.Accessor + ")");
                using (new BraceWrapper(param.FG))
                {
                    subGen.GenerateWrite(
                        new XmlWriteGenerationParameters()
                        {
                            XmlGen = param.XmlGen,
                            Object = param.Object,
                            FG = param.FG,
                            Field = listType.SubTypeGeneration,
                            Accessor = "listObj",
                            Name = null
                        });
                }
            }
        }

        public override void GenerateRead(XmlReadGenerationParameters param)
        {
            ContainerType listType = param.Field as ContainerType;

            if (!param.XmlGen.TryGetFieldGen(listType.SubTypeGeneration.GetType(), out XmlFieldTranslationGeneration subGen))
            {
                throw new ArgumentException();
            }

            param.FG.AppendLine("foreach (var listElem in " + param.XmlNodeName + ".Elements())");
            using (new BraceWrapper(param.FG))
            {
                param.FG.AppendLine(listType.SubTypeGeneration.TypeName + " tmpItem;");
                LevType levType = listType.SubTypeGeneration as LevType;
                subGen.PrepSubRead(
                    new XmlReadGenerationParameters()
                    {
                        XmlGen = param.XmlGen,
                        FG = param.FG,
                        Obj = param.Obj,
                        Field = param.Field,
                        Accessor = "tmpItem",
                        XmlNodeName = "dictElem"
                    });
                subGen.GenerateRead(
                    new XmlReadGenerationParameters()
                    {
                        XmlGen = param.XmlGen,
                        FG = param.FG,
                        Obj = param.Obj,
                        Field = listType.SubTypeGeneration,
                        Accessor = "tmpItem",
                        MaskAccessor = "tmpItem_Mask",
                        GenerateErrorMask = (err) => listType.AddMaskException(param.FG, param.MaskAccessor, err),
                        Name = listType.Name + "SubItem",
                        XmlNodeName = "listElem"
                    });
                param.FG.AppendLine(param.Accessor + ".Add(tmpItem);");
            }
        }

        public override string GetElementName(object field)
        {
            return this.ElementName;
        }
    }
}
