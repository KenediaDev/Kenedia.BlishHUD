using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Kenedia.Modules.BuildsManager.Controls.Selection.SelectionPanelSelectable;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class GearSelection : BaseSelection
    {
        private List<SelectionPanelSelectable> _selectables = [];
        private string _filterText = string.Empty;
        private List<SelectionPanelSelectable> _armors = [];
        private List<SelectionPanelSelectable> _trinkets = [];
        private List<SelectionPanelSelectable> _backs = [];
        private List<SelectionPanelSelectable> _weapons = [];
        private List<SelectionPanelSelectable> _pvpAmulets = [];
        private List<SelectionPanelSelectable> _pveSigils = [];
        private List<SelectionPanelSelectable> _pvpSigils = [];
        private List<SelectionPanelSelectable> _pveRunes = [];
        private List<SelectionPanelSelectable> _pvpRunes = [];
        private List<SelectionPanelSelectable> _nourishment = [];
        private List<SelectionPanelSelectable> _utilites = [];
        private List<SelectionPanelSelectable> _enrichments = [];
        private List<SelectionPanelSelectable> _infusions = [];
        private List<SelectionPanelSelectable> _powerCores = [];
        private List<SelectionPanelSelectable> _pveRelics = [];
        private List<SelectionPanelSelectable> _pvpRelics = [];

        public GearSelection(TemplatePresenter templatePresenter, Data data)
        {
            TemplatePresenter = templatePresenter;
            Data = data;

            Search.TextChangedAction = (txt) =>
            {
                _filterText = txt.Trim().ToLower();
                PerformFiltering();
            };

            SelectionContent.ControlPadding = new(2);
            SelectionContent.FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom;
            SelectionContent.SetLocation(Search.Left, Search.Bottom + 5);

            Data.Loaded += Data_Loaded;
        }

        private void Data_Loaded(object sender, System.EventArgs e)
        {
            _armors.Clear();
            _trinkets.Clear();
            _backs.Clear();
            _weapons.Clear();
            _pvpAmulets.Clear();
            _pveSigils.Clear();
            _pvpSigils.Clear();
            _pveRunes.Clear();
            _pvpRunes.Clear();
            _nourishment.Clear();
            _utilites.Clear();
            _enrichments.Clear();
            _infusions.Clear();
            _powerCores.Clear();
            _pveRelics.Clear();
            _pvpRelics.Clear();

            _armors = AddItems<SelectionPanelSelectable, Armor>(Data.Armors.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _trinkets = AddItems<SelectionPanelSelectable, Trinket>(Data.Trinkets.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _backs = AddItems<SelectionPanelSelectable, Trinket>(Data.Backs.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _weapons = AddItems<SelectionPanelSelectable, Weapon>(Data.Weapons.Values.OrderBy(e => e.WeaponType).ThenByDescending(e => e.Rarity).ThenBy(e => e.Id));
            _pvpAmulets = AddItems<SelectionPanelSelectable, PvpAmulet>(Data.PvpAmulets.Values.OrderBy(e => e.Name).ThenBy(e => e.Id));
            _pveSigils = AddItems<SelectionPanelSelectable, Sigil>(Data.PveSigils.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _pvpSigils = AddItems<SelectionPanelSelectable, Sigil>(Data.PvpSigils.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _pveRunes = AddItems<SelectionPanelSelectable, Rune>(Data.PveRunes.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _pvpRunes = AddItems<SelectionPanelSelectable, Rune>(Data.PvpRunes.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _nourishment = AddItems<SelectionPanelSelectable, Nourishment>(Data.Nourishments.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _utilites = AddItems<SelectionPanelSelectable, Enhancement>(Data.Enhancements.Values.OrderBy(e => e.Name).ThenBy(e => e.Id));
            _enrichments = AddItems<SelectionPanelSelectable, Enrichment>(Data.Enrichments.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _infusions = AddItems<SelectionPanelSelectable, Infusion>(Data.Infusions.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name.Length).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _powerCores = AddItems<SelectionPanelSelectable, PowerCore>(Data.PowerCores.Values.OrderByDescending(e => e.Rarity).ThenByDescending(e => e.Name.Length).ThenByDescending(e => e.Name).ThenBy(e => e.Id));
            _pveRelics = AddItems<SelectionPanelSelectable, Relic>(Data.PveRelics.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
            _pvpRelics = AddItems<SelectionPanelSelectable, Relic>(Data.PvpRelics.Values.OrderByDescending(e => e.Rarity).ThenBy(e => e.Name).ThenBy(e => e.Id));
        }

        private List<S> AddItems<S, T>(IOrderedEnumerable<T> items)
                where T : BaseItem
                where S : SelectionPanelSelectable, new()
        {
            List<S> list = [];
            S entry;

            foreach (var item in items)
            {
                _selectables.Add(entry = new()
                {
                    Parent = SelectionContent,
                    Item = item,
                    Visible = false,
                    Width = 330,
                    OnClickAction = () =>
                    {
                        if (TemplatePresenter?.Template is not null)
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

        public TemplatePresenter TemplatePresenter { get; private set => Common.SetProperty(ref field, value, OnTemplatePresenterChanged); }
        public Data Data { get; }

        public TemplateSlotType ActiveSlot { get; set => Common.SetProperty(ref field, value, ApplySlot); }

        public GearSubSlotType SubSlotType { get; set => Common.SetProperty(ref field, value, ApplySubSlot); }

        private void OnTemplatePresenterChanged(object sender, ValueChangedEventArgs<TemplatePresenter> e)
        {
            if (e.OldValue is not null)
            {
                e.OldValue.TemplateChanged -= Template_TemplateChanged;
                e.OldValue.GameModeChanged -= Template_GameModeChanged;
            }

            if (e.NewValue is not null)
            {
                e.NewValue.TemplateChanged += Template_TemplateChanged;
                e.NewValue.GameModeChanged += Template_GameModeChanged;
            }
        }

        private void Template_GameModeChanged(object sender, ValueChangedEventArgs<GameModeType> e)
        {
            PerformFiltering();
        }

        private void Template_TemplateChanged(object sender, ValueChangedEventArgs<Template> e)
        {
            ApplySlot();
        }

        private void Template_ProfessionChanged(object sender, PropertyChangedEventArgs e)
        {
            string tempTxt = Search.Text;

            ApplySubSlot(sender, e);

            Search.Text = tempTxt;
        }

        private bool MatchingMethod(BaseItem item)
        {
            string filterText = _filterText ?? string.Empty;

            switch (item?.Type)
            {
                case Core.DataModels.ItemType.Consumable:
                    if (item is Enhancement enhancement)
                    {
                        return item.Name == null || string.IsNullOrEmpty(filterText) || item.Name.ToLower().Contains(filterText) || enhancement.Details?.Description?.ToLower()?.Contains(filterText) == true;
                    }
                    else if (item is Nourishment nourishment)
                    {
                        return item.Name == null || string.IsNullOrEmpty(filterText) || item.Name.ToLower().Contains(filterText) || nourishment.Details?.Description?.ToLower()?.Contains(filterText) == true;
                    }
                    break;

                case Core.DataModels.ItemType.UpgradeComponent:
                    if (item is Rune rune)
                    {
                        return item.Name == null || string.IsNullOrEmpty(filterText) || item.Name.ToLower().Contains(filterText) || rune.Bonus.ToLower()?.Contains(filterText) == true;
                    }
                    else if (item is Sigil sigil)
                    {
                        return item.Name == null || string.IsNullOrEmpty(filterText) || item.Name.ToLower().Contains(filterText) || sigil.Buff.ToLower()?.Contains(filterText) == true;
                    }
                    else if (item is Infusion infusion)
                    {
                        return item.Name == null || string.IsNullOrEmpty(filterText) || item.Name.ToLower().Contains(filterText) || infusion.Bonus.ToLower()?.Contains(filterText) == true;
                    }
                    break;

                case Core.DataModels.ItemType.PvpAmulet:
                    if (item is PvpAmulet amulet)
                    {
                        bool matched = true;
                        foreach (string s in filterText.Split(' '))
                        {
                            string searchTxt = s.Trim().ToLower();

                            if (!string.IsNullOrEmpty(searchTxt))
                            {
                                matched = matched && (item.Name.ToLower().Contains(searchTxt) || amulet.AttributesString?.ToLower()?.Contains(searchTxt) == true);
                            }
                        }

                        return item.Name == null || string.IsNullOrEmpty(filterText) || matched;
                    }

                    break;
            }

            return item?.Name == null || string.IsNullOrEmpty(filterText) || item.Name.ToLower().Contains(filterText);
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

            SelectionContent?.Children?.DisposeAll();
            _selectables?.DisposeAll();
        }

        public void PerformFiltering()
        {
            if (TemplatePresenter?.Template is null)
                return;

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
                            if (item.Item is not Weapon)
                            {
                                continue;
                            }

                            var weapon = Data.Professions[TemplatePresenter?.Template.Profession ?? Gw2Sharp.Models.ProfessionType.Guardian].Weapons.Where(e => (item.Item as Weapon).WeaponType.IsWeaponType(e.Value.Type)).FirstOrDefault();

                            if (weapon.Value is not null)
                            {
                                bool terrainMatch =
                                    (ActiveSlot is TemplateSlotType.AltAquatic or TemplateSlotType.Aquatic) ?
                                    weapon.Value.Type.IsAquatic(ActiveSlot) :
                                    !weapon.Value.Type.IsAquatic(ActiveSlot);
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
                        foreach (var item in TemplatePresenter?.IsPve == false ? _pveSigils : _pvpSigils)
                        {
                            item.Visible = false;
                        }

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

                case TemplateSlotType.Enhancement:
                    foreach (var item in _utilites)
                    {
                        item.Visible = MatchingMethod(item.Item);
                    }

                    break;

                case TemplateSlotType.PveRelic:
                case TemplateSlotType.PvpRelic:
                    foreach (var item in TemplatePresenter?.IsPve == false ? _pvpRelics : _pveRelics)
                    {
                        item.Visible = MatchingMethod(item.Item);
                    }

                    break;

                case TemplateSlotType.PowerCore:
                    foreach (var item in _powerCores)
                    {
                        item.Visible = MatchingMethod(item.Item);
                    }

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

            SelectionContent.SortChildren<SelectionPanelSelectable>((a, b) => a.Item.Name.CompareTo(b.Item.Name));
            SelectionContent.Invalidate();
        }

        public void ApplySlot()
        {
            foreach (var item in _selectables)
            {
                item.Visible = false;
                item.ActiveSlot = ActiveSlot;
                item.Width = 330;
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
