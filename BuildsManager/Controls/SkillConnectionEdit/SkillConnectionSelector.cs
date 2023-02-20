using System;
using System.Threading.Tasks;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.BuildsManager.Views;

namespace Kenedia.Modules.BuildsManager.Controls.SkillConnectionEdit
{
    public class SkillConnectionSelector : Selector<SkillConnectionEntryControl, SkillConnection>
    {
        public SkillConnectionSelector(SkillConnectionEditor editor)
        {
            Editor = editor;
            Editor.ProfessionChanged += SkillConnectionEditor_ProfessionChanged;
        }

        private void SkillConnectionEditor_ProfessionChanged(object sender, Gw2Sharp.Models.ProfessionType e)
        {
            _ = FilterItems(string.Empty);
        }

        public Action<SkillConnectionEntryControl, SingleSkillChild> OnClickAction { get; set; }

        public SingleSkillChild Anchor { get; set; }

        public SkillConnectionEditor Editor { get; }

        protected override void ApplyItems()
        {
            SelectionPanel.Children.Clear();

            foreach (var item in Items)
            {
                _ = new SkillConnectionEntryControl()
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

            foreach (SkillConnectionEntryControl item in SelectionPanel.Children)
            {
                item.Visible = item.Skill?.Professions.Count == 1 && (item.Skill?.Professions.Contains(Editor?.Profession.ToString()) == true) && (obj == string.Empty || item.Skill?.Name.ToLower().Contains(obj.ToLower()) == true || item.Skill?.Id.ToString().ToLower().Contains(obj.ToLower()) == true);
            }

            SelectionPanel.SortChildren((SkillConnectionEntryControl a, SkillConnectionEntryControl b) =>
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
    }
}
