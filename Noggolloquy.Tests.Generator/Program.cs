using Noggolloquy.Generation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noggolloquy.Tests.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            NoggolloquyGenerator gen = new NoggolloquyGenerator(
                new DirectoryInfo("../../../Noggolloquy.Tests"))
            {
                DefaultNamespace = "Noggolloquy.Tests",
                RaisePropertyChangedDefault = false
            };
            
            // Add Projects
            gen.AddProjectToModify(
                new FileInfo(Path.Combine(gen.CommonGenerationFolder.FullName, "Noggolloquy.Tests.csproj")));

            gen.AddProtocol(
                new ProtocolGeneration(
                    gen,
                    new ProtocolDefinition(
                        new ProtocolKey(1),
                        "NoggolloquyTests")));

            gen.Generate();
        }
    }
}
