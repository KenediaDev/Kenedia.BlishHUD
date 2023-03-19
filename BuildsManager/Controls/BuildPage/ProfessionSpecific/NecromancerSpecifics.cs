using Blish_HUD;
using Blish_HUD.Controls;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class NecromancerSpecifics : ProfessionSpecifics
    {
        // _noTripleShade 1636739
        // _noSingleShade 1636740
        // _oneTripleShade 1636741
        // _oneSingleShade 1636742
        // _twoTripleShade 1636743
        // _threeTripleShade 1636744

        private readonly DetailedTexture _lifeForceBarBackground = new(1636710); // black background?
        private readonly DetailedTexture _lifeForceBar = new(2479935); // needs black background
        private readonly DetailedTexture _lifeForceScourge = new(1636711);
        private readonly DetailedTexture _lifeForce = new(156436);
        private readonly DetailedTexture _shades = new(1636744);

        private readonly SkillIcon[] _skills =
        {
            new(),
            new(),
            new(),
            new(),
            new(),
        };

        public NecromancerSpecifics()
        {

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            int xOffset = 80;

            switch (Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Scourge:
                    _shades.Bounds = new(xOffset + 10 + 46, 28, 36, 36);
                    _shades.TextureRegion = new(0, 2, _shades.Texture.Width, _shades.Texture.Height - 4);

                    // Shade
                    _skills[0].Bounds = new(xOffset + 10, 25, 42, 42);

                    //Shade Skills
                    for (int i = 1; i < _skills.Length; i++)
                    {
                        _skills[i].Bounds = new(xOffset + 54 + (i * 39), 28, 36, 36);
                    }

                    _lifeForceBarBackground.Bounds = new(xOffset + 10, 75, 250, 20);
                    _lifeForceScourge.Bounds = new(xOffset + 11, 76, 247, 18);

                    break;

                case (int)SpecializationType.Harbinger:
                    _lifeForceBarBackground.Bounds = new(xOffset + 10, 70, 205, 20);
                    _lifeForceBar.Bounds = new(xOffset + 11, 71, 203, 18);
                    _skills[0].Bounds = new(xOffset + 215, 55, 42, 42);
                    break;

                default:
                    _lifeForceBarBackground.Bounds = new(xOffset + 10, 70, 205, 20);
                    _lifeForce.Bounds = new(xOffset + 10, 70, 205, 20);
                    _lifeForce.TextureRegion = new(1, 42, _lifeForce.Texture.Width - 30, _lifeForce.Texture.Height - 49);
                    _skills[0].Bounds = new(xOffset + 215, 55, 42, 42);
                    break;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            switch (Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Scourge:
                    _shades.Draw(this, spriteBatch);
                    _lifeForceBarBackground.Draw(this, spriteBatch);
                    _lifeForceScourge.Draw(this, spriteBatch, null, Color.LightGray * 0.7F);

                    for (int i = 0; i < _skills.Length; i++)
                    {
                        _skills[i].Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    spriteBatch.DrawStringOnCtrl(this, "100%", Content.DefaultFont12, _lifeForceScourge.Bounds, Color.White, false, HorizontalAlignment.Center, VerticalAlignment.Middle);
                    break;

                case (int)SpecializationType.Harbinger:
                    _lifeForceBarBackground.Draw(this, spriteBatch);
                    _lifeForceBar.Draw(this, spriteBatch, null, Color.LightGray * 0.7F);
                    _skills[0].Draw(this, spriteBatch, RelativeMousePosition);
                    spriteBatch.DrawStringOnCtrl(this, "100%", Content.DefaultFont12, _lifeForceBar.Bounds, Color.White, false, HorizontalAlignment.Center, VerticalAlignment.Middle);
                    break;

                default:
                    _lifeForceBarBackground.Draw(this, spriteBatch);
                    _lifeForce.Draw(this, spriteBatch, null, Color.LightGray * 0.7F);
                    _skills[0].Draw(this, spriteBatch, RelativeMousePosition);
                    spriteBatch.DrawStringOnCtrl(this, "100%", Content.DefaultFont12, _lifeForce.Bounds, Color.White, false, HorizontalAlignment.Center, VerticalAlignment.Middle);
                    break;
            }
        }

        protected override void ApplyTemplate()
        {
            RecalculateLayout();

            var skills = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Necromancer].Skills;

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

            switch (Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Scourge:
                    _skills[0].Skill = GetSkill(SkillSlot.Profession1);
                    _skills[1].Skill = GetSkill(SkillSlot.Profession2);
                    _skills[2].Skill = GetSkill(SkillSlot.Profession3);
                    _skills[3].Skill = GetSkill(SkillSlot.Profession4);
                    _skills[4].Skill = GetSkill(SkillSlot.Profession5);
                    break;

                case (int)SpecializationType.Harbinger:
                    _skills[0].Skill = skills[62567];
                    break;

                case (int)SpecializationType.Reaper:
                    _skills[0].Skill = skills[30792];
                    break;

                default:
                    _skills[0].Skill = GetSkill(SkillSlot.Profession1);
                    break;
            }
        }
    }
}
