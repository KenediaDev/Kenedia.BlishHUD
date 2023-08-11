using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class JadeBotTemplateEntry : TemplateEntry
    {
        private JadeBotCore _jadeBotCore;

        public JadeBotTemplateEntry(TemplateSlotType slot) : base(slot)
        {
        }

        public event EventHandler<ValueChangedEventArgs<JadeBotCore>> JadeBotCoreChanged;

        public JadeBotCore JadeBotCore { get => _jadeBotCore; set => Common.SetProperty(ref _jadeBotCore, value, OnJadeBotCoreChanged); }

        private void OnJadeBotCoreChanged(object sender, ValueChangedEventArgs<JadeBotCore> e)
        {
            JadeBotCoreChanged?.Invoke(sender, e);
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                JadeBotCore ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 1;

            JadeBotCore = BuildsManager.Data.JadeBotCores.Values.Where(e => e.MappedId == array[0]).FirstOrDefault();

            return GearTemplateCode.RemoveFromStart(array, newStartIndex);
        }
    }
}
