using Loqui;
using Loqui.Tests.Generated;

namespace Loqui
{
    public class ProtocolDefinition_LoquiTestsGenerated : IProtocolRegistration
    {
        public readonly static ProtocolKey ProtocolKey = new ProtocolKey("LoquiTestsGenerated");
        public void Register()
        {
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestGenericObject_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Notifying_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.ObjectToRef_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Notifying_SubClass_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestGenericObject_SubClass_Defined_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestGenericObject_SubClass_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_HasBeenSet_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_ReadOnly_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Notifying_ReadOnly_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_HasBeenSet_ReadOnly_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Derivative_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Notifying_Derivative_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_HasBeenSet_Derivative_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_PrivateCtor_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Derivative_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_HasBeenSet_Derivative_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_HasBeenSet_ReadOnly_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_HasBeenSet_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Notifying_Derivative_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Notifying_ReadOnly_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Notifying_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Abstract_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestGenericSpecification_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Notifying_HasBeenSet_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Notifying_HasBeenSet_Derivative_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Notifying_HasBeenSet_Derivative_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Notifying_HasBeenSet_ReadOnly_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Notifying_HasBeenSet_ReadOnly_RPC_Registration.Instance);
            LoquiRegistration.Register(Loqui.Tests.Generated.Internals.TestObject_Notifying_HasBeenSet_RPC_Registration.Instance);
        }
    }
}
