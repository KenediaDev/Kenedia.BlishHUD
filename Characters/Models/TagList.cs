using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

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

        public void ForceCollectionChanged()
        {
            base.OnCollectionChanged(null);
        }

        public void AddTag(string tag, bool fireEvent = true)
        {
            if (!Contains(tag))
            {
                _fireEvent = fireEvent;
                Add(tag);
                _fireEvent = true;
            }
        }

        public void AddTags(IEnumerable<string> tags, bool fireEvent = true)
        {
            _fireEvent = false;
            string last = tags.LastOrDefault();

            foreach (string tag in tags)
            {
                if (fireEvent) _fireEvent = last == tag;

                if (!Contains(tag))
                {
                    Add(tag);
                }
            }

            _fireEvent = true;
        }
    }
}
