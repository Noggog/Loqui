using Xunit;

namespace Loqui.Tests;

public class Comment_Tests
{
    [Fact]
    public void EmptyComments()
    {
        var commentWrapper = new CommentWrapper(null);

        var fg = new FileGeneration();

        commentWrapper.Apply(fg);

        Assert.True(fg.Empty);
    }

    [Fact]
    public void NoTargetEmpty()
    {
        var commentWrapper = new CommentWrapper(null);

        commentWrapper.Apply(null);

        Assert.True(true);
    }

    [Fact]
    public void NoTargetOneSummaryLine()
    {
        var commentWrapper = new CommentWrapper(null);

        commentWrapper.Summary.AppendLine("An awesome summary.");

        commentWrapper.Apply(null);

        Assert.True(true);
    }

    [Fact]
    public void OneSummaryLine()
    {
        var commentWrapper = new CommentWrapper(null);

        commentWrapper.Summary.AppendLine("An awesome summary.");

        var fg = new FileGeneration();

        commentWrapper.Apply(fg);

        var expected = new string[]
        {
            "/// <summary>",
            "/// An awesome summary.",
            "/// </summary>",
        };

        Assert.Equal(expected, fg);
    }

    [Fact]
    public void OneParameter()
    {
        var commentWrapper = new CommentWrapper(null);

        commentWrapper.AddParameter("name", "The name of the thing");

        var fg = new FileGeneration();

        commentWrapper.Apply(fg);

        var expected = new string[]
        {
            "/// <param name=\"name\">The name of the thing</param>",
        };

        Assert.Equal(expected, fg);
    }

    [Fact]
    public void TwoParameters()
    {
        var commentWrapper = new CommentWrapper(null);

        commentWrapper.AddParameter("name", "The name of the thing");
        commentWrapper.AddParameter("thing", "The thing of the thing");

        var fg = new FileGeneration();

        commentWrapper.Apply(fg);

        var expected = new string[]
        {
            "/// <param name=\"name\">The name of the thing</param>",
            "/// <param name=\"thing\">The thing of the thing</param>",
        };

        Assert.Equal(expected, fg);
    }

    [Fact]
    public void MultiLineParameter()
    {
        var commentWrapper = new CommentWrapper(null);

        var description = new FileGeneration();
        description.AppendLine("The name of");
        description.AppendLine("the thing");

        commentWrapper.Parameters["name"] = description;

        var fg = new FileGeneration();

        commentWrapper.Apply(fg);

        var expected = new string[]
        {
            "/// <param name=\"name\">",
            "/// The name of",
            "/// the thing",
            "/// </param>",
        };

        Assert.Equal(expected, fg);
    }

    [Fact]
    public void Returns()
    {
        var commentWrapper = new CommentWrapper(null);

        commentWrapper.Return.AppendLine("Awesomeness");

        var fg = new FileGeneration();

        commentWrapper.Apply(fg);

        var expected = new string[]
        {
            "/// <returns>Awesomeness</returns>",
        };

        Assert.Equal(expected, fg);
    }

    [Fact]
    public void MultiLineReturns()
    {
        var commentWrapper = new CommentWrapper(null);

        commentWrapper.Return.AppendLine("Awesomeness,");
        commentWrapper.Return.AppendLine("sheer awesomeness!");

        var fg = new FileGeneration();

        commentWrapper.Apply(fg);

        var expected = new string[]
        {
            "/// <returns>",
            "/// Awesomeness,",
            "/// sheer awesomeness!",
            "/// </returns>",
        };

        Assert.Equal(expected, fg);
    }

    [Fact]
    public void WriteOnDispose()
    {
        var fg = new FileGeneration();

        using (var commentWrapper = new CommentWrapper(fg))
        {
            commentWrapper.Return.AppendLine("Awesomeness,");
            var description = new FileGeneration();
            description.AppendLine("The name of");
            description.AppendLine("the thing");

            commentWrapper.Parameters["name"] = description;

            commentWrapper.Summary.AppendLine("An awesome summary.");
            commentWrapper.Return.AppendLine("sheer awesomeness!");

            commentWrapper.AddParameter("thing", "The thing of the thing");
        }

        var expected = new string[]
        {
            "/// <summary>",
            "/// An awesome summary.",
            "/// </summary>",
            "/// <param name=\"name\">",
            "/// The name of",
            "/// the thing",
            "/// </param>",
            "/// <param name=\"thing\">The thing of the thing</param>",
            "/// <returns>",
            "/// Awesomeness,",
            "/// sheer awesomeness!",
            "/// </returns>",
        };

        Assert.Equal(expected, fg);
    }

}