﻿using Kenedia.Modules.BuildsManager.Models.Templates;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Utility;
using System;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.BuildsManager.Interfaces;

namespace Kenedia.Modules.BuildsManager.TemplateEntries
{
    public class ArmorTemplateEntry : TemplateEntry, IDisposable, IStatTemplateEntry, IRuneTemplateEntry, ISingleInfusionTemplateEntry, IArmorTemplateEntry
    {
        private bool _isDisposed;
        private Stat _stat;
        private Infusion _infusion1;
        private Rune _rune;
        private Armor _armor;

        public ArmorTemplateEntry(TemplateSlotType slot) : base(slot)
        {
        }

        public Armor Armor { get => _armor; private set => Common.SetProperty(ref _armor, value); }

        public Rune Rune { get => _rune; private set => Common.SetProperty(ref _rune, value); }

        public Infusion Infusion1 { get => _infusion1; private set => Common.SetProperty(ref _infusion1, value); }

        public Stat Stat { get => _stat; private set => Common.SetProperty(ref _stat, value); }

        protected override void OnItemChanged(object sender, ValueChangedEventArgs<BaseItem> e)
        {
            base.OnItemChanged(sender, e);

            if (e.NewValue is Armor armor)
            {
                Armor = armor;
            }
        }

        public override byte[] AddToCodeArray(byte[] array)
        {
            return array.Concat(new byte[]
            {
                Stat?.MappedId ?? 0,
                Rune ?.MappedId ?? 0,
                Infusion1 ?.MappedId ?? 0,
            }).ToArray();
        }

        public override byte[] GetFromCodeArray(byte[] array)
        {
            int newStartIndex = 3;

            if (array is not null && array.Length > 0)
            {
                Stat = BuildsManager.Data.Stats.Items.Where(e => e.Value.MappedId == array[0]).FirstOrDefault().Value;
                Rune = BuildsManager.Data.PveRunes.Items.Where(e => e.Value.MappedId == array[1]).FirstOrDefault().Value;
                Infusion1 = BuildsManager.Data.Infusions.Items.Where(e => e.Value.MappedId == array[2]).FirstOrDefault().Value;
            }

            return array is not null && array.Length > 0 ? GearTemplateCode.RemoveFromStart(array, newStartIndex) : array;
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
