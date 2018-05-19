using Noggog;
using Noggog.Notifying;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Internal
{
    public class ObjectCentralizationSubscriptions<T>
    {
        private FireDictionary<int, FireDictionary<WeakReferenceEquatable, FireList<NotifyingSetItemInternalCallback<T>>>> _subscriptions
            = new FireDictionary<int, FireDictionary<WeakReferenceEquatable, FireList<NotifyingSetItemInternalCallback<T>>>>();

        public void FireSubscriptions(
            int index,
            bool oldHasBeenSet,
            bool newHasBeenSet,
            T oldVal,
            T newVal,
            NotifyingFireParameters cmds)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(int index, object owner)
        {
            if (!_subscriptions.TryGetValue(index, out var subDict)) return;
            subDict.Remove(new WeakReferenceEquatable(owner));
        }
    }
}
