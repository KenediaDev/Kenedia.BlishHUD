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
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class InfusionControl : Blish_HUD.Controls.Container
    {
        private readonly bool _created;

        private readonly DetailedTexture _infusion = new() { TextureRegion = new(36, 36, 56, 56) };
        private readonly DetailedTexture _identifier = new(517199) { TextureRegion = new(8, 8, 48, 48) };

        private readonly (DetailedTexture, NumberBox) _power = new(new(66722) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _thoughness = new(new(156612) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _vitality = new(new(156613) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _precision = new(new(156609) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _ferocity = new(new(156602) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _condition = new(new(156600) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _expertise = new(new(156601) { TextureRegion = new(4, 4, 24, 24) }, null);
        private readonly (DetailedTexture, NumberBox) _concentration = new(new(156599) { TextureRegion = new(4, 4, 24, 24) }, null);

        private Rectangle _headerBounds;

        public InfusionControl()
        {
            _infusion.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");

            _power.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _thoughness.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _vitality.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _precision.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _ferocity.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _condition.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _expertise.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };
            _concentration.Item2 = new() { Parent = this, MinValue = 0, MaxValue = 16, Value = 0, };

            _created = true;
            Size = new(380, 300);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            _identifier.Draw(this, spriteBatch, RelativeMousePosition);
            //_infusion.Draw(this, spriteBatch, RelativeMousePosition);
            spriteBatch.DrawStringOnCtrl(this, "Infusions", Content.DefaultFont18, _headerBounds, Color.White);

            _power.Item1.Draw(this, spriteBatch);
            _precision.Item1.Draw(this, spriteBatch);
            _ferocity.Item1.Draw(this, spriteBatch);
            _thoughness.Item1.Draw(this, spriteBatch);

            _vitality.Item1.Draw(this, spriteBatch);
            _condition.Item1.Draw(this, spriteBatch);
            _expertise.Item1.Draw(this, spriteBatch);
            _concentration.Item1.Draw(this, spriteBatch);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_created)
            {
                int size = 64;
                int height = _power.Item2.Height;

                _identifier.Bounds = new(0, 0, size / 2, size / 2);
                //_infusion.Bounds = new(_identifier.Bounds.Right + 2, 0, size, size);
                _headerBounds = new(_identifier.Bounds.Right + 5, 0, Width - (_identifier.Bounds.Right + 5), size / 2);

                _power.Item1.Bounds = new(0, _headerBounds.Bottom + 5, height, height);
                _power.Item2.Location = new(_power.Item1.Bounds.Right + 4, _power.Item1.Bounds.Top);

                _precision.Item1.Bounds = new(0, _power.Item1.Bounds.Bottom + 5, height, height);
                _precision.Item2.Location = new(_precision.Item1.Bounds.Right + 4, _precision.Item1.Bounds.Top);

                _ferocity.Item1.Bounds = new(0, _precision.Item1.Bounds.Bottom + 5, height, height);
                _ferocity.Item2.Location = new(_ferocity.Item1.Bounds.Right + 4, _ferocity.Item1.Bounds.Top);

                _thoughness.Item1.Bounds = new(0, _ferocity.Item1.Bounds.Bottom + 5, height, height);
                _thoughness.Item2.Location = new(_thoughness.Item1.Bounds.Right + 4, _thoughness.Item1.Bounds.Top);

                _condition.Item1.Bounds = new(_power.Item2.Right + 25, _power.Item1.Bounds.Top, height, height);
                _condition.Item2.Location = new(_condition.Item1.Bounds.Right + 4, _condition.Item1.Bounds.Top);

                _expertise.Item1.Bounds = new(_condition.Item1.Bounds.Left, _condition.Item1.Bounds.Bottom + 5, height, height);
                _expertise.Item2.Location = new(_expertise.Item1.Bounds.Right + 4, _expertise.Item1.Bounds.Top);

                _concentration.Item1.Bounds = new(_expertise.Item1.Bounds.Left, _expertise.Item1.Bounds.Bottom + 5, height, height);
                _concentration.Item2.Location = new(_concentration.Item1.Bounds.Right + 4, _concentration.Item1.Bounds.Top);

                _vitality.Item1.Bounds = new(_concentration.Item1.Bounds.Left, _concentration.Item1.Bounds.Bottom + 5, height, height);
                _vitality.Item2.Location = new(_vitality.Item1.Bounds.Right + 4, _vitality.Item1.Bounds.Top);
            }
        }
    }

    public class UtilityControl : Blish_HUD.Controls.Control
    {
        private readonly DetailedTexture _utility = new() { TextureRegion = new(36, 36, 56, 56) };
        private readonly DetailedTexture _identifier = new(436368) { };

        public UtilityControl()
        {
            _utility.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\utilityslot.png");
            Size = new(96, 64);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _utility.Draw(this, spriteBatch, RelativeMousePosition);
            _identifier.Draw(this, spriteBatch, RelativeMousePosition);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);

            _identifier.Bounds = new(0, 0, size / 2, size / 2);
            _utility.Bounds = new(_identifier.Bounds.Right + 2, 0, size, size);
        }
    }

    public class NourishmentControl : Blish_HUD.Controls.Control
    {
        private readonly DetailedTexture _nourishment = new() { TextureRegion = new(36, 36, 56, 56) };
        private readonly DetailedTexture _identifier = new(436367) { };

        public NourishmentControl()
        {
            _nourishment.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\foodslot.png");
            Size = new(96, 64);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _identifier.Draw(this, spriteBatch, RelativeMousePosition);
            _nourishment.Draw(this, spriteBatch, RelativeMousePosition);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);

            _identifier.Bounds = new(0, 0, size / 2, size / 2);
            _nourishment.Bounds = new(_identifier.Bounds.Right + 2, 0, size, size);
        }
    }

    public class GearSlotControl : Blish_HUD.Controls.Control
    {
        private GearSlot _gearSlot = GearSlot.None;
        private readonly DetailedTexture _slot = new(156189) { TextureRegion = new(40, 40, 64, 64) };
        private readonly DetailedTexture _upgrade = new(784323) { TextureRegion = new(36, 36, 56, 56) };
        private readonly DetailedTexture _upgrade2 = new(784323) { TextureRegion = new(36, 36, 56, 56) };
        private DetailedTexture[] _infusions = new DetailedTexture[1]
        {
            new(517199) { TextureRegion = new(8, 8, 48, 48) },
        };
        private Rectangle _textBounds;
        private Rectangle _secondUpgradeTextBounds;
        private Rectangle _statBounds;
        private readonly DetailedTexture _icon = new() { TextureRegion = new(36, 36, 56, 56) };

        public GearSlot GearSlot { get => _gearSlot; set => Common.SetProperty(ref _gearSlot, value, ApplySlot); }

        public GearSlotControl()
        {
            Size = new(380, 64);
        }

        public GearSlotControl(GearSlot gearSlot, Blish_HUD.Controls.Container parent) : this()
        {
            GearSlot = gearSlot;
            Parent = parent;
        }

        private void ApplySlot()
        {
            // 156189 Slot
            // 157256 Slot
            // 157205 Slot

            // 2229699 PvE
            // 2229701 PvP

            // 784322 PvP Amulet
            // 784307 Outfit
            // 358410 Dye
            // 517201 Enrichment
            // 517200 Infusion (Offensive?)
            // 517199 Infusion (Defensive)
            // 784323 Rune
            // 784324 Sigil
            // 156303 Sickle
            // 156304 Axe
            // 156305 Pickaxe

            var assetIds = new Dictionary<GearSlot, int>()
            {
                {GearSlot.AquaBreather, 156308},
                {GearSlot.Head, 156307},
                {GearSlot.Shoulder, 156311},
                {GearSlot.Chest, 156297},
                {GearSlot.Hand, 156306},
                {GearSlot.Leg, 156309},
                {GearSlot.Foot, 156300},
                {GearSlot.MainHand, 156316},
                {GearSlot.OffHand, 156320},
                {GearSlot.Aquatic, 156313},
                {GearSlot.AltMainHand, 156316},
                {GearSlot.AltOffHand, 156320},
                {GearSlot.AltAquatic, 156313},
                {GearSlot.Back, 156293},
                {GearSlot.Amulet, 156310},
                {GearSlot.Accessory_1, 156298},
                {GearSlot.Accessory_2, 156299},
                {GearSlot.Ring_1, 156301},
                {GearSlot.Ring_2, 156302},
                {GearSlot.PvpAmulet, 784322},
            };

            if (assetIds.TryGetValue(GearSlot, out int assetId))
            {
                _icon.Texture = AsyncTexture2D.FromAssetId(assetId);
            }

            if (GearSlot is GearSlot.Aquatic or GearSlot.AltAquatic or GearSlot.MainHand or GearSlot.OffHand or GearSlot.AltMainHand or GearSlot.AltOffHand)
            {
                _upgrade.Texture = AsyncTexture2D.FromAssetId(784324);
                _upgrade2.Texture = AsyncTexture2D.FromAssetId(784324);
            }

            _infusions = GearSlot switch
            {
                GearSlot.Back => new DetailedTexture[2]
                    {
                        new(517199) { TextureRegion = new(8, 8, 48, 48) },
                        new(517199) { TextureRegion = new(8, 8, 48, 48) },
                    },
                GearSlot.Ring_1 or GearSlot.Ring_2 => new DetailedTexture[3]
                    {
                        new(517199) { TextureRegion = new(8, 8, 48, 48) },
                        new(517199) { TextureRegion = new(8, 8, 48, 48) },
                        new(517199) { TextureRegion = new(8, 8, 48, 48) },
                    },
                GearSlot.Amulet => new DetailedTexture[1]
                    {
                        new(517201) { TextureRegion = new(8, 8, 48, 48) },
                    },
                _ => new DetailedTexture[1]
                    {
                        new(517199) { TextureRegion = new(8, 8, 48, 48) },
                    },
            };

            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);
            _icon.Bounds = new(0, 0, size, size);

            bool isJuwellery = GearSlot is GearSlot.Accessory_1 or GearSlot.Accessory_2 or GearSlot.Amulet or GearSlot.Back or GearSlot.Ring_1 or GearSlot.Ring_2;
            _upgrade.Bounds = new(_icon.Bounds.Right + 2, 0, isJuwellery ? 0 : size, isJuwellery ? 0 : size);

            int infusionSize = (size - 4) / 3;
            for (int i = 0; i < _infusions.Length; i++)
            {
                _infusions[i].Bounds = new(_upgrade.Bounds.Right, i * (infusionSize + 2), infusionSize, infusionSize);
            }

            if (GearSlot is GearSlot.Aquatic or GearSlot.AltAquatic)
            {
                int upgradeSize = (size - 4) / 2;
                _upgrade.Bounds = new(_icon.Bounds.Right + 2, 2, upgradeSize, upgradeSize);
                _upgrade2.Bounds = new(_icon.Bounds.Right + 2, upgradeSize + 4, upgradeSize, upgradeSize);
            }
            _statBounds = new(_upgrade.Bounds.Right + 5, _upgrade.Bounds.Top + 2, Width - (_upgrade.Bounds.Right + 5), Content.DefaultFont16.LineHeight + 2);
            _textBounds = new(_upgrade.Bounds.Right + 5, _statBounds.Bottom, _statBounds.Width, Content.DefaultFont14.LineHeight + 2);
            _secondUpgradeTextBounds = new(_upgrade.Bounds.Right + 5, _textBounds.Bottom, _statBounds.Width, Content.DefaultFont14.LineHeight + 2);

        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _icon.Draw(this, spriteBatch, RelativeMousePosition);

            bool hasUpgrades = GearSlot is not (GearSlot.Accessory_1 or GearSlot.Accessory_2 or GearSlot.Amulet or GearSlot.Back or GearSlot.Ring_1 or GearSlot.Ring_2);

            if (GearSlot is not (GearSlot.Accessory_1 or GearSlot.Accessory_2 or GearSlot.Amulet or GearSlot.Back or GearSlot.Ring_1 or GearSlot.Ring_2))
            {
                _upgrade.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, "Berserker", Content.DefaultFont16, _statBounds, Color.White, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }

            if (GearSlot is GearSlot.Aquatic or GearSlot.AltAquatic)
            {
                _upgrade2.Draw(this, spriteBatch, RelativeMousePosition);
            }

            if (GearSlot is GearSlot.Aquatic or GearSlot.AltAquatic)
            {
                spriteBatch.DrawStringOnCtrl(this, "Force", Content.DefaultFont14, _textBounds, Color.Orange, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
                spriteBatch.DrawStringOnCtrl(this, "Impact", Content.DefaultFont14, _secondUpgradeTextBounds, Color.Orange, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }
            else if (GearSlot is GearSlot.Aquatic or GearSlot.AltAquatic or GearSlot.MainHand or GearSlot.AltMainHand)
            {
                spriteBatch.DrawStringOnCtrl(this, "Force", Content.DefaultFont14, _textBounds, Color.Orange, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }
            else if (GearSlot is GearSlot.Aquatic or GearSlot.AltAquatic or GearSlot.MainHand or GearSlot.OffHand or GearSlot.AltMainHand or GearSlot.AltOffHand)
            {
                spriteBatch.DrawStringOnCtrl(this, "Impact", Content.DefaultFont14, _textBounds, Color.Orange, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }
            else if (hasUpgrades)
            {
                spriteBatch.DrawStringOnCtrl(this, "Chronomancer", Content.DefaultFont14, _textBounds, Color.Orange, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }
        }
    }

    public class GearPage : Blish_HUD.Controls.Container
    {
        private readonly TexturesService _texturesService;
        private readonly TextBox _gearCodeBox;
        private readonly ImageButton _copyButton;

        private Template _template;
        private Rectangle _headerBounds;

        private Dictionary<GearSlot, GearSlotControl> _slots = new();

        private InfusionControl _infusions;
        private NourishmentControl _nourishment;
        private UtilityControl _utility;

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
                    ApplyTemplate();
                }
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
                {GearSlot.AquaBreather, new GearSlotControl(GearSlot.AquaBreather, this)},
                {GearSlot.Head, new GearSlotControl(GearSlot.Head, this)},
                {GearSlot.Shoulder, new GearSlotControl(GearSlot.Shoulder, this)},
                {GearSlot.Chest, new GearSlotControl(GearSlot.Chest, this)},
                {GearSlot.Hand, new GearSlotControl(GearSlot.Hand, this)},
                {GearSlot.Leg, new GearSlotControl(GearSlot.Leg, this)},
                {GearSlot.Foot, new GearSlotControl(GearSlot.Foot, this)},
                {GearSlot.MainHand, new GearSlotControl(GearSlot.MainHand, this)},
                {GearSlot.OffHand, new GearSlotControl(GearSlot.OffHand, this){}},
                {GearSlot.Aquatic, new GearSlotControl(GearSlot.Aquatic, this)},
                {GearSlot.AltMainHand, new GearSlotControl(GearSlot.AltMainHand, this)},
                {GearSlot.AltOffHand, new GearSlotControl(GearSlot.AltOffHand, this){ }},
                {GearSlot.AltAquatic, new GearSlotControl(GearSlot.AltAquatic, this)},
                {GearSlot.Back, new GearSlotControl(GearSlot.Back, this) { Width = 64 } },
                {GearSlot.Amulet, new GearSlotControl(GearSlot.Amulet, this){ Width = 64 }},
                {GearSlot.Accessory_1, new GearSlotControl(GearSlot.Accessory_1, this){ Width = 64 }},
                {GearSlot.Accessory_2, new GearSlotControl(GearSlot.Accessory_2, this){ Width = 64 }},
                {GearSlot.Ring_1, new GearSlotControl(GearSlot.Ring_1, this){ Width = 64 }},
                {GearSlot.Ring_2, new GearSlotControl(GearSlot.Ring_2, this){ Width = 64 }},
                {GearSlot.PvpAmulet, new GearSlotControl(GearSlot.PvpAmulet, this)},
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

                    if (_template != null) _template.Changed += TemplateChanged;
                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_gearCodeBox != null) _gearCodeBox.Width = Width - _gearCodeBox.Left;

            if (_slots.Count > 0)
            {

                _pve.Bounds = new(0, _gearCodeBox.Bottom + 5, 42, 42);
                _pvp.Bounds = new(0, _gearCodeBox.Bottom + 5, 42, 42);
                _headerBounds = new(_pve.Bounds.Right + 15, _pve.Bounds.Top, 300, _pve.Bounds.Height);

                _slots[GearSlot.AquaBreather].Location = new(_pve.Bounds.Left, _headerBounds.Bottom + 10);
                _slots[GearSlot.PvpAmulet].Location = new(_pve.Bounds.Left, _headerBounds.Bottom + 10);

                _slots[GearSlot.MainHand].Location = new(_slots[GearSlot.AquaBreather].Left, _slots[GearSlot.AquaBreather].Bottom + 15);
                _slots[GearSlot.OffHand].Location = new(_slots[GearSlot.MainHand].Left, _slots[GearSlot.MainHand].Bottom + 5);

                _slots[GearSlot.AltMainHand].Location = new(_slots[GearSlot.OffHand].Left, _slots[GearSlot.OffHand].Bottom + 15);
                _slots[GearSlot.AltOffHand].Location = new(_slots[GearSlot.AltMainHand].Left, _slots[GearSlot.AltMainHand].Bottom + 5);

                _slots[GearSlot.Aquatic].Location = new(_slots[GearSlot.AltOffHand].Left, _slots[GearSlot.AltOffHand].Bottom + 15);
                _slots[GearSlot.AltAquatic].Location = new(_slots[GearSlot.AquaBreather].Left, _slots[GearSlot.Aquatic].Bottom + 5);

                _nourishment.Location = new(_slots[GearSlot.AltAquatic].Left, _slots[GearSlot.AltAquatic].Bottom + 15);
                _utility.Location = new(_nourishment.Left, _nourishment.Bottom + 5);

                int secondColumn = _slots[GearSlot.AquaBreather].Right + 10;

                _slots[GearSlot.Head].Location = new(secondColumn, _gearCodeBox.Bottom + 5);
                _slots[GearSlot.Shoulder].Location = new(_slots[GearSlot.Head].Left, _slots[GearSlot.Head].Bottom + 5);
                _slots[GearSlot.Chest].Location = new(_slots[GearSlot.Shoulder].Left, _slots[GearSlot.Shoulder].Bottom + 5);
                _slots[GearSlot.Hand].Location = new(_slots[GearSlot.Chest].Left, _slots[GearSlot.Chest].Bottom + 5);
                _slots[GearSlot.Leg].Location = new(_slots[GearSlot.Hand].Left, _slots[GearSlot.Hand].Bottom + 5);
                _slots[GearSlot.Foot].Location = new(_slots[GearSlot.Leg].Left, _slots[GearSlot.Leg].Bottom + 5);

                _slots[GearSlot.Back].Location = new(_slots[GearSlot.Foot].Left, _slots[GearSlot.Foot].Bottom + 15);
                _slots[GearSlot.Accessory_1].Location = new(_slots[GearSlot.Back].Right + 5, _slots[GearSlot.Back].Top);
                _slots[GearSlot.Accessory_2].Location = new(_slots[GearSlot.Accessory_1].Right + 5, _slots[GearSlot.Back].Top);

                _slots[GearSlot.Amulet].Location = new(_slots[GearSlot.Back].Left, _slots[GearSlot.Back].Bottom + 5);
                _slots[GearSlot.Ring_1].Location = new(_slots[GearSlot.Amulet].Right + 5, _slots[GearSlot.Amulet].Top);
                _slots[GearSlot.Ring_2].Location = new(_slots[GearSlot.Ring_1].Right + 5, _slots[GearSlot.Amulet].Top);

                _infusions.Location = new(_slots[GearSlot.Amulet].Left, _slots[GearSlot.Ring_2].Bottom + 15);
            }
        }

        public void ApplyTemplate()
        {
            _gearCodeBox.Text = Template?.GearTemplate?.ParseGearCode();

            _infusions.Visible = Template.PvE;
            _nourishment.Visible = Template.PvE;
            _utility.Visible = Template.PvE;

            foreach (var slot in _slots.Values)
            {
                slot.Visible = !Template.PvE
                    ? slot.GearSlot is GearSlot.MainHand or GearSlot.AltMainHand or GearSlot.OffHand or GearSlot.AltOffHand or GearSlot.PvpAmulet
                    : slot.GearSlot is not GearSlot.PvpAmulet;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (Template != null)
            {
                (Template.PvE ? _pve : _pvp).Draw(this, spriteBatch, RelativeMousePosition);

                spriteBatch.DrawStringOnCtrl(this, Template.PvE ? "PvE Build" : "PvP Build", Content.DefaultFont18, _headerBounds, Color.White);
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
