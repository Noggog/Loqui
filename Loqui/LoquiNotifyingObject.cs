using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Loqui
{
    public class LoquiNotifyingObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedChecked([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaiseAndSetIfChanged<T>(
            ref T item,
            T newItem,
            [CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged == null)
            {
                item = newItem;
            }
            else
            {
                if (EqualityComparer<T>.Default.Equals(item, newItem)) return;
                item = newItem;
                this.RaisePropertyChanged(propertyName);
            }
        }

        protected void RaiseAndSetIfChanged<T>(
            ref T item,
            T newItem,
            ref bool hasBeenSet,
            bool newHasBeenSet,
            string name,
            string hasBeenSetName)
        {
            if (PropertyChanged == null)
            {
                item = newItem;
                hasBeenSet = newHasBeenSet;
            }
            else
            {
                if (!newHasBeenSet)
                {
                    this.RaiseAndSetIfChanged(ref hasBeenSet, newHasBeenSet, propertyName: hasBeenSetName);
                }
                this.RaiseAndSetIfChanged(ref item, newItem, propertyName: name);
                if (newHasBeenSet)
                {
                    this.RaiseAndSetIfChanged(ref hasBeenSet, newHasBeenSet, propertyName: hasBeenSetName);
                }
            }
        }

        protected void RaiseAndSetIfChanged<T>(
            ref T item,
            T newItem,
            BitArray hasBeenSet,
            bool newHasBeenSet,
            int index,
            string name,
            string hasBeenSetName)
        {
            if (PropertyChanged == null)
            {
                hasBeenSet[index] = newHasBeenSet;
                item = newItem;
            }
            else
            {
                var oldHasBeenSet = hasBeenSet[index];
                bool itemEqual = EqualityComparer<T>.Default.Equals(item, newItem);
                if (oldHasBeenSet != newHasBeenSet)
                {
                    hasBeenSet[index] = newHasBeenSet;
                }
                if (!itemEqual)
                {
                    item = newItem;
                    this.RaisePropertyChanged(name);
                }
                if (oldHasBeenSet != newHasBeenSet)
                {
                    this.RaisePropertyChanged(hasBeenSetName);
                }
            }
        }

        protected void RaiseAndSetIfChanged(
            BitArray hasBeenSet,
            bool newHasBeenSet,
            int index,
            string name)
        {
            if (PropertyChanged == null)
            {
                hasBeenSet[index] = newHasBeenSet;
            }
            else
            {
                var oldHasBeenSet = hasBeenSet[index];
                if (oldHasBeenSet == newHasBeenSet) return;
                hasBeenSet[index] = newHasBeenSet;
                this.RaisePropertyChanged(name);
            }
        }

    }
}
