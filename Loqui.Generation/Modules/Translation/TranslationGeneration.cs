using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public abstract class TranslationGeneration
    {
        public MaskModule MaskModule;
        public virtual bool ShouldGenerateCopyIn(TypeGeneration typeGen) => typeGen.IntegrateField;
        public virtual bool ShouldGenerateWrite(TypeGeneration typeGen) => !typeGen.Derivative && typeGen.IntegrateField;
        public virtual string GetTranslationIfAccessor(TypeGeneration typeGen, string translationAccessor)
        {
            return $"({translationAccessor}?.GetShouldTranslate({typeGen.IndexEnumInt}) ?? true)";
        }

        public static bool IsParseInto(
            TypeGeneration typeGen,
            Accessor itemAccessor)
        {
            return itemAccessor != null
                && typeGen.PrefersProperty
                && itemAccessor.PropertyAccess != null;
        }

        public static void WrapParseCall(
            FileGeneration fg,
            TypeGeneration typeGen,
            string translatorLine,
            string maskAccessor,
            string indexAccessor,
            string translationMaskAccessor,
            Accessor itemAccessor,
            Action<FileGeneration> unsetCall,
            params string[] extraargs)
        {
            MaskGenerationUtility.WrapErrorFieldIndexPush(fg,
                () =>
                {
                    ArgsWrapper args;
                    bool parseInto = IsParseInto(typeGen, itemAccessor);
                    args = new ArgsWrapper(fg,
                       $"{(parseInto ? null : "if (")}{translatorLine}.{(parseInto ? "ParseInto" : "Parse")}",
                       suffixLine: (parseInto ? null : ")"))
                    {
                        SemiColon = parseInto,
                    };
                    using (args)
                    {
                        foreach (var extra in extraargs)
                        {
                            args.Add(extra);
                        }
                        if (parseInto)
                        {
                            args.Add($"item: {itemAccessor.PropertyAccess}");
                            if (indexAccessor != null)
                            {
                                args.Add($"fieldIndex: {indexAccessor}");
                            }
                        }
                        else
                        {
                            args.Add($"item: out {typeGen.TypeName} {typeGen.Name}Parse");
                        }
                        args.Add($"errorMask: {maskAccessor}");
                        if (translationMaskAccessor != null)
                        {
                            args.Add($"translationMask: {translationMaskAccessor}");
                        }
                    }
                    if (!parseInto)
                    {
                        using (new BraceWrapper(fg))
                        {
                            fg.AppendLine($"{itemAccessor.DirectAccess} = {typeGen.Name}Parse;");
                        }
                        fg.AppendLine("else");
                        using (new BraceWrapper(fg))
                        {
                            if (unsetCall != null)
                            {
                                unsetCall(fg);
                            }
                            else
                            {
                                if (typeGen.NotifyingType == NotifyingType.NotifyingItem)
                                {
                                    fg.AppendLine($"item.Unset{typeGen.Name}();");
                                }
                                else
                                {
                                    fg.AppendLine($"{itemAccessor.DirectAccess} = default({typeGen.TypeName});");
                                }
                            }
                        }
                    }
                },
                maskAccessor: maskAccessor,
                indexAccessor: indexAccessor,
                doIt: indexAccessor != null && !IsParseInto(typeGen, itemAccessor));
        }

        public static void WrapParseCall(
            FileGeneration fg,
            TypeGeneration typeGen,
            string translatorLine,
            string maskAccessor,
            string indexAccessor,
            string translationMaskAccessor,
            Accessor itemAccessor,
            params string[] extraargs)
        {
            WrapParseCall(
                fg: fg,
                typeGen: typeGen,
                translatorLine: translatorLine,
                maskAccessor: maskAccessor,
                indexAccessor: indexAccessor,
                itemAccessor: itemAccessor,
                translationMaskAccessor: translationMaskAccessor,
                unsetCall: null,
                extraargs: extraargs);
        }

        public static void WrapParseCall(
            FileGeneration fg,
            TypeGeneration typeGen,
            string translatorLine,
            string maskAccessor,
            string translationMaskAccessor,
            Accessor itemAccessor,
            params string[] extraargs)
        {
            WrapParseCall(
                fg: fg,
                typeGen: typeGen,
                translatorLine: translatorLine,
                maskAccessor: maskAccessor,
                indexAccessor: null,
                translationMaskAccessor: translationMaskAccessor,
                itemAccessor: itemAccessor,
                unsetCall: null,
                extraargs: extraargs);
        }

        public static void WrapParseCall(
            FileGeneration fg,
            TypeGeneration typeGen,
            string translatorLine,
            string maskAccessor,
            string translationMaskAccessor,
            Accessor itemAccessor,
            Action<FileGeneration> unsetCall,
            params string[] extraargs)
        {
            WrapParseCall(
                fg: fg,
                typeGen: typeGen,
                indexAccessor: null,
                translatorLine: translatorLine,
                maskAccessor: maskAccessor,
                itemAccessor: itemAccessor,
                translationMaskAccessor: translationMaskAccessor,
                unsetCall: unsetCall,
                extraargs: extraargs);
        }
    }
}
