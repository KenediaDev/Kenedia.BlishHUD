using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class TemplatePresenter
    {
        private Template _template;
        private AttunementType _mainAttunement = AttunementType.Fire;
        private AttunementType _altAttunement = AttunementType.Fire;
        private LegendSlot _legendSlot = LegendSlot.TerrestrialActive;

        public TemplatePresenter()
        {

        }

        public event EventHandler Loaded;

        public event EventHandler BuildCodeChanged;

        public event ValueChangedEventHandler<Template> TemplateChanged;

        public event ValueChangedEventHandler<LegendSlot> LegendSlotChanged;

        public event ValueChangedEventHandler<AttunementType> AttunementChanged;

        public event DictionaryItemChangedEventHandler<PetSlot, Pet> PetChanged;

        public event ValueChangedEventHandler<ProfessionType> ProfessionChanged;

        public event ValueChangedEventHandler<Races> RaceChanged;

        public event DictionaryItemChangedEventHandler<SkillSlot, Skill> SkillChanged;

        public event ValueChangedEventHandler<Specialization> EliteSpecializationChanged;

        public event DictionaryItemChangedEventHandler<LegendSlot, Legend> LegendChanged;

        public event DictionaryItemChangedEventHandler<SpecializationSlot, Specialization> SpecializationChanged;

        public Template Template { get => _template; set => Common.SetProperty(ref _template, value, OnTemplateChanged); }

        public AttunementType MainAttunement { get => _mainAttunement; set => Common.SetProperty(ref _mainAttunement, value, OnAttunementChanged); }

        public AttunementType AltAttunement { get => _altAttunement; set => Common.SetProperty(ref _altAttunement, value, OnAttunementChanged); }

        public LegendSlot LegendSlot { get => _legendSlot; set => Common.SetProperty(ref _legendSlot, value, OnLegendSlotChanged); }

        private void OnTemplateChanged(object sender, ValueChangedEventArgs<Template> e)
        {
            if (e.OldValue != null)
            {
                e.OldValue.BuildTemplate.BuildCodeChanged -= BuildTemplate_BuildCodeChanged;
                e.OldValue.BuildTemplate.ProfessionChanged -= OnProfessionChanged;
                e.OldValue.BuildTemplate.PetChanged -= On_PetChanged;
                e.OldValue.BuildTemplate.SkillChanged -= OnSkillChanged;
                e.OldValue.BuildTemplate.EliteSpecializationChanged -= On_EliteSpecializationChanged;
                e.OldValue.BuildTemplate.LegendChanged -= OnLegendChanged;
                e.OldValue.BuildTemplate.SpecializationChanged -= OnSpecializationChanged;
                e.OldValue.BuildTemplate.Loaded -= OnLoaded;
                e.OldValue.RaceChanged -= OnRaceChanged;
            }

            if (e.NewValue != null)
            {
                e.NewValue.BuildTemplate.BuildCodeChanged += BuildTemplate_BuildCodeChanged;
                e.NewValue.BuildTemplate.ProfessionChanged += OnProfessionChanged;
                e.NewValue.BuildTemplate.PetChanged += On_PetChanged;
                e.NewValue.BuildTemplate.SkillChanged += OnSkillChanged;
                e.NewValue.BuildTemplate.EliteSpecializationChanged += On_EliteSpecializationChanged;
                e.NewValue.BuildTemplate.LegendChanged += OnLegendChanged;
                e.NewValue.BuildTemplate.SpecializationChanged += OnSpecializationChanged;
                e.NewValue.BuildTemplate.Loaded += OnLoaded;
                e.NewValue.RaceChanged += OnRaceChanged;
            }

            TemplateChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void OnRaceChanged(object sender, ValueChangedEventArgs<Races> e)
        {
            RaceChanged?.Invoke(sender, e);
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            Loaded?.Invoke(sender, e);
        }

        private void BuildTemplate_BuildCodeChanged(object sender, EventArgs e)
        {
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_EliteSpecializationChanged(object sender, ValueChangedEventArgs<Specialization> e)
        {
            EliteSpecializationChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void OnLegendChanged(object sender, DictionaryItemChangedEventArgs<LegendSlot, Legend> e)
        {
            LegendChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void OnSpecializationChanged(object sender, DictionaryItemChangedEventArgs<SpecializationSlot, Specialization> e)
        {
            SpecializationChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void OnSkillChanged(object sender, DictionaryItemChangedEventArgs<SkillSlot, Skill> e)
        {
            SkillChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_PetChanged(object sender, DictionaryItemChangedEventArgs<PetSlot, Pet> e)
        {
            PetChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void OnProfessionChanged(object sender, ValueChangedEventArgs<ProfessionType> e)
        {
            ProfessionChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void OnLegendSlotChanged(object sender, ValueChangedEventArgs<LegendSlot> e)
        {
            LegendSlotChanged?.Invoke(sender, e);
        }

        private void OnAttunementChanged(object sender, ValueChangedEventArgs<AttunementType> e)
        {
            AttunementChanged?.Invoke(sender, e);
        }

        public void SetAttunmenemt(AttunementType attunement)
        {
            if (_mainAttunement.Equals(attunement)) return;

            _altAttunement = _mainAttunement;
            _mainAttunement = attunement;

            OnAttunementChanged(this, new(_altAttunement, _mainAttunement));
        }

        public void SwapLegend(LegendSlot? legendSlot = null)
        {
            legendSlot ??= LegendSlot is LegendSlot.AquaticActive or LegendSlot.TerrestrialActive ? LegendSlot.TerrestrialInactive : LegendSlot.TerrestrialActive;
            LegendSlot slot = legendSlot is LegendSlot.AquaticActive or LegendSlot.TerrestrialActive ? LegendSlot.TerrestrialActive : LegendSlot.TerrestrialInactive;

            if (_legendSlot.Equals(slot)) return;

            var oldLegend = LegendSlot;
            _legendSlot = slot;

            OnLegendSlotChanged(this, new(oldLegend, _legendSlot));
        }

        public void SetProfession(ProfessionType profession)
        {
            if (Template != null)
            {
                Template.Profession = profession;
            }
        }

        public void SetRace(Races race)
        {
            if (Template != null)
            {
                Template.Race = race;
            }
        }
    }
}
