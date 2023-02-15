using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Utility;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class BuildSpecialization : INotifyPropertyChanged
    {
        private int _index;
        private Specialization _specialization;

        public BuildSpecialization()
        {
            Traits.CollectionChanged += Traits_CollectionChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Index { get => _index; set => Common.SetProperty(ref _index, value, PropertyChanged); }

        public Specialization Specialization { get => _specialization; set => Common.SetProperty(ref _specialization, value, PropertyChanged); }

        public TraitCollection Traits { get; } = new();

        private void Traits_CollectionChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        internal static BuildSpecialization FromByte(byte id, ProfessionType profession)
        {
            return BuildsManager.Data.Professions?[profession]?.Specializations.TryGetValue(id, out Specialization specialization) == true
                ? (new()
                {
                    Specialization = specialization,
                })
                : null;
        }
    }
}
