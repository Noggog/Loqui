namespace Loqui.Generation;

public class CommentCollection
{
    public readonly CommentWrapper Comments;
    public CommentWrapper SetterInterface;
    public CommentWrapper GetterInterface;

    public CommentCollection()
    {
        Comments = new CommentWrapper(null);
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