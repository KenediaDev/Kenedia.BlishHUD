using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System.Linq;
using System;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class UtilityTemplateEntry : TemplateEntry
    {
        private DataModels.Items.Utility _utility;

        public UtilityTemplateEntry(TemplateSlotType slot) : base(slot)
        {
        }

        public event EventHandler<ValueChangedEventArgs<DataModels.Items.Utility>> UtilityChanged;

        public DataModels.Items.Utility Utility { get => _utility; set => Common.SetProperty(ref _utility, value, OnUtilityChanged); }

        private void OnUtilityChanged(object sender, ValueChangedEventArgs<DataModels.Items.Utility> e)
        {
            UtilityChanged?.Invoke(this, e);
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Utility ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 1;

            Utility = BuildsManager.Data.Utilities.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
