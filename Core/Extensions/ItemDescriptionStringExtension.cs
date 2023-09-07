using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Kenedia.Modules.Core.Extensions
{
    public static class ItemDescriptionStringExtension
    {
        //public static Regex DescriptionRegEx = new("/<br>|<c[=@][@=]?([^>]+)>([^]*?)(<\\/?c\\/?>|$)/g]");
        public static Regex DescriptionRegEx = new(pattern: @"<br>|<c[=@][@=]?([^>]+)>([^]*?)(<\/?c\/?>|$])");

        public static string InterpretItemDescription(this string s)
        {
            if (s == null) return s;

            s = s.Replace("<c=@reminder>", Environment.NewLine);
            s = s.Replace("<c=@flavor>", Environment.NewLine);
            s = s.Replace("<c=@abilitytype>", string.Empty);
            s = s.Replace("<c=@Warning>", string.Empty);
            s = s.Replace("<br>", Environment.NewLine);
            s = s.Replace("</br>", string.Empty);
            s = s.Replace("</c>", string.Empty);

            return s;
        }
    }
}
