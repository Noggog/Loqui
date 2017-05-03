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
            string pathPrefix = "../../../Noggolloquy.Tests";

            NoggolloquyGenerator gen = new NoggolloquyGenerator(
                new DirectoryInfo(pathPrefix))
            {
                DefaultNamespace = "Noggolloquy.Tests",
                RaisePropertyChangedDefault = false
            };

            // Add Field Types
            gen.AddTypicalTypeAssociations();

            // Add Modules
            var xmlTransGen = new XmlTranslationGeneration();
            gen.Add(xmlTransGen);

            var maskModule = new MaskModule();
            gen.Add(maskModule);

            // Add interfaces

            // Add Projects
            gen.AddProjectToModify(
                new FileInfo(pathPrefix + "/Noggolloquy.Tests.csproj"));

            gen.AddProtocol(
                new ProtocolGeneration(
                    gen,
                    new ProtocolDefinition(
                        new ProtocolKey(1),
                        "NoggolloquyTests")));

            // Add Object Sources
            gen.AddSearchableFolder(
                new DirectoryInfo(pathPrefix + "/"));

            gen.Generate();
        }
    }
}
