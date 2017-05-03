using Noggolloquy;
using Noggolloquy.Tests;

namespace Noggolloquy
{
    public class ProtocolDefinition_NoggolloquyTests : IProtocolRegistration
    {
        public readonly static ProtocolKey ProtocolKey = new ProtocolKey(1);
        public readonly static ProtocolDefinition Definition = new ProtocolDefinition(
            key: ProtocolKey,
            nickname: "NoggolloquyTests");

        public void Register()
        {
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestGenericObject_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_Notifying_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.ObjectToRef_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_Notifying_SubClass_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestGenericObject_SubClass_Defined_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestGenericObject_SubClass_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_HasBeenSet_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_ReadOnly_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_Notifying_ReadOnly_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_HasBeenSet_ReadOnly_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_Derivative_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_Notifying_Derivative_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_HasBeenSet_Derivative_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_PrivateCtor_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_RPC_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_Derivative_RPC_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_HasBeenSet_Derivative_RPC_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_HasBeenSet_ReadOnly_RPC_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_HasBeenSet_RPC_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_Notifying_Derivative_RPC_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_Notifying_ReadOnly_RPC_Registration.Instance);
            NoggolloquyRegistration.Register(Noggolloquy.Tests.Internals.TestObject_Notifying_RPC_Registration.Instance);
        }
    }
}
