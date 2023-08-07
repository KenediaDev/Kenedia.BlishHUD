using System.ComponentModel;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Extensions;
using System.Linq;
using System.Data;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class BaseTemplateEntry : INotifyPropertyChanged
    {
        private GearTemplateSlot _slot = (GearTemplateSlot)(-2);
        private int _mappedId = -1;
        private BaseItem _item;

        public GearTemplateEntryType Type { get; set; }

        public GearTemplateSlot Slot { get => _slot; set => Common.SetProperty(ref _slot, value, OnSlotApply); }

        public BaseItem Item { get => _item; set => Common.SetProperty(ref _item, value, OnItemChanged); }

        public int MappedId
        {
            get => _mappedId; set
            {
                if (Common.SetProperty(ref _mappedId, value, OnPropertyChanged))
                {
                    Item = BuildsManager.Data.TryGetItemsFor<BaseItem>(_slot, out var items) ? items.Values.Where(e => e.MappedId == _mappedId).FirstOrDefault() : null;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnSlotApply()
        {
            Type =
                _slot.IsArmor() ? GearTemplateEntryType.Armor :
                _slot.IsWeapon() ? GearTemplateEntryType.Weapon :
                _slot.IsJuwellery() ? GearTemplateEntryType.Equipment :
                GearTemplateEntryType.None;
        }

        public virtual void Reset()
        {
            //ResetItem();
        }

        public void ResetItem()
        {
            Item = null;
        }

        public virtual string ToCode()
        {
            return $"[{MappedId}]";
        }

        public virtual void OnItemChanged()
        {
            _mappedId = Item?.MappedId ?? -1;

            OnPropertyChanged(this, new(nameof(Item)));
        }

        public virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        public virtual void FromCode(string code)
        {
            MappedId = int.TryParse(code, out int mappedId) ? mappedId : -1;
        }
    }
}
