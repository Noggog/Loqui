using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class UnsafeXmlTranslationGeneration : XmlTranslationGeneration
    {
        public string ErrMaskString = "object";

        public override void GenerateWrite(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            string writerAccessor,
            Accessor itemAccessor,
            string doMaskAccessor,
            string maskAccessor,
            string nameAccessor)
        {
            using (var args = new ArgsWrapper(fg,
                $"WildcardXmlTranslation.Instance.Write"))
            {
                args.Add($"writer: {writerAccessor}");
                args.Add($"name: {nameAccessor}");
                args.Add($"item: {itemAccessor.DirectAccess}");
                if (typeGen.HasIndex)
                {
                    args.Add($"fieldIndex: (int){typeGen.IndexEnumName}");
                    args.Add($"errorMask: {maskAccessor}");
                }
                else
                {
                    args.Add($"doMasks: {doMaskAccessor}");
                    args.Add($"errorMask: out {maskAccessor}");
                }
            }
        }

        public override void GenerateCopyIn(
            FileGeneration fg, 
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor itemAccessor,
            string doMaskAccessor,
            string maskAccessor)
        {
            GenerateCopyInRet(fg, typeGen, nodeAccessor, $"var tryGet = ", doMaskAccessor, maskAccessor);
            if (itemAccessor.PropertyAccess != null)
            {
                fg.AppendLine($"{itemAccessor.PropertyAccess}.{nameof(HasBeenSetItemExt.SetIfSucceeded)}(tryGet.Bubble<{typeGen.TypeName}>(i => ({typeGen.TypeName})i));");
            }
            else
            {
                fg.AppendLine($"if (tryGet.Succeeded)");
                using (new BraceWrapper(fg))
                {
                    fg.AppendLine($"{itemAccessor.DirectAccess} = ({typeGen.TypeName})tryGet.Value;");
                }
            }
        }

        public override void GenerateCopyInRet(
            FileGeneration fg, 
            TypeGeneration typeGen, 
            string nodeAccessor,
            string retAccessor,
            string doMaskAccessor,
            string maskAccessor)
        {
            UnsafeType unsafeType = typeGen as UnsafeType;
            using (var args = new ArgsWrapper(fg,
                $"var tryGet = WildcardXmlTranslation.Instance.Parse"))
            {
                args.Add($"root: {nodeAccessor}");
                args.Add($"doMasks: {doMaskAccessor}");
                args.Add($"maskObj: out {maskAccessor}");
            }
        }

        public override XElement GenerateForXSD(
            XElement rootElement, 
            XElement choiceElement, 
            TypeGeneration typeGen,
            string nameOverride = null)
        {
            return null;
        }
    }
}
