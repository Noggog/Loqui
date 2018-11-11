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

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RaiseAndSetIfChanged<T>(
            ref T item,
            T newItem,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(item, newItem)) return;

            item = newItem;
            this.RaisePropertyChanged(propertyName);
        }

        public void RaiseAndSetIfChanged<T>(
            ref T item,
            T newItem,
            ref bool hasBeenSet,
            bool newHasBeenSet,
            string name,
            string hasBeenSetName)
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

        public void RaiseAndSetIfChanged<T>(
            ref T item,
            T newItem,
            BitArray hasBeenSet,
            bool newHasBeenSet,
            int index,
            string name,
            string hasBeenSetName)
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

        public void RaiseAndSetIfChanged(
            BitArray hasBeenSet,
            bool newHasBeenSet,
            int index,
            string name)
        {
            var oldHasBeenSet = hasBeenSet[index];
            if (oldHasBeenSet == newHasBeenSet) return;
            hasBeenSet[index] = newHasBeenSet;
            this.RaisePropertyChanged(name);
        }

    }
}
