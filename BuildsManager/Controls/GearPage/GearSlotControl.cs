using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static Blish_HUD.ContentService;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.Controls.Selection;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class GearSlotControl : Blish_HUD.Controls.Control
    {
        private readonly Texture2D _infusionTexture;

        private GearTemplateSlot _gearSlot = GearTemplateSlot.None;
        private readonly DetailedTexture _slot = new(156189) { TextureRegion = new(40, 40, 64, 64) };
        private readonly DetailedTexture _upgrade = new(784323) { TextureRegion = new(36, 36, 56, 56) };
        private readonly DetailedTexture _upgrade2 = new(784323) { TextureRegion = new(36, 36, 56, 56) };
        private DetailedTexture[] _infusions = new DetailedTexture[1]
        {
            new(517199) { TextureRegion = new(8, 8, 48, 48) },
        };
        private Rectangle _textBounds;
        private Rectangle _secondUpgradeTextBounds;
        private Rectangle _infusionBounds;
        private Rectangle _infusion2Bounds;
        private Rectangle _statBounds;
        private Template _template;

        private readonly DetailedTexture _icon = new() { TextureRegion = new(36, 36, 56, 56) };

        Color _statColor = Colors.ColonialWhite;
        Color _upgradeColor = Color.Orange;
        Color _infusionColor = new(153, 238, 221);

        public GearTemplateSlot GearSlot { get => _gearSlot; set => Common.SetProperty(ref _gearSlot, value, ApplySlot); }

        public GearSlotControl()
        {
            _infusionTexture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            Size = new(380, 64);
        }

        public GearSlotControl(GearTemplateSlot gearSlot, Blish_HUD.Controls.Container parent) : this()
        {
            GearSlot = gearSlot;
            Parent = parent;
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

        public SelectionPanel SelectionPanel { get; set; }

        public void ApplyTemplate()
        {
            RecalculateLayout();
        }

        private void TemplateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyTemplate();
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

            var assetIds = new Dictionary<GearTemplateSlot, int>()
            {
                {GearTemplateSlot.AquaBreather, 156308},
                {GearTemplateSlot.Head, 156307},
                {GearTemplateSlot.Shoulder, 156311},
                {GearTemplateSlot.Chest, 156297},
                {GearTemplateSlot.Hand, 156306},
                {GearTemplateSlot.Leg, 156309},
                {GearTemplateSlot.Foot, 156300},
                {GearTemplateSlot.MainHand, 156316},
                {GearTemplateSlot.OffHand, 156320},
                {GearTemplateSlot.Aquatic, 156313},
                {GearTemplateSlot.AltMainHand, 156316},
                {GearTemplateSlot.AltOffHand, 156320},
                {GearTemplateSlot.AltAquatic, 156313},
                {GearTemplateSlot.Back, 156293},
                {GearTemplateSlot.Amulet, 156310},
                {GearTemplateSlot.Accessory_1, 156298},
                {GearTemplateSlot.Accessory_2, 156299},
                {GearTemplateSlot.Ring_1, 156301},
                {GearTemplateSlot.Ring_2, 156302},
                {GearTemplateSlot.PvpAmulet, 784322},
            };

            if (assetIds.TryGetValue(GearSlot, out int assetId))
            {
                _icon.Texture = AsyncTexture2D.FromAssetId(assetId);
            }

            if (GearSlot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic or GearTemplateSlot.MainHand or GearTemplateSlot.OffHand or GearTemplateSlot.AltMainHand or GearTemplateSlot.AltOffHand)
            {
                _upgrade.Texture = AsyncTexture2D.FromAssetId(784324);
                _upgrade2.Texture = AsyncTexture2D.FromAssetId(784324);
            }

            _infusions = GearSlot switch
            {
                GearTemplateSlot.Back or GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic => new DetailedTexture[2]
                    {
                        new() { Texture = _infusionTexture, TextureRegion = new(36, 36, 56, 56) },
                        new() { Texture = _infusionTexture, TextureRegion = new(36, 36, 56, 56) },
                    },
                GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2 => new DetailedTexture[3]
                    {
                        new() { Texture = _infusionTexture, TextureRegion = new(36, 36, 56, 56) },
                        new() { Texture = _infusionTexture, TextureRegion = new(36, 36, 56, 56) },
                        new() { Texture = _infusionTexture, TextureRegion = new(36, 36, 56, 56) },
                    },
                GearTemplateSlot.Amulet => new DetailedTexture[1]
                    {
                        new() { Texture = _infusionTexture, TextureRegion = new(36, 36, 56, 56) },
                    },
                _ => new DetailedTexture[1]
                    {
                        new() { Texture = _infusionTexture, TextureRegion = new(36, 36, 56, 56) },
                    },
            };

            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);
            _icon.Bounds = new(0, 0, size, size);

            bool isJuwellery = GearSlot is GearTemplateSlot.Accessory_1 or GearTemplateSlot.Accessory_2 or GearTemplateSlot.Amulet or GearTemplateSlot.Back or GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2;
            _upgrade.Bounds = new(_icon.Bounds.Right + 2, 0, isJuwellery ? 0 : size, isJuwellery ? 0 : size);

            int infusionSize = (size - 4) / 3;
            int upgradeSize = (size - 4) / 2;

            if (GearSlot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic)
            {
                _upgrade.Bounds = new(_icon.Bounds.Right + 2, 0, upgradeSize, upgradeSize);
                _upgrade2.Bounds = new(_icon.Bounds.Right + 2 + upgradeSize + 4, 0, upgradeSize, upgradeSize);

                for (int i = 0; i < _infusions.Length; i++)
                {
                    _infusions[i].Bounds = new(_icon.Bounds.Right + 2 + (i * (upgradeSize + 4)), (upgradeSize + 4), upgradeSize, upgradeSize);
                }

                _statBounds = new(_upgrade2.Bounds.Right + 5, _upgrade2.Bounds.Top, Width - (_upgrade2.Bounds.Right + 5), Content.DefaultFont16.LineHeight + 2);
                _textBounds = new(_upgrade2.Bounds.Right + 5, _statBounds.Bottom, _statBounds.Width / 2, Content.DefaultFont12.LineHeight + 2);
                _secondUpgradeTextBounds = new(_textBounds.Right + 5, _textBounds.Top, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);

                _infusionBounds = new(_upgrade2.Bounds.Right + 5, _textBounds.Bottom, _statBounds.Width / 2, Content.DefaultFont12.LineHeight + 2);
                _infusion2Bounds = new(_secondUpgradeTextBounds.Left, _secondUpgradeTextBounds.Bottom, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);
            }
            else if (GearSlot is GearTemplateSlot.AquaBreather)
            {
                _upgrade.Bounds = new(_icon.Bounds.Right + 2, 0, upgradeSize, upgradeSize);
                for (int i = 0; i < _infusions.Length; i++)
                {
                    _infusions[i].Bounds = new(_icon.Bounds.Right + 2, upgradeSize + 4, upgradeSize, upgradeSize);
                }

                _statBounds = new(_upgrade.Bounds.Right + 5 + upgradeSize + 4, _upgrade.Bounds.Top, Width - (_upgrade.Bounds.Right + 5), Content.DefaultFont16.LineHeight + 2);
                _textBounds = new(_upgrade.Bounds.Right + 5 + upgradeSize + 4, _statBounds.Bottom, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);
                _infusionBounds = new(_upgrade.Bounds.Right + 5 + upgradeSize + 4, _textBounds.Bottom, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);
            }
            else if (!isJuwellery)
            {
                int padding = GearSlot is GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand ? 9 : 0;

                if (Template != null && !Template.PvE)
                {
                    padding = GearSlot is GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand ? 5 : 0;
                    upgradeSize = 48;
                    _upgrade.Bounds = new(_icon.Bounds.Right + 2 + padding, (_icon.Bounds.Height - upgradeSize) / 2, upgradeSize, upgradeSize);

                    _statBounds = new(_upgrade.Bounds.Right + 5, _upgrade.Bounds.Top, Width - (_upgrade.Bounds.Right + 5), Content.DefaultFont16.LineHeight + 2);
                    _textBounds = new(_upgrade.Bounds.Right + 5, _statBounds.Bottom, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);
                    _infusionBounds = new(_upgrade.Bounds.Right + 5, _textBounds.Bottom, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);
                }
                else
                {
                    _upgrade.Bounds = new(_icon.Bounds.Right + 2 + padding, 0, upgradeSize, upgradeSize);
                    for (int i = 0; i < _infusions.Length; i++)
                    {
                        _infusions[i].Bounds = new(_icon.Bounds.Right + 2 + padding, upgradeSize + 4, upgradeSize, upgradeSize);
                    }

                    _statBounds = new(_upgrade.Bounds.Right + 5, _upgrade.Bounds.Top, Width - (_upgrade.Bounds.Right + 5), Content.DefaultFont16.LineHeight + 2);
                    _textBounds = new(_upgrade.Bounds.Right + 5, _statBounds.Bottom, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);
                    _infusionBounds = new(_upgrade.Bounds.Right + 5, _textBounds.Bottom, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);
                }
            }
            else
            {
                for (int i = 0; i < _infusions.Length; i++)
                {
                    _infusions[i].Bounds = new(_icon.Bounds.Right + 2, i * (infusionSize + 2), infusionSize, infusionSize);
                }
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            //_statColor = Colors.DullColor;
            //_statColor = Colors.Chardonnay;
            _statColor = Colors.ColonialWhite;
            //_statColor = new(255, 255, 255);
            //_statColor = new(0, 255, 0);
            //_statColor = new(85, 153, 255);
            //_statColor = new(153, 51, 255);

            _icon.Draw(this, spriteBatch, RelativeMousePosition);

            bool hasUpgrades = GearSlot is not (GearTemplateSlot.Accessory_1 or GearTemplateSlot.Accessory_2 or GearTemplateSlot.Amulet or GearTemplateSlot.Back or GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2);

            if (GearSlot is not (GearTemplateSlot.Accessory_1 or GearTemplateSlot.Accessory_2 or GearTemplateSlot.Amulet or GearTemplateSlot.Back or GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2))
            {
                _upgrade.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, "Berserker", Content.DefaultFont16, _statBounds, _statColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }

            if (GearSlot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic)
            {
                _upgrade2.Draw(this, spriteBatch, RelativeMousePosition);
            }

            if (GearSlot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic)
            {
                spriteBatch.DrawStringOnCtrl(this, "Force", Content.DefaultFont12, _textBounds, _upgradeColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
                spriteBatch.DrawStringOnCtrl(this, "Impact", Content.DefaultFont12, _secondUpgradeTextBounds, _upgradeColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }
            else if (GearSlot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic or GearTemplateSlot.MainHand or GearTemplateSlot.AltMainHand)
            {
                spriteBatch.DrawStringOnCtrl(this, "Force", Content.DefaultFont12, _textBounds, _upgradeColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }
            else if (GearSlot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic or GearTemplateSlot.MainHand or GearTemplateSlot.OffHand or GearTemplateSlot.AltMainHand or GearTemplateSlot.AltOffHand)
            {
                spriteBatch.DrawStringOnCtrl(this, "Impact", Content.DefaultFont12, _textBounds, _upgradeColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }
            else if (hasUpgrades)
            {
                spriteBatch.DrawStringOnCtrl(this, "Chronomancer", Content.DefaultFont12, _textBounds, _upgradeColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }

            if (Template != null && Template.PvE)
            {
                for (int i = 0; i < _infusions.Length; i++)
                {
                    _infusions[i].Draw(this, spriteBatch, RelativeMousePosition);
                }

                if (hasUpgrades)
                {
                    if (_infusions.Length > 0) spriteBatch.DrawStringOnCtrl(this, "Mighty", Content.DefaultFont12, _infusionBounds, _infusionColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
                    if (_infusions.Length > 1) spriteBatch.DrawStringOnCtrl(this, "Precise", Content.DefaultFont12, _infusion2Bounds, _infusionColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            var a = AbsoluteBounds;

            if (_icon.Hovered)
                SelectionPanel?.SetGearAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_icon.Bounds), GearSlot, GearSlot.ToString().ToLowercaseNamingConvention());

            if (_upgrade.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_upgrade.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());

            if (_upgrade2.Hovered)
                SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(_upgrade2.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());

            foreach (var infusion in _infusions)
            {
                if (infusion.Hovered)
                    SelectionPanel?.SetItemIdAnchor(this, new Rectangle(a.Location, Point.Zero).Add(infusion.Bounds), GearSlot.ToString().ToLowercaseNamingConvention());
            }
        }
    }
}
