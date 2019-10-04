using Loqui.Internal;
using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class TranslationModule<G> : GenerationModule, ITranslationModule
        where G : TranslationGeneration
    {
        public LoquiGenerator Gen;
        public abstract string ModuleNickname { get; }
        public override string RegionString => $"{ModuleNickname} Translation";
        public abstract string Namespace { get; }
        public abstract bool GenerateAbstractCreates { get; }
        public IEnumerable<TranslationModuleAPI> AllAPI => MainAPI.And(MinorAPIs);
        public TranslationModuleAPI MainAPI;
        protected List<TranslationModuleAPI> MinorAPIs = new List<TranslationModuleAPI>();
        public bool ShouldGenerateCopyIn = true;
        public bool TranslationMaskParameter = true;
        public bool DoErrorMasks = true;
        public string TranslationWriteClassName(ObjectGeneration obj) => $"{obj.Name}{ModuleNickname}WriteTranslation";
        public string TranslationWriteClass(ObjectGeneration obj) => $"{TranslationWriteClassName(obj)}";
        public string TranslationCreateClassName(ObjectGeneration obj) => $"{obj.Name}{ModuleNickname}CreateTranslation";
        public string TranslationCreateClass(ObjectGeneration obj) => $"{TranslationCreateClassName(obj)}{obj.GetGenericTypes(MaskType.Normal)}";
        public string TranslationMixInClass(ObjectGeneration obj) => $"{obj.Name}{ModuleNickname}TranslationMixIn";
        public virtual bool DoTranslationInterface(ObjectGeneration obj) => true;
        public virtual bool DirectTranslationReference(ObjectGeneration obj) => false;
        public string TranslationWriteInterface => $"I{this.ModuleNickname}WriteTranslator";
        public string TranslationItemInterface => $"I{this.ModuleNickname}Item";
        public string TranslationWriteItemMember => $"{this.ModuleNickname}WriteTranslator";
        public virtual string TranslatorReference(ObjectGeneration obj, Accessor item) => $"(({this.TranslationWriteClass(obj)}){item}.{this.TranslationWriteItemMember})";

        public const string ErrorMaskKey = "ErrorMask";
        public const string ErrorMaskBuilderKey = "ErrorMaskBuilder";
        public const string DoMaskKey = "DoMasks";
        public const string TranslationMaskKey = "TranslationMask";

        public virtual string CreateFromPrefix => "CreateFrom";
        public virtual string WriteToPrefix => "WriteTo";

        protected Dictionary<Type, G> _typeGenerations = new Dictionary<Type, G>();

        public List<Func<ObjectGeneration, FileGeneration, Task>> ExtraTranslationTasks = new List<Func<ObjectGeneration, FileGeneration, Task>>();

        public TranslationModule(LoquiGenerator gen)
        {
            this.Gen = gen;
        }

        public override async Task PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node)
        {
            if (!_typeGenerations.TryGetValue(field.GetType(), out var transl)) return;
            transl.Load(obj, field, node);
        }

        public override async Task<IEnumerable<(LoquiInterfaceType Location, string Interface)>> Interfaces(ObjectGeneration obj)
        {
            if (this.DoTranslationInterface(obj))
            {
                return (LoquiInterfaceType.IGetter, TranslationItemInterface).Single().ToArray();
            }
            else
            {
                return EnumerableExt<(LoquiInterfaceType Location, string Interface)>.Empty.ToArray();
            }
        }

        public override async Task<IEnumerable<string>> RequiredUsingStatements(ObjectGeneration obj)
        {
            return "System.Diagnostics".Single();
        }

        public override async Task Modify(LoquiGenerator gen)
        {
        }

        public override async Task GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg, bool internalInterface)
        {
        }

        public override async Task GenerateInVoid(ObjectGeneration obj, FileGeneration fg)
        {
            using (new NamespaceWrapper(fg, obj.InternalNamespace))
            {
                using (var args = new ClassWrapper(fg, TranslationWriteClass(obj)))
                {
                    args.Partial = true;
                    args.BaseClass = obj.HasLoquiBaseObject ? TranslationWriteClass(obj.BaseClass) : null;
                    if (this.DoTranslationInterface(obj))
                    {
                        args.Interfaces.Add(this.TranslationWriteInterface);
                    }
                }
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"public{obj.NewOverride()}readonly static {TranslationWriteClass(obj)} Instance = new {TranslationWriteClass(obj)}();");
                    fg.AppendLine();

                    await GenerateInTranslationWriteClass(obj, fg);
                }
                fg.AppendLine();

                using (var args = new ClassWrapper(fg, TranslationCreateClass(obj)))
                {
                    args.Partial = true;
                    args.BaseClass = obj.HasLoquiBaseObject ? TranslationCreateClass(obj.BaseClass) : null;
                    args.Wheres.AddRange(obj.GenerateWhereClauses(LoquiInterfaceType.ISetter, obj.Generics));
                }
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"public{obj.NewOverride()}readonly static {TranslationCreateClass(obj)} Instance = new {TranslationCreateClass(obj)}();");
                    fg.AppendLine();

                    await GenerateInTranslationCreateClass(obj, fg);
                }
                fg.AppendLine();
            }

            using (new NamespaceWrapper(fg, obj.Namespace))
            {
                using (new RegionWrapper(fg, $"{this.ModuleNickname} Write Mixins"))
                {
                    using (var args = new ClassWrapper(fg, TranslationMixInClass(obj)))
                    {
                        args.Static = true;
                    }
                    using (new BraceWrapper(fg))
                    {
                        await GenerateWriteMixIn(obj, fg);
                    }
                }
                fg.AppendLine();
            }
        }

        public virtual async Task GenerateInTranslationWriteClass(ObjectGeneration obj, FileGeneration fg)
        {
            await TranslationWrite(obj, fg);
            foreach (var extra in ExtraTranslationTasks)
            {
                await extra(obj, fg);
            }
        }

        public virtual async Task GenerateInTranslationCreateClass(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public override async Task MiscellaneousGenerationActions(ObjectGeneration obj)
        {
        }

        private async Task<string> ErrorLabel(ObjectGeneration obj) => await this.AsyncImport(obj) ? "WithErrorMask" : null;

        public override async Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            if (this.MainAPI == null)
            {
                throw new ArgumentException("Main API need to be set.");
            }
            if (this.DoTranslationInterface(obj))
            {
                await GenerateTranslationInterfaceImplementation(obj, fg);
            }
            if (!obj.Abstract || this.GenerateAbstractCreates)
            {
                await GenerateCreate(obj, fg);
            }
            if (ShouldGenerateCopyIn)
            {
                await GenerateCopyIn(obj, fg);
            }
        }

        public async Task GenerateTranslationInterfaceImplementation(ObjectGeneration obj, FileGeneration fg)
        {
            fg.AppendLine($"protected{await obj.FunctionOverride(async c => this.DoTranslationInterface(c))}object {this.TranslationWriteItemMember} => {this.TranslationWriteClass(obj)}.Instance;");
            if (!obj.BaseClassTrail().Any(b => this.DoTranslationInterface(b)))
            {
                fg.AppendLine($"object {this.TranslationItemInterface}.{this.TranslationWriteItemMember} => this.{this.TranslationWriteItemMember};");
            }
            using (var args = new FunctionWrapper(fg,
                $"void {this.TranslationItemInterface}.WriteTo{this.ModuleNickname}"))
            {
                FillWriterArgs(args, obj, objParam: null);
            }
            using (new BraceWrapper(fg))
            {
                using (var args = new ArgsWrapper(fg,
                    $"{this.TranslatorReference(obj, "this")}.Write"))
                {
                    args.Add("item: this");
                    foreach (var item in this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Writer))
                    {
                        args.Add(item);
                    }
                    foreach (var item in this.MainAPI.InternalFallbackArgs(obj, TranslationModuleAPI.Direction.Writer))
                    {
                        args.Add(item);
                    }
                    args.AddPassArg($"errorMask");
                    if (this.TranslationMaskParameter)
                    {
                        args.AddPassArg("translationMask");
                    }
                }
            }
        }

        private async Task GenerateCopyIn(ObjectGeneration obj, FileGeneration fg)
        {
            if (obj is StructGeneration) return;
            if (this.MainAPI == null) return;

            using (new RegionWrapper(fg, $"{this.ModuleNickname} Copy In"))
            {
                if (obj.CanAssume())
                {
                    using (var args = new FunctionWrapper(fg,
                        $"public{obj.FunctionOverride()}void CopyIn{ModuleNickname}"))
                    {
                        foreach (var (API, Public) in this.MainAPI.ReaderAPI.IterateAPI(obj))
                        {
                            if (Public)
                            {
                                args.Add(API.Result);
                            }
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        using (var args = new ArgsWrapper(fg,
                            $"CopyIn{ModuleNickname}Internal"))
                        {
                            args.Add(this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Reader));
                            args.Add($"errorMask: null");
                            args.Add($"translationMask: null");
                        }
                    }
                    fg.AppendLine();
                }

                using (var args = new FunctionWrapper(fg, $"public virtual void CopyIn{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}"))
                {
                    args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.ISetter, maskTypes: GetMaskTypes(MaskType.Error)));
                    foreach (var item in this.MainAPI.ReaderAPI.MajorAPI)
                    {
                        if (item.TryResolve(obj, out var line))
                        {
                            args.Add(line.Result);
                        }
                    }
                    args.Add($"out {obj.Mask(MaskType.Error)} errorMask");
                    if (this.TranslationMaskParameter)
                    {
                        args.Add(GetTranslationMaskParameter().Resolver(obj).Result);
                    }
                    foreach (var item in this.MainAPI.ReaderAPI.OptionalAPI)
                    {
                        if (item.TryResolve(obj, out var line))
                        {
                            args.Add(line.Result);
                        }
                    }
                    args.Add("bool doMasks = true");
                }
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("ErrorMaskBuilder errorMaskBuilder = doMasks ? new ErrorMaskBuilder() : null;");
                    using (var args = new ArgsWrapper(fg,
                        $"CopyIn{ModuleNickname}Internal"))
                    {
                        args.Add(this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Reader));
                        args.Add($"errorMask: errorMaskBuilder");
                        args.Add($"translationMask: translationMask?.GetCrystal()");
                    }
                    fg.AppendLine($"errorMask = {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder);");
                }
                fg.AppendLine();

                using (var args = new FunctionWrapper(fg,
                    $"protected{obj.FunctionOverride()}void CopyIn{ModuleNickname}Internal"))
                {
                    foreach (var item in this.MainAPI.ReaderAPI.MajorAPI)
                    {
                        if (item.TryResolve(obj, out var line))
                        {
                            args.Add(line.Result);
                        }
                    }
                    args.Add($"ErrorMaskBuilder errorMask");
                    if (this.TranslationMaskParameter)
                    {
                        args.Add($"{nameof(TranslationCrystal)} translationMask");
                    }
                    foreach (var item in this.MainAPI.ReaderAPI.OptionalAPI)
                    {
                        if (item.TryResolve(obj, out var line))
                        {
                            args.Add(line.Result);
                        }
                    }
                }
                using (new BraceWrapper(fg))
                {
                    // ToDo
                    // Actually implement a copy in, without needing to create an object
                    using (var args = new ArgsWrapper(fg,
                        $"var obj = {obj.ObjectName}.{CreateFromPrefix}{ModuleNickname}"))
                    {
                        foreach (var item in this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Reader))
                        {
                            args.Add(item);
                        }
                        args.AddPassArg($"errorMask");
                        if (this.TranslationMaskParameter)
                        {
                            args.AddPassArg($"translationMask");
                        }
                    }
                    fg.AppendLine("this.CopyFieldsFrom(obj);");
                }
                fg.AppendLine();

                foreach (var minorAPI in this.MinorAPIs)
                {
                    if (!minorAPI.When?.Invoke(obj) ?? false) continue;
                    if (obj.CanAssume())
                    {
                        using (var args = new FunctionWrapper(fg,
                            $"public void CopyIn{ModuleNickname}"))
                        {
                            foreach (var (API, Public) in minorAPI.ReaderAPI.IterateAPI(obj))
                            {
                                if (Public)
                                {
                                    args.Add(API.Result);
                                }
                            }
                        }
                        using (new BraceWrapper(fg))
                        {
                            minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                            {
                                using (var args = new ArgsWrapper(fg,
                                    $"this.CopyIn{ModuleNickname}"))
                                {
                                    foreach (var item in this.MainAPI.WrapAccessors(obj, TranslationModuleAPI.Direction.Reader, accessor))
                                    {
                                        args.Add(item);
                                    }
                                }
                            });
                        }
                        fg.AppendLine();
                    }

                    using (var args = new FunctionWrapper(fg, $"public void CopyIn{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}"))
                    {
                        args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.ISetter, maskTypes: GetMaskTypes(MaskType.Error)));
                        foreach (var item in minorAPI.ReaderAPI.MajorAPI)
                        {
                            if (item.TryResolve(obj, out var line))
                            {
                                args.Add(line.Result);
                            }
                        }
                        args.Add($"out {obj.Mask(MaskType.Error)} errorMask");
                        if (this.TranslationMaskParameter)
                        {
                            args.Add($"{obj.Mask(MaskType.Translation)} translationMask");
                        }
                        foreach (var item in minorAPI.ReaderAPI.OptionalAPI)
                        {
                            if (item.TryResolve(obj, out var line))
                            {
                                args.Add(line.Result);
                            }
                        }
                        args.Add("bool doMasks = true");
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"this.CopyIn{ModuleNickname}"))
                            using (new DepthWrapper(fg))
                            {
                                foreach (var item in this.MainAPI.WrapAccessors(obj, TranslationModuleAPI.Direction.Reader, accessor))
                                {
                                    args.Add(item);
                                }
                                args.Add($"errorMask: out errorMask");
                                args.Add($"translationMask: translationMask");
                                args.Add($"doMasks: doMasks");
                            }
                        });
                    }
                    fg.AppendLine();
                }

                foreach (var baseClass in obj.BaseClassTrail())
                {
                    using (var args = new FunctionWrapper(fg,
                        $"public override void CopyIn{ModuleNickname}{obj.GetBaseMask_GenericTypes(MaskType.Error)}"))
                    {
                        foreach (var item in this.MainAPI.ReaderAPI.MajorAPI)
                        {
                            if (item.TryResolve(obj, out var line))
                            {
                                args.Add(line.Result);
                            }
                        }
                        args.Add($"out {baseClass.Mask(MaskType.Error)} errorMask");
                        if (this.TranslationMaskParameter)
                        {
                            args.Add(
                                GetTranslationMaskParameter().Resolver(baseClass).Result);
                        }
                        foreach (var item in this.MainAPI.ReaderAPI.OptionalAPI)
                        {
                            if (item.TryResolve(obj, out var line))
                            {
                                args.Add(line.Result);
                            }
                        }
                        args.Add("bool doMasks = true");
                    }
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine("ErrorMaskBuilder errorMaskBuilder = doMasks ? new ErrorMaskBuilder() : null;");
                        using (var args = new ArgsWrapper(fg,
                            $"CopyIn{ModuleNickname}Internal"))
                        {
                            args.Add(this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Reader));
                            args.Add($"errorMask: errorMaskBuilder");
                            args.Add($"translationMask: translationMask?.GetCrystal()");
                        }
                        fg.AppendLine($"errorMask = {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder);");
                    }
                    fg.AppendLine();
                }
            }
        }

        protected abstract Task GenerateCreateSnippet(ObjectGeneration obj, FileGeneration fg);

        public MaskType[] GetMaskTypes(params MaskType[] otherMasks)
        {
            if (this.TranslationMaskParameter)
            {
                return otherMasks.And(MaskType.Translation).ToArray();
            }
            else
            {
                return otherMasks;
            }
        }

        public APILine GetTranslationMaskParameter(bool nullable = true)
        {
            return new APILine(
                nicknameKey: TranslationMaskKey,
                when: (obj) => this.TranslationMaskParameter,
                resolver: obj => $"{obj.Mask(MaskType.Translation)} translationMask{(nullable ? " = null" : null)}");
        }

        public virtual async Task<bool> AsyncImport(ObjectGeneration obj)
        {
            if (obj.HasLoquiBaseObject)
            {
                if (await AsyncImport(obj.BaseClass)) return true;
            }
            foreach (var field in obj.Fields)
            {
                if (!this.TryGetTypeGeneration(field.GetType(), out var gen)) continue;
                if (gen.IsAsync(field, read: true)) return true;
            }
            return false;
        }

        public async Task<string> ObjectReturn(ObjectGeneration obj, bool maskReturn, bool hasReturn = true)
        {
            if (await this.AsyncImport(obj))
            {
                if (maskReturn)
                {
                    if (hasReturn)
                    {
                        return Utility.TaskWrap($"({obj.ObjectName} Object, {obj.Mask(MaskType.Error)} ErrorMask)");
                    }
                    else
                    {
                        return Utility.TaskWrap($"{obj.Mask(MaskType.Error)}");
                    }
                }
                else if (hasReturn)
                {
                    return Utility.TaskWrap(obj.ObjectName);
                }
                else
                {
                    return "async Task";
                }
            }
            else if (hasReturn)
            {
                return obj.ObjectName;
            }
            else
            {
                return "void";
            }
        }

        protected virtual bool GenerateMainCreate(ObjectGeneration obj) => true;

        private async Task GenerateCreate(ObjectGeneration obj, FileGeneration fg)
        {
            using (new RegionWrapper(fg, $"{this.ModuleNickname} Create"))
            {
                var asyncImport = await this.AsyncImport(obj);
                var errorLabel = await ErrorLabel(obj);
                if (obj.CanAssume())
                {
                    fg.AppendLine("[DebuggerStepThrough]");
                    using (var args = new FunctionWrapper(fg,
                        $"public static {await this.ObjectReturn(obj, maskReturn: false)} {CreateFromPrefix}{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes())}"))
                    {
                        args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: GetMaskTypes()));
                        foreach (var (API, Public) in this.MainAPI.ReaderAPI.IterateAPI(obj))
                        {
                            if (Public)
                            {
                                args.Add(API.Result);
                            }
                        }
                        if (this.TranslationMaskParameter)
                        {
                            args.Add(GetTranslationMaskParameter().Resolver(obj).Result);
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        using (var args = new ArgsWrapper(fg,
                            $"return {Utility.Await(asyncImport)}{CreateFromPrefix}{ModuleNickname}",
                            suffixLine: Utility.ConfigAwait(asyncImport)))
                        {
                            args.Add(this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Reader));
                            foreach (var customArgs in this.MainAPI.InternalFallbackArgs(obj, TranslationModuleAPI.Direction.Reader))
                            {
                                args.Add(customArgs);
                            }
                            args.Add("errorMask: null");
                            if (this.TranslationMaskParameter)
                            {
                                args.Add($"translationMask: translationMask?.GetCrystal()");
                            }
                        }
                    }
                    fg.AppendLine();
                }

                fg.AppendLine("[DebuggerStepThrough]");
                using (var args = new FunctionWrapper(fg,
                    $"public static {await this.ObjectReturn(obj, maskReturn: true)} {CreateFromPrefix}{ModuleNickname}{errorLabel}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}"))
                {
                    args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: GetMaskTypes(MaskType.Error)));
                    foreach (var (API, Public) in this.MainAPI.ReaderAPI.IterateAPI(
                        obj,
                        new APILine(ErrorMaskKey, resolver: (o) => $"out {o.Mask(MaskType.Error)} errorMask", when: (o) => !asyncImport),
                        new APILine(DoMaskKey, $"bool doMasks = true"),
                        GetTranslationMaskParameter()))
                    {
                        if (Public)
                        {
                            args.Add(API.Result);
                        }
                    }
                }
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("ErrorMaskBuilder errorMaskBuilder = doMasks ? new ErrorMaskBuilder() : null;");
                    using (var args = new ArgsWrapper(fg,
                        $"var ret = {Utility.Await(asyncImport)}{CreateFromPrefix}{ModuleNickname}",
                        suffixLine: Utility.ConfigAwait(asyncImport)))
                    {
                        args.Add(this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Reader));
                        foreach (var customArgs in this.MainAPI.InternalFallbackArgs(obj, TranslationModuleAPI.Direction.Reader))
                        {
                            args.Add(customArgs);
                        }
                        args.Add("errorMask: errorMaskBuilder");
                        if (this.TranslationMaskParameter)
                        {
                            args.Add("translationMask: translationMask.GetCrystal()");
                        }
                    }
                    if (asyncImport)
                    {
                        fg.AppendLine($"return (ret, {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder));");
                    }
                    else
                    {
                        fg.AppendLine($"errorMask = {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder);");
                        fg.AppendLine("return ret;");
                    }
                }
                fg.AppendLine();

                if (GenerateMainCreate(obj))
                {
                    using (var args = new FunctionWrapper(fg,
                        $"public{obj.NewOverride()}static {await this.ObjectReturn(obj, maskReturn: false)} {CreateFromPrefix}{ModuleNickname}"))
                    {
                        foreach (var (API, Public) in this.MainAPI.ReaderAPI.IterateAPI(obj,
                            new APILine(ErrorMaskKey, "ErrorMaskBuilder errorMask"),
                            new APILine(TranslationMaskKey, $"{nameof(TranslationCrystal)} translationMask", (o) => this.TranslationMaskParameter)))
                        {
                            args.Add(API.Result);
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        await GenerateCreateSnippet(obj, fg);
                    }
                    fg.AppendLine();
                }

                foreach (var minorAPI in this.MinorAPIs)
                {
                    if (!minorAPI.When?.Invoke(obj) ?? false) continue;
                    if (obj.CanAssume())
                    {
                        using (var args = new FunctionWrapper(fg,
                            $"public static {await this.ObjectReturn(obj, maskReturn: false)} {CreateFromPrefix}{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes())}"))
                        {
                            args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: GetMaskTypes()));
                            foreach (var (API, Public) in minorAPI.ReaderAPI.IterateAPI(obj))
                            {
                                if (Public)
                                {
                                    args.Add(API.Result);
                                }
                            }
                            if (this.TranslationMaskParameter)
                            {
                                args.Add(GetTranslationMaskParameter().Resolver(obj).Result);
                            }
                        }
                        using (new BraceWrapper(fg))
                        {
                            minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                            {
                                using (var args = new ArgsWrapper(fg,
                                    $"return {Utility.Await(asyncImport)}{CreateFromPrefix}{ModuleNickname}"))
                                {
                                    foreach (var item in this.MainAPI.WrapAccessors(obj, TranslationModuleAPI.Direction.Reader, accessor))
                                    {
                                        args.Add(item);
                                    }
                                    if (this.TranslationMaskParameter)
                                    {
                                        args.Add("translationMask: translationMask");
                                    }
                                }
                            });
                        }
                        fg.AppendLine();
                    }

                    using (var args = new FunctionWrapper(fg,
                        $"public static {await this.ObjectReturn(obj, maskReturn: true)} {CreateFromPrefix}{ModuleNickname}{errorLabel}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}"))
                    {
                        args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: GetMaskTypes(MaskType.Error)));
                        foreach (var (API, Public) in minorAPI.ReaderAPI.IterateAPI(
                            obj,
                            new APILine(ErrorMaskKey, resolver: (o) => $"out {o.Mask(MaskType.Error)} errorMask", when: (o) => !asyncImport),
                            GetTranslationMaskParameter()))
                        {
                            if (Public)
                            {
                                args.Add(API.Result);
                            }
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"return {Utility.Await(asyncImport)}{CreateFromPrefix}{ModuleNickname}{errorLabel}"))
                            using (new DepthWrapper(fg))
                            {
                                foreach (var item in this.MainAPI.WrapAccessors(obj, TranslationModuleAPI.Direction.Reader, accessor))
                                {
                                    args.Add(item);
                                }
                                if (!asyncImport)
                                {
                                    args.Add($"errorMask: out errorMask");
                                }
                                if (this.TranslationMaskParameter)
                                {
                                    args.Add("translationMask: translationMask");
                                }
                            }
                        });
                    }
                    fg.AppendLine();

                    using (var args = new FunctionWrapper(fg,
                        $"public static {await this.ObjectReturn(obj, maskReturn: false)} {CreateFromPrefix}{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}"))
                    {
                        args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.Direct, maskTypes: GetMaskTypes(MaskType.Error)));
                        foreach (var (API, Public) in minorAPI.ReaderAPI.IterateAPI(
                            obj,
                            new APILine(ErrorMaskBuilderKey, $"ErrorMaskBuilder errorMask"),
                            GetTranslationMaskParameter()))
                        {
                            if (Public)
                            {
                                args.Add(API.Result);
                            }
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"return {Utility.Await(asyncImport)}{CreateFromPrefix}{ModuleNickname}"))
                            using (new DepthWrapper(fg))
                            {
                                foreach (var item in this.MainAPI.WrapFinalAccessors(obj, TranslationModuleAPI.Direction.Reader, accessor))
                                {
                                    args.Add(item);
                                }
                                args.Add($"errorMask: errorMask");
                                if (this.TranslationMaskParameter)
                                {
                                    args.Add("translationMask: translationMask?.GetCrystal()");
                                }
                            }
                        });
                    }
                    fg.AppendLine();
                }
            }
        }

        private void FillWriterArgs(
            FunctionWrapper args, 
            ObjectGeneration obj, 
            bool? objParam = false,
            bool doFallbackCustom = true,
            bool addIndex = false)
        {
            foreach (var item in this.MainAPI.WriterAPI.MajorAPI)
            {
                if (item.TryResolve(obj, out var line))
                {
                    args.Add(line.Result);
                }
            }
            if (objParam.HasValue)
            {
                args.Add($"{(objParam.Value ? "object" : obj.Interface(getter: true))} item");
            }
            if (doFallbackCustom)
            {
                foreach (var item in this.MainAPI.WriterAPI.CustomAPI)
                {
                    if (item.API.TryResolve(obj, out var line))
                    {
                        args.Add(line.Result);
                    }
                }
            }
            args.Add($"ErrorMaskBuilder errorMask");
            if (addIndex)
            {
                args.Add("int fieldIndex");
            }
            if (this.TranslationMaskParameter)
            {
                args.Add($"{nameof(TranslationCrystal)} translationMask");
            }
            foreach (var item in this.MainAPI.WriterAPI.OptionalAPI)
            {
                if (item.TryResolve(obj, out var line))
                {
                    args.Add(line.Result);
                }
            }
        }

        protected virtual async Task TranslationWrite(ObjectGeneration obj, FileGeneration fg)
        {
            var inheriting = (await obj.InheritingObjects()).Any();

            using (var args = new FunctionWrapper(fg,
                $"public{(inheriting ? " virtual" : null)} void Write{obj.GetGenericTypes(MaskType.Normal)}"))
            {
                args.Wheres.AddRange(obj.GenerateWhereClauses(LoquiInterfaceType.IGetter, defs: obj.Generics));
                FillWriterArgs(args, obj);
            }
            using (new BraceWrapper(fg))
            {
                GenerateWriteSnippet(obj, fg);
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public{obj.FunctionOverride()}void Write"))
            {
                FillWriterArgs(args, obj, objParam: true);
            }
            using (new BraceWrapper(fg))
            {
                if (obj.Generics.Count > 0)
                {
                    fg.AppendLine("throw new NotImplementedException();");
                }
                else
                {
                    using (var args = new ArgsWrapper(fg, $"Write"))
                    {
                        args.Add($"item: ({obj.Interface(getter: true)})item");
                        args.Add(this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Writer));
                        args.Add(this.MainAPI.InternalPassArgs(obj, TranslationModuleAPI.Direction.Writer));
                        args.Add($"errorMask: errorMask");
                        if (this.TranslationMaskParameter)
                        {
                            args.Add($"translationMask: translationMask");
                        }
                    }
                }
            }
            fg.AppendLine();

            foreach (var baseObj in obj.BaseClassTrail())
            {
                using (var args = new FunctionWrapper(fg,
                    $"public override void Write{obj.GetGenericTypes(MaskType.Normal)}"))
                {
                    FillWriterArgs(args, baseObj);
                }
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg, $"Write"))
                    {
                        args.Add($"item: ({obj.Interface(getter: true)})item");
                        args.Add(this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Writer));
                        args.Add(this.MainAPI.InternalPassArgs(obj, TranslationModuleAPI.Direction.Writer));
                        args.Add($"errorMask: errorMask");
                        if (this.TranslationMaskParameter)
                        {
                            args.Add($"translationMask: translationMask");
                        }
                    }
                }
                fg.AppendLine();
            }

            if (obj.IsTopClass
                && this.DoErrorMasks)
            {
                using (var args = new FunctionWrapper(fg,
                    $"public void Write{obj.GetGenericTypes(MaskType.Normal)}"))
                {
                    args.Wheres.AddRange(obj.GenerateWhereClauses(LoquiInterfaceType.IGetter, defs: obj.Generics));
                    FillWriterArgs(args, obj, addIndex: true);
                }
                using (new BraceWrapper(fg))
                {
                    MaskGenerationUtility.WrapErrorFieldIndexPush(
                        fg,
                        errorMaskAccessor: "errorMask",
                        indexAccessor: "fieldIndex",
                        toDo: () =>
                        {
                            using (var args = new ArgsWrapper(fg, $"Write"))
                            {
                                args.Add($"item: ({obj.Interface(getter: true)})item");
                                args.Add(this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Writer));
                                args.Add(this.MainAPI.InternalPassArgs(obj, TranslationModuleAPI.Direction.Writer));
                                args.Add($"errorMask: errorMask");
                                if (this.TranslationMaskParameter)
                                {
                                    args.Add($"translationMask: translationMask");
                                }
                            }
                        });
                }
                fg.AppendLine();
            }
        }

        protected abstract void GenerateWriteSnippet(ObjectGeneration obj, FileGeneration fg);

        private async Task GenerateWriteMixIn(ObjectGeneration obj, FileGeneration fg)
        {
            using (var args = new FunctionWrapper(fg,
                $"public static void {WriteToPrefix}{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Normal, MaskType.Error))}"))
            {
                args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.IGetter, maskTypes: GetMaskTypes(MaskType.Normal, MaskType.Error)));
                args.Add($"this {obj.Interface(getter: true)} item");
                foreach (var item in this.MainAPI.WriterAPI.MajorAPI)
                {
                    if (item.TryResolve(obj, out var line))
                    {
                        args.Add(line.Result);
                    }
                }
                foreach (var item in this.MainAPI.WriterAPI.CustomAPI)
                {
                    if (!item.Public) continue;
                    if (item.API.TryResolve(obj, out var line))
                    {
                        args.Add(line.Result);
                    }
                }
                args.Add($"out {obj.Mask(MaskType.Error)} errorMask");
                args.Add($"bool doMasks = true");
                if (this.TranslationMaskParameter)
                {
                    args.Add($"{obj.Mask(MaskType.Translation)} translationMask = null");
                }
                foreach (var item in this.MainAPI.WriterAPI.OptionalAPI)
                {
                    if (item.TryResolve(obj, out var line))
                    {
                        args.Add(line.Result);
                    }
                }
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("ErrorMaskBuilder errorMaskBuilder = doMasks ? new ErrorMaskBuilder() : null;");
                using (var args = new ArgsWrapper(fg,
                    $"{this.TranslatorReference(obj, "item")}.Write"))
                {
                    args.Add("item: item");
                    foreach (var item in this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Writer))
                    {
                        args.Add(item);
                    }
                    foreach (var item in this.MainAPI.InternalFallbackArgs(obj, TranslationModuleAPI.Direction.Writer))
                    {
                        args.Add(item);
                    }
                    args.Add($"errorMask: errorMaskBuilder");
                    if (this.TranslationMaskParameter)
                    {
                        args.Add("translationMask: translationMask?.GetCrystal()");
                    }
                }
                fg.AppendLine($"errorMask = {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder);");
            }
            fg.AppendLine();

            foreach (var minorAPI in this.MinorAPIs)
            {
                if (!minorAPI.When?.Invoke(obj) ?? false) continue;
                using (var args = new FunctionWrapper(fg,
                    $"public static void {WriteToPrefix}{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Normal, MaskType.Error))}"))
                {
                    args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.IGetter, maskTypes: GetMaskTypes(MaskType.Normal, MaskType.Error)));
                    args.Add($"this {obj.Interface(getter: true)} item");
                    foreach (var item in minorAPI.WriterAPI.IterateAPI(
                        obj,
                        new APILine(ErrorMaskKey, $"out {obj.Mask(MaskType.Error)} errorMask"),
                        new APILine(TranslationMaskKey, $"{obj.Mask(MaskType.Translation)} translationMask = null", (o) => this.TranslationMaskParameter),
                        new APILine(DoMaskKey, "bool doMasks = true")))
                    {
                        if (!item.Public) continue;
                        args.Add(item.API.Result);
                    }
                }
                using (new BraceWrapper(fg))
                {
                    minorAPI.Funnel.OutConverter(obj, fg, (accessor) =>
                    {
                        using (var args = new ArgsWrapper(fg,
                            $"{WriteToPrefix}{ModuleNickname}"))
                        using (new DepthWrapper(fg))
                        {
                            args.Add("item: item");
                            foreach (var item in this.MainAPI.WrapAccessors(obj, TranslationModuleAPI.Direction.Writer, accessor))
                            {
                                args.Add(item);
                            }
                            args.Add($"errorMask: out errorMask");
                            args.Add("doMasks: doMasks");
                            if (this.TranslationMaskParameter)
                            {
                                args.Add("translationMask: translationMask");
                            }
                        }
                    });
                }
                fg.AppendLine();

                if (obj.IsTopClass)
                {
                    using (var args = new FunctionWrapper(fg,
                        $"public static void {WriteToPrefix}{ModuleNickname}{obj.GetGenericTypes(MaskType.Normal)}"))
                    {
                        args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.IGetter, maskTypes: MaskType.Normal));
                        args.Add($"this {obj.Interface(getter: true)} item");
                        foreach (var item in minorAPI.WriterAPI.IterateAPI(
                            obj,
                            new APILine(ErrorMaskKey, $"{nameof(ErrorMaskBuilder)} errorMask"),
                            new APILine(TranslationMaskKey, $"{nameof(TranslationCrystal)} translationMask = null", (o) => this.TranslationMaskParameter),
                            new APILine(DoMaskKey, "bool doMasks = true")))
                        {
                            if (!item.Public) continue;
                            args.Add(item.API.Result);
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.OutConverter(obj, fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"{WriteToPrefix}{ModuleNickname}{obj.GetGenericTypes(MaskType.Normal)}"))
                            using (new DepthWrapper(fg))
                            {
                                args.Add("item: item");
                                foreach (var item in this.MainAPI.WrapAccessors(obj, TranslationModuleAPI.Direction.Writer, accessor))
                                {
                                    args.Add(item);
                                }
                                args.Add($"errorMask: errorMask");
                                if (this.TranslationMaskParameter)
                                {
                                    args.Add("translationMask: translationMask");
                                }
                            }
                        });
                    }
                    fg.AppendLine();
                }
            }

            if (obj.IsTopClass)
            {
                using (var args = new FunctionWrapper(fg,
                    $"public static void {WriteToPrefix}{ModuleNickname}{obj.GetGenericTypes(MaskType.Normal)}"))
                {
                    args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.IGetter, maskTypes: MaskType.Normal));
                    args.Add($"this {obj.Interface(getter: true)} item");
                    foreach (var item in this.MainAPI.WriterAPI.MajorAPI)
                    {
                        if (item.TryResolve(obj, out var line))
                        {
                            args.Add(line.Result);
                        }
                    }
                    foreach (var item in this.MainAPI.WriterAPI.CustomAPI)
                    {
                        if (!item.Public) continue;
                        if (item.API.TryResolve(obj, out var line))
                        {
                            args.Add(line.Result);
                        }
                    }
                    args.Add($"ErrorMaskBuilder errorMask");
                    if (this.TranslationMaskParameter)
                    {
                        args.Add($"{nameof(TranslationCrystal)} translationMask = null");
                    }
                    foreach (var item in this.MainAPI.WriterAPI.OptionalAPI)
                    {
                        if (item.TryResolve(obj, out var line))
                        {
                            args.Add(line.Result);
                        }
                    }
                }
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{this.TranslatorReference(obj, "item")}.Write"))
                    {
                        args.Add("item: item");
                        foreach (var item in this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Writer))
                        {
                            args.Add(item);
                        }
                        foreach (var item in this.MainAPI.InternalFallbackArgs(obj, TranslationModuleAPI.Direction.Writer))
                        {
                            args.Add(item);
                        }
                        args.Add($"errorMask: errorMask");
                        if (this.TranslationMaskParameter)
                        {
                            args.Add("translationMask: translationMask");
                        }
                    }
                }
                fg.AppendLine();

                using (var args = new FunctionWrapper(fg,
                    $"public static void {WriteToPrefix}{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Normal, MaskType.Error))}"))
                {
                    args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.IGetter, maskTypes: GetMaskTypes(MaskType.Normal, MaskType.Error)));
                    args.Add($"this {obj.Interface(getter: true)} item");
                    foreach (var (API, Public) in this.MainAPI.WriterAPI.IterateAPI(obj))
                    {
                        if (Public)
                        {
                            args.Add(API.Result);
                        }
                    }
                    if (this.TranslationMaskParameter)
                    {
                        args.Add($"{obj.Mask(MaskType.Translation)} translationMask = null");
                    }
                }
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{this.TranslatorReference(obj, "item")}.Write"))
                    {
                        args.Add("item: item");
                        foreach (var item in this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Writer))
                        {
                            args.Add(item);
                        }
                        foreach (var item in this.MainAPI.InternalFallbackArgs(obj, TranslationModuleAPI.Direction.Writer))
                        {
                            args.Add(item);
                        }
                        args.Add($"errorMask: null");
                        if (this.TranslationMaskParameter)
                        {
                            args.Add("translationMask: translationMask.GetCrystal()");
                        }
                    }
                }
                fg.AppendLine();

                foreach (var minorAPI in this.MinorAPIs)
                {
                    if (!minorAPI.When?.Invoke(obj) ?? false) continue;
                    using (var args = new FunctionWrapper(fg,
                        $"public static void {WriteToPrefix}{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Normal))}"))
                    {
                        args.Wheres.AddRange(obj.GenericTypeMaskWheres(LoquiInterfaceType.IGetter, maskTypes: GetMaskTypes(MaskType.Normal)));
                        args.Add($"this {obj.Interface(getter: true)} item");
                        foreach (var (API, Public) in minorAPI.WriterAPI.IterateAPI(obj))
                        {
                            if (Public)
                            {
                                args.Add(API.Result);
                            }
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.OutConverter(obj, fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"{this.TranslatorReference(obj, "item")}.Write"))
                            {
                                args.Add("item: item");
                                foreach (var item in this.MainAPI.WrapAccessors(obj, TranslationModuleAPI.Direction.Writer, accessor))
                                {
                                    args.Add(item);
                                }
                                foreach (var item in this.MainAPI.InternalFallbackArgs(obj, TranslationModuleAPI.Direction.Writer))
                                {
                                    args.Add(item);
                                }
                                args.Add($"errorMask: null");
                                if (this.TranslationMaskParameter)
                                {
                                    args.Add("translationMask: null");
                                }
                            }
                        });
                    }
                    fg.AppendLine();
                }
            }
        }

        public void AddTypeAssociation<T>(G transl, bool overrideExisting = false)
            where T : TypeGeneration
        {
            if (overrideExisting)
            {
                this._typeGenerations[typeof(T)] = transl;
            }
            else
            {
                this._typeGenerations.Add(typeof(T), transl);
            }
        }

        public bool TryGetTypeGeneration(Type t, out G gen)
        {
            if (!this._typeGenerations.TryGetValue(t, out gen))
            {
                foreach (var kv in _typeGenerations.ToList())
                {
                    if (t.InheritsFrom(kv.Key))
                    {
                        _typeGenerations[t] = kv.Value;
                        gen = kv.Value;
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        public G GetTypeGeneration(Type t)
        {
            return this._typeGenerations[t];
        }

        public override async Task GenerateInRegistration(ObjectGeneration obj, FileGeneration fg)
        {
            await base.GenerateInRegistration(obj, fg);
            fg.AppendLine($"public static readonly Type {this.ModuleNickname}WriteTranslation = typeof({this.TranslationWriteClassName(obj)});");
        }

        public override async Task LoadWrapup(ObjectGeneration obj)
        {
            await base.LoadWrapup(obj);
            foreach (var gen in obj.Generics.Values)
            {
                if (!gen.Loqui) continue;
                gen.Add(this.TranslationItemInterface);
            }
        }
    }
}
