using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.Models;
using System;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public abstract class ProfessionSpecifics : Control
    {
        protected TemplatePresenter Internal_TemplatePresenter;

        public ProfessionSpecifics()
        {
            ClipsBounds = false;
            ZIndex = int.MaxValue;
        }

        public ProfessionSpecifics(TemplatePresenter template) : this()
        {
            TemplatePresenter = template;
        }

        public BuildPage BuildPage { get; set; }

        public TemplatePresenter TemplatePresenter
        {
            get => Internal_TemplatePresenter; set => Common.SetProperty(ref Internal_TemplatePresenter, value, SetTemplatePresenter);
        }

        private void SetTemplatePresenter(object sender, ValueChangedEventArgs<TemplatePresenter> e)
        {
            if (e.OldValue != null)
            {
                e.OldValue.Loaded -= OnLoaded;
                e.OldValue.TemplateChanged -= TemplatePresenter_TemplateChanged;
                e.OldValue.BuildCodeChanged -= OnBuildCodeChanged;
                e.OldValue.LegendChanged -= OnLegendChanged;
            }

            if (e.NewValue != null)
            {
                e.NewValue.Loaded += OnLoaded;
                e.NewValue.TemplateChanged += TemplatePresenter_TemplateChanged;
                e.NewValue.BuildCodeChanged += OnBuildCodeChanged;
                e.NewValue.LegendChanged += OnLegendChanged;
            }
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            ApplyTemplate();
        }

        private void OnBuildCodeChanged(object sender, EventArgs e)
        {
            ApplyTemplate();
        }

        private void OnLegendChanged(object sender, DictionaryItemChangedEventArgs<LegendSlot, DataModels.Professions.Legend> e)
        {
            ApplyTemplate();
        }

        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<Template> e)
        {
            ApplyTemplate();
        }

        protected virtual void ApplyTemplate()
        {
            RecalculateLayout();
        }
    }
}
