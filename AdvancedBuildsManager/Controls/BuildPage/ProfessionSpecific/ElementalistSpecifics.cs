﻿using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Kenedia.Modules.Core.DataModels;
using Blish_HUD;
using static Blish_HUD.ContentService;
using Blish_HUD.Input;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class ElementalistSpecifics : ProfessionSpecifics
    {
        private readonly DetailedTexture _catalistSeparator = new(2492046);
        private readonly SkillIcon[] _skills =
        {
            new(),
            new(),
            new(),
            new(),
            new(),
        };
        private readonly (Rectangle bounds, Color color)[] _backgrounds =
        {
            new(Rectangle.Empty, new(255, 125, 0)),
            new(Rectangle.Empty, new(0, 170, 255)),
            new(Rectangle.Empty, new(165, 101, 255)),
            new(Rectangle.Empty, new(231, 195, 22)),
            new(Rectangle.Empty, Color.Transparent),
        };
        private Rectangle _catalystEnergy;
        private Color _catalystEnergyColor;

        public ElementalistSpecifics()
        {

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int xOffset = 70;
            var lastRect = new Rectangle(xOffset + 25, 52, 0, 0);

            for (int i = 0; i < _skills.Length; i++)
            {
                var skill = _skills[i];

                bool main =
                    (i == 0 && Template.MainAttunement == AttunementType.Fire) ||
                    (i == 1 && Template.MainAttunement == AttunementType.Water) ||
                    (i == 2 && Template.MainAttunement == AttunementType.Air) ||
                    (i == 3 && Template.MainAttunement == AttunementType.Earth);

                bool secondary =
                    (i == 0 && Template.AltAttunement == AttunementType.Fire) ||
                    (i == 1 && Template.AltAttunement == AttunementType.Water) ||
                    (i == 2 && Template.AltAttunement == AttunementType.Air) ||
                    (i == 3 && Template.AltAttunement == AttunementType.Earth);

                _backgrounds[i].bounds =
                    main ? new(lastRect.Right + 4, 47, 44, 44) :
                    secondary ? new(lastRect.Right + 4, 49, 39, 39) :
                    new(lastRect.Right + 4, 54, 34, 0);

                skill.Bounds =
                    main ? new(lastRect.Right + 6, 49, 40, 40) :
                    secondary ? new(lastRect.Right + 6, 51, 35, 35) :
                    new(lastRect.Right + 4 + (i == 4 ? 20 : 0), i == 4 ? 52 : 54, i == 4 ? 38 : 34, i == 4 ? 38 : 34);
                lastRect = skill.Bounds;

                if (i == 4)
                    _catalystEnergy = new(skill.Bounds.Left, skill.Bounds.Top - 4, skill.Bounds.Width, 4);
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_skills[0].Hovered)
            {
                Template.AltAttunement = Template.EliteSpecialization?.Id == (int)SpecializationType.Weaver ? Template.MainAttunement : AttunementType.Fire;
                Template.MainAttunement = AttunementType.Fire;
            }

            if (_skills[1].Hovered)
            {
                Template.AltAttunement = Template.EliteSpecialization?.Id == (int)SpecializationType.Weaver ? Template.MainAttunement : AttunementType.Water;
                Template.MainAttunement = AttunementType.Water;
            }

            if (_skills[2].Hovered)
            {
                Template.AltAttunement = Template.EliteSpecialization?.Id == (int)SpecializationType.Weaver ? Template.MainAttunement : AttunementType.Air;
                Template.MainAttunement = AttunementType.Air;
            }

            if (_skills[3].Hovered)
            {
                Template.AltAttunement = Template.EliteSpecialization?.Id == (int)SpecializationType.Weaver ? Template.MainAttunement : AttunementType.Earth;
                Template.MainAttunement = AttunementType.Earth;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            RecalculateLayout();
            switch (Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Catalyst:
                    for (int i = 0; i < 4; i++)
                    {
                        spriteBatch.DrawOnCtrl(this, Textures.Pixel, _backgrounds[i].bounds, _backgrounds[i].color);
                    }

                    spriteBatch.DrawOnCtrl(this, Textures.Pixel, _catalystEnergy, _catalystEnergyColor);

                    foreach (var skill in _skills)
                    {
                        skill.Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    break;

                default:
                    for (int i = 0; i < 4; i++)
                    {
                        spriteBatch.DrawOnCtrl(this, Textures.Pixel, _backgrounds[i].bounds, _backgrounds[i].color);

                        var skill = _skills[i];
                        skill.Draw(this, spriteBatch, RelativeMousePosition);
                    }
                    break;
            }
        }

        protected override void ApplyTemplate()
        {
            RecalculateLayout();
            var skills = AdvancedBuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Elementalist].Skills;

            Skill GetSkill(SkillSlot slot)
            {
                Skill skill = null;

                foreach (var item in skills.Values.Where(e => e.Slot == slot))
                {
                    skill ??= item.Specialization == Template.EliteSpecialization?.Id || item.Specialization == 0 ? item : skill;
                }

                return skill;
            }

            _skills[0].Skill = Template.EliteSpecialization?.Id == (int) SpecializationType.Tempest && Template.MainAttunement  == AttunementType.Fire ? skills[29706] : GetSkill(SkillSlot.Profession1);
            _skills[1].Skill = Template.EliteSpecialization?.Id == (int)SpecializationType.Tempest && Template.MainAttunement == AttunementType.Water ? skills[29415] : GetSkill(SkillSlot.Profession2);
            _skills[2].Skill = Template.EliteSpecialization?.Id == (int)SpecializationType.Tempest && Template.MainAttunement == AttunementType.Air ? skills[29719] : GetSkill(SkillSlot.Profession3);
            _skills[3].Skill = Template.EliteSpecialization?.Id == (int)SpecializationType.Tempest && Template.MainAttunement == AttunementType.Earth ? skills[29618] : GetSkill(SkillSlot.Profession4);
            _skills[4].Skill = 
                Template.MainAttunement == AttunementType.Fire ? skills[62813] :
                Template.MainAttunement == AttunementType.Water ? skills[62723] :
                Template.MainAttunement == AttunementType.Air ? skills[62940] :
                Template.MainAttunement == AttunementType.Earth? skills[62837] :
                null;

            _catalystEnergyColor = 
                Template.MainAttunement == AttunementType.Fire ? _backgrounds[0].color :
                Template.MainAttunement == AttunementType.Water ? _backgrounds[1].color :
                Template.MainAttunement == AttunementType.Air ? _backgrounds[2].color :
                Template.MainAttunement == AttunementType.Earth? _backgrounds[3].color :
                Color.Black;
        }
    }
}
