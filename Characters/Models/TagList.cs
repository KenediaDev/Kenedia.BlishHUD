using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Kenedia.Modules.Characters.Models
{
    public class TagList : ObservableCollection<string>
    {
        private bool _fireEvent = true;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_fireEvent)
            {
                base.OnCollectionChanged(e);
            }
        }

        public void AddTag(string tag, bool fireEvent = true)
        {
            _fireEvent = fireEvent;
            Add(tag);
            _fireEvent = false;
        }
    }
}
