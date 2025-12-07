using Blish_HUD.Content;
using Blish_HUD;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Blish_HUD.ContentService;
using System;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Blish_HUD.Input;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.SkillConnectionEdit
{
    public class TraitControl : Blish_HUD.Controls.Control
    {
        private readonly DetailedTexture _skillIcon = new()
        {
            FallBackTexture = AsyncTexture2D.FromAssetId(157154),
            TextureRegion = new(4, 4, 58, 58),
        };
        private Rectangle _nameBounds;
        private Rectangle _idBounds;

        public TraitControl()
        {
            Height = 32;
            Width = 400;
        }

        public TraitControl(int traitid) : this()
        {
            var trait = GetTrait(traitid);

            if (trait is not null)
            {
                Trait = trait;
            }
        }

        public Trait Trait { get; set => Common.SetProperty(ref field, value, ApplyTrait); }

        public Action<int?> OnIconAction { get; set; }

        public Action<int?> OnDeleteAction { get; set; }

        public Action<int?> OnTraitChangedAction{ get; set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int padding = 4;
            int sizePadding = padding * 2;

            _skillIcon.Bounds = new(0, 0, Height - sizePadding, Height - sizePadding);
            _idBounds = new(_skillIcon.Bounds.Right + 10, 0, 50, Height - sizePadding);
            _nameBounds = new(_idBounds.Right + 5, 0, Math.Max(0, Width - _idBounds.Right), Height - sizePadding);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            if (_skillIcon.Hovered) OnIconAction?.Invoke(Trait?.Id);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string txt = string.Empty;
            _skillIcon.Draw(this, spriteBatch, RelativeMousePosition);

            if (Trait is not null)
            {
                spriteBatch.DrawStringOnCtrl(this, $"{Trait.Id}", Content.DefaultFont16, _idBounds, Color.White);
                spriteBatch.DrawStringOnCtrl(this, Trait.Name, Content.DefaultFont16, _nameBounds, Color.White);

                txt = txt == string.Empty ? Trait.Name : txt;
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

        protected void ApplyTrait()
        {
            _skillIcon.Texture = Trait?.Icon;
            BasicTooltipText = Trait?.Name;

            OnTraitChangedAction?.Invoke(Trait?.Id);
        }

        private Trait GetTrait(int id)
        {
            foreach (var p in AdvancedBuildsManager.Data.Professions)
            {
                foreach (var s in p.Value.Specializations)
                {
                    foreach (var t in s.Value.MinorTraits)
                    {
                        if (t.Value.Id == id) return t.Value;
                    }

                    foreach (var t in s.Value.MajorTraits)
                    {
                        if (t.Value.Id == id) return t.Value;
                    }
                }
            }

            return null;
        }
    }
}