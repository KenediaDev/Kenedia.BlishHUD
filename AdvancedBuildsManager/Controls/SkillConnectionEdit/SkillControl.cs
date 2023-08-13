using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using static Blish_HUD.ContentService;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.DataModels;
using Skill = Gw2Sharp.WebApi.V2.Models.Skill;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Models;
using Blish_HUD;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.SkillConnectionEdit
{
    public class SkillControl : Blish_HUD.Controls.Control
    {
        private readonly DetailedTexture _skillIcon = new()
        {
            FallBackTexture = AsyncTexture2D.FromAssetId(157154),
            TextureRegion = new(14, 14, 100, 100),
        };

        private readonly DetailedTexture _delete = new(2175783, 2175784);

        private BaseSkill _skill;

        private Rectangle _nameBounds;
        private Rectangle _idBounds;

        public SkillControl()
        {
            Height = 32;
            Width = 400;
        }

        public SkillControl(int skillid) : this()
        {
            if (AdvancedBuildsManager.Data.BaseSkills.TryGetValue(skillid, out BaseSkill skill))
            {
                Skill = skill;
            }
        }

        public BaseSkill Skill { get => _skill; 
            set 
            {
                int? prev = _skill?.Id;

                if(Common.SetProperty(ref _skill, value, ApplySkill))
                {
                    OnChangedAction?.Invoke(prev, Skill?.Id);
                }
            } 
        }

        public OldSkillConnection SkillConnection { get; set; }

        public Action<int?> OnIconAction { get; set; }

        public Action<int?> OnDeleteAction { get; set; }

        public Action<int?, int?> OnChangedAction { get; set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int padding = 4;
            int sizePadding = padding * 2;

            _delete.Bounds = new(0, 0, Height - sizePadding, Height - sizePadding);
            _skillIcon.Bounds = new(_delete.Bounds.Right + 5, 0, Height - sizePadding, Height - sizePadding);
            _idBounds = new(_skillIcon.Bounds.Right + 10, 0, 50, Height - sizePadding);
            _nameBounds = new(_idBounds.Right + 5, 0, Math.Max(0, Width - _idBounds.Right), Height - sizePadding);
        }

        private void ApplySkill()
        {
            _skillIcon.Texture = Skill?.Icon;
            BasicTooltipText = Skill?.Name;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_delete.Hovered)
            {
                OnDeleteAction?.Invoke(Skill?.Id);
                Skill = null;
            }

            if (_skillIcon.Hovered) OnIconAction?.Invoke(Skill?.Id);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string txt = string.Empty;
            _delete.Draw(this, spriteBatch, RelativeMousePosition);
            if (_delete.Hovered) txt = "Delete";

            _skillIcon.Draw(this, spriteBatch, RelativeMousePosition);

            if (Skill is not null)
            {
                spriteBatch.DrawStringOnCtrl(this, $"{Skill.Id}", Content.DefaultFont16, _idBounds, Color.White);
                spriteBatch.DrawStringOnCtrl(this, Skill.Name, Content.DefaultFont16, _nameBounds, Color.White);

                txt = txt == string.Empty ? Skill.Name : txt;
            }

            Color borderColor = _skillIcon.Hovered ? Colors.ColonialWhite : Color.Black;
            var ctrl = this;
            var Bounds = _skillIcon.Bounds;

            // Top
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Bottom - 1, Bounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Top, 1, Bounds.Height), Rectangle.Empty, borderColor * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Right - 1, Bounds.Top, 1, Bounds.Height), Rectangle.Empty, borderColor * 0.6f);

            BasicTooltipText = txt;
        }
    }
}
