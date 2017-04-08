using Noggog;
using System;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    public class XmlTranslationGeneration : GenerationModule
    {
        public override string RegionString => "XML Translation";

        public override IEnumerable<string> RequiredUsingStatements()
        {
            yield return "System.Xml";
            yield return "System.Xml.Linq";
            yield return "System.IO";
            yield return "Noggog.Xml";
            yield return "Noggolloquy.Xml";
        }

        public override IEnumerable<string> Interfaces(ObjectGeneration obj)
        {
            yield break;
        }

        public override void GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            GenerateRead(obj, fg);
            GenerateWrite(obj, fg);
        }

        private void GenerateRead(ObjectGeneration obj, FileGeneration fg)
        {
            var param = new XmlReadGenerationParameters()
            {
                Obj = obj,
                Accessor = "this",
                FG = fg,
                Field = null,
                Name = "Root",
                XmlNodeName = "root",
                XmlGen = this,
                MaskAccessor = "mask"
            };

            if (!obj.Abstract)
            {
                fg.AppendLine($"public static {obj.ObjectName} CreateFromXML(XElement root)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"var ret = new {obj.Name}();");
                    fg.AppendLine($"NoggXmlTranslation<{obj.Name}, {obj.GetErrorMaskItemString()}>.Instance.CopyIn(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"root: root,");
                        fg.AppendLine($"item: ret,");
                        fg.AppendLine($"doMasks: false,");
                        fg.AppendLine($"mask: out {obj.GetErrorMaskItemString()} errorMask,");
                        fg.AppendLine($"cmds: null);");
                    }
                    fg.AppendLine("return ret;");
                }
                fg.AppendLine();

                fg.AppendLine($"public static {obj.ObjectName} CreateFromXML(XElement root, out {obj.GetErrorMaskItemString()} errorMask)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"var ret = new {obj.Name}();");
                    fg.AppendLine($"NoggXmlTranslation<{obj.Name}, {obj.GetErrorMaskItemString()}>.Instance.CopyIn(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"root: root,");
                        fg.AppendLine($"item: ret,");
                        fg.AppendLine($"doMasks: true,");
                        fg.AppendLine($"mask: out errorMask,");
                        fg.AppendLine($"cmds: null);");
                    }
                    fg.AppendLine("return ret;");
                }
                fg.AppendLine();
            }

            if (obj is StructGeneration) return;
            fg.AppendLine("public" + obj.FunctionOverride + "void CopyInFromXML(XElement root, NotifyingFireParameters? cmds = null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"NoggXmlTranslation<{obj.Name}, {obj.GetErrorMaskItemString()}>.Instance.CopyIn(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"root: root,");
                    fg.AppendLine($"item: this,");
                    fg.AppendLine($"doMasks: false,");
                    fg.AppendLine($"mask: out {obj.GetErrorMaskItemString()} errorMask,");
                    fg.AppendLine($"cmds: cmds);");
                }
            }
            fg.AppendLine();

            fg.AppendLine($"public virtual void CopyInFromXML(XElement root, out {obj.GetErrorMaskItemString()} errorMask, NotifyingFireParameters? cmds = null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"NoggXmlTranslation<{obj.Name}, {obj.GetErrorMaskItemString()}>.Instance.CopyIn(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"root: root,");
                    fg.AppendLine($"item: this,");
                    fg.AppendLine($"doMasks: true,");
                    fg.AppendLine($"mask: out errorMask,");
                    fg.AppendLine($"cmds: cmds);");
                }
            }
            fg.AppendLine();

            foreach (var baseClass in obj.BaseClassTrail())
            {
                fg.AppendLine($"public override void CopyInFromXML(XElement root, out {baseClass.GetErrorMaskItemString()} errorMask, NotifyingFireParameters? cmds = null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"var ret = new {obj.GetErrorMaskItemString()}();");
                    fg.AppendLine("errorMask = ret;");
                    fg.AppendLine("CopyInFromXML_Internal(root, ret, cmds: cmds);");
                }
                fg.AppendLine();
            }
        }

        private void GenerateWrite(ObjectGeneration obj, FileGeneration fg)
        {
            if (obj.IsTopClass)
            {
                fg.AppendLine("public void WriteXMLToStream(Stream stream)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("using (var writer = new XmlTextWriter(stream, Encoding.ASCII))");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("writer.Formatting = Formatting.Indented;");
                        fg.AppendLine("writer.Indentation = 3;");
                        fg.AppendLine("WriteXML(writer);");
                    }
                }
                fg.AppendLine();
            }

            if (obj.Abstract)
            {
                if (!obj.BaseClass?.Abstract ?? true)
                {
                    fg.AppendLine("public abstract void WriteXML(XmlWriter writer, string name = null);");
                    fg.AppendLine();
                }
            }
            else if (obj.IsTopClass
                || (!obj.Abstract && (obj.BaseClass?.Abstract ?? true)))
            {
                fg.AppendLine($"public void WriteXML(XmlWriter writer, out {obj.GetErrorMaskItemString()} errorMask, string name = null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"NoggXmlTranslation<{obj.Name}, {obj.GetErrorMaskItemString()}>.Instance.Write(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"writer: writer,");
                        fg.AppendLine($"name: name,");
                        fg.AppendLine($"item: this,");
                        fg.AppendLine($"doMasks: true,");
                        fg.AppendLine($"mask: out errorMask);");
                    }
                }
                fg.AppendLine();

                fg.AppendLine($"public{obj.FunctionOverride}void WriteXML(XmlWriter writer, string name)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"NoggXmlTranslation<{obj.Name}, {obj.GetErrorMaskItemString()}>.Instance.Write(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"writer: writer,");
                        fg.AppendLine($"name: name,");
                        fg.AppendLine($"item: this,");
                        fg.AppendLine($"doMasks: false,");
                        fg.AppendLine($"mask: out {obj.GetErrorMaskItemString()} errorMask);");
                    }
                }
                fg.AppendLine();

                fg.AppendLine($"public{obj.FunctionOverride}void WriteXML(XmlWriter writer)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"NoggXmlTranslation<{obj.Name}, {obj.GetErrorMaskItemString()}>.Instance.Write(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"writer: writer,");
                        fg.AppendLine($"name: null,");
                        fg.AppendLine($"item: this,");
                        fg.AppendLine($"doMasks: false,");
                        fg.AppendLine($"mask: out {obj.GetErrorMaskItemString()} errorMask);");
                    }
                }
                fg.AppendLine();
            }
        }

        public override void Modify(ObjectGeneration obj)
        {
        }

        public override void Modify(NoggolloquyGenerator gen)
        {
        }

        public override void GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public override void Generate(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public override IEnumerable<string> GetWriterInterfaces(ObjectGeneration obj)
        {
            yield break;
        }

        public override IEnumerable<string> GetReaderInterfaces(ObjectGeneration obj)
        {
            yield break;
        }
    }
}
