using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Kenedia.Modules.Core.DataModels;
using Blish_HUD;
using static Blish_HUD.ContentService;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models;
using System.Diagnostics;
using System;

namespace Kenedia.Modules.BuildsManager.Controls.ProfessionSpecific
{
    public class ElementalistSpecifics : ProfessionSpecifics
    {
        private readonly DetailedTexture _catalistSeparator = new(2492046);

        protected override SkillIcon[] Skills { get; } = {
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

        public ElementalistSpecifics(TemplatePresenter template) : base(template)
        {
            template.AttunementChanged += AttunementChanged;
        }

        private void AttunementChanged(object sender, AttunementChangedEventArgs e)
        {
            ApplyTemplate();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            CalculateSkillBounds();
        }

        private void CalculateSkillBounds()
        {
            int xOffset = 70;
            var lastRect = new Rectangle(xOffset + 25, 52, 0, 0);

            for (int i = 0; i < Skills.Length; i++)
            {
                var skill = Skills[i];

                bool main =
                    (i == 0 && TemplatePresenter.MainAttunement == AttunementType.Fire) ||
                    (i == 1 && TemplatePresenter.MainAttunement == AttunementType.Water) ||
                    (i == 2 && TemplatePresenter.MainAttunement == AttunementType.Air) ||
                    (i == 3 && TemplatePresenter.MainAttunement == AttunementType.Earth);

                bool secondary =
                    (i == 0 && TemplatePresenter.AltAttunement == AttunementType.Fire) ||
                    (i == 1 && TemplatePresenter.AltAttunement == AttunementType.Water) ||
                    (i == 2 && TemplatePresenter.AltAttunement == AttunementType.Air) ||
                    (i == 3 && TemplatePresenter.AltAttunement == AttunementType.Earth);

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

            var attunement = GetAttunement();
            if (attunement is not AttunementType.None)
            {
                TemplatePresenter.SetAttunement(attunement);
            }
        }

        private AttunementType GetAttunement()
        {
            return 
                Skills[0].Hovered ? AttunementType.Fire
                : Skills[1].Hovered ? AttunementType.Water
                : Skills[2].Hovered ? AttunementType.Air 
                : Skills[3].Hovered ? AttunementType.Earth 
                : AttunementType.None;
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            switch (TemplatePresenter.Template.EliteSpecialization?.Id)
            {
                case (int)SpecializationType.Catalyst:
                    for (int i = 0; i < 4; i++)
                    {
                        spriteBatch.DrawOnCtrl(this, Textures.Pixel, _backgrounds[i].bounds, _backgrounds[i].color);
                    }

                    spriteBatch.DrawOnCtrl(this, Textures.Pixel, _catalystEnergy, _catalystEnergyColor);

                    foreach (var skill in Skills)
                    {
                        skill.Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    break;

                default:
                    for (int i = 0; i < 4; i++)
                    {
                        spriteBatch.DrawOnCtrl(this, Textures.Pixel, _backgrounds[i].bounds, _backgrounds[i].color);

                        var skill = Skills[i];
                        skill.Draw(this, spriteBatch, RelativeMousePosition);
                    }

                    break;
            }

            SetTooltipSkill();
        }

        protected override void ApplyTemplate()
        {
            if (TemplatePresenter?.Template is null) return;
            base.ApplyTemplate();

            var skills = BuildsManager.Data?.Professions?[Gw2Sharp.Models.ProfessionType.Elementalist]?.Skills;
            if (skills is null) return;

            Skill GetSkill(SkillSlot slot)
            {
                Skill skill = null;

                foreach (var item in skills.Values.Where(e => e.Slot == slot))
                {
                    skill ??= item.Specialization == TemplatePresenter.Template.EliteSpecialization?.Id || item.Specialization == 0 ? item : skill;
                }

                return skill;
            }

            Skills[0].Skill = TemplatePresenter.Template.EliteSpecialization?.Id == (int)SpecializationType.Tempest && TemplatePresenter.MainAttunement == AttunementType.Fire ? skills.Values.FirstOrDefault(e => e.Id == 29706) : GetSkill(SkillSlot.Profession1);
            Skills[1].Skill = TemplatePresenter.Template.EliteSpecialization?.Id == (int)SpecializationType.Tempest && TemplatePresenter.MainAttunement == AttunementType.Water ? skills.Values.FirstOrDefault(e => e.Id == 29415) : GetSkill(SkillSlot.Profession2);
            Skills[2].Skill = TemplatePresenter.Template.EliteSpecialization?.Id == (int)SpecializationType.Tempest && TemplatePresenter.MainAttunement == AttunementType.Air ? skills.Values.FirstOrDefault(e => e.Id == 29719) : GetSkill(SkillSlot.Profession3);
            Skills[3].Skill = TemplatePresenter.Template.EliteSpecialization?.Id == (int)SpecializationType.Tempest && TemplatePresenter.MainAttunement == AttunementType.Earth ? skills.Values.FirstOrDefault(e => e.Id == 29618) : GetSkill(SkillSlot.Profession4);
            Skills[4].Skill =
                TemplatePresenter.MainAttunement == AttunementType.Fire ? skills.Values.FirstOrDefault(e => e.Id == 62813) :
                TemplatePresenter.MainAttunement == AttunementType.Water ? skills.Values.FirstOrDefault(e => e.Id == 62723) :
                TemplatePresenter.MainAttunement == AttunementType.Air ? skills.Values.FirstOrDefault(e => e.Id == 62940) :
                TemplatePresenter.MainAttunement == AttunementType.Earth ? skills.Values.FirstOrDefault(e => e.Id == 62837) :
                null;

            _catalystEnergyColor =
                TemplatePresenter.MainAttunement == AttunementType.Fire ? _backgrounds[0].color :
                TemplatePresenter.MainAttunement == AttunementType.Water ? _backgrounds[1].color :
                TemplatePresenter.MainAttunement == AttunementType.Air ? _backgrounds[2].color :
                TemplatePresenter.MainAttunement == AttunementType.Earth ? _backgrounds[3].color :
                Color.Black;

            RecalculateLayout();
        }
    }
}
