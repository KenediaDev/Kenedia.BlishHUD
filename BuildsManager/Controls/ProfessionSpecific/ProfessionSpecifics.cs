using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.Models;
using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Blish_HUD.Input;

namespace Kenedia.Modules.BuildsManager.Controls.ProfessionSpecific
{
    public abstract class ProfessionSpecifics : Panel
    {
        private TemplatePresenter? _templatePresenter;

        protected virtual SkillIcon[] Skills { get; } = Array.Empty<SkillIcon>();

        public ProfessionSpecifics(TemplatePresenter template)
        {
            TemplatePresenter = template;
            ClipsBounds = false;
            ZIndex = int.MaxValue / 2;

            Tooltip = SkillTooltip = new();
            ApplyTemplate();
        }

        protected SkillTooltip SkillTooltip { get; }

        public TemplatePresenter TemplatePresenter
        {
            get => _templatePresenter; set => Common.SetProperty(ref _templatePresenter, value, SetTemplatePresenter);
        }

        private void SetTemplatePresenter(object sender, ValueChangedEventArgs<TemplatePresenter> e)
        {
            if (e.OldValue is not null)
            {
                e.OldValue.LegendChanged -= OnLegendChanged;
                e.OldValue.EliteSpecializationChanged -= OnEliteSpecializationChanged;
                e.OldValue.TemplateChanged -= OnTemplateChanged;
                e.OldValue.TraitChanged -= OnTraitChanged;
                e.OldValue.SkillChanged -= OnSkillChanged;
            }

            if (e.NewValue is not null)
            {
                e.NewValue.LegendChanged += OnLegendChanged;
                e.NewValue.EliteSpecializationChanged += OnEliteSpecializationChanged;
                e.NewValue.TemplateChanged += OnTemplateChanged;
                e.NewValue.TraitChanged += OnTraitChanged;
                e.NewValue.SkillChanged += OnSkillChanged;
            }
        }

        private void OnSkillChanged(object sender, SkillChangedEventArgs e)
        {
            ApplyTemplate();
        }

        private void OnTraitChanged(object sender, TraitChangedEventArgs e)
        {
            ApplyTemplate();
        }

        private void OnEliteSpecializationChanged(object sender, SpecializationChangedEventArgs e)
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
            SetTooltipSkill();
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);

            SetTooltipSkill();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            TemplatePresenter = null;
        }

        protected void SetTooltipSkill()
        {
            SkillTooltip.Skill = Skills.FirstOrDefault(x => x.Hovered)?.Skill;
        }
    }
}
