using Noggog;

namespace Loqui.Generation;

public class FilePathType : StringType
{
    public override Type Type(bool getter) => typeof(FilePath);
    public override bool IsReference => false;
}