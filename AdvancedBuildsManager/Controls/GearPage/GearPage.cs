using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Kenedia.Modules.AdvancedBuildsManager.Controls.Selection;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.GearPage
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
        private Dictionary<GearTemplateSlot, BaseSlotControl> _slots = new();

        private FramedImage _framedSpecIcon;
        private SelectionPanel _selectionPanel;
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
                Height = 365,
                Width = 270,
                ShowBorder = true,
                ShowRightBorder = true,
                Visible = false,
            };

            _stats = new()
            {
                Parent = this,
                Height = _statPanel.Height,
                Width = _statPanel.Width,
            };

            _framedSpecIcon = new()
            {
                Parent = this,
            };

            _slots = new()
            {
                {GearTemplateSlot.Head, new ArmorSlotControl(GearTemplateSlot.Head, this)},
                {GearTemplateSlot.Shoulder, new ArmorSlotControl(GearTemplateSlot.Shoulder, this)},
                {GearTemplateSlot.Chest, new ArmorSlotControl(GearTemplateSlot.Chest, this)},
                {GearTemplateSlot.Hand, new ArmorSlotControl(GearTemplateSlot.Hand, this)},
                {GearTemplateSlot.Leg, new ArmorSlotControl(GearTemplateSlot.Leg, this)},
                {GearTemplateSlot.Foot, new ArmorSlotControl(GearTemplateSlot.Foot, this)},
                {GearTemplateSlot.MainHand, new WeaponSlotControl(GearTemplateSlot.MainHand, this)},
                {GearTemplateSlot.OffHand, new WeaponSlotControl(GearTemplateSlot.OffHand, this){ Height = 55 }},
                {GearTemplateSlot.Aquatic, new AquaticSlotControl(GearTemplateSlot.Aquatic, this){ Height = 55}},
                {GearTemplateSlot.AltMainHand, new WeaponSlotControl(GearTemplateSlot.AltMainHand, this)},
                {GearTemplateSlot.AltOffHand, new WeaponSlotControl(GearTemplateSlot.AltOffHand, this){ Height = 55 }},
                {GearTemplateSlot.AltAquatic, new AquaticSlotControl(GearTemplateSlot.AltAquatic, this){ Height = 55 }},
                {GearTemplateSlot.Back, new JuwellerySlotControl(GearTemplateSlot.Back, this) { Width = 85 } },
                {GearTemplateSlot.Amulet, new AmuletSlotControl(GearTemplateSlot.Amulet, this){ Width = 85 }},
                {GearTemplateSlot.Accessory_1, new JuwellerySlotControl(GearTemplateSlot.Accessory_1, this){ Width = 85 }},
                {GearTemplateSlot.Accessory_2, new JuwellerySlotControl(GearTemplateSlot.Accessory_2, this){ Width = 85}},
                {GearTemplateSlot.Ring_1, new JuwellerySlotControl(GearTemplateSlot.Ring_1, this){ Width = 85 }},
                {GearTemplateSlot.Ring_2, new JuwellerySlotControl(GearTemplateSlot.Ring_2, this){ Width = 85 }},
                {GearTemplateSlot.AquaBreather, new ArmorSlotControl(GearTemplateSlot.AquaBreather, this){ Height = 55 }},
                {GearTemplateSlot.PvpAmulet, new PvpAmuletSlotControl (GearTemplateSlot.PvpAmulet, this)},
                {GearTemplateSlot.Utility, new UtilityControl(GearTemplateSlot.Utility, this)},
                {GearTemplateSlot.Nourishment, new NourishmentControl(GearTemplateSlot.Nourishment, this)},
                {GearTemplateSlot.JadeBotCore, new JadeBotControl(GearTemplateSlot.JadeBotCore, this)},
            };

            //_slots[GearTemplateSlot.Utility].ClickAction = () =>
            //{
            //    SelectionPanel?.SetGearAnchor(_slots[GearTemplateSlot.Utility], _slots[GearTemplateSlot.Utility].AbsoluteBounds, GearTemplateSlot.Utility, "Utilities");
            //};

            //_slots[GearTemplateSlot.Nourishment].ClickAction = () =>
            //{
            //    if (SelectionPanel != null)
            //    {
            //        SelectionPanel?.SetGearAnchor(_slots[GearTemplateSlot.Nourishment], _slots[GearTemplateSlot.Nourishment].AbsoluteBounds, GearTemplateSlot.Nourishment, "Nourishments");
            //    }
            //};

            //_slots[GearTemplateSlot.JadeBotCore].ClickAction = () =>
            //{
            //    if (SelectionPanel != null)
            //    {
            //        SelectionPanel?.SetGearAnchor(_slots[GearTemplateSlot.JadeBotCore], _slots[GearTemplateSlot.JadeBotCore].AbsoluteBounds, GearTemplateSlot.JadeBotCore, "JadeBotCore");
            //    }
            //};
        }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (temp != null) temp.PropertyChanged -= TemplateChanged;

                    _stats.Template = _template;

                    foreach (var slot in _slots)
                    {
                        slot.Value.Template = _template;
                    }

                    if (_template != null) _template.PropertyChanged += TemplateChanged;
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

                int secondColumn = _slots[GearTemplateSlot.AquaBreather].Width + 10;
                int gearSpacing = 8;

                _headerBounds = new(0, _copyButton.Top, 300, _copyButton.Height);

                _pve.Bounds = new(secondColumn, _headerBounds.Bottom + 5, 45, 45);
                _pvp.Bounds = new(secondColumn, _headerBounds.Bottom + 5, 45, 45);

                _stats.Location = new(secondColumn - 5, _pve.Bounds.Bottom + 5);
                _framedSpecIcon.Location = new(_stats.Right - 45, _headerBounds.Bottom + 5);
                _framedSpecIcon.Size = new(45, 45);

                /// Change to 3 once we get the JadeBotCore implemented
                int amount = 2;
                int padding = (_framedSpecIcon.Left - _pve.Bounds.Right - (45 * amount)) / (amount + 1);
                
                _slots[GearTemplateSlot.Nourishment].Location = new(_pve.Bounds.Right + padding + ((45 + padding) * 0), _pve.Bounds.Top);
                _slots[GearTemplateSlot.Utility].Location = new(_pve.Bounds.Right + padding + ((45 + padding) * 1), _pve.Bounds.Top);
                _slots[GearTemplateSlot.JadeBotCore].Location = new(_pve.Bounds.Right + padding + ((45 + padding) * 2), _pve.Bounds.Top);

                _slots[GearTemplateSlot.Head].Location = new(0, _gearCodeBox.Bottom + 5);
                _slots[GearTemplateSlot.Shoulder].Location = new(_slots[GearTemplateSlot.Head].Left, _slots[GearTemplateSlot.Head].Bottom + gearSpacing);
                _slots[GearTemplateSlot.Chest].Location = new(_slots[GearTemplateSlot.Shoulder].Left, _slots[GearTemplateSlot.Shoulder].Bottom + gearSpacing);
                _slots[GearTemplateSlot.Hand].Location = new(_slots[GearTemplateSlot.Chest].Left, _slots[GearTemplateSlot.Chest].Bottom + gearSpacing);
                _slots[GearTemplateSlot.Leg].Location = new(_slots[GearTemplateSlot.Hand].Left, _slots[GearTemplateSlot.Hand].Bottom + gearSpacing);
                _slots[GearTemplateSlot.Foot].Location = new(_slots[GearTemplateSlot.Leg].Left, _slots[GearTemplateSlot.Leg].Bottom + gearSpacing);

                _slots[GearTemplateSlot.PvpAmulet].Location = new(_slots[GearTemplateSlot.Leg].Left, _slots[GearTemplateSlot.Leg].Bottom + gearSpacing);

                _slots[GearTemplateSlot.MainHand].Location = new(_slots[GearTemplateSlot.Foot].Left, _slots[GearTemplateSlot.Foot].Bottom + 15);
                _slots[GearTemplateSlot.OffHand].Location = new(_slots[GearTemplateSlot.MainHand].Left + 4, _slots[GearTemplateSlot.MainHand].Bottom + 8);

                _slots[GearTemplateSlot.AltMainHand].Location = new(_slots[GearTemplateSlot.OffHand].Left, _slots[GearTemplateSlot.OffHand].Bottom + 35);
                _slots[GearTemplateSlot.AltOffHand].Location = new(_slots[GearTemplateSlot.AltMainHand].Left + 4, _slots[GearTemplateSlot.AltMainHand].Bottom + 8);

                _slots[GearTemplateSlot.Back].Location = new(secondColumn, _stats.Bottom + 15);
                _slots[GearTemplateSlot.Accessory_1].Location = new(_slots[GearTemplateSlot.Back].Right + 3, _slots[GearTemplateSlot.Back].Top);
                _slots[GearTemplateSlot.Accessory_2].Location = new(_slots[GearTemplateSlot.Accessory_1].Right + 3, _slots[GearTemplateSlot.Back].Top);

                _slots[GearTemplateSlot.Amulet].Location = new(_slots[GearTemplateSlot.Back].Left, _slots[GearTemplateSlot.Back].Bottom + 3);
                _slots[GearTemplateSlot.Ring_1].Location = new(_slots[GearTemplateSlot.Amulet].Right + 3, _slots[GearTemplateSlot.Amulet].Top);
                _slots[GearTemplateSlot.Ring_2].Location = new(_slots[GearTemplateSlot.Ring_1].Right + 3, _slots[GearTemplateSlot.Amulet].Top);

                _slots[GearTemplateSlot.AquaBreather].Location = new(_slots[GearTemplateSlot.Back].Left, _slots[GearTemplateSlot.Amulet].Bottom + 10);
                _slots[GearTemplateSlot.Aquatic].Location = new(_slots[GearTemplateSlot.AquaBreather].Left, _slots[GearTemplateSlot.AquaBreather].Bottom + 15);
                _slots[GearTemplateSlot.AltAquatic].Location = new(_slots[GearTemplateSlot.Aquatic].Left, _slots[GearTemplateSlot.Aquatic].Bottom + 8);
            }
        }

        public void ApplyTemplate()
        {
            _gearCodeBox.Text = Template?.GearTemplate?.ParseGearCode();

            if (_template != null && AdvancedBuildsManager.Data.Professions.ContainsKey(_template.Profession))
            {
                _framedSpecIcon.Texture = _template.EliteSpecialization?.ProfessionIconBig ??
                    AdvancedBuildsManager.Data.Professions[_template.Profession].IconBig;
            }

            foreach (var slot in _slots.Values)
            {
                slot.Visible = 
                    (slot.GearSlot is not GearTemplateSlot.JadeBotCore) &&
                    (slot.GearSlot is not GearTemplateSlot.AltAquatic || Template.Profession is not Gw2Sharp.Models.ProfessionType.Engineer and not Gw2Sharp.Models.ProfessionType.Elementalist) &&
                    (Template?.PvE == false
                    ? slot.GearSlot is GearTemplateSlot.MainHand or GearTemplateSlot.AltMainHand or GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand or GearTemplateSlot.PvpAmulet
                    : slot.GearSlot is not GearTemplateSlot.PvpAmulet);
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

                //spriteBatch.DrawStringOnCtrl(this, Template.PvE ? "PvE Build" : "PvP Build", Content.DefaultFont18, _headerBounds, Color.White);
                //spriteBatch.DrawStringOnCtrl(this, "Attributes", Content.DefaultFont18, _statPanelHeaderBounds, Color.White);
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
