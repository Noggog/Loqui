using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Loqui.Generation
{
    /*
    * A Generation module is added once and affects all Loquis
    */
    public abstract class GenerationModule
    {
        public abstract string RegionString { get; }
        public virtual IEnumerable<string> RequiredUsingStatements()
        {
            yield break;
        }
        public virtual IEnumerable<string> Interfaces(ObjectGeneration obj)
        {
            yield break;
        }
        public virtual IEnumerable<string> GetWriterInterfaces(ObjectGeneration obj)
        {
            yield break;
        }
        public virtual IEnumerable<string> GetReaderInterfaces(ObjectGeneration obj)
        {
            yield break;
        }
        public virtual void PreLoad(ObjectGeneration obj)
        {

        }
        public virtual void PostLoad(ObjectGeneration obj)
        {

        }
        public virtual void PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node)
        {

        }
        public virtual void Modify(LoquiGenerator gen)
        {

        }
        public virtual void GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {

        }
        public virtual void GenerateInCommonExt(ObjectGeneration obj, FileGeneration fg)
        {

        }
        public virtual void Generate(ObjectGeneration obj, FileGeneration fg)
        {

        }
        public virtual void GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg)
        {

        }
        public virtual void Generate(ObjectGeneration obj)
        {

        }
    }
}
