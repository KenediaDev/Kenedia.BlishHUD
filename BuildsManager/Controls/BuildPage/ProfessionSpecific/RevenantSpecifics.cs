using Blish_HUD;
using Blish_HUD.Input;
using Gw2Sharp;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
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

        private readonly LegendSelector _legendSelector;

        private LegendSlotType _selectedLegendSlot;

        private LegendIcon _selectorAnchor;

        public RevenantSpecifics(TemplatePresenter template) : base(template)
        {
            _legendSelector = new()
            {
                Parent = Graphics.SpriteScreen,
                OnClickAction = (legend) =>
                {
                    if (_selectedLegendSlot is LegendSlotType.TerrestrialActive or LegendSlotType.TerrestrialInactive || !legend.Swap.Flags.HasFlag(SkillFlag.NoUnderwater))
                    {
                        var slot = GetOtherSlot(_selectorAnchor.LegendSlot);
                        var otherLegend = TemplatePresenter.Template.Legends[slot];

                        if (otherLegend is not null && otherLegend == legend)
                        {
                            TemplatePresenter.Template.SetLegend(_selectorAnchor.Legend, slot);
                        }

                        TemplatePresenter.Template.SetLegend(legend, _selectorAnchor.LegendSlot);
                        _selectorAnchor.Legend = legend;
                    }

                    _legendSelector?.Hide();
                }
            };

            foreach (var c in _legends.Values)
            {
                c.Parent = this;
                c.LeftClickAction = SetSelector;
                c.RightClickAction = SetSelector;
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
            base.RecalculateLayout();

            int xOffset = 90;

            _legends[LegendSlotType.TerrestrialInactive].SetLocation(xOffset, 45);
            int legendSize = _legends[LegendSlotType.TerrestrialInactive].Width;

            _swaps[0].Bounds = new(xOffset + 2 + legendSize - ((int)(legendSize / 1.5) / 2), 40 + (legendSize/2), (int)(legendSize / 1.5), (int)(legendSize / 1.5));

            _legends[LegendSlotType.TerrestrialActive].SetLocation(xOffset + 5 + legendSize, 45);

            xOffset += 10 + legendSize + 320;
            _legends[LegendSlotType.AquaticInactive].SetLocation(xOffset, 45);

            _swaps[1].Bounds = new(xOffset + 2 + legendSize - ((int)(legendSize / 1.5) / 2), 40 + (legendSize / 2), (int)(legendSize / 1.5), (int)(legendSize / 1.5));

            _legends[LegendSlotType.AquaticActive].SetLocation(xOffset + 5 + legendSize, 45);
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
        }

        protected override void ApplyTemplate()
        {
            if (TemplatePresenter?.Template is null) return;
            base.ApplyTemplate();

            foreach (LegendSlotType slot in Enum.GetValues(typeof(LegendSlotType)))
            {
                _legends[slot].Legend = TemplatePresenter?.Template?.Legends?[slot];
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            foreach (var swap in _swaps)
            {
                swap.Draw(this, spriteBatch, RelativeMousePosition);
            }

            var r = _legends[LegendSlotType.TerrestrialActive].LocalBounds;
            spriteBatch.DrawStringOnCtrl(this, TemplatePresenter.LegendSlot is LegendSlotType.TerrestrialActive ? strings.ActiveLegend : strings.InactiveLegend, Content.DefaultFont16, new(r.Right + 5, r.Center.Y - (Content.DefaultFont14.LineHeight / 2), 100, Content.DefaultFont16.LineHeight), Color.White);

            r = _legends[LegendSlotType.AquaticActive].LocalBounds;
            spriteBatch.DrawStringOnCtrl(this, TemplatePresenter.LegendSlot is LegendSlotType.TerrestrialActive ? strings.ActiveLegend : strings.InactiveLegend, Content.DefaultFont16, new(r.Right + 5, r.Center.Y - (Content.DefaultFont14.LineHeight / 2), 100, Content.DefaultFont16.LineHeight), Color.White);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _legendSelector?.Dispose();
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
            _selectedLegendSlot = legendSlot;

            var legends = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Revenant].Legends.Where(e =>
            e.Value.Specialization == 0 || e.Value.Specialization == TemplatePresenter.Template.EliteSpecialization?.Id).Select(e => e.Value);
            _legendSelector.SetItems(legends);
        }
    }
}
