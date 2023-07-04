using Blish_HUD.Input;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.SkillConnectionEdit
{
    public class TraitEntryControl : BaseConnectionControl
    {
        private Trait _trait;

        public Trait Trait { get => _trait; set => Common.SetProperty(ref _trait, value, ApplyItem); }

        public Action<TraitEntryControl> OnClickAction { get; set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);
        }

        protected override void ApplyItem()
        {
            base.ApplyItem();

            Name = Trait?.Name;
            Id = Trait?.Id;
            Icon = Trait?.Icon;
            BasicTooltipText = Name;
        }

        protected override void ApplyConnection()
        {
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            OnClickAction?.Invoke(this);
        }
    }
}