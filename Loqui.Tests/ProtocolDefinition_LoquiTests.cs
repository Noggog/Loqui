using Loqui;

namespace Loqui
{
    public class ProtocolDefinition_LoquiTests : IProtocolRegistration
    {
        public readonly static ProtocolKey ProtocolKey = new ProtocolKey("LoquiTests");
        void IProtocolRegistration.Register() => Register();
        public static void Register()
        {
        }
    }
}
