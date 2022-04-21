using Noggog;

namespace Loqui.Generation;

public class FilePathNullType : StringType
{
    public override Type Type(bool getter) => typeof(FilePath?);
}