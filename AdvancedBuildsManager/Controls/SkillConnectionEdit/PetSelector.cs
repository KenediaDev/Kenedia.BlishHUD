using System;
using System.Threading.Tasks;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using Kenedia.Modules.AdvancedBuildsManager.Views;
using Kenedia.Modules.Core.Extensions;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.SkillConnectionEdit
{
    public class PetSelector : Selector<PetEntryControl, Pet>
    {
        public PetSelector(SkillConnectionEditor editor)
        {
            Editor = editor;
            Editor.ProfessionChanged += SkillConnectionEditor_ProfessionChanged;
        }

        public Action<PetEntryControl, PetControl> OnClickAction { get; set; }

        public PetControl Anchor { get; set; }

        public SkillConnectionEditor Editor { get; }

        protected override void ApplyItems()
        {
            SelectionPanel.Children.Clear();

            foreach (var item in Items)
            {
                _ = new PetEntryControl()
                {
                    Entry = item.Value,
                    Parent = SelectionPanel,
                    Height = 40,
                    Width = SelectionPanel.Width - 20,
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

            foreach (PetEntryControl item in SelectionPanel.Children)
            {
                item.Visible =
                    obj == string.Empty ||
                    item.Pet?.Name.ToLower().ContainsAnyTrimmed(strings) == true ||
                    item.Pet?.Id.ToString().ToLower().ContainsAnyTrimmed(strings) == true;
            }

            SelectionPanel.Invalidate();
        }

        private void SkillConnectionEditor_ProfessionChanged(object sender, Gw2Sharp.Models.ProfessionType? e)
        {
            _ = FilterItems(string.Empty);
        }
    }
}
