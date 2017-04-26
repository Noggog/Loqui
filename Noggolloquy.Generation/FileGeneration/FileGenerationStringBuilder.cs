using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noggolloquy.Generation
{
    public class FileGenerationStringBuilder : FileGeneration
    {
        private StringBuilder sb = new StringBuilder();

        public override void Append(string str)
        {
            sb.Append(str);
        }

        public override string GetString()
        {
            return sb.ToString();
        }
    }
}
