using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class PrimitiveXmlTranslationGeneration<T> : XmlTranslationGeneration
    {
        public override bool OutputsErrorMask => false;
        private string typeName;

        public PrimitiveXmlTranslationGeneration(string typeName = null)
        {
            this.typeName = typeName ?? typeof(T).GetName().Replace("?", string.Empty);
        }

        public override void GenerateWrite(
            FileGeneration fg,
            TypeGeneration typeGen,
            string writerAccessor, 
            string itemAccessor,
            string maskAccessor,
            string nameAccessor)
        {
            using (var args = new ArgsWrapper(fg,
                $"{this.typeName}XmlTranslation.Instance.Write"))
            {
                args.Add(writerAccessor);
                args.Add(nameAccessor);
                args.Add($"{itemAccessor}{(typeGen.Name == null ? string.Empty : $".{typeGen.Name}")}");
            }
        }
    }
}
