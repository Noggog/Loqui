using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Loqui.Tests
{
    public class LoquiRegistration_Tests
    {
        public static ObjectKey TestObjectKey = new ObjectKey(
            protocolKey: new ProtocolKey(1),
            msgID: 2,
            version: 0);

        public static ObjectKey GenericTestObjectKey = new ObjectKey(
            protocolKey: new ProtocolKey(1),
            msgID: 1,
            version: 0);

        [Fact]
        public void GetRegistration_ByType()
        {
            var registration = LoquiRegistration.GetRegister(typeof(TestObject_Notifying));
            Assert.NotNull(registration);
        }

        [Fact]
        public void GetRegistration_Key()
        {
            var registration = LoquiRegistration.GetRegister(TestObjectKey);
            Assert.NotNull(registration);
        }

        [Fact]
        public void GetRegistration_FullName()
        {
            var registration = LoquiRegistration.GetRegisterByFullName("Loqui.Tests.TestObject");
            Assert.NotNull(registration);
        }

        [Fact]
        public void GetRegistration_Generic_ByType()
        {
            var registration = LoquiRegistration.GetRegister(typeof(TestGenericObject<,,>));
            Assert.NotNull(registration);
        }

        [Fact]
        public void GetRegistration_Generic_Key()
        {
            var registration = LoquiRegistration.GetRegister(GenericTestObjectKey);
            Assert.NotNull(registration);
        }

        [Fact]
        public void GetRegistration_Generic_FullName()
        {
            var registration = LoquiRegistration.GetRegisterByFullName("Loqui.Tests.TestGenericObject");
            Assert.NotNull(registration);
        }

        [Fact]
        public void GetRegistration_GenericTyped_ByType()
        {
            var registration = LoquiRegistration.GetRegister(typeof(TestGenericObject<bool, ObjectToRef, ObjectToRef>));
            Assert.NotNull(registration);
        }

        [Fact]
        public void GetRegistration_GenericTyped_FullName()
        {
            var registration = LoquiRegistration.GetRegisterByFullName("Loqui.Tests.TestGenericObject<System.Boolean, Loqui.Tests.ObjectToRef, Loqui.Tests.ObjectToRef>");
            Assert.NotNull(registration);
        }

        [Fact]
        public void GetCreateFunc()
        {
            var func = LoquiRegistration.GetCreateFunc<TestObject_Notifying>();
            Assert.NotNull(func);
            Assert.IsType(typeof(Func<IEnumerable<KeyValuePair<ushort, object>>, TestObject_Notifying>), func);
        }

        [Fact]
        public void GetCreateFunc_Generic()
        {
            var func = LoquiRegistration.GetCreateFunc<TestGenericObject<bool, ObjectToRef, ObjectToRef>>();
            Assert.NotNull(func);
            Assert.IsType(typeof(Func<IEnumerable<KeyValuePair<ushort, object>>, TestGenericObject<bool, ObjectToRef, ObjectToRef>>), func);
        }

        [Fact]
        public void GetCopyInFunc()
        {
            var func = LoquiRegistration.GetCopyInFunc<TestObject_Notifying>();
            Assert.NotNull(func);
            Assert.IsType(typeof(Action<IEnumerable<KeyValuePair<ushort, object>>, TestObject_Notifying>), func);
        }

        [Fact]
        public void GetCopyInFunc_Generic()
        {
            var func = LoquiRegistration.GetCopyInFunc<TestGenericObject<bool, ObjectToRef, ObjectToRef>>();
            Assert.NotNull(func);
            Assert.IsType(typeof(Action<IEnumerable<KeyValuePair<ushort, object>>, TestGenericObject<bool, ObjectToRef, ObjectToRef>>), func);
        }

        //[Fact]
        //public void GetCopyFunc()
        //{
        //    var func = LoquiRegistration.GetCopyFunc<TestObject_Notifying>();
        //    Assert.NotNull(func);
        //    Assert.IsType(typeof(Func<TestObject_Notifying, object, object, TestObject_Notifying>), func);
        //}

        //[Fact]
        //public void GetCopyFunc_Generic()
        //{
        //    var func = LoquiRegistration.GetCopyFunc<TestGenericObject<bool, ObjectToRef, ObjectToRef>>();
        //    Assert.NotNull(func);
        //    Assert.IsType(typeof(Func<TestGenericObject<bool, ObjectToRef, ObjectToRef>, object, object, TestGenericObject<bool, ObjectToRef, ObjectToRef>>), func);
        //}
    }
}
