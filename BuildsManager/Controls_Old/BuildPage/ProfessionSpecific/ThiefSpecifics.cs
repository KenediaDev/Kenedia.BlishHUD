using Blish_HUD;
using Blish_HUD.Controls;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Controls_Old.BuildPage.ProfessionSpecific
{
    public class ThiefSpecifics : ProfessionSpecifics
    {
        //_initiative 156440
        //_noInitiative 156439
        private readonly DetailedTexture _barBackground = new(1636710); // black background?
        private readonly DetailedTexture _specterBar = new(2468316);
        private readonly SkillIcon[] _skills =
        {
            new(),
            new(),
        };

        private readonly DetailedTexture[] _initiative =
        {
            new(156440),
            new(156440),
            new(156440),
            new(156440),
            new(156440),
            new(156440),
            new(156440),
            new(156440),
            new(156440),
            new(156440),
            new(156440),
            new(156440),
        };

        public ThiefSpecifics(TemplatePresenter template) : base(template)
        {

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int xOffset = 90;

            switch (TemplatePresenter?.Template?.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Specter:
                    for (int i = 0; i < _initiative.Length; i++)
                    {
                        _initiative[i].Bounds = new(xOffset + 90 + (i * 18), 55 - (i % 2 * 13), 26, 26);
                    }

                    _skills[0].Bounds = new(xOffset + 3, 50, 42, 42);
                    _skills[1].Bounds = new(xOffset + 3 + 45, 50, 42, 42);
                    _barBackground.Bounds = new(xOffset + 90, 80, 170, 12);
                    _specterBar.Bounds = new(xOffset + 91, 81, 168, 10);
                    break;

                default:
                    for (int i = 0; i < _initiative.Length; i++)
                    {
                        _initiative[i].Bounds = new(xOffset + 90 + (i * 13), 63 - (i % 2 * 13), 26, 26);
                    }

                    _skills[0].Bounds = new(xOffset + 3, 50, 42, 42);
                    break;
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
            switch (TemplatePresenter.Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Specter:
                    _skills[1].Draw(this, spriteBatch, RelativeMousePosition);
                    _barBackground.Draw(this, spriteBatch);
                    _specterBar.Draw(this, spriteBatch);

                    for (int i = 0; i < 9; i++)
                    {
                        _initiative[i].Draw(this, spriteBatch);
                    }
                    spriteBatch.DrawStringOnCtrl(this, "100%", Content.DefaultFont12, _specterBar.Bounds, Color.White, false, HorizontalAlignment.Center, VerticalAlignment.Bottom);

                    break;

                default:
                    for (int i = 0; i < _initiative.Length; i++)
                    {
                        _initiative[i].Draw(this, spriteBatch);
                    }
                    break;
            }

            _skills[0].Draw(this, spriteBatch, RelativeMousePosition);
        }

        protected override void ApplyTemplate()
        {
            if (TemplatePresenter?.Template is null) return;
            base.ApplyTemplate();

            var skills = BuildsManager.Data?.Professions?[Gw2Sharp.Models.ProfessionType.Thief]?.Skills;
            if (skills is null) return;

            Skill GetSkill(SkillSlot slot)
            {
                Skill skill = null;

                foreach (var item in skills.Values.Where(e => e.Slot == slot))
                {
                    skill ??= item.Specialization == TemplatePresenter?.Template?.EliteSpecialization?.Id || item.Specialization == 0 ? item : skill;
                    if (item.Specialization == TemplatePresenter?.Template?.EliteSpecialization?.Id && skill.Specialization == 0)
                    {
                        skill = item;
                    }
                }

                return skill;
            }

            switch (TemplatePresenter?.Template?.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Specter:

                    break;
            }

            _skills[0].Skill = GetSkill(SkillSlot.Profession1);
            _skills[1].Skill = GetSkill(SkillSlot.Profession2);
        }
    }
}
