using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class PowerCoreTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;
        private PowerCore _powerCore;

        public PowerCoreTemplateEntry(TemplateSlotType slot) : base(slot)
        {
        }

        public event EventHandler<ValueChangedEventArgs<PowerCore>> PowerCoreChanged;

        public PowerCore PowerCore { get => _powerCore; set => Common.SetProperty(ref _powerCore, value, OnPowerCoreChanged); }

        private void OnPowerCoreChanged(object sender, ValueChangedEventArgs<PowerCore> e)
        {
            PowerCoreChanged?.Invoke(sender, e);
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                PowerCore ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 1;

            if (array is not null && array.Length > 0)
            {
                PowerCore = BuildsManager.Data.PowerCores.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();
            }

            return array is not null && array.Length > 0 ? GearTemplateCode.RemoveFromStart(array, newStartIndex) : array;
        }

        public void Dispose()
        {
            if(_isDisposed) return;
            _isDisposed = true;

            PowerCore = null;
        }
    }
}
