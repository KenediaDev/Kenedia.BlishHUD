using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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

            if (!terrestrial &&  Legend?.Swap.Flags.HasFlag(SkillFlag.NoUnderwater) == true)
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
            else if (Hovered || forceHover == true)
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
             new(965680), // Normal
             new(965680), // Normal
             new(965680), // Normal
             new(965680), // Normal
             new(965682), // Full
        };
        private readonly DetailedTexture[] _pipsEmpty = new DetailedTexture[5]
        {
             new(965681), // Empty
             new(965681), // Empty
             new(965681), // Empty
             new(965681), // Empty
             new(965681), // Empty
        };
        private readonly DetailedTexture[] _underlines =
        {
            new(965679),
            new(965679),
        };

        private readonly DetailedTexture _selector1 = new(157138, 157140);
        private readonly DetailedTexture _selector2 = new(157138, 157140);
        private readonly DetailedTexture _energyBar = new(965677); // 156414 // 965677
        private readonly DetailedTexture _energyDisplay = new(965695);

        //TODO: Implement Masks for Energy 
        private readonly DetailedTexture _energyBarMask = new(965678);
        private readonly DetailedTexture _energy = new(156464); //156414

        private readonly SkillIcon _professionSkill2 = new();
        private readonly SkillIcon _professionSkill3 = new();
        private readonly SkillIcon _professionSkill4 = new();

        private readonly LegendIcon _legend1 = new() { LegendSlot = LegendSlot.TerrestrialActive };
        private readonly LegendIcon _legend2 = new() { LegendSlot = LegendSlot.TerrestrialInactive };
        private readonly int _legendSize = 48;
        private List<LegendIcon> _selectableLegends = new();

        private LegendSlot _selectedLegendSlot;
        private Rectangle _selectorBounds;
        private LegendIcon _selectorAnchor;
        private bool _selectorOpen = false;

        private Rectangle _energyDisplayValue = Rectangle.Empty;

        public RevenantSpecifics()
        {
            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int xOffset = 90;
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

            switch (Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Renegade:
                    _professionSkill2.Bounds = new(xOffset + 2 + (_legendSize * 1) + 60 + 32 - 18, 15, 36, 36);
                    _professionSkill2.TextureRegion = new(6, 6, 52, 52);

                    _professionSkill3.Bounds = new(xOffset + 2 + (_legendSize * 2) + 49 + 32 - 18, 15, 36, 36);
                    _professionSkill3.TextureRegion = new(6, 6, 52, 52);

                    _professionSkill4.Bounds = new(xOffset + 2 + (_legendSize * 3) + 37 + 32 - 18, 15, 36, 36);
                    _professionSkill4.TextureRegion = new(6, 6, 52, 52);
                    break;

                case (int)SpecializationType.Vindicator:
                    _professionSkill2.Bounds = new(xOffset + 2 + (_legendSize * 2) + 49 + 32 - 18-18, 15, 36, 36);
                    _professionSkill2.TextureRegion = new(14, 14, 100, 100);

                    _professionSkill3.Bounds = new(xOffset + 2 + (_legendSize * 2) + 49 + 32 - 18+18, 15, 36, 36);
                    _professionSkill3.TextureRegion = new(14, 14, 100, 100);
                    break;

                case (int)SpecializationType.Herald:
                    _professionSkill2.Bounds = new(xOffset + 2 + (_legendSize * 2) + 49 + 32 - 18, 15, 36, 36);
                    _professionSkill2.TextureRegion = new(14, 14, 100, 100);
                    break;

                case null:
                    _professionSkill2.Bounds = new(xOffset + 2 + (_legendSize * 2) + 49 + 32 - 18, 15, 36, 36);
                    _professionSkill2.TextureRegion = new(6, 6, 52, 52);
                    break;
            }
        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e)
        {
            base.OnRightMouseButtonPressed(e);

            if (_legend1.Hovered)
            {
                _selectedLegendSlot = _legend1.LegendSlot;
                _selectorAnchor = _legend1;
                _selectorOpen = !_selectorOpen;
                if (_selectorOpen)
                {
                    GetSelectableLegends(_selectedLegendSlot);
                }

                return;
            }

            if (_legend2.Hovered)
            {
                _selectedLegendSlot = _legend2.LegendSlot;
                _selectorAnchor = _legend2;
                _selectorOpen = !_selectorOpen;
                if (_selectorOpen)
                {
                    GetSelectableLegends(_selectedLegendSlot);
                }

                return;
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_swap.Hovered)
            {
                Template.LegendSlot = GetOtherSlot();
                var slot = Template.LegendSlot;

                _legend1.Legend = Template.BuildTemplate.Legends[slot];
                _legend1.LegendSlot = slot;

                _legend2.Legend = Template.BuildTemplate.Legends[GetOtherSlot()];
                _legend2.LegendSlot = GetOtherSlot();
            }

            if (_selector1.Hovered)
            {
                _selectedLegendSlot = _legend1.LegendSlot;
                _selectorAnchor = _legend1;
                _selectorOpen = !_selectorOpen;
                if (_selectorOpen)
                {
                    GetSelectableLegends(_selectedLegendSlot);
                }
            }

            if (_selector2.Hovered)
            {
                _selectedLegendSlot = _legend2.LegendSlot;
                _selectorAnchor = _legend2;
                _selectorOpen = !_selectorOpen;
                if (_selectorOpen)
                {
                    GetSelectableLegends(_selectedLegendSlot);
                }
            }
        }

        protected override void ApplyTemplate()
        {
            var slot = Template.LegendSlot;

            _legend1.Legend = Template.BuildTemplate.Legends[slot];
            _legend1.LegendSlot = slot;

            _legend2.Legend = Template.BuildTemplate.Legends[GetOtherSlot()];
            _legend2.LegendSlot = GetOtherSlot();

            if (Template.EliteSpecialization != null && Template.EliteSpecialization.Id == (int)SpecializationType.Renegade)
            {
                _professionSkill2.Skill = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Revenant]?.Skills.Where(e => e.Value.Specialization == (int)SpecializationType.Renegade && e.Value.Slot == Gw2Sharp.WebApi.V2.Models.SkillSlot.Profession2)?.FirstOrDefault().Value;
                _professionSkill3.Skill = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Revenant]?.Skills.Where(e => e.Value.Specialization == (int)SpecializationType.Renegade && e.Value.Slot == Gw2Sharp.WebApi.V2.Models.SkillSlot.Profession3)?.FirstOrDefault().Value;
                _professionSkill4.Skill = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Revenant]?.Skills.Where(e => e.Value.Specialization == (int)SpecializationType.Renegade && e.Value.Slot == Gw2Sharp.WebApi.V2.Models.SkillSlot.Profession4)?.FirstOrDefault().Value;
            }
            else if (Template.EliteSpecialization != null && Template.EliteSpecialization.Id == (int)SpecializationType.Herald)
            {
                _professionSkill2.Skill = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Revenant]?.Skills.Where(e => e.Value.Specialization == (int) SpecializationType.Herald && e.Value.Slot == Gw2Sharp.WebApi.V2.Models.SkillSlot.Profession2)?.FirstOrDefault().Value;
            }
            else if (Template.EliteSpecialization != null && Template.EliteSpecialization.Id == (int)SpecializationType.Vindicator)
            {
                var skills = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Revenant]?.Skills.Where(e => e.Value.Specialization == (int)SpecializationType.Vindicator && e.Value.Slot == Gw2Sharp.WebApi.V2.Models.SkillSlot.Profession2);
                _professionSkill2.Skill = skills?.ElementAt(0).Value;
                _professionSkill3.Skill = skills?.ElementAt(1).Value;
            }
            else
            {
                _professionSkill2.Skill = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Revenant].Skills[55029];
            }

            base.ApplyTemplate();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            RecalculateLayout();

            _selector1.Draw(this, spriteBatch, RelativeMousePosition);
            _legend1.Draw(this, spriteBatch, Template.Terrestrial, RelativeMousePosition, null, null, _legend1.LegendSlot == Template.LegendSlot);

            _selector2.Draw(this, spriteBatch, RelativeMousePosition, Color.White * 0.5F);
            _legend2.Draw(this, spriteBatch, Template.Terrestrial, RelativeMousePosition, Color.White * 0.5F, null, false);

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

            switch (Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Renegade:
                    _professionSkill3.Draw(this, spriteBatch, RelativeMousePosition);
                    _professionSkill4.Draw(this, spriteBatch, RelativeMousePosition);
                    break;

                case (int)SpecializationType.Vindicator:
                    _professionSkill3.Draw(this, spriteBatch, RelativeMousePosition);
                    break;

                case (int)SpecializationType.Herald:
                case null:
                    break;
            }

            spriteBatch.DrawStringOnCtrl(this, "50%", Content.DefaultFont12, _energyDisplayValue, Color.White, false, HorizontalAlignment.Center, VerticalAlignment.Middle);

            if (_selectorOpen)
            {
                DrawSelector(spriteBatch, bounds);
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Input.Mouse.LeftMouseButtonPressed -= Mouse_LeftMouseButtonPressed;
        }

        private LegendSlot GetOtherSlot(LegendSlot? slot = null)
        {
            slot ??= Template.LegendSlot;

            return Template.Terrestrial
                ? slot is LegendSlot.TerrestrialActive ? LegendSlot.TerrestrialInactive : LegendSlot.TerrestrialActive
                : slot is LegendSlot.AquaticActive ? LegendSlot.AquaticInactive : LegendSlot.AquaticActive;
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (_selectorOpen)
            {
                foreach (var s in _selectableLegends)
                {
                    if (s.Hovered)
                    {
                        if (Template.Terrestrial || !s.Legend.Swap.Flags.HasFlag(SkillFlag.NoUnderwater))
                        {
                            var otherLegend = Template.BuildTemplate.Legends[GetOtherSlot(_selectorAnchor.LegendSlot)];

                            if (otherLegend != null && otherLegend == s.Legend)
                            {
                                Template.BuildTemplate.SetLegend(_selectorAnchor.Legend, GetOtherSlot(_selectorAnchor.LegendSlot));
                            }

                            Template.BuildTemplate.SetLegend(s.Legend, _selectorAnchor.LegendSlot);
                            _selectorAnchor.Legend = s.Legend;
                        }
                    }
                }

                _selectorOpen = false;
            }
        }

        private void GetSelectableLegends(LegendSlot legendSlot)
        {
            _selectableLegends.Clear();

            var legends = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Revenant].Legends.Where(e => e.Value.Specialization == 0 || e.Value.Specialization == Template.EliteSpecialization?.Id);

            int columns = Math.Min(legends.Count(), 4);
            int rows = (int)Math.Ceiling(legends.Count() / (double)columns);
            _selectorBounds = new(_selectorAnchor.Bounds.X - ((((_legendSize * columns) + 8) / 2) - (_legendSize / 2)), _selectorAnchor.Bounds.Bottom, (_legendSize * columns) + 4, (_legendSize * rows) + 40);

            int column = 0;
            int row = 0;
            foreach (var legend in legends)
            {
                _selectableLegends.Add(new() { Legend = legend.Value, Bounds = new(_selectorBounds.Left + 4 + (column * _legendSize), _selectorBounds.Top + 4 + (row * _legendSize), _legendSize - 4, _legendSize - 4) });
                column++;

                if (column > 3)
                {
                    column = 0;
                    row++;
                }
            }
        }

        private void DrawSelector(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, _selectorBounds, Rectangle.Empty, Color.Black * 0.7f);
            Color borderColor = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(_selectorBounds.Left, _selectorBounds.Top, _selectorBounds.Width, 2), Rectangle.Empty, borderColor * 0.8f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(_selectorBounds.Left, _selectorBounds.Bottom - 2, _selectorBounds.Width, 2), Rectangle.Empty, borderColor * 0.8f);

            // Left
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(_selectorBounds.Left, _selectorBounds.Top, 2, _selectorBounds.Height), Rectangle.Empty, borderColor * 0.8f);

            // Right
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(_selectorBounds.Right - 2, _selectorBounds.Top, 2, _selectorBounds.Height), Rectangle.Empty, borderColor * 0.8f);

            foreach (var s in _selectableLegends)
            {
                s.Draw(this, spriteBatch, Template.Terrestrial, RelativeMousePosition);
            }

            spriteBatch.DrawStringOnCtrl(this, "Legends", Content.DefaultFont18, new Rectangle(_selectorBounds.Left, _selectorBounds.Bottom - 12 - Content.DefaultFont18.LineHeight, _selectorBounds.Width, Content.DefaultFont18.LineHeight), Color.White, false, HorizontalAlignment.Center);
        }
    }
}
