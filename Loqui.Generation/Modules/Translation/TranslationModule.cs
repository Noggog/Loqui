using Loqui.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class TranslationModule<G> : GenerationModule
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
        public string TranslationClassName(ObjectGeneration obj) => $"{obj.Name}{ModuleNickname}Translation";
        public string TranslationClass(ObjectGeneration obj) => $"{TranslationClassName(obj)}{obj.GetGenericTypes(MaskType.Normal)}";

        public const string ErrorMaskKey = "ErrorMask";
        public const string ErrorMaskBuilderKey = "ErrorMaskBuilder";
        public const string DoMaskKey = "DoMasks";
        public const string TranslationMaskKey = "TranslationMask";

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

        public override async Task<IEnumerable<string>> Interfaces(ObjectGeneration obj)
        {
            return Enumerable.Empty<string>();
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
            using (var args = new ClassWrapper(fg, TranslationClass(obj)))
            {
                args.Partial = true;
                args.BaseClass = obj.HasLoquiBaseObject ? TranslationClass(obj.BaseClass) : null;
            }
            obj.WriteWhereClauses(fg, obj.Generics);
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"public{obj.NewOverride()}readonly static {TranslationClass(obj)} Instance = new {TranslationClass(obj)}();");
                fg.AppendLine();

                await GenerateInTranslationClass(obj, fg);
            }
        }

        public virtual async Task GenerateInTranslationClass(ObjectGeneration obj, FileGeneration fg)
        {
            using (new RegionWrapper(fg, $"{ModuleNickname} Write"))
            {
                CommonWrite(obj, fg);
                foreach (var extra in ExtraTranslationTasks)
                {
                    await extra(obj, fg);
                }
            }
        }

        public override async Task MiscellaneousGenerationActions(ObjectGeneration obj)
        {
        }

        private async Task<string> ErrorLabel(ObjectGeneration obj) => await this.AsyncImport(obj) ? "_Error" : null;

        public override async Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            if (this.MainAPI == null)
            {
                throw new ArgumentException("Main API need to be set.");
            }
            if (!obj.Abstract || this.GenerateAbstractCreates)
            {
                await GenerateCreate(obj, fg);
            }
            if (ShouldGenerateCopyIn)
            {
                await GenerateCopyIn(obj, fg);
            }
            await GenerateWrite(obj, fg);
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
                        $"public{obj.FunctionOverride()}void CopyIn_{ModuleNickname}"))
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
                            $"CopyIn_{ModuleNickname}_Internal"))
                        {
                            args.Add(this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Reader));
                            args.Add($"errorMask: null");
                            args.Add($"translationMask: null");
                        }
                    }
                    fg.AppendLine();
                }

                using (var args = new FunctionWrapper(fg,
                    $"public virtual void CopyIn_{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}",
                    wheres: obj.GenericTypeMaskWheres(GetMaskTypes(MaskType.Error))))
                {
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
                        $"CopyIn_{ModuleNickname}_Internal"))
                    {
                        args.Add(this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Reader));
                        args.Add($"errorMask: errorMaskBuilder");
                        args.Add($"translationMask: translationMask?.GetCrystal()");
                    }
                    fg.AppendLine($"errorMask = {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder);");
                }
                fg.AppendLine();

                using (var args = new FunctionWrapper(fg,
                    $"protected{obj.FunctionOverride()}void CopyIn_{ModuleNickname}_Internal"))
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
                    using (var args = new ArgsWrapper(fg,
                        $"Loqui{ModuleNickname}Translation<{obj.ObjectName}>.Instance.CopyIn"))
                    {
                        foreach (var item in this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Reader))
                        {
                            args.Add(item);
                        }
                        args.Add($"item: this");
                        args.Add($"skipProtected: true");
                        args.Add($"errorMask: errorMask");
                        if (this.TranslationMaskParameter)
                        {
                            args.Add($"translationMask: translationMask");
                        }
                    }
                }
                fg.AppendLine();

                foreach (var minorAPI in this.MinorAPIs)
                {
                    if (!minorAPI.When?.Invoke(obj) ?? false) continue;
                    if (obj.CanAssume())
                    {
                        using (var args = new FunctionWrapper(fg,
                            $"public void CopyIn_{ModuleNickname}"))
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
                                    $"this.CopyIn_{ModuleNickname}"))
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

                    using (var args = new FunctionWrapper(fg,
                        $"public void CopyIn_{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}",
                        wheres: obj.GenericTypeMaskWheres(GetMaskTypes(MaskType.Error))))
                    {
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
                                $"this.CopyIn_{ModuleNickname}"))
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
                        $"public override void CopyIn_{ModuleNickname}{obj.GetBaseMask_GenericTypes(MaskType.Error)}"))
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
                                $"CopyIn_{ModuleNickname}_Internal"))
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
                        $"public static {await this.ObjectReturn(obj, maskReturn: false)} Create_{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes())}",
                        wheres: obj.GenericTypeMaskWheres(GetMaskTypes())))
                    {
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
                            $"return {Utility.Await(asyncImport)}Create_{ModuleNickname}"))
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
                    $"public static {await this.ObjectReturn(obj, maskReturn: true)} Create_{ModuleNickname}{errorLabel}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}",
                    wheres: obj.GenericTypeMaskWheres(GetMaskTypes(MaskType.Error))))
                {
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
                        $"var ret = {Utility.Await(asyncImport)}Create_{ModuleNickname}"))
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
                        $"public{obj.NewOverride()}static {await this.ObjectReturn(obj, maskReturn: false)} Create_{ModuleNickname}"))
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
                            $"public static {await this.ObjectReturn(obj, maskReturn: false)} Create_{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes())}",
                            wheres: obj.GenericTypeMaskWheres(GetMaskTypes())))
                        {
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
                                    $"return {Utility.Await(asyncImport)}Create_{ModuleNickname}"))
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
                        $"public static {await this.ObjectReturn(obj, maskReturn: true)} Create_{ModuleNickname}{errorLabel}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}",
                        wheres: obj.GenericTypeMaskWheres(GetMaskTypes(MaskType.Error))))
                    {
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
                                $"return {Utility.Await(asyncImport)}Create_{ModuleNickname}{errorLabel}"))
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
                        $"public static {await this.ObjectReturn(obj, maskReturn: false)} Create_{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}",
                        wheres: obj.GenericTypeMaskWheres(GetMaskTypes(MaskType.Error))))
                    {
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
                                $"return {Utility.Await(asyncImport)}Create_{ModuleNickname}"))
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

        private void CommonWrite(ObjectGeneration obj, FileGeneration fg)
        {
            using (var args = new FunctionWrapper(fg,
                $"public void Write_{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}",
                obj.GenericTypeMaskWheres(GetMaskTypes(MaskType.Error)).ToArray()))
            {
                foreach (var item in this.MainAPI.WriterAPI.MajorAPI)
                {
                    if (item.TryResolve(obj, out var line))
                    {
                        args.Add(line.Result);
                    }
                }
                args.Add($"{obj.Interface(internalInterface: obj.HasInternalInterface, getter: true)} item");
                foreach (var item in this.MainAPI.WriterAPI.CustomAPI)
                {
                    if (item.API.TryResolve(obj, out var line))
                    {
                        args.Add(line.Result);
                    }
                }
                args.Add($"bool doMasks");
                args.Add($"out {obj.Mask(MaskType.Error)} errorMask");
                if (this.TranslationMaskParameter)
                {
                    args.Add($"{obj.Mask(MaskType.Translation)} translationMask");
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
                fg.AppendLine($"ErrorMaskBuilder errorMaskBuilder = doMasks ? new ErrorMaskBuilder() : null;");
                using (var args = new ArgsWrapper(fg,
                    $"Write_{ModuleNickname}"))
                {
                    foreach (var item in this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Writer))
                    {
                        args.Add(item);
                    }
                    args.Add("item: item");
                    foreach (var item in this.MainAPI.InternalPassArgs(obj, TranslationModuleAPI.Direction.Writer))
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

            using (var args = new FunctionWrapper(fg,
                $"public void Write_{ModuleNickname}"))
            {
                foreach (var item in this.MainAPI.WriterAPI.MajorAPI)
                {
                    if (item.TryResolve(obj, out var line))
                    {
                        args.Add(line.Result);
                    }
                }
                args.Add($"{obj.Interface(internalInterface: obj.HasInternalInterface, getter: true)} item");
                foreach (var item in this.MainAPI.WriterAPI.CustomAPI)
                {
                    if (item.API.TryResolve(obj, out var line))
                    {
                        args.Add(line.Result);
                    }
                }
                args.Add($"ErrorMaskBuilder errorMask");
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
            using (new BraceWrapper(fg))
            {
                GenerateWriteSnippet(obj, fg);
            }
        }

        protected abstract void GenerateWriteSnippet(ObjectGeneration obj, FileGeneration fg);

        private async Task GenerateWrite(ObjectGeneration obj, FileGeneration fg)
        {
            using (new RegionWrapper(fg, $"{this.ModuleNickname} Write"))
            {
                using (var args = new FunctionWrapper(fg,
                    $"public virtual void Write_{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}",
                    wheres: obj.GenericTypeMaskWheres(GetMaskTypes(MaskType.Error))))
                {
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
                        $"{TranslationClass(obj)}.Instance.Write_{ModuleNickname}"))
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
                        $"public virtual void Write_{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}",
                        wheres: obj.GenericTypeMaskWheres(GetMaskTypes(MaskType.Error))))
                    {
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
                                $"Write_{ModuleNickname}"))
                            using (new DepthWrapper(fg))
                            {
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

                    using (var args = new FunctionWrapper(fg,
                        $"public{obj.FunctionOverride()}void Write_{ModuleNickname}"))
                    {
                        foreach (var item in minorAPI.WriterAPI.IterateAPI(obj,
                            new APILine(ErrorMaskBuilderKey, $"ErrorMaskBuilder errorMask"),
                            new APILine(TranslationMaskKey, $"{nameof(TranslationCrystal)} translationMask", (o) => this.TranslationMaskParameter)))
                        {
                            args.Add(item.API.Result);
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.OutConverter(obj, fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"this.Write_{ModuleNickname}"))
                            {
                                foreach (var item in this.MainAPI.WrapAccessors(obj, TranslationModuleAPI.Direction.Writer, accessor))
                                {
                                    args.Add(item);
                                }
                                foreach (var item in this.MainAPI.InternalFallbackArgs(obj, TranslationModuleAPI.Direction.Writer))
                                {
                                    args.Add(item);
                                }
                                args.Add("errorMask: errorMask");
                                if (this.TranslationMaskParameter)
                                {
                                    args.Add($"translationMask: translationMask");
                                }
                            }
                        });
                    }
                }

                if (obj.IsTopClass)
                {
                    using (var args = new FunctionWrapper(fg,
                        $"public{obj.FunctionOverride()}void Write_{ModuleNickname}{obj.GetGenericTypes(GetMaskTypes(MaskType.Error))}",
                        wheres: obj.GenericTypeMaskWheres(GetMaskTypes(MaskType.Error))))
                    {
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
                            $"this.Write_{ModuleNickname}"))
                        {
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
                            $"public{obj.FunctionOverride()}void Write_{ModuleNickname}"))
                        {
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
                                    $"this.Write_{ModuleNickname}"))
                                {
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
                else if (obj.BaseClassTrail().Any())
                {
                    using (new RegionWrapper(fg, "Base Class Trickdown Overrides"))
                    {
                        foreach (var baseClass in obj.BaseClassTrail())
                        {
                            using (var args = new FunctionWrapper(fg,
                                $"public override void Write_{ModuleNickname}{obj.GetBaseMask_GenericTypes(MaskType.Error)}"))
                            {
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
                                args.Add($"out {baseClass.Mask(MaskType.Error)} errorMask");
                                args.Add($"bool doMasks = true");
                                if (this.TranslationMaskParameter)
                                {
                                    args.Add($"{baseClass.Mask(MaskType.Translation)} translationMask = null");
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
                                    $"{TranslationClass(obj)}.Instance.Write_{ModuleNickname}"))
                                {
                                    args.Add("item: this");
                                    foreach (var item in this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Writer))
                                    {
                                        args.Add(item);
                                    }
                                    args.Add($"errorMask: errorMaskBuilder");
                                    if (this.TranslationMaskParameter)
                                    {
                                        args.Add("translationMask: translationMask?.GetCrystal()");
                                    }
                                    foreach (var item in this.MainAPI.InternalFallbackArgs(obj, TranslationModuleAPI.Direction.Writer))
                                    {
                                        args.Add(item);
                                    }
                                }
                                fg.AppendLine($"errorMask = {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder);");
                            }
                            fg.AppendLine();
                        }
                    }
                }

                using (var args = new FunctionWrapper(fg,
                    $"public{obj.FunctionOverride()}void Write_{ModuleNickname}"))
                {
                    foreach (var item in this.MainAPI.WriterAPI.IterateAPI(
                        obj,
                        new APILine(ErrorMaskKey, $"ErrorMaskBuilder errorMask"),
                        new APILine(TranslationMaskKey, $"{nameof(TranslationCrystal)} translationMask", (o) => this.TranslationMaskParameter)))
                    {
                        args.Add(item.API.Result);
                    }
                }
                using (new BraceWrapper(fg))
                {
                    string funcName = $"Write_{ModuleNickname}";
                    using (var args = new ArgsWrapper(fg,
                        $"{TranslationClass(obj)}.Instance.{funcName}"))
                    {
                        args.Add("item: this");
                        foreach (var item in this.MainAPI.PassArgs(obj, TranslationModuleAPI.Direction.Writer))
                        {
                            args.Add(item);
                        }
                        foreach (var item in this.MainAPI.InternalPassArgs(obj, TranslationModuleAPI.Direction.Writer))
                        {
                            args.Add(item);
                        }
                        args.Add("errorMask: errorMask");
                        if (this.TranslationMaskParameter)
                        {
                            args.Add($"translationMask: translationMask");
                        }
                    }
                }

                if (obj.HasNewGenerics)
                {
                    fg.AppendLine();
                    using (var args = new FunctionWrapper(fg,
                        $"protected void Write_{ModuleNickname}"))
                    {
                        foreach (var item in this.MainAPI.WriterAPI.MajorAPI)
                        {
                            if (item.TryResolve(obj, out var line))
                            {
                                args.Add(line.Result);
                            }
                        }
                        args.Add($"ErrorMaskBuilder errorMask");
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
                        string funcName = $"Write_{ModuleNickname}{obj.GetGenericTypes(MaskType.Normal, MaskType.Error)}";
                        using (var args = new ArgsWrapper(fg,
                            $"{obj.ExtCommonName}.{funcName}"))
                        {
                            args.Add("writer: writer");
                            args.Add("item: this");
                            args.Add("errorMask: errorMask");
                        }
                    }
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
    }
}
