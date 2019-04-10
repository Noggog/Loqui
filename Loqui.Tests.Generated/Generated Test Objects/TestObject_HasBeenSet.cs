using Loqui.Tests.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Tests
{
    public partial class TestObject_HasBeenSet
    {
        public void SetGetterSingleton()
        {
            this._RefGetter_Singleton_Object.CopyFieldsFrom(ObjectToRef.TYPICAL_VALUE);
        }
    }
}
