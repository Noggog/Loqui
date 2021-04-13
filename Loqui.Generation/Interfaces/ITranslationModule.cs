using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public interface ITranslationModule
    {
        string Namespace { get; }
        bool DoErrorMasks { get; }
        Task GenerateTranslationInterfaceImplementation(ObjectGeneration obj, FileGeneration fg);
        bool DoTranslationInterface(ObjectGeneration obj);
        void ReplaceTypeAssociation<Target, Replacement>()
            where Target : TypeGeneration
            where Replacement : TypeGeneration;
    }
}
