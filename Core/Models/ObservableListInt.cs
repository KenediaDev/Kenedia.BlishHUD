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
                if (base[i].Equals(value)) return;
                base[i] = value;
                OnPropertyChanged(this, new($"{i}"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }
    }
}
