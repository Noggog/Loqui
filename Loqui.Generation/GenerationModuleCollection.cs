using System.Xml.Linq;

namespace Loqui.Generation;

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
        subModules.Add(module);
        return module;
    }

    public Task GenerateInVoid(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return Task.WhenAll(
            subModules.Select(
                async (subGen) =>
                {
                    using (new RegionWrapper(sb, subGen.RegionString))
                    {
                        await subGen.GenerateInVoid(obj, sb);
                    }
                }));
    }

    public Task MiscellaneousGenerationActions(ObjectGeneration obj)
    {
        return Task.WhenAll(
            subModules.Select(
                (subGen) =>
                {
                    return subGen.MiscellaneousGenerationActions(obj);
                }));
    }

    public Task GenerateInClass(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return Task.WhenAll(
            subModules.Select(
                async (subGen) =>
                {
                    using (new RegionWrapper(sb, subGen.RegionString))
                    {
                        await subGen.GenerateInClass(obj, sb);
                    }
                }));
    }

    public Task GenerateInNonGenericClass(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return Task.WhenAll(
            subModules.Select(
                async (subGen) =>
                {
                    using (new RegionWrapper(sb, subGen.RegionString))
                    {
                        await subGen.GenerateInNonGenericClass(obj, sb);
                    }
                }));
    }

    public Task GenerateInCommon(ObjectGeneration obj, StructuredStringBuilder sb, MaskTypeSet maskTypes)
    {
        return Task.WhenAll(
            subModules.Select(
                async (subGen) =>
                {
                    using (new RegionWrapper(sb, subGen.RegionString))
                    {
                        await subGen.GenerateInCommon(obj, sb, maskTypes);
                    }
                }));
    }

    public Task GenerateInCommonMixin(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return Task.WhenAll(
            subModules.Select(
                async (subGen) =>
                {
                    using (new RegionWrapper(sb, subGen.RegionString))
                    {
                        await subGen.GenerateInCommonMixin(obj, sb);
                    }
                }));
    }

    public Task GenerateInCtor(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return Task.WhenAll(
            subModules.Select(
                async (subGen) =>
                {
                    using (new RegionWrapper(sb, subGen.RegionString))
                    {
                        await subGen.GenerateInCtor(obj, sb);
                    }
                }));
    }

    public Task GenerateInInterface(ObjectGeneration obj, StructuredStringBuilder sb, bool internalInterface, bool getter)
    {
        return Task.WhenAll(
            subModules.Select(
                async (subGen) =>
                {
                    using (new RegionWrapper(sb, subGen.RegionString))
                    {
                        await subGen.GenerateInInterface(obj, sb, internalInterface, getter);
                    }
                }));
    }

    public Task GenerateInRegistration(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return Task.WhenAll(
            subModules.Select(
                async (subGen) =>
                {
                    using (new RegionWrapper(sb, subGen.RegionString))
                    {
                        await subGen.GenerateInRegistration(obj, sb);
                    }
                }));
    }

    public Task GenerateInStaticCtor(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return Task.WhenAll(
            subModules.Select(
                async (subGen) =>
                {
                    using (new RegionWrapper(sb, subGen.RegionString))
                    {
                        await subGen.GenerateInStaticCtor(obj, sb);
                    }
                }));
    }

    public IAsyncEnumerable<(LoquiInterfaceType Location, string Interface)> Interfaces(ObjectGeneration obj)
    {
        return subModules.ToAsyncEnumerable().SelectMany((subGen) => subGen.Interfaces(obj));
    }

    public Task Modify(LoquiGenerator gen)
    {
        return Task.WhenAll(
            subModules.Select(
                (subGen) =>
                {
                    return subGen.Modify(gen);
                }));
    }

    public Task PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node)
    {
        return Task.WhenAll(
            subModules.Select(
                (subGen) =>
                {
                    return subGen.PostFieldLoad(obj, field, node);
                }));
    }

    public Task LoadWrapup(ObjectGeneration obj)
    {
        return Task.WhenAll(
            subModules.Select(
                (subGen) =>
                {
                    return subGen.LoadWrapup(obj);
                }));
    }

    public Task PostLoad(ObjectGeneration obj)
    {
        return Task.WhenAll(
            subModules.Select(
                (subGen) =>
                {
                    return subGen.PostLoad(obj);
                }));
    }

    public Task PreLoad(ObjectGeneration obj)
    {
        return Task.WhenAll(
            subModules.Select(
                (subGen) =>
                {
                    return subGen.PreLoad(obj);
                }));
    }

    public IAsyncEnumerable<string> RequiredUsingStatements(ObjectGeneration obj)
    {
        return subModules.ToAsyncEnumerable().SelectMany((subGen) => subGen.RequiredUsingStatements(obj));
    }

    public Task Resolve(ObjectGeneration obj)
    {
        return Task.WhenAll(
            subModules.Select(
                (subGen) =>
                {
                    return subGen.Resolve(obj);
                }));
    }

    public Task FinalizeGeneration(ProtocolGeneration proto)
    {
        return Task.WhenAll(
            subModules.Select(
                (subGen) =>
                {
                    return subGen.FinalizeGeneration(proto);
                }));
    }

    public Task FinalizeGeneration(IEnumerable<ProtocolGeneration> proto)
    {
        return Task.WhenAll(
            subModules.Select(
                (subGen) =>
                {
                    return subGen.FinalizeGeneration(proto);
                }));
    }

    public Task PrepareGeneration(ProtocolGeneration proto)
    {
        return Task.WhenAll(
            subModules.Select(
                (subGen) =>
                {
                    return subGen.PrepareGeneration(proto);
                }));
    }

    public Task GenerateInField(ObjectGeneration obj, TypeGeneration typeGeneration, StructuredStringBuilder sb, LoquiInterfaceType type)
    {
        return Task.WhenAll(
            subModules.Select(
                (subGen) =>
                {
                    return subGen.GenerateInField(obj, typeGeneration, sb, type);
                }));
    }
}