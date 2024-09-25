﻿using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.Models;
using System;

namespace Kenedia.Modules.BuildsManager.Controls.ProfessionSpecific
{
    public abstract class ProfessionSpecifics : Panel
    {
        private TemplatePresenter _templatePresenter;

        public ProfessionSpecifics(TemplatePresenter template)
        {
            TemplatePresenter = template;
            ClipsBounds = false;
            ZIndex = int.MaxValue / 2;

            ApplyTemplate();
        }

        public TemplatePresenter TemplatePresenter
        {
            get => _templatePresenter; set => Common.SetProperty(ref _templatePresenter, value, SetTemplatePresenter);
        }

        private void SetTemplatePresenter(object sender, ValueChangedEventArgs<TemplatePresenter> e)
        {
            if (e.OldValue is not null)
            {
                e.OldValue.LegendChanged -= OnLegendChanged;
                e.OldValue.EliteSpecializationChanged_OLD -= OnEliteSpecializationChanged;
                e.OldValue.TemplateChanged -= OnTemplateChanged;
            }

            if (e.NewValue is not null)
            {
                e.NewValue.LegendChanged += OnLegendChanged;
                e.NewValue.EliteSpecializationChanged_OLD += OnEliteSpecializationChanged;
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

        private void OnLegendChanged(object sender, LegendChangedEventArgs e)
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
