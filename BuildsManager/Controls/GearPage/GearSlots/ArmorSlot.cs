using Container = Blish_HUD.Controls.Container;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using static Kenedia.Modules.BuildsManager.Controls.Selection.SelectionPanel;
using Kenedia.Modules.BuildsManager.Res;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots
{
    public class ArmorSlot : GearSlot
    {
        private readonly ItemControl _runeControl = new(new(784323) { TextureRegion = new(38, 38, 52, 52) });
        private readonly ItemControl _infusionControl = new(new (){ TextureRegion = new(38, 38, 52, 52) });

        private Stat _stat;
        private Rune _rune;
        private Infusion _infusion;

        private Rectangle _runeBounds;
        private Rectangle _infusionBounds;

        public ArmorSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _infusionControl.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");

            _runeControl.Parent = this;
            _infusionControl.Parent = this;
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Rune Rune { get => _rune; set => Common.SetProperty(ref _rune, value, OnRuneChanged); }

        public Infusion Infusion { get => _infusion; set => Common.SetProperty(ref _infusion, value, OnInfusionChanged); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int upgradeSize = (ItemControl.Height - 4) / 2;
            int iconPadding = 0;
            int textPadding = Slot is TemplateSlotType.AquaBreather ? upgradeSize + 5 : 5;

            _runeControl.SetBounds(new(ItemControl.Right + 2 + iconPadding, iconPadding , upgradeSize, upgradeSize));
            _infusionControl.SetBounds(new(ItemControl.Right + 2 + iconPadding, ItemControl.Bottom - (upgradeSize + iconPadding), upgradeSize, upgradeSize));

            int x = _runeControl.LocalBounds.Right + textPadding + 4;
            _runeBounds = new(x, _runeControl.LocalBounds.Top - 1, Width - x, _runeControl.LocalBounds.Height);
            _infusionBounds = new(x, _infusionControl.LocalBounds.Top, Width - x, _infusionControl.LocalBounds.Height);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (TemplatePresenter.IsPve != false)
            {
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Rune?.DisplayText ?? string.Empty), UpgradeFont, _runeBounds, UpgradeColor, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
                spriteBatch.DrawStringOnCtrl(this, GetDisplayString(Infusion?.DisplayText ?? string.Empty), InfusionFont, _infusionBounds, InfusionColor, true, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter?.Template?[Slot] as ArmorTemplateEntry;

            Infusion = armor?.Infusion;
            Rune = armor?.Rune;
            Stat = armor?.Stat;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            var a = AbsoluteBounds;

            if (ItemControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Stat>(this, new Rectangle(a.Location, Point.Zero).Add(ItemControl.LocalBounds), SelectionTypes.Stats, Slot, GearSubSlotType.None, (stat) =>
                {
                    (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Stat = stat;
                    Stat = stat;
                },
                (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Armor?.StatChoices,
                (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Armor?.AttributeAdjustment);
            }

            if (_runeControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Rune>(this, new Rectangle(a.Location, Point.Zero).Add(_runeControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Rune, (rune) =>
                {
                    (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Rune = rune;
                    Rune = rune;
                });
            }

            if (_infusionControl.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusionControl.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as ArmorTemplateEntry).Infusion = infusion;
                    Infusion = infusion;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => strings.Reset, () => string.Format(strings.ResetEntry, $"{strings.Stat}, {strings.Rune} {strings.And} {strings.Infusion}"), () =>
            {
                Stat = null;
                Rune = null;
                Infusion = null;
            }, new()
            {
                new(() => strings.Stat, () => string.Format(strings.ResetEntry, strings.Stat), () => Stat = null ),
                new(() => strings.Rune, () => string.Format(strings.ResetEntry, strings.Rune), () => Rune = null ),
                new(() => strings.Infusion, () => string.Format(strings.ResetEntry, strings.Infusion), () => Infusion = null ),
                });

            CreateSubMenu(() => strings.Fill, () => string.Format(strings.FillEntry, $"{strings.Stat}, {strings.Rune} {strings.And} {strings.Infusion} {strings.EmptyArmorSlots}"), () =>
            {
                SetGroupStat(Stat, false);
                SetGroupRune(Rune, false);
                SetGroupInfusion(Infusion, false);
            }, new()
            {
                new(() => strings.Stat, () => string.Format(strings.FillEntry, $"{strings.Stat} {strings.EmptyArmorSlots}"), () => SetGroupStat(Stat, false)),
                new(() => strings.Rune, () => string.Format(strings.FillEntry, $"{strings.Rune} {strings.EmptyArmorSlots}"), () => SetGroupRune(Rune, false)),
                new(() => strings.Infusion, () => string.Format(strings.FillEntry, $"{strings.Infusion} {strings.EmptyArmorSlots}"), () => SetGroupInfusion(Infusion, false)),
                });

            CreateSubMenu(() => strings.Override, () => string.Format(strings.OverrideEntry, $"{strings.Stat}, {strings.Rune} {strings.And} {strings.Infusions} {strings.ArmorSlots}"), () =>
            {
                SetGroupStat(Stat, true);
                SetGroupRune(Rune, true);
                SetGroupInfusion(Infusion, true);
            }, new()
            {
                new(() => strings.Stat, () => string.Format(strings.OverrideEntry, $"{strings.Stat} {strings.ArmorSlots}"), () => SetGroupStat(Stat, true)),
                new(() => strings.Rune, () => string.Format(strings.OverrideEntry, $"{strings.Rune} {strings.ArmorSlots}"), () => SetGroupRune(Rune, true)),
                new(() => strings.Infusion, () => string.Format(strings.OverrideEntry, $"{strings.Infusion} {strings.ArmorSlots}"), () => SetGroupInfusion(Infusion, true)),
                });

            CreateSubMenu(() => string.Format(strings.ResetAll, strings.Armors), () => string.Format(strings.ResetEntry, $"{strings.Stats}, {strings.Runes} {strings.And} {strings.Infusions} {strings.ArmorSlots}"), () =>
            {
                SetGroupStat(null, true);
                SetGroupRune(null, true);
                SetGroupInfusion(null, true);
            }, new()
            {
                new(() => strings.Stats, () =>  string.Format(strings.ResetAll, $"{strings.Stats} {strings.ArmorSlots}"), () => SetGroupStat(null, true)),
                new(() => strings.Runes, () => string.Format(strings.ResetAll, $"{strings.Runes} {strings.ArmorSlots}"), () => SetGroupRune(null, true)),
                new(() => strings.Infusions, () => string.Format(strings.ResetAll, $"{strings.Infusions} {strings.ArmorSlots}"), () => SetGroupInfusion(null, true) ),
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
            ItemControl.Stat = Stat;
        }

        private void OnRuneChanged(object sender, Core.Models.ValueChangedEventArgs<Rune> e)
        {
            _runeControl.Item = Rune;
        }

        private void OnInfusionChanged(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusionControl.Item = Infusion;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Stat = null;
            Rune = null;
            Infusion = null;
        }
    }
}
