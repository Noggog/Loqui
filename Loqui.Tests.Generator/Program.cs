using Loqui.Generation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Tests.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            LoquiGenerator gen = new LoquiGenerator(
                new DirectoryInfo("../../../Loqui.Tests"))
            {
                DefaultNamespace = "Loqui.Tests",
                RaisePropertyChangedDefault = false,
                ProtocolDefault = new ProtocolKey("LoquiTests")
            };
            gen.XmlTranslation.ShouldGenerateXSD = false;

            // Add Projects
            gen.AddProjectToModify(
                new FileInfo(Path.Combine(gen.CommonGenerationFolder.FullName, "Loqui.Tests.csproj")));

            gen.AddProtocol(
                new ProtocolGeneration(
                    gen,
                    gen.ProtocolDefault));

            gen.Generate();
        }
    }
}
