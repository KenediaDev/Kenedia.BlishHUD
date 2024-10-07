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
        protected virtual SkillIcon[] Skills { get; } = Array.Empty<SkillIcon>();

        public ProfessionSpecifics(TemplatePresenter templatePresenter)
        {
            TemplatePresenter = templatePresenter;
            ClipsBounds = false;
            ZIndex = int.MaxValue / 2;

            Tooltip = SkillTooltip = new();

            SetTemplatePresenter();
            ApplyTemplate();
        }

        protected SkillTooltip SkillTooltip { get; }

        public TemplatePresenter TemplatePresenter { get; }

        private void SetTemplatePresenter()
        {
            if (TemplatePresenter is not null)
            {
                TemplatePresenter.LegendChanged += OnLegendChanged;
                TemplatePresenter.EliteSpecializationChanged += OnEliteSpecializationChanged;
                TemplatePresenter.TemplateChanged += OnTemplateChanged;
                TemplatePresenter.TraitChanged += OnTraitChanged;
                TemplatePresenter.SkillChanged += OnSkillChanged;
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

            if (TemplatePresenter is not null)
            {
                TemplatePresenter.LegendChanged -= OnLegendChanged;
                TemplatePresenter.EliteSpecializationChanged -= OnEliteSpecializationChanged;
                TemplatePresenter.TemplateChanged -= OnTemplateChanged;
                TemplatePresenter.TraitChanged -= OnTraitChanged;
                TemplatePresenter.SkillChanged -= OnSkillChanged;
            }
        }

        protected void SetTooltipSkill()
        {
            SkillTooltip.Skill = Skills.FirstOrDefault(x => x.Hovered)?.Skill;
        }
    }
}
