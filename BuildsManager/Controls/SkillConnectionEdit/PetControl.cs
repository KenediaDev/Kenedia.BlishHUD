using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Kenedia.Modules.Core.Utility;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Professions;

namespace Kenedia.Modules.BuildsManager.Controls.SkillConnectionEdit
{
    public class PetControl : SkillConnectionControl
    {
        private Pet _pet;

        public Pet Pet { get => _pet; set => Common.SetProperty(ref _pet, value, ApplyItem); }

        public Action<PetControl> OnClickAction { get; set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);
        }

        protected override void ApplyItem()
        {
            base.ApplyItem();

            Name = Pet.Name;
            Id = Pet.Id;
            Icon = Pet.Icon;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            OnClickAction?.Invoke(this);
        }
    }
}
