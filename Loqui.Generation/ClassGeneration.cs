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
        private bool _objectCentralizedDefault;
        public override bool ObjectCentralizedDefault => _objectCentralizedDefault;
        public string BaseClassStr { get; set; }
        private List<ClassGeneration> _derivativeClasses = new List<ClassGeneration>();
        public bool HasDerivativeClasses => _derivativeClasses.Count > 0;

        public ClassGeneration(LoquiGenerator gen, ProtocolGeneration protoGen, FileInfo sourceFile)
            : base(gen, protoGen, sourceFile)
        {
        }

        public override string NewOverride(Func<ObjectGeneration, bool> baseObjFilter = null, bool doIt = true)
        {
            if (!doIt || !HasLoquiBaseObject) return " ";
            if (baseObjFilter == null) return " new ";
            foreach (var baseClass in this.BaseClassTrail())
            {
                if (baseObjFilter(baseClass)) return " new ";
            }
            return " ";
        }

        public override async Task Load()
        {
            BaseClassStr = Node.GetAttribute("baseClass");
            _abstract = Node.GetAttribute<bool>("abstract", false);
            _notifyingDefault = Node.GetAttribute<NotifyingType>("notifyingDefault", this.ProtoGen.NotifyingDefault);
            _hasBeenSetDefault = Node.GetAttribute<bool>("hasBeenSetDefault", this.ProtoGen.HasBeenSetDefault);
            _objectCentralizedDefault = Node.GetAttribute<bool>("objectCentralizedDefault", this.ProtoGen.ObjectCentralizedDefault);

            if (this.NeedsReflectionGeneration)
            {
                this.Interfaces.Add(nameof(ILoquiReflectionSetter));
            }
            else
            {
                this.Interfaces.Add(nameof(ILoquiObjectSetter));
            }

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
                    baseClass._derivativeClasses.Add(this);
                }
            }
            this.WiredBaseClassTCS.Complete();

            await base.Load();
        }

        protected override async Task GenerateClassLine(FileGeneration fg)
        {
            // Generate class header and interfaces
            fg.AppendLine($"public {(this.Abstract ? "abstract " : string.Empty)}partial class {this.ObjectName} : ");

            var list = new List<string>();
            if (HasLoquiBaseObject && this.HasNonLoquiBaseObject)
            {
                throw new ArgumentException("Cannot define both a loqui and non-loqui base class");
            }
            if (HasLoquiBaseObject)
            {
                list.Add(BaseClassName);
            }
            else if (HasNonLoquiBaseObject)
            {
                list.Add(NonLoquiBaseClass);
            }
            list.Add(this.Interface(getter: false));
            if (this.HasInternalInterface)
            {
                list.Add(this.Interface(getter: false, internalInterface: true));
            }
            list.Add($"ILoquiObject<{this.ObjectName}>");
            list.AddRange(this.Interfaces);
            list.AddRange(
                (await Task.WhenAll(this.gen.GenerationModules
                        .Select((tr) => tr.Interfaces(this))))
                .SelectMany(i => i));
            list.AddRange(
                (await Task.WhenAll(this.gen.GenerationModules
                        .Select((tr) => tr.Interfaces(this))))
                .SelectMany(i => i));
            list.AddRange(this.ProtoGen.Interfaces);
            list.AddRange(this.gen.Interfaces);
            list.Add($"IEquatable<{this.ObjectName}>");
            using (new DepthWrapper(fg))
            {
                foreach (var item in list.Distinct().IterateMarkLast())
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
                fg.AppendLine($"{BasicCtorPermission.ToStringFast_Enum_Only()} {this.Name}()");
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

        public override async Task<OverrideType> GetFunctionOverrideType(Func<ClassGeneration, Task<bool>> tester = null)
        {
            if (this.HasLoquiBaseObject)
            {
                foreach (var baseObj in this.BaseClassTrail())
                {
                    if (tester == null || await tester(baseObj))
                    {
                        return OverrideType.HasBase;
                    }
                }
            }
            if (this.HasDerivativeClasses)
            {
                foreach (var derivClass in this.GetDerivativeClasses())
                {
                    if (tester == null || await tester(derivClass))
                    {
                        return OverrideType.OnlyHasDerivative;
                    }
                }
            }
            return OverrideType.None;
        }

        public override OverrideType GetFunctionOverrideType()
        {
            if (this.HasLoquiBaseObject)
            {
                foreach (var baseObj in this.BaseClassTrail())
                {
                    return OverrideType.HasBase;
                }
            }
            if (this.HasDerivativeClasses)
            {
                foreach (var derivClass in this.GetDerivativeClasses())
                {
                    return OverrideType.OnlyHasDerivative;
                }
            }
            return OverrideType.None;
        }

        public IEnumerable<ClassGeneration> GetDerivativeClasses()
        {
            foreach (var item in _derivativeClasses)
            {
                yield return item;
                foreach (var subItem in item.GetDerivativeClasses())
                {
                    yield return subItem;
                }
            }
        }
    }
}
