using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public static class TranslationGenerationSnippets
    {
        public static void DirectTryGetSetting(
            FileGeneration fg,
            Accessor retAccessor,
            TypeGeneration typeGen)
        {
            if (!typeGen.PrefersProperty)
            {
                fg.AppendLine($"if ({typeGen.Name}tryGet.Succeeded)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{retAccessor.DirectAccess} = {typeGen.Name}tryGet.Value;");
                }
                fg.AppendLine("else");
                using (new BraceWrapper(fg))
                {
                    if (typeGen.Notifying && typeGen.ObjectCentralized)
                    {
                        using (var args = new ArgsWrapper(fg,
                            $"item.Unset{typeGen.Name}"))
                        {
                        }
                    }
                    else
                    {
                        fg.AppendLine($"{retAccessor.DirectAccess} = default({typeGen.TypeName});");
                    }
                }
            }
        }
    }
}
