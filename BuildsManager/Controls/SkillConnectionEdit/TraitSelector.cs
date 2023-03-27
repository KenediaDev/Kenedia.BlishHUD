using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Views;
using Kenedia.Modules.Core.Extensions;
using System;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Controls.SkillConnectionEdit
{
    public class TraitSelector : Selector<TraitEntryControl, Trait>
    {
        public TraitSelector(SkillConnectionEditor editor)
        {
            Editor = editor;
            Editor.ProfessionChanged += SkillConnectionEditor_ProfessionChanged;
        }

        public Action<TraitEntryControl, TraitControl> OnClickAction { get; set; }

        public TraitControl Anchor { get; set; }

        public SkillConnectionEditor Editor { get; }

        protected override void ApplyItems()
        {
            SelectionPanel.Children.Clear();

            foreach (var item in Items)
            {
                _ = new TraitEntryControl()
                {
                    Entry = item,
                    Parent = SelectionPanel,
                    Height = 40,
                    Width = SelectionPanel.Width - 20,
                    Trait = item.Value,
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

            foreach (TraitEntryControl item in SelectionPanel.Children)
            {
                item.Visible = 
                    obj == string.Empty ||
                    item.Trait?.Name.ToLower().ContainsAnyTrimmed(strings) == true ||
                    item.Trait?.Id.ToString().ToLower().ContainsAnyTrimmed(strings) == true;
            }

            SelectionPanel.SortChildren((TraitEntryControl a, TraitEntryControl b) =>
            {
                string name1 = a.Trait?.Name == null ? string.Empty : a.Trait.Name;
                string name2 = b.Trait?.Name == null ? string.Empty : b.Trait.Name;
                int id1 = a.Trait?.Id == null ? 0 : a.Trait.Id;
                int id2 = b.Trait?.Id == null ? 0 : b.Trait.Id;

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