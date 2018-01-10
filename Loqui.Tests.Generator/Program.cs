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
            LoquiGenerator gen = new LoquiGenerator()
            {
                RaisePropertyChangedDefault = false,
            };
            gen.XmlTranslation.ShouldGenerateXSD = false;

            var proto = gen.AddProtocol(
                new ProtocolGeneration(
                    gen,
                    new ProtocolKey("LoquiTests"),
                    new DirectoryInfo("../../../Loqui.Tests"))
                {
                    DefaultNamespace = "Loqui.Tests",
                });
            proto.AddProjectToModify(
                new FileInfo(Path.Combine(proto.GenerationFolder.FullName, "Loqui.Tests.csproj")));

            gen.Generate().Wait();
        }
    }
}
