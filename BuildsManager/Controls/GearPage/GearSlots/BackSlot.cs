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
    public class BackSlot : GearSlot
    {
        private readonly ItemControl _infusion1Control = new(new() { TextureRegion = new(38, 38, 52, 52) });
        private readonly ItemControl _infusion2Control = new(new() { TextureRegion = new(38, 38, 52, 52) });

        private Stat _stat;
        private Infusion _infusion1;
        private Infusion _infusion2;

        public BackSlot(TemplateSlotType gearSlot, Container parent, TemplatePresenter templatePresenter) : base(gearSlot, parent, templatePresenter)
        {
            _infusion1Control.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            _infusion2Control.Placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\infusionslot.png");
            ItemControl.Item = BuildsManager.Data.Backs[94947];

            _infusion1Control.Parent = this;
            _infusion2Control.Parent = this;
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public Infusion Infusion1 { get => _infusion1; set => Common.SetProperty(ref _infusion1, value, OnInfusion1Changed); }

        public Infusion Infusion2 { get => _infusion2; set => Common.SetProperty(ref _infusion2, value, OnInfusion2Changed); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int infusionSize = (ItemControl.LocalBounds.Size.Y - 4) / 3;

            _infusion1Control.SetBounds(new(ItemControl.LocalBounds.Right + 2, ItemControl.LocalBounds.Top, infusionSize, infusionSize));
            _infusion2Control.SetBounds(new(ItemControl.LocalBounds.Right + 2, ItemControl.LocalBounds.Top + ((infusionSize + 2) * 1), infusionSize, infusionSize));
        }

        protected override void SetItems(object sender, EventArgs e)
        {
            base.SetItems(sender, e);

            var armor = TemplatePresenter.Template[Slot] as BackTemplateEntry;

            Infusion1 = armor?.Infusion1;
            Infusion2 = armor?.Infusion2;

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
                    (TemplatePresenter?.Template[Slot] as BackTemplateEntry).Stat = stat;
                    Stat = stat;
                }, (TemplatePresenter?.Template[Slot] as BackTemplateEntry).Back?.StatChoices,
                (TemplatePresenter?.Template[Slot] as BackTemplateEntry).Back?.AttributeAdjustment);
            }

            if (_infusion1Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusion1Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as BackTemplateEntry).Infusion1 = infusion;
                    Infusion1 = infusion;
                });
            }

            if (_infusion2Control.MouseOver)
            {
                SelectionPanel?.SetAnchor<Infusion>(this, new Rectangle(a.Location, Point.Zero).Add(_infusion2Control.LocalBounds), SelectionTypes.Items, Slot, GearSubSlotType.Infusion, (infusion) =>
                {
                    (TemplatePresenter?.Template[Slot] as BackTemplateEntry).Infusion2 = infusion;
                    Infusion2 = infusion;
                });
            }
        }

        protected override void CreateSubMenus()
        {
            base.CreateSubMenus();

            CreateSubMenu(() => "Reset", () => "Reset stat and infusion", () =>
            {
                Stat = null;
                Infusion1 = null;
                Infusion2 = null;
            }, new()
            {
                new(() => "Stat",() => "Reset the stat.",() => Stat = null),
                new(() => "Infusion",() => "Reset the infusions",() => {
                Infusion1 = null;
                Infusion2 = null;
                }),
            });

            CreateSubMenu(() => "Fill", () => "Fill the stat and infusions of all empty juwellery slots", () =>
            {
                SetGroupStat(Stat, false);
                SetGroupInfusion(Infusion1, false);
            }, new()
            {
                new(() => "Stat", () => "Fill all empty stat slots.", () => SetGroupStat(Stat, false)),
                new(() => "Infusion", () => "Fill all empty infusion slots.", () => SetGroupInfusion(Infusion1, false)),
                });

            CreateSubMenu(() => "Override", () => "Override the stat and infusions of all juwellery slots", () =>
            {
                SetGroupStat(Stat, true);
                SetGroupInfusion(Infusion1, true);
            }, new()
            {
                new(() => "Stat", () => "Override all stat slots.", () => SetGroupStat(Stat, true)),
                new(() => "Infusion", () => "Override all infusion slots.", () => SetGroupInfusion(Infusion1, true)),
                });

            CreateSubMenu(() => "Reset all juwellery", () => "Reset all stats and infusions for all juwellery slots", () =>
            {
                SetGroupStat(null, true);
                SetGroupInfusion(null, true);
            }, new()
            {
                new(() => "Stats",() => "Reset all stats of all juwellery slots.",() => SetGroupStat(null, true)),
                new(() => "Infusions",() => "Reset all infusions of all juwellery slots.",() => SetGroupInfusion(null, true)),
            });
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

        private void OnStatChanged(object sender, Core.Models.ValueChangedEventArgs<Stat> e)
        {
            ItemControl.Stat = Stat;
        }

        private void OnInfusion2Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion2Control.Item = Infusion2;
        }

        private void OnInfusion1Changed(object sender, Core.Models.ValueChangedEventArgs<Infusion> e)
        {
            _infusion1Control.Item = Infusion1;
        }
    }
}
