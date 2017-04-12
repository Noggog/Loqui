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
                    errorMask: typeof(TestGenericObject_ErrorMask),
                    fullName: "TestGenericObject",
                    genericCount: 2,
                    objectKey: new ObjectKey(ProtocolKey, 1, 0)));
            NoggolloquyRegistration.Register(
                new ObjectKey(ProtocolKey, 2, 0),
                new NoggolloquyTypeRegister(
                    classType: typeof(TestObject),
                    errorMask: typeof(TestObject_ErrorMask),
                    fullName: "TestObject",
                    genericCount: 0,
                    objectKey: new ObjectKey(ProtocolKey, 2, 0)));
            NoggolloquyRegistration.Register(
                new ObjectKey(ProtocolKey, 3, 0),
                new NoggolloquyTypeRegister(
                    classType: typeof(ObjectToRef),
                    errorMask: typeof(ObjectToRef_ErrorMask),
                    fullName: "ObjectToRef",
                    genericCount: 0,
                    objectKey: new ObjectKey(ProtocolKey, 3, 0)));
        }
    }
}
