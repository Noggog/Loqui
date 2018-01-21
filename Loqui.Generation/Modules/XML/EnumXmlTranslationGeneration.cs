using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class EnumXmlTranslationGeneration : XmlTranslationGeneration
    {
        PrimitiveXmlTranslationGeneration<string> _subGen = new PrimitiveXmlTranslationGeneration<string>();
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
            var eType = typeGen as EnumType;

            using (var args = new ArgsWrapper(fg,
                $"EnumXmlTranslation<{eType.NoNullTypeName}>.Instance.Write"))
            {
                args.Add($"writer: {writerAccessor}");
                args.Add($"name: {nameAccessor}");
                args.Add($"item: {itemAccessor.PropertyOrDirectAccess}");
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
            var eType = typeGen as EnumType;
            var isProperty = itemAccessor.PropertyAccess != null;
            string prefix = isProperty ? $"{itemAccessor.PropertyAccess}.{nameof(HasBeenSetItemExt.SetIfSucceeded)}(" : $"{itemAccessor.DirectAccess} = ";
            using (var args = new ArgsWrapper(fg,
                $"{prefix}EnumXmlTranslation<{eType.NoNullTypeName}>.Instance.Parse",
                suffixLine: $"{(eType.Nullable ? string.Empty : $".Bubble((o) => o.Value)")}{(isProperty ? ")" : $".GetOrDefault({itemAccessor.DirectAccess})")}"))
            {
                args.Add(nodeAccessor);
                args.Add($"nullable: {eType.Nullable.ToString().ToLower()}");
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

        public override void GenerateCopyInRet(
            FileGeneration fg,
            TypeGeneration typeGen, 
            string nodeAccessor,
            Accessor retAccessor,
            string doMaskAccessor,
            string maskAccessor)
        {
            var eType = typeGen as EnumType;
            using (var args = new ArgsWrapper(fg,
                $"{retAccessor.DirectAccess}EnumXmlTranslation<{eType.NoNullTypeName}>.Instance.Parse{(eType.Nullable ? null : "NonNull")}"))
            {
                args.Add(nodeAccessor);
                args.Add($"doMasks: {doMaskAccessor}");
                args.Add($"errorMask: out {maskAccessor}");
            }
        }

        public override XElement GenerateForXSD(
            ObjectGeneration obj,
            XElement rootElement, 
            XElement choiceElement,
            TypeGeneration typeGen,
            string nameOverride = null)
        {
            return _subGen.GenerateForXSD(obj, rootElement, choiceElement, typeGen, nameOverride);
        }
    }
}
