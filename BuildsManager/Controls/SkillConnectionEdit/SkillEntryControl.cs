using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Kenedia.Modules.Core.DataModels;
using Blish_HUD.Input;
using MonoGame.Extended.Collections;

namespace Kenedia.Modules.BuildsManager.Controls.SkillConnectionEdit
{
    public class SkillEntryControl : BaseConnectionControl
    {
        public BaseSkill Skill { get; private set; } = null;

        public Action<SkillEntryControl> OnClickAction { get; set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);
        }

        protected override void ApplyItem()
        {
            base.ApplyItem();

        }

        protected override void ApplyConnection()
        {
            Skill = BuildsManager.Data.BaseSkills.GetValueOrDefault(SkillConnection.Id);
            Name = Skill?.Name;
            Id = Skill?.Id;
            Icon = Skill?.Icon;
            BasicTooltipText = Name;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            OnClickAction?.Invoke(this);
        }
    }
}
