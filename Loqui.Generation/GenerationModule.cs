using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public interface IGenerationModule
    {
        string RegionString { get; }
        IEnumerable<string> RequiredUsingStatements(ObjectGeneration obj);
        IEnumerable<string> Interfaces(ObjectGeneration obj);
        IEnumerable<string> GetWriterInterfaces(ObjectGeneration obj);
        IEnumerable<string> GetReaderInterfaces(ObjectGeneration obj);
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

        public virtual IEnumerable<string> RequiredUsingStatements(ObjectGeneration obj)
        {
            return SubModules.RequiredUsingStatements(obj);
        }

        public virtual IEnumerable<string> Interfaces(ObjectGeneration obj)
        {
            return SubModules.Interfaces(obj);
        }

        public virtual IEnumerable<string> GetWriterInterfaces(ObjectGeneration obj)
        {
            return SubModules.GetWriterInterfaces(obj);
        }

        public virtual IEnumerable<string> GetReaderInterfaces(ObjectGeneration obj)
        {
            return SubModules.GetReaderInterfaces(obj);
        }

        public virtual Task PreLoad(ObjectGeneration obj)
        {
            return SubModules.PreLoad(obj);
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

        public virtual Task GenerateInCtor(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInCtor(obj, fg);
        }

        public virtual Task GenerateInCommonExt(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInCommonExt(obj, fg);
        }

        public virtual Task GenerateInVoid(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInVoid(obj, fg);
        }

        public virtual Task GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg)
        {
            return SubModules.GenerateInInterfaceGetter(obj, fg);
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
    }
}
