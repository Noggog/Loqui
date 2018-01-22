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
        public override void GenerateCopyIn(FileGeneration fg, TypeGeneration typeGen, string nodeAccessor, Accessor itemAccessor, string doMaskAccessor, string maskAccessor)
        {
        }

        public override void GenerateCopyInRet(FileGeneration fg, TypeGeneration typeGen, string nodeAccessor, Accessor retAccessor, string doMaskAccessor, string maskAccessor)
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
            string doMaskAccessor, 
            string maskAccessor, 
            string nameAccessor)
        {
        }

        public override void GenerateForCommonXSD(XElement rootElement, TypeGeneration typeGen)
        {
        }
    }
}
