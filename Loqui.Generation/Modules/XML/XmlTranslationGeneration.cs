using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public abstract class XmlTranslationGeneration : TranslationGeneration
    {
        public XmlTranslationModule XmlMod;

        public abstract void GenerateWrite(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            Accessor writerAccessor,
            Accessor itemAccessor,
            Accessor errorMaskAccessor,
            Accessor nameAccessor,
            Accessor translationMaskAccessor);

        public abstract void GenerateCopyIn(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            Accessor nodeAccessor,
            Accessor itemAccessor,
            Accessor errorMaskAccessor,
            Accessor translationMaskAccessor);

        public abstract string GetTranslatorInstance(TypeGeneration typeGen);

        public abstract void GenerateCopyInRet(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            Accessor nodeAccessor,
            Accessor retAccessor,
            Accessor indexAccessor,
            Accessor errorMaskAccessor,
            Accessor translationMaskAccessor);

        public abstract XElement GenerateForXSD(
            ObjectGeneration objGen,
            XElement rootElement,
            XElement choiceElement,
            TypeGeneration typeGen,
            string nameOverride);

        public abstract void GenerateForCommonXSD(
            XElement rootElement,
            TypeGeneration typeGen);
    }
}
