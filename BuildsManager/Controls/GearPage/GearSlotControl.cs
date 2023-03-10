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
using Kenedia.Modules.BuildsManager.Extensions;
using System.Diagnostics;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class ItemTexture : DetailedTexture
    {
        private BaseItem _item;

        private Rectangle _itemBoudns;
        private Color _frameColor;

        public BaseItem Item { get => _item; set => Common.SetProperty(ref _item, value, ApplyItem); }

        private void ApplyItem()
        {
            _frameColor = Item != null ? (Color)Item?.Rarity.GetColor() : Color.White * 0.5F;
            Texture = Item?.Icon;
        }

        public void Draw(Blish_HUD.Controls.Control ctrl, SpriteBatch spriteBatch, Point? mousePos = null, Color? color = null)
        {
            if (FallBackTexture != null || Texture != null)
            {
                Hovered = mousePos != null && Bounds.Contains((Point)mousePos);
                color ??= (Hovered && HoverDrawColor != null ? HoverDrawColor : DrawColor) ?? Color.White;

                if (Texture != null)
                {
                    spriteBatch.DrawOnCtrl(
                        ctrl,
                        Texture,
                        Bounds.Add(2, 2, -4, -4),
                        TextureRegion,
                        (Color)color,
                        0F,
                        Vector2.Zero);
                }

                spriteBatch.DrawFrame(ctrl, Bounds, _frameColor, 2);
            }
        }
    }

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
        private Rectangle _statIconBounds;
        private Template _template;

        private readonly DetailedTexture _icon = new() { TextureRegion = new(36, 36, 56, 56) };
        private readonly ItemTexture _item = new() { };
        private readonly ItemTexture Upgrade1 = new() { };
        private readonly ItemTexture Upgrade2 = new() { };
        private readonly ItemTexture Infusion1 = new() { };
        private readonly ItemTexture Infusion2 = new() { };
        private readonly ItemTexture Infusion3 = new() { };

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

        public Stat Stat { get; set; }

        public void ApplyTemplate()
        {
            RecalculateLayout();

            if (GearSlot.IsArmor())
            {
                _item.Item = Template?.GearTemplate.Armors[GearSlot].Item;
                Stat = Template?.GearTemplate.Armors[GearSlot].Stat != null && BuildsManager.Data.Stats.TryGetValue((int)Template?.GearTemplate.Armors[GearSlot].Stat, out Stat stat) ? stat : null;
                if (Template != null) Infusion1.Item = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == Template.GearTemplate.Armors[GearSlot].InfusionIds[0]).FirstOrDefault().Value;
                if (Template != null) Upgrade1.Item = BuildsManager.Data.PveRunes.Where(e => e.Value.MappedId == Template.GearTemplate.Armors[GearSlot].RuneIds[0]).FirstOrDefault().Value;
            }
            else if (GearSlot.IsWeapon())
            {
                _item.Item = Template?.GearTemplate.Weapons[GearSlot].Item;
                Stat = Template?.GearTemplate.Weapons[GearSlot].Stat != null && BuildsManager.Data.Stats.TryGetValue((int)Template?.GearTemplate.Weapons[GearSlot].Stat, out Stat stat) ? stat : null;

                if (GearSlot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic)
                {
                    if (Template != null) Infusion1.Item = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == Template.GearTemplate.Weapons[GearSlot].InfusionIds[0]).FirstOrDefault().Value;
                    if (Template != null) Infusion2.Item = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == Template.GearTemplate.Weapons[GearSlot].InfusionIds[1]).FirstOrDefault().Value;

                    if (Template != null) Upgrade1.Item = BuildsManager.Data.PveSigils.Where(e => e.Value.MappedId == Template.GearTemplate.Weapons[GearSlot].SigilIds[0]).FirstOrDefault().Value;
                    if (Template != null) Upgrade2.Item = BuildsManager.Data.PveSigils.Where(e => e.Value.MappedId == Template.GearTemplate.Weapons[GearSlot].SigilIds[0]).FirstOrDefault().Value;
                }
                else
                {
                    if (Template != null) Infusion1.Item = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == Template.GearTemplate.Weapons[GearSlot].InfusionIds[0]).FirstOrDefault().Value;
                    if (Template != null) Upgrade1.Item = BuildsManager.Data.PveSigils.Where(e => e.Value.MappedId == Template.GearTemplate.Weapons[GearSlot].SigilIds[0]).FirstOrDefault().Value;
                }
            }
            else if (GearSlot.IsJuwellery())
            {
                _item.Item = Template?.GearTemplate.Juwellery[GearSlot].Item;
                Stat = Template?.GearTemplate.Juwellery[GearSlot].Stat != null && BuildsManager.Data.Stats.TryGetValue((int)Template?.GearTemplate.Juwellery[GearSlot].Stat, out Stat stat) ? stat : null;

                if (GearSlot is GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2)
                {
                    if (Template != null) Infusion1.Item = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == Template.GearTemplate.Juwellery[GearSlot].InfusionIds[0]).FirstOrDefault().Value;
                    if (Template != null) Infusion2.Item = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == Template.GearTemplate.Juwellery[GearSlot].InfusionIds[1]).FirstOrDefault().Value;
                    if (Template != null) Infusion3.Item = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == Template.GearTemplate.Juwellery[GearSlot].InfusionIds[2]).FirstOrDefault().Value;
                }
                else if (GearSlot is GearTemplateSlot.Back)
                {
                    if (Template != null) Infusion1.Item = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == Template.GearTemplate.Juwellery[GearSlot].InfusionIds[0]).FirstOrDefault().Value;
                    if (Template != null) Infusion2.Item = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == Template.GearTemplate.Juwellery[GearSlot].InfusionIds[1]).FirstOrDefault().Value;
                }
                else
                {
                    if (Template != null) Infusion1.Item = BuildsManager.Data.Infusions.Where(e => e.Value.MappedId == Template.GearTemplate.Juwellery[GearSlot].InfusionIds[0]).FirstOrDefault().Value;
                }
            }
            else if (GearSlot is GearTemplateSlot.PvpAmulet)
            {
                _item.Item = Template?.GearTemplate.PvpAmulet[GearSlot].Item;
                //if (Template != null) Upgrade1 = BuildsManager.Data.PvpRunes.Where(e => e.Value.MappedId == Template.GearTemplate.PvpAmulet[GearSlot].RuneIds[0]).FirstOrDefault().Value;
                //Stat = Template?.GearTemplate.PvpAmulet[GearSlot].Stat != null && BuildsManager.Data.Stats.TryGetValue((int)Template?.GearTemplate.PvpAmulet[GearSlot].Stat, out Stat stat) ? stat : null;
                Stat = null;
            }
            else
            {
                _item.Item = Template?.GearTemplate.Common[GearSlot].Item;
                Stat = null;
            }
        }

        private void TemplateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        private void ApplySlot()
        {
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
            _item.Bounds = new(0, 0, size, size);

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
                _statIconBounds = new(_upgrade2.Bounds.Right + 2, _upgrade2.Bounds.Top, Content.DefaultFont16.LineHeight, Content.DefaultFont16.LineHeight);
                _statBounds = new(_statIconBounds.Right + 2, _upgrade2.Bounds.Top, Width - _statIconBounds.Right, Content.DefaultFont16.LineHeight + 2);
                _textBounds = new(_upgrade2.Bounds.Right + 5, _statBounds.Bottom, _statBounds.Width / 2, Content.DefaultFont12.LineHeight + 2);
                _secondUpgradeTextBounds = new(_textBounds.Right + 5, _textBounds.Top, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);

                _infusionBounds = new(_upgrade2.Bounds.Right + 5, _textBounds.Bottom, _statBounds.Width / 2, Content.DefaultFont12.LineHeight + 2);
                _infusion2Bounds = new(_secondUpgradeTextBounds.Left, _secondUpgradeTextBounds.Bottom, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);

                Upgrade1.Bounds = _upgrade.Bounds;
                Infusion1.Bounds = _infusions[0].Bounds;

                Upgrade2.Bounds = _upgrade2.Bounds;
                Infusion2.Bounds = _infusions[1].Bounds;
            }
            else if (GearSlot is GearTemplateSlot.AquaBreather)
            {
                _upgrade.Bounds = new(_icon.Bounds.Right + 2, 0, upgradeSize, upgradeSize);
                for (int i = 0; i < _infusions.Length; i++)
                {
                    _infusions[i].Bounds = new(_icon.Bounds.Right + 2, upgradeSize + 4, upgradeSize, upgradeSize);
                }

                _statIconBounds = new(_upgrade.Bounds.Right + upgradeSize + 4, _upgrade.Bounds.Top, Content.DefaultFont16.LineHeight, Content.DefaultFont16.LineHeight);
                _statBounds = new(_statIconBounds.Right + 2, _upgrade.Bounds.Top, Width - _statIconBounds.Right, Content.DefaultFont16.LineHeight + 2);

                _textBounds = new(_upgrade.Bounds.Right + 5 + upgradeSize + 4, _statBounds.Bottom, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);
                _infusionBounds = new(_upgrade.Bounds.Right + 5 + upgradeSize + 4, _textBounds.Bottom, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);

                Upgrade1.Bounds = _upgrade.Bounds;
                Infusion1.Bounds = _infusions[0].Bounds;
            }
            else if (!isJuwellery)
            {
                int padding = GearSlot is GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand ? 9 : 0;

                if (Template != null && !Template.PvE)
                {
                    padding = GearSlot is GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand ? 5 : 0;
                    upgradeSize = 48;
                    _upgrade.Bounds = new(_icon.Bounds.Right + 2 + padding, (_icon.Bounds.Height - upgradeSize) / 2, upgradeSize, upgradeSize);

                    _statIconBounds = new(_upgrade.Bounds.Right + 2, _upgrade.Bounds.Top, Content.DefaultFont16.LineHeight, Content.DefaultFont16.LineHeight);
                    _statBounds = new(_statIconBounds.Right + 2, _upgrade.Bounds.Top, Width - _statIconBounds.Right, Content.DefaultFont16.LineHeight + 2);
                    //_statBounds = new(_upgrade.Bounds.Right + 5, _upgrade.Bounds.Top, Width - (_upgrade.Bounds.Right + 5), Content.DefaultFont16.LineHeight + 2);
                    _textBounds = new(_upgrade.Bounds.Right + 5, _statBounds.Bottom, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);
                    _infusionBounds = new(_upgrade.Bounds.Right + 5, _textBounds.Bottom, _statBounds.Width, Content.DefaultFont12.LineHeight + 2);

                    Upgrade1.Bounds = _upgrade.Bounds;
                    Infusion1.Bounds = _infusions[0].Bounds;
                }
                else
                {
                    _upgrade.Bounds = new(_icon.Bounds.Right + 2 + padding, 0, upgradeSize, upgradeSize);

                    for (int i = 0; i < _infusions.Length; i++)
                    {
                        _infusions[i].Bounds = new(_icon.Bounds.Right + 2 + padding, upgradeSize + 4, upgradeSize, upgradeSize);
                    }

                    Upgrade1.Bounds = _upgrade.Bounds;
                    Infusion1.Bounds = _infusions[0].Bounds;

                    _statIconBounds = new(_upgrade.Bounds.Right + 2, _upgrade.Bounds.Top, Content.DefaultFont16.LineHeight, Content.DefaultFont16.LineHeight);
                    _statBounds = new(_statIconBounds.Right + 2, _upgrade.Bounds.Top, Width - _statIconBounds.Right, Content.DefaultFont16.LineHeight + 2);

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

                Infusion1.Bounds = _infusions.Length > 0 ? _infusions[0].Bounds : Rectangle.Empty;
                Infusion2.Bounds = _infusions.Length > 1 ? _infusions[1].Bounds : Rectangle.Empty;
                Infusion3.Bounds = _infusions.Length > 2 ? _infusions[2].Bounds : Rectangle.Empty;

                _statIconBounds = new(_item.Bounds.Center, new(_item.Bounds.Height / 2));
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            //_statColor = Colors.DullColor;
            //_statColor = Colors.Chardonnay;
            //_statColor = Colors.ColonialWhite;
            _statColor = new(255, 255, 255);
            //_statColor = new(0, 255, 0);
            //_statColor = new(85, 153, 255);
            //_statColor = new(153, 51, 255);

            _icon.Draw(this, spriteBatch, RelativeMousePosition);
            int val = GearSlot.IsJuwellery() ? 100 : 255;
            _item.Draw(this, spriteBatch, RelativeMousePosition, new Color(val, val, val));

            if (Stat != null && Stat.Icon != null)
            {
                spriteBatch.DrawOnCtrl(this, Stat.Icon, _statIconBounds);
            }

            bool hasUpgrades = GearSlot is not (GearTemplateSlot.Accessory_1 or GearTemplateSlot.Accessory_2 or GearTemplateSlot.Amulet or GearTemplateSlot.Back or GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2);

            if (GearSlot is not (GearTemplateSlot.Accessory_1 or GearTemplateSlot.Accessory_2 or GearTemplateSlot.Amulet or GearTemplateSlot.Back or GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2))
            {
                _upgrade.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, Stat?.Name ?? "No Stat Selected", Content.DefaultFont16, _statBounds, _statColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }

            if (GearSlot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic)
            {
                _upgrade2.Draw(this, spriteBatch, RelativeMousePosition);
            }

            if (GearSlot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic)
            {
                spriteBatch.DrawStringOnCtrl(this, Upgrade1.Item?.Name ?? "No Upgrade", Content.DefaultFont12, _textBounds, _upgradeColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
                spriteBatch.DrawStringOnCtrl(this, Upgrade2.Item?.Name ?? "No Upgrade", Content.DefaultFont12, _secondUpgradeTextBounds, _upgradeColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }
            else if (GearSlot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic or GearTemplateSlot.MainHand or GearTemplateSlot.AltMainHand)
            {
                spriteBatch.DrawStringOnCtrl(this, Upgrade1.Item?.Name ?? "No Upgrade", Content.DefaultFont12, _textBounds, _upgradeColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }
            else if (GearSlot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic or GearTemplateSlot.MainHand or GearTemplateSlot.OffHand or GearTemplateSlot.AltMainHand or GearTemplateSlot.AltOffHand)
            {
                spriteBatch.DrawStringOnCtrl(this, Upgrade1.Item?.Name ?? "No Upgrade", Content.DefaultFont12, _textBounds, _upgradeColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }
            else if (hasUpgrades)
            {
                spriteBatch.DrawStringOnCtrl(this, Upgrade1.Item?.Name ?? "No Upgrade", Content.DefaultFont12, _textBounds, _upgradeColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
            }

            if (Template != null && Template.PvE)
            {
                for (int i = 0; i < _infusions.Length; i++)
                {
                    _infusions[i].Draw(this, spriteBatch, RelativeMousePosition);
                }

                Infusion1.Draw(this, spriteBatch, RelativeMousePosition);
                Infusion2.Draw(this, spriteBatch, RelativeMousePosition);
                Infusion3.Draw(this, spriteBatch, RelativeMousePosition);

                if (hasUpgrades)
                {
                    if (_infusions.Length > 0) spriteBatch.DrawStringOnCtrl(this, Infusion1.Item?.Name ?? "No Infusion", Content.DefaultFont12, _infusionBounds, _infusionColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);
                    if (_infusions.Length > 1) spriteBatch.DrawStringOnCtrl(this, Infusion2.Item?.Name ?? "No Infusion", Content.DefaultFont12, _infusion2Bounds, _infusionColor, false, Blish_HUD.Controls.HorizontalAlignment.Left, Blish_HUD.Controls.VerticalAlignment.Top);

                    Upgrade1.Draw(this, spriteBatch, RelativeMousePosition);
                    Upgrade2.Draw(this, spriteBatch, RelativeMousePosition);
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
