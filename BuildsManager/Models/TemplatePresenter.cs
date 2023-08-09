using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class TemplatePresenter
    {
        private VTemplate _template = new();
        private GameModeType _gameMode = GameModeType.PvE;
        private AttunementType _mainAttunement = AttunementType.Fire;
        private AttunementType _altAttunement = AttunementType.Fire;
        private LegendSlot _legendSlot = LegendSlot.TerrestrialActive;

        public TemplatePresenter()
        {
            RegisterEvents(_template);
        }

        public event EventHandler LoadedGearFromCode;

        public event EventHandler LoadedBuildFromCode;

        public event EventHandler BuildCodeChanged;

        public event ValueChangedEventHandler<VTemplate> TemplateChanged;

        public event ValueChangedEventHandler<LegendSlot> LegendSlotChanged;

        public event ValueChangedEventHandler<GameModeType> GameModeChanged;

        public event ValueChangedEventHandler<AttunementType> AttunementChanged;

        public event DictionaryItemChangedEventHandler<PetSlot, Pet> PetChanged;

        public event ValueChangedEventHandler<ProfessionType> ProfessionChanged;

        public event ValueChangedEventHandler<Races> RaceChanged;

        public event DictionaryItemChangedEventHandler<SkillSlot, Skill> SkillChanged;

        public event ValueChangedEventHandler<Specialization> EliteSpecializationChanged;

        public event DictionaryItemChangedEventHandler<LegendSlot, Legend> LegendChanged;

        public event DictionaryItemChangedEventHandler<SpecializationSlot, Specialization> SpecializationChanged;

        public VTemplate Template { get => _template; set => Common.SetProperty(ref _template, value, On_TemplateChanged); }

        public AttunementType MainAttunement { get => _mainAttunement; set => Common.SetProperty(ref _mainAttunement, value, On_AttunementChanged); }

        public AttunementType AltAttunement { get => _altAttunement; set => Common.SetProperty(ref _altAttunement, value, On_AttunementChanged); }

        public LegendSlot LegendSlot { get => _legendSlot; set => Common.SetProperty(ref _legendSlot, value, On_LegendSlotChanged); }

        public GameModeType GameMode { get => _gameMode; set => Common.SetProperty(ref _gameMode, value, On_GameModeChanged); }

        public bool IsPve => GameMode == GameModeType.PvE;

        public bool IsPvp => GameMode == GameModeType.PvP;

        public bool IsWvw => GameMode == GameModeType.WvW;

        private void On_TemplateChanged(object sender, ValueChangedEventArgs<VTemplate> e)
        {
            if (e.OldValue != null)
            {
                e.OldValue.RaceChanged -= On_RaceChanged;
                e.OldValue.BuildChanged -= On_BuildChanged;
                e.OldValue.ProfessionChanged -= On_ProfessionChanged;
                e.OldValue.EliteSpecializationChanged -= On_EliteSpecializationChanged;
                e.OldValue.SpecializationChanged -= On_SpecializationChanged;

                e.OldValue.LoadedBuildFromCode -= On_LoadedBuildFromCode;
                e.OldValue.LoadedGearFromCode -= On_LoadedGearFromCode;
            }

            if (e.NewValue != null)
            {
                RegisterEvents(e.NewValue);
            }

            TemplateChanged?.Invoke(this, e);
        }

        private void RegisterEvents(VTemplate template)
        {
            template.RaceChanged += On_RaceChanged;
            template.BuildChanged += On_BuildChanged;
            template.ProfessionChanged += On_ProfessionChanged;
            template.EliteSpecializationChanged += On_EliteSpecializationChanged;
            template.SpecializationChanged += On_SpecializationChanged;

            template.LoadedBuildFromCode += On_LoadedBuildFromCode;
            template.LoadedGearFromCode += On_LoadedGearFromCode;
        }

        private void On_LoadedGearFromCode(object sender, EventArgs e)
        {
            LoadedGearFromCode?.Invoke(sender, e);
        }

        private void On_LoadedBuildFromCode(object sender, EventArgs e)
        {
            LoadedBuildFromCode?.Invoke(sender, e);
        }

        private void On_RaceChanged(object sender, ValueChangedEventArgs<Races> e)
        {
            RaceChanged?.Invoke(sender, e);
        }

        private void On_BuildChanged(object sender, EventArgs e)
        {
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_EliteSpecializationChanged(object sender, ValueChangedEventArgs<Specialization> e)
        {
            EliteSpecializationChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_LegendChanged(object sender, DictionaryItemChangedEventArgs<LegendSlot, Legend> e)
        {
            LegendChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_SpecializationChanged(object sender, DictionaryItemChangedEventArgs<SpecializationSlot, Specialization> e)
        {
            SpecializationChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_SkillChanged(object sender, DictionaryItemChangedEventArgs<SkillSlot, Skill> e)
        {
            SkillChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_PetChanged(object sender, DictionaryItemChangedEventArgs<PetSlot, Pet> e)
        {
            PetChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_ProfessionChanged(object sender, ValueChangedEventArgs<ProfessionType> e)
        {
            ProfessionChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_LegendSlotChanged(object sender, ValueChangedEventArgs<LegendSlot> e)
        {
            LegendSlotChanged?.Invoke(sender, e);
        }

        private void On_GameModeChanged(object sender, ValueChangedEventArgs<GameModeType> e)
        {
            GameModeChanged?.Invoke(sender, e);
        }

        private void On_AttunementChanged(object sender, ValueChangedEventArgs<AttunementType> e)
        {
            AttunementChanged?.Invoke(sender, e);
        }

        public void SetAttunmenemt(AttunementType attunement)
        {
            if (_mainAttunement.Equals(attunement)) return;

            _altAttunement = _mainAttunement;
            _mainAttunement = attunement;

            On_AttunementChanged(this, new(_altAttunement, _mainAttunement));
        }

        public void SwapLegend(LegendSlot? legendSlot = null)
        {
            legendSlot ??= LegendSlot is LegendSlot.AquaticActive or LegendSlot.TerrestrialActive ? LegendSlot.TerrestrialInactive : LegendSlot.TerrestrialActive;
            LegendSlot slot = legendSlot is LegendSlot.AquaticActive or LegendSlot.TerrestrialActive ? LegendSlot.TerrestrialActive : LegendSlot.TerrestrialInactive;

            if (_legendSlot.Equals(slot)) return;

            var oldLegend = LegendSlot;
            _legendSlot = slot;

            On_LegendSlotChanged(this, new(oldLegend, _legendSlot));
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
