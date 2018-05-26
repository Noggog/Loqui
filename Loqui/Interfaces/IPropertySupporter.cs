using Noggog.Notifying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public interface IPropertySupporter<T>
    {
        T Get(int index);
        void Set(int index, T item, bool hasBeenSet, NotifyingFireParameters cmds);
        bool GetHasBeenSet(int index);
        void SetHasBeenSet(int index, bool on);
        T DefaultValue(int index);
        void Unset(int index, NotifyingUnsetParameters cmds);
        void SetCurrentAsDefault(int index);
        void Unsubscribe(int index, object owner);
        void Subscribe(int index, object owner, NotifyingSetItemInternalCallback<T> callback, NotifyingSubscribeParameters cmds);
    }
}
