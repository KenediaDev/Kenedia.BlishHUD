using System;

namespace Kenedia.Modules.Characters.Models
{
    public class SearchFilter<T>
    {
        public bool IsEnabled { get; set; }

        public SearchFilter()
        {

        }

        public SearchFilter(Func<T, bool> check, bool enabled = false)
            : this()
        {
            Check = check;
            IsEnabled = enabled;
        }

        public bool CheckForMatch(T target)
        {
            return !IsEnabled || Check(target);
        }

        public Func<T, bool> Check;
    }

    public class FilterEntry
    {
        public FilterEntry(object entry)
        {
            Entry = entry;
        }

        public object Entry { get; set; }

        public int Threshold { get; set; }

        public bool Enabled { get; set; } = false;
    }
}
