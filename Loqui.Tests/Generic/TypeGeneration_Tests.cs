using Loqui.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Loqui.Tests
{
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
        public async void TestValidLoad(XElement valid)
        {
            await Thing.Load(valid);
        }

        [Theory]
        [MemberData(nameof(StaticInvalidElements))]
        public void TestInvalidLoad(XElement invalid)
        {
            Assert.Throws<AggregateException>(() =>
            {
                var task = Thing.Load(invalid);
                task.Wait();
            });
        }

        /* TODO figoure out how to associate the right output with the right classes
        [Theory]
        [InlineData("fred", "george", false, "fred.Equals(george)")]
        [InlineData("fred", "george",  true, "!fred.Equals(george)")]
        [InlineData("fred", "george", false, "fred == george")]
        [InlineData("fred", "george",  true, "fred != george")]
        public virtual void TestGenerateEqualsSnippet(string f1, string f2, bool negate, string expected)
        {
            var thing = Thing;
            var a1 = new Accessor(f1);
            var a2 = new Accessor(f2);
            var foo = thing.GenerateEqualsSnippet(a1, a2, negate);
            Assert.Equal(expected, foo);
        }
        */

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

        private T ThingWithInterfaceComments(string commentString, bool getter)
        {
            var thing = Thing;
            if (commentString is null) return thing;

            var comments = (thing.Comments ??= new());
            if (getter)
                (comments.GetterInterface ??= new(null!)).Summary.AppendLine(commentString);
            else
                (comments.SetterInterface ??= new(null!)).Summary.AppendLine(commentString);

            return thing;
        }

        [Theory]
        [MemberData(nameof(CommentData))]
        public async Task TestClassComments(string commentString, string[] expected)
        {
            var thing = ThingWithComments(commentString);
            var fg = new FileGeneration();

            await thing.GenerateForClass(fg);

            Assert.Equal(expected, fg.Where(x => x.StartsWith("//")).ToArray());
        }

        public static TheoryData<bool> InterfaceOptionsData => new()
        {
            true,
            false,
        };

        public static MatrixTheoryData<string, string[], bool> InterfaceCommentsData => new(CommentData, InterfaceOptionsData);

        [Theory]
        [MemberData(nameof(InterfaceCommentsData))]
        public async Task TestInterfaceComments(string commentString, string[] expected, bool getter)
        {
            var thing = ThingWithInterfaceComments(commentString, getter);
            var fg = new FileGeneration();

            thing.GenerateForInterface(fg, getter, false);

            Assert.Equal(expected, fg.Where(x => x.StartsWith("//")).ToArray());
        }
    }
}
