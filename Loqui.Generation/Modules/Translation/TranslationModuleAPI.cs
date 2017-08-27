using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public delegate void InternalTranslation(params string[] accessors);
    public class TranslationModuleAPI
    {
        public MethodAPI WriterAPI { get; private set; }
        private string[] WriterMemberNames => WriterAPI.Select((r) => r.Split(' ')[1]).ToArray();
        public string[] WriterPassArgs => WrapAccessors(WriterMemberNames, WriterMemberNames).ToArray();
        public MethodAPI ReaderAPI { get; private set; }
        private string[] ReaderMemberNames => ReaderAPI.Select((r) => r.Split(' ')[1]).ToArray();
        public string[] ReaderPassArgs => WrapAccessors(ReaderMemberNames, ReaderMemberNames).ToArray();
        public TranslationFunnel Funnel;

        public TranslationModuleAPI(MethodAPI api)
        {
            this.WriterAPI = api;
            this.ReaderAPI = api;
        }

        public TranslationModuleAPI(
            MethodAPI writerAPI,
            MethodAPI readerAPI)
        {
            this.WriterAPI = writerAPI;
            this.ReaderAPI = readerAPI;
        }

        private IEnumerable<string> WrapAccessors(
            string[] memberNames,
            string[] accessors)
        {
            if (memberNames.Length != accessors.Length)
            {
                throw new ArgumentException();
            }
            for (int i = 0; i < memberNames.Length; i++)
            {
                yield return $"{memberNames[i]}: {accessors[i]}";
            }
        }

        public IEnumerable<string> WrapReaderAccessors(string[] accessors)
        {
            return WrapAccessors(
                this.ReaderMemberNames,
                accessors);
        }

        public IEnumerable<string> WrapWriterAccessors(string[] accessors)
        {
            return WrapAccessors(
                this.WriterMemberNames,
                accessors);
        }
    }

    public class TranslationFunnel
    {
        public TranslationModuleAPI FunneledTo { get; private set; }
        public Action<FileGeneration, InternalTranslation> OutConverter { get; private set; }
        public Action<FileGeneration, InternalTranslation> InConverter { get; private set; }

        public TranslationFunnel(
            TranslationModuleAPI funnelTo,
            Action<FileGeneration, InternalTranslation> outConverter,
            Action<FileGeneration, InternalTranslation> inConverter)
        {
            this.FunneledTo = funnelTo;
            this.OutConverter = outConverter;
            this.InConverter = inConverter;
        }
    }
}
