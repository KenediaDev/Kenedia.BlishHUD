using Container = Blish_HUD.Controls.Container;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Blish_HUD;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.TemplateEntries;
using static Kenedia.Modules.BuildsManager.Controls.Selection.SelectionPanel;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage.GearSlots
{
    public class AccessoireSlot : GearSlot
    {
        private readonly DetailedTexture _infusionSlotTexture = new() { TextureRegion = new(37, 37, 54, 54) };

        private readonly DetailedTexture _statTexture = new() { };

        private readonly ItemTexture _infusionTexture = new() { };

        private Stat _stat;
        private Infusion _infusion;

        public AccessoireSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _infusionSlotTexture.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            ItemTexture.Item = BuildsManager.Data.Trinkets[80002];
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Infusion Infusion { get => _infusion; set => Common.SetProperty(ref _infusion, value, OnInfusionChanged); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);
            int padding = 3;
            _statTexture.Bounds = new(Icon.Bounds.Center.Add(new Point(-padding, -padding)), new((size - (padding * 2)) / 2));

            int infusionSize = (Icon.Bounds.Size.Y - 4) / 3;

            _infusionSlotTexture.Bounds = new(Icon.Bounds.Right + 2, Icon.Bounds.Top, infusionSize, infusionSize);
            _infusionTexture.Bounds = _infusionSlotTexture.Bounds;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);
            _statTexture.Draw(this, spriteBatch);

            _infusionSlotTexture.Draw(this, spriteBatch, RelativeMousePosition);
            _infusionTexture.Draw(this, spriteBatch, RelativeMousePosition);
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter.Template[Slot] as AccessoireTemplateEntry;

            Infusion = armor?.Infusion;
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
                    (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Stat = stat;
                    Stat = stat;
                }, (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Accessoire?.StatChoices,
                (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Accessoire?.AttributeAdjustment);
            }

            if (_infusionSlotTexture.Hovered)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusionSlotTexture.Bounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as AccessoireTemplateEntry).Infusion = infusion;
                    Infusion = infusion;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => "Reset", () => "Reset stat and infusion", () =>
            {
                Stat = null;
                Infusion = null;
            }, new()
            {
                new(() => "Stat",() => "Reset the stat.",() => Stat = null),
                new(() => "Infusion",() => "Reset the infusion.",() => Infusion = null),
            });

            CreateSubMenu(() => "Fill", () => "Fill the stat and infusions of all empty juwellery slots", () =>
            {
                SetGroupStat(Stat, false);
                SetGroupInfusion(Infusion, false);
            }, new()
            {
                new(() => "Stat", () => "Fill all empty stat slots.", () => SetGroupStat(Stat, false)),
                new(() => "Infusion", () => "Fill all empty infusion slots.", () => SetGroupInfusion(Infusion, false)),
                });

            CreateSubMenu(() => "Override", () => "Override the stat and infusions of all juwellery slots", () =>
            {
                SetGroupStat(Stat, true);
                SetGroupInfusion(Infusion, true);
            }, new()
            {
                new(() => "Stat", () => "Override all stat slots.", () => SetGroupStat(Stat, true)),
                new(() => "Infusion", () => "Override all infusion slots.", () => SetGroupInfusion(Infusion, true)),
                });

            CreateSubMenu(() => "Reset all juwellery", () => "Reset all stats and infusions for all juwellery slots", () =>
            {
                SetGroupStat(null, true);
                SetGroupInfusion(null, true);
            }, new()
            { new(() => "Stats",() => "Reset all stats of all juwellery slots.",() => SetGroupStat(null, true)),
                new(() => "Infusions",() => "Reset all infusions of all juwellery slots.",() => SetGroupInfusion(null, true)), });
        }

        private void SetGroupStat(Stat stat, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                switch (slot.Slot)
                {
                    case TemplateSlotType.Accessory_1:
                    case TemplateSlotType.Accessory_2:
                        var accessoire = slot as AccessoireSlot;
                        accessoire.Stat = overrideExisting ? stat : accessoire.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as AccessoireTemplateEntry).Stat = overrideExisting ? stat : accessoire.Stat ?? stat; ;
                        break;

                    case TemplateSlotType.Back:
                        var back = slot as BackSlot;
                        back.Stat = overrideExisting ? stat : back.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Stat = overrideExisting ? stat : back.Stat ?? stat; ;
                        break;

                    case TemplateSlotType.Amulet:
                        var amulet = slot as AmuletSlot;
                        amulet.Stat = overrideExisting ? stat : amulet.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as AmuletTemplateEntry).Stat = overrideExisting ? stat : amulet.Stat ?? stat; ;
                        break;

                    case TemplateSlotType.Ring_1:
                    case TemplateSlotType.Ring_2:
                        var ring = slot as RingSlot;
                        ring.Stat = overrideExisting ? stat : ring.Stat ?? stat;
                        (TemplatePresenter.Template[slot.Slot] as RingTemplateEntry).Stat = overrideExisting ? stat : ring.Stat ?? stat; ;
                        break;
                }
            }
        }

        private void SetGroupInfusion(Infusion infusion, bool overrideExisting)
        {
            foreach (var slot in SlotGroup)
            {
                switch (slot.Slot)
                {
                    case TemplateSlotType.Accessory_1:
                    case TemplateSlotType.Accessory_2:
                        var accessoire = slot as AccessoireSlot;
                        accessoire.Infusion = overrideExisting ? infusion : accessoire.Infusion ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as AccessoireTemplateEntry).Infusion = overrideExisting ? infusion : accessoire.Infusion ?? infusion;
                        break;

                    case TemplateSlotType.Back:
                        var back = slot as BackSlot;

                        back.Infusion1 = overrideExisting ? infusion : back.Infusion1 ?? infusion;
                        back.Infusion2 = overrideExisting ? infusion : back.Infusion2 ?? infusion;

                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Infusion1 = overrideExisting ? infusion : back.Infusion1 ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as BackTemplateEntry).Infusion2 = overrideExisting ? infusion : back.Infusion2 ?? infusion;
                        break;

                    case TemplateSlotType.Ring_1:
                    case TemplateSlotType.Ring_2:
                        var ring = slot as RingSlot;

                        ring.Infusion1 = overrideExisting ? infusion : ring.Infusion1 ?? infusion;
                        ring.Infusion2 = overrideExisting ? infusion : ring.Infusion2 ?? infusion;
                        ring.Infusion3 = overrideExisting ? infusion : ring.Infusion3 ?? infusion;

                        (TemplatePresenter.Template[slot.Slot] as RingTemplateEntry).Infusion1 = overrideExisting ? infusion : ring.Infusion1 ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as RingTemplateEntry).Infusion2 = overrideExisting ? infusion : ring.Infusion2 ?? infusion;
                        (TemplatePresenter.Template[slot.Slot] as RingTemplateEntry).Infusion3 = overrideExisting ? infusion : ring.Infusion3 ?? infusion;
                        break;
                }
            }
        }

        private void OnInfusionChanged(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusionTexture.Texture = Infusion?.Icon;
        }

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            _statTexture.Texture = Stat?.Icon;
        }
    }
}
