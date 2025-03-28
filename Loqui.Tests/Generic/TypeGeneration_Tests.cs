using Loqui.Generation;
using System.Xml.Linq;
using Noggog.StructuredStrings;
using Xunit;

namespace Loqui.Tests;

public abstract class TypeGeneration_Tests<T>
    where T : TypeGeneration, new()
{
    public abstract T Thing { get; }

    public abstract TheoryData<XElement> ValidElements { get; }

    public abstract TheoryData<XElement> InvalidElements { get; }

    public static TheoryData<XElement> StaticInvalidElements => GetInstance().InvalidElements;
        
    public static TheoryData<XElement> StaticValidElements => GetInstance().ValidElements;

    protected static TypeGeneration_Tests<T> GetInstance()
    {
        static IEnumerable<Type> enumerable()
        {
            foreach (var domainAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (domainAssembly.IsDynamic) continue;
                foreach (var assemblyType in domainAssembly.GetExportedTypes())
                {
                    if (assemblyType.IsAbstract) continue;
                    if (typeof(TypeGeneration_Tests<T>).IsAssignableFrom(assemblyType))
                    {
                        yield return assemblyType;
                    }
                }
            }
        }

        return (TypeGeneration_Tests<T>)enumerable().Single().GetConstructors().Single().Invoke(null);
    }

    [Theory]
    [MemberData(nameof(StaticValidElements))]
    public async Task TestValidLoad(XElement valid)
    {
        await Thing.Load(valid);
    }

    [Theory]
    [MemberData(nameof(StaticInvalidElements))]
    public async Task TestInvalidLoad(XElement invalid)
    {
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await Thing.Load(invalid);
        });
    }

    public static TheoryData<string, string[]> CommentData => new()
    {
        { null, Array.Empty<string>() },
        { "An awesome comment", new string[] 
            { 
                "/// <summary>",
                "/// An awesome comment",
                "/// </summary>"
            } 
        }
    };

    private T ThingWithComments(string commentString)
    {
        var thing = Thing;
        if (commentString is not null)
            (thing.Comments ??= new()).Comments.Summary.AppendLine(commentString);
        return thing;
    }

    [Theory]
    [MemberData(nameof(CommentData))]
    public async Task TestClassComments(string commentString, string[] expected)
    {
        var thing = ThingWithComments(commentString);
        var sb = new StructuredStringBuilder();

        await thing.GenerateForClass(sb);

        Assert.Equal(expected, sb.Where(x => x.StartsWith("//")).ToArray());
    }
}