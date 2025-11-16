using Blish_HUD;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static Blish_HUD.ContentService;
using static Kenedia.Modules.BuildsManager.DataModels.Professions.Weapon;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;

namespace Kenedia.Modules.BuildsManager.Controls.ProfessionSpecific
{
    public class WarriorSpecifics : ProfessionSpecifics
    {
        //F:\Guild Wars 2 Assets\Textures\UI Textures\256x64
        private readonly DetailedTexture _emptyAdrenalin = new(156441);
        private readonly DetailedTexture _adrenalin1 = new(156442);
        private readonly DetailedTexture _adrenalin2 = new(156443);
        private readonly DetailedTexture _adrenalin3 = new(156444);
        private Rectangle _separatorBounds;
        
        private readonly DetailedTexture _motivationBackground = new(3680713);
        private readonly DetailedTexture _motivation = new(3680713, 3680717);

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
            new(),
            new(),
        };

        public WarriorSpecifics(TemplatePresenter template, Data data) : base(template, data)
        {
            template.GearCodeChanged += Template_GearCodeChanged;
        }

        private void Template_GearCodeChanged(object sender, EventArgs e)
        {
            ApplyTemplate();
            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            int xOffset = 90;

            switch (TemplatePresenter.Template.EliteSpecialization?.Id)
            {

                case (int)SpecializationType.Paragon:
                    xOffset = 68;

                    int adrenalin_size = 280;
                    _emptyAdrenalin.Bounds = new(xOffset, 80, adrenalin_size, 14);
                    _emptyAdrenalin.TextureRegion = new(0, 25, 217, 14);

                    _adrenalin1.Bounds = new(xOffset, 80, adrenalin_size, 14);
                    _adrenalin1.TextureRegion = new(0, 25, 217, 14);

                    _adrenalin2.Bounds = new(xOffset, 80, adrenalin_size, 14);
                    _adrenalin2.TextureRegion = new(0, 25, 217, 14);

                    _adrenalin3.Bounds = new(xOffset, 80, adrenalin_size, 14);
                    _adrenalin3.TextureRegion = new(0, 25, 217, 14);

                    Skills[0].Bounds = new(xOffset, 30, 42, 42);
                    _separatorBounds = new(Skills[0].Bounds.Right + 10, Skills[1].Bounds.Top + 3, 2, Skills[1].Bounds.Height - 6);

                    _motivation.Bounds = new(xOffset + 210, 20, 64, 64);

                    for (int i = 1; i < 4; i++)
                    {
                        SkillIcon skill = Skills[i];
                        skill.Bounds = new(_separatorBounds.Right + 10 + ((i - 1) * 48), 30, 42, 42);
                    }

                    break;
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
            RecalculateLayout();
            base.PaintAfterChildren(spriteBatch, bounds);
            switch (TemplatePresenter.Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Paragon:

                    Skills[0].Draw(this, spriteBatch, RelativeMousePosition);
                    Skills[1].Draw(this, spriteBatch, RelativeMousePosition);
                    Skills[2].Draw(this, spriteBatch, RelativeMousePosition);
                    Skills[3].Draw(this, spriteBatch, RelativeMousePosition);

                    _emptyAdrenalin.Draw(this, spriteBatch);
                    _adrenalin1.Draw(this, spriteBatch);
                    _adrenalin2.Draw(this, spriteBatch);
                    _adrenalin3.Draw(this, spriteBatch);

                    _motivation.Draw(this, spriteBatch, RelativeMousePosition,  color: _motivation.Bounds.Contains(RelativeMousePosition) ? new(32, 32, 32, 180) : new(32, 32, 32, 120));

                    spriteBatch.DrawOnCtrl(this, Textures.Pixel, _separatorBounds, Color.Black);

                    break;

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
            if (!Data.IsLoaded) return;

            base.ApplyTemplate();

            var skills = Data?.Professions?[Gw2Sharp.Models.ProfessionType.Warrior]?.Skills;
            if (skills is null) return;

            Skill? GetSkill(SkillSlot slot)
            {
                Skill? skill = null;
                bool spellbreaker = TemplatePresenter.Template.EliteSpecialization?.Id == (int)SpecializationType.Spellbreaker;
                bool bladesworn = TemplatePresenter.Template.EliteSpecialization?.Id == (int)SpecializationType.Bladesworn;

                if (spellbreaker && slot == SkillSlot.Profession2)
                {
                    return skills.Get(44165);
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
                            List<int> underwater_skills = [
                                14443, // Underwater Spear
                                ];

                            foreach (var item in weaponSkills)
                            {
                                if (underwater_skills.Contains(item.Id))
                                {
                                    continue;
                                }

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

            switch (TemplatePresenter.Template.EliteSpecialization?.Id)
            {                
                case (int)SpecializationType.Spellbreaker:
                case (int)SpecializationType.Berserker:
                    Skills[0].Skill = GetSkill(SkillSlot.Profession2);
                    Skills[1].Skill = GetSkill(SkillSlot.Profession1);
                    break;

                default:
                    Skills[0].Skill = GetSkill(SkillSlot.Profession1);
                    Skills[1].Skill = GetSkill(SkillSlot.Profession2);
                    Skills[2].Skill = GetSkill(SkillSlot.Profession3);
                    Skills[3].Skill = GetSkill(SkillSlot.Profession4);
                    break;
            }
        }

        protected override void DisposeControl()
        {
            TemplatePresenter.GearCodeChanged -= Template_GearCodeChanged;
            base.DisposeControl();
        }
    }
}
