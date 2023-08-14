using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    //TODO Memser Utility Skills are wonky af
    public class MesmerSpecifics : ProfessionSpecifics
    {
        //156430 One Clone
        //156429 No Clone
        private readonly DetailedTexture[] _clones =
        {
            new(156430),
            new(156430),
            new(156430),
            new(156429),
            new(156429),
        };

        // TODO find and add black separator line

        private readonly SkillIcon[] _skills =
        {
            new(),
            new(),
            new(),
            new(),
            new(),
        };

        public MesmerSpecifics(TemplatePresenter template) : base(template)
        {

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int xOffset = 70;

            switch (TemplatePresenter?.Template?.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Virtuoso:
                    for (int i = 0; i < _clones.Length; i++)
                    {
                        var clone = _clones[i];
                        clone.Bounds = new(xOffset + 90 + (i * 24), 24, 30, 30);
                    }
                    break;

                default:
                    for (int i = 0; i < 3; i++)
                    {
                        var clone = _clones[i];
                        clone.Bounds = new(xOffset + 80 + (i * 32), 12, 42, 42);
                    }

                    break;
            }

            for (int i = 0; i < _skills.Length; i++)
            {
                var skill = _skills[i];
                skill.Bounds = new(xOffset + (i == 4 ? 10 : 0) + 42 + (i * 46), 52, 42, 42);
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            switch (TemplatePresenter.Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Virtuoso:
                    for (int i = 0; i < _clones.Length; i++)
                    {
                        var clone = _clones[i];
                        clone.Draw(this, spriteBatch);
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        var skill = _skills[i];
                        skill.Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    break;

                case (int)SpecializationType.Chronomancer:
                    for (int i = 0; i < 3; i++)
                    {
                        var clone = _clones[i];
                        clone.Draw(this, spriteBatch);
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        var skill = _skills[i];
                        skill.Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    break;

                default:
                    for (int i = 0; i < 3; i++)
                    {
                        var clone = _clones[i];
                        clone.Draw(this, spriteBatch);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        var skill = _skills[i];
                        skill.Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    break;
            }
        }

        protected override void ApplyTemplate()
        {
            base.ApplyTemplate();

            var skills = BuildsManager.Data?.Professions?[Gw2Sharp.Models.ProfessionType.Mesmer]?.Skills;
            if (skills is null) return;

            Skill GetSkill(SkillSlot slot)
            {
                Skill skill = null;

                foreach(var item in skills.Values.Where(e => e.Slot == slot))
                {
                    skill ??= item.Specialization == TemplatePresenter?.Template?.EliteSpecialization?.Id || item.Specialization == 0 ? item : skill;
                }

                return skill;
            }

            _skills[0].Skill = GetSkill(SkillSlot.Profession1);
            _skills[1].Skill = GetSkill(SkillSlot.Profession2);
            _skills[2].Skill = GetSkill(SkillSlot.Profession3);
            _skills[3].Skill = GetSkill(SkillSlot.Profession4);
            _skills[4].Skill = GetSkill(SkillSlot.Profession5);
        }
    }
}
