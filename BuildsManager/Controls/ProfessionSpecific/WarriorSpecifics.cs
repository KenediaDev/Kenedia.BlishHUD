using Kenedia.Modules.Core.DataModels;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;
using System.Linq;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using System;

namespace Kenedia.Modules.BuildsManager.Controls.ProfessionSpecific
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

        protected override SkillIcon[] Skills { get; } = {
            new(),
            new(),
        };

        public WarriorSpecifics(TemplatePresenter template) : base(template)
        {

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            int xOffset = 90;

            switch (TemplatePresenter.Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Bladesworn:
                    for (int i = 0; i < _charges.Length; i++)
                    {
                        _charges[i].Bounds = new(xOffset + 90 + 1 + (i * 17), 57, 12, 24);
                    }
                    Skills[0].Bounds = new(xOffset, 56, 42, 42);
                    Skills[1].Bounds = new(xOffset + 44, 56, 42, 42);
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

                    Skills[0].Bounds = new(xOffset + 211, 56, 42, 42);
                    Skills[1].Bounds = new(xOffset + 211 - 45, 56, 42, 42);
                    break;

                case (int)SpecializationType.Spellbreaker:
                    _emptyAdrenalin.Bounds = new(xOffset + 3, 80, 163, 14);
                    _emptyAdrenalin.TextureRegion = new(0, 25, 145, 14);

                    _adrenalin1.Bounds = new(xOffset + 3, 80, 163, 14);
                    _adrenalin1.TextureRegion = new(0, 25, 145, 14);

                    _adrenalin2.Bounds = new(xOffset + 3, 80, 163, 14);
                    _adrenalin2.TextureRegion = new(0, 25, 145, 14);

                    Skills[0].Bounds = new(xOffset + 211, 56, 42, 42);
                    Skills[1].Bounds = new(xOffset + 211 - 45, 56, 42, 42);
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

                    Skills[0].Bounds = new(xOffset + 211, 56, 42, 42);
                    break;
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
            switch (TemplatePresenter.Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Bladesworn:
                    _barBackground.Draw(this, spriteBatch);
                    Skills[0].Draw(this, spriteBatch, RelativeMousePosition);
                    Skills[1].Draw(this, spriteBatch, RelativeMousePosition);

                    for (int i = 0; i < _charges.Length; i++)
                    {
                        _charges[i].Draw(this, spriteBatch);
                    }

                    break;

                case (int)SpecializationType.Spellbreaker:
                    Skills[0].Draw(this, spriteBatch, RelativeMousePosition);
                    Skills[1].Draw(this, spriteBatch, RelativeMousePosition);
                    _emptyAdrenalin.Draw(this, spriteBatch);
                    _adrenalin1.Draw(this, spriteBatch);
                    _adrenalin2.Draw(this, spriteBatch);
                    break;

                case (int)SpecializationType.Berserker:
                    Skills[0].Draw(this, spriteBatch, RelativeMousePosition);
                    Skills[1].Draw(this, spriteBatch, RelativeMousePosition);
                    _emptyAdrenalin.Draw(this, spriteBatch);
                    _adrenalin1.Draw(this, spriteBatch);
                    _adrenalin2.Draw(this, spriteBatch);
                    _adrenalin3.Draw(this, spriteBatch);
                    break;

                default:
                    Skills[0].Draw(this, spriteBatch, RelativeMousePosition);
                    _emptyAdrenalin.Draw(this, spriteBatch);
                    _adrenalin1.Draw(this, spriteBatch);
                    _adrenalin2.Draw(this, spriteBatch);
                    _adrenalin3.Draw(this, spriteBatch);
                    break;
            }
        }

        protected override void ApplyTemplate()
        {
            if (TemplatePresenter?.Template is null) return;
            base.ApplyTemplate();

            var skills = BuildsManager.Data?.Professions?[Gw2Sharp.Models.ProfessionType.Warrior]?.Skills;
            if (skills is null) return;

            Skill? GetSkill(SkillSlot slot)
            {
                Skill? skill = null;
                bool spellbreaker = TemplatePresenter.Template.EliteSpecialization?.Id == (int)SpecializationType.Spellbreaker;
                bool bladesworn = TemplatePresenter.Template.EliteSpecialization?.Id == (int)SpecializationType.Bladesworn;

                if (spellbreaker && slot == SkillSlot.Profession2)
                {
                    return skills[44165];
                }

                var slotSkills = skills.Values.Where(e => e.Slot == slot);

                if (!bladesworn && slot is SkillSlot.Profession1)
                {
                    string typeString = TemplatePresenter.Template?.MainHand?.Weapon?.WeaponType.ToString();

                    if (!string.IsNullOrEmpty(typeString))
                    {
                        var weapon = (WeaponType)Enum.Parse(typeof(WeaponType), TemplatePresenter.Template?.MainHand?.Weapon?.WeaponType.ToString());

                        if (weapon is not WeaponType.Unknown)
                        {
                            var weaponSkills = slotSkills.Where(x => x.Specialization == TemplatePresenter?.Template?.EliteSpecialization?.Id || x.Specialization == 0).Where(x => x.WeaponType is null || x.WeaponType == weapon).OrderBy(x => x.Specialization == 0);

                            foreach (var item in weaponSkills)
                            {
                                skill ??= item.Specialization == TemplatePresenter?.Template?.EliteSpecialization?.Id || item.Specialization == 0 ? item : skill;
                            }
                        }
                    }
                }
                else
                {
                    var eliteSpecSkills = (TemplatePresenter?.Template?.EliteSpecialization?.Id ?? 0) != 0 ? slotSkills.Where(x => x.Specialization == TemplatePresenter?.Template?.EliteSpecialization?.Id) : [];

                    foreach (var item in eliteSpecSkills)
                    {
                        skill ??= item.Specialization == TemplatePresenter?.Template?.EliteSpecialization?.Id || item.Specialization == 0 ? item : skill;
                    }
                }

                return skill;
            }

            if (TemplatePresenter.Template.EliteSpecialization?.Id is (int) SpecializationType.Spellbreaker or (int) SpecializationType.Berserker)
            {
                Skills[0].Skill = GetSkill(SkillSlot.Profession2);
                Skills[1].Skill = GetSkill(SkillSlot.Profession1);
            }
            else
            {
                Skills[0].Skill = GetSkill(SkillSlot.Profession1);
                Skills[1].Skill = GetSkill(SkillSlot.Profession2);
            }
        }
    }
}
