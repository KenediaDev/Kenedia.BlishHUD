using Blish_HUD;
using Blish_HUD.Controls;
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
    public class ThiefSpecifics : ProfessionSpecifics
    {
        //Trickery Trait -> +3 Innitiative

        //_initiative 156440
        //_noInitiative 156439
        private readonly DetailedTexture _barBackground = new(1636710); // black background?
        private readonly DetailedTexture _specterBar = new(2468316);
        private Rectangle _separatorBounds;
        private int _initiativeCount = 0;

        protected override SkillIcon[] Skills { get; } = {
            new(),
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
            new(156440),
            new(156440),
            new(156440),
        };

        public ThiefSpecifics(TemplatePresenter template, Data data) : base(template, data)
        {
            template.SpecializationChanged += Template_SpecializationChanged;
        }

        private void Template_SpecializationChanged(object sender, SpecializationChangedEventArgs e)
        {
            ApplyTemplate();
            RecalculateLayout();
            Invalidate();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int xOffset = 90;

            switch (TemplatePresenter?.Template?.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Antiquary:
                    xOffset = 55;
                    Skills[0].Bounds = new(xOffset + 3, 50, 42, 42);
                    _separatorBounds = new(Skills[0].Bounds.Right + 6, Skills[0].Bounds.Top, 2, Skills[0].Bounds.Height + 2);

                    for (int i = 1; i < 3; i++)
                    {
                        SkillIcon skill = Skills[i];
                        skill.Bounds = new(_separatorBounds.Right + ((i - 1) * 44) + 6, 50, 42, 42);
                    }

                    for (int i = 0; i < _initiativeCount; i++)
                    {
                        // Draw in a 3 row format
                        // The middle row is moved by 10 pixels to the right
                        var initiative = i < _initiative.Length ? _initiative[i] : null;
                        if (initiative is null) continue;

                        initiative.Bounds = new(xOffset + 200 + ((i / 3) * (_initiativeCount == 12 ? 26 : 20)) + (i % 3 == 1 ? (_initiativeCount == 12 ? 13 : 10) : 0), 40 + ((i % 3) * 15), 26, 26);
                    }

                    break;

                case (int)SpecializationType.Daredevil:
                    for (int i = 0; i < _initiativeCount; i++)
                    {
                        var initiative = i < _initiative.Length ? _initiative[i] : null;
                        if (initiative is null) continue;

                        initiative.Bounds = new(xOffset + (_initiativeCount  == 12 ? 100 : 80) + (i * 13), 55 - (i % 2 * 15), 26, 26);
                    }

                    Skills[0].Bounds = new(xOffset  - 20 + 3, 50, 42, 42);
                    Skills[1].Bounds = new(xOffset + 3 + 45, 50, 42, 42);
                    _barBackground.Bounds = new(xOffset + 90, 80, 170, 12);
                    _specterBar.Bounds = new(xOffset + 91, 81, 168, 10);
                    break;

                case (int)SpecializationType.Specter:
                    for (int i = 0; i < _initiativeCount; i++)
                    {
                        var initiative = i < _initiative.Length ? _initiative[i] : null;
                        if (initiative is null) continue;

                        initiative.Bounds = new(xOffset + 90 + (i * (_initiativeCount == 12 ? 13 : 10)), 55 - (i % 2 * 13), 26, 26);
                    }

                    Skills[0].Bounds = new(xOffset + 3, 50, 42, 42);
                    Skills[1].Bounds = new(xOffset + 3 + 45, 50, 42, 42);
                    _barBackground.Bounds = new(xOffset + 90, 80, 170, 12);
                    _specterBar.Bounds = new(xOffset + 91, 81, 168, 10);
                    break;

                default:
                    for (int i = 0; i < _initiativeCount; i++)
                    {
                        var initiative = i < _initiative.Length ? _initiative[i] : null;
                        if (initiative is null) continue;

                        initiative.Bounds = new(xOffset + (_initiativeCount == 12 ? 100 : 80) + (i * 13), 55 - (i % 2 * 15), 26, 26);
                    }

                    Skills[0].Bounds = new(xOffset + 3, 50, 42, 42);
                    break;
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            switch (TemplatePresenter.Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Antiquary:
                    for (int i = 0; i < 3; i++)
                    {
                        SkillIcon skill = Skills[i];
                        skill.Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    for (int i = 0; i < _initiativeCount; i++)
                    {
                        var initiative = i < _initiative.Length ? _initiative[i] : null;
                        if (initiative is null) continue;
                        initiative.Draw(this, spriteBatch);
                    }

                    spriteBatch.DrawOnCtrl(this, Textures.Pixel, _separatorBounds, Color.Black);
                    break;

                case (int)SpecializationType.Specter:
                    Skills[1].Draw(this, spriteBatch, RelativeMousePosition);
                    _barBackground.Draw(this, spriteBatch);
                    _specterBar.Draw(this, spriteBatch);

                    for (int i = 0; i < _initiativeCount; i++)
                    {
                        var initiative = i < _initiative.Length ? _initiative[i] : null;
                        if (initiative is null) continue;
                        initiative.Draw(this, spriteBatch);
                    }

                    spriteBatch.DrawStringOnCtrl(this, "100%", Content.DefaultFont12, _specterBar.Bounds, Color.White, false, HorizontalAlignment.Center, VerticalAlignment.Bottom);

                    break;

                default:
                    for (int i = 0; i < _initiativeCount; i++)
                    {
                        var initiative = i < _initiative.Length ? _initiative[i] : null;
                        if (initiative is null) continue;
                        initiative.Draw(this, spriteBatch);
                    }

                    break;
            }

            Skills[0].Draw(this, spriteBatch, RelativeMousePosition);
        }

        protected override void ApplyTemplate()
        {
            if (TemplatePresenter?.Template is null) return;
            if (!Data.IsLoaded) return;

            base.ApplyTemplate();

            var skills = Data?.Professions?[Gw2Sharp.Models.ProfessionType.Thief]?.Skills;
            if (skills is null) return;

            bool hasTrickery = TemplatePresenter?.Template?.Specializations.Any(x => x.Specialization?.Id == 44) ?? false;
            _initiativeCount = hasTrickery ? 15 : 12;

            Skill? GetSkill(SkillSlot slot)
            {
                Skill? skill = null;

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
                case (int)SpecializationType.Antiquary:
                    Skills[0].Skill = GetSkill(SkillSlot.Profession1);
                    Skills[1].Skill = null;
                    Skills[2].Skill = null;
                    break;

                default:
                    Skills[0].Skill = GetSkill(SkillSlot.Profession1);
                    Skills[1].Skill = GetSkill(SkillSlot.Profession2);
                    break;
            }

        }

        protected override void DisposeControl()
        {
            TemplatePresenter.SpecializationChanged -= Template_SpecializationChanged;
            base.DisposeControl();
        }
    }
}
