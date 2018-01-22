using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class GenerationModuleCollection : IGenerationModule
    {
        private List<IGenerationModule> subModules = new List<IGenerationModule>();

        public string RegionString => null;

        public M Add<M>(M module)
            where M : IGenerationModule
        {
            this.subModules.Add(module);
            return module;
        }

        public Task GenerateInVoid(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInVoid(obj, fg);
                        }
                    }));
        }

        public Task MiscellaneousGenerationActions(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) => subGen.MiscellaneousGenerationActions(obj)));
        }

        public Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInClass(obj, fg);
                        }
                    }));
        }

        public Task GenerateInCommonExt(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInCommonExt(obj, fg);
                        }
                    }));
        }

        public Task GenerateInCtor(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInCtor(obj, fg);
                        }
                    }));
        }

        public Task GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInInterfaceGetter(obj, fg);
                        }
                    }));
        }

        public Task GenerateInRegistration(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInRegistration(obj, fg);
                        }
                    }));
        }

        public Task GenerateInStaticCtor(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInStaticCtor(obj, fg);
                        }
                    }));
        }

        public IEnumerable<string> GetReaderInterfaces(ObjectGeneration obj)
        {
            return this.subModules.SelectMany((subGen) => subGen.GetReaderInterfaces(obj));
        }

        public IEnumerable<string> GetWriterInterfaces(ObjectGeneration obj)
        {
            return this.subModules.SelectMany((subGen) => subGen.GetWriterInterfaces(obj));
        }

        public IEnumerable<string> Interfaces(ObjectGeneration obj)
        {
            return this.subModules.SelectMany((subGen) => subGen.Interfaces(obj));
        }

        public Task Modify(LoquiGenerator gen)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) => subGen.Modify(gen)));
        }

        public Task PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) => subGen.PostFieldLoad(obj, field, node)));
        }

        public Task PostLoad(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) => subGen.PostLoad(obj)));
        }

        public Task PreLoad(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) => subGen.PreLoad(obj)));
        }

        public IEnumerable<string> RequiredUsingStatements(ObjectGeneration obj)
        {
            return this.subModules.SelectMany((subGen) => subGen.RequiredUsingStatements(obj));
        }

        public Task Resolve(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) => subGen.Resolve(obj)));
        }

        public Task FinalizeGeneration(ProtocolGeneration proto)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) => subGen.FinalizeGeneration(proto)));
        }
    }
}
