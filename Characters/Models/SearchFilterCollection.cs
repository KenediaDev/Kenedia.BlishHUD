using Blish_HUD;
using System.Collections.Generic;

namespace Kenedia.Modules.Characters.Models
{
    public class SearchFilterCollection : Dictionary<string, SearchFilter<Character_Model>>
    {
        public new void AddOrUpdate(string key, SearchFilter<Character_Model> value)
        {
            if (!ContainsKey(key))
            {
                base.Add(key, value);
                return;
            }

            Logger.GetLogger(typeof(SearchFilterCollection)).Debug($"Key {key} exists already. Updating filter '{key}' instead.");
            this[key] = value;
        }
    }
}