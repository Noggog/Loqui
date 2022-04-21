using Noggog;

namespace Loqui.Generation;

public class DirectoryPathType : StringType
{
    public override Type Type(bool getter) => typeof(DirectoryPath);
    public override bool IsReference => false;
}