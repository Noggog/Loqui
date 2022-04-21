using Noggog;

namespace Loqui.Generation;

public class DirectoryPathNullType : StringType
{
    public override Type Type(bool getter) => typeof(DirectoryPath?);
}