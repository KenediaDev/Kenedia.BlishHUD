using System;
using System.Threading.Tasks;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.AdvancedBuildsManager.Views;
using Kenedia.Modules.Core.Extensions;
using System.Diagnostics;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.SkillConnectionEdit
{
    public class SkillSelector : Selector<SkillEntryControl, OldSkillConnection>
    {
        public SkillSelector(SkillConnectionEditor editor)
        {
            Editor = editor;
            Editor.ProfessionChanged += SkillConnectionEditor_ProfessionChanged;
        }

        public Action<SkillEntryControl, SkillControl> OnClickAction { get; set; }

        public SkillControl Anchor { get; set; }

        public SkillConnectionEditor Editor { get; }

        protected override void ApplyItems()
        {
            SelectionPanel.Children.Clear();

            foreach (var item in Items)
            {
                _ = new SkillEntryControl()
                {
                    Entry = item,
                    Parent = SelectionPanel,
                    Height = 40,
                    Width = SelectionPanel.Width - 20,
                    SkillConnection = item.Value,
                    OnClickAction = (arg1) =>
                    {
                        OnClickAction(arg1, Anchor);
                        if (Parent == Graphics.SpriteScreen) Hide();
                    },
                };
            }

            _ = FilterItems(string.Empty);
        }

        protected override async Task FilterItems(string obj)
        {
            if (!await WaitAndCatch()) return;

            string[] strings = obj.Split(',');

            string profString = Editor?.Profession.ToString();
            bool any = Editor?.Profession == null;

            foreach (SkillEntryControl item in SelectionPanel.Children)
            {
                item.Visible = 
                    (any || item.Skill?.Professions.Count == 1) &&
                    (any || item.Skill?.Professions.Contains(profString) == true) &&
                    (obj == string.Empty ||
                    item.Skill?.Name?.ToLower().ContainsAnyTrimmed(strings) == true ||
                    item.Skill?.Id.ToString().ToLower().ContainsAnyTrimmed(strings) == true);
            }

            SelectionPanel.SortChildren((SkillEntryControl a, SkillEntryControl b) =>
            {
                string name1 = a.Skill?.Name == null ? string.Empty : a.Skill.Name;
                string name2 = b.Skill?.Name == null ? string.Empty : b.Skill.Name;
                int id1 = a.Skill?.Id == null ? 0 : a.Skill.Id;
                int id2 = b.Skill?.Id == null ? 0 : b.Skill.Id;

                int r1 = name1.CompareTo(name2);
                int r2 = id1.CompareTo(id2);
                return r1 == 0 ? r2 - r1 : r1;
            });
            SelectionPanel.Invalidate();
        }

        private void SkillConnectionEditor_ProfessionChanged(object sender, Gw2Sharp.Models.ProfessionType? e)
        {
            _ = FilterItems(string.Empty);
        }
    }
}
