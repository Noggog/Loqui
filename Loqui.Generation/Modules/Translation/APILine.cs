using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class APILine : IEquatable<APILine>, IAPIItem
    {
        public string NicknameKey { get; }
        public Func<ObjectGeneration, APIResult> Resolver { get; }
        public Func<ObjectGeneration, bool> When { get; }

        public APILine(
            string nicknameKey,
            Func<ObjectGeneration, string> resolver,
            Func<ObjectGeneration, bool> when = null)
        {
            this.NicknameKey = nicknameKey;
            this.Resolver = (obj) => new APIResult(this, resolver(obj));
            this.When = when ?? ((obj) => true);
        }

        public APILine(
            string nicknameKey,
            string resolutionString,
            Func<ObjectGeneration, bool> when = null)
        {
            this.NicknameKey = nicknameKey;
            this.Resolver = (obj) => new APIResult(this, resolutionString);
            this.When = when ?? ((obj) => true);
        }

        public bool TryResolve(ObjectGeneration obj, out APIResult line)
        {
            var get = this.When(obj);
            if (!get)
            {
                line = default;
                return false;
            }
            line = Resolver(obj);
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
            return HashHelper.GetHashCode(this.NicknameKey);
        }
    }
}
