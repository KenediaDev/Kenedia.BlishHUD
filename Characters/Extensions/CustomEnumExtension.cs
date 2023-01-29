using Characters.Res;
using static Kenedia.Modules.Characters.Services.SettingsModel;

namespace Kenedia.Modules.Characters.Extensions
{
    public static class CustomEnumExtension
    {
        public static ESortType GetSortType(this string s)
        {
            if(s == string.Format(strings.SortBy, strings.Name))
            {
                return ESortType.SortByName;
            }
            else if (s == string.Format(strings.SortBy, strings.Tag))
            {
                return ESortType.SortByTag;
            }
            else if (s == string.Format(strings.SortBy, strings.Profession))
            {
                return ESortType.SortByProfession;
            }
            else if (s == string.Format(strings.SortBy, strings.LastLogin))
            {
                return ESortType.SortByLastLogin;
            }
            else if (s == string.Format(strings.SortBy, strings.Map))
            {
                return ESortType.SortByMap;
            }

            return ESortType.Custom;
        }

        public static string GetSortType(this ESortType st)
        {
            return st switch
            {
                ESortType.SortByName => string.Format(strings.SortBy, strings.Name),
                ESortType.SortByTag => string.Format(strings.SortBy, strings.Tag),
                ESortType.SortByProfession => string.Format(strings.SortBy, strings.Profession),
                ESortType.SortByLastLogin => string.Format(strings.SortBy, strings.LastLogin),
                ESortType.SortByMap => string.Format(strings.SortBy, strings.Map),
                ESortType.Custom => strings.Custom,
                _ => strings.Custom,
            };
        }

        public static ESortOrder GetSortOrder(this string s)
        {
            return s == strings.Descending ? ESortOrder.Descending : ESortOrder.Ascending;
        }

        public static string GetSortOrder(this ESortOrder so)
        {
            return so switch
            {
                ESortOrder.Ascending => strings.Ascending,
                ESortOrder.Descending => strings.Descending,
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
