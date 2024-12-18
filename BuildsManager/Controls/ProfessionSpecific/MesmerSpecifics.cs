﻿using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;
using Blish_HUD;
using static Blish_HUD.ContentService;
using Kenedia.Modules.BuildsManager.Services;

namespace Kenedia.Modules.BuildsManager.Controls.ProfessionSpecific
{
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

        protected override SkillIcon[] Skills { get; } = {
            new(),
            new(),
            new(),
            new(),
            new(),
        };

        public MesmerSpecifics(TemplatePresenter template, Data data) : base(template, data)
        {

        }

        private Rectangle _separatorBounds;

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

            for (int i = 0; i < Skills.Length; i++)
            {
                var skill = Skills[i];
                skill.Bounds = new(xOffset + (i == 4 ? 10 : 0) + 42 + (i * 46), 52, 42, 42);
            }

            var p = Skills[3].Bounds;
            _separatorBounds = new(p.Right + 6, p.Y, 2, p.Height);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
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
                        var skill = Skills[i];
                        skill.Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    spriteBatch.DrawOnCtrl(this, Textures.Pixel, _separatorBounds, Color.Black);

                    break;

                case (int)SpecializationType.Chronomancer:
                    for (int i = 0; i < 3; i++)
                    {
                        var clone = _clones[i];
                        clone.Draw(this, spriteBatch);
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        var skill = Skills[i];
                        skill.Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    spriteBatch.DrawOnCtrl(this, Textures.Pixel, _separatorBounds, Color.Black);
                    break;

                default:
                    for (int i = 0; i < 3; i++)
                    {
                        var clone = _clones[i];
                        clone.Draw(this, spriteBatch);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        var skill = Skills[i];
                        skill.Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    break;
            }
        }

        protected override void ApplyTemplate()
        {
            if (TemplatePresenter?.Template is null) return;
            if (!Data.IsLoaded) return;

            base.ApplyTemplate();

            var skills = Data?.Professions?[Gw2Sharp.Models.ProfessionType.Mesmer]?.Skills;
            if (skills is null) return;

            Skill GetSkill(SkillSlot slot)
            {
                Skill skill = null;
                var slotSkills = skills.Values.Where(e => e.Slot == slot);
                var eliteSpecSkills = (TemplatePresenter?.Template?.EliteSpecialization?.Id  ?? 0 )!= 0 ? slotSkills.Where(x => x.Specialization == TemplatePresenter?.Template?.EliteSpecialization?.Id) : [];

                foreach (var item in eliteSpecSkills)
                {
                    skill ??= item.Specialization == TemplatePresenter?.Template?.EliteSpecialization?.Id || item.Specialization == 0 ? item : skill;
                }

                if (skill is not null)
                    return skill;

                foreach(var item in skills.Values.Where(e => e.Slot == slot))
                {
                    skill ??= item.Specialization == TemplatePresenter?.Template?.EliteSpecialization?.Id || item.Specialization == 0 ? item : skill;
                }

                return skill;
            }

            Skills[0].Skill = GetSkill(SkillSlot.Profession1);
            Skills[1].Skill = GetSkill(SkillSlot.Profession2);
            Skills[2].Skill = GetSkill(SkillSlot.Profession3);
            Skills[3].Skill = GetSkill(SkillSlot.Profession4);
            Skills[4].Skill = GetSkill(SkillSlot.Profession5);
        }
    }
}
