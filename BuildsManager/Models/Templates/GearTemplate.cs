using Blish_HUD.Gw2Mumble;
using Blish_HUD;
using Gw2Sharp.Models;
using System.ComponentModel;
using Kenedia.Modules.Core.Utility;
using System.Diagnostics;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using System.Collections.Generic;
using System;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using System.Linq;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using Kenedia.Modules.Core.Models;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class ObservableListInt : List<int>, INotifyPropertyChanged
    {
        public new int this[int i]
        {
            get => base[i];
            set
            {
                if (base[i] == value) return;
                base[i] = value;
                OnPropertyChanged(this, new($"{i}"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }
    }

    public class GearTemplateEntry : INotifyPropertyChanged
    {
        private GearTemplateSlot _slot = (GearTemplateSlot)(-2);
        private int _mappedId = -1;
        private BaseItem _item;

        public GearTemplateEntryType Type { get; set; }

        public GearTemplateSlot Slot { get => _slot; set => Common.SetProperty(ref _slot, value, OnSlotApply); }

        public BaseItem Item { get => _item; set => Common.SetProperty(ref _item, value, OnItemChanged); }

        public int MappedId { get => _mappedId; set => Common.SetProperty(ref _mappedId, value, OnPropertyChanged); }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnSlotApply()
        {
            Type =
                _slot.IsArmor() ? GearTemplateEntryType.Armor :
                _slot.IsWeapon() ? GearTemplateEntryType.Weapon :
                _slot.IsJuwellery() ? GearTemplateEntryType.Equipment :
                GearTemplateEntryType.None;
        }

        public virtual string ToCode()
        {
            return $"[{MappedId}]";
        }

        public virtual void OnItemChanged()
        {
            _mappedId = Item?.MappedId ?? 0;

            OnPropertyChanged(this, new(nameof(Item)));
        }

        public virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        public virtual void FromCode(string code)
        {

        }
    }

    public class EquipmentEntry : GearTemplateEntry
    {
        public EquipmentStat Stat { get; set; }

        public ObservableListInt InfusionIds { get; set; } = new();

        public override void OnSlotApply()
        {
            base.OnSlotApply();

            InfusionIds = Slot switch
            {
                GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2 => new() { -1, -1, -1, },
                GearTemplateSlot.Back or GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic => new() { -1, -1 },
                _ => new() { -1 },
            };
        }

        public override string ToCode()
        {
            string infusions = string.Join("|", InfusionIds);
            return $"[{MappedId}|{(int)Stat}|{infusions}]";
        }

        public override void FromCode(string code)
        {
            string[] parts = code.Split('|');

            Debug.WriteLine($"Slot: {Slot}");
            Debug.WriteLine($"Code: {code}");
            Debug.WriteLine($"Parts: {parts.Length}");

            if (Slot is GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2)
            {
                if (parts.Length != 5) return;
            }
            else if (Slot is GearTemplateSlot.Back)
            {
                if (parts.Length != 4) return;
            }
            else
            {
                if (parts.Length != 3) return;
            }

            MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
            Stat = (EquipmentStat)(int.TryParse(parts[1], out int stat) ? stat : -1);

            if (Slot == GearTemplateSlot.Back)
            {
                InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
                InfusionIds[1] = int.TryParse(parts[3], out int infusion2) ? infusion2 : -1;
            }
            else if (Slot is GearTemplateSlot.Ring_1 or GearTemplateSlot.Ring_2)
            {
                InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
                InfusionIds[1] = int.TryParse(parts[3], out int infusion2) ? infusion2 : -1;
                InfusionIds[2] = int.TryParse(parts[4], out int infusion3) ? infusion3 : -1;
            }
            else
            {
                InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
            }

            if (MappedId > -1)
            {
                Item = Slot == GearTemplateSlot.Back ?
                    BuildsManager.Data.Backs.Values.Where(e => e.MappedId == mappedId).FirstOrDefault() :
                    BuildsManager.Data.Trinkets.Values.Where(e => e.MappedId == mappedId).FirstOrDefault();
            }
        }
    }

    public class WeaponEntry : EquipmentEntry
    {
        private WeaponType _weapon = WeaponType.Unknown;

        public ObservableListInt SigilIds { get; set; } = new();

        public WeaponType Weapon { get => _weapon; set => Common.SetProperty(ref _weapon, value, OnPropertyChanged); }

        public override void OnSlotApply()
        {
            base.OnSlotApply();

            SigilIds = Slot switch
            {
                GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic => new() { -1, -1 },
                _ => new() { -1, -1, },
            };
        }

        public override string ToCode()
        {
            string infusions = string.Join("|", InfusionIds);
            string sigils = string.Join("|", SigilIds);

            return $"[{MappedId}|{(int)Stat}|{infusions}|{sigils}]";
        }

        public override void FromCode(string code)
        {
            string[] parts = code.Split('|');

            if (Slot is GearTemplateSlot.Aquatic or GearTemplateSlot.AltAquatic)
            {
                if (parts.Length == 6)
                {
                    MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
                    Stat = (EquipmentStat)(int.TryParse(parts[1], out int stat) ? stat : -1);
                    InfusionIds[0] = int.TryParse(parts[2], out int infusion1) ? infusion1 : -1;
                    InfusionIds[1] = int.TryParse(parts[3], out int infusion2) ? infusion2 : -1;
                    SigilIds[0] = int.TryParse(parts[4], out int sigil1) ? sigil1 : -1;
                    SigilIds[1] = int.TryParse(parts[5], out int sigil2) ? sigil2 : -1;
                }
            }
            else if (parts.Length == 5)
            {
                MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
                Stat = (EquipmentStat)(int.TryParse(parts[1], out int stat) ? stat : -1);
                InfusionIds[0] = int.TryParse(parts[2], out int infusionId) ? infusionId : -1;
                SigilIds[0] = int.TryParse(parts[3], out int sigil) ? sigil : -1;
                SigilIds[1] = int.TryParse(parts[4], out int pvpsigil) ? pvpsigil : -1;
            }

            if (MappedId > -1)
            {
                Item = BuildsManager.Data.Weapons.Values.Where(e => e.MappedId == MappedId).FirstOrDefault();
            }
        }
    }

    public class ArmorEntry : EquipmentEntry
    {
        public ObservableListInt RuneIds { get; set; } = new() { 0 };

        public override void OnSlotApply()
        {
            base.OnSlotApply();

        }

        public override string ToCode()
        {
            string infusions = string.Join("|", InfusionIds);
            string runes = string.Join("|", RuneIds);

            return $"[{MappedId}|{(int)Stat}|{infusions}|{runes}]";
        }

        public override void FromCode(string code)
        {
            string[] parts = code.Split('|');

            if (parts.Length == 4)
            {
                MappedId = int.TryParse(parts[0], out int mappedId) ? mappedId : -1;
                Stat = (EquipmentStat)(int.TryParse(parts[1], out int stat) ? stat : 0);
                InfusionIds[0] = int.TryParse(parts[2], out int infusionId) ? infusionId : 0;
                RuneIds[0] = int.TryParse(parts[3], out int runeId) ? runeId : 0;

                if (MappedId > -1)
                {
                    Item = BuildsManager.Data.Armors.Values.Where(e => e.MappedId == mappedId).FirstOrDefault();
                }
            }
        }
    }

    public enum GearTemplateEntryType
    {
        None,
        Equipment,
        Weapon,
        Armor,
        PvpAmulet,
    }

    public class GearTemplate : Dictionary<GearTemplateEntryType, DeepObservableDictionary<GearTemplateSlot, GearTemplateEntry>>, INotifyPropertyChanged
    {
        public GearTemplate()
        {
            foreach (GearTemplateEntryType type in Enum.GetValues(typeof(GearTemplateEntryType)))
            {
                Add(type, new());
            }

            foreach (GearTemplateSlot slot in Enum.GetValues(typeof(GearTemplateSlot)))
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
                    this[GearTemplateEntryType.Equipment].Add(slot, new EquipmentEntry() { Slot = slot });
                }
                else if (slot is GearTemplateSlot.PvpAmulet)
                {
                    this[GearTemplateEntryType.PvpAmulet].Add(slot, new ArmorEntry() { Slot = slot });
                }
                else if (slot is not GearTemplateSlot.None)
                {
                    this[GearTemplateEntryType.None].Add(slot, new GearTemplateEntry() { Slot = slot });
                }
            }

            this[GearTemplateEntryType.Weapon].ItemChanged += ItemChanged;
            this[GearTemplateEntryType.Armor].ItemChanged += ItemChanged;
            this[GearTemplateEntryType.Equipment].ItemChanged += ItemChanged;
            this[GearTemplateEntryType.None].ItemChanged += ItemChanged;
        }

        private void ItemChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        public GearTemplate(string code) : this()
        {
            LoadFromCode(code);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Dictionary<GearTemplateSlot, WeaponEntry> Weapons => this[GearTemplateEntryType.Weapon].ToDictionary(e => e.Key, e => (WeaponEntry)e.Value);

        public Dictionary<GearTemplateSlot, ArmorEntry> Armors => this[GearTemplateEntryType.Armor].ToDictionary(e => e.Key, e => (ArmorEntry)e.Value);

        public Dictionary<GearTemplateSlot, EquipmentEntry> Juwellery => this[GearTemplateEntryType.Equipment].ToDictionary(e => e.Key, e => (EquipmentEntry)e.Value);

        public Dictionary<GearTemplateSlot, GearTemplateEntry> PvpAmulet => this[GearTemplateEntryType.PvpAmulet];

        public Dictionary<GearTemplateSlot, GearTemplateEntry> Common => this[GearTemplateEntryType.None];

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

            foreach (var entry in Common.Values.OrderBy(e => e.Slot))
            {
                code += entry.ToCode();
            }

            return code;
        }

        public void LoadFromCode(string code)
        {
            string[] parts = code.Split(']');

            Debug.WriteLine($"Armors {Armors.Count}");
            Debug.WriteLine($"Weapons {Weapons.Count}");
            Debug.WriteLine($"Juwellery {Juwellery.Count}");
            Debug.WriteLine($"Common {Common.Count}");

            if (parts.Length == 23)
            {
                for (int i = (int)GearTemplateSlot.Head; i <= (int)GearTemplateSlot.AquaBreather; i++)
                {
                    Armors[(GearTemplateSlot)i].FromCode(parts[i].Substring(1, parts[i].Length - 1));
                }

                for (int i = (int)GearTemplateSlot.MainHand; i <= (int)GearTemplateSlot.AltAquatic; i++)
                {
                    Weapons[(GearTemplateSlot)i].FromCode(parts[i].Substring(1, parts[i].Length - 1));
                }

                for (int i = (int)GearTemplateSlot.Back; i <= (int)GearTemplateSlot.Ring_2; i++)
                {
                    Juwellery[(GearTemplateSlot)i].FromCode(parts[i].Substring(1, parts[i].Length - 1));
                }

                Common[GearTemplateSlot.Nourishment].FromCode(parts[(int)GearTemplateSlot.Nourishment].Substring(1, parts[(int)GearTemplateSlot.Nourishment].Length - 1));
                Common[GearTemplateSlot.Utility].FromCode(parts[(int)GearTemplateSlot.Utility].Substring(1, parts[(int)GearTemplateSlot.Utility].Length - 1));
                //Common[GearTemplateSlot.JadeBotCore].FromCode(parts[(int)GearTemplateSlot.JadeBotCore].Substring(1, parts[(int)GearTemplateSlot.JadeBotCore].Length - 1));
                //PvpAmulets[GearTemplateSlot.PvpAmulet].FromCode(parts[(int)GearTemplateSlot.PvpAmulet].Substring(1, parts[(int)GearTemplateSlot.PvpAmulet].Length - 1));
            }
        }
    }
}
