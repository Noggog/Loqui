using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public delegate void InternalTranslation(params IAPIItem[] accessors);
    public class TranslationModuleAPI
    {
        public MethodAPI WriterAPI { get; private set; }
        public MethodAPI ReaderAPI { get; private set; }
        private MethodAPI Get(TranslationDirection dir) => dir == TranslationDirection.Reader ? this.ReaderAPI : this.WriterAPI;
        public IEnumerable<APIResult> PublicMembers(ObjectGeneration obj, TranslationDirection dir) => Get(dir).IterateAPI(obj, dir).Where((a) => a.Public).Select((r) => r.API);
        public string[] PassArgs(ObjectGeneration obj, TranslationDirection dir) =>
            ZipAccessors(
                PublicMembers(obj, dir), 
                PublicMembers(obj, dir))
            .Select(api => 
                CombineResults(
                    api.lhs.GetParameterName(obj).Result,
                    api.rhs.GetParameterName(obj).Result))
            .ToArray();
        public IEnumerable<CustomMethodAPI> InternalMembers(ObjectGeneration obj, TranslationDirection dir) => Get(dir).CustomAPI.Where((a) => !a.Public).Where(o => o.API.When(obj, dir));
        public string[] InternalFallbackArgs(ObjectGeneration obj, TranslationDirection dir) =>
            InternalMembers(obj, dir).Select(custom =>
                CombineResults(
                    custom.API.GetParameterName(obj),
                    custom.DefaultFallback))
            .ToArray();
        public string[] InternalPassArgs(ObjectGeneration obj, TranslationDirection dir) =>
            InternalMembers(obj, dir).Select(custom =>
                CombineResults(
                    custom.API.GetParameterName(obj),
                    custom.API.GetParameterName(obj)))
            .ToArray();
        public TranslationFunnel Funnel;

        public Func<ObjectGeneration, TranslationDirection, bool> When { get; set; }

        public TranslationModuleAPI(MethodAPI api, Func<ObjectGeneration, TranslationDirection, bool> when = null)
        {
            this.WriterAPI = api;
            this.ReaderAPI = api;
            this.When = when;
        }

        public TranslationModuleAPI(
            MethodAPI writerAPI,
            MethodAPI readerAPI,
            Func<ObjectGeneration, TranslationDirection, bool> when = null)
        {
            this.WriterAPI = writerAPI;
            this.ReaderAPI = readerAPI;
            this.When = when;
        }

        private IEnumerable<(T lhs, T rhs)> ZipAccessors<T>(
            IEnumerable<T> lhs,
            IEnumerable<T> rhs)
            where T : IAPIItem
        {
            if (lhs.Count() != rhs.Count())
            {
                throw new ArgumentException("Zip inputs did not have the same number of elements");
            }
            Dictionary<string, T> cache = new Dictionary<string, T>();
            foreach (var item in lhs)
            {
                cache.Add(item.NicknameKey, item);
            }

            foreach (var rhsItem in rhs.OrderBy(l => l.NicknameKey))
            {
                if (!cache.TryGetValue(rhsItem.NicknameKey, out var lhsItem))
                {
                    throw new ArgumentException();
                }
                yield return (lhsItem, rhsItem);
                cache.Remove(rhsItem.NicknameKey);
            }
        }

        private string CombineResults(
            APIResult lhs,
            APIResult rhs)
        {
            return CombineResults(lhs.Result, rhs.Result);
        }

        private string CombineResults(
            string lhs,
            string rhs)
        {
            return $"{lhs}: {rhs}";
        }

        private IEnumerable<string> WrapAccessors(
            APILine[] memberNames,
            APILine[] accessors)
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

        public IEnumerable<string> WrapAccessors(ObjectGeneration obj, TranslationDirection dir, IAPIItem[] accessors) =>
            ZipAccessors(
                Get(dir).IterateAPI(obj, dir).Where((a) => a.Public).Select(a => a.API),
                accessors.Select(api => api.Resolve(obj)))
            .Select(api =>
                CombineResults(
                    api.lhs.GetParameterName(obj),
                    api.rhs.GetParameterName(obj)));

        public IEnumerable<string> WrapFinalAccessors(ObjectGeneration obj, TranslationDirection dir, IAPIItem[] accessors) =>
            ZipAccessors(
                Get(dir).IterateAPI(obj, dir).Select(a => a.API),
                accessors
                    .Select(api => api.GetParameterName(obj))
                    .And(
                        this.Get(dir).CustomAPI
                        .Where(a => !a.Public)
                        .Where(a => a.API.When(obj, dir))
                        .Select(a => a.DefaultFallback)))
            .Select(api =>
                CombineResults(
                    api.lhs.GetParameterName(obj),
                    api.rhs));
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
