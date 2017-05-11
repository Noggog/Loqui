using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noggolloquy.Generation
{
    public class NoggXmlTranslationGeneration : XmlTranslationGeneration
    {
        public override bool OutputsErrorMask => true;
        UnsafeXmlTranslationGeneration unsafeXml = new UnsafeXmlTranslationGeneration();

        public override void GenerateWrite(
            FileGeneration fg,
            TypeGeneration typeGen,
            string writerAccessor,
            string itemAccessor,
            string maskAccessor,
            string nameAccessor)
        {
            var noggGen = typeGen as NoggType;
            var refObjGen = noggGen.ObjectGeneration;
            if (refObjGen != null)
            {
                using (var args = new ArgsWrapper(fg,
                    $"{refObjGen.ExtCommonName}.Write_XML"))
                {
                    args.Add($"writer: {writerAccessor}");
                    args.Add($"item: {(typeGen.Name != null ? $"{itemAccessor}.{typeGen.Name}" : $"{itemAccessor}")}");
                    args.Add($"name: {nameAccessor}");
                    args.Add($"doMasks: doMasks");
                    args.Add($"errorMask: out {noggGen.ErrorMaskItemString} sub{maskAccessor}");
                }

                if (typeGen.Name != null)
                {
                    fg.AppendLine($"if (sub{maskAccessor} != null)");
                    using (new BraceWrapper(fg))
                    {
                        fg.AppendLine($"{maskAccessor}().SetNthMask((ushort){typeGen.IndexEnumName}, sub{maskAccessor});");
                    }
                }
                else
                {
                    fg.AppendLine($"{maskAccessor} = sub{maskAccessor};");
                }
            }
            else
            {
                unsafeXml.GenerateWrite(
                    fg: fg, 
                    typeGen: typeGen, 
                    writerAccessor: writerAccessor,
                    itemAccessor: itemAccessor,
                    maskAccessor: maskAccessor,
                    nameAccessor: nameAccessor);
            }
        }
    }
}
