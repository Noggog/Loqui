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
        public int? TimeoutMS;

        public GenerationModuleCollection(int? timeoutMS = 4000)
        {
            this.TimeoutMS = timeoutMS;
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
                            await subGen.GenerateInVoid(obj, fg)
                                .TimeoutButContinue(
                                    TimeoutMS, 
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} generate in void taking a long time."));
                        }
                    }));
        }

        public Task MiscellaneousGenerationActions(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.MiscellaneousGenerationActions(obj)
                            .TimeoutButContinue(
                                TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} {obj.Name} misc actions taking a long time."));
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
                            await subGen.GenerateInClass(obj, fg)
                                .TimeoutButContinue(
                                    TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} gen in class taking a long time."));
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
                            await subGen.GenerateInCommon(obj, fg, maskTypes)
                                .TimeoutButContinue(
                                    TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} gen common taking a long time."));
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
                            await subGen.GenerateInCommonMixin(obj, fg)
                                .TimeoutButContinue(
                                    TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} gen common taking a long time."));
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
                            await subGen.GenerateInCtor(obj, fg)
                                .TimeoutButContinue(
                                    TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} gen ctor taking a long time."));
                        }
                    }));
        }

        public Task GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg, bool internalInterface)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    async (subGen) =>
                    {
                        using (new RegionWrapper(fg, subGen.RegionString))
                        {
                            await subGen.GenerateInInterfaceGetter(obj, fg, internalInterface)
                                .TimeoutButContinue(
                                    TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} gen interface getter taking a long time."));
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
                            await subGen.GenerateInRegistration(obj, fg)
                                .TimeoutButContinue(
                                    TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} gen in registration taking a long time."));
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
                            await subGen.GenerateInStaticCtor(obj, fg)
                                .TimeoutButContinue(
                                    TimeoutMS,
                                    () => System.Console.WriteLine($"{subGen} {obj.Name} static ctor taking a long time."));
                        }
                    }));
        }

        public async Task<IEnumerable<(LoquiInterfaceType Location, string Interface)>> Interfaces(ObjectGeneration obj)
        {
            return (await Task.WhenAll(this.subModules.Select((subGen) => subGen.Interfaces(obj))))
                .SelectMany(i => i);
        }

        public Task Modify(LoquiGenerator gen)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.Modify(gen)
                            .TimeoutButContinue(
                                TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} modify taking a long time."));
                    }));
        }

        public Task PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.PostFieldLoad(obj, field, node)
                            .TimeoutButContinue(
                                TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} {obj.Name}.{field.Name} post field load taking a long time."));
                    }));
        }

        public Task PostLoad(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.PostLoad(obj)
                            .TimeoutButContinue(
                                TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} {obj.Name} post load taking a long time."));
                    }));
        }

        public Task PreLoad(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.PreLoad(obj)
                            .TimeoutButContinue(
                                TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} {obj.Name} pre load taking a long time."));
                    }));
        }

        public async Task<IEnumerable<string>> RequiredUsingStatements(ObjectGeneration obj)
        {
            return (await Task.WhenAll(this.subModules.Select((subGen) => subGen.RequiredUsingStatements(obj))))
                .SelectMany(i => i);
        }

        public Task Resolve(ObjectGeneration obj)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.Resolve(obj)
                            .TimeoutButContinue(
                                TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} {obj.Name} resolve taking a long time."));
                    }));
        }

        public Task FinalizeGeneration(ProtocolGeneration proto)
        {
            return Task.WhenAll(
                this.subModules.Select(
                    (subGen) =>
                    {
                        return subGen.FinalizeGeneration(proto)
                            .TimeoutButContinue(
                                TimeoutMS,
                                () => System.Console.WriteLine($"{subGen} finalize taking a long time."));
                    }));
        }
    }
}
