using Noggolloquy;
using Noggolloquy.Tests;

namespace Noggolloquy
{
    public class ProtocolDefinition_NoggolloquyTests : IProtocolRegistration
    {
        public readonly ProtocolKey ProtocolKey = new ProtocolKey(1);

        public void Register()
        {
            NoggolloquyRegistration.Register(
                new ObjectKey(ProtocolKey, 1, 0),
                new NoggolloquyTypeRegister(
                    classType: typeof(TestGenericObject<,>),
                    fullName: "TestGenericObject",
                    genericCount: 2,
                    objectKey: new ObjectKey(ProtocolKey, 1, 0)));
            NoggolloquyRegistration.Register(
                new ObjectKey(ProtocolKey, 2, 0),
                new NoggolloquyTypeRegister(
                    classType: typeof(TestObject),
                    fullName: "TestObject",
                    genericCount: 0,
                    objectKey: new ObjectKey(ProtocolKey, 2, 0)));
            NoggolloquyRegistration.Register(
                new ObjectKey(ProtocolKey, 3, 0),
                new NoggolloquyTypeRegister(
                    classType: typeof(ObjectToRef),
                    fullName: "ObjectToRef",
                    genericCount: 0,
                    objectKey: new ObjectKey(ProtocolKey, 3, 0)));
        }
    }
}
