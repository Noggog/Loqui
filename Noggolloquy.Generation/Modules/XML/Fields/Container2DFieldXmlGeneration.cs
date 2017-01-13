using System;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class Container2DFieldXmlGeneration : XmlFieldTranslationGeneration
    {
        public string ElementName { get; private set; }
        private static StructTypeXmlGeneration pointFieldGen = new StructTypeXmlGeneration("Point2D");

        public Container2DFieldXmlGeneration(string elementName)
        {
            this.ElementName = elementName;
        }

        public override IEnumerable<string> GenerateCommonReadVariables(XmlReadGenerationParameters param, object field)
        {
            ContainerType listType = field as ContainerType;

            foreach (var val in base.GenerateCommonReadVariables(param, field))
            {
                yield return val;
            }

            XmlFieldTranslationGeneration fieldGen;
            if (param.XmlGen.FieldGenerators.TryGetValue(
                listType.SubTypeGeneration.GetType(),
                out fieldGen))
            {
                foreach (var val in fieldGen.GenerateCommonReadVariables(param, field))
                {
                    yield return val;
                }
            }
        }

        public override void GenerateWrite(XmlWriteGenerationParameters param)
        {
            ContainerType listType = param.Field as ContainerType;

            XmlFieldTranslationGeneration subGen;
            if (!param.XmlGen.TryGetFieldGen(listType.SubTypeGeneration.GetType(), out subGen))
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

            XmlFieldTranslationGeneration subGen;
            if (!param.XmlGen.TryGetFieldGen(listType.SubTypeGeneration.GetType(), out subGen))
            {
                throw new ArgumentException();
            }

            param.FG.AppendLine("foreach (var listElem in " + param.XmlNodeName + ".Elements())");
            using (new BraceWrapper(param.FG))
            {
                param.FG.AppendLine("Point2D containerPt;");
                pointFieldGen.GenerateRead(
                    new XmlReadGenerationParameters()
                    {
                        XmlGen = param.XmlGen,
                        FG = param.FG,
                        Obj = param.Obj,
                        Field = listType.SubTypeGeneration,
                        Accessor = "containerPt",
                        GenerateErrorMask = (err) => listType.AddMaskException(param.FG, param.MaskAccessor, err),
                        Name = listType.Name + "Point",
                        XmlNodeName = "listElem"
                    });

                subGen.PrepSubRead(
                    new XmlReadGenerationParameters()
                    {
                        XmlGen = param.XmlGen,
                        FG = param.FG,
                        Obj = param.Obj,
                        Field = param.Field,
                        Accessor = param.Accessor + "[containerPt]",
                        Name = listType.Name + "SubItem",
                        XmlNodeName = "listElem"
                    });
                subGen.GenerateRead(
                    new XmlReadGenerationParameters()
                    {
                        XmlGen = param.XmlGen,
                        FG = param.FG,
                        Obj = param.Obj,
                        Field = listType.SubTypeGeneration,
                        Accessor = param.Accessor + "[containerPt]",
                        GenerateErrorMask = (err) => listType.AddMaskException(param.FG, param.MaskAccessor, err),
                        Name = listType.Name + "SubItem",
                        XmlNodeName = "listElem"
                    });
            }
        }

        public override string GetElementName(object field)
        {
            return this.ElementName;
        }
    }
}
