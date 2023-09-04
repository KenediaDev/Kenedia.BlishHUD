using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System.Linq;
using System;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class EnhancementTemplateEntry : TemplateEntry, IDisposable
    {
        private bool _isDisposed;
        private DataModels.Items.Enhancement _utility;

        public EnhancementTemplateEntry(TemplateSlotType slot) : base(slot)
        {
        }

        public event EventHandler<ValueChangedEventArgs<DataModels.Items.Enhancement>> UtilityChanged;

        public DataModels.Items.Enhancement Enhancement { get => _utility; set => Common.SetProperty(ref _utility, value, OnUtilityChanged); }

        private void OnUtilityChanged(object sender, ValueChangedEventArgs<DataModels.Items.Enhancement> e)
        {
            UtilityChanged?.Invoke(this, e);
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Enhancement ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 1;

            Enhancement = BuildsManager.Data.Enhancements.Items.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Enhancement = null;
        }
    }
}
