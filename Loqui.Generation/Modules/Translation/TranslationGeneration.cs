using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class TranslationGeneration
    {
        public MaskModule MaskModule;
        public virtual bool ShouldGenerateCopyIn(TypeGeneration typeGen) => typeGen.IntegrateField;
        public virtual bool ShouldGenerateWrite(TypeGeneration typeGen) => !typeGen.Derivative && typeGen.IntegrateField;

        public static void WrapParseCall(
            FileGeneration fg,
            TypeGeneration typeGen,
            string callLine,
            string maskAccessor,
            string indexAccessor,
            Accessor itemAccessor,
            Action<FileGeneration> unsetCall,
            params string[] extraargs)
        {
            MaskGenerationUtility.WrapErrorFieldIndexPush(fg,
                () =>
                {
                    WrapParseCall(
                        fg: fg,
                        typeGen: typeGen,
                        callLine: callLine,
                        maskAccessor: maskAccessor,
                        itemAccessor: itemAccessor,
                        extraargs: extraargs,
                        unsetCall: unsetCall);
                },
                maskAccessor: maskAccessor,
                indexAccessor: indexAccessor,
                doIt: indexAccessor != null);
        }

        public static void WrapParseCall(
            FileGeneration fg,
            TypeGeneration typeGen,
            string callLine,
            string maskAccessor,
            string indexAccessor,
            Accessor itemAccessor,
            params string[] extraargs)
        {
            MaskGenerationUtility.WrapErrorFieldIndexPush(fg,
                () =>
                {
                    WrapParseCall(
                        fg: fg,
                        typeGen: typeGen,
                        callLine: callLine,
                        maskAccessor: maskAccessor,
                        itemAccessor: itemAccessor,
                        extraargs: extraargs,
                        unsetCall: null);
                },
                maskAccessor: maskAccessor,
                indexAccessor: indexAccessor,
                doIt: indexAccessor != null);
        }

        public static void WrapParseCall(
            FileGeneration fg,
            TypeGeneration typeGen,
            string callLine,
            string maskAccessor,
            Accessor itemAccessor,
            params string[] extraargs)
        {
            WrapParseCall(
                fg: fg,
                typeGen: typeGen,
                callLine: callLine,
                maskAccessor: maskAccessor,
                itemAccessor: itemAccessor,
                unsetCall: null,
                extraargs: extraargs);
        }

        public static void WrapParseCall(
            FileGeneration fg,
            TypeGeneration typeGen,
            string callLine,
            string maskAccessor,
            Accessor itemAccessor,
            Action<FileGeneration> unsetCall,
            params string[] extraargs)
        {
            ArgsWrapper args;
            args = new ArgsWrapper(fg,
               $"if ({callLine}",
               suffixLine: ")")
            {
                SemiColon = false,
            };
            using (args)
            {
                foreach (var extra in extraargs)
                {
                    args.Add(extra);
                }
                args.Add($"item: out {typeGen.TypeName} {typeGen.Name}Parse");
                args.Add($"errorMask: {maskAccessor}");
            }
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
                    if (typeGen.PrefersProperty)
                    {
                        fg.AppendLine($"{itemAccessor.PropertyAccess}.Unset();");
                    }
                    else if (typeGen.Notifying == NotifyingType.ObjectCentralized)
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
    }
}
