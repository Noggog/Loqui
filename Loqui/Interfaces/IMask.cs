namespace Loqui;

public interface IMask<T>
{
    bool All(Func<T, bool> eval);
    bool Any(Func<T, bool> eval);
}