using Noggog;
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
        public string Name => "Generation Submodules";

        public GenerationModuleCollection()
        {
        }

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
                    (subGen) =>
                    {
                        return subGen.MiscellaneousGenerationActions(obj);
                    }));
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

        public Task GenerateInNonGenericClass(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInNonGenericClass(obj, fg);
                        }
                    }));
        }

        public Task GenerateInCommon(ObjectGeneration obj, FileGeneration fg, MaskTypeSet maskTypes)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInCommon(obj, fg, maskTypes);
                        }
                    }));
        }

        public Task GenerateInCommonMixin(ObjectGeneration obj, FileGeneration fg)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInCommonMixin(obj, fg);
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

        public Task GenerateInInterface(ObjectGeneration obj, FileGeneration fg, bool internalInterface, bool getter)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInInterface(obj, fg, internalInterface, getter);
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

        public IAsyncEnumerable<(LoquiInterfaceType Location, string Interface)> Interfaces(ObjectGeneration obj)
        {
            return this.subModules.ToAsyncEnumerable().SelectMany((subGen) => subGen.Interfaces(obj));
        }

        public Task Modify(LoquiGenerator gen)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.Modify(gen);
                    }));
        }

        public Task PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.PostFieldLoad(obj, field, node);
                    }));
        }

        public Task LoadWrapup(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.LoadWrapup(obj);
                    }));
        }

        public Task PostLoad(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.PostLoad(obj);
                    }));
        }

        public Task PreLoad(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.PreLoad(obj);
                    }));
        }

        public IAsyncEnumerable<string> RequiredUsingStatements(ObjectGeneration obj)
        {
            return this.subModules.ToAsyncEnumerable().SelectMany((subGen) => subGen.RequiredUsingStatements(obj));
        }

        public Task Resolve(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.Resolve(obj);
                    }));
        }

        public Task FinalizeGeneration(ProtocolGeneration proto)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.FinalizeGeneration(proto);
                    }));
        }

        public Task PrepareGeneration(ProtocolGeneration proto)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.PrepareGeneration(proto);
                    }));
        }

        public Task GenerateInField(ObjectGeneration obj, TypeGeneration typeGeneration, FileGeneration fg, LoquiInterfaceType type)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.GenerateInField(obj, typeGeneration, fg, type);
                    }));
        }
    }
}
