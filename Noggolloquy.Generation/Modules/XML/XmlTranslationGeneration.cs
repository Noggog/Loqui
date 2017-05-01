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
                fg.AppendLine($"public{obj.NewOverride}static {obj.ObjectName} Create_XML(XElement root)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"var ret = new {obj.ObjectName}();");
                    fg.AppendLine($"NoggXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.CopyIn(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"root: root,");
                        fg.AppendLine($"item: ret,");
                        fg.AppendLine($"skipProtected: false,");
                        fg.AppendLine($"doMasks: false,");
                        fg.AppendLine($"mask: out {obj.ErrorMask} errorMask,");
                        fg.AppendLine($"cmds: null);");
                    }
                    fg.AppendLine("return ret;");
                }
                fg.AppendLine();

                fg.AppendLine($"public static {obj.ObjectName} Create_XML(XElement root, out {obj.ErrorMask} errorMask)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"var ret = new {obj.ObjectName}();");
                    fg.AppendLine($"NoggXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.CopyIn(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"root: root,");
                        fg.AppendLine($"item: ret,");
                        fg.AppendLine($"skipProtected: false,");
                        fg.AppendLine($"doMasks: true,");
                        fg.AppendLine($"mask: out errorMask,");
                        fg.AppendLine($"cmds: null);");
                    }
                    fg.AppendLine("return ret;");
                }
                fg.AppendLine();
            }

            if (obj is StructGeneration) return;
            fg.AppendLine("public" + obj.FunctionOverride + "void CopyIn_XML(XElement root, NotifyingFireParameters? cmds = null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"NoggXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.CopyIn(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"root: root,");
                    fg.AppendLine($"item: this,");
                    fg.AppendLine($"skipProtected: true,");
                    fg.AppendLine($"doMasks: false,");
                    fg.AppendLine($"mask: out {obj.ErrorMask} errorMask,");
                    fg.AppendLine($"cmds: cmds);");
                }
            }
            fg.AppendLine();

            fg.AppendLine($"public virtual void CopyIn_XML(XElement root, out {obj.ErrorMask} errorMask, NotifyingFireParameters? cmds = null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"NoggXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.CopyIn(");
                using (new DepthWrapper(fg))
                {
                    fg.AppendLine($"root: root,");
                    fg.AppendLine($"item: this,");
                    fg.AppendLine($"skipProtected: true,");
                    fg.AppendLine($"doMasks: true,");
                    fg.AppendLine($"mask: out errorMask,");
                    fg.AppendLine($"cmds: cmds);");
                }
            }
            fg.AppendLine();

            foreach (var baseClass in obj.BaseClassTrail())
            {
                fg.AppendLine($"public override void CopyIn_XML(XElement root, out {baseClass.ErrorMask} errorMask, NotifyingFireParameters? cmds = null)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"CopyIn_XML(root, out {obj.ErrorMask} errMask, cmds: cmds);");
                    fg.AppendLine("errorMask = errMask;");
                }
                fg.AppendLine();
            }
        }

        private void GenerateWrite(ObjectGeneration obj, FileGeneration fg)
        {
            if (obj.IsTopClass)
            {
                fg.AppendLine("public void Write_XML(Stream stream)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("using (var writer = new XmlTextWriter(stream, Encoding.ASCII))");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("writer.Formatting = Formatting.Indented;");
                        fg.AppendLine("writer.Indentation = 3;");
                        fg.AppendLine("Write_XML(writer);");
                    }
                }
                fg.AppendLine();
            }

            fg.AppendLine($"public void Write_XML(Stream stream, out {obj.ErrorMask} errorMask)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("using (var writer = new XmlTextWriter(stream, Encoding.ASCII))");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("writer.Formatting = Formatting.Indented;");
                    fg.AppendLine("writer.Indentation = 3;");
                    fg.AppendLine("Write_XML(writer, out errorMask);");
                }
            }
            fg.AppendLine();

            fg.AppendLine($"public void Write_XML(XmlWriter writer, out {obj.ErrorMask} errorMask, string name = null)");
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"NoggXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.Write(");
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
            
            if (obj.Abstract)
            {
                if (!obj.BaseClass?.Abstract ?? true)
                {
                    fg.AppendLine("public abstract void Write_XML(XmlWriter writer, string name = null);");
                    fg.AppendLine();
                }
            }
            else if (obj.IsTopClass
                || (!obj.Abstract && (obj.BaseClass?.Abstract ?? true)))
            {
                fg.AppendLine($"public{obj.FunctionOverride}void Write_XML(XmlWriter writer, string name)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"NoggXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.Write(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"writer: writer,");
                        fg.AppendLine($"name: name,");
                        fg.AppendLine($"item: this,");
                        fg.AppendLine($"doMasks: false,");
                        fg.AppendLine($"mask: out {obj.ErrorMask} errorMask);");
                    }
                }
                fg.AppendLine();

                fg.AppendLine($"public{obj.FunctionOverride}void Write_XML(XmlWriter writer)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"NoggXmlTranslation<{obj.ObjectName}, {obj.ErrorMask}>.Instance.Write(");
                    using (new DepthWrapper(fg))
                    {
                        fg.AppendLine($"writer: writer,");
                        fg.AppendLine($"name: null,");
                        fg.AppendLine($"item: this,");
                        fg.AppendLine($"doMasks: false,");
                        fg.AppendLine($"mask: out {obj.ErrorMask} errorMask);");
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
