using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Services;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Utility
{
    public class TemplateTagComparer : IComparer<TemplateTag>
    {
        public TemplateTagComparer(TagGroups tagGroups)
        {
            TagGroups = tagGroups;
        }

        public TagGroups TagGroups { get; }

        public static int CompareGroups(TagGroup? a, TagGroup? b)
        {
            return a is null && b is null ? 0
                : a is null ? 1
                : b is null ? -1
                : a.Priority.CompareTo(b.Priority) == 0 ? a.Name.CompareTo(b.Name) : a.Priority.CompareTo(b.Priority);
        }

        public static int CompareTags(TemplateTag? a, TemplateTag? b)
        {
            return a is null && b is null ? 0
                : a is null ? 1
                : b is null ? -1
                : a.Priority.CompareTo(b.Priority) == 0 ? a.Name.CompareTo(b.Name) : a.Priority.CompareTo(b.Priority);
        }

        public int Compare(TemplateTag x, TemplateTag y)
        {
            var xGroup = TagGroups.FirstOrDefault(group => group.Name == x.Group);
            var yGroup = TagGroups.FirstOrDefault(group => group.Name == y.Group);

            int grp = xGroup is not null && yGroup is not null ? CompareGroups(xGroup, yGroup) : 0;
            int tag = CompareTags(x, y);

            int result = grp == 0 ? tag : grp;

            //Compare Groups first then compare tags
            //return xGroup is null && yGroup is null ? CompareTags(x, y) : xGroup is null ? 1 : yGroup is null ? -1 : CompareGroups(xGroup, yGroup);
            return result;
        }
    }
}
