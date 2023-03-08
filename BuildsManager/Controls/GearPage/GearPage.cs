using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Screens;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.BuildsManager.Controls.Selection;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class GearPage : Blish_HUD.Controls.Container
    {
        private readonly TexturesService _texturesService;
        private readonly TextBox _gearCodeBox;
        private readonly ImageButton _copyButton;
        private readonly Panel _statPanel;
        private readonly StatSummary _stats;

        private Template _template;
        private Rectangle _headerBounds;
        private Rectangle _statPanelHeaderBounds;

        private Dictionary<GearTemplateSlot, GearSlotControl> _slots = new();

        private InfusionControl _infusions;
        private NourishmentControl _nourishment;
        private UtilityControl _utility;
        private SelectionPanel _selectionPanel;
        private readonly DetailedTexture _specIcon = new(1770214);
        private readonly DetailedTexture _pve = new(2229699, 2229700);
        private readonly DetailedTexture _pvp = new(2229701, 2229702);

        public GearPage(TexturesService _texturesService)
        {
            this._texturesService = _texturesService;
            WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill;

            _copyButton = new()
            {
                Parent = this,
                Location = new(0, 0),
                Texture = AsyncTexture2D.FromAssetId(2208345),
                HoveredTexture = AsyncTexture2D.FromAssetId(2208347),
                Size = new(26),
                ClickAction = (m) =>
                {
                    try
                    {
                        _ = ClipboardUtil.WindowsClipboardService.SetTextAsync(_gearCodeBox.Text);
                    }
                    catch (ArgumentException)
                    {
                        Blish_HUD.Controls.ScreenNotification.ShowNotification("Failed to set the clipboard text!", Blish_HUD.Controls.ScreenNotification.NotificationType.Error);
                    }
                    catch
                    {
                    }
                }
            };

            _gearCodeBox = new()
            {
                Parent = this,
                Location = new(_copyButton.Right + 2, 0),
                EnterPressedAction = (code) =>
                {
                    Template.GearTemplate.LoadFromCode(code);
                    ApplyTemplate();
                }
            };

            _statPanel = new()
            {
                Parent = this,
                Title = "♥",
                Height = 370,
                Width = 270,
                ShowBorder = true,
            };

            _stats = new()
            {
                Parent = _statPanel,
                Height = _statPanel.Height,
                Width = _statPanel.Width,
            };

            _infusions = new()
            {
                Parent = this,
            };
            _nourishment = new()
            {
                Parent = this,
            };
            _utility = new()
            {
                Parent = this,
            };

            _slots = new()
            {
                {GearTemplateSlot.Head, new GearSlotControl(GearTemplateSlot.Head, this)},
                {GearTemplateSlot.Shoulder, new GearSlotControl(GearTemplateSlot.Shoulder, this)},
                {GearTemplateSlot.Chest, new GearSlotControl(GearTemplateSlot.Chest, this)},
                {GearTemplateSlot.Hand, new GearSlotControl(GearTemplateSlot.Hand, this)},
                {GearTemplateSlot.Leg, new GearSlotControl(GearTemplateSlot.Leg, this)},
                {GearTemplateSlot.Foot, new GearSlotControl(GearTemplateSlot.Foot, this)},
                {GearTemplateSlot.MainHand, new GearSlotControl(GearTemplateSlot.MainHand, this)},
                {GearTemplateSlot.OffHand, new GearSlotControl(GearTemplateSlot.OffHand, this){ Height = 55 }},
                {GearTemplateSlot.Aquatic, new GearSlotControl(GearTemplateSlot.Aquatic, this){ Height = 55}},
                {GearTemplateSlot.AltMainHand, new GearSlotControl(GearTemplateSlot.AltMainHand, this)},
                {GearTemplateSlot.AltOffHand, new GearSlotControl(GearTemplateSlot.AltOffHand, this){ Height = 55 }},
                {GearTemplateSlot.AltAquatic, new GearSlotControl(GearTemplateSlot.AltAquatic, this){ Height = 55 }},
                {GearTemplateSlot.Back, new GearSlotControl(GearTemplateSlot.Back, this) { Width = 85 } },
                {GearTemplateSlot.Amulet, new GearSlotControl(GearTemplateSlot.Amulet, this){ Width = 85 }},
                {GearTemplateSlot.Accessory_1, new GearSlotControl(GearTemplateSlot.Accessory_1, this){ Width = 85 }},
                {GearTemplateSlot.Accessory_2, new GearSlotControl(GearTemplateSlot.Accessory_2, this){ Width = 85}},
                {GearTemplateSlot.Ring_1, new GearSlotControl(GearTemplateSlot.Ring_1, this){ Width = 85 }},
                {GearTemplateSlot.Ring_2, new GearSlotControl(GearTemplateSlot.Ring_2, this){ Width = 85 }},
                {GearTemplateSlot.AquaBreather, new GearSlotControl(GearTemplateSlot.AquaBreather, this){ Height = 55 }},
                {GearTemplateSlot.PvpAmulet, new GearSlotControl(GearTemplateSlot.PvpAmulet, this)},
            };

            _utility.ClickAction = () =>
            {
                SelectionPanel?.SetGearAnchor(_utility, _utility.AbsoluteBounds, GearTemplateSlot.Utility, "Utilities");
            };

            _nourishment.ClickAction = () =>
            {
                if (SelectionPanel != null)
                {
                    SelectionPanel?.SetGearAnchor(_nourishment, _nourishment.AbsoluteBounds, GearTemplateSlot.Nourishment, "Nourishments");
                }
            };
        }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate, value != null))
                {
                    if (temp != null) temp.Changed -= TemplateChanged;
                    if (temp != null) temp.Changed -= TemplateChanged;

                    _infusions.Template = _template;
                    _nourishment.Template = _template;
                    _utility.Template = _template;
                    _stats.Template = _template;

                    foreach (var slot in _slots)
                    {
                        slot.Value.Template = _template;
                    }

                    if (_template != null) _template.Changed += TemplateChanged;
                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        public SelectionPanel SelectionPanel
        {
            get => _selectionPanel; set
            {
                if (_selectionPanel == value) return;
                _selectionPanel = value;

                foreach (var slot in _slots.Values)
                {
                    slot.SelectionPanel = value;
                }
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_gearCodeBox != null) _gearCodeBox.Width = Width - _gearCodeBox.Left;

            if (_slots.Count > 0)
            {
                _headerBounds = new(0, _copyButton.Top, 300, _copyButton.Height);

                int secondColumn = _slots[GearTemplateSlot.AquaBreather].Width + 10;

                _slots[GearTemplateSlot.Head].Location = new(0, _gearCodeBox.Bottom + 5);
                _slots[GearTemplateSlot.Shoulder].Location = new(_slots[GearTemplateSlot.Head].Left, _slots[GearTemplateSlot.Head].Bottom + 3);
                _slots[GearTemplateSlot.Chest].Location = new(_slots[GearTemplateSlot.Shoulder].Left, _slots[GearTemplateSlot.Shoulder].Bottom + 3);
                _slots[GearTemplateSlot.Hand].Location = new(_slots[GearTemplateSlot.Chest].Left, _slots[GearTemplateSlot.Chest].Bottom + 3);
                _slots[GearTemplateSlot.Leg].Location = new(_slots[GearTemplateSlot.Hand].Left, _slots[GearTemplateSlot.Hand].Bottom + 3);
                _slots[GearTemplateSlot.Foot].Location = new(_slots[GearTemplateSlot.Leg].Left, _slots[GearTemplateSlot.Leg].Bottom + 3);

                _slots[GearTemplateSlot.PvpAmulet].Location = new(_slots[GearTemplateSlot.Leg].Left, _slots[GearTemplateSlot.Leg].Bottom + 3);

                _slots[GearTemplateSlot.MainHand].Location = new(_slots[GearTemplateSlot.Foot].Left, _slots[GearTemplateSlot.Foot].Bottom + 15);
                _slots[GearTemplateSlot.OffHand].Location = new(_slots[GearTemplateSlot.MainHand].Left + 4, _slots[GearTemplateSlot.MainHand].Bottom + 3);

                _slots[GearTemplateSlot.AltMainHand].Location = new(_slots[GearTemplateSlot.OffHand].Left, _slots[GearTemplateSlot.OffHand].Bottom + 35);
                _slots[GearTemplateSlot.AltOffHand].Location = new(_slots[GearTemplateSlot.AltMainHand].Left + 4, _slots[GearTemplateSlot.AltMainHand].Bottom + 3);

                _slots[GearTemplateSlot.Back].Location = new(secondColumn, _slots[GearTemplateSlot.Foot].Bottom + 25);
                _slots[GearTemplateSlot.Accessory_1].Location = new(_slots[GearTemplateSlot.Back].Right + 3, _slots[GearTemplateSlot.Back].Top);
                _slots[GearTemplateSlot.Accessory_2].Location = new(_slots[GearTemplateSlot.Accessory_1].Right + 3, _slots[GearTemplateSlot.Back].Top);

                _slots[GearTemplateSlot.Amulet].Location = new(_slots[GearTemplateSlot.Back].Left, _slots[GearTemplateSlot.Back].Bottom + 3);
                _slots[GearTemplateSlot.Ring_1].Location = new(_slots[GearTemplateSlot.Amulet].Right + 3, _slots[GearTemplateSlot.Amulet].Top);
                _slots[GearTemplateSlot.Ring_2].Location = new(_slots[GearTemplateSlot.Ring_1].Right + 3, _slots[GearTemplateSlot.Amulet].Top);

                _slots[GearTemplateSlot.AquaBreather].Location = new(_slots[GearTemplateSlot.Back].Left, _slots[GearTemplateSlot.Amulet].Bottom + 15);
                _slots[GearTemplateSlot.Aquatic].Location = new(_slots[GearTemplateSlot.AquaBreather].Left, _slots[GearTemplateSlot.AquaBreather].Bottom + 3);
                _slots[GearTemplateSlot.AltAquatic].Location = new(_slots[GearTemplateSlot.Aquatic].Left, _slots[GearTemplateSlot.Aquatic].Bottom + 3);

                //_nourishment.Location = new(_slots[GearTemplateSlot.Back].Left, _slots[GearTemplateSlot.AquaBreather].Top);
                //_utility.Location = new(_nourishment.Left, _nourishment.Bottom + 5);

                _infusions.Location = new(_slots[GearTemplateSlot.Amulet].Left, _slots[GearTemplateSlot.Ring_2].Bottom + 15);

                _pve.Bounds = new(secondColumn, _headerBounds.Bottom + 5, 45, 45);
                _pvp.Bounds = new(secondColumn, _headerBounds.Bottom + 5, 45, 45);

                _nourishment.Location = new(_pve.Bounds.Right + 3 + 35, _pve.Bounds.Top);
                _utility.Location = new(_nourishment.Right + 3, _nourishment.Top);

                _statPanel.Location = new(secondColumn - 5, _pve.Bounds.Bottom + 5);
                _statPanelHeaderBounds = new(_statPanel.Left + 15, _statPanel.Top, _statPanel.Width - 15, 32);

                _specIcon.Bounds = new(_statPanel.Right - 45, _headerBounds.Bottom + 5, 45, 45);
            }
        }

        public void ApplyTemplate()
        {
            _gearCodeBox.Text = Template?.GearTemplate?.ParseGearCode();

            _infusions.Visible = false;
            _nourishment.Visible = Template.PvE;
            _utility.Visible = Template.PvE;

            foreach (var slot in _slots.Values)
            {
                slot.Visible = !Template.PvE
                    ? slot.GearSlot is GearTemplateSlot.MainHand or GearTemplateSlot.AltMainHand or GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand or GearTemplateSlot.PvpAmulet
                    : slot.GearSlot is not GearTemplateSlot.PvpAmulet;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            if (Template != null)
            {
                (Template.PvE ? _pve : _pvp).Draw(this, spriteBatch, RelativeMousePosition);
                _specIcon.Draw(this, spriteBatch, RelativeMousePosition);

                //spriteBatch.DrawStringOnCtrl(this, Template.PvE ? "PvE Build" : "PvP Build", Content.DefaultFont18, _headerBounds, Color.White);
                spriteBatch.DrawStringOnCtrl(this, "Attributes", Content.DefaultFont18, _statPanelHeaderBounds, Color.White);
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if ((Template != null && Template.PvE ? _pve : _pvp).Hovered)
            {
                Template.PvE = !Template.PvE;
            }
        }

        private void TemplateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }
    }
}
