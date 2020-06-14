using Noggog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public interface IGenerationModule
    {
        string RegionString { get; }
        string Name { get; }
        IAsyncEnumerable<string> RequiredUsingStatements(ObjectGeneration obj);
        IAsyncEnumerable<(LoquiInterfaceType Location, string Interface)> Interfaces(ObjectGeneration obj);
        Task PreLoad(ObjectGeneration obj);
        Task LoadWrapup(ObjectGeneration obj);
        Task PostLoad(ObjectGeneration obj);
        Task PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node);
        Task Modify(LoquiGenerator gen);
        Task GenerateInStaticCtor(ObjectGeneration obj, FileGeneration fg);
        Task GenerateInClass(ObjectGeneration obj, FileGeneration fg);
        Task GenerateInNonGenericClass(ObjectGeneration obj, FileGeneration fg);
        Task GenerateInCtor(ObjectGeneration obj, FileGeneration fg);
        Task GenerateInCommon(ObjectGeneration obj, FileGeneration fg, MaskTypeSet maskTypes);
        Task GenerateInCommonMixin(ObjectGeneration obj, FileGeneration fg);
        Task GenerateInVoid(ObjectGeneration obj, FileGeneration fg);
        Task GenerateInInterface(ObjectGeneration obj, FileGeneration fg, bool internalInterface, bool getter);
        Task GenerateInRegistration(ObjectGeneration obj, FileGeneration fg);
        Task MiscellaneousGenerationActions(ObjectGeneration obj);
        Task Resolve(ObjectGeneration obj);
        Task FinalizeGeneration(ProtocolGeneration proto);
    }

    public abstract class GenerationModule : IGenerationModule
    {
        public virtual string RegionString { get; }
        public GenerationModuleCollection SubModules = new GenerationModuleCollection();
        public string Name => RegionString ?? this.GetType().Name;

        public virtual IAsyncEnumerable<string> RequiredUsingStatements(ObjectGeneration obj)
        {
            return SubModules.RequiredUsingStatements(obj);
        }

        public virtual IAsyncEnumerable<(LoquiInterfaceType Location, string Interface)> Interfaces(ObjectGeneration obj)
        {
            return SubModules.Interfaces(obj);
        }

        public virtual Task PreLoad(ObjectGeneration obj)
        {
            return SubModules.PreLoad(obj)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} pre load taking a long time."));
        }

        public virtual Task LoadWrapup(ObjectGeneration obj)
        {
            return SubModules.LoadWrapup(obj)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} load wrap up taking a long time."));
        }

        public virtual Task PostLoad(ObjectGeneration obj)
        {
            return SubModules.PostLoad(obj)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} post load taking a long time."));
        }

        public virtual Task PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node)
        {
            return SubModules.PostFieldLoad(obj, field, node)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name}.{field.Name} post load taking a long time."));
        }

        public virtual Task Modify(LoquiGenerator gen)
        {
            return SubModules.Modify(gen)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} modify taking a long time."));
        }

        public virtual Task GenerateInStaticCtor(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInStaticCtor(obj, fg)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in static ctor taking a long time."));
        }

        public virtual Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInClass(obj, fg)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in class taking a long time."));
        }

        public virtual Task GenerateInNonGenericClass(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInNonGenericClass(obj, fg)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in non-generic class taking a long time."));
        }

        public virtual Task GenerateInCtor(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInCtor(obj, fg)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in ctor taking a long time."));
        }

        public virtual Task GenerateInCommon(ObjectGeneration obj, FileGeneration fg, MaskTypeSet maskTypes)
        {
            return SubModules.GenerateInCommon(obj, fg, maskTypes)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in common taking a long time."));
        }

        public virtual Task GenerateInCommonMixin(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInCommonMixin(obj, fg)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in common mixin taking a long time."));
        }

        public virtual Task GenerateInVoid(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInVoid(obj, fg)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in void taking a long time."));
        }

        public virtual Task GenerateInInterface(ObjectGeneration obj, FileGeneration fg, bool internalInterface, bool getter)
        {
            return SubModules.GenerateInInterface(obj, fg, internalInterface, getter)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in interface getter taking a long time."));
        }

        public virtual Task GenerateInRegistration(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInRegistration(obj, fg)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in registration taking a long time."));
        }

        public virtual Task MiscellaneousGenerationActions(ObjectGeneration obj)
        {
            return SubModules.MiscellaneousGenerationActions(obj)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate misc actions taking a long time."));
        }

        public virtual Task Resolve(ObjectGeneration obj)
        {
            return SubModules.Resolve(obj)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} resolve taking a long time."));
        }

        public virtual Task FinalizeGeneration(ProtocolGeneration proto)
        {
            return SubModules.FinalizeGeneration(proto)
                .TimeoutButContinue(Utility.TimeoutMS, () => System.Console.WriteLine($"{this.Name} finalize taking a long time."));
        }
    }
}
