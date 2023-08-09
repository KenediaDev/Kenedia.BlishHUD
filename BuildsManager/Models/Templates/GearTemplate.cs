using System.ComponentModel;
using System.Collections.Generic;
using System;
using Kenedia.Modules.BuildsManager.Extensions;
using System.Linq;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using Kenedia.Modules.Core.Models;
using System.Data;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Extensions;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class GearTemplate : Dictionary<GearTemplateEntryType, DeepObservableDictionary<TemplateSlot, BaseTemplateEntry>>, INotifyPropertyChanged
    {
        private bool _triggerEvents = true;
        private bool _disposed = false;

        private bool _loading = false;
        private ProfessionType _profession = ProfessionType.Guardian;

        public GearTemplate()
        {
            foreach (GearTemplateEntryType type in Enum.GetValues(typeof(GearTemplateEntryType)))
            {
                Add(type, new());
            }

            foreach (TemplateSlot slot in Enum.GetValues(typeof(TemplateSlot)))
            {
                if (slot.IsWeapon())
                {
                    this[GearTemplateEntryType.Weapon].Add(slot, new WeaponEntry() { Slot = slot });
                }
                else if (slot.IsArmor())
                {
                    this[GearTemplateEntryType.Armor].Add(slot, new ArmorEntry() { Slot = slot });
                }
                else if (slot.IsJuwellery())
                {
                    this[GearTemplateEntryType.Equipment].Add(slot, new JuwelleryEntry() { Slot = slot });
                }
                else if (slot is TemplateSlot.PvpAmulet)
                {
                    this[GearTemplateEntryType.PvpAmulet].Add(slot, new ArmorEntry() { Slot = slot });
                }
                else if (slot is not TemplateSlot.None)
                {
                    this[GearTemplateEntryType.None].Add(slot, new BaseTemplateEntry() { Slot = slot });
                }
            }

            this[GearTemplateEntryType.Weapon].ItemChanged += ItemChanged;
            this[GearTemplateEntryType.Armor].ItemChanged += ItemChanged;
            this[GearTemplateEntryType.Equipment].ItemChanged += ItemChanged;
            this[GearTemplateEntryType.None].ItemChanged += ItemChanged;
        }

        private void ItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_loading) return;

            if (Weapons[TemplateSlot.MainHand].Weapon is WeaponType.Staff or WeaponType.Rifle or WeaponType.Hammer or WeaponType.Greatsword or WeaponType.LongBow or WeaponType.ShortBow)
            {
                Weapons[TemplateSlot.OffHand].Item = null;
                Weapons[TemplateSlot.OffHand].MappedId = -1;
            }

            if (Weapons[TemplateSlot.AltMainHand].Weapon is WeaponType.Staff or WeaponType.Rifle or WeaponType.Hammer or WeaponType.Greatsword or WeaponType.LongBow or WeaponType.ShortBow)
            {
                Weapons[TemplateSlot.AltOffHand].Item = null;
                Weapons[TemplateSlot.AltOffHand].MappedId = -1;
            }

            PropertyChanged?.Invoke(sender, e);
        }

        public GearTemplate(string code) : this()
        {
            LoadFromCode(code);
        }

        public event ValueChangedEventHandler<ProfessionType> ProfessionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public Dictionary<TemplateSlot, WeaponEntry> Weapons => this[GearTemplateEntryType.Weapon].ToDictionary(e => e.Key, e => (WeaponEntry)e.Value);

        public Dictionary<TemplateSlot, ArmorEntry> Armors => this[GearTemplateEntryType.Armor].ToDictionary(e => e.Key, e => (ArmorEntry)e.Value);

        public Dictionary<TemplateSlot, JuwelleryEntry> Juwellery => this[GearTemplateEntryType.Equipment].ToDictionary(e => e.Key, e => (JuwelleryEntry)e.Value);

        public Dictionary<TemplateSlot, BaseTemplateEntry> PvpAmulets => this[GearTemplateEntryType.PvpAmulet];

        public Dictionary<TemplateSlot, BaseTemplateEntry> Common => this[GearTemplateEntryType.None];

        public ProfessionType Profession { get => _profession; set => Core.Utility.Common.SetProperty(ref _profession, value, OnProfessionChanged); }

        public string ParseGearCode()
        {
            string code = "";

            foreach (var entry in Armors.Values.OrderBy(e => e.Slot))
            {
                code += entry.ToCode();
            }

            foreach (var entry in Weapons.Values.OrderBy(e => e.Slot))
            {
                code += entry.ToCode();
            }

            foreach (var entry in Juwellery.Values.OrderBy(e => e.Slot))
            {
                code += entry.ToCode();
            }

            foreach (var entry in PvpAmulets.Values.OrderBy(e => e.Slot))
            {
                code += entry.ToCode();
            }

            foreach (var entry in Common.Values.OrderBy(e => e.Slot))
            {
                code += entry.ToCode();
            }

            return code;
        }

        public void LoadFromCode(string code)
        {
            string[] parts = code.Split(']');

            if (parts.Length == 24)
            {
                _loading = true;

                for (int i = (int)TemplateSlot.Head; i <= (int)TemplateSlot.AquaBreather; i++)
                {
                    Armors[(TemplateSlot)i].FromCode(parts[i].Substring(1, parts[i].Length - 1));
                }

                for (int i = (int)TemplateSlot.MainHand; i <= (int)TemplateSlot.AltAquatic; i++)
                {
                    Weapons[(TemplateSlot)i].FromCode(parts[i].Substring(1, parts[i].Length - 1));
                }

                for (int i = (int)TemplateSlot.Back; i <= (int)TemplateSlot.Ring_2; i++)
                {
                    Juwellery[(TemplateSlot)i].FromCode(parts[i].Substring(1, parts[i].Length - 1));
                }

                PvpAmulets[TemplateSlot.PvpAmulet].FromCode(parts[(int)TemplateSlot.PvpAmulet].Substring(1, parts[(int)TemplateSlot.PvpAmulet].Length - 1));
                Common[TemplateSlot.Nourishment].FromCode(parts[(int)TemplateSlot.Nourishment].Substring(1, parts[(int)TemplateSlot.Nourishment].Length - 1));
                Common[TemplateSlot.Utility].FromCode(parts[(int)TemplateSlot.Utility].Substring(1, parts[(int)TemplateSlot.Utility].Length - 1));
                Common[TemplateSlot.JadeBotCore].FromCode(parts[(int)TemplateSlot.JadeBotCore].Substring(1, parts[(int)TemplateSlot.JadeBotCore].Length - 1));

                PropertyChanged?.Invoke(this, null);

                _loading = false;
            }
        }

        private void OnProfessionChanged(object sender, ValueChangedEventArgs<ProfessionType> e)
        {
            switch (Profession.GetArmorType())
            {
                case ItemWeightType.Heavy:
                    this[GearTemplateEntryType.Armor][TemplateSlot.AquaBreather].Item = BuildsManager.Data.Armors[79895];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Head].Item = BuildsManager.Data.Armors[85193];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Shoulder].Item = BuildsManager.Data.Armors[84875];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Chest].Item = BuildsManager.Data.Armors[85084];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Hand].Item = BuildsManager.Data.Armors[85140];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Leg].Item = BuildsManager.Data.Armors[84887];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Foot].Item = BuildsManager.Data.Armors[85055];
                    break;
                case ItemWeightType.Medium:
                    this[GearTemplateEntryType.Armor][TemplateSlot.AquaBreather].Item = BuildsManager.Data.Armors[79838];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Head].Item = BuildsManager.Data.Armors[80701];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Shoulder].Item = BuildsManager.Data.Armors[80825];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Chest].Item = BuildsManager.Data.Armors[84977];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Hand].Item = BuildsManager.Data.Armors[85169];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Leg].Item = BuildsManager.Data.Armors[85264];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Foot].Item = BuildsManager.Data.Armors[80836];
                    break;
                case ItemWeightType.Light:
                    this[GearTemplateEntryType.Armor][TemplateSlot.AquaBreather].Item = BuildsManager.Data.Armors[79873];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Head].Item = BuildsManager.Data.Armors[85128];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Shoulder].Item = BuildsManager.Data.Armors[84918];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Chest].Item = BuildsManager.Data.Armors[85333];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Hand].Item = BuildsManager.Data.Armors[85070];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Leg].Item = BuildsManager.Data.Armors[85362];
                    this[GearTemplateEntryType.Armor][TemplateSlot.Foot].Item = BuildsManager.Data.Armors[80815];
                    break;
            }

            this[GearTemplateEntryType.Equipment][TemplateSlot.Back].Item = BuildsManager.Data.Backs[94947];
            this[GearTemplateEntryType.Equipment][TemplateSlot.Amulet].Item = BuildsManager.Data.Trinkets[79980];
            this[GearTemplateEntryType.Equipment][TemplateSlot.Accessory_1].Item = BuildsManager.Data.Trinkets[80002];
            this[GearTemplateEntryType.Equipment][TemplateSlot.Accessory_2].Item = BuildsManager.Data.Trinkets[80002];
            this[GearTemplateEntryType.Equipment][TemplateSlot.Ring_1].Item = BuildsManager.Data.Trinkets[80058];
            this[GearTemplateEntryType.Equipment][TemplateSlot.Ring_2].Item = BuildsManager.Data.Trinkets[80058];

            ProfessionChanged?.Invoke(this, e);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

        }

        public async void PauseEvents(int ms = 500)
        {
            _triggerEvents = false;
            await Task.Delay(ms);
            _triggerEvents = true;
        }
    }
}
