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
        Task PrepareGeneration(ProtocolGeneration proto);
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
            return SubModules.PreLoad(obj);
        }

        public virtual Task LoadWrapup(ObjectGeneration obj)
        {
            return SubModules.LoadWrapup(obj);
        }

        public virtual Task PostLoad(ObjectGeneration obj)
        {
            return SubModules.PostLoad(obj);
        }

        public virtual Task PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node)
        {
            return SubModules.PostFieldLoad(obj, field, node);
        }

        public virtual Task Modify(LoquiGenerator gen)
        {
            return SubModules.Modify(gen);
        }

        public virtual Task GenerateInStaticCtor(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInStaticCtor(obj, fg);
        }

        public virtual Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInClass(obj, fg);
        }

        public virtual Task GenerateInNonGenericClass(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInNonGenericClass(obj, fg);
        }

        public virtual Task GenerateInCtor(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInCtor(obj, fg);
        }

        public virtual Task GenerateInCommon(ObjectGeneration obj, FileGeneration fg, MaskTypeSet maskTypes)
        {
            return SubModules.GenerateInCommon(obj, fg, maskTypes);
        }

        public virtual Task GenerateInCommonMixin(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInCommonMixin(obj, fg);
        }

        public virtual Task GenerateInVoid(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInVoid(obj, fg);
        }

        public virtual Task GenerateInInterface(ObjectGeneration obj, FileGeneration fg, bool internalInterface, bool getter)
        {
            return SubModules.GenerateInInterface(obj, fg, internalInterface, getter);
        }

        public virtual Task GenerateInRegistration(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInRegistration(obj, fg);
        }

        public virtual Task MiscellaneousGenerationActions(ObjectGeneration obj)
        {
            return SubModules.MiscellaneousGenerationActions(obj);
        }

        public virtual Task Resolve(ObjectGeneration obj)
        {
            return SubModules.Resolve(obj);
        }

        public virtual Task FinalizeGeneration(ProtocolGeneration proto)
        {
            return SubModules.FinalizeGeneration(proto);
        }

        public virtual Task PrepareGeneration(ProtocolGeneration proto)
        {
            return SubModules.PrepareGeneration(proto);
        }
    }
}
