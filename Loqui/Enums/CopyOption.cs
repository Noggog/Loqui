namespace Loqui;

public enum CopyOption
{
    // Skip field and do nothing
    Skip,
    // Replace target with reference from the copy source
    Reference,
    // Copy fields into target
    CopyIn,
    // Make a copy and replace target
    MakeCopy
}