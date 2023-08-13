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

            var armor = TemplatePresenter.Template[Slot] as ArmorTemplateEntry;

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
    }
}
