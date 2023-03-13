using Blish_HUD.Input;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class GearSelection : BaseSelection
    {
        private Dictionary<GearTemplateSlot, List<Selectable>> _selectables = new();
        private GearTemplateSlot _activeSlot;
        private Template _template;

        public GearSelection()
        {
            var armorslots = new List<GearTemplateSlot>()
            {
                GearTemplateSlot.Head,
                GearTemplateSlot.Shoulder,
                GearTemplateSlot.Chest,
                GearTemplateSlot.Hand,
                GearTemplateSlot.Leg,
                GearTemplateSlot.Foot,
                GearTemplateSlot.AquaBreather,
            };

            var trinketslots = new List<GearTemplateSlot>()
            {
                GearTemplateSlot.Amulet,
                GearTemplateSlot.Accessory_1,
                GearTemplateSlot.Accessory_2,
                GearTemplateSlot.Ring_1,
                GearTemplateSlot.Ring_2,
            };

            var backslots = new List<GearTemplateSlot>()
            {
                GearTemplateSlot.Back,
            };

            var mainHandSlots = new List<GearTemplateSlot>()
            {
                GearTemplateSlot.MainHand,
                GearTemplateSlot.AltMainHand,
            };

            var offHandSlots = new List<GearTemplateSlot>()
            {
                GearTemplateSlot.OffHand,
                GearTemplateSlot.AltOffHand,
            };

            var aquaticSlots = new List<GearTemplateSlot>()
            {
                GearTemplateSlot.Aquatic,
                GearTemplateSlot.AltAquatic,
            };

            foreach (GearTemplateSlot slot in armorslots)
            {
                _selectables[slot] = new();

                foreach (var item in BuildsManager.Data.Armors.Values.Where(e => e.TemplateSlot == slot).OrderByDescending(e => e.Rarity).ThenBy(e => e.Id))
                {
                    _selectables[slot].Add(new()
                    {
                        Parent = SelectionContent,
                        TemplateSlot = slot,
                        Item = item,
                        Visible = false,
                        OnClickAction = () =>
                        {
                            if(Template != null)
                            {
                                Template.GearTemplate.Armors[slot].Item = item;
                            }
                        }
                    });
                }
            };

            foreach (GearTemplateSlot slot in trinketslots)
            {
                _selectables[slot] = new();

                Debug.WriteLine($"Adding Items for Slot {slot}");
                var effectiveSlot = slot is GearTemplateSlot.Ring_2 ? GearTemplateSlot.Ring_1 : slot is GearTemplateSlot.Accessory_2 ? GearTemplateSlot.Accessory_1 : slot;

                foreach (var item in BuildsManager.Data.Trinkets.Values.Where(e => e.TemplateSlot == effectiveSlot).OrderByDescending(e => e.Rarity).ThenBy(e => e.Id))
                {
                    Debug.WriteLine($"Adding Items for Slot {slot} {_selectables[slot].Count}");
                    _selectables[slot].Add(new()
                    {
                        Parent = SelectionContent,
                        TemplateSlot = slot,
                        Item = item,
                        Visible = false,
                        OnClickAction = () =>
                        {
                            if (Template != null)
                            {
                                Debug.WriteLine($"SLOT {slot}");
                                Template.GearTemplate.Juwellery[slot].Item = item;
                            }
                        }
                    });
                }
            };

            foreach (GearTemplateSlot slot in backslots)
            {
                _selectables[slot] = new();

                foreach (var item in BuildsManager.Data.Backs.Values.Where(e => e.TemplateSlot == slot).OrderByDescending(e => e.Rarity).ThenBy(e => e.Id))
                {
                    _selectables[slot].Add(new()
                    {
                        Parent = SelectionContent,
                        TemplateSlot = slot,
                        Item = item,
                        Visible = false,
                        OnClickAction = () =>
                        {
                            if (Template != null)
                            {
                                Template.GearTemplate.Juwellery[slot].Item = item;
                            }
                        }
                    });
                }
            };

            foreach (GearTemplateSlot slot in mainHandSlots)
            {
                _selectables[slot] = new();

                foreach (var item in BuildsManager.Data.Weapons.Values.OrderByDescending(e => e.Rarity).OrderBy(e => e.WeaponType))
                {
                    if (item.WeaponType is not ItemWeaponType.Shield and not ItemWeaponType.Warhorn and not ItemWeaponType.Focus and not ItemWeaponType.Torch and not ItemWeaponType.Speargun and not ItemWeaponType.Harpoon and not ItemWeaponType.Trident)
                    {
                        _selectables[slot].Add(new()
                        {
                            Parent = SelectionContent,
                            TemplateSlot = slot,
                            Item = item,
                            Visible = false,
                            OnClickAction = () =>
                            {
                                if (Template != null)
                                {
                                    Template.GearTemplate.Weapons[slot].Item = item;
                                }
                            }
                        });
                    }
                }
            };

            foreach (GearTemplateSlot slot in offHandSlots)
            {
                _selectables[slot] = new();

                foreach (var item in BuildsManager.Data.Weapons.Values.OrderByDescending(e => e.Rarity).OrderBy(e => e.WeaponType))
                {
                    if (item.WeaponType is not ItemWeaponType.Speargun and not ItemWeaponType.Harpoon and not ItemWeaponType.Trident and not ItemWeaponType.Scepter and not ItemWeaponType.Greatsword and not ItemWeaponType.LongBow and not ItemWeaponType.ShortBow and not ItemWeaponType.Staff and not ItemWeaponType.Hammer and not ItemWeaponType.Rifle)
                    {
                        _selectables[slot].Add(new()
                        {
                            Parent = SelectionContent,
                            TemplateSlot = slot,
                            Item = item,
                            Visible = false,
                            OnClickAction = () =>
                            {
                                if (Template != null)
                                {
                                    Template.GearTemplate.Weapons[slot].Item = item;
                                }
                            }
                        });
                    }
                }
            };

            foreach (GearTemplateSlot slot in aquaticSlots)
            {
                _selectables[slot] = new();

                foreach (var item in BuildsManager.Data.Weapons.Values.OrderByDescending(e => e.Rarity).OrderBy(e => e.WeaponType))
                {
                    if (item.WeaponType is ItemWeaponType.Speargun or ItemWeaponType.Harpoon or ItemWeaponType.Trident)
                    {
                        _selectables[slot].Add(new()
                        {
                            Parent = SelectionContent,
                            TemplateSlot = slot,
                            Item = item,
                            Visible = false,
                            OnClickAction = () =>
                            {
                                if (Template != null)
                                {
                                    Template.GearTemplate.Weapons[slot].Item = item;
                                }
                            }
                        });
                    }
                }
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

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        private void ApplyTemplate()
        {

        }

        public GearTemplateSlot ActiveSlot { get => _activeSlot; set => Common.SetProperty(ref _activeSlot, value, ApplySlot); }

        public void ApplySlot()
        {
            bool light = Template != null && Template.Profession is Gw2Sharp.Models.ProfessionType.Elementalist or Gw2Sharp.Models.ProfessionType.Mesmer or Gw2Sharp.Models.ProfessionType.Necromancer;
            bool medium = Template != null && Template.Profession is Gw2Sharp.Models.ProfessionType.Ranger or Gw2Sharp.Models.ProfessionType.Thief or Gw2Sharp.Models.ProfessionType.Engineer;
            bool heavy = Template != null && Template.Profession is Gw2Sharp.Models.ProfessionType.Warrior or Gw2Sharp.Models.ProfessionType.Guardian or Gw2Sharp.Models.ProfessionType.Revenant;

            foreach (var selectables in _selectables)
            {
                foreach (var item in selectables.Value)
                {
                    bool slotMatch = item.TemplateSlot == ActiveSlot; // || (ActiveSlot == GearTemplateSlot.Ring_2 && item.TemplateSlot == GearTemplateSlot.Ring_1) || (ActiveSlot == GearTemplateSlot.Accessory_2 && item.TemplateSlot == GearTemplateSlot.Accessory_1);

                    bool weaponMatch = false;

                    if (ActiveSlot.IsWeapon() && Template != null && BuildsManager.Data.Professions.ContainsKey(Template.Profession))
                    {
                        bool slotIsOffhand = ActiveSlot is GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand;

                        foreach (var weapon in BuildsManager.Data.Professions[Template.Profession].Weapons)
                        {
                            if (item.Item != null && item.Item.Type == ItemType.Weapon)
                            {
                                if ((item.Item as Weapon).WeaponType.IsWeaponType(weapon.Value.Type))
                                {
                                    bool wieldMatch = slotIsOffhand ? weapon.Value.Wielded.HasFlag(Gw2Sharp.ProfessionWeaponFlag.Offhand) : weapon.Value.Wielded.HasFlag(Gw2Sharp.ProfessionWeaponFlag.Mainhand) || weapon.Value.Wielded.HasFlag(Gw2Sharp.ProfessionWeaponFlag.TwoHand);
                                    // No Elite Spec
                                    if (weapon.Value.Specialization == 0 && wieldMatch)
                                    {
                                        weaponMatch = true;
                                        break;
                                    }

                                    //Elite Spec Matched
                                    if (weapon.Value.Specialization == Template.EliteSpecialization?.Id && wieldMatch)
                                    {
                                        weaponMatch = true;
                                        break;
                                    }

                                    if (weapon.Value.SpecializationWielded != null && wieldMatch)
                                    {
                                        if ((slotIsOffhand && !weapon.Value.SpecializationWielded.Value.HasFlag(Gw2Sharp.ProfessionWeaponFlag.Offhand)) || (!slotIsOffhand && weapon.Value.SpecializationWielded.Value.HasFlag(Gw2Sharp.ProfessionWeaponFlag.Offhand)))
                                        {
                                            weaponMatch = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        weaponMatch = true;
                    }

                    item.Visible = weaponMatch && slotMatch && (!ActiveSlot.IsArmor() ||
                        (light ? (item.Item as Armor).Weight == ItemWeightType.Light :
                        medium ? (item.Item as Armor).Weight == ItemWeightType.Medium :
                        heavy && (item.Item as Armor).Weight == ItemWeightType.Heavy));
                }
            }

            SelectionContent.Invalidate();
        }
    }
}
