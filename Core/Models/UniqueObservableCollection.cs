using System;
using System.Collections.ObjectModel;

namespace Kenedia.Modules.Core.Models
{
    public class UniqueObservableCollection<T> : ObservableCollection<T> where T : class
    {
        public event EventHandler<T> ItemAdded;
        public event EventHandler<T> ItemRemoved;

        protected override void InsertItem(int index, T item)
        {
            if (!Contains(item))
            {
                base.InsertItem(index, item);
                ItemAdded?.Invoke(this, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            T removedItem = this[index];
            base.RemoveItem(index);
            ItemRemoved?.Invoke(this, removedItem);
        }
    }
}
