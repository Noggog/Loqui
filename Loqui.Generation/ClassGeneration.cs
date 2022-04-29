using Noggog;

namespace Loqui.Generation;

public class ClassGeneration : ObjectGeneration
{
    private bool _abstract;
    public override bool Abstract => _abstract;
    private bool _nullableDefault;
    public override bool NullableDefault => _nullableDefault;
    public string BaseClassStr { get; set; }
    private List<ClassGeneration> _derivativeClasses = new List<ClassGeneration>();
    public bool HasDerivativeClasses => _derivativeClasses.Count > 0;

    public ClassGeneration(LoquiGenerator gen, ProtocolGeneration protoGen, FilePath sourceFile)
        : base(gen, protoGen, sourceFile)
    {
    }

    public override string NewOverride(Func<ObjectGeneration, bool> baseObjFilter = null, bool doIt = true)
    {
        if (!doIt || !HasLoquiBaseObject) return " ";
        if (baseObjFilter == null) return " new ";
        foreach (var baseClass in BaseClassTrail())
        {
            if (baseObjFilter(baseClass)) return " new ";
        }
        return " ";
    }

    public override async Task Load()
    {
        BaseClassStr = Node.GetAttribute("baseClass");
        _abstract = Node.GetAttribute<bool>("abstract", false);
        _nullableDefault = Node.GetAttribute<bool>("nullableDefault", ProtoGen.NullableDefault);

        if (NeedsReflectionGeneration)
        {
            Interfaces.Add(LoquiInterfaceDefinitionType.ISetter, nameof(ILoquiReflectionSetter));
        }

        if (ObjectNamedKey.TryFactory(BaseClassStr, ProtoGen.Protocol, out var baseClassObjKey))
        {
            if (!gen.ObjectGenerationsByObjectNameKey.TryGetValue(baseClassObjKey, out ObjectGeneration baseObj)
                || !(baseObj is ClassGeneration))
            {
                throw new ArgumentException($"Could not resolve base class object: {BaseClassStr}");
            }
            else
            {
                ClassGeneration baseClass = baseObj as ClassGeneration;
                BaseClass = baseClass;
                baseClass._derivativeClasses.Add(this);
            }
        }
#if NETSTANDARD2_0
        WiredBaseClassTCS.TrySetResult(true);
#else
            WiredBaseClassTCS.TrySetResult();
#endif

        await base.Load();
    }

    protected override async Task GenerateClassLine(StructuredStringBuilder sb)
    {
        using (var args = sb.Class(ObjectName))
        {
            args.Abstract = Abstract;
            args.Partial = true;
            if (HasLoquiBaseObject && HasNonLoquiBaseObject)
            {
                throw new ArgumentException("Cannot define both a loqui and non-loqui base class");
            }
            if (HasLoquiBaseObject)
            {
                args.BaseClass = BaseClassName;
            }
            else if (HasNonLoquiBaseObject && SetBaseClass)
            {
                args.BaseClass = NonLoquiBaseClass;
            }
            args.Interfaces.Add(Interface(getter: false, internalInterface: true));
            args.Interfaces.Add($"ILoquiObjectSetter<{ObjectName}>");
            args.Interfaces.Add(Interfaces.Get(LoquiInterfaceType.Direct));
            args.Interfaces.Add(await GetApplicableInterfaces(LoquiInterfaceType.Direct).ToListAsync());
            args.Interfaces.Add(ProtoGen.Interfaces);
            args.Interfaces.Add(gen.Interfaces);
            args.Interfaces.Add($"IEquatable<{Interface(getter: true, internalInterface: true)}>");
        }
    }
        
    protected override async Task GenerateCtor(StructuredStringBuilder sb)
    {
        if (BasicCtorPermission == CtorPermissionLevel.noGeneration) return;
        using (sb.Region("Ctor"))
        {
            sb.AppendLine($"{BasicCtorPermission.ToStringFast_Enum_Only()} {Name}()");
            using (sb.CurlyBrace())
            {
                List<Task> toDo = new List<Task>();
                toDo.AddRange(gen.GenerationModules.Select(mod => mod.GenerateInCtor(this, sb)));
                var fieldsTask = Task.WhenAll(IterateFields().Select(field => field.GenerateForCtor(sb)));
                toDo.Add(fieldsTask);
                await fieldsTask;
                fieldCtorsGenerated.SetResult();
                await Task.WhenAll(toDo);
                await GenerateInitializer(sb);
                sb.AppendLine("CustomCtor();");
            }
            sb.AppendLine("partial void CustomCtor();");
        }
    }

    public override async Task<OverrideType> GetFunctionOverrideType(Func<ClassGeneration, Task<bool>> tester = null)
    {
        if (HasLoquiBaseObject)
        {
            foreach (var baseObj in BaseClassTrail())
            {
                if (tester == null || await tester(baseObj))
                {
                    return OverrideType.HasBase;
                }
            }
        }
        if (HasDerivativeClasses)
        {
            foreach (var derivClass in GetDerivativeClasses())
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
        if (HasLoquiBaseObject)
        {
            foreach (var baseObj in BaseClassTrail())
            {
                return OverrideType.HasBase;
            }
        }
        if (HasDerivativeClasses)
        {
            foreach (var derivClass in GetDerivativeClasses())
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
        if (HasDerivativeClasses) return " virtual "; 
        return " ";
    }
}