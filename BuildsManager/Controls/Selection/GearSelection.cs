using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class GearSelection : BaseSelection
    {
        private readonly Dictionary<GearTemplateSlot, List<Selectable>> _selectablesPerSlot = new();
        private readonly List<Selectable> _selectables = new();
        private GearTemplateSlot _activeSlot;
        private GearSubSlotType _subSlotType;
        private Template _template;
        private string _filterText;
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

        private readonly Button _autoEquipItems;

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
                                //OnItemSelectedX(item);
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
            _pveSigils = AddItems(BuildsManager.Data.PveSigils.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _pvpSigils = AddItems(BuildsManager.Data.PvpSigils.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _pveRunes = AddItems(BuildsManager.Data.PveRunes.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _pvpRunes = AddItems(BuildsManager.Data.PvpRunes.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _nourishment = AddItems(BuildsManager.Data.Nourishments.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _utilites = AddItems(BuildsManager.Data.Utilities.Values.OrderByDescending(e => e.Name).ThenBy(e => e.Id));
            _enrichments = AddItems(BuildsManager.Data.Enrichments.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _infusions = AddItems(BuildsManager.Data.Infusions.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));

            Search.TextChangedAction = (txt) =>
            {
                _filterText = txt.Trim().ToLower();
                PerformFiltering();
            };

            _autoEquipItems = new()
            {
                Parent = this,
                Location = new(Search.Left + 2, Search.Bottom + 5),
                Text = "Equip Ascended Items",
                ClickAction = EquipItems,
            };

            SelectionContent.SetLocation(Search.Left, _autoEquipItems.Bottom + 5);
        }

        private void EquipItems()
        {
            var armors = _armors.Where(e => (e.Item as Armor).Weight == Template?.Profession.GetArmorType() && (e.Item.Rarity == Gw2Sharp.WebApi.V2.Models.ItemRarity.Ascended || e.TemplateSlot == GearTemplateSlot.AquaBreather))?.Select(e => e.Item);
            foreach (var armor in Template?.GearTemplate?.Armors.Values)
            {
                armor.Item = armors.Where(e => e.TemplateSlot == armor.Slot)?.FirstOrDefault();
            }

            var trinkets = _trinkets.Where(e => e.Item.Rarity == Gw2Sharp.WebApi.V2.Models.ItemRarity.Ascended)?.Select(e => e.Item);
            trinkets = trinkets.Append(_backs.Where(e => e.Item.Rarity == Gw2Sharp.WebApi.V2.Models.ItemRarity.Ascended)?.Select(e => e.Item).FirstOrDefault());
            foreach (var trinket in Template?.GearTemplate?.Juwellery.Values)
            {
                var effectiveSlot =
                    trinket.Slot is GearTemplateSlot.Ring_2 ? GearTemplateSlot.Ring_1 :
                    trinket.Slot is GearTemplateSlot.Accessory_2 ? GearTemplateSlot.Accessory_1 :
                    trinket.Slot;

                trinket.Item = trinkets.Where(e => e.TemplateSlot == effectiveSlot)?.FirstOrDefault();
            }
        }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (temp != null) temp.Changed -= TemplateChanged;
                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        public BaseTemplateEntry TemplateSlot { get; set; }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        private void ApplyTemplate()
        {

        }

        private bool MatchingMethod(BaseItem item)
        {
            return item.Name == null || string.IsNullOrEmpty(_filterText) || item.Name.ToLower().Contains(_filterText);
        }

        public GearTemplateSlot ActiveSlot { get => _activeSlot; set => Common.SetProperty(ref _activeSlot, value, ApplySlot); }

        public GearSubSlotType SubSlotType { get => _subSlotType; set => Common.SetProperty(ref _subSlotType, value, ApplySubSlot); }

        private void ApplySubSlot(object sender, PropertyChangedEventArgs e)
        {
            Search.Text = null;
            ApplySlot();
        }

        public Action<BaseItem> OnItemSelected { get; set; }

        public void PerformFiltering()
        {
            switch (ActiveSlot)
            {
                case GearTemplateSlot.Head:
                case GearTemplateSlot.Shoulder:
                case GearTemplateSlot.Chest:
                case GearTemplateSlot.Hand:
                case GearTemplateSlot.Leg:
                case GearTemplateSlot.Foot:
                case GearTemplateSlot.AquaBreather:
                    if (SubSlotType == GearSubSlotType.Item)
                    {
                        foreach (var item in _armors)
                        {
                            item.Visible =
                                MatchingMethod(item.Item) &&
                                item.Item?.TemplateSlot == ActiveSlot &&
                                Template?.Profession.GetArmorType() == (item.Item as Armor).Weight;
                        }

                    }
                    else if (SubSlotType == GearSubSlotType.Rune)
                    {
                        foreach (var item in Template?.PvE == false ? _pvpRunes : _pveRunes)
                        {
                            item.Visible = MatchingMethod(item.Item);
                        }
                    }
                    else if (SubSlotType == GearSubSlotType.Infusion)
                    {
                        foreach (var item in _infusions)
                        {
                            item.Visible = MatchingMethod(item.Item);
                        }
                    }

                    break;

                case GearTemplateSlot.MainHand:
                case GearTemplateSlot.AltMainHand:
                case GearTemplateSlot.OffHand:
                case GearTemplateSlot.AltOffHand:
                case GearTemplateSlot.Aquatic:
                case GearTemplateSlot.AltAquatic:
                    bool slotIsOffhand = ActiveSlot is GearTemplateSlot.OffHand or GearTemplateSlot.AltOffHand;

                    if (SubSlotType == GearSubSlotType.Item)
                    {
                        foreach (var item in _weapons)
                        {
                            bool weaponMatch = false;

                            var weapon = BuildsManager.Data.Professions[Template.Profession].Weapons.Where(e => (item.Item as DataModels.Items.Weapon).WeaponType.IsWeaponType(e.Value.Type)).FirstOrDefault();

                            if (weapon.Value != null)
                            {
                                bool terrainMatch =
                                    (ActiveSlot is GearTemplateSlot.AltAquatic or GearTemplateSlot.Aquatic) ?
                                    weapon.Value.Type.IsAquatic() :
                                    !weapon.Value.Type.IsAquatic();
                                bool wieldMatch = slotIsOffhand ? weapon.Value.Wielded.HasFlag(WieldingFlag.Offhand) : weapon.Value.Wielded.HasFlag(WieldingFlag.Mainhand) || weapon.Value.Wielded.HasFlag(WieldingFlag.TwoHand);

                                // No Elite Spec
                                if (weapon.Value.Specialization == 0 && wieldMatch)
                                {
                                    weaponMatch = true;
                                }

                                //Elite Spec Matched
                                if (weapon.Value.Specialization == Template.EliteSpecialization?.Id && wieldMatch)
                                {
                                    weaponMatch = true;
                                }

                                var effectiveSlot =
                                    ActiveSlot is GearTemplateSlot.AltAquatic ? GearTemplateSlot.Aquatic :
                                    ActiveSlot is GearTemplateSlot.AltMainHand ? GearTemplateSlot.MainHand :
                                    ActiveSlot is GearTemplateSlot.AltOffHand ? GearTemplateSlot.OffHand :
                                    ActiveSlot;

                                item.Visible =
                                    wieldMatch &&
                                    weaponMatch &&
                                    terrainMatch &&
                                    MatchingMethod(item.Item);
                            }
                        }
                    }
                    else if (SubSlotType == GearSubSlotType.Sigil)
                    {
                        foreach (var item in Template?.PvE == false ? _pvpSigils : _pveSigils)
                        {
                            item.Visible = MatchingMethod(item.Item);
                        }
                    }
                    else if (SubSlotType == GearSubSlotType.Infusion)
                    {
                        foreach (var item in _infusions)
                        {
                            item.Visible = MatchingMethod(item.Item);
                        }
                    }

                    break;

                case GearTemplateSlot.Amulet:
                    if (SubSlotType == GearSubSlotType.Item)
                    {
                        foreach (var item in _trinkets)
                        {
                            item.Visible =
                                MatchingMethod(item.Item) &&
                                item.Item?.TemplateSlot == ActiveSlot;
                        }
                    }
                    else if (SubSlotType == GearSubSlotType.Enrichment)
                    {
                        foreach (var item in _enrichments)
                        {
                            item.Visible = MatchingMethod(item.Item);
                        }
                    }

                    break;

                case GearTemplateSlot.Back:
                    if (SubSlotType == GearSubSlotType.Item)
                    {
                        foreach (var item in _backs)
                        {
                            item.Visible =
                                MatchingMethod(item.Item) &&
                                item.Item?.TemplateSlot == ActiveSlot;
                        }
                    }
                    else if (SubSlotType == GearSubSlotType.Infusion)
                    {
                        foreach (var item in _infusions)
                        {
                            item.Visible = MatchingMethod(item.Item);
                        }
                    }

                    break;

                case GearTemplateSlot.Ring_1:
                case GearTemplateSlot.Ring_2:
                case GearTemplateSlot.Accessory_1:
                case GearTemplateSlot.Accessory_2:
                    if (SubSlotType == GearSubSlotType.Item)
                    {
                        foreach (var item in _trinkets)
                        {
                            var effectiveSlot =
                                ActiveSlot is GearTemplateSlot.Ring_2 ? GearTemplateSlot.Ring_1 :
                                ActiveSlot is GearTemplateSlot.Accessory_2 ? GearTemplateSlot.Accessory_1 :
                                ActiveSlot;

                            item.Visible =
                                MatchingMethod(item.Item) &&
                                 item.Item?.TemplateSlot == effectiveSlot;
                        }
                    }
                    else if (SubSlotType == GearSubSlotType.Infusion)
                    {
                        foreach (var item in _infusions)
                        {
                            item.Visible = MatchingMethod(item.Item);
                        }
                    }

                    break;

                case GearTemplateSlot.Nourishment:
                    foreach (var item in _nourishment)
                    {
                        item.Visible = MatchingMethod(item.Item);
                    }

                    break;

                case GearTemplateSlot.Utility:
                    foreach (var item in _utilites)
                    {
                        item.Visible = MatchingMethod(item.Item);
                    }

                    break;

                case GearTemplateSlot.JadeBotCore:

                    break;
            }

            SelectionContent.Invalidate();
        }

        public void ApplySlot()
        {
            foreach (var item in _selectables)
            {
                item.Visible = false;
            }

            PerformFiltering();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            Search?.SetSize(Width - Search.Left);
            _autoEquipItems?.SetSize(Width - _autoEquipItems.Left);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
        }
    }
}
