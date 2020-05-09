using System;
using System.Collections.Generic;
using System.Text;

namespace Loqui.Generation
{
    public class APIResult : IAPIItem
    {
        public string NicknameKey { get; }
        public string Result { get; }

        public APIResult(
            IAPIItem sourceLine,
            string result)
        {
            this.NicknameKey = sourceLine.NicknameKey;
            this.Result = result;
        }

        public APIResult(
            string nicknameKey,
            string result)
        {
            this.NicknameKey = nicknameKey;
            this.Result = result;
        }

        public APIResult Resolve(ObjectGeneration obj) => this;

        public override string ToString()
        {
            return this.Result;
        }
    }
}
