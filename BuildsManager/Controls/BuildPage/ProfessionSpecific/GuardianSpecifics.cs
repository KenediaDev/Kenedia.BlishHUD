using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class GuardianSpecifics : ProfessionSpecifics
    {
        private readonly Dictionary<int, DetailedTexture> _fivePages = new()
        {
            { 0, new (1636723)},
            { 1, new (1636724)},
            { 2, new (1636725)},
            { 3, new (1636726)},
            { 4, new (1636727)},
            { 5, new (1636728)},
        };
        private readonly Dictionary<int, DetailedTexture> _eightPages = new()
        {
            { 0, new (1636729)},
            { 1, new (1636730)},
            { 2, new (1636731)},
            { 3, new (1636732)},
            { 4, new (1636733)},
            { 5, new (1636734)},
            { 6, new (1636735)},
            { 7, new (1636736)},
            { 8, new (1636737)},
        };

        private readonly DetailedTexture _pagesBackground = new(1636722);
        private readonly DetailedTexture _pages = new(1636728);
        private readonly SkillIcon[] _skills = {
            new(),
            new(),
            new(),
        };

        public GuardianSpecifics(TemplatePresenter template) : base(template)
        {
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int xOffset = 90;

            switch (TemplatePresenter.Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Firebrand:
                    _pagesBackground.Bounds = new(xOffset + 10, 50, 256, 64);
                    _pages.Bounds = new(xOffset + 125, 50, 140, 44);
                    for (int i = 0; i < _skills.Length; i++)
                    {
                        _skills[i].Bounds = new(xOffset + 3 + (i * 40), 53, 38, 38);
                    }

                    break;

                default:
                    for (int i = 0; i < _skills.Length; i++)
                    {
                        _skills[i].Bounds = new(xOffset + 100 + (i * 42), 56, 42, 42);
                    }

                    break;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            switch (TemplatePresenter.Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Firebrand:
                    _pagesBackground.Draw(this, spriteBatch);
                    _pages.Draw(this, spriteBatch);
                    break;
            }

            for (int i = 0; i < _skills.Length; i++)
            {
                _skills[i].Draw(this, spriteBatch, RelativeMousePosition);
            }
        }

        protected override void ApplyTemplate()
        {
            var skills = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Guardian].Skills;

            Skill GetSkill(SkillSlot slot)
            {
                Skill skill = null;

                foreach (var item in skills.Values.Where(e => e.Slot == slot))
                {
                    skill ??= item.Specialization == TemplatePresenter.Template.EliteSpecialization?.Id || item.Specialization == 0 ? item : skill;
                    if (item.Specialization == TemplatePresenter.Template.EliteSpecialization?.Id && skill.Specialization == 0)
                    {
                        skill = item;
                    }
                }

                return skill;
            }

            _skills[0].Skill = GetSkill(SkillSlot.Profession1);
            _skills[1].Skill = GetSkill(SkillSlot.Profession2);
            _skills[2].Skill = GetSkill(SkillSlot.Profession3);

            base.ApplyTemplate();
        }
    }
}
