using System;
using System.Collections.Generic;

namespace Loqui.Generation
{
    /*
    * A Generation module is added once and affects all Loquis
    */
    public abstract class GenerationModule
    {
        public abstract string RegionString { get; }
        public abstract IEnumerable<string> RequiredUsingStatements();
        public abstract IEnumerable<string> Interfaces(ObjectGeneration obj);
        public abstract IEnumerable<string> GetWriterInterfaces(ObjectGeneration obj);
        public abstract IEnumerable<string> GetReaderInterfaces(ObjectGeneration obj);
        public virtual void PreLoad(ObjectGeneration obj)
        {

        }
        public virtual void PostLoad(ObjectGeneration obj)
        {

        }
        public abstract void Modify(LoquiGenerator gen);
        public abstract void GenerateInClass(ObjectGeneration obj, FileGeneration fg);
        public abstract void GenerateInCommonExt(ObjectGeneration obj, FileGeneration fg);
        public abstract void Generate(ObjectGeneration obj, FileGeneration fg);
        public abstract void GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg);
        public abstract void Generate(ObjectGeneration obj);
    }
}
