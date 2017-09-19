using Loqui;
using Loqui.Tests;

namespace Loqui
{
    public class ProtocolDefinition_LoquiTests : IProtocolRegistration
    {
        public readonly static ProtocolKey ProtocolKey = new ProtocolKey("LoquiTests");
        public void Register()
        {
            LoquiRegistration.Register(Loqui.Tests.Internals.TestGenericObject_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_Notifying_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.ObjectToRef_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_Notifying_SubClass_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestGenericObject_SubClass_Defined_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestGenericObject_SubClass_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_HasBeenSet_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_ReadOnly_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_Notifying_ReadOnly_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_HasBeenSet_ReadOnly_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_Derivative_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_Notifying_Derivative_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_HasBeenSet_Derivative_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_PrivateCtor_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_Derivative_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_HasBeenSet_Derivative_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_HasBeenSet_ReadOnly_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_HasBeenSet_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_Notifying_Derivative_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_Notifying_ReadOnly_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_Notifying_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestObject_Abstract_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Internals.TestGenericSpecification_Registration.Instance);
        }
    }
}
