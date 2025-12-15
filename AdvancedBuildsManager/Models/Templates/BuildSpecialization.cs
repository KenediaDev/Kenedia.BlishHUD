using Gw2Sharp.Models;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Utility;
using System.ComponentModel;

namespace Kenedia.Modules.AdvancedBuildsManager.Models.Templates
{
    public class BuildSpecialization : INotifyPropertyChanged
    {
        public BuildSpecialization()
        {
            Traits.CollectionChanged += Traits_CollectionChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Index { get; set => Common.SetProperty(field, value, v => field = v, PropertyChanged); }

        public Specialization Specialization { get; set => Common.SetProperty(field, value, v => field = v, PropertyChanged); }

        public TraitCollection Traits { get; } = [];

        private void Traits_CollectionChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        internal static BuildSpecialization FromByte(byte id, ProfessionType profession)
        {
            return AdvancedBuildsManager.Data.Professions?[profession]?.Specializations.TryGetValue(id, out Specialization specialization) == true
                ? new()
                {
                    Specialization = specialization,
                }
                : new();
        }
    }
}
