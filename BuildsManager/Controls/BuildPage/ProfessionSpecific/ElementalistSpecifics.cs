using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Kenedia.Modules.Core.DataModels;
using Blish_HUD;
using static Blish_HUD.ContentService;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
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

        private int _inactiveSize = 34;
        private int _secondary = 39;
        private int _main = 44;
        private Rectangle _catalystEnergy;
        private Color _catalystEnergyColor;

        public ElementalistSpecifics(TemplatePresenter template) : base(template)
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

            if (_skills[0].Hovered)
            {
                TemplatePresenter.AltAttunement = TemplatePresenter.Template.BuildTemplate.EliteSpecialization?.Id == (int)SpecializationType.Weaver ? TemplatePresenter.MainAttunement : AttunementType.Fire;
                TemplatePresenter.MainAttunement = AttunementType.Fire;
            }

            if (_skills[1].Hovered)
            {
                TemplatePresenter.AltAttunement= TemplatePresenter.Template.BuildTemplate.EliteSpecialization?.Id == (int)SpecializationType.Weaver ? TemplatePresenter.MainAttunement : AttunementType.Water;
                TemplatePresenter.MainAttunement = AttunementType.Water;
            }

            if (_skills[2].Hovered)
            {
                TemplatePresenter.AltAttunement= TemplatePresenter.Template.BuildTemplate.EliteSpecialization?.Id == (int)SpecializationType.Weaver ? TemplatePresenter.MainAttunement : AttunementType.Air;
                TemplatePresenter.MainAttunement = AttunementType.Air;
            }

            if (_skills[3].Hovered)
            {
                TemplatePresenter.AltAttunement= TemplatePresenter.Template.BuildTemplate.EliteSpecialization?.Id == (int)SpecializationType.Weaver ? TemplatePresenter.MainAttunement : AttunementType.Earth;
                TemplatePresenter.MainAttunement = AttunementType.Earth;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            RecalculateLayout();
            switch (TemplatePresenter.Template.BuildTemplate.EliteSpecialization?.Id)
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
            var skills = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Elementalist].Skills;

            Skill GetSkill(SkillSlot slot)
            {
                Skill skill = null;

                foreach (var item in skills.Values.Where(e => e.Slot == slot))
                {
                    skill ??= item.Specialization == TemplatePresenter.Template.BuildTemplate.EliteSpecialization?.Id || item.Specialization == 0 ? item : skill;
                }

                return skill;
            }

            _skills[0].Skill = TemplatePresenter.Template.BuildTemplate.EliteSpecialization?.Id == (int) SpecializationType.Tempest && TemplatePresenter.MainAttunement  == AttunementType.Fire ? skills[29706] : GetSkill(SkillSlot.Profession1);
            _skills[1].Skill = TemplatePresenter.Template.BuildTemplate.EliteSpecialization?.Id == (int)SpecializationType.Tempest && TemplatePresenter.MainAttunement == AttunementType.Water ? skills[29415] : GetSkill(SkillSlot.Profession2);
            _skills[2].Skill = TemplatePresenter.Template.BuildTemplate.EliteSpecialization?.Id == (int)SpecializationType.Tempest && TemplatePresenter.MainAttunement == AttunementType.Air ? skills[29719] : GetSkill(SkillSlot.Profession3);
            _skills[3].Skill = TemplatePresenter.Template.BuildTemplate.EliteSpecialization?.Id == (int)SpecializationType.Tempest && TemplatePresenter.MainAttunement == AttunementType.Earth ? skills[29618] : GetSkill(SkillSlot.Profession4);
            _skills[4].Skill = 
                TemplatePresenter.MainAttunement == AttunementType.Fire ? skills[62813] :
                TemplatePresenter.MainAttunement == AttunementType.Water ? skills[62723] :
                TemplatePresenter.MainAttunement == AttunementType.Air ? skills[62940] :
                TemplatePresenter.MainAttunement == AttunementType.Earth? skills[62837] :
                null;

            _catalystEnergyColor = 
                TemplatePresenter.MainAttunement == AttunementType.Fire ? _backgrounds[0].color :
                TemplatePresenter.MainAttunement == AttunementType.Water ? _backgrounds[1].color :
                TemplatePresenter.MainAttunement == AttunementType.Air ? _backgrounds[2].color :
                TemplatePresenter.MainAttunement == AttunementType.Earth? _backgrounds[3].color :
                Color.Black;
        }
    }
}
