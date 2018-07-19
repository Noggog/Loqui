using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Noggog;
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
        public string[] WriterMemberNames(ObjectGeneration obj) => WriterAPI.IterateAPI(obj).Where((a) => a.Public).Select((r) => GetParameterName(r.API)).ToArray();
        public string[] WriterPassArgs (ObjectGeneration obj) => WrapAccessors(WriterMemberNames(obj), WriterMemberNames(obj)).ToArray();
        public string[] WriterInternalMemberNames (ObjectGeneration obj) => WriterAPI.CustomAPI.Where((a) => !a.Public).SelectWhere((r) => GetParameterName(r.API.Resolver(obj))).ToArray();
        public string[] WriterInternalFallbackArgs (ObjectGeneration obj) => WrapAccessors(WriterInternalMemberNames(obj), WriterAPI.CustomAPI.Where((a) => !a.Public).Select((r) => r.DefaultFallback).ToArray()).ToArray();
        public string[] WriterInternalPassArgs(ObjectGeneration obj) => WrapAccessors(WriterInternalMemberNames(obj), WriterInternalMemberNames(obj)).ToArray();
        public MethodAPI ReaderAPI { get; private set; }
        public string[] ReaderMemberNames (ObjectGeneration obj) => ReaderAPI.IterateAPI(obj).Where((a) => a.Public).Select((r) => GetParameterName(r.API)).ToArray();
        public string[] ReaderPassArgs (ObjectGeneration obj) => WrapAccessors(ReaderMemberNames(obj), ReaderMemberNames(obj)).ToArray();
        public string[] ReaderInternalMemberNames (ObjectGeneration obj) => ReaderAPI.CustomAPI.Where((a) => !a.Public).SelectWhere((r) => GetParameterName(r.API.Resolver(obj))).ToArray();
        public string[] ReaderInternalFallbackArgs (ObjectGeneration obj) => WrapAccessors(ReaderInternalMemberNames(obj), ReaderAPI.CustomAPI.Where((a) => !a.Public).Select((r) => r.DefaultFallback).ToArray()).ToArray();
        public string[] ReaderInternalPassArgs (ObjectGeneration obj) => WrapAccessors(ReaderInternalMemberNames(obj), ReaderInternalMemberNames(obj)).ToArray();
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

        private TryGet<string> GetParameterName(TryGet<string> api)
        {
            if (api.Failed) return api;
            return TryGet<string>.Succeed(GetParameterName(api.Value));
        }

        private string GetParameterName(string api)
        {
            var root = CSharpSyntaxTree.ParseText(api).GetRoot();
            var idents = root.DescendantNodes().OfType<VariableDeclaratorSyntax>();
            if (!idents.Any())
            {
                throw new ArgumentException("API given had no name");
            }
            var ident = idents.First();
            return ident.Identifier.Text;
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

        public IEnumerable<string> WrapReaderAccessors(ObjectGeneration obj, string[] accessors)
        {
            return WrapAccessors(
                this.ReaderMemberNames(obj),
                accessors);
        }

        public IEnumerable<string> WrapWriterAccessors(ObjectGeneration obj, string[] accessors)
        {
            return WrapAccessors(
                this.WriterMemberNames(obj),
                accessors);
        }
    }

    public class TranslationFunnel
    {
        public TranslationModuleAPI FunneledTo { get; private set; }
        public Action<ObjectGeneration, FileGeneration, InternalTranslation> OutConverter { get; private set; }
        public Action<ObjectGeneration, FileGeneration, InternalTranslation> InConverter { get; private set; }

        public TranslationFunnel(
            TranslationModuleAPI funnelTo,
            Action<ObjectGeneration, FileGeneration, InternalTranslation> outConverter,
            Action<ObjectGeneration, FileGeneration, InternalTranslation> inConverter)
        {
            this.FunneledTo = funnelTo;
            this.OutConverter = outConverter;
            this.InConverter = inConverter;
        }
    }
}
