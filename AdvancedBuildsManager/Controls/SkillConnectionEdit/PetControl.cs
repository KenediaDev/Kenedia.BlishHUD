using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using static Blish_HUD.ContentService;
using Kenedia.Modules.Core.Utility;
using Blish_HUD;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using Blish_HUD.Input;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.SkillConnectionEdit
{
    public class PetControl : Blish_HUD.Controls.Control
    {
        private readonly DetailedTexture _skillIcon = new()
        {
            FallBackTexture = AsyncTexture2D.FromAssetId(157154),
            TextureRegion = new(16, 16, 200, 200),
        };

        private readonly DetailedTexture _delete = new(2175783, 2175784);
        private Rectangle _nameBounds;
        private Rectangle _idBounds;

        public PetControl()
        {
            Height = 32;
            Width = 400;
        }

        public PetControl(int petid) : this()
        {
            if (AdvancedBuildsManager.Data.Pets.TryGetValue(petid, out Pet pet))
            {
                Pet = pet;
            }
        }

        public Pet Pet { get; set => Common.SetProperty(field, value, v => field = v, ApplyPet); }

        public Action<PetEntryControl> OnClickAction { get; set; }

        public Action<int?> OnIconAction { get; set; }

        public Action<int?> OnDeleteAction { get; set; }

        public Action<int?> OnChangedAction { get; set; }

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

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_delete.Hovered)
            {
                OnDeleteAction?.Invoke(Pet?.Id);
                Pet = null;
            }

            if (_skillIcon.Hovered) OnIconAction?.Invoke(Pet?.Id);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string txt = string.Empty;
            _delete.Draw(this, spriteBatch, RelativeMousePosition);
            if (_delete.Hovered) txt = "Delete";

            _skillIcon.Draw(this, spriteBatch, RelativeMousePosition);

            if (Pet is not null)
            {
                spriteBatch.DrawStringOnCtrl(this, $"{Pet.Id}", Content.DefaultFont16, _idBounds, Color.White);
                spriteBatch.DrawStringOnCtrl(this, Pet.Name, Content.DefaultFont16, _nameBounds, Color.White);

                txt = txt == string.Empty ? Pet.Name : txt;
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

        protected void ApplyPet()
        {
            _skillIcon.Texture = Pet?.Icon;
            BasicTooltipText = Pet?.Name;

            OnChangedAction?.Invoke(Pet?.Id);
        }
    }
}
