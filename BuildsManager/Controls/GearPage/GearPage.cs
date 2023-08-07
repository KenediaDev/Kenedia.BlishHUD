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
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.Core.Extensions;
using ItemWeightType = Gw2Sharp.WebApi.V2.Models.ItemWeightType;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class GearPage : Blish_HUD.Controls.Container
    {
        private readonly TexturesService _texturesService;
        private readonly TextBox _gearCodeBox;
        private readonly ImageButton _copyButton;

        private Template _template;
        private Rectangle _headerBounds;

        private Dictionary<GearTemplateSlot, BaseSlotControl> _slots = new();

        private FramedImage _framedSpecIcon;
        private SelectionPanel _selectionPanel;
        private readonly DetailedTexture _pve = new(2229699, 2229700);
        private readonly DetailedTexture _pvp = new(2229701, 2229702);
        private readonly ProfessionRaceSelection _professionRaceSelection;

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
                {GearTemplateSlot.Relic, new RelicControl(GearTemplateSlot.Relic, this)},
            };

            _professionRaceSelection = new()
            {
                Parent = this,
                Visible = false,
                ClipsBounds = false,
                ZIndex = 16,
            };
        }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (temp != null) temp.PropertyChanged -= TemplateChanged;

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

                _pve.Bounds = new(secondColumn, _headerBounds.Bottom + 5, 64, 64);
                _pvp.Bounds = new(secondColumn, _headerBounds.Bottom + 5, 64, 64);

                _framedSpecIcon.Location = new(secondColumn + 270 - 45, _headerBounds.Bottom + 5);
                _framedSpecIcon.Size = new(64, 64);

                /// Change to 3 once we get the JadeBotCore implemented
                int amount = 2;
                int padding = (_framedSpecIcon.Left - _pve.Bounds.Right - (45 * amount)) / (amount + 1);
                
                _slots[GearTemplateSlot.Nourishment].Location = new(secondColumn, _pve.Bounds.Bottom + 20);
                _slots[GearTemplateSlot.Utility].Location = new(secondColumn, _slots[GearTemplateSlot.Nourishment].Bottom + 5);
                _slots[GearTemplateSlot.JadeBotCore].Location = new(secondColumn, _slots[GearTemplateSlot.Utility].Bottom + 20);
                _slots[GearTemplateSlot.Relic].Location = new(secondColumn, _slots[GearTemplateSlot.JadeBotCore].Bottom + 5);

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

                _slots[GearTemplateSlot.Back].Location = new(secondColumn, _slots[GearTemplateSlot.Relic].Bottom + 20);
                _slots[GearTemplateSlot.Accessory_1].Location = new(_slots[GearTemplateSlot.Back].Right + 3, _slots[GearTemplateSlot.Back].Top);
                _slots[GearTemplateSlot.Accessory_2].Location = new(_slots[GearTemplateSlot.Accessory_1].Right + 3, _slots[GearTemplateSlot.Back].Top);

                _slots[GearTemplateSlot.Amulet].Location = new(_slots[GearTemplateSlot.Back].Left, _slots[GearTemplateSlot.Back].Bottom + 3);
                _slots[GearTemplateSlot.Ring_1].Location = new(_slots[GearTemplateSlot.Amulet].Right + 3, _slots[GearTemplateSlot.Amulet].Top);
                _slots[GearTemplateSlot.Ring_2].Location = new(_slots[GearTemplateSlot.Ring_1].Right + 3, _slots[GearTemplateSlot.Amulet].Top);

                _slots[GearTemplateSlot.AquaBreather].Location = new(_slots[GearTemplateSlot.Back].Left, _slots[GearTemplateSlot.Amulet].Bottom + 20);
                _slots[GearTemplateSlot.Aquatic].Location = new(_slots[GearTemplateSlot.AquaBreather].Left, _slots[GearTemplateSlot.AquaBreather].Bottom + 15);
                _slots[GearTemplateSlot.AltAquatic].Location = new(_slots[GearTemplateSlot.Aquatic].Left, _slots[GearTemplateSlot.Aquatic].Bottom + 8);
            }
        }

        public void ApplyTemplate()
        {
            _gearCodeBox.Text = Template?.GearTemplate?.ParseGearCode();

            if (_template != null && BuildsManager.Data.Professions.ContainsKey(_template.Profession))
            {
                _framedSpecIcon.Texture = _template.EliteSpecialization?.ProfessionIconBig ??
                    BuildsManager.Data.Professions[_template.Profession].IconBig;

                switch(_template.Profession.GetArmorType())
                {
                    case ItemWeightType.Heavy:
                        _slots[GearTemplateSlot.AquaBreather].Item.Item = BuildsManager.Data.Armors[79895];
                        _slots[GearTemplateSlot.Head].Item.Item = BuildsManager.Data.Armors[85193];
                        _slots[GearTemplateSlot.Shoulder].Item.Item = BuildsManager.Data.Armors[84875];
                        _slots[GearTemplateSlot.Chest].Item.Item = BuildsManager.Data.Armors[85084];
                        _slots[GearTemplateSlot.Hand].Item.Item = BuildsManager.Data.Armors[85140];
                        _slots[GearTemplateSlot.Leg].Item.Item = BuildsManager.Data.Armors[84887];
                        _slots[GearTemplateSlot.Foot].Item.Item = BuildsManager.Data.Armors[85055];
                        break;
                    case ItemWeightType.Medium:
                        _slots[GearTemplateSlot.AquaBreather].Item.Item = BuildsManager.Data.Armors[79838];
                        _slots[GearTemplateSlot.Head].Item.Item = BuildsManager.Data.Armors[80701];
                        _slots[GearTemplateSlot.Shoulder].Item.Item = BuildsManager.Data.Armors[80825];
                        _slots[GearTemplateSlot.Chest].Item.Item = BuildsManager.Data.Armors[84977];
                        _slots[GearTemplateSlot.Hand].Item.Item = BuildsManager.Data.Armors[85169];
                        _slots[GearTemplateSlot.Leg].Item.Item = BuildsManager.Data.Armors[85264];
                        _slots[GearTemplateSlot.Foot].Item.Item = BuildsManager.Data.Armors[80836];
                        break;
                    case ItemWeightType.Light:
                        _slots[GearTemplateSlot.AquaBreather].Item.Item = BuildsManager.Data.Armors[79873];
                        _slots[GearTemplateSlot.Head].Item.Item = BuildsManager.Data.Armors[85128];
                        _slots[GearTemplateSlot.Shoulder].Item.Item = BuildsManager.Data.Armors[84918];
                        _slots[GearTemplateSlot.Chest].Item.Item = BuildsManager.Data.Armors[85333];
                        _slots[GearTemplateSlot.Hand].Item.Item = BuildsManager.Data.Armors[85070];
                        _slots[GearTemplateSlot.Leg].Item.Item = BuildsManager.Data.Armors[85362];
                        _slots[GearTemplateSlot.Foot].Item.Item = BuildsManager.Data.Armors[80815];
                        break;
                }

                _slots[GearTemplateSlot.Back].Item.Item = BuildsManager.Data.Backs[94947];
                _slots[GearTemplateSlot.Amulet].Item.Item = BuildsManager.Data.Trinkets[79980];
                _slots[GearTemplateSlot.Accessory_1].Item.Item = BuildsManager.Data.Trinkets[80002];
                _slots[GearTemplateSlot.Accessory_2].Item.Item = BuildsManager.Data.Trinkets[80002];
                _slots[GearTemplateSlot.Ring_1].Item.Item = BuildsManager.Data.Trinkets[80058];
                _slots[GearTemplateSlot.Ring_2].Item.Item = BuildsManager.Data.Trinkets[80058];
            }

            foreach (var slot in _slots.Values)
            {
                slot.Visible = 
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

            if (_framedSpecIcon?.MouseOver == true)
            {
                _professionRaceSelection.Visible = true;
                _professionRaceSelection.Type = ProfessionRaceSelection.SelectionType.Profession;
                _professionRaceSelection.Location = RelativeMousePosition;
                _professionRaceSelection.OnClickAction = (value) => Template.Profession = (ProfessionType)value;
            }
        }

        private void TemplateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }
    }
}
