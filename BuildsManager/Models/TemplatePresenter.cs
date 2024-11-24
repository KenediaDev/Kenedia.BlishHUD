using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using System;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Models
{
    public class TemplatePresenter
    {
        private Template _template = Template.Empty;
        private GameModeType _gameMode = GameModeType.PvE;
        private AttunementType _mainAttunement = AttunementType.Fire;
        private AttunementType _altAttunement = AttunementType.Fire;
        private LegendSlotType _legendSlot = LegendSlotType.TerrestrialActive;

        public TemplatePresenter(TemplateFactory templateFactory, Data data)
        {
            TemplateFactory = templateFactory;
            Data = data;

            Data.Loaded += Data_Loaded;
        }

        private void Data_Loaded(object sender, EventArgs e)
        {
            On_TemplateChanged(this, new(Template, Template));
        }

        public event EventHandler BuildCodeChanged;

        public event EventHandler GearCodeChanged;

        public event TemplateSlotChangedEventHandler? TemplateSlotChanged;

        public event ValueChangedEventHandler<string> NameChanged;

        public event ValueChangedEventHandler<Template> TemplateChanged;

        public event ValueChangedEventHandler<LegendSlotType> LegendSlotChanged;

        public event ValueChangedEventHandler<GameModeType> GameModeChanged;

        public event AttunementChangedEventHandler AttunementChanged;

        public event DictionaryItemChangedEventHandler<PetSlotType, Pet> PetChanged;

        public event ValueChangedEventHandler<ProfessionType> ProfessionChanged;

        public event ValueChangedEventHandler<Races> RaceChanged;

        public event DictionaryItemChangedEventHandler<SkillSlotType, Skill> SkillChanged_OLD;

        public event ValueChangedEventHandler<Specialization> EliteSpecializationChanged_OLD;

        public event LegendChangedEventHandler? LegendChanged;

        //REWORKED

        public event SkillChangedEventHandler SkillChanged;

        public event TraitChangedEventHandler TraitChanged;

        public event SpecializationChangedEventHandler SpecializationChanged;

        public event SpecializationChangedEventHandler EliteSpecializationChanged;

        public Template Template
        {
            get => _template; private set => Common.SetProperty(ref _template, value, On_TemplateChanged);
        }

        public void SetTemplate(Template? template)
        {
            template ??= Template.Empty;
            Template = template;
        }

        public AttunementType MainAttunement { get => _mainAttunement; private set => Common.SetProperty(ref _mainAttunement, value); }

        public AttunementType AltAttunement { get => _altAttunement; private set => Common.SetProperty(ref _altAttunement, value); }

        public LegendSlotType LegendSlot { get => _legendSlot; set => Common.SetProperty(ref _legendSlot, value, OnLegendSlotChanged); }

        public GameModeType GameMode { get => _gameMode; set => Common.SetProperty(ref _gameMode, value, On_GameModeChanged); }

        public bool IsPve => GameMode == GameModeType.PvE;

        public bool IsPvp => GameMode == GameModeType.PvP;

        public bool IsWvw => GameMode == GameModeType.WvW;

        public TemplateFactory TemplateFactory { get; }

        public Data Data { get; }

        private void On_TemplateChanged(object sender, ValueChangedEventArgs<Template> e)
        {
            if (e.OldValue is not null)
            {
                e.OldValue.RaceChanged -= On_RaceChanged;
                e.OldValue.BuildCodeChanged -= On_BuildChanged;
                e.OldValue.GearCodeChanged -= On_GearChanged;
                e.OldValue.ProfessionChanged -= On_ProfessionChanged;
                e.OldValue.LegendChanged -= On_LegendChanged;

                e.OldValue.NameChanged -= On_NameChanged;

                //REWORKED STUFF
                e.OldValue.TemplateSlotChanged -= OnTemplateSlotChanged;
                e.OldValue.SkillChanged -= OnSkillChanged;
                e.OldValue.TraitChanged -= OnTraitChanged;
                e.OldValue.EliteSpecializationChanged -= OnEliteSpecializationChanged;
                e.OldValue.SpecializationChanged -= OnSpecializationChanged;
            }

            e.NewValue?.Load();

            if (e.NewValue is not null)
            {
                RegisterEvents(e.NewValue);
            }

            TemplateChanged?.Invoke(this, e);
        }

        private void RegisterEvents(Template template)
        {
            if (template is null) return;

            template.RaceChanged += On_RaceChanged;
            template.BuildCodeChanged += On_BuildChanged;
            template.GearCodeChanged += On_GearChanged;
            template.ProfessionChanged += On_ProfessionChanged;
            template.LegendChanged += On_LegendChanged;

            template.NameChanged += On_NameChanged;

            //REWORKED STUFF
            template.TemplateSlotChanged += OnTemplateSlotChanged;
            template.SkillChanged += OnSkillChanged;
            template.TraitChanged += OnTraitChanged;
            template.SpecializationChanged += OnSpecializationChanged;
            template.EliteSpecializationChanged += OnEliteSpecializationChanged;
        }

        private void OnTemplateSlotChanged(object sender, TemplateSlotChangedEventArgs e)
        {
            TemplateSlotChanged?.Invoke(sender, e);
        }

        private void OnEliteSpecializationChanged(object sender, SpecializationChangedEventArgs e)
        {
            EliteSpecializationChanged?.Invoke(sender, e);
            SetAttunement(MainAttunement);
        }

        private void OnSpecializationChanged(object sender, SpecializationChangedEventArgs e)
        {
            SpecializationChanged?.Invoke(sender, e);
        }

        private void OnTraitChanged(object sender, TraitChangedEventArgs e)
        {
            TraitChanged?.Invoke(sender, e);
        }

        private void OnSkillChanged(object sender, SkillChangedEventArgs e)
        {
            SkillChanged?.Invoke(sender, e);
        }

        private void On_NameChanged(object sender, ValueChangedEventArgs<string> e)
        {
            NameChanged?.Invoke(sender, e);
        }

        private void On_GearChanged(object sender, EventArgs e)
        {
            GearCodeChanged?.Invoke(sender, e);
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
            EliteSpecializationChanged_OLD?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_LegendChanged(object sender, LegendChangedEventArgs e)
        {
            LegendChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void On_ProfessionChanged(object sender, ValueChangedEventArgs<ProfessionType> e)
        {
            ProfessionChanged?.Invoke(sender, e);
            BuildCodeChanged?.Invoke(sender, e);
        }

        private void OnLegendSlotChanged(object sender, ValueChangedEventArgs<LegendSlotType> e)
        {
            LegendSlotChanged?.Invoke(sender, e);
        }

        private void On_GameModeChanged(object sender, ValueChangedEventArgs<GameModeType> e)
        {
            GameModeChanged?.Invoke(sender, e);
        }

        private void OnAttunementChanged(object sender, AttunementChangedEventArgs e)
        {
            AttunementChanged?.Invoke(sender, e);
        }

        public void SetAttunement(AttunementType attunement)
        {
            var slot = MainAttunement != attunement ? AttunementSlotType.Main
                : AltAttunement != attunement ? AttunementSlotType.Alt:
                AttunementSlotType.Main;

            if (MainAttunement != attunement || AltAttunement != attunement)
            {
                var previous = MainAttunement;

                MainAttunement = attunement;
                AltAttunement = Template.EliteSpecializationId == (int)SpecializationType.Weaver ? previous : AttunementType.None;

                OnAttunementChanged(this, new(slot, previous, attunement));
            }
        }

        public void SwapLegend()
        {
            LegendSlot = LegendSlot switch
            {
                LegendSlotType.TerrestrialActive or LegendSlotType.AquaticActive => LegendSlotType.TerrestrialInactive,
                LegendSlotType.TerrestrialInactive or LegendSlotType.AquaticInactive => LegendSlotType.TerrestrialActive,
                _ => LegendSlotType.TerrestrialActive,
            };
        }

        public void InvokeTemplateSwitch()
        {
            TemplateChanged?.Invoke(this, new(_template, _template));
        }
    }
}
