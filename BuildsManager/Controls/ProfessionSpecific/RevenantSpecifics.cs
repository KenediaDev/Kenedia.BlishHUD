using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Controls.Selectables;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.BuildsManager.Controls.ProfessionSpecific
{
    public class RevenantSpecifics : ProfessionSpecifics
    {
        private readonly Dictionary<LegendSlotType, LegendIcon> _legends = new()
        {
            { LegendSlotType.TerrestrialActive, new() { LegendSlot = LegendSlotType.TerrestrialActive} },
            { LegendSlotType.TerrestrialInactive, new() { LegendSlot = LegendSlotType.TerrestrialInactive} },
            { LegendSlotType.AquaticActive, new() { LegendSlot = LegendSlotType.AquaticActive} },
            { LegendSlotType.AquaticInactive, new() { LegendSlot = LegendSlotType.AquaticInactive} },
        };

        private readonly LegendSelector _legendSelector;

        private LegendSlotType _selectedLegendSlot;

        private LegendIcon _selectorAnchor;

        public ImageButton AquaticSwapButton { get; }

        public ImageButton TerrestrialSwapButton { get; }

        public RevenantSpecifics(TemplatePresenter template, Data data) : base(template, data)
        {
            _legendSelector = new()
            {
                Parent = Graphics.SpriteScreen,
                Visible = false,
                OnClickAction = (legend) =>
                {
                    TemplatePresenter.Template?.SetLegend(_selectedLegendSlot, legend);
                    _legendSelector.Hide();
                }
            };

            foreach (var c in _legends.Values)
            {
                c.Parent = this;
                c.LeftClickAction = SetSelector;
                c.RightClickAction = SetSelector;
            }

            AquaticSwapButton = new ImageButton()
            {
                Parent = this,
                ClickAction = (b) => TemplatePresenter.SwapLegend(),
                Texture = AsyncTexture2D.FromAssetId(784346),
                ColorHovered = Colors.ColonialWhite,
            };

            TerrestrialSwapButton = new ImageButton()
            {
                Parent = this,
                ClickAction = (b) => TemplatePresenter.SwapLegend(),
                Texture = AsyncTexture2D.FromAssetId(784346),
                ColorHovered = Colors.ColonialWhite,
            };

            TemplatePresenter.LegendSlotChanged += TemplatePresenter_LegendSlotChanged;
            ApplyCurrentLegendSlot();
        }

        private void TemplatePresenter_LegendSlotChanged(object sender, Core.Models.ValueChangedEventArgs<LegendSlotType> e)
        {
            ApplyCurrentLegendSlot();
        }

        private void ApplyCurrentLegendSlot()
        {
            foreach (var (legend, isActive) in from legend in _legends
                                               let isActive = TemplatePresenter.LegendSlot is LegendSlotType.AquaticActive or LegendSlotType.TerrestrialActive
                                               select (legend, isActive))
            {
                legend.Value.IsActive = isActive ? legend.Key is LegendSlotType.AquaticActive or LegendSlotType.TerrestrialActive : legend.Key is LegendSlotType.AquaticInactive or LegendSlotType.TerrestrialInactive;
            }
        }

        private void SetSelector(LegendIcon icon)
        {
            _selectorAnchor = icon;

            _legendSelector.Anchor = icon;
            _legendSelector.ZIndex = ZIndex + 1000;
            _legendSelector.SelectedItem = icon.Legend;
            _legendSelector.LegendSlot = icon.LegendSlot;
            _legendSelector.AnchorOffset = new(0, 0);
            _legendSelector.Label = icon.LegendSlot switch
            {
                LegendSlotType.TerrestrialActive => "Terrestrial Active",
                LegendSlotType.TerrestrialInactive => "Terrestrial Inactive",
                LegendSlotType.AquaticActive => "Aquatic Active",
                LegendSlotType.AquaticInactive => "Aquatic Inactive",
                _ => ""
            };

            GetSelectableLegends(icon.LegendSlot);
        }

        public override void RecalculateLayout()
        {
            if (TerrestrialSwapButton is null) return;

            base.RecalculateLayout();

            int xOffset = 90;

            _legends[LegendSlotType.TerrestrialInactive].SetLocation(xOffset, 45);
            int legendSize = _legends[LegendSlotType.TerrestrialInactive].Width;

            TerrestrialSwapButton.SetBounds(new(xOffset + 2 + legendSize - ((int)(legendSize / 1.5) / 2), 40 + (legendSize / 2), (int)(legendSize / 1.5), (int)(legendSize / 1.5)));

            _legends[LegendSlotType.TerrestrialActive].SetLocation(xOffset + 5 + legendSize, 45);

            xOffset += 10 + legendSize + 320;
            _legends[LegendSlotType.AquaticInactive].SetLocation(xOffset, 45);

            AquaticSwapButton.SetBounds(new(xOffset + 2 + legendSize - ((int)(legendSize / 1.5) / 2), 40 + (legendSize / 2), (int)(legendSize / 1.5), (int)(legendSize / 1.5)));

            _legends[LegendSlotType.AquaticActive].SetLocation(xOffset + 5 + legendSize, 45);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

        }

        protected override void ApplyTemplate()
        {
            if (TemplatePresenter?.Template is null) return;
            if (!Data.IsLoaded) return;

            base.ApplyTemplate();

            foreach (LegendSlotType slot in Enum.GetValues(typeof(LegendSlotType)))
            {
                _legends[slot].Legend = TemplatePresenter?.Template?.Legends?[slot];
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            var r = _legends[LegendSlotType.TerrestrialActive].LocalBounds;
            spriteBatch.DrawStringOnCtrl(this, TemplatePresenter.LegendSlot is LegendSlotType.TerrestrialActive ? strings.ActiveLegend : strings.InactiveLegend, Content.DefaultFont16, new(r.Right + 5, r.Center.Y - (Content.DefaultFont14.LineHeight / 2), 100, Content.DefaultFont16.LineHeight), Color.White);

            r = _legends[LegendSlotType.AquaticActive].LocalBounds;
            spriteBatch.DrawStringOnCtrl(this, TemplatePresenter.LegendSlot is LegendSlotType.TerrestrialActive ? strings.ActiveLegend : strings.InactiveLegend, Content.DefaultFont16, new(r.Right + 5, r.Center.Y - (Content.DefaultFont14.LineHeight / 2), 100, Content.DefaultFont16.LineHeight), Color.White);
        }

        protected override void DisposeControl()
        {

            TemplatePresenter.LegendSlotChanged -= TemplatePresenter_LegendSlotChanged;
            _legendSelector?.Dispose();

            base.DisposeControl();
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

        private void GetSelectableLegends(LegendSlotType legendSlot)
        {
            if (!Data.IsLoaded) return;

            _selectedLegendSlot = legendSlot;

            var legends = Data.Professions[Gw2Sharp.Models.ProfessionType.Revenant].Legends.Where(e =>
            e.Value.Specialization == 0 || e.Value.Specialization == TemplatePresenter.Template.EliteSpecialization?.Id).Select(e => e.Value);
            _legendSelector.SetItems(legends);
        }
    }
}
