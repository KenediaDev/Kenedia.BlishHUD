using Container = Blish_HUD.Controls.Container;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Blish_HUD.Content;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using static Kenedia.Modules.BuildsManager.Controls.Selection.SelectionPanel;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots
{
    public class ArmorSlot : GearSlot
    {
        private readonly DetailedTexture _runeSlotTexture = new() { Texture = AsyncTexture2D.FromAssetId(784323), TextureRegion = new(37, 37, 54, 54), };
        private readonly DetailedTexture _infusionSlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };

        private readonly DetailedTexture _statTexture = new() { };

        private readonly ItemTexture _runeTexture = new() { };
        private readonly ItemTexture _infusionTexture = new() { };

        private Stat _stat;
        private Rune _rune;
        private Infusion _infusion;

        private Rectangle _runeBounds;
        private Rectangle _infusionBounds;

        public ArmorSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _infusionSlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Rune Rune { get => _rune; set => Common.SetProperty(ref _rune, value, OnRuneChanged); }

        public Infusion Infusion { get => _infusion; set => Common.SetProperty(ref _infusion, value, OnInfusionChanged); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int upgradeSize = (Icon.Bounds.Size.Y - 4) / 2;
            int iconPadding = 0;
            int textPadding = Slot is TemplateSlotType.AquaBreather ? upgradeSize + 5 : 5;

            int size = Math.Min(Width, Height);
            int padding = 3;
            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new Point(-padding, -padding)), new((size - (padding * 2)) / 2));

            _runeSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, 0, upgradeSize, upgradeSize);
            _runeTexture.Bounds = _runeSlotTexture.Bounds;

            _infusionSlotTexture.Bounds = new(Icon.Bounds.Right + 2 + iconPadding, _runeSlotTexture.Bounds.Bottom + 4, upgradeSize, upgradeSize);
            _infusionTexture.Bounds = _infusionSlotTexture.Bounds;

            int x = _runeSlotTexture.Bounds.Right + textPadding + 4;
            _runeBounds = new(x, _runeSlotTexture.Bounds.Top - 1, Width - x, _runeSlotTexture.Bounds.Height);
            _infusionBounds = new(x, _infusionSlotTexture.Bounds.Top, Width - x, _infusionSlotTexture.Bounds.Height);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (TemplatePresenter.IsPve != false)
            {
                base.Paint(spriteBatch, bounds);
                _statTexture.Draw(this, spriteBatch);
                _runeSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _runeTexture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Rune?.DisplayText ?? string.Empty), UpgradeFont, _runeBounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);

                _infusionSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
                _infusionTexture.Draw(this, spriteBatch, RelativeMousePosition);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Infusion?.DisplayText ?? string.Empty), InfusionFont, _infusionBounds, InfusionColor, true, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter.Template[Slot] as ArmorTemplateEntry;

            Infusion = armor?.Infusion;
            Rune = armor?.Rune;
            Stat = armor?.Stat;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (Icon.Hovered)
            {
                SelectionPanel?.SetAnchor<Stat>(this, new Rectangle(a.Location, Point.Zero).Add(Icon.Bounds), SelectionTypes.Stats, Slot, GearSubSlotType.None, (stat) =>
                {
                    (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Stat = stat;
                    Stat = stat;
                },
                (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Armor?.StatChoices,
                (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Armor?.AttributeAdjustment);
            }

            if (_runeSlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Rune>(this, new Rectangle(a.Location, Point.Zero).Add(_runeSlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Rune, (rune) =>
                {
                    (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Rune = rune;
                    Rune = rune;
                });
            }

            if (_infusionSlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusionSlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Infusion = infusion;
                    Infusion = infusion;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => "Reset", () => "Reset stat, rune and infusion", () =>
            {
                Stat = null;
                Rune = null;
                Infusion = null;
            }, new()
            {
                new(() => "Stat", () => "Reset the stat.", () => Stat = null ),
                new(() => "Rune", () => "Reset the rune.", () => Rune = null ),
                new(() => "Infusion", () => "Reset the infusion.", () => Infusion = null ),
                });

            CreateSubMenu(() => "Fill", () => "Fill the stat, rune and infusion of all empty armor slots", () =>
            {
                SetGroupStat(Stat, false);
                SetGroupRune(Rune, false);
                SetGroupInfusion(Infusion, false);
            }, new()
            {
                new(() => "Stat", () => "Fill all empty stat slots.", () => SetGroupStat(Stat, false)),
                new(() => "Rune", () => "Fill all empty rune slots.", () => SetGroupRune(Rune, false)),
                new(() => "Infusion", () => "Fill all empty infusion slots.", () => SetGroupInfusion(Infusion, false)),
                });

            CreateSubMenu(() => "Override", () => "Override the stat, rune and infusion of all armor slots", () =>
            {
                SetGroupStat(Stat, true);
                SetGroupRune(Rune, true);
                SetGroupInfusion(Infusion, true);
            }, new()
            {
                new(() => "Stat", () => "Override all stat slots.", () => SetGroupStat(Stat, true)),
                new(() => "Rune", () => "Override all rune slots.", () => SetGroupRune(Rune, true)),
                new(() => "Infusion", () => "Override all infusion slots.", () => SetGroupInfusion(Infusion, true)),
                });

            CreateSubMenu(() => "Reset all armor", () => "Reset all stats, runes and infusions for all armor slots", () =>
            {
                SetGroupStat(null, true);
                SetGroupRune(null, true);
                SetGroupInfusion(null, true);
            }, new()
            {
                new(() => "Stats", () => "Reset all stats of all armor slots.", () => SetGroupStat(null, true)),
                new(() => "Runes", () => "Reset all runes of all armor slots.", () => SetGroupRune(null, true)),
                new(() => "Infusions", () => "Reset all infusions of all armor slots.", () => SetGroupInfusion(null, true) ),
                });
        }

        private void SetGroupStat(Stat stat, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                var armor = slot as ArmorSlot;

                if (overrideExisting || armor.Stat == null)
                {
                    armor.Stat = stat;
                    (TemplatePresenter.Template[armor.Slot] as ArmorTemplateEntry).Stat = stat;
                }
            }
        }

        private void SetGroupRune(Rune rune, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                var armor = slot as ArmorSlot;

                if (overrideExisting || armor.Rune == null)
                {
                    armor.Rune = rune;
                    (TemplatePresenter.Template[armor.Slot] as ArmorTemplateEntry).Rune = rune;
                }
            }
        }

        private void SetGroupInfusion(Infusion infusion, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                var armor = slot as ArmorSlot;

                if (overrideExisting || armor.Infusion == null)
                {
                    armor.Infusion = infusion;
                    (TemplatePresenter.Template[armor.Slot] as ArmorTemplateEntry).Infusion = infusion;
                }
            }
        }

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            _statTexture.Texture = Stat?.Icon;
        }

        private void OnRuneChanged(object sender, Core.Models.ValueChangedEventArgs<Rune> e)
        {
            _runeTexture.Texture = Rune?.Icon;
        }

        private void OnInfusionChanged(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusionTexture.Texture = Infusion?.Icon;
        }
    }
}
