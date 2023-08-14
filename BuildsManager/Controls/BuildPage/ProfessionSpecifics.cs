using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.Models;
using System;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public abstract class ProfessionSpecifics : Control
    {
        private TemplatePresenter _templatePresenter;

        public ProfessionSpecifics(TemplatePresenter template)
        {
            TemplatePresenter = template;
            ClipsBounds = false;
            ZIndex = int.MaxValue;

            ApplyTemplate();
        }

        public BuildPage BuildPage { get; set; }

        public TemplatePresenter TemplatePresenter
        {
            get => _templatePresenter; set => Common.SetProperty(ref _templatePresenter, value, SetTemplatePresenter);
        }

        private void SetTemplatePresenter(object sender, ValueChangedEventArgs<TemplatePresenter> e)
        {
            if (e.OldValue is not null)
            {
                e.OldValue.LoadedBuildFromCode -= OnLoaded;
                e.OldValue.LegendChanged -= OnLegendChanged;
                e.OldValue.EliteSpecializationChanged -= OnEliteSpecializationChanged;
                e.OldValue.TemplateChanged -= OnTemplateChanged;
            }

            if (e.NewValue is not null)
            {
                e.NewValue.LoadedBuildFromCode += OnLoaded;
                e.NewValue.LegendChanged += OnLegendChanged;
                e.NewValue.EliteSpecializationChanged += OnEliteSpecializationChanged;
                e.NewValue.TemplateChanged += OnTemplateChanged;
            }
        }

        private void OnEliteSpecializationChanged(object sender, ValueChangedEventArgs<DataModels.Professions.Specialization> e)
        {
            ApplyTemplate();
        }

        private void OnTemplateChanged(object sender, ValueChangedEventArgs<Template> e)
        {
            ApplyTemplate();
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            ApplyTemplate();
        }

        private void OnLegendChanged(object sender, DictionaryItemChangedEventArgs<LegendSlotType, DataModels.Professions.Legend> e)
        {
            ApplyTemplate();
        }

        protected virtual void ApplyTemplate()
        {
            if(TemplatePresenter?.Template is null)
            {
                return;
            }

            RecalculateLayout();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
        }
    }
}
