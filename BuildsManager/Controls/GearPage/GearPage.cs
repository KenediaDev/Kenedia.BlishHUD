using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.Core.Extensions;
using ItemWeightType = Gw2Sharp.WebApi.V2.Models.ItemWeightType;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class GearPage : Blish_HUD.Controls.Container
    {
        private readonly TexturesService _texturesService;
        private readonly TextBox _gearCodeBox;
        private readonly ImageButton _copyButton;
        private readonly ButtonImage _framedSpecIcon;

        private Rectangle _headerBounds;

        private Dictionary<TemplateSlotType, GearSlot> _templateSlots = new();

        private SelectionPanel _selectionPanel;
        private TemplatePresenter _templatePresenter;
        private readonly DetailedTexture _pve = new(2229699, 2229700);
        private readonly DetailedTexture _pvp = new(2229701, 2229702);
        private readonly ProfessionRaceSelection _professionRaceSelection;

        public GearPage(TexturesService _texturesService, TemplatePresenter templatePresenter)
        {
            this._texturesService = _texturesService;
            TemplatePresenter = templatePresenter;

            WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill;

            string gearCodeDisclaimer = strings.EquipmentCodeDisclaimer;
            _copyButton = new()
            {
                Parent = this,
                Location = new(0, 0),
                Texture = AsyncTexture2D.FromAssetId(2208345),
                HoveredTexture = AsyncTexture2D.FromAssetId(2208347),
                Size = new(26),
                SetLocalizedTooltip = () => gearCodeDisclaimer,
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
                EnterPressedAction = (txt) => TemplatePresenter.Template?.LoadGearFromCode(txt, true),
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size11, ContentService.FontStyle.Regular),
                SetLocalizedTooltip = () => gearCodeDisclaimer,
            };

            _framedSpecIcon = new()
            {
                Parent = this,
            };

            _templateSlots = new()
            {
                {TemplateSlotType.Head, new ArmorSlot(TemplateSlotType.Head, this, TemplatePresenter)},
                {TemplateSlotType.Shoulder, new ArmorSlot(TemplateSlotType.Shoulder, this, TemplatePresenter)},
                {TemplateSlotType.Chest, new ArmorSlot(TemplateSlotType.Chest, this, TemplatePresenter)},
                {TemplateSlotType.Hand, new ArmorSlot(TemplateSlotType.Hand, this, TemplatePresenter)},
                {TemplateSlotType.Leg, new ArmorSlot(TemplateSlotType.Leg, this, TemplatePresenter)},
                {TemplateSlotType.Foot, new ArmorSlot(TemplateSlotType.Foot, this, TemplatePresenter)},

                {TemplateSlotType.MainHand, new WeaponSlot(TemplateSlotType.MainHand, this, TemplatePresenter)},
                {TemplateSlotType.OffHand, new WeaponSlot(TemplateSlotType.OffHand, this, TemplatePresenter){ Height = 55 }},
                {TemplateSlotType.AltMainHand, new WeaponSlot(TemplateSlotType.AltMainHand, this, TemplatePresenter)},
                {TemplateSlotType.AltOffHand, new WeaponSlot(TemplateSlotType.AltOffHand, this, TemplatePresenter){ Height = 55 }},

                {TemplateSlotType.AquaBreather, new ArmorSlot(TemplateSlotType.AquaBreather, this, TemplatePresenter){ Height = 55 }},
                {TemplateSlotType.Aquatic, new AquaticWeaponSlot(TemplateSlotType.Aquatic, this, TemplatePresenter){ Height = 55 }},
                {TemplateSlotType.AltAquatic, new AquaticWeaponSlot(TemplateSlotType.AltAquatic, this, TemplatePresenter){ Height = 55 }},

                {TemplateSlotType.Back, new BackSlot(TemplateSlotType.Back, this, TemplatePresenter){ Width = 85}},
                {TemplateSlotType.Amulet, new AmuletSlot(TemplateSlotType.Amulet, this, TemplatePresenter){ Width = 85}},
                {TemplateSlotType.Ring_1, new RingSlot(TemplateSlotType.Ring_1, this, TemplatePresenter){ Width = 85}},
                {TemplateSlotType.Ring_2, new RingSlot(TemplateSlotType.Ring_2, this, TemplatePresenter){ Width = 85}},
                {TemplateSlotType.Accessory_1, new AccessoireSlot(TemplateSlotType.Accessory_1, this, TemplatePresenter){ Width = 85}},
                {TemplateSlotType.Accessory_2, new AccessoireSlot(TemplateSlotType.Accessory_2, this, TemplatePresenter){ Width = 85}},

                {TemplateSlotType.PvpAmulet, new PvpAmuletSlot(TemplateSlotType.PvpAmulet, this, TemplatePresenter) {Visible = false } },
                {TemplateSlotType.Nourishment, new NourishmentSlot(TemplateSlotType.Nourishment, this, TemplatePresenter)},
                {TemplateSlotType.Enhancement, new EnhancementSlot(TemplateSlotType.Enhancement, this, TemplatePresenter)},
                {TemplateSlotType.JadeBotCore, new JadeBotCoreSlot(TemplateSlotType.JadeBotCore, this, TemplatePresenter)},
                {TemplateSlotType.Relic, new RelicSlot(TemplateSlotType.Relic, this, TemplatePresenter)},
            };

            (_templateSlots[TemplateSlotType.MainHand] as WeaponSlot).OtherHandSlot = _templateSlots[TemplateSlotType.OffHand] as WeaponSlot;
            (_templateSlots[TemplateSlotType.AltMainHand] as WeaponSlot).OtherHandSlot = _templateSlots[TemplateSlotType.AltOffHand] as WeaponSlot;

            (_templateSlots[TemplateSlotType.OffHand] as WeaponSlot).OtherHandSlot = _templateSlots[TemplateSlotType.MainHand] as WeaponSlot;
            (_templateSlots[TemplateSlotType.AltOffHand] as WeaponSlot).OtherHandSlot = _templateSlots[TemplateSlotType.AltMainHand] as WeaponSlot;

            List<GearSlot> armors = new();
            List<GearSlot> weapons = new();
            List<GearSlot> jewellery  = new();

            foreach (var slot in _templateSlots.Values)
            {
                if(slot.Slot.IsArmor())
                {
                    armors.Add(slot);
                }
                else if(slot.Slot.IsWeapon())
                {
                    weapons.Add(slot);
                }
                else if(slot.Slot.IsJewellery())
                {
                    jewellery .Add(slot);
                } 
            }

            foreach (GearSlot armor in armors)
            {
                armor.SlotGroup = armors;
            }

            foreach (GearSlot weapon in weapons)
            {
                weapon.SlotGroup = weapons;
            }

            foreach (GearSlot jewelrySlot in jewellery )
            {
                jewelrySlot.SlotGroup = jewellery ;
            }

            _professionRaceSelection = new()
            {
                Parent = this,
                Visible = false,
                ClipsBounds = false,
                ZIndex = 16,
            };

            ApplyTemplate();
        }

        private void TemplatePresenter_GearCodeChanged(object sender, EventArgs e)
        {
            _gearCodeBox.Text = TemplatePresenter?.Template?.GearCode;
        }

        private void TemplatePresenter_EliteSpecializationChanged(object sender, Core.Models.ValueChangedEventArgs<DataModels.Professions.Specialization> e)
        {

        }

        private void TemplatePresenter_ProfessionChanged(object sender, Core.Models.ValueChangedEventArgs<ProfessionType> e)
        {
            ApplyTemplate();
        }

        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<Template> e)
        {
            ApplyTemplate();
        }

        private void TemplatePresenter_LoadedGearFromCode(object sender, EventArgs e)
        {
            ApplyTemplate();
        }

        public TemplatePresenter TemplatePresenter { get => _templatePresenter; private set => Common.SetProperty(ref _templatePresenter ,value, OnTemplatePresenterChanged); }

        public SelectionPanel SelectionPanel
        {
            get => _selectionPanel; set
            {
                if (_selectionPanel == value) return;
                _selectionPanel = value;

                foreach (var slot in _templateSlots.Values)
                {
                    slot.SelectionPanel = value;
                }
            }
        }

        private void OnTemplatePresenterChanged(object sender, Core.Models.ValueChangedEventArgs<TemplatePresenter> e)
        {
            if(e.OldValue is not null)
            {
                e.OldValue.TemplateChanged -= TemplatePresenter_TemplateChanged;
                e.OldValue.LoadedGearFromCode -= TemplatePresenter_LoadedGearFromCode;
                e.OldValue.ProfessionChanged -= TemplatePresenter_ProfessionChanged;
                e.OldValue.EliteSpecializationChanged -= TemplatePresenter_EliteSpecializationChanged;
                e.OldValue.GearCodeChanged -= TemplatePresenter_GearCodeChanged;
            }

            if(e.NewValue is not null)
            {
                e.NewValue.TemplateChanged += TemplatePresenter_TemplateChanged;
                e.NewValue.LoadedGearFromCode += TemplatePresenter_LoadedGearFromCode;
                e.NewValue.ProfessionChanged += TemplatePresenter_ProfessionChanged;
                e.NewValue.EliteSpecializationChanged += TemplatePresenter_EliteSpecializationChanged;
                e.NewValue.GearCodeChanged += TemplatePresenter_GearCodeChanged;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_gearCodeBox is not null) _gearCodeBox.Width = Width - _gearCodeBox.Left;

            if (_templateSlots.Count > 0)
            {
                int secondColumn = _templateSlots[TemplateSlotType.AquaBreather].Width + 10;
                int gearSpacing = 8;

                _headerBounds = new(0, _copyButton.Top, 300, _copyButton.Height);

                _pve.Bounds = new(secondColumn, _headerBounds.Bottom + 5, 64, 64);
                _pvp.Bounds = new(secondColumn, _headerBounds.Bottom + 5, 64, 64);

                _framedSpecIcon.Location = new(secondColumn + 270 - 45, _headerBounds.Bottom + 5);
                _framedSpecIcon.Size = new(64, 64);

                _templateSlots[TemplateSlotType.Head].Location = new(0, _gearCodeBox.Bottom + 5);
                _templateSlots[TemplateSlotType.Shoulder].Location = new(_templateSlots[TemplateSlotType.Head].Left, _templateSlots[TemplateSlotType.Head].Bottom + gearSpacing);
                _templateSlots[TemplateSlotType.Chest].Location = new(_templateSlots[TemplateSlotType.Shoulder].Left, _templateSlots[TemplateSlotType.Shoulder].Bottom + gearSpacing);
                _templateSlots[TemplateSlotType.Hand].Location = new(_templateSlots[TemplateSlotType.Chest].Left, _templateSlots[TemplateSlotType.Chest].Bottom + gearSpacing);
                _templateSlots[TemplateSlotType.Leg].Location = new(_templateSlots[TemplateSlotType.Hand].Left, _templateSlots[TemplateSlotType.Hand].Bottom + gearSpacing);
                _templateSlots[TemplateSlotType.Foot].Location = new(_templateSlots[TemplateSlotType.Leg].Left, _templateSlots[TemplateSlotType.Leg].Bottom + gearSpacing);

                _templateSlots[TemplateSlotType.Nourishment].Location = new(secondColumn, _pve.Bounds.Bottom + 20);
                _templateSlots[TemplateSlotType.Enhancement].Location = new(secondColumn, _templateSlots[TemplateSlotType.Nourishment].Bottom + 5);
                _templateSlots[TemplateSlotType.JadeBotCore].Location = new(secondColumn, _templateSlots[TemplateSlotType.Enhancement].Bottom + 20);
                _templateSlots[TemplateSlotType.Relic].Location = new(secondColumn, _templateSlots[TemplateSlotType.JadeBotCore].Bottom + 5);

                _templateSlots[TemplateSlotType.PvpAmulet].Location = new(_templateSlots[TemplateSlotType.Leg].Left, _templateSlots[TemplateSlotType.Leg].Bottom + gearSpacing);

                _templateSlots[TemplateSlotType.MainHand].Location = new(_templateSlots[TemplateSlotType.Foot].Left, _templateSlots[TemplateSlotType.Foot].Bottom + 15);
                _templateSlots[TemplateSlotType.OffHand].Location = new(_templateSlots[TemplateSlotType.MainHand].Left + 4, _templateSlots[TemplateSlotType.MainHand].Bottom + 8);

                _templateSlots[TemplateSlotType.AltMainHand].Location = new(_templateSlots[TemplateSlotType.OffHand].Left, _templateSlots[TemplateSlotType.OffHand].Bottom + 35);
                _templateSlots[TemplateSlotType.AltOffHand].Location = new(_templateSlots[TemplateSlotType.AltMainHand].Left + 4, _templateSlots[TemplateSlotType.AltMainHand].Bottom + 8);

                _templateSlots[TemplateSlotType.Back].Location = new(secondColumn, _templateSlots[TemplateSlotType.Relic].Bottom + 20);
                _templateSlots[TemplateSlotType.Accessory_1].Location = new(_templateSlots[TemplateSlotType.Back].Right + 3, _templateSlots[TemplateSlotType.Back].Top);
                _templateSlots[TemplateSlotType.Accessory_2].Location = new(_templateSlots[TemplateSlotType.Accessory_1].Right + 3, _templateSlots[TemplateSlotType.Back].Top);

                _templateSlots[TemplateSlotType.Amulet].Location = new(_templateSlots[TemplateSlotType.Back].Left, _templateSlots[TemplateSlotType.Back].Bottom + 3);
                _templateSlots[TemplateSlotType.Ring_1].Location = new(_templateSlots[TemplateSlotType.Amulet].Right + 3, _templateSlots[TemplateSlotType.Amulet].Top);
                _templateSlots[TemplateSlotType.Ring_2].Location = new(_templateSlots[TemplateSlotType.Ring_1].Right + 3, _templateSlots[TemplateSlotType.Amulet].Top);

                _templateSlots[TemplateSlotType.AquaBreather].Location = new(_templateSlots[TemplateSlotType.Back].Left, _templateSlots[TemplateSlotType.Amulet].Bottom + 20);
                _templateSlots[TemplateSlotType.Aquatic].Location = new(_templateSlots[TemplateSlotType.AquaBreather].Left, _templateSlots[TemplateSlotType.AquaBreather].Bottom + 15);
                _templateSlots[TemplateSlotType.AltAquatic].Location = new(_templateSlots[TemplateSlotType.Aquatic].Left, _templateSlots[TemplateSlotType.Aquatic].Bottom + 8);
            }
        }

        public void ApplyTemplate()
        {
            _gearCodeBox.Text = TemplatePresenter?.Template?.ParseGearCode();

            if (TemplatePresenter?.Template is not null && TemplatePresenter?.Template?.Profession is not null && BuildsManager.Data?.Professions?.ContainsKey(TemplatePresenter?.Template?.Profession ?? ProfessionType.Guardian) == true)
            {
                _framedSpecIcon.Texture = TemplatePresenter.Template.EliteSpecialization?.ProfessionIconBig ??
                    BuildsManager.Data.Professions[TemplatePresenter.Template.Profession].IconBig;

                switch(TemplatePresenter.Template.Profession.GetArmorType())
                {
                    case ItemWeightType.Heavy:
                        _templateSlots[TemplateSlotType.AquaBreather].Item = BuildsManager.Data.Armors[79895];
                        _templateSlots[TemplateSlotType.Head].Item = BuildsManager.Data.Armors[85193];
                        _templateSlots[TemplateSlotType.Shoulder].Item = BuildsManager.Data.Armors[84875];
                        _templateSlots[TemplateSlotType.Chest].Item = BuildsManager.Data.Armors[85084];
                        _templateSlots[TemplateSlotType.Hand].Item = BuildsManager.Data.Armors[85140];
                        _templateSlots[TemplateSlotType.Leg].Item = BuildsManager.Data.Armors[84887];
                        _templateSlots[TemplateSlotType.Foot].Item = BuildsManager.Data.Armors[85055];
                        break;
                    case ItemWeightType.Medium:
                        _templateSlots[TemplateSlotType.AquaBreather].Item = BuildsManager.Data.Armors[79838];
                        _templateSlots[TemplateSlotType.Head].Item = BuildsManager.Data.Armors[80701];
                        _templateSlots[TemplateSlotType.Shoulder].Item = BuildsManager.Data.Armors[80825];
                        _templateSlots[TemplateSlotType.Chest].Item = BuildsManager.Data.Armors[84977];
                        _templateSlots[TemplateSlotType.Hand].Item = BuildsManager.Data.Armors[85169];
                        _templateSlots[TemplateSlotType.Leg].Item = BuildsManager.Data.Armors[85264];
                        _templateSlots[TemplateSlotType.Foot].Item = BuildsManager.Data.Armors[80836];
                        break;
                    case ItemWeightType.Light:
                        _templateSlots[TemplateSlotType.AquaBreather].Item = BuildsManager.Data.Armors[79873];
                        _templateSlots[TemplateSlotType.Head].Item = BuildsManager.Data.Armors[85128];
                        _templateSlots[TemplateSlotType.Shoulder].Item = BuildsManager.Data.Armors[84918];
                        _templateSlots[TemplateSlotType.Chest].Item = BuildsManager.Data.Armors[85333];
                        _templateSlots[TemplateSlotType.Hand].Item = BuildsManager.Data.Armors[85070];
                        _templateSlots[TemplateSlotType.Leg].Item = BuildsManager.Data.Armors[85362];
                        _templateSlots[TemplateSlotType.Foot].Item = BuildsManager.Data.Armors[80815];
                        break;
                }

                var t = TemplatePresenter.Template;
                _templateSlots[TemplateSlotType.MainHand].Item = t.MainHand.Weapon; 
                _templateSlots[TemplateSlotType.OffHand].Item = t.OffHand.Weapon; 
                _templateSlots[TemplateSlotType.Aquatic].Item = t.Aquatic.Weapon;

                _templateSlots[TemplateSlotType.AltMainHand].Item = t.AltMainHand.Weapon;
                _templateSlots[TemplateSlotType.AltOffHand].Item = t.AltOffHand.Weapon;
                _templateSlots[TemplateSlotType.AltAquatic].Item = t.AltAquatic.Weapon;

                _templateSlots[TemplateSlotType.PvpAmulet].Item = t.PvpAmulet.PvpAmulet;

                _templateSlots[TemplateSlotType.Nourishment].Item = t.Nourishment.Nourishment;
                _templateSlots[TemplateSlotType.Enhancement].Item = t.Enhancement.Enhancement;
                _templateSlots[TemplateSlotType.JadeBotCore].Item = t.JadeBotCore.JadeBotCore;
                _templateSlots[TemplateSlotType.Relic].Item = t.Relic.Relic;
            }

            foreach (var slot in _templateSlots.Values)
            {
                slot.Visible = 
                    (slot.Slot is not TemplateSlotType.AltAquatic || TemplatePresenter.Template.Profession is not ProfessionType.Engineer and not ProfessionType.Elementalist) &&
                    (TemplatePresenter.IsPve == false
                    ? slot.Slot is TemplateSlotType.MainHand or TemplateSlotType.AltMainHand or TemplateSlotType.OffHand or TemplateSlotType.AltOffHand or TemplateSlotType.PvpAmulet
                    : slot.Slot is not TemplateSlotType.PvpAmulet);
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            if (TemplatePresenter.Template is not null)
            {
                (TemplatePresenter.IsPve ? _pve : _pvp).Draw(this, spriteBatch, RelativeMousePosition);
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if ((TemplatePresenter.Template is not null && TemplatePresenter.IsPve ? _pve : _pvp).Hovered)
            {
                TemplatePresenter.GameMode = TemplatePresenter.IsPve ? GameModeType.PvP : GameModeType.PvE;

                foreach (var slot in _templateSlots.Values)
                {
                    slot.Visible =
                        (slot.Slot is not TemplateSlotType.AltAquatic || TemplatePresenter.Template.Profession is not ProfessionType.Engineer and not ProfessionType.Elementalist) &&
                        (TemplatePresenter.IsPve == false
                        ? slot.Slot is TemplateSlotType.MainHand or TemplateSlotType.AltMainHand or TemplateSlotType.OffHand or TemplateSlotType.AltOffHand or TemplateSlotType.PvpAmulet
                        : slot.Slot is not TemplateSlotType.PvpAmulet);
                }
            }

            if (_framedSpecIcon?.MouseOver == true)
            {
                _professionRaceSelection.Visible = true;
                _professionRaceSelection.Type = ProfessionRaceSelection.SelectionType.Profession;
                _professionRaceSelection.Location = RelativeMousePosition;
                _professionRaceSelection.OnClickAction = (value) => TemplatePresenter.Template.Profession = (ProfessionType)value;
            }
        }

        private void TemplateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _templateSlots?.Values?.DisposeAll();
            _templateSlots?.Clear();

            _pve?.Dispose();
            _pvp?.Dispose();

            TemplatePresenter = null;
        }
    }
}
