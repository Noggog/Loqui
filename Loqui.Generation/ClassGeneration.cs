using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class ClassGeneration : ObjectGeneration
    {
        private bool _abstract;
        public override bool Abstract => _abstract;
        private NotifyingType _notifyingDefault;
        public override NotifyingType NotifyingDefault => _notifyingDefault;
        private bool _hasBeenSetDefault;
        public override bool HasBeenSetDefault => _hasBeenSetDefault;
        public string BaseClassStr { get; set; }
        public List<ClassGeneration> DerivativeClasses = new List<ClassGeneration>();
        public bool HasDerivativeClasses => DerivativeClasses.Count > 0;
        public override string NewOverride => HasBaseObject ? " new " : " ";

        public ClassGeneration(LoquiGenerator gen, ProtocolGeneration protoGen, FileInfo sourceFile)
            : base(gen, protoGen, sourceFile)
        {
        }

        public override async Task Load()
        {
            BaseClassStr = Node.GetAttribute("baseClass");
            _abstract = Node.GetAttribute<bool>("abstract", false);
            _notifyingDefault = Node.GetAttribute<NotifyingType>("notifyingDefault", this.ProtoGen.NotifyingDefault);
            _hasBeenSetDefault = Node.GetAttribute<bool>("hasBeenSetDefault", this.ProtoGen.HasBeenSetDefault);

            this.Interfaces.Add($"ILoquiObjectSetter");

            if (ObjectNamedKey.TryFactory(this.BaseClassStr, this.ProtoGen.Protocol, out var baseClassObjKey))
            {
                if (!this.gen.ObjectGenerationsByObjectNameKey.TryGetValue(baseClassObjKey, out ObjectGeneration baseObj)
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
            this.WiredBaseClassTCS.Complete();

            await base.Load();
        }

        protected override void GenerateClassLine(FileGeneration fg)
        {
            // Generate class header and interfaces
            fg.AppendLine($"public {(this.Abstract ? "abstract " : string.Empty)}partial class {this.ObjectName} : ");

            var list = new List<string>();
            if (HasBaseObject)
            {
                list.Add(BaseClassName);
            }
            list.Add(this.InterfaceStr);
            list.Add($"ILoquiObject<{this.ObjectName}>");
            list.AddRange(
                this.Interfaces
                    .Union(this.gen.GenerationModules
                        .SelectMany((tr) => tr.Interfaces(this)))
                    .Union(this.gen.GenerationModules
                        .SelectMany((tr) => tr.GetReaderInterfaces(this)))
                    .Union(this.GenerationInterfaces
                        .SelectMany((tr) => tr.Interfaces(this))));
            list.Add($"IEquatable<{this.ObjectName}>");
            using (new DepthWrapper(fg))
            {
                foreach (var item in list.IterateMarkLast())
                {
                    fg.AppendLine($"{item.Item}{(item.Last ? null : ",")}");
                }
            }
        }

        protected override void GenerateEqualsCode(FileGeneration fg)
        {
            fg.AppendLine($"if (!(obj is {this.ObjectName} rhs)) return false;");
            fg.AppendLine("return Equals(rhs);");
        }

        protected override async Task GenerateCtor(FileGeneration fg)
        {
            using (new RegionWrapper(fg, "Ctor"))
            {
                fg.AppendLine($"{(this.GeneratePublicBasicCtor ? "public" : "protected")} {this.Name}()");
                using (new BraceWrapper(fg))
                {
                    foreach (var mod in this.gen.GenerationModules)
                    {
                        await mod.GenerateInCtor(this, fg);
                    }
                    foreach (var field in this.IterateFields())
                    {
                        field.GenerateForCtor(fg);
                    }
                    fg.AppendLine("CustomCtor();");
                }
                fg.AppendLine("partial void CustomCtor();");
            }
        }

        public override async Task<string> FunctionOverride(Func<ClassGeneration, Task<bool>> tester = null)
        {
            if (this.HasBaseObject)
            {
                foreach (var baseObj in this.BaseClassTrail())
                {
                    if (tester == null || await tester(baseObj))
                    {
                        return " override ";
                    }
                }
            }
            if (this.HasDerivativeClasses)
            {
                foreach (var baseObj in this.DerivativeClasses)
                {
                    if (tester == null || await tester(baseObj))
                    {
                        return " virtual ";
                    }
                }
            }
            return " ";
        }
    }
}
