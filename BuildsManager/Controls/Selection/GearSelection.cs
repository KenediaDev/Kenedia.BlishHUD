using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Kenedia.Modules.BuildsManager.Controls.Selection.Selectable;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class GearSelection : BaseSelection
    {
        private readonly List<Selectable> _selectables = new();
        private TemplateSlotType _activeSlot;
        private GearSubSlotType _subSlotType;
        private string _filterText;
        private readonly List<Selectable> _armors;
        private readonly List<Selectable> _trinkets;
        private readonly List<Selectable> _backs;
        private readonly List<Selectable> _weapons;
        private readonly List<Selectable> _pvpAmulets;
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
                            if (TemplatePresenter?.Template != null)
                            {
                                OnClickAction(item);
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
            _weapons = AddItems<Selectable, Weapon>(BuildsManager.Data.Weapons.Values.OrderBy(e => e.WeaponType).ThenByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _pvpAmulets = AddItems<Selectable, PvpAmulet>(BuildsManager.Data.PvpAmulets.Values.OrderBy(e => e.Name).ThenBy(e => e.Id));
            _pveSigils = AddItems<Selectable, Sigil>(BuildsManager.Data.PveSigils.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _pvpSigils = AddItems<Selectable, Sigil>(BuildsManager.Data.PvpSigils.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _pveRunes = AddItems<Selectable, Rune>(BuildsManager.Data.PveRunes.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _pvpRunes = AddItems<Selectable, Rune>(BuildsManager.Data.PvpRunes.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _nourishment = AddItems<Selectable, Nourishment>(BuildsManager.Data.Nourishments.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _utilites = AddItems<Selectable, DataModels.Items.Utility>(BuildsManager.Data.Utilities.Values.OrderByDescending(e => e.Name).ThenBy(e => e.Id));
            _enrichments = AddItems<Selectable, Enrichment>(BuildsManager.Data.Enrichments.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _infusions = AddItems<Selectable, Infusion>(BuildsManager.Data.Infusions.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name.Length).ThenBy(e => e.Name).ThenBy(e => e.Id));

            Search.TextChangedAction = (txt) =>
            {
                _filterText = txt.Trim().ToLower();
                PerformFiltering();
            };

            SelectionContent.SetLocation(Search.Left, Search.Bottom + 5);
        }

        public TemplatePresenter TemplatePresenter { get; set; } = new();

        public TemplateSlotType ActiveSlot { get => _activeSlot; set => Common.SetProperty(ref _activeSlot, value, ApplySlot); }

        public GearSubSlotType SubSlotType { get => _subSlotType; set => Common.SetProperty(ref _subSlotType, value, ApplySubSlot); }

        private void Template_ProfessionChanged(object sender, PropertyChangedEventArgs e)
        {
            string tempTxt = Search.Text;

            ApplySubSlot(sender, e);

            Search.Text = tempTxt;
        }

        private bool MatchingMethod(BaseItem item)
        {
            return item.Name == null || string.IsNullOrEmpty(_filterText) || item.Name.ToLower().Contains(_filterText);
        }

        private void ApplySubSlot(object sender, PropertyChangedEventArgs e)
        {
            Search.Text = null;
            foreach (var item in _selectables)
            {
                item.SubSlotType = SubSlotType;
            }

            ApplySlot();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
        }

        public void PerformFiltering()
        {
            switch (ActiveSlot)
            {
                case TemplateSlotType.Head:
                case TemplateSlotType.Shoulder:
                case TemplateSlotType.Chest:
                case TemplateSlotType.Hand:
                case TemplateSlotType.Leg:
                case TemplateSlotType.Foot:
                case TemplateSlotType.AquaBreather:
                    if (SubSlotType == GearSubSlotType.Item)
                    {
                        foreach (var item in _armors)
                        {
                            item.Visible =
                                MatchingMethod(item.Item) &&
                                item.Item?.TemplateSlot == ActiveSlot &&
                                TemplatePresenter?.Template?.Profession.GetArmorType() == (item.Item as Armor).Weight;
                        }

                    }
                    else if (SubSlotType == GearSubSlotType.Rune)
                    {
                        foreach (var item in _pveRunes)
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

                case TemplateSlotType.MainHand:
                case TemplateSlotType.AltMainHand:
                case TemplateSlotType.OffHand:
                case TemplateSlotType.AltOffHand:
                case TemplateSlotType.Aquatic:
                case TemplateSlotType.AltAquatic:
                    bool slotIsOffhand = ActiveSlot is TemplateSlotType.OffHand or TemplateSlotType.AltOffHand;

                    if (SubSlotType == GearSubSlotType.Item)
                    {
                        foreach (var item in _weapons)
                        {
                            bool weaponMatch = true;

                            var weapon = BuildsManager.Data.Professions[TemplatePresenter?.Template.Profession ?? Gw2Sharp.Models.ProfessionType.Guardian].Weapons.Where(e => (item.Item as Weapon).WeaponType.IsWeaponType(e.Value.Type)).FirstOrDefault();

                            if (weapon.Value != null)
                            {
                                bool terrainMatch =
                                    (ActiveSlot is TemplateSlotType.AltAquatic or TemplateSlotType.Aquatic) ?
                                    weapon.Value.Type.IsAquatic() :
                                    !weapon.Value.Type.IsAquatic();
                                bool wieldMatch = slotIsOffhand ? weapon.Value.Wielded.HasFlag(WieldingFlag.Offhand) : weapon.Value.Wielded.HasFlag(WieldingFlag.Mainhand) || weapon.Value.Wielded.HasFlag(WieldingFlag.TwoHand);

                                // No Elite Spec
                                if (weapon.Value.Specialization == 0 && wieldMatch)
                                {
                                    weaponMatch = true;
                                }

                                //Elite Spec Matched
                                if (weapon.Value.Specialization == TemplatePresenter?.Template.EliteSpecialization?.Id && wieldMatch)
                                {
                                    weaponMatch = true;
                                }

                                var effectiveSlot =
                                    ActiveSlot is TemplateSlotType.AltAquatic ? TemplateSlotType.Aquatic :
                                    ActiveSlot is TemplateSlotType.AltMainHand ? TemplateSlotType.MainHand :
                                    ActiveSlot is TemplateSlotType.AltOffHand ? TemplateSlotType.OffHand :
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
                        foreach (var item in TemplatePresenter?.IsPve == false ? _pvpSigils : _pveSigils)
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

                case TemplateSlotType.Amulet:
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

                case TemplateSlotType.Back:
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

                case TemplateSlotType.Ring_1:
                case TemplateSlotType.Ring_2:
                case TemplateSlotType.Accessory_1:
                case TemplateSlotType.Accessory_2:
                    if (SubSlotType == GearSubSlotType.Item)
                    {
                        foreach (var item in _trinkets)
                        {
                            var effectiveSlot =
                                ActiveSlot is TemplateSlotType.Ring_2 ? TemplateSlotType.Ring_1 :
                                ActiveSlot is TemplateSlotType.Accessory_2 ? TemplateSlotType.Accessory_1 :
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

                case TemplateSlotType.Nourishment:
                    foreach (var item in _nourishment)
                    {
                        item.Visible = MatchingMethod(item.Item);
                    }

                    break;

                case TemplateSlotType.Utility:
                    foreach (var item in _utilites)
                    {
                        item.Visible = MatchingMethod(item.Item);
                    }

                    break;

                case TemplateSlotType.JadeBotCore:
                    break;

                case TemplateSlotType.PvpAmulet:
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
    }
}
