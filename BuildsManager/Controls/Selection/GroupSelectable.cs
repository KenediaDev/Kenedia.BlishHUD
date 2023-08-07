using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class GroupSelectable : Selectable
    {
        private ContextMenuStripItem _allSlots;
        private ContextMenuStripItem _itemGroup;

        public GroupSelectable()
        {
            Menu = new();

            _allSlots = Menu.AddMenuItem("Apply to all slots");
            _allSlots.Click += AllSlots_Click;

            _itemGroup = Menu.AddMenuItem("Apply to item group");
            _itemGroup.Click += ItemGroup_Click;
        }

        protected override void OnTypeChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnTypeChanged(sender, e);

            if (Type != SelectableType.Infusion && _allSlots != null)
            {
                _allSlots.Click -= AllSlots_Click;
                _allSlots.Dispose();
                _allSlots = null;
            }
            else if (_allSlots == null)
            {
                _allSlots = Menu.AddMenuItem("Apply to all slots");
                _allSlots.Click += AllSlots_Click;
            }
        }

        private void ItemGroup_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            switch (Type)
            {
                case SelectableType.Rune:
                    var armors = Template?.GearTemplate?.Armors.Values.Cast<ArmorEntry>();

                    foreach (var slot in armors)
                    {
                        //if (slot.Item is not null)
                        //{
                        //}

                        slot.Rune = (Rune)Item;
                    }

                    break;

                case SelectableType.Sigil:
                    var weapons = Template?.GearTemplate?.Weapons.Values.Cast<WeaponEntry>();

                    foreach (var slot in weapons)
                    {
                        //if (slot.Item is not null)
                        //{
                        //}

                        slot.Sigil = (Sigil)Item;
                        slot.Sigil2 = (Sigil)Item;
                        slot.PvpSigil = (Sigil)Item;
                    }

                    break;

                case SelectableType.Infusion:
                    var slots =
                        ActiveSlot.IsArmor() ? Template?.GearTemplate?.Armors.Values.Cast<JuwelleryEntry>() :
                        ActiveSlot.IsWeapon() ? Template?.GearTemplate?.Weapons.Values.Cast<JuwelleryEntry>() :
                        ActiveSlot.IsJuwellery() ? Template?.GearTemplate?.Juwellery.Values.Cast<JuwelleryEntry>() : null;

                    foreach (var slot in slots)
                    {
                        //if (slot.Item is not null)
                        //{
                        //}

                        slot.Infusion = (Infusion)Item;
                        slot.Infusion2 = (Infusion)Item;
                        slot.Infusion3 = (Infusion)Item;
                    }

                    break;
            }
        }

        private void AllSlots_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            switch (Type)
            {
                case SelectableType.Rune:
                    var armors = Template?.GearTemplate?.Armors.Values.Cast<ArmorEntry>();

                    foreach (var slot in armors)
                    {
                        slot.Rune = (Rune)Item;
                    }

                    break;

                case SelectableType.Sigil:
                    var weapons = Template?.GearTemplate?.Weapons.Values.Cast<WeaponEntry>();

                    foreach (var slot in weapons)
                    {
                        slot.Sigil = (Sigil)Item;
                        slot.Sigil2 = (Sigil)Item;
                    }

                    break;

                case SelectableType.Infusion:
                    var slots = new List<JuwelleryEntry>();

                    slots.AddRange(Template?.GearTemplate?.Armors.Values.Cast<JuwelleryEntry>());
                    slots.AddRange(Template?.GearTemplate?.Weapons.Values.Cast<JuwelleryEntry>());
                    slots.AddRange(Template?.GearTemplate?.Juwellery.Values.Cast<JuwelleryEntry>());

                    foreach (var slot in slots)
                    {
                        slot.Infusion = (Infusion)Item;
                        slot.Infusion2 = (Infusion)Item;
                        slot.Infusion3 = (Infusion)Item;
                    }

                    break;
            }
        }
    }
}
