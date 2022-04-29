using Xunit;

namespace Loqui.Tests;

public class Comment_Tests
{
    [Fact]
    public void EmptyComments()
    {
        var commentWrapper = new CommentWrapper(null);

        var sb = new StructuredStringBuilder();

        commentWrapper.Apply(sb);

        Assert.True(sb.Empty);
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

        var sb = new StructuredStringBuilder();

        commentWrapper.Apply(sb);

        var expected = new string[]
        {
            "/// <summary>",
            "/// An awesome summary.",
            "/// </summary>",
        };

        Assert.Equal(expected, sb);
    }

    [Fact]
    public void OneParameter()
    {
        var commentWrapper = new CommentWrapper(null);

        commentWrapper.AddParameter("name", "The name of the thing");

        var sb = new StructuredStringBuilder();

        commentWrapper.Apply(sb);

        var expected = new string[]
        {
            "/// <param name=\"name\">The name of the thing</param>",
        };

        Assert.Equal(expected, sb);
    }

    [Fact]
    public void TwoParameters()
    {
        var commentWrapper = new CommentWrapper(null);

        commentWrapper.AddParameter("name", "The name of the thing");
        commentWrapper.AddParameter("thing", "The thing of the thing");

        var sb = new StructuredStringBuilder();

        commentWrapper.Apply(sb);

        var expected = new string[]
        {
            "/// <param name=\"name\">The name of the thing</param>",
            "/// <param name=\"thing\">The thing of the thing</param>",
        };

        Assert.Equal(expected, sb);
    }

    [Fact]
    public void MultiLineParameter()
    {
        var commentWrapper = new CommentWrapper(null);

        var description = new StructuredStringBuilder();
        description.AppendLine("The name of");
        description.AppendLine("the thing");

        commentWrapper.Parameters["name"] = description;

        var sb = new StructuredStringBuilder();

        commentWrapper.Apply(sb);

        var expected = new string[]
        {
            "/// <param name=\"name\">",
            "/// The name of",
            "/// the thing",
            "/// </param>",
        };

        Assert.Equal(expected, sb);
    }

    [Fact]
    public void Returns()
    {
        var commentWrapper = new CommentWrapper(null);

        commentWrapper.Return.AppendLine("Awesomeness");

        var sb = new StructuredStringBuilder();

        commentWrapper.Apply(sb);

        var expected = new string[]
        {
            "/// <returns>Awesomeness</returns>",
        };

        Assert.Equal(expected, sb);
    }

    [Fact]
    public void MultiLineReturns()
    {
        var commentWrapper = new CommentWrapper(null);

        commentWrapper.Return.AppendLine("Awesomeness,");
        commentWrapper.Return.AppendLine("sheer awesomeness!");

        var sb = new StructuredStringBuilder();

        commentWrapper.Apply(sb);

        var expected = new string[]
        {
            "/// <returns>",
            "/// Awesomeness,",
            "/// sheer awesomeness!",
            "/// </returns>",
        };

        Assert.Equal(expected, sb);
    }

    [Fact]
    public void WriteOnDispose()
    {
        var sb = new StructuredStringBuilder();

        using (var commentWrapper = new CommentWrapper(sb))
        {
            commentWrapper.Return.AppendLine("Awesomeness,");
            var description = new StructuredStringBuilder();
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

        Assert.Equal(expected, sb);
    }

}