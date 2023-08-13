using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.Extensions;
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

        public LegendSlotType LegendSlot { get; set; }

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

            if (!terrestrial && Legend?.Swap.Flags.HasFlag(SkillFlag.NoUnderwater) == true)
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
        private readonly List<DetailedTexture> _swaps = new()
        {
            new(784346){HoverDrawColor = Colors.ColonialWhite},
            new(784346){HoverDrawColor = Colors.ColonialWhite},
        };
        private readonly Dictionary<LegendSlotType, LegendIcon> _legends = new()
        {
            { LegendSlotType.TerrestrialActive, new() { LegendSlot = LegendSlotType.TerrestrialActive} },
            { LegendSlotType.TerrestrialInactive, new() { LegendSlot = LegendSlotType.TerrestrialInactive} },
            { LegendSlotType.AquaticActive, new() { LegendSlot = LegendSlotType.AquaticActive} },
            { LegendSlotType.AquaticInactive, new() { LegendSlot = LegendSlotType.AquaticInactive} },
        };

        private readonly Dictionary<LegendSlotType, DetailedTexture> _selectors = new()
        {
            { LegendSlotType.TerrestrialActive, new(157138, 157140) },
            { LegendSlotType.TerrestrialInactive, new(157138, 157140) },
            { LegendSlotType.AquaticActive, new(157138, 157140) },
            { LegendSlotType.AquaticInactive, new(157138, 157140) },
        };

        private readonly int _legendSize = 48;
        private List<LegendIcon> _selectableLegends = new();

        private LegendSlotType _selectedLegendSlot;
        private Rectangle _selectorBounds;
        private LegendIcon _selectorAnchor;
        private bool _selectorOpen = false;

        private Rectangle _energyDisplayValue = Rectangle.Empty;

        public RevenantSpecifics(TemplatePresenter template) : base(template)
        {
            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int xOffset = 90;

            _legends[LegendSlotType.TerrestrialInactive].Bounds = new(xOffset, 45, _legendSize, _legendSize);
            _selectors[LegendSlotType.TerrestrialInactive].Bounds = new(xOffset, 35, _legendSize, 10);

            _swaps[0].Bounds = new(xOffset + 2 + _legendSize - ((int)(_legendSize / 1.5) / 2), 55, (int)(_legendSize / 1.5), (int)(_legendSize / 1.5));

            _legends[LegendSlotType.TerrestrialActive].Bounds = new(xOffset + 5 + _legendSize, 45, _legendSize, _legendSize);
            _selectors[LegendSlotType.TerrestrialActive].Bounds = new(xOffset + 5 + _legendSize, 35, _legendSize, 10);

            xOffset += 10 + _legendSize + 320;
            _legends[LegendSlotType.AquaticInactive].Bounds = new(xOffset, 45, _legendSize, _legendSize);
            _selectors[LegendSlotType.AquaticInactive].Bounds = new(xOffset, 35, _legendSize, 10);

            _swaps[1].Bounds = new(xOffset + 2 + _legendSize - ((int)(_legendSize / 1.5) / 2), 55, (int)(_legendSize / 1.5), (int)(_legendSize / 1.5));

            _legends[LegendSlotType.AquaticActive].Bounds = new(xOffset + 5 + _legendSize, 45, _legendSize, _legendSize);
            _selectors[LegendSlotType.AquaticActive].Bounds = new(xOffset + 5 + _legendSize, 35, _legendSize, 10);
        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e)
        {
            base.OnRightMouseButtonPressed(e);

            foreach (LegendSlotType slot in Enum.GetValues(typeof(LegendSlotType)))
            {
                if (_selectors[slot].Hovered || _legends[slot].Hovered)
                {
                    _selectorAnchor = _legends[slot];
                    _selectorOpen = !_selectorOpen;
                    if (_selectorOpen)
                    {
                        GetSelectableLegends(_selectedLegendSlot);
                    }
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            foreach (var swap in _swaps)
            {
                if (swap.Hovered)
                {
                    TemplatePresenter.SwapLegend();
                    return;
                }
            }

            foreach (LegendSlotType slot in Enum.GetValues(typeof(LegendSlotType)))
            {
                if (_selectors[slot].Hovered || _legends[slot].Hovered)
                {
                    _selectorAnchor = _legends[slot];
                    _selectorOpen = !_selectorOpen;
                    if (_selectorOpen)
                    {
                        GetSelectableLegends(_selectedLegendSlot);
                    }
                }
            }
        }

        protected override void ApplyTemplate()
        {
            foreach (LegendSlotType slot in Enum.GetValues(typeof(LegendSlotType)))
            {
                _legends[slot].Legend = TemplatePresenter?.Template?.Legends[slot];
            }

            base.ApplyTemplate();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            foreach (var legend in _legends)
            {
                _legends[legend.Key].Legend ??= TemplatePresenter.Template.Legends[legend.Key];

                legend.Value.Draw(this, spriteBatch, RelativeMousePosition);

                if (legend.Key is LegendSlotType.AquaticActive or LegendSlotType.TerrestrialActive)
                {
                    spriteBatch.DrawFrame(this, legend.Value.Bounds, Colors.ColonialWhite * 0.75F, 2);
                }
            }

            foreach (var selector in _selectors)
            {
                selector.Value.Draw(this, spriteBatch, RelativeMousePosition);
            }

            foreach (var swap in _swaps)
            {
                swap.Draw(this, spriteBatch, RelativeMousePosition);
            }

            var r = _legends[LegendSlotType.TerrestrialActive].Bounds;
            spriteBatch.DrawStringOnCtrl(this, TemplatePresenter.LegendSlot is LegendSlotType.TerrestrialActive ? strings.ActiveLegend : strings.InactiveLegend, Content.DefaultFont16, new(r.Right + 5, r.Center.Y - (Content.DefaultFont14.LineHeight / 2), 100, Content.DefaultFont16.LineHeight), Color.White);

            r = _legends[LegendSlotType.AquaticActive].Bounds;
            spriteBatch.DrawStringOnCtrl(this, TemplatePresenter.LegendSlot is LegendSlotType.TerrestrialActive ? strings.ActiveLegend : strings.InactiveLegend, Content.DefaultFont16, new(r.Right + 5, r.Center.Y - (Content.DefaultFont14.LineHeight / 2), 100, Content.DefaultFont16.LineHeight), Color.White);

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

        private LegendSlotType GetOtherSlot(LegendSlotType? slot = null)
        {
            slot ??= TemplatePresenter.LegendSlot;

            return slot switch
            {
                LegendSlotType.AquaticActive => LegendSlotType.AquaticInactive,
                LegendSlotType.AquaticInactive => LegendSlotType.AquaticActive,
                LegendSlotType.TerrestrialActive => LegendSlotType.TerrestrialInactive,
                LegendSlotType.TerrestrialInactive => LegendSlotType.TerrestrialActive,
                null => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (_selectorOpen)
            {
                foreach (var s in _selectableLegends)
                {
                    if (s.Hovered)
                    {
                        if (true|| !s.Legend.Swap.Flags.HasFlag(SkillFlag.NoUnderwater))
                        {
                            var slot = GetOtherSlot(_selectorAnchor.LegendSlot);
                            var otherLegend = TemplatePresenter.Template.Legends[slot];

                            if (otherLegend is not null && otherLegend == s.Legend)
                            {
                                TemplatePresenter.Template.SetLegend(_selectorAnchor.Legend, slot);
                            }

                            TemplatePresenter.Template.SetLegend(s.Legend, _selectorAnchor.LegendSlot);
                            _selectorAnchor.Legend = s.Legend;
                        }
                    }
                }

                _selectorOpen = false;
            }
        }

        private void GetSelectableLegends(LegendSlotType legendSlot)
        {
            _selectableLegends.Clear();

            var legends = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Revenant].Legends.Where(e => e.Value.Specialization == 0 || e.Value.Specialization == TemplatePresenter.Template.EliteSpecialization?.Id);

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
                s.Draw(this, spriteBatch, true, RelativeMousePosition);
            }

            spriteBatch.DrawStringOnCtrl(this, "Legends", Content.DefaultFont18, new Rectangle(_selectorBounds.Left, _selectorBounds.Bottom - 12 - Content.DefaultFont18.LineHeight, _selectorBounds.Width, Content.DefaultFont18.LineHeight), Color.White, false, HorizontalAlignment.Center);
        }
    }
}
