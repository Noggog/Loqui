using System.Xml.Linq;
using Noggog.StructuredStrings;

namespace Loqui.Generation;

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
    Task GenerateInStaticCtor(ObjectGeneration obj, StructuredStringBuilder sb);
    Task GenerateInClass(ObjectGeneration obj, StructuredStringBuilder sb);
    Task GenerateInField(ObjectGeneration obj, TypeGeneration typeGeneration, StructuredStringBuilder sb, LoquiInterfaceType type);
    Task GenerateInNonGenericClass(ObjectGeneration obj, StructuredStringBuilder sb);
    Task GenerateInCtor(ObjectGeneration obj, StructuredStringBuilder sb);
    Task GenerateInCommon(ObjectGeneration obj, StructuredStringBuilder sb, MaskTypeSet maskTypes);
    Task GenerateInCommonMixin(ObjectGeneration obj, StructuredStringBuilder sb);
    Task GenerateInVoid(ObjectGeneration obj, StructuredStringBuilder sb);
    Task GenerateInInterface(ObjectGeneration obj, StructuredStringBuilder sb, bool internalInterface, bool getter);
    Task GenerateInRegistration(ObjectGeneration obj, StructuredStringBuilder sb);
    Task MiscellaneousGenerationActions(ObjectGeneration obj);
    Task Resolve(ObjectGeneration obj);
    Task PrepareGeneration(ProtocolGeneration proto);
    Task FinalizeGeneration(ProtocolGeneration proto);
    Task FinalizeGeneration(IEnumerable<ProtocolGeneration> proto);
}

public abstract class GenerationModule : IGenerationModule
{
    public virtual string RegionString { get; }
    public GenerationModuleCollection SubModules = new();
    public string Name => RegionString ?? GetType().Name;

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

    public virtual Task GenerateInStaticCtor(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return SubModules.GenerateInStaticCtor(obj, sb);
    }

    public virtual Task GenerateInClass(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return SubModules.GenerateInClass(obj, sb);
    }

    public virtual Task GenerateInNonGenericClass(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return SubModules.GenerateInNonGenericClass(obj, sb);
    }

    public virtual Task GenerateInCtor(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return SubModules.GenerateInCtor(obj, sb);
    }

    public virtual Task GenerateInCommon(ObjectGeneration obj, StructuredStringBuilder sb, MaskTypeSet maskTypes)
    {
        return SubModules.GenerateInCommon(obj, sb, maskTypes);
    }

    public virtual Task GenerateInCommonMixin(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return SubModules.GenerateInCommonMixin(obj, sb);
    }

    public virtual Task GenerateInVoid(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return SubModules.GenerateInVoid(obj, sb);
    }

    public virtual Task GenerateInInterface(ObjectGeneration obj, StructuredStringBuilder sb, bool internalInterface, bool getter)
    {
        return SubModules.GenerateInInterface(obj, sb, internalInterface, getter);
    }

    public virtual Task GenerateInRegistration(ObjectGeneration obj, StructuredStringBuilder sb)
    {
        return SubModules.GenerateInRegistration(obj, sb);
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

    public virtual Task FinalizeGeneration(IEnumerable<ProtocolGeneration> proto)
    {
        return SubModules.FinalizeGeneration(proto);
    }

    public virtual Task GenerateInField(ObjectGeneration obj, TypeGeneration typeGeneration, StructuredStringBuilder sb, LoquiInterfaceType type)
    {
        return SubModules.GenerateInField(obj, typeGeneration, sb, type);
    }
}