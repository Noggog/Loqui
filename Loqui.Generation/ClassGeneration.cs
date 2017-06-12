using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Loqui.Generation
{
    public class ClassGeneration : ObjectGeneration
    {
        private bool _abstract;
        public override bool Abstract => _abstract;
        private NotifyingOption _notifyingDefault;
        public override NotifyingOption NotifyingDefault => _notifyingDefault;
        public string BaseClassStr { get; set; }
        public List<ClassGeneration> DerivativeClasses = new List<ClassGeneration>();
        public bool HasDerivativeClasses => DerivativeClasses.Count > 0; 
        public override string FunctionOverride
        {
            get
            {
                if (this.HasBaseObject)
                {
                    return " override ";
                }
                if (this.HasDerivativeClasses)
                {
                    return " virtual ";
                }
                else
                {
                    return " ";
                }
            }
        }
        public override string NewOverride => HasBaseObject ? " new " : " ";

        public ClassGeneration(LoquiGenerator gen, ProtocolGeneration protoGen, FileInfo sourceFile)
            : base(gen, protoGen, sourceFile)
        {
        }

        public override void Load()
        {
            BaseClassStr = Node.GetAttribute("baseClass");
            _abstract = Node.GetAttribute<bool>("abstract", false);
            _notifyingDefault = Node.GetAttribute<NotifyingOption>("notifyingDefault", NotifyingOption.None);
            
            this.Interfaces.Add($"ILoquiObjectSetter");

            base.Load();
        }

        public override void Resolve()
        {
            if (!string.IsNullOrWhiteSpace(this.BaseClassStr))
            {
                if (!this.ProtoGen.ObjectGenerationsByName.TryGetValue(this.BaseClassStr, out ObjectGeneration baseObj)
                    || !(baseObj is ClassGeneration))
                {
                    throw new ArgumentException($"Could not resolve base class object: {this.BaseClassStr}");
                }
                else
                {
                    ClassGeneration baseClass = baseObj as ClassGeneration;
                    this.BaseClass = baseClass;
                    baseClass.DerivativeClasses.Add(this);
                }
            }
            base.Resolve();
        }

        protected override void GenerateClassLine(FileGeneration fg)
        {
            // Generate class header and interfaces
            using (new LineWrapper(fg))
            {
                fg.Append($"public {(this.Abstract ? "abstract " : string.Empty)}partial class {Name}{this.GenericTypes} : ");

                var list = new List<string>();
                if (HasBaseObject)
                {
                    list.Add(BaseClassName);
                }
                list.Add(this.InterfaceStr);
                list.AddRange(
                    this.Interfaces
                        .Union(this.gen.GenerationModules
                            .SelectMany((tr) => tr.Interfaces(this)))
                        .Union(this.gen.GenerationModules
                            .SelectMany((tr) => tr.GetReaderInterfaces(this)))
                        .Union(this.GenerationInterfaces
                            .SelectMany((tr) => tr.Interfaces(this))));
                list.Add($"IEquatable<{this.ObjectName}>");
                fg.Append(string.Join(", ", list));
            }
        }

        protected override void GenerateEqualsCode(FileGeneration fg)
        {
            fg.AppendLine($"if (!(obj is {this.ObjectName} rhs)) return false;");
            fg.AppendLine("return Equals(rhs);");
        }

        protected override void GenerateCtor(FileGeneration fg)
        {
            using (new RegionWrapper(fg, "Ctor"))
            {
                fg.AppendLine($"{(this.GeneratePublicBasicCtor ? "public" : "protected")} {this.Name}()");
                using (new BraceWrapper(fg))
                {
                    foreach (var field in this.Fields)
                    {
                        field.GenerateForCtor(fg);
                    }
                    fg.AppendLine("CustomCtor();");
                }
                fg.AppendLine("partial void CustomCtor();");
            }
        }
    }
}
