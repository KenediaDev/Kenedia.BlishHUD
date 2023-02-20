using System;
using System.Threading.Tasks;
using Kenedia.Modules.Core.DataModels;

namespace Kenedia.Modules.BuildsManager.Controls.SkillConnectionEdit
{
    public class PetSelector : Selector<PetControl, SkillConnection>
    {
        public PetSelector() { }

        public Action<PetControl> OnClickAction { get; set; }

        protected override void ApplyItems()
        {
            SelectionPanel.Children.Clear();

            foreach (var item in Items)
            {
                _ = new PetControl()
                {
                    Entry = item,
                    Parent = SelectionPanel,
                    Height = 40,
                    Width = SelectionPanel.Width - 20,
                    SkillConnection = item.Value,
                    OnClickAction = OnClickAction,
                };
            }
        }

        protected override async Task FilterItems(string obj)
        {
            if (!await WaitAndCatch()) return;

            foreach (SkillConnectionEntryControl item in SelectionPanel.Children)
            {
                item.Visible = item.Skill?.Professions.Count == 1 && (item.Skill?.Professions.Contains(BuildsManager.ModuleInstance.SkillConnectionEditor.Profession.ToString()) == true) && (item.Skill?.Name.ToLower().Contains(obj.ToLower()) == true || item.Skill?.Id.ToString().ToLower().Contains(obj.ToLower()) == true);
            }
            SelectionPanel.Invalidate();
        }
    }
}
