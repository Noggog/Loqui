using System;
using System.Collections.Generic;

namespace Noggolloquy.Generation
{
    /*
    * A Generation module is added once and affects all Noggolloquys
    */
    public abstract class GenerationModule
    {
        public abstract string RegionString { get; }
        public abstract IEnumerable<string> RequiredUsingStatements();
        public abstract IEnumerable<string> Interfaces(ObjectGeneration obj);
        public abstract IEnumerable<string> GetWriterInterfaces(ObjectGeneration obj);
        public abstract IEnumerable<string> GetReaderInterfaces(ObjectGeneration obj);
        public abstract void Modify(ObjectGeneration obj);
        public abstract void Modify(NoggolloquyGenerator gen);
        public abstract void GenerateInClass(ObjectGeneration obj, FileGeneration fg);
        public abstract void Generate(ObjectGeneration obj, FileGeneration fg);
        public abstract void GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg);
    }
}
