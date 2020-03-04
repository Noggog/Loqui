using Noggog;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace Loqui.Generation
{
    public class BehaviorSetSubject<T> :
        ISubject<(T Item, bool HasBeenSet)>,
        ISubject<(T Item, bool HasBeenSet), (T Item, bool HasBeenSet)>,
        IObserver<(T Item, bool HasBeenSet)>,
        IObservable<(T Item, bool HasBeenSet)>,
        IDisposable
    {
        private readonly BehaviorSubject<(T Item, bool HasBeenSet)> _subject;
        public T Value => _subject.Value.Item;

        public bool HasBeenSet
        {
            get => this._subject.Value.HasBeenSet;
            set => this.OnNext(this.Value, true);
        }

        public BehaviorSetSubject(T defaultValue = default)
        {
            this._subject = new BehaviorSubject<(T Item, bool HasBeenSet)>((defaultValue, false));
        }

        public void Dispose()
        {
            _subject.Dispose();
        }

        public void OnCompleted()
        {
            _subject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }

        public void OnNext(T value)
        {
            _subject.OnNext((value, true));
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _subject
                .Select(i => i.Item)
                .Subscribe(observer);
        }

        public void Unset() => this.OnNext(default, hasBeenSet: false);

        public void OnNext((T Item, bool HasBeenSet) value)
        {
            this._subject.OnNext(value);
        }

        public void OnNext(T item, bool hasBeenSet = true)
        {
            this._subject.OnNext((item, hasBeenSet));
        }

        public void SetIfNotSet(T item, bool markAsSet = true)
        {
            if (this.HasBeenSet) return;
            this.OnNext(item, markAsSet);
        }

        public IDisposable Subscribe(IObserver<(T Item, bool HasBeenSet)> observer)
        {
            return this._subject.Subscribe(observer);
        }
    }
}
