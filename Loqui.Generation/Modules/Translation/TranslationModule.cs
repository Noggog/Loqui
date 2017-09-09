using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public abstract class TranslationModule<G> : GenerationModule
    {
        public LoquiGenerator Gen;
        protected Dictionary<Type, G> _typeGenerations = new Dictionary<Type, G>();
        public abstract string ModuleNickname { get; }
        public override string RegionString => $"{ModuleNickname} Translation";
        public abstract string Namespace { get; }
        public IEnumerable<TranslationModuleAPI> AllAPI => MainAPI.And(MinorAPIs);
        public TranslationModuleAPI MainAPI;
        protected List<TranslationModuleAPI> MinorAPIs = new List<TranslationModuleAPI>();

        public TranslationModule(LoquiGenerator gen)
        {
            this.Gen = gen;
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
            }
            return true;
        }

        public G GetTypeGeneration(Type t)
        {
            return this._typeGenerations[t];
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

        public override IEnumerable<string> RequiredUsingStatements()
        {
            yield break;
        }

        public override void Modify(LoquiGenerator gen)
        {
        }

        public override void GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public override void Generate(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public override void Generate(ObjectGeneration obj)
        {
        }

        public override void GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
            if (!obj.Abstract)
            {
                GenerateCreate(obj, fg);
            }
            GenerateCopyIn(obj, fg);
            GenerateWrite(obj, fg);
        }

        protected abstract void GenerateCopyInSnippet(ObjectGeneration obj, FileGeneration fg, bool usingErrorMask);

        private void GenerateCopyIn(ObjectGeneration obj, FileGeneration fg)
        {
            if (obj is StructGeneration) return;
            if (this.MainAPI == null) return;

            using (new RegionWrapper(fg, $"{this.ModuleNickname} Copy In"))
            {
                using (var args = new FunctionWrapper(fg,
                    $"public{obj.FunctionOverride}void CopyIn_{ModuleNickname}"))
                {
                    foreach (var item in this.MainAPI.ReaderAPI)
                    {
                        args.Add(item);
                    }
                    args.Add("NotifyingFireParameters? cmds = null");
                }
                using (new BraceWrapper(fg))
                {
                    GenerateCopyInSnippet(obj, fg, usingErrorMask: false);
                }
                fg.AppendLine();

                using (var args = new FunctionWrapper(fg,
                    $"public virtual void CopyIn_{ModuleNickname}"))
                {
                    foreach (var item in this.MainAPI.ReaderAPI.API)
                    {
                        args.Add(item);
                    }
                    args.Add($"out {obj.ErrorMask} errorMask");
                    foreach (var item in this.MainAPI.ReaderAPI.OptionalAPI)
                    {
                        args.Add(item);
                    }
                    args.Add("NotifyingFireParameters? cmds = null");
                }
                using (new BraceWrapper(fg))
                {
                    GenerateCopyInSnippet(obj, fg, usingErrorMask: true);
                }
                fg.AppendLine();

                foreach (var minorAPI in this.MinorAPIs)
                {
                    using (var args = new FunctionWrapper(fg,
                        $"public void CopyIn_{ModuleNickname}"))
                    {
                        foreach (var item in minorAPI.ReaderAPI)
                        {
                            args.Add(item);
                        }
                        args.Add("NotifyingFireParameters? cmds = null");
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.InConverter(fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"this.CopyIn_{ModuleNickname}"))
                            using (new DepthWrapper(fg))
                            {
                                foreach (var item in this.MainAPI.WrapReaderAccessors(accessor))
                                {
                                    args.Add(item);
                                }
                                args.Add($"cmds: cmds");
                            }
                        });
                    }
                    fg.AppendLine();

                    using (var args = new FunctionWrapper(fg,
                        $"public void CopyIn_{ModuleNickname}"))
                    {
                        foreach (var item in minorAPI.ReaderAPI.API)
                        {
                            args.Add(item);
                        }
                        args.Add($"out {obj.ErrorMask} errorMask");
                        foreach (var item in minorAPI.ReaderAPI.OptionalAPI)
                        {
                            args.Add(item);
                        }
                        args.Add("NotifyingFireParameters? cmds = null");
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.InConverter(fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"this.CopyIn_{ModuleNickname}"))
                            using (new DepthWrapper(fg))
                            {
                                foreach (var item in this.MainAPI.WrapReaderAccessors(accessor))
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
                        $"public override void CopyIn_{ModuleNickname}"))
                    {
                        foreach (var item in this.MainAPI.ReaderAPI.API)
                        {
                            args.Add(item);
                        }
                        args.Add($"out {baseClass.ErrorMask} errorMask");
                        foreach (var item in this.MainAPI.ReaderAPI.OptionalAPI)
                        {
                            args.Add(item);
                        }
                        args.Add($"NotifyingFireParameters? cmds = null");
                    }
                    using (new BraceWrapper(fg))
                    {
                        using (var args = new ArgsWrapper(fg,
                            $"this.CopyIn_{ModuleNickname}"))
                        {
                            args.Add(this.MainAPI.ReaderPassArgs);
                            args.Add($"errorMask: out {obj.ErrorMask} errMask");
                            args.Add($"cmds: cmds");
                        }
                        fg.AppendLine("errorMask = errMask;");
                    }
                    fg.AppendLine();
                }
            }
        }

        protected abstract void GenerateCreateSnippet(ObjectGeneration obj, FileGeneration fg);

        private void GenerateCreate(ObjectGeneration obj, FileGeneration fg)
        {
            using (new RegionWrapper(fg, $"{this.ModuleNickname} Create"))
            {
                using (var args = new FunctionWrapper(fg,
                    $"public{obj.NewOverride}static {obj.ObjectName} Create_{ModuleNickname}"))
                {
                    foreach (var item in this.MainAPI.ReaderAPI)
                    {
                        args.Add(item);
                    }
                }
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg,
                        $"return Create_{ModuleNickname}"))
                    {
                        args.Add(this.MainAPI.ReaderPassArgs);
                        args.Add("doMasks: false");
                        args.Add("errorMask: out var errorMask");
                    }
                }
                fg.AppendLine();

                using (var args = new FunctionWrapper(fg,
                    $"public static {obj.ObjectName} Create_{ModuleNickname}"))
                {
                    foreach (var item in this.MainAPI.ReaderAPI)
                    {
                        args.Add(item);
                    }
                    args.Add($"out {obj.ErrorMask} errorMask");
                }
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg,
                        $"return Create_{ModuleNickname}"))
                    {
                        args.Add(this.MainAPI.ReaderPassArgs);
                        args.Add("doMasks: true");
                        args.Add("errorMask: out errorMask");
                    }
                }
                fg.AppendLine();

                using (var args = new FunctionWrapper(fg,
                    $"public static {obj.ObjectName} Create_{ModuleNickname}"))
                {
                    foreach (var item in this.MainAPI.ReaderAPI)
                    {
                        args.Add(item);
                    }
                    args.Add("bool doMasks");
                    args.Add($"out {obj.ErrorMask} errorMask");
                }
                using (new BraceWrapper(fg))
                {
                    GenerateCreateSnippet(obj, fg);
                }
                fg.AppendLine();

                foreach (var minorAPI in this.MinorAPIs)
                {
                    using (var args = new FunctionWrapper(fg,
                        $"public static {obj.ObjectName} Create_{ModuleNickname}"))
                    {
                        foreach (var item in minorAPI.ReaderAPI)
                        {
                            args.Add(item);
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.InConverter(fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"return Create_{ModuleNickname}"))
                            using (new DepthWrapper(fg))
                            {
                                foreach (var item in this.MainAPI.WrapReaderAccessors(accessor))
                                {
                                    args.Add(item);
                                }
                            }
                        });
                    }
                    fg.AppendLine();

                    using (var args = new FunctionWrapper(fg,
                        $"public static {obj.ObjectName} Create_{ModuleNickname}"))
                    {
                        foreach (var item in minorAPI.ReaderAPI)
                        {
                            args.Add(item);
                        }
                        args.Add($"out {obj.ErrorMask} errorMask");
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.InConverter(fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"return Create_{ModuleNickname}"))
                            using (new DepthWrapper(fg))
                            {
                                foreach (var item in this.MainAPI.WrapReaderAccessors(accessor))
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

        public override void GenerateInCommonExt(ObjectGeneration obj, FileGeneration fg)
        {
            using (new RegionWrapper(fg, $"{ModuleNickname} Write"))
            {
                CommonWrite(obj, fg);
            }
        }

        private void CommonWrite(ObjectGeneration obj, FileGeneration fg)
        {
            using (var args = new FunctionWrapper(fg,
                $"public static void Write_{ModuleNickname}{obj.GenericTypes}",
                obj.GenerateWhereClauses().ToArray()))
            {
                foreach (var item in this.MainAPI.WriterAPI.API)
                {
                    args.Add(item);
                }
                args.Add($"{obj.Getter_InterfaceStr} item");
                args.Add($"bool doMasks");
                args.Add($"out {obj.ErrorMask} errorMask");
                foreach (var item in this.MainAPI.WriterAPI.OptionalAPI)
                {
                    args.Add(item);
                }
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine($"{obj.ErrorMask} errMaskRet = null;");
                using (var args = new ArgsWrapper(fg,
                    $"Write_{ModuleNickname}_Internal"))
                {
                    foreach (var item in this.MainAPI.WriterPassArgs)
                    {
                        args.Add(item);
                    }
                    args.Add("item: item");
                    args.Add("doMasks: doMasks");
                    args.Add($"errorMask: doMasks ? () => errMaskRet ?? (errMaskRet = new {obj.ErrorMask}()) : default(Func<{obj.ErrorMask}>)");
                }
                fg.AppendLine($"errorMask = errMaskRet;");
            }
            fg.AppendLine();

            using (var args = new FunctionWrapper(fg,
                $"private static void Write_{ModuleNickname}_Internal{obj.GenericTypes}",
                obj.GenerateWhereClauses().ToArray()))
            {
                foreach (var item in this.MainAPI.WriterAPI.API)
                {
                    args.Add(item);
                }
                args.Add($"{obj.Getter_InterfaceStr} item");
                args.Add($"bool doMasks");
                args.Add($"Func<{obj.ErrorMask}> errorMask");
                foreach (var item in this.MainAPI.WriterAPI.OptionalAPI)
                {
                    args.Add(item);
                }
            }
            using (new BraceWrapper(fg))
            {
                fg.AppendLine("try");
                using (new BraceWrapper(fg))
                {
                    GenerateWriteSnippet(obj, fg);
                }
                fg.AppendLine("catch (Exception ex)");
                fg.AppendLine("when (doMasks)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine("errorMask().Overall = ex;");
                }
            }
        }

        protected abstract void GenerateWriteSnippet(ObjectGeneration obj, FileGeneration fg);

        private void GenerateWrite(ObjectGeneration obj, FileGeneration fg)
        {
            using (new RegionWrapper(fg, $"{this.ModuleNickname} Write"))
            {
                using (var args = new FunctionWrapper(fg,
                    $"public virtual void Write_{ModuleNickname}"))
                {
                    foreach (var item in this.MainAPI.WriterAPI.API)
                    {
                        args.Add(item);
                    }
                    args.Add($"out {obj.ErrorMask} errorMask");
                    foreach (var item in this.MainAPI.WriterAPI.OptionalAPI)
                    {
                        args.Add(item);
                    }
                }
                using (new BraceWrapper(fg))
                {
                    using (var args = new ArgsWrapper(fg,
                        $"{obj.ExtCommonName}.Write_{ModuleNickname}"))
                    {
                        foreach (var item in this.MainAPI.WriterPassArgs)
                        {
                            args.Add(item);
                        }
                        args.Add($"item: this");
                        args.Add($"doMasks: true");
                        args.Add($"errorMask: out errorMask");
                    }
                }
                fg.AppendLine();

                foreach (var minorAPI in this.MinorAPIs)
                {
                    using (var args = new FunctionWrapper(fg,
                        $"public virtual void Write_{ModuleNickname}"))
                    {
                        foreach (var item in minorAPI.WriterAPI.API)
                        {
                            args.Add(item);
                        }
                        args.Add($"out {obj.ErrorMask} errorMask");
                        foreach (var item in minorAPI.WriterAPI.OptionalAPI)
                        {
                            args.Add(item);
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        minorAPI.Funnel.OutConverter(fg, (accessor) =>
                        {
                            using (var args = new ArgsWrapper(fg,
                                $"Write_{ModuleNickname}"))
                            using (new DepthWrapper(fg))
                            {
                                foreach (var item in this.MainAPI.WrapWriterAccessors(accessor))
                                {
                                    args.Add(item);
                                }
                                args.Add($"errorMask: out errorMask");
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
                                foreach (var item in api.WriterAPI)
                                {
                                    args.Add(item);
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
                        $"public{obj.FunctionOverride}void Write_{ModuleNickname}"))
                    {
                        foreach (var item in this.MainAPI.WriterAPI)
                        {
                            args.Add(item);
                        }
                    }
                    using (new BraceWrapper(fg))
                    {
                        using (var args = new ArgsWrapper(fg,
                            $"{obj.ExtCommonName}.Write_{ModuleNickname}"))
                        {
                            foreach (var item in this.MainAPI.WriterPassArgs)
                            {
                                args.Add(item);
                            }
                            args.Add($"item: this");
                            args.Add($"doMasks: false");
                            args.Add($"errorMask: out {obj.ErrorMask} errorMask");
                        }
                    }
                    fg.AppendLine();

                    foreach (var minorAPI in this.MinorAPIs)
                    {
                        using (var args = new FunctionWrapper(fg,
                            $"public{obj.FunctionOverride}void Write_{ModuleNickname}"))
                        {
                            foreach (var item in minorAPI.WriterAPI)
                            {
                                args.Add(item);
                            }
                        }
                        using (new BraceWrapper(fg))
                        {
                            minorAPI.Funnel.OutConverter(fg, (accessor) =>
                            {
                                using (var args = new ArgsWrapper(fg,
                                    $"Write_{ModuleNickname}"))
                                using (new DepthWrapper(fg))
                                {
                                    foreach (var item in this.MainAPI.WrapWriterAccessors(accessor))
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
                                    $"public override void Write_{ModuleNickname}"))
                                {
                                    foreach (var item in api.WriterAPI.API)
                                    {
                                        args.Add(item);
                                    }
                                    args.Add($"out {baseClass.ErrorMask} errorMask");
                                    foreach (var item in api.WriterAPI.OptionalAPI)
                                    {
                                        args.Add(item);
                                    }
                                }
                                using (new BraceWrapper(fg))
                                {
                                    using (var args = new ArgsWrapper(fg,
                                        $"Write_{this.ModuleNickname}"))
                                    {
                                        foreach (var item in api.WriterPassArgs)
                                        {
                                            args.Add(item);
                                        }
                                        args.Add($"errorMask: out {obj.ErrorMask} errMask");
                                    }
                                    fg.AppendLine("errorMask = errMask;");
                                }
                                fg.AppendLine();
                            }
                        }
                    }
                }
            }
        }
    }
}
