using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Noggolloquy.Tests
{
    public class NoggolloquyRegistration_Tests
    {
        [Fact]
        public void GetCreateFunc()
        {
            var func = NoggolloquyRegistration.GetCreateFunc<TestObject>();
            Assert.NotNull(func);
            Assert.IsType(typeof(Func<IEnumerable<KeyValuePair<ushort, object>>, TestObject>), func);
        }

        [Fact]
        public void GetCreateFunc_Generic()
        {
            var func = NoggolloquyRegistration.GetCreateFunc<TestGenericObject<bool, ObjectToRef>>();
            Assert.NotNull(func);
            Assert.IsType(typeof(Func<IEnumerable<KeyValuePair<ushort, object>>, TestGenericObject<bool, ObjectToRef>>), func);
        }

        [Fact]
        public void GetCopyInFunc()
        {
            var func = NoggolloquyRegistration.GetCopyInFunc<TestObject>();
            Assert.NotNull(func);
            Assert.IsType(typeof(Action<IEnumerable<KeyValuePair<ushort, object>>, TestObject>), func);
        }

        [Fact]
        public void GetCopyInFunc_Generic()
        {
            var func = NoggolloquyRegistration.GetCopyInFunc<TestGenericObject<bool, ObjectToRef>>();
            Assert.NotNull(func);
            Assert.IsType(typeof(Action<IEnumerable<KeyValuePair<ushort, object>>, TestGenericObject<bool, ObjectToRef>>), func);
        }
    }
}
