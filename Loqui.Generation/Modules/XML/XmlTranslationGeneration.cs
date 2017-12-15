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
            string writerAccessor,
            Accessor itemAccessor,
            string doMaskAccessor,
            string maskAccessor,
            string nameAccessor);

        public abstract void GenerateCopyIn(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor itemAccessor,
            string doMaskAccessor,
            string maskAccessor);

        public abstract void GenerateCopyInRet(
            FileGeneration fg,
            TypeGeneration typeGen,
            string nodeAccessor,
            Accessor retAccessor,
            string doMaskAccessor,
            string maskAccessor);

        public abstract XElement GenerateForXSD(
            XElement rootElement,
            XElement choiceElement,
            TypeGeneration typeGen,
            string nameOverride);
    }
}
