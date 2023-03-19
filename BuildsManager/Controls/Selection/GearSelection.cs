using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
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

        public GearTemplateEntry TemplateSlot { get; set; }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        private void ApplyTemplate()
        {

        }

        private void OnItemSelectedX(BaseItem item)
        {
            switch (SubSlotType)
            {
                case GearSubSlotType.Item:
                    if (TemplateSlot != null) TemplateSlot.Item = item;
                    break;

                case GearSubSlotType.Rune:
                    if (TemplateSlot != null) (TemplateSlot as ArmorEntry).Rune = (item as Rune);
                    break;

                case GearSubSlotType.Sigil:
                    if (ActiveSlot is not GearTemplateSlot.Aquatic and not GearTemplateSlot.AltAquatic)
                    {
                        if (Template?.PvE == false)
                        {
                            if (TemplateSlot != null) (TemplateSlot as WeaponEntry).PvpSigil = (item as Sigil);
                        }
                        else
                        {
                            if (TemplateSlot != null) (TemplateSlot as WeaponEntry).Sigil = (item as Sigil);
                        }
                    }
                    else
                    {

                    }

                    break;

                case GearSubSlotType.Infusion:

                    break;

                case GearSubSlotType.Enrichment:
                    if (TemplateSlot != null) (TemplateSlot as JuwelleryEntry).Enrichment = item as Enrichment;
                    break;
            }
        }

        private bool MatchingMethod(BaseItem item)
        {
            return item.Name == null || string.IsNullOrEmpty(_filterText) || item.Name.ToLower().Contains(_filterText);
        }

        public GearTemplateSlot ActiveSlot { get => _activeSlot; set => Common.SetProperty(ref _activeSlot, value, ApplySlot); }

        public GearSubSlotType SubSlotType { get => _subSlotType; set => Common.SetProperty(ref _subSlotType, value, ApplySlot); }

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
                                bool wieldMatch = slotIsOffhand ? weapon.Value.Wielded.HasFlag(Gw2Sharp.ProfessionWeaponFlag.Offhand) : weapon.Value.Wielded.HasFlag(Gw2Sharp.ProfessionWeaponFlag.Mainhand) || weapon.Value.Wielded.HasFlag(Gw2Sharp.ProfessionWeaponFlag.TwoHand);

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
                                    weaponMatch &&
                                    MatchingMethod(item.Item) &&
                                    item.Item?.TemplateSlot == effectiveSlot;
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

        protected override void DisposeControl()
        {
            base.DisposeControl();
        }
    }
}
