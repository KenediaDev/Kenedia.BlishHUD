using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Linq;
using static Blish_HUD.ContentService;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;

namespace Kenedia.Modules.BuildsManager.Controls.ProfessionSpecific
{
    public class EngineerSpecifics : ProfessionSpecifics
    {
        private readonly DetailedTexture _target = new(156812);
        private readonly DetailedTexture _return = new(156816);
        private readonly DetailedTexture _combatState = new(2572084);

        private Color _healthColor = new(162, 17, 11);
        private Rectangle _healthRectangle;
        private Enviroment Enviroment = Enviroment.Terrestrial;

        private readonly DetailedTexture _energyBg = new(1636718);
        private readonly DetailedTexture _energy = new(1636719);
        private readonly DetailedTexture _overheat = new(1636720);
        private Rectangle _separatorBounds;

        protected override SkillIcon[] Skills { get; } = {
            new(),
            new(),
            new(),
            new(),
            new(),
            new(),
        };

        public EngineerSpecifics(TemplatePresenter template, Data data) : base(template, data)
        {
            template.SkillChanged += Template_SkillChanged;
        }

        private void Template_SkillChanged(object sender, SkillChangedEventArgs e)
        {
            ApplyTemplate();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int xOffset = 90;

            Skills[0].TextureRegion = new(14, 14, 100, 100);
            switch (TemplatePresenter.Template?.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Holosmith:
                    for (int i = 0; i < 5; i++)
                    {
                        Skills[i].Bounds = new(xOffset + 20 + (i * 44) + (i == 4 ? 10 : 0), 36, 42, 42);
                    }

                    _energyBg.Bounds = new(xOffset + 5, 83, 250, 12);
                    _overheat.Bounds = new(xOffset + 5, 83, 250, 12);
                    _energy.TextureRegion = new(0, 0, _energy.Texture.Width / 3 * 2, _energy.Texture.Height);
                    _energy.Bounds = new(xOffset + 5, 83, 205, 12);
                    _energy.TextureRegion = new(0, 0, _energy.Texture.Width / 3 * 2, _energy.Texture.Height);
                    break;

                case (int)SpecializationType.Mechanist:
                    Skills[0].Bounds = new(xOffset + 175, 40, 56, 56);
                    _target.Bounds = new(xOffset + 5 + (0 * 34), 5, 32, 32);
                    _return.Bounds = new(xOffset + 5 + (1 * 34), 5, 32, 32);
                    _combatState.Bounds = new(xOffset + 5 + (2 * 34), 5, 32, 32);
                    _healthRectangle = new(xOffset + 5, 81, 170, 14);
                    for (int i = 1; i < 4; i++)
                    {
                        Skills[i].Bounds = new(xOffset + 15 - 44 + (i * 34), 40, 32, 32);
                    }

                    break;

                default:
                    for (int i = 0; i < 5; i++)
                    {
                        Skills[i].Bounds = new(xOffset + 30 + (i * 44), 55, 42, 42);
                    }

                    break;
            }

            if (TemplatePresenter.Template?.EliteSpecialization?.Id == (int)SpecializationType.Scrapper)
            {
                Skills[4].TextureRegion = new(6, 6, 51, 51);
            }

            var p = Skills[3].Bounds;
            _separatorBounds = new(p.Right + 6, p.Y, 2, p.Height);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            switch (TemplatePresenter.Template?.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Holosmith:
                    for (int i = 0; i < 5; i++)
                    {
                        Skills[i].Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    _energyBg.Draw(this, spriteBatch);
                    _overheat.Draw(this, spriteBatch);
                    _energy.Draw(this, spriteBatch);

                    spriteBatch.DrawOnCtrl(this, Textures.Pixel, _separatorBounds, Color.Black);

                    break;

                case (int)SpecializationType.Mechanist:
                    _target.Draw(this, spriteBatch);
                    _return.Draw(this, spriteBatch);
                    _combatState.Draw(this, spriteBatch);

                    for (int i = 0; i < 4; i++)
                    {
                        Skills[i].Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    Color borderColor = Color.Black;
                    Rectangle b = _healthRectangle;

                    spriteBatch.DrawOnCtrl(this, Textures.Pixel, _healthRectangle, Rectangle.Empty, _healthColor);

                    // Top
                    spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(b.Left, b.Top, b.Width, 1), Rectangle.Empty, borderColor * 0.6f);

                    // Bottom
                    spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(b.Left, b.Bottom - 1, b.Width, 1), Rectangle.Empty, borderColor * 0.6f);

                    // Left
                    spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(b.Left, b.Top, 1, b.Height), Rectangle.Empty, borderColor * 0.6f);

                    // Right
                    spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(b.Right - 1, b.Top, 1, b.Height), Rectangle.Empty, borderColor * 0.6f);

                    spriteBatch.DrawStringOnCtrl(this, "100%", GameService.Content.DefaultFont14, _healthRectangle, Color.White, false, HorizontalAlignment.Center, VerticalAlignment.Middle);
                    break;

                default:
                    for (int i = 0; i < 5; i++)
                    {
                        Skills[i].Draw(this, spriteBatch, RelativeMousePosition);
                    }
                    break;
            }

        }

        protected override void ApplyTemplate()
        {
            if (TemplatePresenter?.Template is null) return;
            if (!Data.IsLoaded) return;

            base.ApplyTemplate();

            var skills = Data?.Professions?[Gw2Sharp.Models.ProfessionType.Engineer]?.Skills;
            if (skills is null) return;

            Skill GetSkill(SkillSlot slot)
            {
                Skill skill = null;

                //foreach (var item in skills.Values.Where(e => e.Slot == slot))
                //{
                //    skill ??= item.Specialization == Template.EliteSpecialization?.Id || item.Specialization == 0 ? item : skill;
                //    if (item.Specialization == Template.EliteSpecialization?.Id && skill.Specialization == 0)
                //    {
                //        skill = item;
                //    }
                //}

                return skill;
            }

            Skill GetToolbeltSkill(SkillSlot slot)
            {
                Models.Templates.SkillSlotType state = Models.Templates.SkillSlotType.Active;
                Models.Templates.SkillSlotType enviroment = Models.Templates.SkillSlotType.Terrestrial;

                var buildSkills = TemplatePresenter.Template?.Skills.Where(e => e.Key.HasFlag(state | enviroment)).ToDictionary(e => e.Key, e => e.Value);

                switch (slot)
                {
                    case SkillSlot.Profession1:
                        if (buildSkills[state | enviroment | Models.Templates.SkillSlotType.Heal] is not null)
                        {
                            return buildSkills[state | enviroment | Models.Templates.SkillSlotType.Heal].ToolbeltSkill is not null && skills.TryGetValue((int)buildSkills[state | enviroment | Models.Templates.SkillSlotType.Heal].ToolbeltSkill, out Skill skill) ? skill : null;
                        }
                        break;

                    case SkillSlot.Profession2:
                        if (buildSkills[state | enviroment | Models.Templates.SkillSlotType.Utility_1] is not null)
                        {
                            return buildSkills[state | enviroment | Models.Templates.SkillSlotType.Utility_1].ToolbeltSkill is not null && skills.TryGetValue((int)buildSkills[state | enviroment | Models.Templates.SkillSlotType.Utility_1].ToolbeltSkill, out Skill skill) ? skill : null;
                        }
                        break;

                    case SkillSlot.Profession3:
                        if (buildSkills[state | enviroment | Models.Templates.SkillSlotType.Utility_2] is not null)
                        {
                            return buildSkills[state | enviroment | Models.Templates.SkillSlotType.Utility_2].ToolbeltSkill is not null && skills.TryGetValue((int)buildSkills[state | enviroment | Models.Templates.SkillSlotType.Utility_2].ToolbeltSkill, out Skill skill) ? skill : null;
                        }
                        break;

                    case SkillSlot.Profession4:
                        if (buildSkills[state | enviroment | Models.Templates.SkillSlotType.Utility_3] is not null)
                        {
                            return buildSkills[state | enviroment | Models.Templates.SkillSlotType.Utility_3].ToolbeltSkill is not null && skills.TryGetValue((int)buildSkills[state | enviroment | Models.Templates.SkillSlotType.Utility_3].ToolbeltSkill, out Skill skill) ? skill : null;
                        }
                        break;

                    case SkillSlot.Profession5:
                        if (buildSkills[state | enviroment | Models.Templates.SkillSlotType.Elite] is not null)
                        {
                            return buildSkills[state | enviroment | Models.Templates.SkillSlotType.Elite].ToolbeltSkill is not null && skills.TryGetValue((int)buildSkills[state | enviroment | Models.Templates.SkillSlotType.Elite].ToolbeltSkill, out Skill skill) ? skill : null;
                        }
                        break;
                }

                return null;
            }

            switch (TemplatePresenter.Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Mechanist:
                    Skills[0].Skill = Enviroment == Enviroment.Terrestrial ? skills[63089] : skills[63210];

                    int adeptSkill = TemplatePresenter.Template.Specializations.Specialization3.Traits.Adept?.Skills?.FirstOrDefault() ?? 0;
                    int masterSkill = TemplatePresenter.Template.Specializations.Specialization3.Traits.Master?.Skills?.FirstOrDefault() ?? 0;
                    int  grandmasterSkill = TemplatePresenter.Template.Specializations.Specialization3.Traits.GrandMaster?.Skills?.FirstOrDefault() ?? 0;

                    Skills[1].Skill = skills.Values.FirstOrDefault(e => e.Id == adeptSkill);
                    Skills[2].Skill = skills.Values.FirstOrDefault(e => e.Id == masterSkill);
                    Skills[3].Skill = skills.Values.FirstOrDefault(e => e.Id == grandmasterSkill);

                    break;

                case (int)SpecializationType.Scrapper:
                    Skills[4].Skill = skills.Values.FirstOrDefault(e => e.Id == 56920);
                    break;

                case (int)SpecializationType.Holosmith:
                    Skills[4].Skill = skills.Values.FirstOrDefault(e => e.Id == 42938);
                    break;

                default:
                    Skills[4].Skill = GetToolbeltSkill(SkillSlot.Profession5);
                    break;
            }

            if (TemplatePresenter.Template.EliteSpecialization?.Id != (int)SpecializationType.Mechanist)
            {
                Skills[0].Skill = GetToolbeltSkill(SkillSlot.Profession1);
                Skills[1].Skill = GetToolbeltSkill(SkillSlot.Profession2);
                Skills[2].Skill = GetToolbeltSkill(SkillSlot.Profession3);
                Skills[3].Skill = GetToolbeltSkill(SkillSlot.Profession4);
            }
        }
    }
}
