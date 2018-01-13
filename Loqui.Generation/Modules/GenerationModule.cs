using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public virtual async Task PreLoad(ObjectGeneration obj)
        {
        }

        public virtual async Task PostLoad(ObjectGeneration obj)
        {
        }

        public virtual async Task PostFieldLoad(ObjectGeneration obj, TypeGeneration field, XElement node)
        {
        }

        public virtual async Task Modify(LoquiGenerator gen)
        {
        }

        public virtual async Task GenerateInStaticCtor(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public virtual async Task GenerateInClass(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public virtual async Task GenerateInCtor(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public virtual async Task GenerateInCommonExt(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public virtual async Task Generate(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public virtual async Task GenerateInInterfaceGetter(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public virtual async Task GenerateInRegistration(ObjectGeneration obj, FileGeneration fg)
        {
        }

        public virtual async Task Generate(ObjectGeneration obj)
        {
        }

        public virtual async Task Resolve(ObjectGeneration obj)
        {
        }
    }
}
