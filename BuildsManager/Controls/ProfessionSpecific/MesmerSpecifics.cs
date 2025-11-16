using Blish_HUD;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using static Blish_HUD.ContentService;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;

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

        private readonly DetailedTexture _notesRegionBackground = new(3680685);

        private readonly DetailedTexture[] _notes = {
            new(3680686),
            new(3680688),
            new(3680690),
        };

        private readonly DetailedTexture[] _notesBackground = {
            new(3680687),
            new(3680689),
            new(3680691),
        };

        private Color _notesColor = new(198, 100, 231);

        private Rectangle _separatorBounds;

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

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int xOffset = 70;

            switch (TemplatePresenter?.Template?.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Troubadour:
                    _notesRegionBackground.Bounds = new(xOffset + 85, 0, 125, 42);

                    _notesBackground[0].Bounds = new(xOffset + 95, 6, 36, 36);
                    _notes[0].Bounds = new(xOffset + 95, 6, 36, 36);

                    _notesBackground[1].Bounds = new(xOffset + 135, 0, 36, 36);
                    _notes[1].Bounds = new(xOffset + 135, 0, 36, 36);

                    _notesBackground[2].Bounds = new(xOffset + 165, 2, 36, 36);
                    _notes[2].Bounds = new(xOffset + 165, 2, 36, 36);

                    break;
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
                case (int)SpecializationType.Troubadour:
                    _notesRegionBackground.Draw(this, spriteBatch);

                    foreach (var noteBg in _notesBackground)
                    {
                        noteBg.Draw(this, spriteBatch, color: Color.Black);
                    }

                    foreach (var note in _notes)
                    {
                        note.Draw(this, spriteBatch, color: _notesColor);
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        var skill = Skills[i];
                        skill.Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    spriteBatch.DrawOnCtrl(this, Textures.Pixel, _separatorBounds, Color.Black);
                    break;

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

            switch (TemplatePresenter.Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Troubadour:
                    Skills[4].Skill = skills.Get(76931);
                    break;
            }
        }
    }
}
