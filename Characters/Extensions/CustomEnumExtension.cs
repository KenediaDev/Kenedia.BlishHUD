using Kenedia.Modules.Characters.Res;
using static Kenedia.Modules.Characters.Services.Settings;

namespace Kenedia.Modules.Characters.Extensions
{
    public static class CustomEnumExtension
    {
        public static SortBy GetSortType(this string s)
        {
            if(s == string.Format(strings.SortBy, strings.Name))
            {
                return SortBy.Name;
            }
            else if (s == string.Format(strings.SortBy, strings.Level))
            {
                return SortBy.Level;
            }
            else if (s == string.Format(strings.SortBy, strings.Race))
            {
                return SortBy.Race;
            }
            else if (s == string.Format(strings.SortBy, strings.Gender))
            {
                return SortBy.Gender;
            }
            else if (s == string.Format(strings.SortBy, strings.Profession))
            {
                return SortBy.Profession;
            }
            else if (s == string.Format(strings.SortBy, strings.Specialization))
            {
                return SortBy.Specialization;
            }
            else if (s == string.Format(strings.SortBy, strings.TimeSinceLogin))
            {
                return SortBy.TimeSinceLogin;
            }
            else if (s == string.Format(strings.SortBy, strings.Map))
            {
                return SortBy.Map;
            }
            else if (s == string.Format(strings.SortBy, strings.Tag))
            {
                return SortBy.Tag;
            }

            return SortBy.Custom;
        }

        public static string GetSortType(this SortBy st)
        {
            return st switch
            {
                SortBy.Name => string.Format(strings.SortBy, strings.Name),
                SortBy.Level => string.Format(strings.SortBy, strings.Level),
                SortBy.Race => string.Format(strings.SortBy, strings.Race),
                SortBy.Gender => string.Format(strings.SortBy, strings.Gender),
                SortBy.Profession => string.Format(strings.SortBy, strings.Profession),
                SortBy.Specialization => string.Format(strings.SortBy, strings.Specialization),
                SortBy.TimeSinceLogin => string.Format(strings.SortBy, strings.TimeSinceLogin),
                SortBy.Map => string.Format(strings.SortBy, strings.Map),
                SortBy.Tag => string.Format(strings.SortBy, strings.Tag),
                SortBy.Custom => strings.Custom,
                _ => strings.Custom,
            };
        }

        public static SortDirection GetSortOrder(this string s)
        {
            return s == strings.Descending ? SortDirection.Descending : SortDirection.Ascending;
        }

        public static string GetSortOrder(this SortDirection so)
        {
            return so switch
            {
                SortDirection.Ascending => strings.Ascending,
                SortDirection.Descending => strings.Descending,
                _ => strings.Ascending,
            };
        }

        public static FilterBehavior GetFilterBehavior(this string s)
        {
            return s == strings.ExcludeMatches ? FilterBehavior.Exclude : FilterBehavior.Include;
        }

        public static string GetFilterBehavior(this FilterBehavior fb)
        {
            return fb switch
            {
                FilterBehavior.Include => strings.IncludeMatches,
                FilterBehavior.Exclude => strings.ExcludeMatches,
                _ => strings.IncludeMatches,
            };
        }

        public static MatchingBehavior GetMatchingBehavior(this string s)
        {
            return s == strings.MatchAllFilter ? MatchingBehavior.MatchAll : MatchingBehavior.MatchAny;
        }

        public static string GetMatchingBehavior(this MatchingBehavior fb)
        {
            return fb switch
            {
                MatchingBehavior.MatchAny => strings.MatchAnyFilter,
                MatchingBehavior.MatchAll => strings.MatchAllFilter,
                _ => strings.MatchAnyFilter,
            };
        }

        public static string GetPanelSize(this PanelSizes s)
        {
            return s switch
            {
                PanelSizes.Small => strings.Small,
                PanelSizes.Normal => strings.Normal,
                PanelSizes.Large => strings.Large,
                PanelSizes.Custom => strings.Custom,
                _ => strings.Normal,
            };
        }

        public static PanelSizes GetPanelSize(this string s)
        {
            if(s == strings.Small)
            {
                return PanelSizes.Small;
            }
            else if(s == strings.Large)
            {
                return PanelSizes.Large;
            }
            else if(s == strings.Custom)
            {
                return PanelSizes.Custom;
            }

            return PanelSizes.Normal;
        }

        public static string GetPanelLayout(this CharacterPanelLayout layout)
        {
            return layout switch
            {
                CharacterPanelLayout.OnlyIcons => strings.OnlyIcons,
                CharacterPanelLayout.OnlyText => strings.OnlyText,
                CharacterPanelLayout.IconAndText => strings.TextAndIcon,
                _ => strings.TextAndIcon,
            };
        }

        public static CharacterPanelLayout GetPanelLayout(this string layout)
        {
            if(layout == strings.OnlyIcons)
            {
                return CharacterPanelLayout.OnlyIcons;
            }
            else if(layout == strings.OnlyText)
            {
                return CharacterPanelLayout.OnlyText;
            }

            return CharacterPanelLayout.IconAndText;
        }
    }
}
