using Loqui.Generation;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loqui.Generation
{
    public interface IAPIItem
    {
        string NicknameKey { get; }
        APIResult Resolve(ObjectGeneration obj);
    }
}

namespace System
{
    public static class IAPIItemExt
    {
        public static APIResult GetParameterName(this IAPIItem api, ObjectGeneration obj)
        {
            var root = CSharpSyntaxTree.ParseText(api.Resolve(obj).Result).GetRoot();
            var idents = root.DescendantNodes().OfType<VariableDeclaratorSyntax>();
            if (!idents.Any())
            {
                throw new ArgumentException("API given had no name");
            }
            var ident = idents.First();
            return new APIResult(api, ident.Identifier.Text);
        }
    }
}