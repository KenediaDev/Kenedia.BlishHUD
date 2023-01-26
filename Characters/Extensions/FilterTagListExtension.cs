using Kenedia.Modules.Characters.Controls;
using System.Collections.Generic;

namespace Kenedia.Modules.Characters.Extensions
{
    public static class FilterTagListExtension
    {
        public static List<FilterTag> CreateFilterTagList(this List<string> strings)
        {
            List<FilterTag> list = new();
            foreach (string s in strings)
            {
                list.Add(new FilterTag()
                {
                    Tag = s,
                });
            }

            return list;
        }
    }
}
