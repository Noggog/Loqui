using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Loqui.Generation
{
    public class NothingXmlTranslationGeneration : XmlTranslationGeneration
    {
        public override string GetTranslatorInstance(TypeGeneration typeGen)
        {
            throw new NotImplementedException();
        }

        public override void GenerateCopyIn(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            string nodeAccessor, 
            Accessor itemAccessor,
            string maskAccessor,
            string translationMaskAccessor)
        {
        }

        public override void GenerateCopyInRet(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen, 
            string nodeAccessor, 
            Accessor retAccessor,
            string indexAccessor,
            string maskAccessor,
            string translationMaskAccessor)
        {
        }

        public override XElement GenerateForXSD(ObjectGeneration obj, XElement rootElement, XElement choiceElement, TypeGeneration typeGen, string nameOverride)
        {
            return null;
        }

        public override void GenerateWrite(
            FileGeneration fg,
            ObjectGeneration objGen,
            TypeGeneration typeGen,
            string writerAccessor, 
            Accessor itemAccessor, 
            string maskAccessor, 
            string nameAccessor,
            string translationMaskAccessor)
        {
        }

        public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
        {
        }
    }
}
