using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public abstract class TranslationModule : GenerationModule
    {
        public LoquiGenerator Gen;
        public abstract string ModuleNickname { get; }
        public override string RegionString => $"{ModuleNickname} Translation";
        public abstract string Namespace { get; }
        public IEnumerable<TranslationModuleAPI> AllAPI => MainAPI.And(MinorAPIs);
        public TranslationModuleAPI MainAPI;
        protected List<TranslationModuleAPI> MinorAPIs = new List<TranslationModuleAPI>();
        public bool ExportWithIGetter = true;
        public bool ShouldGenerateCopyIn = true;

        public TranslationModule(LoquiGenerator gen)
        {
            this.Gen = gen;
        }

        public override IEnumerable<string> GetWriterInterfaces(ObjectGeneration obj)
        {
            yield break;
        }

        public override IEnumerable<string> GetReaderInterfaces(ObjectGeneration obj)
        {
            yield break;
        }

        public override IEnumerable<string> Interfaces(ObjectGeneration obj)
        {
            yield break;
        }

        public override IEnumerable<string> RequiredUsingStatements(ObjectGeneration obj)
        {
            yield return "System.Diagnostics";
        }

        public override async Task Modify(LoquiGenerator gen)
        {
        }

        public override async Task GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public override async Task GenerateInVoid(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public override async Task MiscellaneousGenerationActions(ObjectGeneration obj)
        {
        }

        public override async Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            if (this.MainAPI == null)
            {
                throw new ArgumentException("Main API need to be set.");
            }
            if (!obj.Abstract)
            {
                await GenerateCreate(obj, fg);
            }
            if (ShouldGenerateCopyIn)
            {
                await GenerateCopyIn(obj, fg);
            }
            await GenerateWrite(obj, fg);
        }

        protected abstract void GenerateCopyInSnippet(ObjectGeneration obj, FileGeneration fg, bool usingErrorMask);

        private async Task GenerateCopyIn(ObjectGeneration obj, FileGeneration fg)
        {
            if (obj is StructGeneration) return;
            if (this.MainAPI == null) return;

            using (new RegionWrapper(fg, $"{this.ModuleNickname} Copy In"))
            {
                if (obj.CanAssume())
                {
                    using (var args = new FunctionWrapper(fg,
                        $"public{await obj.FunctionOverride()}void CopyIn_{ModuleNickname}"))
                    {
                        foreach (var (API, Public) in this.MainAPI.ReaderAPI.IterateAPI(obj))
                        {
                            if (Public)
                            {
                                args.Add(API);
                            }
                        }
                        args.Add("NotifyingFireParameters cmds = null");
                    }
                    using (new BraceWrapper(fg))
                    {
                        GenerateCopyInSnippet(obj, fg, usingErrorMask: false);
                    }
                    fg.AppendLine();
                }

                using (var args = new FunctionWrapper(fg,
                    $"public virtual void CopyIn_{ModuleNickname}{obj.Mask_GenericClause(MaskType.Error)}",
                    wheres: obj.GenericTypes_ErrorMaskWheres))
                {
                    foreach (var item in this.MainAPI.ReaderAPI.MajorAPI)
                    {
                        if (item.TryResolve(obj, out var line))
                        {
                            args.Add(line);
                        }
                    }
                    args.Add($"out {obj.Mask(MaskType.Error)} errorMask");
                    foreach (var item in this.MainAPI.ReaderAPI.OptionalAPI)
                    {
                        if (item.TryResolve(obj, out var line))
                        {
                            args.Add(line);
                        }
                    }
                    args.Add("NotifyingFireParameters cmds = null");
                }
                using (new BraceWrapper(fg))
                {
                    GenerateCopyInSnippet(obj, fg, usingErrorMask: true);
                }
                fg.AppendLine();

                foreach (var minorAPI in this.MinorAPIs)
                {
                    if (obj.CanAssume())
                    {
                        using (var args = new FunctionWrapper(fg,
                            $"public void CopyIn_{ModuleNickname}"))
                        {
                            foreach (var (API, Public) in minorAPI.ReaderAPI.IterateAPI(obj))
                            {
                                if (Public)
                                {
                                    args.Add(API);
                                }
                            }
                            args.Add("NotifyingFireParameters cmds = null");
                        }
                        using (new BraceWrapper(fg))
                        {
                            minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                            {
                                using (var args = new ArgsWrapper(fg,
                                    $"this.CopyIn_{ModuleNickname}"))
                                using (new DepthWrapper(fg))
                                {
                                    foreach (var item in this.MainAPI.WrapReaderAccessors(obj, accessor))
                                    {
                                        args.Add(item);
                                    }
                                    args.Add($"cmds: cmds");
                                }
                            });
                        }
                        fg.AppendLine();
                    }

                    using (var args = new FunctionWrapper(fg,
                        $"public void CopyIn_{ModuleNickname}{obj.Mask_GenericClause(MaskType.Error)}",
                        wheres: obj.GenericTypes_ErrorMaskWheres))
                    {
                        foreach (var item in minorAPI.ReaderAPI.MajorAPI)
                        {
                            if (item.TryResolve(obj, out var line))
                            {
                                args.Add(line);
                            }
                        }
                        args.Add($"out {obj.Mask(MaskType.Error)} errorMask");
                        foreach (var item in minorAPI.ReaderAPI.OptionalAPI)
                        {
                            if (item.TryResolve(obj, out var line))
                            {
                                args.Add(line);
                            }
                        }
                        args.Add("NotifyingFireParameters cmds = null");
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"this.CopyIn_{ModuleNickname}"))
                            using (new DepthWrapper(fg))
                            {
                                foreach (var item in this.MainAPI.WrapReaderAccessors(obj, accessor))
                                {
                                    args.Add(item);
                                }
                                args.Add($"errorMask: out errorMask");
                                args.Add($"cmds: cmds");
                            }
                        });
                    }
                    fg.AppendLine();
                }

                foreach (var baseClass in obj.BaseClassTrail())
                {
                    using (var args = new FunctionWrapper(fg,
                        $"public override void CopyIn_{ModuleNickname}{obj.BaseMask_GenericClause(MaskType.Error)}"))
                    {
                        foreach (var item in this.MainAPI.ReaderAPI.MajorAPI)
                        {
                            if (item.TryResolve(obj, out var line))
                            {
                                args.Add(line);
                            }
                        }
                        args.Add($"out {baseClass.Mask(MaskType.Error)} errorMask");
                        foreach (var item in this.MainAPI.ReaderAPI.OptionalAPI)
                        {
                            if (item.TryResolve(obj, out var line))
                            {
                                args.Add(line);
                            }
                        }
                        args.Add($"NotifyingFireParameters cmds = null");
                    }
                    using (new BraceWrapper(fg))
                    {
                        using (var args = new ArgsWrapper(fg,
                            $"this.CopyIn_{ModuleNickname}"))
                        {
                            args.Add(this.MainAPI.ReaderPassArgs(obj));
                            args.Add($"errorMask: out {obj.Mask_GenericAssumed(MaskType.Error, onlyAssumeSubclass: true)} errMask");
                            args.Add($"cmds: cmds");
                        }
                        fg.AppendLine("errorMask = errMask;");
                    }
                    fg.AppendLine();
                }
            }
        }

        protected abstract Task GenerateCreateSnippet(ObjectGeneration obj, FileGeneration fg);

        private async Task GenerateCreate(ObjectGeneration obj, FileGeneration fg)
        {
            using (new RegionWrapper(fg, $"{this.ModuleNickname} Create"))
            {
                if (obj.CanAssume())
                {
                    fg.AppendLine("[DebuggerStepThrough]");
                    using (var args = new FunctionWrapper(fg,
                        $"public{obj.NewOverride}static {obj.ObjectName} Create_{ModuleNickname}"))
                    {
                        foreach (var (API, Public) in this.MainAPI.ReaderAPI.IterateAPI(obj))
                        {
                            if (Public)
                            {
                                args.Add(API);
                            }
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        using (var args = new ArgsWrapper(fg,
                            $"return Create_{ModuleNickname}"))
                        {
                            args.Add(this.MainAPI.ReaderPassArgs(obj));
                            foreach (var customArgs in this.MainAPI.ReaderInternalFallbackArgs(obj))
                            {
                                args.Add(customArgs);
                            }
                            args.Add("errorMask: null");
                        }
                    }
                    fg.AppendLine();
                }

                fg.AppendLine("[DebuggerStepThrough]");
                using (var args = new FunctionWrapper(fg,
                    $"public static {obj.ObjectName} Create_{ModuleNickname}{obj.Mask_GenericClause(MaskType.Error)}",
                    wheres: obj.GenericTypes_ErrorMaskWheres))
                {
                    foreach (var (API, Public) in this.MainAPI.ReaderAPI.IterateAPI(
                        obj,
                        $"out {obj.Mask(MaskType.Error)} errorMask",
                        $"bool doMasks = true"))
                    {
                        if (Public)
                        {
                            args.Add(API);
                        }
                    }
                }
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("ErrorMaskBuilder errorMaskBuilder = doMasks ? new ErrorMaskBuilder() : null;");
                    using (var args = new ArgsWrapper(fg,
                        $"var ret = Create_{ModuleNickname}"))
                    {
                        args.Add(this.MainAPI.ReaderPassArgs(obj));
                        foreach (var customArgs in this.MainAPI.ReaderInternalFallbackArgs(obj))
                        {
                            args.Add(customArgs);
                        }
                        args.Add("errorMask: errorMaskBuilder");
                    }
                    fg.AppendLine($"errorMask = {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder);");
                    fg.AppendLine("return ret;");
                }
                fg.AppendLine();
                
                using (var args = new FunctionWrapper(fg,
                    $"public static {obj.ObjectName} Create_{ModuleNickname}"))
                {
                    foreach (var (API, Public) in this.MainAPI.ReaderAPI.IterateAPI
                        (obj,
                        "ErrorMaskBuilder errorMask"))
                    {
                        args.Add(API);
                    }
                }
                using (new BraceWrapper(fg))
                {
                    await GenerateCreateSnippet(obj, fg);
                }
                fg.AppendLine();

                foreach (var minorAPI in this.MinorAPIs)
                {
                    if (obj.CanAssume())
                    {
                        using (var args = new FunctionWrapper(fg,
                            $"public static {obj.ObjectName} Create_{ModuleNickname}"))
                        {
                            foreach (var (API, Public) in minorAPI.ReaderAPI.IterateAPI(obj))
                            {
                                if (Public)
                                {
                                    args.Add(API);
                                }
                            }
                        }
                        using (new BraceWrapper(fg))
                        {
                            minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                            {
                                using (var args = new ArgsWrapper(fg,
                                    $"return Create_{ModuleNickname}"))
                                using (new DepthWrapper(fg))
                                {
                                    foreach (var item in this.MainAPI.WrapReaderAccessors(obj, accessor))
                                    {
                                        args.Add(item);
                                    }
                                }
                            });
                        }
                        fg.AppendLine();
                    }

                    using (var args = new FunctionWrapper(fg,
                        $"public static {obj.ObjectName} Create_{ModuleNickname}{obj.Mask_GenericClause(MaskType.Error)}",
                        wheres: obj.GenericTypes_ErrorMaskWheres))
                    {
                        foreach (var (API, Public) in minorAPI.ReaderAPI.IterateAPI(
                            obj,
                            $"out {obj.Mask(MaskType.Error)} errorMask"))
                        {
                            if (Public)
                            {
                                args.Add(API);
                            }
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.InConverter(obj, fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"return Create_{ModuleNickname}"))
                            using (new DepthWrapper(fg))
                            {
                                foreach (var item in this.MainAPI.WrapReaderAccessors(obj, accessor))
                                {
                                    args.Add(item);
                                }
                                args.Add($"errorMask: out errorMask");
                            }
                        });
                    }
                    fg.AppendLine();
                }
            }
        }

        public override async Task GenerateInCommonExt(ObjectGeneration obj, FileGeneration fg)
        {
            using (new RegionWrapper(fg, $"{ModuleNickname} Write"))
            {
                CommonWrite(obj, fg);
            }
        }

        private void CommonWrite(ObjectGeneration obj, FileGeneration fg)
        {
            using (var args = new FunctionWrapper(fg,
                $"public static void Write_{ModuleNickname}{obj.GenericTypes_ErrMask}",
                obj.GenerateWhereClauses().And(obj.GenericTypes_ErrorMaskWheres).ToArray()))
            {
                foreach (var item in this.MainAPI.WriterAPI.MajorAPI)
                {
                    if (item.TryResolve(obj, out var line))
                    {
                        args.Add(line);
                    }
                }
                if (obj.ExportWithIGetter && this.ExportWithIGetter)
                {
                    args.Add($"{obj.Getter_InterfaceStr} item");
                }
                else
                {
                    args.Add($"{obj.ObjectName} item");
                }
                foreach (var item in this.MainAPI.WriterAPI.CustomAPI)
                {
                    if (item.API.TryResolve(obj, out var line))
                    {
                        args.Add(line);
                    }
                }
                args.Add($"bool doMasks");
                args.Add($"out {obj.Mask(MaskType.Error)} errorMask");
                foreach (var item in this.MainAPI.WriterAPI.OptionalAPI)
                {
                    if (item.TryResolve(obj, out var line))
                    {
                        args.Add(line);
                    }
                }
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"ErrorMaskBuilder errorMaskBuilder = doMasks ? new ErrorMaskBuilder() : null;");
                using (var args = new ArgsWrapper(fg,
                    $"Write_{ModuleNickname}{obj.GenericTypes}"))
                {
                    foreach (var item in this.MainAPI.WriterPassArgs(obj))
                    {
                        args.Add(item);
                    }
                    args.Add("item: item");
                    foreach (var item in this.MainAPI.WriterInternalPassArgs(obj))
                    {
                        args.Add(item);
                    }
                    args.Add($"errorMask: errorMaskBuilder");
                }
                fg.AppendLine($"errorMask = {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder);");
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"public static void Write_{ModuleNickname}{obj.GenericTypes}",
                obj.GenerateWhereClauses().ToArray()))
            {
                foreach (var item in this.MainAPI.WriterAPI.MajorAPI)
                {
                    if (item.TryResolve(obj, out var line))
                    {
                        args.Add(line);
                    }
                }
                if (obj.ExportWithIGetter && this.ExportWithIGetter)
                {
                    args.Add($"{obj.Getter_InterfaceStr} item");
                }
                else
                {
                    args.Add($"{obj.ObjectName} item");
                }
                foreach (var item in this.MainAPI.WriterAPI.CustomAPI)
                {
                    if (item.API.TryResolve(obj, out var line))
                    {
                        args.Add(line);
                    }
                }
                args.Add($"ErrorMaskBuilder errorMask");
                foreach (var item in this.MainAPI.WriterAPI.OptionalAPI)
                {
                    if (item.TryResolve(obj, out var line))
                    {
                        args.Add(line);
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
                    $"public virtual void Write_{ModuleNickname}{obj.Mask_GenericClause(MaskType.Error)}",
                    wheres: obj.GenericTypes_ErrorMaskWheres))
                {
                    foreach (var item in this.MainAPI.WriterAPI.MajorAPI)
                    {
                        if (item.TryResolve(obj, out var line))
                        {
                            args.Add(line);
                        }
                    }
                    foreach (var item in this.MainAPI.WriterAPI.CustomAPI)
                    {
                        if (!item.Public) continue;
                        if (item.API.TryResolve(obj, out var line))
                        {
                            args.Add(line);
                        }
                    }
                    args.Add($"out {obj.Mask(MaskType.Error)} errorMask");
                    args.Add($"bool doMasks = true");
                    foreach (var item in this.MainAPI.WriterAPI.OptionalAPI)
                    {
                        if (item.TryResolve(obj, out var line))
                        {
                            args.Add(line);
                        }
                    }
                }
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("ErrorMaskBuilder errorMaskBuilder = doMasks ? new ErrorMaskBuilder() : null;");
                    using (var args = new ArgsWrapper(fg,
                        $"this.Write_{ModuleNickname}_Internal"))
                    {
                        foreach (var item in this.MainAPI.WriterPassArgs(obj))
                        {
                            args.Add(item);
                        }
                        foreach (var item in this.MainAPI.WriterInternalFallbackArgs(obj))
                        {
                            args.Add(item);
                        }
                        args.Add($"errorMask: errorMaskBuilder");
                    }
                    fg.AppendLine($"errorMask = {obj.Mask(MaskType.Error)}.Factory(errorMaskBuilder);");
                }
                fg.AppendLine();

                foreach (var minorAPI in this.MinorAPIs)
                {
                    using (var args = new FunctionWrapper(fg,
                        $"public virtual void Write_{ModuleNickname}{obj.Mask_GenericClause(MaskType.Error)}",
                        wheres: obj.GenericTypes_ErrorMaskWheres))
                    {
                        foreach (var item in minorAPI.WriterAPI.IterateAPI(obj,
                            $"out {obj.Mask(MaskType.Error)} errorMask",
                            "bool doMasks = true"))
                        {
                            if (!item.Public) continue;
                            args.Add(item.API);
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
                                foreach (var item in this.MainAPI.WrapWriterAccessors(obj, accessor))
                                {
                                    args.Add(item);
                                }
                                args.Add($"errorMask: out errorMask");
                                args.Add("doMasks: doMasks");
                            }
                        });
                    }
                    fg.AppendLine();
                }

                if (obj.Abstract)
                {
                    if (!obj.BaseClass?.Abstract ?? true)
                    {
                        foreach (var api in this.AllAPI)
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"public abstract void Write_{ModuleNickname}"))
                            {
                                foreach (var (API, Public) in api.WriterAPI.IterateAPI(obj))
                                {
                                    if (Public)
                                    {
                                        args.Add(API);
                                    }
                                }
                            }
                        }
                        fg.AppendLine();
                    }
                }
                else if (obj.IsTopClass
                    || (!obj.Abstract && (obj.BaseClass?.Abstract ?? true)))
                {
                    using (var args = new FunctionWrapper(fg,
                        $"public{await obj.FunctionOverride()}void Write_{ModuleNickname}"))
                    {
                        foreach (var (API, Public) in this.MainAPI.WriterAPI.IterateAPI(obj))
                        {
                            if (Public)
                            {
                                args.Add(API);
                            }
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        using (var args = new ArgsWrapper(fg,
                            $"this.Write_{ModuleNickname}_Internal"))
                        {
                            foreach (var item in this.MainAPI.WriterPassArgs(obj))
                            {
                                args.Add(item);
                            }
                            foreach (var item in this.MainAPI.WriterInternalFallbackArgs(obj))
                            {
                                args.Add(item);
                            }
                            args.Add($"errorMask: null");
                        }
                    }
                    fg.AppendLine();

                    foreach (var minorAPI in this.MinorAPIs)
                    {
                        using (var args = new FunctionWrapper(fg,
                            $"public{await obj.FunctionOverride()}void Write_{ModuleNickname}"))
                        {
                            foreach (var (API, Public) in minorAPI.WriterAPI.IterateAPI(obj))
                            {
                                if (Public)
                                {
                                    args.Add(API);
                                }
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
                                    foreach (var item in this.MainAPI.WrapWriterAccessors(obj, accessor))
                                    {
                                        args.Add(item);
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
                            foreach (var api in this.AllAPI)
                            {
                                using (var args = new FunctionWrapper(fg,
                                    $"public override void Write_{ModuleNickname}{obj.BaseMask_GenericClause(MaskType.Error)}"))
                                {
                                    foreach (var item in api.WriterAPI.MajorAPI)
                                    {
                                        if (item.TryResolve(obj, out var line))
                                        {
                                            args.Add(line);
                                        }
                                    }
                                    args.Add($"out {baseClass.Mask(MaskType.Error)} errorMask");
                                    args.Add($"bool doMasks = true");
                                    foreach (var item in api.WriterAPI.OptionalAPI)
                                    {
                                        if (item.TryResolve(obj, out var line))
                                        {
                                            args.Add(line);
                                        }
                                    }
                                }
                                using (new BraceWrapper(fg))
                                {
                                    using (var args = new ArgsWrapper(fg,
                                        $"Write_{this.ModuleNickname}"))
                                    {
                                        foreach (var item in api.WriterPassArgs(obj))
                                        {
                                            args.Add(item);
                                        }
                                        args.Add($"errorMask: out {obj.Mask_GenericAssumed(MaskType.Error, onlyAssumeSubclass: true)} errMask");
                                        args.Add("doMasks: doMasks");
                                    }
                                    fg.AppendLine("errorMask = errMask;");
                                }
                                fg.AppendLine();
                            }
                        }
                    }
                }

                using (var args = new FunctionWrapper(fg,
                    $"protected{await obj.FunctionOverride()}void Write_{ModuleNickname}_Internal"))
                {
                    foreach (var item in this.MainAPI.WriterAPI.IterateAPI(
                        obj,
                        $"ErrorMaskBuilder errorMask"))
                    {
                        args.Add(item.API);
                    }
                }
                using (new BraceWrapper(fg))
                {
                    string funcName = $"Write_{ModuleNickname}{(obj.HasNewGenerics ? obj.GenericTypes_SubTypeAssumedErrMask : obj.GenericTypes)}";
                    using (var args = new ArgsWrapper(fg,
                        $"{obj.ExtCommonName}.{funcName}"))
                    {
                        args.Add("item: this");
                        foreach (var item in this.MainAPI.WriterPassArgs(obj))
                        {
                            args.Add(item);
                        }
                        foreach (var item in this.MainAPI.WriterInternalPassArgs(obj))
                        {
                            args.Add(item);
                        }
                        args.Add("errorMask: errorMask");
                    }
                }

                if (obj.HasNewGenerics)
                {
                    fg.AppendLine();
                    using (var args = new FunctionWrapper(fg,
                        $"protected void Write_{ModuleNickname}_Internal"))
                    {
                        foreach (var item in this.MainAPI.WriterAPI.MajorAPI)
                        {
                            if (item.TryResolve(obj, out var line))
                            {
                                args.Add(line);
                            }
                        }
                        args.Add($"ErrorMaskBuilder errorMask");
                        foreach (var item in this.MainAPI.WriterAPI.OptionalAPI)
                        {
                            if (item.TryResolve(obj, out var line))
                            {
                                args.Add(line);
                            }
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        string funcName = $"Write_{ModuleNickname}{obj.GenericTypes_ErrMask}";
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
    }

    public abstract class TranslationModule<G> : TranslationModule
    {
        protected Dictionary<Type, G> _typeGenerations = new Dictionary<Type, G>();

        public TranslationModule(LoquiGenerator gen)
            : base(gen)
        {
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
