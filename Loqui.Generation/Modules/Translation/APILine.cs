using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public delegate bool ApiWhen(ObjectGeneration objGen, TranslationDirection dir);
    public class APILine : IEquatable<APILine>, IAPIItem
    {
        public string NicknameKey { get; }
        public Func<ObjectGeneration, APIResult> Resolver { get; }
        public ApiWhen When { get; }

        public APILine(
            string nicknameKey,
            Func<ObjectGeneration, string> resolver,
            ApiWhen when = null)
        {
            this.NicknameKey = nicknameKey;
            this.Resolver = (obj) => new APIResult(this, resolver(obj));
            this.When = when ?? ((obj, input) => true);
        }

        public APILine(
            string nicknameKey,
            string resolutionString,
            ApiWhen when = null)
        {
            this.NicknameKey = nicknameKey;
            this.Resolver = (obj) => new APIResult(this, resolutionString);
            this.When = when ?? ((obj, input) => true);
        }

        public bool TryResolve(ObjectGeneration obj, TranslationDirection dir, out APIResult line)
        {
            var get = this.When(obj, dir);
            if (!get)
            {
                line = default;
                return false;
            }
            line = Resolver(obj);
            return true;
        }

        public bool TryGetParameterName(ObjectGeneration obj, TranslationDirection dir, out APIResult result)
        {
            var get = this.When(obj, dir);
            if (!get)
            {
                result = default;
                return false;
            }
            result = this.GetParameterName(obj);
            return true;
        }

        public bool TryGetPassthrough(ObjectGeneration obj, TranslationDirection dir, out string result)
        {
            var get = this.When(obj, dir);
            if (!get)
            {
                result = default;
                return false;
            }
            var name = this.GetParameterName(obj);
            result = $"{name}: {name}";
            return true;
        }

        public APIResult Resolve(ObjectGeneration obj) => this.Resolver(obj);

        public override bool Equals(object obj)
        {
            if (!(obj is APILine rhs)) return false;
            return obj.Equals(rhs);
        }

        public bool Equals(APILine other)
        {
            return string.Equals(this.NicknameKey, other.NicknameKey);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.NicknameKey);
        }
    }
}
