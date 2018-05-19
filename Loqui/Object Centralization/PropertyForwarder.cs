using Noggog.Notifying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public class PropertyForwarder<O, T> : INotifyingSetItem<T>
        where O : IPropertySupporter<T>
    {
        private readonly O _item;
        private readonly int _index;

        public PropertyForwarder(O item, int index)
        {
            this._item = item;
            this._index = index;
        }

        public T Item
        {
            get => _item.Get(_index);
            set => _item.Set(index: _index, item: value, hasBeenSet: true, cmds: null);
        }

        public T DefaultValue => _item.DefaultValue(_index);

        public bool HasBeenSet
        {
            get => _item.GetHasBeenSet(_index);
            set => _item.SetHasBeenSet(_index, value);
        }

        T INotifyingSetItemGetter<T>.Item => this.Item;
        T INotifyingItemGetter<T>.Item => this.Item;
        T IHasItemGetter<T>.Item => this.Item;
        bool IHasBeenSet.HasBeenSet => this.HasBeenSet;

        public void Bind(object owner, INotifyingSetItem<T> rhs, NotifyingBindParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Bind<R>(object owner, INotifyingSetItem<R> rhs, Func<T, R> toConv, Func<R, T> fromConv, NotifyingBindParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Bind(INotifyingSetItem<T> rhs, NotifyingBindParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Bind<R>(INotifyingSetItem<R> rhs, Func<T, R> toConv, Func<R, T> fromConv, NotifyingBindParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Bind(object owner, INotifyingItem<T> rhs, NotifyingBindParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Bind<R>(object owner, INotifyingItem<R> rhs, Func<T, R> toConv, Func<R, T> fromConv, NotifyingBindParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Bind(INotifyingItem<T> rhs, NotifyingBindParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Bind<R>(INotifyingItem<R> rhs, Func<T, R> toConv, Func<R, T> fromConv, NotifyingBindParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Set(T item, bool hasBeenSet, NotifyingFireParameters cmds)
        {
            this._item.Set(_index, item, hasBeenSet, cmds);
        }

        public void Set(T value, NotifyingFireParameters cmds)
        {
            this._item.Set(_index, value, hasBeenSet: true, cmds: cmds);
        }

        public void Set(T item, bool hasBeenSet = true)
        {
            this._item.Set(_index, item, hasBeenSet: hasBeenSet, cmds: null);
        }

        public void SetCurrentAsDefault()
        {
            this._item.SetCurrentAsDefault(_index);
        }

        public void Subscribe(NotifyingSetItemSimpleCallback<T> callback, NotifyingSubscribeParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(object owner, NotifyingSetItemSimpleCallback<T> callback, NotifyingSubscribeParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Subscribe<O1>(O1 owner, NotifyingSetItemCallback<O1, T> callback, NotifyingSubscribeParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(Action callback, NotifyingSubscribeParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(object owner, Action callback, NotifyingSubscribeParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(NotifyingItemSimpleCallback<T> callback, NotifyingSubscribeParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(object owner, NotifyingItemSimpleCallback<T> callback, NotifyingSubscribeParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Subscribe<O1>(O1 owner, NotifyingItemCallback<O1, T> callback, NotifyingSubscribeParameters cmds = null)
        {
            throw new NotImplementedException();
        }

        public void Unset(NotifyingUnsetParameters cmds)
        {
            this._item.Unset(_index, cmds: cmds);
        }

        public void Unset()
        {
            this._item.Unset(_index, cmds: null);
        }

        public void Unsubscribe(object owner)
        {
            this._item.Unsubscribe(_index, owner);
        }
    }
}
