using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.DataModels;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Professions;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class WarriorSpecifics : ProfessionSpecifics
    {
        //F:\Guild Wars 2 Assets\Textures\UI Textures\256x64
        private readonly DetailedTexture _emptyAdrenalin = new(156441);
        private readonly DetailedTexture _adrenalin1 = new(156442);
        private readonly DetailedTexture _adrenalin2 = new(156443);
        private readonly DetailedTexture _adrenalin3 = new(156444);

        private readonly DetailedTexture _barBackground = new(1636710); // black background?
        private readonly DetailedTexture _bladeswornCharges = new(2492047, 2492048);
        private readonly DetailedTexture[] _charges =
        {
            new(2492048),
            new(2492048),
            new(2492048),
            new(2492048),
            new(2492048),
            new(2492047),
            new(2492047),
            new(2492047),
            new(2492047),
            new(2492047),
        };

        private readonly SkillIcon[] _skills = {
            new(),
            new(),
        };

        public WarriorSpecifics()
        {

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            int xOffset = 90;

            switch (Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Bladesworn:
                    for (int i = 0; i < _charges.Length; i++)
                    {
                        _charges[i].Bounds = new(xOffset + 90 + 1 + (i * 17), 57, 12, 24);
                    }
                    _skills[0].Bounds = new(xOffset, 56, 42, 42);
                    _skills[1].Bounds = new(xOffset + 44, 56, 42, 42);
                    _barBackground.Bounds = new(xOffset + 90, 83, 165, 14);

                    break;

                case (int)SpecializationType.Berserker:
                    _emptyAdrenalin.Bounds = new(xOffset + 3, 80, 163, 14);
                    _emptyAdrenalin.TextureRegion = new(0, 25, 217, 14);

                    _adrenalin1.Bounds = new(xOffset + 3, 80, 163, 14);
                    _adrenalin1.TextureRegion = new(0, 25, 217, 14);

                    _adrenalin2.Bounds = new(xOffset + 3, 80, 163, 14);
                    _adrenalin2.TextureRegion = new(0, 25, 217, 14);

                    _adrenalin3.Bounds = new(xOffset + 3, 80, 163, 14);
                    _adrenalin3.TextureRegion = new(0, 25, 217, 14);

                    _skills[0].Bounds = new(xOffset + 211, 56, 42, 42);
                    _skills[1].Bounds = new(xOffset + 211 - 45, 56, 42, 42);
                    break;

                case (int)SpecializationType.Spellbreaker:
                    _emptyAdrenalin.Bounds = new(xOffset + 3, 80, 163, 14);
                    _emptyAdrenalin.TextureRegion = new(0, 25, 145, 14);

                    _adrenalin1.Bounds = new(xOffset + 3, 80, 163, 14);
                    _adrenalin1.TextureRegion = new(0, 25, 145, 14);

                    _adrenalin2.Bounds = new(xOffset + 3, 80, 163, 14);
                    _adrenalin2.TextureRegion = new(0, 25, 145, 14);

                    _skills[0].Bounds = new(xOffset + 211, 56, 42, 42);
                    _skills[1].Bounds = new(xOffset + 211 - 45, 56, 42, 42);
                    break;

                default:
                    _emptyAdrenalin.Bounds = new(xOffset + 3, 80, 208, 14);
                    _emptyAdrenalin.TextureRegion = new(0, 25, 217, 14);

                    _adrenalin1.Bounds = new(xOffset + 3, 80, 208, 14);
                    _adrenalin1.TextureRegion = new(0, 25, 217, 14);

                    _adrenalin2.Bounds = new(xOffset + 3, 80, 208, 14);
                    _adrenalin2.TextureRegion = new(0, 25, 217, 14);

                    _adrenalin3.Bounds = new(xOffset + 3, 80, 208, 14);
                    _adrenalin3.TextureRegion = new(0, 25, 217, 14);

                    _skills[0].Bounds = new(xOffset + 211, 56, 42, 42);
                    break;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            switch (Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Bladesworn:
                    _barBackground.Draw(this, spriteBatch);
                    _skills[0].Draw(this, spriteBatch, RelativeMousePosition);
                    _skills[1].Draw(this, spriteBatch, RelativeMousePosition);

                    for (int i = 0; i < _charges.Length; i++)
                    {
                        _charges[i].Draw(this, spriteBatch);
                    }

                    break;

                case (int)SpecializationType.Spellbreaker:
                    _skills[0].Draw(this, spriteBatch, RelativeMousePosition);
                    if (_skills[1].Hovered)
                    {
                        //Debug.WriteLine($"{_skills[1].Skill.Name}");
                    }
                    _skills[1].Draw(this, spriteBatch, RelativeMousePosition);
                    _emptyAdrenalin.Draw(this, spriteBatch);
                    _adrenalin1.Draw(this, spriteBatch);
                    _adrenalin2.Draw(this, spriteBatch);
                    break;

                case (int)SpecializationType.Berserker:
                    _skills[0].Draw(this, spriteBatch, RelativeMousePosition);
                    _skills[1].Draw(this, spriteBatch, RelativeMousePosition);
                    _emptyAdrenalin.Draw(this, spriteBatch);
                    _adrenalin1.Draw(this, spriteBatch);
                    _adrenalin2.Draw(this, spriteBatch);
                    _adrenalin3.Draw(this, spriteBatch);
                    break;

                default:
                    _skills[0].Draw(this, spriteBatch, RelativeMousePosition);
                    _emptyAdrenalin.Draw(this, spriteBatch);
                    _adrenalin1.Draw(this, spriteBatch);
                    _adrenalin2.Draw(this, spriteBatch);
                    _adrenalin3.Draw(this, spriteBatch);
                    break;
            }
        }

        protected override void ApplyTemplate()
        {
            var skills = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Warrior].Skills;

            Skill GetSkill(SkillSlot slot)
            {
                Skill skill = null;
                bool spellbreaker = Template.EliteSpecialization?.Id == (int)SpecializationType.Spellbreaker;
                bool berserker = Template.EliteSpecialization?.Id == (int)SpecializationType.Berserker;
                if (spellbreaker && slot == SkillSlot.Profession2)
                {
                    return skills[44165];
                }

                foreach (var item in skills.Values.Where(
                    e => e.Slot == slot &&                     
                    e.WeaponType != null && ((!spellbreaker && e.WeaponType == Weapon.WeaponType.None) || e.WeaponType == (Template.Terrestrial ? Template.GearTemplate.Weapons[GearTemplateSlot.MainHand].Weapon : Template.GearTemplate.Weapons[GearTemplateSlot.Aquatic].Weapon))))
                {
                    skill ??= item.Specialization == Template.EliteSpecialization?.Id || item.Specialization == 0 ? item : skill;
                    if (!berserker && item.Specialization == Template.EliteSpecialization?.Id && skill.Specialization == 0)
                    {
                        skill = item;
                    }
                }

                return skill;
            }

            if (Template.EliteSpecialization?.Id is (int) SpecializationType.Spellbreaker or (int) SpecializationType.Berserker)
            {
                _skills[0].Skill = GetSkill(SkillSlot.Profession2);
                _skills[1].Skill = GetSkill(SkillSlot.Profession1);
            }
            else
            {
                _skills[0].Skill = GetSkill(SkillSlot.Profession1);
                _skills[1].Skill = GetSkill(SkillSlot.Profession2);
            }

            RecalculateLayout();
        }
    }
}
