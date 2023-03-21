using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using static Blish_HUD.ContentService;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class EngineerSpecifics : ProfessionSpecifics
    {
        //TODO find and Add Separator Line
        private readonly DetailedTexture _target = new(156812);
        private readonly DetailedTexture _return = new(156816);
        private readonly DetailedTexture _combatState = new(2572084);

        private Color _healthColor = new(162, 17, 11);
        private Rectangle _healthRectangle;

        private readonly DetailedTexture _energyBg = new(1636718);
        private readonly DetailedTexture _energy = new(1636719);
        private readonly DetailedTexture _overheat = new(1636720);
        private readonly SkillIcon[] _skills = {
            new(),
            new(),
            new(),
            new(),
            new(),
        };

        public EngineerSpecifics()
        {

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int xOffset = 90;

            _skills[0].TextureRegion = new(14, 14, 100, 100);
            switch (Template?.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Holosmith:
                    for (int i = 0; i < 5; i++)
                    {
                        _skills[i].Bounds = new(xOffset + 20 + (i * 44) + (i == 4 ? 10 : 0), 36, 42, 42);
                    }

                    _energyBg.Bounds = new(xOffset + 5, 83, 250, 12);
                    _overheat.Bounds = new(xOffset + 5, 83, 250, 12);
                    _energy.TextureRegion = new(0, 0, _energy.Texture.Width / 3 * 2, _energy.Texture.Height);
                    _energy.Bounds = new(xOffset + 5, 83, 205, 12);
                    _energy.TextureRegion = new(0, 0, _energy.Texture.Width / 3 * 2, _energy.Texture.Height);
                    break;

                case (int)SpecializationType.Mechanist:
                    _skills[0].Bounds = new(xOffset + 175, 40, 56, 56);
                    _target.Bounds = new(xOffset + 5 + (0 * 34), 5, 32, 32);
                    _return.Bounds = new(xOffset + 5 + (1 * 34), 5, 32, 32);
                    _combatState.Bounds = new(xOffset + 5 + (2 * 34), 5, 32, 32);
                    _healthRectangle = new(xOffset + 5, 81, 170, 14);
                    for (int i = 1; i < 4; i++)
                    {
                        _skills[i].Bounds = new(xOffset + 15 - 44 + (i * 34), 40, 32, 32);
                    }

                    break;

                default:
                    for (int i = 0; i < 5; i++)
                    {
                        _skills[i].Bounds = new(xOffset + 30 + (i * 44), 55, 42, 42);
                    }

                    break;
            }

            if (Template?.EliteSpecialization?.Id == (int)SpecializationType.Scrapper)
            {
                _skills[4].TextureRegion = new(6, 6, 51, 51);
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            RecalculateLayout();

            switch (Template?.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Holosmith:
                    for (int i = 0; i < 5; i++)
                    {
                        _skills[i].Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    _energyBg.Draw(this, spriteBatch);
                    _overheat.Draw(this, spriteBatch);
                    _energy.Draw(this, spriteBatch);

                    break;

                case (int)SpecializationType.Mechanist:
                    _target.Draw(this, spriteBatch);
                    _return.Draw(this, spriteBatch);
                    _combatState.Draw(this, spriteBatch);

                    for (int i = 0; i < 4; i++)
                    {
                        _skills[i].Draw(this, spriteBatch, RelativeMousePosition);
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
                        _skills[i].Draw(this, spriteBatch, RelativeMousePosition);
                    }
                    break;
            }

        }

        protected override void ApplyTemplate()
        {
            var skills = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Engineer].Skills;

            Skill GetSkill(SkillSlot slot)
            {
                Skill skill = null;

                foreach (var item in skills.Values.Where(e => e.Slot == slot))
                {
                    skill ??= item.Specialization == Template.EliteSpecialization?.Id || item.Specialization == 0 ? item : skill;
                    if (item.Specialization == Template.EliteSpecialization?.Id && skill.Specialization == 0)
                    {
                        skill = item;
                    }
                }

                return skill;
            }

            Skill GetToolbeltSkill(SkillSlot slot)
            {
                var buildSkills = Template.GetActiveSkills();

                switch (slot)
                {
                    case SkillSlot.Profession1:
                        if (buildSkills[BuildSkillSlot.Heal] != null)
                        {
                            return buildSkills[BuildSkillSlot.Heal].ToolbeltSkill != null && skills.TryGetValue((int)buildSkills[BuildSkillSlot.Heal].ToolbeltSkill, out Skill skill) ? skill : null;
                        }
                        break;

                    case SkillSlot.Profession2:
                        if (buildSkills[BuildSkillSlot.Utility_1] != null)
                        {
                            return buildSkills[BuildSkillSlot.Utility_1].ToolbeltSkill != null && skills.TryGetValue((int)buildSkills[BuildSkillSlot.Utility_1].ToolbeltSkill, out Skill skill) ? skill : null;
                        }
                        break;

                    case SkillSlot.Profession3:
                        if (buildSkills[BuildSkillSlot.Utility_2] != null)
                        {
                            return buildSkills[BuildSkillSlot.Utility_2].ToolbeltSkill != null && skills.TryGetValue((int)buildSkills[BuildSkillSlot.Utility_2].ToolbeltSkill, out Skill skill) ? skill : null;
                        }
                        break;

                    case SkillSlot.Profession4:
                        if (buildSkills[BuildSkillSlot.Utility_3] != null)
                        {
                            return buildSkills[BuildSkillSlot.Utility_3].ToolbeltSkill != null && skills.TryGetValue((int)buildSkills[BuildSkillSlot.Utility_3].ToolbeltSkill, out Skill skill) ? skill : null;
                        }
                        break;

                    case SkillSlot.Profession5:
                        if (buildSkills[BuildSkillSlot.Elite] != null)
                        {
                            return buildSkills[BuildSkillSlot.Elite].ToolbeltSkill != null && skills.TryGetValue((int)buildSkills[BuildSkillSlot.Elite].ToolbeltSkill, out Skill skill) ? skill : null;
                        }
                        break;
                }

                return null;
            }

            switch (Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Mechanist:
                    _skills[0].Skill = skills[63089];
                    //TODO add Mech Skills

                    //_skills[1].Skill = GetSkill(SkillSlot.Profession2);
                    //_skills[2].Skill = GetSkill(SkillSlot.Profession3);
                    //_skills[3].Skill = GetSkill(SkillSlot.Profession4);
                    //_skills[4].Skill = GetSkill(SkillSlot.Profession5);
                    break;

                case (int)SpecializationType.Scrapper:
                    _skills[4].Skill = skills[56920];
                    break;

                case (int)SpecializationType.Holosmith:
                    _skills[4].Skill = skills[42938];
                    break;

                default:
                    _skills[4].Skill = GetToolbeltSkill(SkillSlot.Profession5);
                    break;
            }

            if (Template.EliteSpecialization?.Id != (int)SpecializationType.Mechanist)
            {
                _skills[0].Skill = GetToolbeltSkill(SkillSlot.Profession1);
                _skills[1].Skill = GetToolbeltSkill(SkillSlot.Profession2);
                _skills[2].Skill = GetToolbeltSkill(SkillSlot.Profession3);
                _skills[3].Skill = GetToolbeltSkill(SkillSlot.Profession4);
            }

            base.ApplyTemplate();
        }
    }
}
