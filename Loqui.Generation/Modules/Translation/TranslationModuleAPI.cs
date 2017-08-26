using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class TranslationModuleAPI
    {
        public string WriterAPI { get; private set; }
        public string ReaderAPI { get; private set; }
        public TranslationFunnel Funnel;

        public TranslationModuleAPI(string api)
        {
            this.WriterAPI = api;
            this.ReaderAPI = api;
        }

        public TranslationModuleAPI(
            string writerAPI,
            string readerAPI)
        {
            this.WriterAPI = writerAPI;
            this.WriterAPI = readerAPI;
        }
    }

    public class TranslationFunnel
    {
        public TranslationModuleAPI FunneledTo { get; private set; }
        public Action<FileGeneration, Action<string>> OutConverter { get; private set; }
        public Action<FileGeneration, Action<string>> InConverter { get; private set; }

        public TranslationFunnel(
            TranslationModuleAPI funnelTo,
            Action<FileGeneration, Action<string>> outConverter,
            Action<FileGeneration, Action<string>> inConverter)
        {
            this.FunneledTo = funnelTo;
            this.OutConverter = outConverter;
            this.InConverter = inConverter;
        }
    }
}
