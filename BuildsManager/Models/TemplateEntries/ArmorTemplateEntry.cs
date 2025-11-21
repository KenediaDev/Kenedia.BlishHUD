using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using System;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.BuildsManager.Interfaces;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class ArmorTemplateEntry : TemplateEntry, IDisposable, IStatTemplateEntry, IRuneTemplateEntry, ISingleInfusionTemplateEntry, IArmorTemplateEntry
    {
        private bool _isDisposed;
        private Stat _stat;
        private Infusion _infusion1;
        private Rune _rune;
        private Armor _armor;

        public ArmorTemplateEntry(TemplateSlotType slot, Data data) : base(slot, data)
        {
        }

        public Armor Armor { get => _armor; private set => Common.SetProperty(ref _armor, value); }

        public Rune Rune { get => _rune; private set => Common.SetProperty(ref _rune, value); }

        public Infusion Infusion1 { get => _infusion1; private set => Common.SetProperty(ref _infusion1, value); }

        public Stat Stat { get => _stat; private set => Common.SetProperty(ref _stat, value); }

        protected override void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is null)
            {
                Armor = null;
            }
            else if (e.NewValue is Armor armor)
            {
                Armor = armor;
            }
        }

        public void Dispose()
        {
            if(_isDisposed)
                return;

            _isDisposed = true;

            Stat = null;
            Rune = null;
            Infusion1 = null;
            Armor = null;
        }

        public override bool SetValue(TemplateSlotType slot, TemplateSubSlotType subSlot, object obj)
        {
            if (subSlot == TemplateSubSlotType.Item)
            {
                if (obj?.Equals(Item) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Item = null;
                    return true;
                }

                if (obj is Armor armor)
                {
                    Item = armor;
                    return true;
                }
            }
            else if (subSlot == TemplateSubSlotType.Stat)
            {
                if (obj?.Equals(Stat) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Stat = null;
                    return true;
                }
                else if (obj is Stat stat)
                {
                    Stat = stat;
                    return true;
                }
            }
            else if (subSlot == TemplateSubSlotType.Rune)
            {
                if (obj?.Equals(Rune) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Rune = null;
                    return true;
                }
                else if (obj is Rune rune)
                {
                    Rune = rune;
                    return true;
                }
            }
            else if (subSlot == TemplateSubSlotType.Infusion1)
            {
                if (obj?.Equals(Infusion1) is true)
                {
                    return false;
                }

                if (obj is null)
                {
                    Infusion1 = null;
                    return true;
                }
                else if (obj is Infusion infusion)
                {
                    Infusion1 = infusion;
                    return true;
                }
            }

            return false;
        }
    }
}
