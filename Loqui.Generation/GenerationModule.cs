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
        Task<IEnumerable<string>> RequiredUsingStatements(ObjectGeneration obj);
        Task<IEnumerable<string>> Interfaces(ObjectGeneration obj);
        Task<IEnumerable<string>> GetWriterInterfaces(ObjectGeneration obj);
        Task<IEnumerable<string>> GetReaderInterfaces(ObjectGeneration obj);
        Task PreLoad(ObjectGeneration obj);
        Task PostLoad(ObjectGeneration obj);
        Task PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node);
        Task Modify(LoquiGenerator gen);
        Task GenerateInStaticCtor(ObjectGeneration obj, FileGeneration fg);
        Task GenerateInClass(ObjectGeneration obj, FileGeneration fg);
        Task GenerateInCtor(ObjectGeneration obj, FileGeneration fg);
        Task GenerateInCommonExt(ObjectGeneration obj, FileGeneration fg);
        Task GenerateInVoid(ObjectGeneration obj, FileGeneration fg);
        Task GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg);
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
        public int? TimeoutMS = 4000;

        public virtual Task<IEnumerable<string>> RequiredUsingStatements(ObjectGeneration obj)
        {
            return SubModules.RequiredUsingStatements(obj);
        }

        public virtual Task<IEnumerable<string>> Interfaces(ObjectGeneration obj)
        {
            return SubModules.Interfaces(obj);
        }

        public virtual Task<IEnumerable<string>> GetWriterInterfaces(ObjectGeneration obj)
        {
            return SubModules.GetWriterInterfaces(obj);
        }

        public virtual Task<IEnumerable<string>> GetReaderInterfaces(ObjectGeneration obj)
        {
            return SubModules.GetReaderInterfaces(obj);
        }

        public virtual Task PreLoad(ObjectGeneration obj)
        {
            return SubModules.PreLoad(obj)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} pre load taking a long time."));
        }

        public virtual Task PostLoad(ObjectGeneration obj)
        {
            return SubModules.PostLoad(obj)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} post load taking a long time."));
        }

        public virtual Task PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node)
        {
            return SubModules.PostFieldLoad(obj, field, node)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name}.{field.Name} post load taking a long time."));
        }

        public virtual Task Modify(LoquiGenerator gen)
        {
            return SubModules.Modify(gen)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} modify taking a long time."));
        }

        public virtual Task GenerateInStaticCtor(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInStaticCtor(obj, fg)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in static ctor taking a long time."));
        }

        public virtual Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInClass(obj, fg)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in class taking a long time."));
        }

        public virtual Task GenerateInCtor(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInCtor(obj, fg)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in ctor taking a long time."));
        }

        public virtual Task GenerateInCommonExt(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInCommonExt(obj, fg)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in common extensions taking a long time."));
        }

        public virtual Task GenerateInVoid(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInVoid(obj, fg)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in void taking a long time."));
        }

        public virtual Task GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInInterfaceGetter(obj, fg)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in interface getter taking a long time."));
        }

        public virtual Task GenerateInRegistration(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInRegistration(obj, fg)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate in registration taking a long time."));
        }

        public virtual Task MiscellaneousGenerationActions(ObjectGeneration obj)
        {
            return SubModules.MiscellaneousGenerationActions(obj)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} generate misc actions taking a long time."));
        }

        public virtual Task Resolve(ObjectGeneration obj)
        {
            return SubModules.Resolve(obj)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} {obj.Name} resolve taking a long time."));
        }

        public virtual Task FinalizeGeneration(ProtocolGeneration proto)
        {
            return SubModules.FinalizeGeneration(proto)
                .TimeoutButContinue(TimeoutMS, () => System.Console.WriteLine($"{this.Name} finalize taking a long time."));
        }
    }
}
