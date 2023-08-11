using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class BuildSpecialization : INotifyPropertyChanged
    {
        private SpecializationSlotType _specializationSlot;
        private Specialization _specialization;

        public BuildSpecialization()
        {
            Traits.CollectionChanged += Traits_CollectionChanged;
            Traits.ItemChanged += Traits_ItemChanged;
        }

        private void Traits_ItemChanged(object sender, DictionaryItemChangedEventArgs<TraitTierType, Trait> e)
        {
            TraitsChanged?.Invoke( sender, e );
        }

        private void Traits_ValueChanged(object sender, DictionaryItemChangedEventArgs<TraitTierType, Trait> e)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<SpecializationChangedEventArgs> SpecChanged;
        public event EventHandler<EventArgs> TraitsChanged;

        public SpecializationSlotType SpecializationSlot { get => _specializationSlot; set => Common.SetProperty(ref _specializationSlot, value, PropertyChanged); }

        public Specialization Specialization
        {
            get => _specialization; set
            {
                var temp = _specialization;
                if (Common.SetProperty(ref _specialization, value, PropertyChanged))
                {
                    SpecChanged?.Invoke(this, new(temp, value, SpecializationSlot));
                }
            }
        }

        public TraitCollection Traits { get; } = new();

        private void Traits_CollectionChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Traits_CollectionChanged");

            PropertyChanged?.Invoke(sender, e);
            TraitsChanged?.Invoke(this, e);
        }

        internal static BuildSpecialization FromByte(byte id, ProfessionType profession, SpecializationSlotType specializationSlot)
        {
            return BuildsManager.Data.Professions?[profession]?.Specializations.TryGetValue(id, out Specialization specialization) == true
                ? new()
                {
                    Specialization = specialization,
                    SpecializationSlot = specializationSlot,
                }
                : new()
                {
                    SpecializationSlot = specializationSlot,
                };
        }

#nullable enable
        public class SpecializationChangedEventArgs : EventArgs
        {
            public SpecializationChangedEventArgs(Specialization? oldValue, Specialization? newValue, SpecializationSlotType specializationSlot = SpecializationSlotType.Line_1)
            {
                OldSpecialization = oldValue;
                NewSpecialization = newValue;
                SpecializationSlot = specializationSlot;
            }

            public Specialization? OldSpecialization { get; set; }

            public Specialization? NewSpecialization { get; set; }

            public SpecializationSlotType SpecializationSlot { get; set; }
        }
#nullable disable
    }
}
