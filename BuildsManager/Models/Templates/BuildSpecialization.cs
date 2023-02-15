using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Utility;
using System;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    public class BuildSpecialization
    {
        private int _index;
        private Specialization _specialization;

        public BuildSpecialization()
        {
        }

        public event EventHandler Changed;

        public int Index { get => _index; set => Common.SetProperty(ref _index, value, OnChanged); }

        public Specialization Specialization { get => _specialization; set => Common.SetProperty(ref _specialization, value, OnChanged); }

        public TraitCollection Traits { get; } = new();

        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
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
