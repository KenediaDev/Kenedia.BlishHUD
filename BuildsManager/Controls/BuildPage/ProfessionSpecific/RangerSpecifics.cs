using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class RangerSpecifics : ProfessionSpecifics
    {
        private readonly List<int> _aquaticPets = new() { 1, 5, 6, 7, 9, 11, 12, 18, 19, 20, 21, 23, 24, 25, 26, 27, 40, 41, 42, 43, 45, 47, 63, };
        private readonly List<int> _terrestrialPets = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 44, 45, 46, 47, 48, 51, 52, 54, 55, 57, 59, 61, 63, 64, 65, 66 };
        private readonly DetailedTexture _stow = new(156800);
        private readonly DetailedTexture _swap = new(156587, 156588);
        private readonly DetailedTexture _target = new(156812);
        private readonly DetailedTexture _return = new(156816);
        private readonly DetailedTexture _combatState = new(156827);
        private readonly SkillIcon _skill = new();
        private readonly SkillIcon _skill2 = new();
        private readonly SkillIcon _skill3 = new();
        private readonly PetIcon _pet = new() { PetSlot = PetSlot.Terrestrial_1 };
        private readonly PetIcon _altPet = new() { PetSlot = PetSlot.Terrestrial_2 };

        private readonly bool _enableUntamed = false;

        private Specialization _specialization;
        private Color _healthColor = new(162, 17, 11);
        private Rectangle _healthRectangle;
        private Rectangle _slotRectangle;
        private readonly List<PetIcon> _selectablePets = new();
        private Point _petSize = new(120);
        private PetIcon _selectorAnchor;
        private Rectangle _selectorBounds;
        private bool _selectorOpen = false;

        public RangerSpecifics()
        {
            Profession = Gw2Sharp.Models.ProfessionType.Ranger;
            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int xOffset = 87;

            _pet.Bounds = new(xOffset + 152, 0, _petSize.X, _petSize.Y);
            _pet.PawRegion = new(xOffset + 186, 40, 58, 58);
            _pet.FallbackBounds = new(xOffset + 186, 40, 58, 58);

            _altPet.Bounds = new(xOffset + 152 + 175, 0, _petSize.X, _petSize.Y);
            _altPet.PawRegion = new(xOffset + 186 + 175, 40, 58, 58);
            _altPet.FallbackBounds = new(xOffset + 186 + 175, 40, 58, 58);

            _swap.Bounds = new(xOffset + 245 + 25, 40, 56, 56);
            _stow.Bounds = new(xOffset + (38 * 4), 36, 36, 36);

            _healthRectangle = new(xOffset, 80, 188, 18);
            _slotRectangle = new(xOffset, 5, 188, 18);
            if (_enableUntamed && _specialization != null && _specialization.Id == (int)SpecializationType.Untamed)
            {
                _combatState.Bounds = new(xOffset + (38 * 2), 0, 36, 36);
                _return.Bounds = new(xOffset + (38 * 1), 0, 36, 36);
                _target.Bounds = new(xOffset, 0, 36, 36);

                _skill.Bounds = new(xOffset, 36, 36, 36);
                _skill2.Bounds = new(xOffset + (38 * 1), 36, 36, 36);
                _skill3.Bounds = new(xOffset + (38 * 2), 36, 36, 36);

            }
            else
            {
                _combatState.Bounds = new(xOffset + (38 * 3), 36, 36, 36);
                _return.Bounds = new(xOffset + (38 * 2), 36, 36, 36);
                _skill.Bounds = new(xOffset + 38, 36, 36, 36);
                _target.Bounds = new(xOffset, 36, 36, 36);
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Template != null)
            {
                if (_enableUntamed && _specialization != null && _specialization.Id == (int)SpecializationType.Untamed)
                {
                    PaintUntamed(spriteBatch, bounds, RelativeMousePosition);
                }
                else
                {
                    spriteBatch.DrawStringOnCtrl(this, _pet.PetSlot.ToString().Replace("_", " "), GameService.Content.DefaultFont18, _slotRectangle, Color.White, false, HorizontalAlignment.Center, VerticalAlignment.Middle);
                    PaintCore(spriteBatch, bounds, RelativeMousePosition);
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

                if (_selectorOpen)
                {
                    DrawSelector(spriteBatch, bounds);
                }
            }
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (_selectorOpen)
            {
                foreach (var s in _selectablePets)
                {
                    if (s.Hovered)
                    {
                        var otherSlot = Template.Terrestrial ? (_selectorAnchor.PetSlot == PetSlot.Terrestrial_1 ? PetSlot.Terrestrial_2 : PetSlot.Terrestrial_1) : (_selectorAnchor.PetSlot == PetSlot.Aquatic_1 ? PetSlot.Aquatic_2 : PetSlot.Aquatic_1);
                        if (Template.BuildTemplate.Pets[otherSlot] != s.Pet)
                        {
                            _pet.Pet = s.Pet;
                            Template.BuildTemplate.Pets[_selectorAnchor.PetSlot] = s.Pet;
                        }

                        break;
                    }
                }

                _selectorOpen = !_selectorOpen;
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_swap.Hovered)
            {
                var otherSlot = _pet.PetSlot;
                _pet.PetSlot = Template.Terrestrial ? (_pet.PetSlot == PetSlot.Terrestrial_1 ? PetSlot.Terrestrial_2 : PetSlot.Terrestrial_1) : (_pet.PetSlot == PetSlot.Aquatic_1 ? PetSlot.Aquatic_2 : PetSlot.Aquatic_1);

                _pet.Pet = Template.BuildTemplate.Pets[_pet.PetSlot];

                _altPet.Pet = Template.BuildTemplate.Pets[otherSlot];
                _altPet.PetSlot = otherSlot;

                var skill = Template.BuildTemplate.Pets[_pet.PetSlot]?.Skills.FirstOrDefault();
                _skill.Skill = skill.HasValue ? skill.Value.Value : _skill.Skill;
            }

            if (_pet.Hovered)
            {
                if (!_selectorOpen)
                {
                    _selectorAnchor = _pet;
                    GetSelectablePets();
                }

                _selectorOpen = !_selectorOpen;
            }

            if (_altPet.Hovered)
            {
                if (!_selectorOpen)
                {
                    _selectorAnchor = _altPet;
                    GetSelectablePets();
                }

                _selectorOpen = !_selectorOpen;
            }
        }

        protected override void ApplyTemplate()
        {
            _specialization = null;

            if (Template != null)
            {
                foreach (var s in Template.BuildTemplate.Specializations)
                {
                    if (s.Value != null && s.Value.Specialization != null && s.Value.Specialization.Elite)
                    {
                        _specialization = s.Value.Specialization;
                        break;
                    }
                }

                if (Template.Terrestrial)
                {
                    _pet.PetSlot = _pet.PetSlot is not PetSlot.Terrestrial_1 and not PetSlot.Terrestrial_2 ? (_pet.PetSlot is PetSlot.Aquatic_1) ? PetSlot.Terrestrial_1 : PetSlot.Terrestrial_2 : _pet.PetSlot;
                    _altPet.PetSlot = _altPet.PetSlot is not PetSlot.Terrestrial_1 and not PetSlot.Terrestrial_2 ? (_altPet.PetSlot is PetSlot.Aquatic_1) ? PetSlot.Terrestrial_1 : PetSlot.Terrestrial_2 : _altPet.PetSlot;
                }
                else
                {
                    _pet.PetSlot = _pet.PetSlot is not PetSlot.Aquatic_1 and not PetSlot.Aquatic_2 ? (_pet.PetSlot is PetSlot.Terrestrial_1) ? PetSlot.Aquatic_1 : PetSlot.Aquatic_2 : _pet.PetSlot;
                    _altPet.PetSlot = _altPet.PetSlot is not PetSlot.Aquatic_1 and not PetSlot.Aquatic_2 ? (_altPet.PetSlot is PetSlot.Terrestrial_1) ? PetSlot.Aquatic_1 : PetSlot.Aquatic_2 : _altPet.PetSlot;
                }

                _pet.Pet = Template.BuildTemplate.Pets[_pet.PetSlot];
                _altPet.Pet = Template.BuildTemplate.Pets[_altPet.PetSlot];

                var skill = Template.BuildTemplate.Pets[_pet.PetSlot]?.Skills.FirstOrDefault();
                _skill.Skill = skill.HasValue ? skill.Value.Value : _skill.Skill;

                if (_enableUntamed && _specialization != null && _specialization.Id == (int)SpecializationType.Untamed)
                {
                    _skill.Skill = _pet.Pet != null ? _pet.Pet.Skills.FirstOrDefault().Value : _skill.Skill;
                    //_skill2.Skill = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Ranger].Skills.Where(e => e.Value.Slot == Gw2Sharp.WebApi.V2.Models.SkillSlot.Profession2 && e.Value.Specialization == (int)Specializations.Soulbeast)?.FirstOrDefault().Value;
                    //_skill3.Skill = BuildsManager.Data.Professions[Gw2Sharp.Models.ProfessionType.Ranger].Skills.Where(e => e.Value.Slot == Gw2Sharp.WebApi.V2.Models.SkillSlot.Profession3 && e.Value.Specialization == (int)Specializations.Soulbeast)?.FirstOrDefault().Value;
                }
            }

            base.ApplyTemplate();
        }

        private void PaintCore(SpriteBatch spriteBatch, Rectangle bounds, Point mousePos)
        {
            _pet.Draw(this, spriteBatch, mousePos);
            _altPet.Draw(this, spriteBatch, mousePos);
            _swap.Draw(this, spriteBatch, mousePos);

            _target.Draw(this, spriteBatch);
            _return.Draw(this, spriteBatch);
            _combatState.Draw(this, spriteBatch);
            _stow.Draw(this, spriteBatch);
            _skill.Draw(this, spriteBatch, mousePos);
        }

        private void PaintUntamed(SpriteBatch spriteBatch, Rectangle bounds, Point mousePos)
        {
            _pet.Draw(this, spriteBatch, mousePos);
            _altPet.Draw(this, spriteBatch, mousePos);
            _swap.Draw(this, spriteBatch, mousePos);

            _target.Draw(this, spriteBatch);
            _return.Draw(this, spriteBatch);
            _combatState.Draw(this, spriteBatch);
            _stow.Draw(this, spriteBatch);
            _skill.Draw(this, spriteBatch, mousePos);
            _skill2.Draw(this, spriteBatch, mousePos);
            _skill3.Draw(this, spriteBatch, mousePos);
        }

        private void DrawSelector(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, _selectorBounds, Rectangle.Empty, Color.Black * 0.7f);

            Color borderColor = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(_selectorBounds.Left, _selectorBounds.Top, _selectorBounds.Width, 2), Rectangle.Empty, borderColor * 0.8f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(_selectorBounds.Left, _selectorBounds.Bottom - 2, _selectorBounds.Width, 2), Rectangle.Empty, borderColor * 0.8f);

            // Left
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(_selectorBounds.Left, _selectorBounds.Top, 2, _selectorBounds.Height), Rectangle.Empty, borderColor * 0.8f);

            // Right
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(_selectorBounds.Right - 2, _selectorBounds.Top, 2, _selectorBounds.Height), Rectangle.Empty, borderColor * 0.8f);

            foreach (var s in _selectablePets)
            {
                s.Draw(this, spriteBatch, RelativeMousePosition);
                if (s.Hovered)
                {
                }
            }

            spriteBatch.DrawStringOnCtrl(this, string.Format("{0} Pets", Template.Terrestrial ? "Terrestrial" : "Aquatic"), Content.DefaultFont18, new Rectangle(_selectorBounds.Left, _selectorBounds.Bottom - 12 - Content.DefaultFont18.LineHeight, _selectorBounds.Width, Content.DefaultFont18.LineHeight), Color.White, false, HorizontalAlignment.Center);
        }

        private void GetSelectablePets()
        {
            if (Template != null)
            {
                _selectablePets.Clear();

                var pets = BuildsManager.Data.Pets.Where(e => (Template.Terrestrial ? _terrestrialPets : _aquaticPets).Contains(e.Value.Id)).ToList().OrderBy(e => e.Value.Id);
                int columns = Math.Min(pets.Count(), 6);
                int rows = (int)Math.Ceiling(pets.Count() / (double)columns);

                int column = 0;
                int row = 0;
                int petSize = (int)(_petSize.X / 1.25);

                _selectorBounds = new(_selectorAnchor.Bounds.X - ((((petSize - 28) * columns) + 8) / 2 - ((petSize - 28) / 2)), _selectorAnchor.Bounds.Bottom - 32, ((petSize - 28) * columns) + 4 + 28, ((petSize - 28) * rows) + 28 + 40);

                foreach (var p in pets)
                {
                    _selectablePets.Add(new PetIcon()
                    {
                        Pet = p.Value,
                        Bounds = new(_selectorBounds.Left + (column * (petSize - 28)), _selectorBounds.Top + (row * (petSize - 28)), petSize, petSize),
                        PawRegion = new(_selectorBounds.Left + (column * (petSize - 28)) + (int)(34 / 1.25), _selectorBounds.Top + (row * (petSize - 28)) + (int)(40 / 1.25), (int)(58 / 1.25), (int)(58 / 1.25)),
                    });
                    column++;

                    if (column >= columns)
                    {
                        column = 0;
                        row++;
                    }
                }

            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Input.Mouse.LeftMouseButtonPressed -= Mouse_LeftMouseButtonPressed;
        }
    }
}
