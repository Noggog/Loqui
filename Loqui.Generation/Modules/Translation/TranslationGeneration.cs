using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class TranslationGeneration
    {
        public MaskModule MaskModule;
        public virtual bool ShouldGenerateCopyIn(TypeGeneration typeGen) => typeGen.IntegrateField;
        public virtual bool ShouldGenerateWrite(TypeGeneration typeGen) => !typeGen.Derivative && typeGen.IntegrateField;
        public virtual bool IsAsync(TypeGeneration gen, bool read) => false;

        public static bool IsParseInto(
            TypeGeneration typeGen,
            Accessor itemAccessor)
        {
            return itemAccessor != null
                && typeGen.PrefersProperty
                && itemAccessor.PropertyAccess != null;
        }

        public static void WrapParseCall(TranslationWrapParseArgs param)
        {
            MaskGenerationUtility.WrapErrorFieldIndexPush(param.FG,
                () =>
                {
                    ArgsWrapper args;
                    bool parseInto = IsParseInto(param.TypeGen, param.ItemAccessor);
                    bool doIf = !parseInto && param.UnsetCall != null;
                    if (param.AsyncMode == AsyncMode.Off)
                    {
                        string prefix = null;
                        if (doIf)
                        {
                            prefix = $"if (";
                        }
                        else if (!parseInto)
                        {
                            prefix = $"{param.ItemAccessor.DirectAccess}{param.ItemAccessor.AssignmentOperator}";
                        }
                        args = new ArgsWrapper(param.FG,
                           $"{prefix}{param.TranslatorLine}.{(parseInto ? "ParseInto" : "Parse")}{(param.Generic == null ? null : $"<{param.Generic}>")}",
                           suffixLine: (doIf ? ")" : null))
                        {
                            SemiColon = !doIf,
                        };
                    }
                    else if (param.AsyncMode == AsyncMode.Async)
                    {
                        args = new ArgsWrapper(param.FG,
                            $"{(doIf ? $"var {param.TypeGen.Name}Parse = " : null)}{Loqui.Generation.Utility.Await()}{param.TranslatorLine}.Parse{(parseInto ? "Into" : null)}{param.Generic}",
                            suffixLine: Loqui.Generation.Utility.ConfigAwait())
                        {
                            SemiColon = !doIf
                        };
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    using (args)
                    {
                        if (param.ExtraArgs != null)
                        {
                            foreach (var extra in param.ExtraArgs)
                            {
                                args.Add(extra);
                            }
                        }
                        if (parseInto)
                        {
                            args.Add($"item: {param.ItemAccessor.PropertyAccess}");
                            if (param.IndexAccessor != null && !param.SkipErrorMask)
                            {
                                args.Add($"fieldIndex: {param.IndexAccessor}");
                            }
                        }
                        else if (param.AsyncMode == AsyncMode.Off && doIf)
                        {
                            args.Add($"item: out {param.TypeOverride ?? param.TypeGen.TypeName(getter: false)} {param.TypeGen.Name}Parse");
                        }
                        if (!param.SkipErrorMask)
                        {
                            args.Add($"errorMask: {param.MaskAccessor}");
                        }
                        if (param.TranslationMaskAccessor != null)
                        {
                            args.Add($"translationMask: {param.TranslationMaskAccessor}");
                        }
                        if (!doIf && param.DefaultOverride != null)
                        {
                            args.Add($"defaultVal: {param.DefaultOverride}");
                        }
                    }
                    if (doIf)
                    {
                        if (param.AsyncMode == AsyncMode.Off)
                        {
                            using (new BraceWrapper(param.FG))
                            {
                                param.FG.AppendLine($"{param.ItemAccessor.DirectAccess} = {param.TypeGen.Name}Parse;");
                            }
                        }
                        else
                        {
                            param.FG.AppendLine($"if ({param.TypeGen.Name}Parse.Succeeded)");
                            using (new BraceWrapper(param.FG))
                            {
                                param.FG.AppendLine($"{param.ItemAccessor.DirectAccess} = {param.TypeGen.Name}Parse.Value;");
                            }
                        }
                        param.FG.AppendLine("else");
                        using (new BraceWrapper(param.FG))
                        {
                            if (param.UnsetCall != null)
                            {
                                param.UnsetCall(param.FG);
                            }
                            else
                            {
                                param.FG.AppendLine($"{param.ItemAccessor.DirectAccess} = {param.DefaultOverride ?? $"default({param.TypeGen.TypeName(getter: false)})"};");
                            }
                        }
                    }
                },
                errorMaskAccessor: param.MaskAccessor,
                indexAccessor: param.IndexAccessor,
                doIt: !param.SkipErrorMask
                    && param.IndexAccessor != null
                    && param.Do
                    && !IsParseInto(param.TypeGen, param.ItemAccessor));
        }

        public virtual void Load(ObjectGeneration obj, TypeGeneration field, XElement node)
        {
        }

        public class TranslationWrapParseArgs
        {
            public FileGeneration FG;
            public TypeGeneration TypeGen;
            public string TranslatorLine;
            public Accessor MaskAccessor;
            public Accessor TranslationMaskAccessor;
            public Accessor ItemAccessor;
            public Accessor IndexAccessor;
            public string TypeOverride;
            public AsyncMode AsyncMode;
            public string DefaultOverride;
            public Action<FileGeneration> UnsetCall;
            public IEnumerable<string> ExtraArgs;
            public bool SkipErrorMask;
            public string Generic;
            public bool Do = true;
        }
    }
}
