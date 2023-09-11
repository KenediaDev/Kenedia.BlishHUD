using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class PvpRelicTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;
        private Relic _relic;

        public PvpRelicTemplateEntry(TemplateSlotType slot) : base(slot)
        {

        }

        public event EventHandler<ValueChangedEventArgs<Relic>> RelicChanged;

        public Relic Relic { get=> _relic; set => Common.SetProperty(ref _relic, value, OnRelicChanged); }

        private void OnRelicChanged(object sender, ValueChangedEventArgs<Relic> e)
        {
            RelicChanged?.Invoke(sender, e);
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Relic ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 1;
            if (array is not null && array.Length > 0)
            {
                Relic = BuildsManager.Data.PvpRelics.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();
            }

            return array is not null && array.Length > 0 ? GearTemplateCode.RemoveFromStart(array, newStartIndex) : array;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Relic = null;
        }
    }
}
