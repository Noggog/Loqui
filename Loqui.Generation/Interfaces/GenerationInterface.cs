using System;
using System.Collections.Generic;

namespace Loqui.Generation
{
    /*
    * A Generation interface is added to single Loquis to modify them
    */
    public abstract class GenerationInterface
    {
        public abstract string RegionString { get; }
        public abstract string KeyString { get; }
        public abstract IEnumerable<string> RequiredUsingStatements();
        public abstract IEnumerable<string> Interfaces(ObjectGeneration obj);
        public abstract void Modify(ObjectGeneration obj);
        public abstract void GenerateInClass(ObjectGeneration obj, FileGeneration fg);
        public abstract void Generate(ObjectGeneration obj, FileGeneration fg);
    }
}
