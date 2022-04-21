namespace Loqui;

public class Initialization
{
    public static void SpinUp()
    {
        LoquiRegistration.SpinUp();
    }

    public static void SpinUp(params IProtocolRegistration[] registrations)
    {
        LoquiRegistrationSettings.AutomaticRegistration = false;
        LoquiRegistration.Register(registrations);
    }
}