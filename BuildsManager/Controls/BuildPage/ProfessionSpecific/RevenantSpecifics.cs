using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules.Managers;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.ComponentModel;
using System.Diagnostics;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class LegendIcon : DetailedTexture
    {
        private Legend _legend;
        private readonly AsyncTexture2D _hoveredFrameTexture = AsyncTexture2D.FromAssetId(157143);
        private readonly AsyncTexture2D _noAquaticFlagTexture = AsyncTexture2D.FromAssetId(157145);
        private Rectangle _noAquaticFlagTextureRegion;
        private Rectangle _hoveredFrameTextureRegion;

        public LegendIcon()
        {
            _noAquaticFlagTextureRegion = new(16, 16, 96, 96);
            _hoveredFrameTextureRegion = new(8, 8, 112, 112);
            TextureRegion = new(14, 14, 100, 100);
            FallBackTexture = AsyncTexture2D.FromAssetId(157154);
        }

        public Legend Legend { get => _legend; set => Common.SetProperty(ref _legend, value, ApplyLegend); }

        public LegendSlot LegendSlot { get; set; }

        private void ApplyLegend()
        {
            Texture = Legend?.Swap.Icon;
        }

        public void Draw(Control ctrl, SpriteBatch spriteBatch, bool terrestrial = true, Point? mousePos = null, Color? color = null, Color? bgColor = null, bool? forceHover = null, float? rotation = null, Vector2? origin = null)
        {
            base.Draw(ctrl, spriteBatch, mousePos, color, bgColor, forceHover, rotation, origin);

            color ??= Color.White;
            origin ??= Vector2.Zero;
            rotation ??= 0F;
            Color borderColor = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Bottom - 1, Bounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Top, 1, Bounds.Height), Rectangle.Empty, borderColor * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Right - 1, Bounds.Top, 1, Bounds.Height), Rectangle.Empty, borderColor * 0.6f);

            color ??= Color.White;
            origin ??= Vector2.Zero;
            rotation ??= 0F;

            if (!terrestrial && Legend.Swap.Flags.HasFlag(Gw2Sharp.SkillFlag.NoUnderwater))
            {
                spriteBatch.DrawOnCtrl(
                    ctrl,
                    _noAquaticFlagTexture,
                    Bounds,
                    _noAquaticFlagTextureRegion,
                    (Color)color,
                    (float)rotation,
                    (Vector2)origin);
            }
            else if (Hovered)
            {
                spriteBatch.DrawOnCtrl(
                    ctrl,
                    _hoveredFrameTexture,
                    Bounds,
                    _hoveredFrameTextureRegion,
                    (Color)color * 0.8F,
                    (float)rotation,
                    (Vector2)origin);
            }
        }
    }

    public class RevenantSpecifics : ProfessionSpecifics
    {
        private readonly DetailedTexture _swap = new(784346);
        private readonly DetailedTexture[] _pipsFull = new DetailedTexture[5]
        {
             new(965680),
             new(965680),
             new(965680),
             new(965680),
             new(965682),
        };
        private readonly DetailedTexture[] _pipsEmpty = new DetailedTexture[5]
        {
             new(965681),
             new(965681),
             new(965681),
             new(965681),
             new(965681),
        };
        private readonly DetailedTexture[] _underlines =
        {
            new(965679),
            new(965679),
        };

        private readonly DetailedTexture _pipNormal = new(965680);
        private readonly DetailedTexture _pipEmpty = new(965681);
        private readonly DetailedTexture _pipFull = new(965682);
        private readonly DetailedTexture _selector1 = new(157138, 157140);
        private readonly DetailedTexture _selector2 = new(157138, 157140);
        private readonly DetailedTexture _energy = new(156464); //156414
        private readonly DetailedTexture _energyBar = new(965677); // 156414 // 965677
        private readonly DetailedTexture _energyBarMask = new(965678);
        private readonly DetailedTexture _energyDisplay = new(965695);
        private readonly DetailedTexture _underline = new(965679);

        private readonly SkillIcon _professionSkill2 = new();
        private readonly SkillIcon _professionSkill3 = new();
        private readonly SkillIcon _professionSkill4 = new();

        private readonly LegendIcon _legend1 = new() { LegendSlot = LegendSlot.TerrestrialActive };
        private readonly LegendIcon _legend2 = new() { LegendSlot = LegendSlot.TerrestrialInactive };
        private readonly int _legendSize = 48;

        private Rectangle _energyDisplayValue = Rectangle.Empty;

        public RevenantSpecifics()
        {

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            var xOffset = 90;
            _legend1.Bounds = new(xOffset + 2 + _legendSize, 45, _legendSize, _legendSize);
            _selector1.Bounds = new(xOffset + 2 + _legendSize, 35, _legendSize, 10);

            _legend2.Bounds = new(xOffset + 0, 45, _legendSize, _legendSize);
            _selector2.Bounds = new(xOffset + 0, 35, _legendSize, 10);

            _swap.Bounds = new(xOffset + 2 + _legendSize - ((int)(_legendSize / 1.5) / 2), 55, (int)(_legendSize / 1.5), (int)(_legendSize / 1.5));
            _energyBar.Bounds = new(xOffset + 2 + (_legendSize * 2), 67, 250, 40);
            _energyDisplay.Bounds = new(xOffset + 2 + (_legendSize * 2) + 48, 51, 68, 34);
            _energyDisplayValue = new(xOffset + 2 + (_legendSize * 2) + 49, 58, 68, Content.DefaultFont12.LineHeight);

            _underlines[0].Bounds = new(xOffset + 93, 58, 68, 18);
            _underlines[1].Bounds = new(xOffset + 202, 58, 68, 18);

            for (int i = 0; i < _pipsFull.Length; i++)
            {
                _pipsFull[i].Bounds = new(xOffset + 203 + (i * 10), 59, 16, 16);
                _pipsEmpty[i].Bounds = new(xOffset + 103 + (i * 10), 59, 16, 16);
            }

            _professionSkill2.Bounds = new(xOffset + 2 + (_legendSize * 2) + 49 + 32 - 18, 15, 36, 36);
            _professionSkill2.TextureRegion = new(6, 6, 52, 52);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_swap.Hovered)
            {
                Template.BuildTemplate.LegendSlot = GetOtherSlot();
                var slot = Template.BuildTemplate.LegendSlot;

                _legend1.Legend = Template.BuildTemplate.Legends[slot];
                _legend1.LegendSlot = slot;

                _legend2.Legend = Template.BuildTemplate.Legends[GetOtherSlot()];
                _legend2.LegendSlot = GetOtherSlot();
            }
        }

        protected override void ApplyTemplate()
        {
            base.ApplyTemplate();

            var slot = Template.BuildTemplate.LegendSlot;

            _legend1.Legend = Template.BuildTemplate.Legends[slot];
            _legend1.LegendSlot = slot;

            _legend2.Legend = Template.BuildTemplate.Legends[GetOtherSlot()];
            _legend2.LegendSlot = GetOtherSlot();

            if (Template.EliteSpecialization != null && Template.EliteSpecialization.Id == (int)Specializations.Renegade)
            {

            }
            else if (Template.EliteSpecialization != null && Template.EliteSpecialization.Id == (int)Specializations.Herald)
            {

            }
            else if (Template.EliteSpecialization != null && Template.EliteSpecialization.Id == (int)Specializations.Vindicator)
            {

            }
            else
            {
                _professionSkill2.Skill = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Revenant].Skills[55029];
            }

            RecalculateLayout();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            RecalculateLayout();

            _selector1.Draw(this, spriteBatch, RelativeMousePosition);
            _legend1.Draw(this, spriteBatch, Template.BuildTemplate.Terrestrial, RelativeMousePosition, null, null, _legend1.LegendSlot == Template.BuildTemplate.LegendSlot);

            _selector2.Draw(this, spriteBatch, RelativeMousePosition, Color.White * 0.5F);
            _legend2.Draw(this, spriteBatch, Template.BuildTemplate.Terrestrial, null, Color.White * 0.5F);

            _swap.Draw(this, spriteBatch, RelativeMousePosition, _swap.Hovered ? Color.White : Color.White * 0.75F);
            _energyBar.Draw(this, spriteBatch);
            _energyDisplay.Draw(this, spriteBatch);

            _underlines[0].Draw(this, spriteBatch, SpriteEffects.FlipHorizontally);
            _underlines[1].Draw(this, spriteBatch);

            for (int i = 0; i < _pipsFull.Length; i++)
            {
                _pipsFull[i].Draw(this, spriteBatch);
                _pipsEmpty[i].Draw(this, spriteBatch, SpriteEffects.FlipHorizontally);
            }

            _professionSkill2.Draw(this, spriteBatch, RelativeMousePosition);

            spriteBatch.DrawStringOnCtrl(this, "50%", Content.DefaultFont12, _energyDisplayValue, Color.White, false, HorizontalAlignment.Center, VerticalAlignment.Middle);

        }

        private LegendSlot GetOtherSlot()
        {
            var slot = Template.BuildTemplate.LegendSlot;

            return Template.BuildTemplate.Terrestrial
                ? slot is LegendSlot.TerrestrialActive ? LegendSlot.TerrestrialInactive : LegendSlot.TerrestrialActive
                : slot is LegendSlot.AquaticActive ? LegendSlot.AquaticInactive : LegendSlot.AquaticActive;
        }
    }
}
