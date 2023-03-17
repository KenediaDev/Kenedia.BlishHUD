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
        private readonly Dictionary<GearTemplateSlot, List<Selectable>> _selectablesPerSlot = new();
        private readonly List<Selectable> _selectables = new();
        private GearTemplateSlot _activeSlot;
        private Template _template;
        private readonly List<Selectable> _armors;
        private readonly List<Selectable> _trinkets;
        private readonly List<Selectable> _backs;
        private readonly List<Selectable> _weapons;
        private readonly List<Selectable> _pveSigils;
        private readonly List<Selectable> _pvpSigils;
        private readonly List<Selectable> _pveRunes;
        private readonly List<Selectable> _pvpRunes;
        private readonly List<Selectable> _nourishment;
        private readonly List<Selectable> _utilites;
        private readonly List<Selectable> _enrichments;
        private readonly List<Selectable> _infusions;

        public GearSelection()
        {
            List<Selectable> AddItems<T>(IOrderedEnumerable<T> items)
                where T : BaseItem
            {
                List<Selectable> list = new();
                Selectable entry;

                foreach (var item in items)
                {
                    _selectables.Add(entry = new()
                    {
                        Parent = SelectionContent,
                        Item = item,
                        Visible = false,
                        OnClickAction = () =>
                        {
                            if (Template != null)
                            {
                                OnItemSelected?.Invoke(item);
                            }
                        }
                    });
                    list.Add(entry);
                }

                return list;
            }

            _armors = AddItems(BuildsManager.Data.Armors.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _trinkets = AddItems(BuildsManager.Data.Trinkets.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _backs = AddItems(BuildsManager.Data.Backs.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _weapons = AddItems(BuildsManager.Data.Weapons.Values.OrderBy(e => e.WeaponType).ThenByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _pveSigils = AddItems(BuildsManager.Data.PveSigils.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _pvpSigils = AddItems(BuildsManager.Data.PvpSigils.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _pveRunes = AddItems(BuildsManager.Data.PveRunes.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _pvpRunes = AddItems(BuildsManager.Data.PvpRunes.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _nourishment = AddItems(BuildsManager.Data.Nourishments.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _utilites = AddItems(BuildsManager.Data.Utilities.Values.OrderByDescending(e => e.Name).ThenBy(e => e.Id));
            _enrichments = AddItems(BuildsManager.Data.Enrichments.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _infusions = AddItems(BuildsManager.Data.Infusions.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
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

        public Action<BaseItem> OnItemSelected { get; set; }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        private void ApplyTemplate()
        {

        }

        public GearTemplateSlot ActiveSlot { get => _activeSlot; set => Common.SetProperty(ref _activeSlot, value, ApplySlot); }

        public void ApplySlot()
        {
            foreach (var item in _selectables)
            {
                item.Visible = false;
            }

            switch (ActiveSlot)
            {
                case GearTemplateSlot.Head:
                case GearTemplateSlot.Shoulder:
                case GearTemplateSlot.Chest:
                case GearTemplateSlot.Hand:
                case GearTemplateSlot.Leg:
                case GearTemplateSlot.Foot:
                case GearTemplateSlot.AquaBreather:
                    foreach(var item in _armors)
                    {
                        item.Visible =
                            item.Item?.TemplateSlot == ActiveSlot && 
                            Template?.Profession.GetArmorType() == (item.Item as Armor).Weight;
                    }

                    break;

                case GearTemplateSlot.MainHand:
                case GearTemplateSlot.AltMainHand:
                case GearTemplateSlot.OffHand:
                case GearTemplateSlot.AltOffHand:
                case GearTemplateSlot.Aquatic:
                case GearTemplateSlot.AltAquatic:
                    foreach (var item in _weapons)
                    {
                        item.Visible =
                            item.Item?.TemplateSlot == ActiveSlot;
                    }
                    break;

                case GearTemplateSlot.Amulet:
                    foreach (var item in _trinkets)
                    {
                        item.Visible =
                            item.Item?.TemplateSlot == ActiveSlot;
                    }

                    break;

                case GearTemplateSlot.Back:
                    foreach (var item in _backs)
                    {
                        item.Visible =
                            item.Item?.TemplateSlot == ActiveSlot;
                    }
                    break;

                case GearTemplateSlot.Ring_1:
                case GearTemplateSlot.Ring_2:
                case GearTemplateSlot.Accessory_1:
                case GearTemplateSlot.Accessory_2:
                    foreach (var item in _trinkets)
                    {
                        item.Visible =
                            item.Item?.TemplateSlot == ActiveSlot;
                    }

                    break;

                case GearTemplateSlot.Nourishment:
                    foreach (var item in _nourishment)
                    {
                        item.Visible =
                            item.Item?.TemplateSlot == ActiveSlot;
                    }
                    break;

                case GearTemplateSlot.Utility:
                    foreach (var item in _utilites)
                    {
                        item.Visible =
                            item.Item?.TemplateSlot == ActiveSlot;
                    }
                    break;

                case GearTemplateSlot.JadeBotCore:

                    break;
            }

            bool light = Template != null && Template.Profession is Gw2Sharp.Models.ProfessionType.Elementalist or Gw2Sharp.Models.ProfessionType.Mesmer or Gw2Sharp.Models.ProfessionType.Necromancer;
            bool medium = Template != null && Template.Profession is Gw2Sharp.Models.ProfessionType.Ranger or Gw2Sharp.Models.ProfessionType.Thief or Gw2Sharp.Models.ProfessionType.Engineer;
            bool heavy = Template != null && Template.Profession is Gw2Sharp.Models.ProfessionType.Warrior or Gw2Sharp.Models.ProfessionType.Guardian or Gw2Sharp.Models.ProfessionType.Revenant;

            if (false)
            {
                foreach (var selectables in _selectablesPerSlot)
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
            }

            SelectionContent.Invalidate();
        }
    }
}
