using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class TemplatePresenter
    {
        private Template _template = new();
        private GameModeType _gameMode = GameModeType.PvE;
        private AttunementType _mainAttunement = AttunementType.Fire;
        private AttunementType _altAttunement = AttunementType.Fire;
        private LegendSlotType _legendSlot = LegendSlotType.TerrestrialActive;

        public TemplatePresenter()
        {
            RegisterEvents(_template);
        }

        public event EventHandler LoadedGearFromCode;

        public event EventHandler LoadedBuildFromCode;

        public event EventHandler BuildCodeChanged;

        public event EventHandler GearCodeChanged;

        public event EventHandler<(TemplateSlotType slot, BaseItem item, Stat stat)> TemplateSlotChanged;

        public event ValueChangedEventHandler<string> NameChanged;

        public event ValueChangedEventHandler<Template> TemplateChanged;

        public event ValueChangedEventHandler<LegendSlotType> LegendSlotChanged;

        public event ValueChangedEventHandler<GameModeType> GameModeChanged;

        public event ValueChangedEventHandler<AttunementType> AttunementChanged;

        public event DictionaryItemChangedEventHandler<PetSlotType, Pet> PetChanged;

        public event ValueChangedEventHandler<ProfessionType> ProfessionChanged;

        public event ValueChangedEventHandler<Races> RaceChanged;

        public event DictionaryItemChangedEventHandler<SkillSlotType, Skill> SkillChanged_OLD;

        public event ValueChangedEventHandler<Specialization> EliteSpecializationChanged;

        public event DictionaryItemChangedEventHandler<LegendSlotType, Legend> LegendChanged;

        public event DictionaryItemChangedEventHandler<SpecializationSlotType, Specialization> SpecializationChanged;

        public event SkillChangedEventHandler SkillChanged;

        public Template Template { get => _template; set => Common.SetProperty(ref _template, value, On_TemplateChanged); }

        public AttunementType MainAttunement { get => _mainAttunement; set => Common.SetProperty(ref _mainAttunement, value, On_AttunementChanged); }

        public AttunementType AltAttunement { get => _altAttunement; set => Common.SetProperty(ref _altAttunement, value, On_AttunementChanged); }

        public LegendSlotType LegendSlot { get => _legendSlot; set => Common.SetProperty(ref _legendSlot, value, On_LegendSlotChanged); }

        public GameModeType GameMode { get => _gameMode; set => Common.SetProperty(ref _gameMode, value, On_GameModeChanged); }

        public bool IsPve => GameMode == GameModeType.PvE;

        public bool IsPvp => GameMode == GameModeType.PvP;

        public bool IsWvw => GameMode == GameModeType.WvW;

        private void On_TemplateChanged(object sender, ValueChangedEventArgs<Template> e)
        {
            if (e.OldValue is not null)
            {
                e.OldValue.RaceChanged -= On_RaceChanged;
                e.OldValue.BuildCodeChanged -= On_BuildChanged;
                e.OldValue.GearCodeChanged -= On_GearChanged;
                e.OldValue.ProfessionChanged -= On_ProfessionChanged;
                e.OldValue.EliteSpecializationChanged -= On_EliteSpecializationChanged;
                e.OldValue.SpecializationChanged -= On_SpecializationChanged;
                e.OldValue.LegendChanged -= On_LegendChanged;
                e.OldValue.SkillChanged_OLD -= On_SkillChanged_OLD;

                e.OldValue.LoadedBuildFromCode -= On_LoadedBuildFromCode;
                e.OldValue.LoadedGearFromCode -= On_LoadedGearFromCode;

                e.OldValue.NameChanged -= On_NameChanged;

                //REWORKED STUFF
                e.OldValue.SkillChanged -= On_SkillChanged;
            }

            if (e.NewValue is not null)
            {
                RegisterEvents(e.NewValue);
            }

            TemplateChanged?.Invoke(this, e);
        }

        private void RegisterEvents(Template template)
        {
            template.RaceChanged += On_RaceChanged;
            template.BuildCodeChanged += On_BuildChanged;
            template.GearCodeChanged += On_GearChanged;
            template.ProfessionChanged += On_ProfessionChanged;
            template.EliteSpecializationChanged += On_EliteSpecializationChanged;
            template.SpecializationChanged += On_SpecializationChanged;
            template.LegendChanged += On_LegendChanged;
            template.SkillChanged_OLD += On_SkillChanged_OLD;

            template.LoadedBuildFromCode += On_LoadedBuildFromCode;
            template.LoadedGearFromCode += On_LoadedGearFromCode;

            template.NameChanged += On_NameChanged;
            template.TemplateSlotChanged += Template_TemplateSlotChanged;

            //REWORKED STUFF
            template.SkillChanged += On_SkillChanged;
        }

        private void On_SkillChanged(object sender, SkillChangedEventArgs e)
        {
            SkillChanged?.Invoke(sender, e);
        }

        private void Template_TemplateSlotChanged(object sender, (TemplateSlotType slot, BaseItem item, Stat stat) e)
        {
            TemplateSlotChanged?.Invoke(sender, e);
        }

        private void On_NameChanged(object sender, ValueChangedEventArgs<string> e)
        {
            NameChanged?.Invoke(sender, e);
        }

        private void On_GearChanged(object sender, EventArgs e)
        {
            GearCodeChanged?.Invoke(sender, e);
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

        private void On_LegendChanged(object sender, DictionaryItemChangedEventArgs<LegendSlotType, Legend> e)
        {
            LegendChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_SpecializationChanged(object sender, DictionaryItemChangedEventArgs<SpecializationSlotType, Specialization> e)
        {
            SpecializationChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_SkillChanged_OLD(object sender, DictionaryItemChangedEventArgs<SkillSlotType, Skill> e)
        {
            //SkillChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_PetChanged(object sender, DictionaryItemChangedEventArgs<PetSlotType, Pet> e)
        {
            PetChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_ProfessionChanged(object sender, ValueChangedEventArgs<ProfessionType> e)
        {
            ProfessionChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_LegendSlotChanged(object sender, ValueChangedEventArgs<LegendSlotType> e)
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

        public void SwapLegend(LegendSlotType? legendSlot = null)
        {
            legendSlot ??= LegendSlot is LegendSlotType.AquaticActive or LegendSlotType.TerrestrialActive ? LegendSlotType.TerrestrialInactive : LegendSlotType.TerrestrialActive;
            LegendSlotType slot = legendSlot is LegendSlotType.AquaticActive or LegendSlotType.TerrestrialActive ? LegendSlotType.TerrestrialActive : LegendSlotType.TerrestrialInactive;

            if (_legendSlot.Equals(slot)) return;

            var oldLegend = LegendSlot;
            _legendSlot = slot;

            On_LegendSlotChanged(this, new(oldLegend, _legendSlot));
        }

        public void SetProfession(ProfessionType profession)
        {
            if (Template is not null)
            {
                Template.Profession = profession;
            }
        }

        public void SetRace(Races race)
        {
            if (Template is not null)
            {
                Template.Race = race;
            }
        }

        public void InvokeTemplateSwitch()
        {
            TemplateChanged?.Invoke(this, new(_template, _template));
        }
    }
}
