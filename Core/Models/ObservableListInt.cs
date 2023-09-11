using System.ComponentModel;
using System.Collections.Generic;

namespace Kenedia.Modules.Core.Models
{
    public class ObservableList<T> : List<T>, INotifyPropertyChanged
    {
        public new T this[int i]
        {
            get => base[i];
            set
            {
                var oldValue = base[i];

                if (base[i].Equals(value)) return;
                base[i] = value;
                OnPropertyChanged(this, new($"{i}"));
                OnItemChanged(this, new(oldValue, value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event ValueChangedEventHandler<T> ItemChanged;

        public virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        public virtual void OnItemChanged(object sender, ValueChangedEventArgs<T> e)
        {
            ItemChanged?.Invoke(sender, e);
        }
    }
}
