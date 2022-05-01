using Noggog.StructuredStrings;

namespace Loqui.Generation;

public class CommentCollection
{
    public readonly Comment Comments;
    public Comment SetterInterface;
    public Comment GetterInterface;

    public CommentCollection()
    {
        Comments = new Comment(null!);
    }

    public void Apply(StructuredStringBuilder sb, LoquiInterfaceType type)
    {
        switch (type)
        {
            case LoquiInterfaceType.Direct:
                Comments.Apply(sb);
                break;
            case LoquiInterfaceType.ISetter:
                (SetterInterface ?? Comments).Apply(sb);
                break;
            case LoquiInterfaceType.IGetter:
                (GetterInterface ?? Comments).Apply(sb);
                break;
            default:
                break;
        }
    }
}