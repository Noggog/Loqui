using Loqui.Generation;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Loqui.Generation
{
    public interface IAPIItem
    {
        string NicknameKey { get; }
        APIResult Resolve(ObjectGeneration obj, Context context);
    }
}

namespace System
{
    public static class IAPIItemExt
    {
        public static APIResult GetParameterName(this IAPIItem api, ObjectGeneration obj, Context context)
        {
            var root = CSharpSyntaxTree.ParseText(api.Resolve(obj, context).Result).GetRoot();
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
