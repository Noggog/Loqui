using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noggog.Notifying;

namespace Loqui.Tests.Generated
{
    public class ObjectToRef_CustomMade : IObjectToRef
    {
        public INotifyingSetItem<int> KeyField_Property { get; } = new NotifyingSetItem<int>();
        public int KeyField { get => KeyField_Property.Item; set => KeyField_Property.Item = value; }
        INotifyingSetItemGetter<int> IObjectToRefGetter.KeyField_Property => KeyField_Property;

        public INotifyingSetItem<bool> SomeField_Property { get; } = new NotifyingSetItem<bool>();
        public bool SomeField { get => SomeField_Property.Item; set => SomeField_Property.Item = value; }
        INotifyingSetItemGetter<bool> IObjectToRefGetter.SomeField_Property => SomeField_Property;

        public ILoquiRegistration Registration => ObjectToRef.Registration;
    }
}
