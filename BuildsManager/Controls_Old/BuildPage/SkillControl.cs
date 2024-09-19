using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Kenedia.Modules.BuildsManager.Controls_Old.BuildPage
{
    public class SkillControl : Control
    {
        private readonly SkillIcon _skillIcon = new() { ShowSelector = true };
        private Skill _skill;
        private TemplatePresenter _templatePresenter;

        public SkillControl()
        {
            Tooltip = new SkillTooltip();
        }

        //public Skill Skill { get => _skill; set => Common.SetProperty(ref _skill, value, ApplySkill); }

        public Skill Skill
        {
            get => TemplatePresenter?.Template?.Skills[Slot];
            set
            {
                if (TemplatePresenter?.Template != null)
                {
                    TemplatePresenter.Template.Skills[Slot] = value;
                }
            }
        }

        public SkillSlotType Slot { get => _skillIcon.Slot; set => _skillIcon.Slot = value; }

        public bool ShowSelector { get => _skillIcon.ShowSelector; set => _skillIcon.ShowSelector = value; }

        public bool IsSelectorHovered => ShowSelector && _skillIcon.Selector.Hovered;

        public TemplatePresenter TemplatePresenter { get => _templatePresenter; set => Common.SetProperty(ref _templatePresenter, value, OnTemplatePresenterChanged); }

        private void OnTemplatePresenterChanged(object sender, ValueChangedEventArgs<TemplatePresenter> e)
        {
            if (e.OldValue != null)
            {
                e.OldValue.SkillChanged_OLD -= Skill_Changed;
            }

            if (e.NewValue != null)
            {
                e.NewValue.SkillChanged_OLD += Skill_Changed;
            }
        }

        private void Skill_Changed(object sender, DictionaryItemChangedEventArgs<SkillSlotType, Skill> e)
        {
            if (e.Key == Slot)
            {
                ApplySkill(this, new(e.NewValue, e.OldValue));
            }
        }

        private void ApplySkill(object sender, Core.Models.ValueChangedEventArgs<Skill> e)
        {
            _skillIcon.Skill = Skill;

            if (Tooltip is SkillTooltip tooltip)
            {
                tooltip.Skill = Skill;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _skillIcon.Draw(this, spriteBatch, Slot.HasFlag(SkillSlotType.Terrestrial), MouseOver ? RelativeMousePosition : null);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int selectorHeight = 15;
            _skillIcon.Selector.Bounds = new(Point.Zero, new(Width, selectorHeight));
            _skillIcon.Bounds = new(new(0, selectorHeight - 2), new(Width, Height - selectorHeight));
        }
    }
}
