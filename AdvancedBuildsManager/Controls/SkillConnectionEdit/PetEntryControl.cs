using System;
using Blish_HUD.Input;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.SkillConnectionEdit
{
    public class PetEntryControl : BaseConnectionControl
    {
        public PetEntryControl()
        {
            TextureRegion = new(16, 16, 200, 200);
        }

        public Pet Pet => Entry != null ? (Pet) Entry : null;

        public Action<PetEntryControl> OnClickAction { get; set; }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            OnClickAction?.Invoke(this);
        }

        protected override void ApplyConnection()
        {

        }

        protected override void ApplyItem()
        {
            base.ApplyItem();

            Name = Pet?.Name;
            Id = Pet?.Id;
            Icon = Pet?.Icon;
            BasicTooltipText = Name;
        }
    }
}
