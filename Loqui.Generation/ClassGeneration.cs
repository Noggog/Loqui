using Noggog;
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

            if (this.NeedsReflectionGeneration)
            {
                this.Interfaces.Add(LoquiInterfaceType.ISetter, nameof(ILoquiReflectionSetter));
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
            using (var args = new ClassWrapper(fg, this.ObjectName))
            {
                args.Abstract = this.Abstract;
                args.Partial = true;
                if (HasLoquiBaseObject && this.HasNonLoquiBaseObject)
                {
                    throw new ArgumentException("Cannot define both a loqui and non-loqui base class");
                }
                if (HasLoquiBaseObject)
                {
                    args.BaseClass = this.BaseClassName;
                }
                else if (HasNonLoquiBaseObject && this.SetBaseClass)
                {
                    args.BaseClass = this.NonLoquiBaseClass;
                }
                args.Interfaces.Add(this.Interface(getter: false, internalInterface: true));
                args.Interfaces.Add($"ILoquiObjectSetter<{this.ObjectName}>");
                args.Interfaces.Add(this.Interfaces.Get(LoquiInterfaceType.Direct));
                args.Interfaces.Add(await this.GetApplicableInterfaces(LoquiInterfaceType.Direct));
                args.Interfaces.Add(this.ProtoGen.Interfaces);
                args.Interfaces.Add(this.gen.Interfaces);
                args.Interfaces.Add($"IEquatable<{this.ObjectName}>");
                args.Interfaces.Add($"IEqualsMask");
            }
        }
        
        protected override async Task GenerateCtor(FileGeneration fg)
        {
            if (this.BasicCtorPermission == CtorPermissionLevel.noGeneration) return;
            using (new RegionWrapper(fg, "Ctor"))
            {
                fg.AppendLine($"{BasicCtorPermission.ToStringFast_Enum_Only()} {this.Name}()");
                using (new BraceWrapper(fg))
                {
                    List<Task> toDo = new List<Task>();
                    toDo.AddRange(this.gen.GenerationModules.Select(mod => mod.GenerateInCtor(this, fg)));
                    var fieldsTask = Task.WhenAll(this.IterateFields().Select(field => field.GenerateForCtor(fg)));
                    toDo.Add(fieldsTask);
                    await fieldsTask;
                    fieldCtorsGenerated.Complete();
                    await Task.WhenAll(toDo);
                    await GenerateInitializer(fg);
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

        public override string Virtual(bool doIt = true)
        {
            if (!doIt) return " ";
            if (this.HasDerivativeClasses) return " virtual "; 
            return " ";
        }
    }
}
