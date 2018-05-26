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
    class ObjectCentralizationSubscriptions
    {
        internal static readonly object NEVER_UNSUB = new object();
    }

    public class ObjectCentralizationSubscriptions<T>
    {
        // ToDo
        // Maybe swap dictionary to array if size can be known ahead of time, and mem costs lower.
        private readonly Dictionary<int, List<(WeakReference Owner, List<NotifyingSetItemInternalCallback<T>> Callbacks)>> _subscriptions
            = new Dictionary<int, List<(WeakReference, List<NotifyingSetItemInternalCallback<T>>)>>();
        private (int Index, WeakReference Owner, NotifyingSetItemInternalCallback<T>[] Callbacks)[] _fireSubscriptions;

        public void FireSubscriptions(
            int index,
            bool oldHasBeenSet,
            bool newHasBeenSet,
            T oldVal,
            T newVal,
            NotifyingFireParameters cmds)
        {
            (int Index, WeakReference Owner, NotifyingSetItemInternalCallback<T>[] Callbacks)[] subs;
            lock (_subscriptions)
            {
                if (_fireSubscriptions == null)
                {
                    _fireSubscriptions = new(int Index, WeakReference Owner, NotifyingSetItemInternalCallback<T>[] Callbacks)[_subscriptions.Count];
                    int i = 0;
                    foreach (var indexList in _subscriptions)
                    {
                        foreach (var (Owner, Callbacks) in indexList.Value)
                        {
                            _fireSubscriptions[i++] = (indexList.Key, Owner, Callbacks.ToArray());
                        }
                    }
                }
                subs = _fireSubscriptions;
            }

            if (subs.Length == 0) return;
            ChangeSet<T> changeSet = null;
            List<Exception> exceptions = null;
            foreach (var (Index, Owner, Callbacks) in subs)
            {
                if (Index != index) continue;
                if (!Owner.IsAlive) continue;
                foreach (var action in Callbacks)
                {
                    try
                    {
                        if (changeSet == null)
                        {
                            changeSet = new ChangeSet<T>(
                                oldVal: oldVal,
                                newVal: newVal,
                                oldSet: oldHasBeenSet,
                                newSet: newHasBeenSet);
                        }
                        action(Owner, changeSet);
                    }
                    catch (Exception ex)
                    {
                        if (exceptions == null)
                        {
                            exceptions = new List<Exception>();
                        }
                        exceptions.Add(ex);
                    }
                }
            }

            if (exceptions != null
                && exceptions.Count > 0)
            {
                Exception ex;
                if (exceptions.Count == 1)
                {
                    ex = exceptions[0];
                }
                else
                {
                    ex = new AggregateException(exceptions.ToArray());
                }

                if (cmds?.ExceptionHandler == null)
                {
                    throw ex;
                }
                else
                {
                    cmds.ExceptionHandler(ex);
                }
            }
        }

        public void Subscribe<O>(
            int index,
            object owner,
            O prop,
            NotifyingSetItemInternalCallback<T> callback,
            NotifyingSubscribeParameters cmds)
            where O : IPropertySupporter<T>
        {
            if (owner == null)
            {
                owner = ObjectCentralizationSubscriptions.NEVER_UNSUB;
            }
            lock (_subscriptions)
            {
                var indexList = _subscriptions.TryCreateValue(index);
                List<NotifyingSetItemInternalCallback<T>> callbacks = null;
                for (int i = indexList.Count - 1; i >= 0; i--)
                {
                    var refList = indexList[i];
                    if (refList.Owner.IsAlive)
                    {
                        if (callbacks != null && object.Equals(refList.Owner, owner))
                        {
                            callbacks = refList.Callbacks;
                            // No break.  Want to clean rest of dead owners
                        }
                    }
                    else
                    {
                        indexList.RemoveAt(i);
                    }
                }
                if (callbacks == null)
                {
                    callbacks = new List<NotifyingSetItemInternalCallback<T>>();
                    indexList.Add((new WeakReference(owner), callbacks));
                }
                callbacks.Add(callback);
                _fireSubscriptions = null;
            }
            cmds = cmds ?? NotifyingSubscribeParameters.Typical;
            if (cmds.FireInitial)
            {
                callback(owner, new ChangeSet<T>(
                    newVal: prop.Get(index),
                    newSet: prop.GetHasBeenSet(index)));
            }
        }

        public void Unsubscribe(int index, object owner)
        {
            lock (_subscriptions)
            {
                if (!_subscriptions.TryGetValue(index, out var indexList)) return;
                bool removed = false;
                for (int i = indexList.Count - 1; i >= 0; i--)
                {
                    var refList = indexList[i];
                    if (refList.Owner.IsAlive)
                    {
                        if (!removed && object.Equals(refList.Owner, owner))
                        {
                            removed = true;
                            indexList.RemoveAt(i);
                            // No break.  Want to clean rest of dead owners
                        }
                    }
                    else
                    {
                        indexList.RemoveAt(i);
                    }
                }
                if (indexList.Count == 0)
                {
                    _subscriptions.Remove(index);
                }
                _fireSubscriptions = null;
            }
        }
    }
}
