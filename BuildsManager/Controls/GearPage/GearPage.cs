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

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class GearPage : Blish_HUD.Controls.Container
    {
        private readonly TexturesService _texturesService;
        private readonly TextBox _gearCodeBox;
        private readonly ImageButton _copyButton;

        private Rectangle _headerBounds;

        private Dictionary<TemplateSlot, GearSlot> _templateSlots = new();

        private FramedImage _framedSpecIcon;
        private SelectionPanel _selectionPanel;
        private readonly DetailedTexture _pve = new(2229699, 2229700);
        private readonly DetailedTexture _pvp = new(2229701, 2229702);
        private readonly ProfessionRaceSelection _professionRaceSelection;

        public GearPage(TexturesService _texturesService, TemplatePresenter templatePresenter)
        {
            this._texturesService = _texturesService;
            TemplatePresenter = templatePresenter;

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
                EnterPressedAction = (txt) => TemplatePresenter.Template?.LoadGearFromCode(txt, true),
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size11, ContentService.FontStyle.Regular),
            };

            _framedSpecIcon = new()
            {
                Parent = this,
            };

            _templateSlots = new()
            {
                {TemplateSlot.Head, new ArmorSlot(TemplateSlot.Head, this, TemplatePresenter)},
                {TemplateSlot.Shoulder, new ArmorSlot(TemplateSlot.Shoulder, this, TemplatePresenter)},
                {TemplateSlot.Chest, new ArmorSlot(TemplateSlot.Chest, this, TemplatePresenter)},
                {TemplateSlot.Hand, new ArmorSlot(TemplateSlot.Hand, this, TemplatePresenter)},
                {TemplateSlot.Leg, new ArmorSlot(TemplateSlot.Leg, this, TemplatePresenter)},
                {TemplateSlot.Foot, new ArmorSlot(TemplateSlot.Foot, this, TemplatePresenter)},

                {TemplateSlot.MainHand, new WeaponSlot(TemplateSlot.MainHand, this, TemplatePresenter)},
                {TemplateSlot.OffHand, new WeaponSlot(TemplateSlot.OffHand, this, TemplatePresenter){ Height = 55 }},
                {TemplateSlot.AltMainHand, new WeaponSlot(TemplateSlot.AltMainHand, this, TemplatePresenter)},
                {TemplateSlot.AltOffHand, new WeaponSlot(TemplateSlot.AltOffHand, this, TemplatePresenter){ Height = 55 }},

                {TemplateSlot.AquaBreather, new ArmorSlot(TemplateSlot.AquaBreather, this, TemplatePresenter){ Height = 55 }},
                {TemplateSlot.Aquatic, new AquaticWeaponSlot(TemplateSlot.Aquatic, this, TemplatePresenter){ Height = 55 }},
                {TemplateSlot.AltAquatic, new AquaticWeaponSlot(TemplateSlot.AltAquatic, this, TemplatePresenter){ Height = 55 }},

                {TemplateSlot.Back, new BackSlot(TemplateSlot.Back, this, TemplatePresenter){ Width = 85}},
                {TemplateSlot.Amulet, new AmuletSlot(TemplateSlot.Amulet, this, TemplatePresenter){ Width = 85}},
                {TemplateSlot.Ring_1, new RingSlot(TemplateSlot.Ring_1, this, TemplatePresenter){ Width = 85}},
                {TemplateSlot.Ring_2, new RingSlot(TemplateSlot.Ring_2, this, TemplatePresenter){ Width = 85}},
                {TemplateSlot.Accessory_1, new AccessoireSlot(TemplateSlot.Accessory_1, this, TemplatePresenter){ Width = 85}},
                {TemplateSlot.Accessory_2, new AccessoireSlot(TemplateSlot.Accessory_2, this, TemplatePresenter){ Width = 85}},

                {TemplateSlot.PvpAmulet, new PvpAmuletSlot(TemplateSlot.PvpAmulet, this, TemplatePresenter) {Visible = false } },
                {TemplateSlot.Nourishment, new NourishmentSlot(TemplateSlot.Nourishment, this, TemplatePresenter)},
                {TemplateSlot.Utility, new UtilitySlot(TemplateSlot.Utility, this, TemplatePresenter)},
                {TemplateSlot.JadeBotCore, new JadeBotCoreSlot(TemplateSlot.JadeBotCore, this, TemplatePresenter)},
                {TemplateSlot.Relic, new RelicSlot(TemplateSlot.Relic, this, TemplatePresenter)},
            };

            _professionRaceSelection = new()
            {
                Parent = this,
                Visible = false,
                ClipsBounds = false,
                ZIndex = 16,
            };

            TemplatePresenter.TemplateChanged += TemplatePresenter_TemplateChanged;
            TemplatePresenter.LoadedGearFromCode += TemplatePresenter_LoadedGearFromCode;
            TemplatePresenter.ProfessionChanged += TemplatePresenter_ProfessionChanged;
            TemplatePresenter.EliteSpecializationChanged += TemplatePresenter_EliteSpecializationChanged;
            TemplatePresenter.GearCodeChanged += TemplatePresenter_GearCodeChanged;

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

        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<VTemplate> e)
        {
            ApplyTemplate();
        }

        private void TemplatePresenter_LoadedGearFromCode(object sender, EventArgs e)
        {
            ApplyTemplate();
        }

        public TemplatePresenter TemplatePresenter { get; private set; }

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

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_gearCodeBox != null) _gearCodeBox.Width = Width - _gearCodeBox.Left;

            if (_templateSlots.Count > 0)
            {
                int secondColumn = _templateSlots[TemplateSlot.AquaBreather].Width + 10;
                int gearSpacing = 8;

                _headerBounds = new(0, _copyButton.Top, 300, _copyButton.Height);

                _pve.Bounds = new(secondColumn, _headerBounds.Bottom + 5, 64, 64);
                _pvp.Bounds = new(secondColumn, _headerBounds.Bottom + 5, 64, 64);

                _framedSpecIcon.Location = new(secondColumn + 270 - 45, _headerBounds.Bottom + 5);
                _framedSpecIcon.Size = new(64, 64);

                _templateSlots[TemplateSlot.Head].Location = new(0, _gearCodeBox.Bottom + 5);
                _templateSlots[TemplateSlot.Shoulder].Location = new(_templateSlots[TemplateSlot.Head].Left, _templateSlots[TemplateSlot.Head].Bottom + gearSpacing);
                _templateSlots[TemplateSlot.Chest].Location = new(_templateSlots[TemplateSlot.Shoulder].Left, _templateSlots[TemplateSlot.Shoulder].Bottom + gearSpacing);
                _templateSlots[TemplateSlot.Hand].Location = new(_templateSlots[TemplateSlot.Chest].Left, _templateSlots[TemplateSlot.Chest].Bottom + gearSpacing);
                _templateSlots[TemplateSlot.Leg].Location = new(_templateSlots[TemplateSlot.Hand].Left, _templateSlots[TemplateSlot.Hand].Bottom + gearSpacing);
                _templateSlots[TemplateSlot.Foot].Location = new(_templateSlots[TemplateSlot.Leg].Left, _templateSlots[TemplateSlot.Leg].Bottom + gearSpacing);

                _templateSlots[TemplateSlot.Nourishment].Location = new(secondColumn, _pve.Bounds.Bottom + 20);
                _templateSlots[TemplateSlot.Utility].Location = new(secondColumn, _templateSlots[TemplateSlot.Nourishment].Bottom + 5);
                _templateSlots[TemplateSlot.JadeBotCore].Location = new(secondColumn, _templateSlots[TemplateSlot.Utility].Bottom + 20);
                _templateSlots[TemplateSlot.Relic].Location = new(secondColumn, _templateSlots[TemplateSlot.JadeBotCore].Bottom + 5);

                _templateSlots[TemplateSlot.PvpAmulet].Location = new(_templateSlots[TemplateSlot.Leg].Left, _templateSlots[TemplateSlot.Leg].Bottom + gearSpacing);

                _templateSlots[TemplateSlot.MainHand].Location = new(_templateSlots[TemplateSlot.Foot].Left, _templateSlots[TemplateSlot.Foot].Bottom + 15);
                _templateSlots[TemplateSlot.OffHand].Location = new(_templateSlots[TemplateSlot.MainHand].Left + 4, _templateSlots[TemplateSlot.MainHand].Bottom + 8);

                _templateSlots[TemplateSlot.AltMainHand].Location = new(_templateSlots[TemplateSlot.OffHand].Left, _templateSlots[TemplateSlot.OffHand].Bottom + 35);
                _templateSlots[TemplateSlot.AltOffHand].Location = new(_templateSlots[TemplateSlot.AltMainHand].Left + 4, _templateSlots[TemplateSlot.AltMainHand].Bottom + 8);

                _templateSlots[TemplateSlot.Back].Location = new(secondColumn, _templateSlots[TemplateSlot.Relic].Bottom + 20);
                _templateSlots[TemplateSlot.Accessory_1].Location = new(_templateSlots[TemplateSlot.Back].Right + 3, _templateSlots[TemplateSlot.Back].Top);
                _templateSlots[TemplateSlot.Accessory_2].Location = new(_templateSlots[TemplateSlot.Accessory_1].Right + 3, _templateSlots[TemplateSlot.Back].Top);

                _templateSlots[TemplateSlot.Amulet].Location = new(_templateSlots[TemplateSlot.Back].Left, _templateSlots[TemplateSlot.Back].Bottom + 3);
                _templateSlots[TemplateSlot.Ring_1].Location = new(_templateSlots[TemplateSlot.Amulet].Right + 3, _templateSlots[TemplateSlot.Amulet].Top);
                _templateSlots[TemplateSlot.Ring_2].Location = new(_templateSlots[TemplateSlot.Ring_1].Right + 3, _templateSlots[TemplateSlot.Amulet].Top);

                _templateSlots[TemplateSlot.AquaBreather].Location = new(_templateSlots[TemplateSlot.Back].Left, _templateSlots[TemplateSlot.Amulet].Bottom + 20);
                _templateSlots[TemplateSlot.Aquatic].Location = new(_templateSlots[TemplateSlot.AquaBreather].Left, _templateSlots[TemplateSlot.AquaBreather].Bottom + 15);
                _templateSlots[TemplateSlot.AltAquatic].Location = new(_templateSlots[TemplateSlot.Aquatic].Left, _templateSlots[TemplateSlot.Aquatic].Bottom + 8);
            }
        }

        public void ApplyTemplate()
        {
            _gearCodeBox.Text = TemplatePresenter.Template?.ParseGearCode();

            if (TemplatePresenter.Template != null && BuildsManager.Data.Professions.ContainsKey(TemplatePresenter.Template.Profession))
            {
                _framedSpecIcon.Texture = TemplatePresenter.Template.EliteSpecialization?.ProfessionIconBig ??
                    BuildsManager.Data.Professions[TemplatePresenter.Template.Profession].IconBig;

                switch(TemplatePresenter.Template.Profession.GetArmorType())
                {
                    case ItemWeightType.Heavy:
                        _templateSlots[TemplateSlot.AquaBreather].Item = BuildsManager.Data.Armors[79895];
                        _templateSlots[TemplateSlot.Head].Item = BuildsManager.Data.Armors[85193];
                        _templateSlots[TemplateSlot.Shoulder].Item = BuildsManager.Data.Armors[84875];
                        _templateSlots[TemplateSlot.Chest].Item = BuildsManager.Data.Armors[85084];
                        _templateSlots[TemplateSlot.Hand].Item = BuildsManager.Data.Armors[85140];
                        _templateSlots[TemplateSlot.Leg].Item = BuildsManager.Data.Armors[84887];
                        _templateSlots[TemplateSlot.Foot].Item = BuildsManager.Data.Armors[85055];
                        break;
                    case ItemWeightType.Medium:
                        _templateSlots[TemplateSlot.AquaBreather].Item = BuildsManager.Data.Armors[79838];
                        _templateSlots[TemplateSlot.Head].Item = BuildsManager.Data.Armors[80701];
                        _templateSlots[TemplateSlot.Shoulder].Item = BuildsManager.Data.Armors[80825];
                        _templateSlots[TemplateSlot.Chest].Item = BuildsManager.Data.Armors[84977];
                        _templateSlots[TemplateSlot.Hand].Item = BuildsManager.Data.Armors[85169];
                        _templateSlots[TemplateSlot.Leg].Item = BuildsManager.Data.Armors[85264];
                        _templateSlots[TemplateSlot.Foot].Item = BuildsManager.Data.Armors[80836];
                        break;
                    case ItemWeightType.Light:
                        _templateSlots[TemplateSlot.AquaBreather].Item = BuildsManager.Data.Armors[79873];
                        _templateSlots[TemplateSlot.Head].Item = BuildsManager.Data.Armors[85128];
                        _templateSlots[TemplateSlot.Shoulder].Item = BuildsManager.Data.Armors[84918];
                        _templateSlots[TemplateSlot.Chest].Item = BuildsManager.Data.Armors[85333];
                        _templateSlots[TemplateSlot.Hand].Item = BuildsManager.Data.Armors[85070];
                        _templateSlots[TemplateSlot.Leg].Item = BuildsManager.Data.Armors[85362];
                        _templateSlots[TemplateSlot.Foot].Item = BuildsManager.Data.Armors[80815];
                        break;
                }

                var t = TemplatePresenter.Template;
                _templateSlots[TemplateSlot.MainHand].Item = t.MainHand.Weapon; 
                _templateSlots[TemplateSlot.OffHand].Item = t.OffHand.Weapon; 
                _templateSlots[TemplateSlot.Aquatic].Item = t.Aquatic.Weapon;

                _templateSlots[TemplateSlot.AltMainHand].Item = t.AltMainHand.Weapon;
                _templateSlots[TemplateSlot.AltOffHand].Item = t.AltOffHand.Weapon;
                _templateSlots[TemplateSlot.AltAquatic].Item = t.AltAquatic.Weapon;

                _templateSlots[TemplateSlot.PvpAmulet].Item = t.PvpAmulet.PvpAmulet;

                _templateSlots[TemplateSlot.Nourishment].Item = t.Nourishment.Nourishment;
                _templateSlots[TemplateSlot.Utility].Item = t.Utility.Utility;
                _templateSlots[TemplateSlot.JadeBotCore].Item = t.JadeBotCore.JadeBotCore;
                _templateSlots[TemplateSlot.Relic].Item = t.Relic.Relic;
            }

            foreach (var slot in _templateSlots.Values)
            {
                slot.Visible = 
                    (slot.Slot is not TemplateSlot.AltAquatic || TemplatePresenter.Template.Profession is not ProfessionType.Engineer and not ProfessionType.Elementalist) &&
                    (TemplatePresenter.IsPve == false
                    ? slot.Slot is TemplateSlot.MainHand or TemplateSlot.AltMainHand or TemplateSlot.OffHand or TemplateSlot.AltOffHand or TemplateSlot.PvpAmulet
                    : slot.Slot is not TemplateSlot.PvpAmulet);
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            if (TemplatePresenter.Template != null)
            {
                (TemplatePresenter.IsPve ? _pve : _pvp).Draw(this, spriteBatch, RelativeMousePosition);
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if ((TemplatePresenter.Template != null && TemplatePresenter.IsPve ? _pve : _pvp).Hovered)
            {
                TemplatePresenter.GameMode = TemplatePresenter.IsPve ? GameModeType.PvP : GameModeType.PvE;

                foreach (var slot in _templateSlots.Values)
                {
                    slot.Visible =
                        (slot.Slot is not TemplateSlot.AltAquatic || TemplatePresenter.Template.Profession is not ProfessionType.Engineer and not ProfessionType.Elementalist) &&
                        (TemplatePresenter.IsPve == false
                        ? slot.Slot is TemplateSlot.MainHand or TemplateSlot.AltMainHand or TemplateSlot.OffHand or TemplateSlot.AltOffHand or TemplateSlot.PvpAmulet
                        : slot.Slot is not TemplateSlot.PvpAmulet);
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
    }
}
