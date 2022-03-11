using Noggog;

namespace Loqui.Generation;

public class Array2dMaskFieldGeneration : ContainerMaskFieldGeneration
{
    public override string IndexStr => nameof(P2Int);
}