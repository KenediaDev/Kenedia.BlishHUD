using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Kenedia.Modules.BuildsManager.Controls.Selection.Selectable;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class GearSelection : BaseSelection
    {
        private readonly Dictionary<TemplateSlot, List<Selectable>> _selectablesPerSlot = new();
        private readonly List<Selectable> _selectables = new();
        private TemplateSlot _activeSlot;
        private GearSubSlotType _subSlotType;
        private Template _template;
        private string _filterText;
        private readonly List<Selectable> _armors;
        private readonly List<Selectable> _trinkets;
        private readonly List<Selectable> _backs;
        private readonly List<Selectable> _weapons;
        private readonly List<Selectable> _pvpAmulets;
        private readonly List<GroupSelectable> _pveSigils;
        private readonly List<GroupSelectable> _pvpSigils;
        private readonly List<GroupSelectable> _pveRunes;
        private readonly List<GroupSelectable> _pvpRunes;
        private readonly List<Selectable> _nourishment;
        private readonly List<Selectable> _utilites;
        private readonly List<Selectable> _enrichments;
        private readonly List<GroupSelectable> _infusions;

        public GearSelection()
        {
            List<S> AddItems<S, T>(IOrderedEnumerable<T> items)
                where T : BaseItem
                where S : Selectable, new()
            {
                List<S> list = new();
                S entry;

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
                        },
                        Type =
                            item is Rune ? SelectableType.Rune :
                            item is Sigil ? SelectableType.Sigil :
                            item is Infusion ? SelectableType.Infusion :
                            SelectableType.None,
                    });
                    list.Add(entry);
                }

                return list;
            }

            _armors = AddItems<Selectable, Armor>(BuildsManager.Data.Armors.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _trinkets = AddItems<Selectable, Trinket>(BuildsManager.Data.Trinkets.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _backs = AddItems<Selectable, Trinket>(BuildsManager.Data.Backs.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _weapons = AddItems<Selectable, DataModels.Items.Weapon>(BuildsManager.Data.Weapons.Values.OrderBy(e => e.WeaponType).ThenByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _pvpAmulets = AddItems<Selectable, PvpAmulet>(BuildsManager.Data.PvpAmulets.Values.OrderBy(e => e.Name).ThenBy(e => e.Id));
            _pveSigils = AddItems<GroupSelectable, Sigil>(BuildsManager.Data.PveSigils.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _pvpSigils = AddItems<GroupSelectable, Sigil>(BuildsManager.Data.PvpSigils.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _pveRunes = AddItems<GroupSelectable, Rune>(BuildsManager.Data.PveRunes.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _pvpRunes = AddItems<GroupSelectable, Rune>(BuildsManager.Data.PvpRunes.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _nourishment = AddItems<Selectable, Nourishment>(BuildsManager.Data.Nourishments.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _utilites = AddItems<Selectable, DataModels.Items.Utility>(BuildsManager.Data.Utilities.Values.OrderByDescending(e => e.Name).ThenBy(e => e.Id));
            _enrichments = AddItems<Selectable, Enrichment>(BuildsManager.Data.Enrichments.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _infusions = AddItems<GroupSelectable, Infusion>(BuildsManager.Data.Infusions.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));

            Search.TextChangedAction = (txt) =>
            {
                _filterText = txt.Trim().ToLower();
                PerformFiltering();
            };

            SelectionContent.SetLocation(Search.Left, Search.Bottom + 5);
        }

        public TemplatePresenter TemplatePresenter { get; set; } = new();

        private async void EquipItems()
        {
            var armors = _armors.Where(e => (e.Item as Armor).Weight == Template?.Profession.GetArmorType() && (e.Item.Rarity == Gw2Sharp.WebApi.V2.Models.ItemRarity.Ascended || e.TemplateSlot == Models.Templates.TemplateSlot.AquaBreather))?.Select(e => e.Item);
            foreach (var armor in Template?.GearTemplate?.Armors.Values)
            {
                armor.Item = armors.Where(e => e.TemplateSlot == armor.Slot)?.FirstOrDefault();
            }

            var trinkets = _trinkets.Where(e => e.Item.Rarity == Gw2Sharp.WebApi.V2.Models.ItemRarity.Ascended)?.Select(e => e.Item);
            trinkets = trinkets.Append(_backs.Where(e => e.Item.Rarity == Gw2Sharp.WebApi.V2.Models.ItemRarity.Ascended)?.Select(e => e.Item).FirstOrDefault());
            foreach (var trinket in Template?.GearTemplate?.Juwellery.Values)
            {
                var effectiveSlot =
                    trinket.Slot is Models.Templates.TemplateSlot.Ring_2 ? Models.Templates.TemplateSlot.Ring_1 :
                    trinket.Slot is Models.Templates.TemplateSlot.Accessory_2 ? Models.Templates.TemplateSlot.Accessory_1 :
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
                    foreach (var slot in _selectables)
                    {
                        slot.Template = value;
                    }

                }
            }
        }

        private void Template_ProfessionChanged(object sender, PropertyChangedEventArgs e)
        {
            string tempTxt = Search.Text;

            ApplySubSlot(sender, e);

            Search.Text = tempTxt;
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

        public TemplateSlot ActiveSlot { get => _activeSlot; set => Common.SetProperty(ref _activeSlot, value, ApplySlot); }

        public GearSubSlotType SubSlotType { get => _subSlotType; set => Common.SetProperty(ref _subSlotType, value, ApplySubSlot); }

        private void ApplySubSlot(object sender, PropertyChangedEventArgs e)
        {
            Search.Text = null;
            foreach (var item in _selectables)
            {
                item.SubSlotType = SubSlotType;
            }

            ApplySlot();
        }

        public Action<BaseItem> OnItemSelected { get; set; }

        public void PerformFiltering()
        {
            switch (ActiveSlot)
            {
                case Models.Templates.TemplateSlot.Head:
                case Models.Templates.TemplateSlot.Shoulder:
                case Models.Templates.TemplateSlot.Chest:
                case Models.Templates.TemplateSlot.Hand:
                case Models.Templates.TemplateSlot.Leg:
                case Models.Templates.TemplateSlot.Foot:
                case Models.Templates.TemplateSlot.AquaBreather:
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

                case Models.Templates.TemplateSlot.MainHand:
                case Models.Templates.TemplateSlot.AltMainHand:
                case Models.Templates.TemplateSlot.OffHand:
                case Models.Templates.TemplateSlot.AltOffHand:
                case Models.Templates.TemplateSlot.Aquatic:
                case Models.Templates.TemplateSlot.AltAquatic:
                    bool slotIsOffhand = ActiveSlot is Models.Templates.TemplateSlot.OffHand or Models.Templates.TemplateSlot.AltOffHand;

                    if (SubSlotType == GearSubSlotType.Item)
                    {
                        foreach (var item in _weapons)
                        {
                            bool weaponMatch = false;

                            var weapon = BuildsManager.Data.Professions[Template.Profession].Weapons.Where(e => (item.Item as DataModels.Items.Weapon).WeaponType.IsWeaponType(e.Value.Type)).FirstOrDefault();

                            if (weapon.Value != null)
                            {
                                bool terrainMatch =
                                    (ActiveSlot is Models.Templates.TemplateSlot.AltAquatic or Models.Templates.TemplateSlot.Aquatic) ?
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
                                    ActiveSlot is Models.Templates.TemplateSlot.AltAquatic ? Models.Templates.TemplateSlot.Aquatic :
                                    ActiveSlot is Models.Templates.TemplateSlot.AltMainHand ? Models.Templates.TemplateSlot.MainHand :
                                    ActiveSlot is Models.Templates.TemplateSlot.AltOffHand ? Models.Templates.TemplateSlot.OffHand :
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

                case Models.Templates.TemplateSlot.Amulet:
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

                case Models.Templates.TemplateSlot.Back:
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

                case Models.Templates.TemplateSlot.Ring_1:
                case Models.Templates.TemplateSlot.Ring_2:
                case Models.Templates.TemplateSlot.Accessory_1:
                case Models.Templates.TemplateSlot.Accessory_2:
                    if (SubSlotType == GearSubSlotType.Item)
                    {
                        foreach (var item in _trinkets)
                        {
                            var effectiveSlot =
                                ActiveSlot is Models.Templates.TemplateSlot.Ring_2 ? Models.Templates.TemplateSlot.Ring_1 :
                                ActiveSlot is Models.Templates.TemplateSlot.Accessory_2 ? Models.Templates.TemplateSlot.Accessory_1 :
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

                case Models.Templates.TemplateSlot.Nourishment:
                    foreach (var item in _nourishment)
                    {
                        item.Visible = MatchingMethod(item.Item);
                    }

                    break;

                case Models.Templates.TemplateSlot.Utility:
                    foreach (var item in _utilites)
                    {
                        item.Visible = MatchingMethod(item.Item);
                    }

                    break;

                case Models.Templates.TemplateSlot.JadeBotCore:
                    break;

                case Models.Templates.TemplateSlot.PvpAmulet:
                    if (SubSlotType == GearSubSlotType.Item)
                    {
                        foreach (var item in _pvpAmulets)
                        {
                            item.Visible =
                                MatchingMethod(item.Item) &&
                                item.Item?.TemplateSlot == ActiveSlot;
                        }
                    }
                    else if (SubSlotType == GearSubSlotType.Rune)
                    {
                        foreach (var item in _pvpRunes)
                        {
                            item.Visible = MatchingMethod(item.Item);
                        }
                    }
                    break;
            }

            SelectionContent.Invalidate();
        }

        public void ApplySlot()
        {
            foreach (var item in _selectables)
            {
                item.Visible = false;
                item.ActiveSlot = ActiveSlot;
            }

            PerformFiltering();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            Search?.SetSize(Width - Search.Left);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
        }
    }
}
