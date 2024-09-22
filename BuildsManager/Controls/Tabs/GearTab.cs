using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Kenedia.Modules.Core.Extensions;
using ItemWeightType = Gw2Sharp.WebApi.V2.Models.ItemWeightType;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Controls_Old.GearPage.GearSlots;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.BuildsManager.Controls.Selection;

namespace Kenedia.Modules.BuildsManager.Controls.Tabs
{
    public class GearTab : Blish_HUD.Controls.Container
    {
        private readonly TextBox _gearCodeBox;
        private readonly ImageButton _copyButton;
        private readonly ButtonImage _framedSpecIcon;

        private Rectangle _headerBounds;

        private Dictionary<TemplateSlotType, GearSlot> _templateSlots = new();

        private TemplatePresenter _templatePresenter;
        private readonly DetailedTexture _terrestrialSet = new(156323);
        private readonly DetailedTexture _alternateTerrestrialSet = new(156324);
        private readonly DetailedTexture _aquaticSet = new(156325);
        private readonly DetailedTexture _alternateAquaticSet = new(156326);

        private readonly DetailedTexture _pve = new(2229699, 2229700);
        private readonly DetailedTexture _pvp = new(2229701, 2229702);
        private readonly ProfessionRaceSelection _professionRaceSelection;

        public GearTab(TemplatePresenter templatePresenter, SelectionPanel selectionPanel)
        {
            TemplatePresenter = templatePresenter;
            SelectionPanel = selectionPanel;

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
                ClickAction = async (m) =>
                {
                    try
                    {
                        if (_gearCodeBox.Text is string s && !string.IsNullOrEmpty(s))
                        {
                            _ = await ClipboardUtil.WindowsClipboardService.SetTextAsync(s);
                        }
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
                {TemplateSlotType.Head, new ArmorSlot(TemplateSlotType.Head, this, TemplatePresenter, SelectionPanel)},
                {TemplateSlotType.Shoulder, new ArmorSlot(TemplateSlotType.Shoulder, this, TemplatePresenter, SelectionPanel)},
                {TemplateSlotType.Chest, new ArmorSlot(TemplateSlotType.Chest, this, TemplatePresenter, SelectionPanel)},
                {TemplateSlotType.Hand, new ArmorSlot(TemplateSlotType.Hand, this, TemplatePresenter, SelectionPanel)},
                {TemplateSlotType.Leg, new ArmorSlot(TemplateSlotType.Leg, this, TemplatePresenter, SelectionPanel)},
                {TemplateSlotType.Foot, new ArmorSlot(TemplateSlotType.Foot, this, TemplatePresenter, SelectionPanel)},

                {TemplateSlotType.MainHand, new WeaponSlot(TemplateSlotType.MainHand, this, TemplatePresenter, SelectionPanel)},
                {TemplateSlotType.OffHand, new WeaponSlot(TemplateSlotType.OffHand, this, TemplatePresenter, SelectionPanel){ Height = 55 }},
                {TemplateSlotType.AltMainHand, new WeaponSlot(TemplateSlotType.AltMainHand, this, TemplatePresenter, SelectionPanel)},
                {TemplateSlotType.AltOffHand, new WeaponSlot(TemplateSlotType.AltOffHand, this, TemplatePresenter, SelectionPanel){ Height = 55 }},

                {TemplateSlotType.AquaBreather, new ArmorSlot(TemplateSlotType.AquaBreather, this, TemplatePresenter, SelectionPanel){ Height = 55 }},
                {TemplateSlotType.Aquatic, new AquaticWeaponSlot(TemplateSlotType.Aquatic, this, TemplatePresenter, SelectionPanel){ Height = 55 }},
                {TemplateSlotType.AltAquatic, new AquaticWeaponSlot(TemplateSlotType.AltAquatic, this, TemplatePresenter, SelectionPanel){ Height = 55 }},

                {TemplateSlotType.Back, new BackSlot(TemplateSlotType.Back, this, TemplatePresenter, SelectionPanel){ Width = 85}},
                {TemplateSlotType.Amulet, new AmuletSlot(TemplateSlotType.Amulet, this, TemplatePresenter, SelectionPanel){ Width = 85}},
                {TemplateSlotType.Ring_1, new RingSlot(TemplateSlotType.Ring_1, this, TemplatePresenter, SelectionPanel){ Width = 85}},
                {TemplateSlotType.Ring_2, new RingSlot(TemplateSlotType.Ring_2, this, TemplatePresenter, SelectionPanel){ Width = 85}},
                {TemplateSlotType.Accessory_1, new AccessoireSlot(TemplateSlotType.Accessory_1, this, TemplatePresenter, SelectionPanel){ Width = 85}},
                {TemplateSlotType.Accessory_2, new AccessoireSlot(TemplateSlotType.Accessory_2, this, TemplatePresenter, SelectionPanel){ Width = 85}},

                {TemplateSlotType.PvpAmulet, new PvpAmuletSlot(TemplateSlotType.PvpAmulet, this, TemplatePresenter, SelectionPanel) {Visible = false } },
                {TemplateSlotType.Nourishment, new NourishmentSlot(TemplateSlotType.Nourishment, this, TemplatePresenter, SelectionPanel)},
                {TemplateSlotType.Enhancement, new EnhancementSlot(TemplateSlotType.Enhancement, this, TemplatePresenter, SelectionPanel)},
                {TemplateSlotType.PowerCore, new PowerCoreSlot(TemplateSlotType.PowerCore, this, TemplatePresenter, SelectionPanel)},
                {TemplateSlotType.PveRelic, new RelicSlot(TemplateSlotType.PveRelic, this, TemplatePresenter, SelectionPanel)},
                {TemplateSlotType.PvpRelic, new RelicSlot(TemplateSlotType.PvpRelic, this, TemplatePresenter, SelectionPanel)},
            };

            (_templateSlots[TemplateSlotType.MainHand] as WeaponSlot).PairedSlot = _templateSlots[TemplateSlotType.OffHand] as WeaponSlot;
            (_templateSlots[TemplateSlotType.AltMainHand] as WeaponSlot).PairedSlot = _templateSlots[TemplateSlotType.AltOffHand] as WeaponSlot;

            (_templateSlots[TemplateSlotType.OffHand] as WeaponSlot).PairedSlot = _templateSlots[TemplateSlotType.MainHand] as WeaponSlot;
            (_templateSlots[TemplateSlotType.AltOffHand] as WeaponSlot).PairedSlot = _templateSlots[TemplateSlotType.AltMainHand] as WeaponSlot;

            (_templateSlots[TemplateSlotType.PveRelic] as RelicSlot).PairedSlot = _templateSlots[TemplateSlotType.PvpRelic] as RelicSlot;
            (_templateSlots[TemplateSlotType.PvpRelic] as RelicSlot).PairedSlot = _templateSlots[TemplateSlotType.PveRelic] as RelicSlot;

            List<GearSlot> armors = new();
            List<GearSlot> weapons = new();
            List<GearSlot> jewellery = new();

            foreach (var slot in _templateSlots.Values)
            {
                if (slot.Slot.IsArmor())
                {
                    armors.Add(slot);
                }
                else if (slot.Slot.IsWeapon())
                {
                    weapons.Add(slot);
                }
                else if (slot.Slot.IsJewellery())
                {
                    jewellery.Add(slot);
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

            foreach (GearSlot jewelrySlot in jewellery)
            {
                jewelrySlot.SlotGroup = jewellery;
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

        public TemplatePresenter TemplatePresenter { get => _templatePresenter; private set => Common.SetProperty(ref _templatePresenter, value, OnTemplatePresenterChanged); }
        public SelectionPanel SelectionPanel { get; }

        private void OnTemplatePresenterChanged(object sender, Core.Models.ValueChangedEventArgs<TemplatePresenter> e)
        {
            if (e.OldValue is not null)
            {
                e.OldValue.TemplateChanged -= TemplatePresenter_TemplateChanged;
                e.OldValue.LoadedGearFromCode -= TemplatePresenter_LoadedGearFromCode;
                e.OldValue.LoadedBuildFromCode -= TemplatePresenter_LoadedBuildFromCode;
                e.OldValue.ProfessionChanged -= TemplatePresenter_ProfessionChanged;
                e.OldValue.EliteSpecializationChanged_OLD -= TemplatePresenter_EliteSpecializationChanged;
                e.OldValue.GearCodeChanged -= TemplatePresenter_GearCodeChanged;
            }

            if (e.NewValue is not null)
            {
                e.NewValue.TemplateChanged += TemplatePresenter_TemplateChanged;
                e.NewValue.LoadedGearFromCode += TemplatePresenter_LoadedGearFromCode;
                e.NewValue.LoadedBuildFromCode += TemplatePresenter_LoadedBuildFromCode;
                e.NewValue.ProfessionChanged += TemplatePresenter_ProfessionChanged;
                e.NewValue.EliteSpecializationChanged_OLD += TemplatePresenter_EliteSpecializationChanged;
                e.NewValue.GearCodeChanged += TemplatePresenter_GearCodeChanged;
            }
        }

        private void TemplatePresenter_LoadedBuildFromCode(object sender, EventArgs e)
        {
            ApplyTemplate();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_gearCodeBox is not null) _gearCodeBox.Width = Width - _gearCodeBox.Left;

            if (_templateSlots.Count > 0)
            {
                int secondColumn = _templateSlots[TemplateSlotType.AquaBreather].Width + 10;
                int gearSpacing = 8;

                int setSize = 36;
                Point p = Point.Zero;
                Point s = Point.Zero;

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
                _templateSlots[TemplateSlotType.PowerCore].Location = new(secondColumn, _templateSlots[TemplateSlotType.Enhancement].Bottom + 20);
                _templateSlots[TemplateSlotType.PveRelic].Location = new(secondColumn, _templateSlots[TemplateSlotType.PowerCore].Bottom + 5);

                _templateSlots[TemplateSlotType.PvpAmulet].Location = new(_templateSlots[TemplateSlotType.Leg].Left, _templateSlots[TemplateSlotType.Leg].Bottom + gearSpacing);
                _templateSlots[TemplateSlotType.PvpRelic].Location = new(_templateSlots[TemplateSlotType.Hand].Left, _templateSlots[TemplateSlotType.Hand].Bottom + gearSpacing);

                p = _templateSlots[TemplateSlotType.MainHand].Location = new(_templateSlots[TemplateSlotType.Foot].Left, _templateSlots[TemplateSlotType.Foot].Bottom + 15);
                _templateSlots[TemplateSlotType.OffHand].Location = new(_templateSlots[TemplateSlotType.MainHand].Left + 4, _templateSlots[TemplateSlotType.MainHand].Bottom + 4);
                s = _templateSlots[TemplateSlotType.MainHand].Size;
                _terrestrialSet.Bounds = new(p.X + (s.Y / 2) - (setSize / 2), p.Y + s.Y - (setSize / 2) + 4, setSize, setSize);

                p = _templateSlots[TemplateSlotType.AltMainHand].Location = new(_templateSlots[TemplateSlotType.OffHand].Left, _templateSlots[TemplateSlotType.OffHand].Bottom + 35);
                _templateSlots[TemplateSlotType.AltOffHand].Location = new(_templateSlots[TemplateSlotType.AltMainHand].Left + 4, _templateSlots[TemplateSlotType.AltMainHand].Bottom + 4);
                s = _templateSlots[TemplateSlotType.AltMainHand].Size;
                _alternateTerrestrialSet.Bounds = new(p.X + (s.Y / 2) - (setSize / 2), p.Y + s.Y - (setSize / 2) + 4, setSize, setSize);

                _templateSlots[TemplateSlotType.Back].Location = new(secondColumn, _templateSlots[TemplateSlotType.PveRelic].Bottom + 20);
                _templateSlots[TemplateSlotType.Accessory_1].Location = new(_templateSlots[TemplateSlotType.Back].Right + 3, _templateSlots[TemplateSlotType.Back].Top);
                _templateSlots[TemplateSlotType.Accessory_2].Location = new(_templateSlots[TemplateSlotType.Accessory_1].Right + 3, _templateSlots[TemplateSlotType.Back].Top);

                _templateSlots[TemplateSlotType.Amulet].Location = new(_templateSlots[TemplateSlotType.Back].Left, _templateSlots[TemplateSlotType.Back].Bottom + 3);
                _templateSlots[TemplateSlotType.Ring_1].Location = new(_templateSlots[TemplateSlotType.Amulet].Right + 3, _templateSlots[TemplateSlotType.Amulet].Top);
                _templateSlots[TemplateSlotType.Ring_2].Location = new(_templateSlots[TemplateSlotType.Ring_1].Right + 3, _templateSlots[TemplateSlotType.Amulet].Top);

                _templateSlots[TemplateSlotType.AquaBreather].Location = new(_templateSlots[TemplateSlotType.Back].Left, _templateSlots[TemplateSlotType.Amulet].Bottom + 20);
                _templateSlots[TemplateSlotType.Aquatic].Location = new(_templateSlots[TemplateSlotType.AquaBreather].Left, _templateSlots[TemplateSlotType.AquaBreather].Bottom + 15);
                var b = _templateSlots[TemplateSlotType.Aquatic].LocalBounds;
                _aquaticSet.Bounds = new(b.Left + (b.Height / 2) - (setSize / 2), b.Bottom - (setSize / 2), setSize, setSize);

                _templateSlots[TemplateSlotType.AltAquatic].Location = new(_templateSlots[TemplateSlotType.Aquatic].Left, _templateSlots[TemplateSlotType.Aquatic].Bottom + 12);
                b = _templateSlots[TemplateSlotType.AltAquatic].LocalBounds;
                _alternateAquaticSet.Bounds = new(b.Left + (b.Height / 2) - (setSize / 2), b.Bottom - (setSize / 2), setSize, setSize);
            }
        }

        public void ApplyTemplate()
        {
            _gearCodeBox.Text = TemplatePresenter?.Template?.ParseGearCode();

            if (TemplatePresenter?.Template is not null && TemplatePresenter?.Template?.Profession is not null && BuildsManager.Data?.Professions?.ContainsKey(TemplatePresenter?.Template?.Profession ?? ProfessionType.Guardian) == true)
            {
                _framedSpecIcon.Texture = TemplatePresenter.Template.EliteSpecialization?.ProfessionIconBig ??
                    BuildsManager.Data.Professions[TemplatePresenter.Template.Profession].IconBig;

                switch (TemplatePresenter.Template.Profession.GetArmorType())
                {
                    case ItemWeightType.Heavy:
                        _templateSlots[TemplateSlotType.AquaBreather].Item = BuildsManager.Data.Armors[79895];
                        _templateSlots[TemplateSlotType.Head].Item = BuildsManager.Data.Armors[80384];
                        _templateSlots[TemplateSlotType.Shoulder].Item = BuildsManager.Data.Armors[80435];
                        _templateSlots[TemplateSlotType.Chest].Item = BuildsManager.Data.Armors[80254];
                        _templateSlots[TemplateSlotType.Hand].Item = BuildsManager.Data.Armors[80205];
                        _templateSlots[TemplateSlotType.Leg].Item = BuildsManager.Data.Armors[80277];
                        _templateSlots[TemplateSlotType.Foot].Item = BuildsManager.Data.Armors[80557];
                        break;
                    case ItemWeightType.Medium:
                        _templateSlots[TemplateSlotType.AquaBreather].Item = BuildsManager.Data.Armors[79838];
                        _templateSlots[TemplateSlotType.Head].Item = BuildsManager.Data.Armors[80296];
                        _templateSlots[TemplateSlotType.Shoulder].Item = BuildsManager.Data.Armors[80145];
                        _templateSlots[TemplateSlotType.Chest].Item = BuildsManager.Data.Armors[80578];
                        _templateSlots[TemplateSlotType.Hand].Item = BuildsManager.Data.Armors[80161];
                        _templateSlots[TemplateSlotType.Leg].Item = BuildsManager.Data.Armors[80252];
                        _templateSlots[TemplateSlotType.Foot].Item = BuildsManager.Data.Armors[80281];
                        break;
                    case ItemWeightType.Light:
                        _templateSlots[TemplateSlotType.AquaBreather].Item = BuildsManager.Data.Armors[79873];
                        _templateSlots[TemplateSlotType.Head].Item = BuildsManager.Data.Armors[80248];
                        _templateSlots[TemplateSlotType.Shoulder].Item = BuildsManager.Data.Armors[80131];
                        _templateSlots[TemplateSlotType.Chest].Item = BuildsManager.Data.Armors[80190];
                        _templateSlots[TemplateSlotType.Hand].Item = BuildsManager.Data.Armors[80111];
                        _templateSlots[TemplateSlotType.Leg].Item = BuildsManager.Data.Armors[80356];
                        _templateSlots[TemplateSlotType.Foot].Item = BuildsManager.Data.Armors[80399];
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
                _templateSlots[TemplateSlotType.PowerCore].Item = t.PowerCore.PowerCore;
                _templateSlots[TemplateSlotType.PveRelic].Item = t.PveRelic.Relic;
                _templateSlots[TemplateSlotType.PvpRelic].Item = t.PvpRelic.Relic;
            }

            SetVisibility();
        }

        private void SetVisibility()
        {
            foreach (var slot in _templateSlots.Values)
            {
                slot.Visible =
                    (slot.Slot is not TemplateSlotType.AltAquatic || TemplatePresenter.Template.Profession is not ProfessionType.Engineer and not ProfessionType.Elementalist) &&
                    TemplatePresenter.IsPvp ?
                    (slot.Slot is TemplateSlotType.PvpRelic or TemplateSlotType.MainHand or TemplateSlotType.AltMainHand or TemplateSlotType.OffHand or TemplateSlotType.AltOffHand or TemplateSlotType.PvpAmulet) :
                    slot.Slot is not TemplateSlotType.PvpAmulet and not TemplateSlotType.PvpRelic;
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

                _terrestrialSet.Draw(this, spriteBatch);
                _alternateTerrestrialSet.Draw(this, spriteBatch);

                if (TemplatePresenter.IsPve == true)
                {
                    _aquaticSet.Draw(this, spriteBatch);

                    if(_templateSlots[TemplateSlotType.AltAquatic].Visible)
                        _alternateAquaticSet.Draw(this, spriteBatch);
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if ((TemplatePresenter.Template is not null && TemplatePresenter.IsPve ? _pve : _pvp).Hovered)
            {
                TemplatePresenter.GameMode = TemplatePresenter.IsPve ? GameModeType.PvP : GameModeType.PvE;

                SetVisibility();
            }

            if (_framedSpecIcon?.MouseOver == true)
            {
                _professionRaceSelection.Visible = true;
                _professionRaceSelection.Type = ProfessionRaceSelection.SelectionType.Profession;
                _professionRaceSelection.Location = RelativeMousePosition;
                _professionRaceSelection.OnClickAction = (value) => TemplatePresenter?.Template?.SetProfession((ProfessionType)value);
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
