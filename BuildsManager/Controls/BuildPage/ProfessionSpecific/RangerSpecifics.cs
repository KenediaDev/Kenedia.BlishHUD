using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.DataModels;
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

        private bool applied = false;
        private readonly Dictionary<PetSlotType, PetIcon> _pets = new()
        {
            {PetSlotType.Terrestrial_1, new() { PetSlot = PetSlotType.Terrestrial_1 }},
            {PetSlotType.Terrestrial_2, new() { PetSlot = PetSlotType.Terrestrial_2 }},
            {PetSlotType.Aquatic_1, new() { PetSlot = PetSlotType.Aquatic_1 }},
            {PetSlotType.Aquatic_2, new() { PetSlot = PetSlotType.Aquatic_2 }},
        };
        private Color _healthColor = new(162, 17, 11);
        private Rectangle _healthRectangle;
        private Rectangle _slotRectangle;
        private readonly List<PetIcon> _selectablePets = new();
        private Point _petSize = new(120);
        private PetIcon _selectorAnchor;
        private Rectangle _selectorBounds;
        private bool _selectorOpen = false;

        public RangerSpecifics(TemplatePresenter template) : base(template)
        {
            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;

            foreach (var pet in BuildsManager.Data.Pets.OrderBy(e => e.Value.Id))
            {
                _selectablePets.Add(new PetIcon() { Pet = pet.Value });
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int xOffset = 60;

            _pets[PetSlotType.Terrestrial_1].Bounds = new(xOffset, 0, _petSize.X, _petSize.Y);
            _pets[PetSlotType.Terrestrial_1].PawRegion = new(xOffset + 34, 40, 58, 58);
            _pets[PetSlotType.Terrestrial_1].FallbackBounds = new(xOffset + 34, 40, 58, 58);

            _pets[PetSlotType.Terrestrial_2].Bounds = new(xOffset + 125, 0, _petSize.X, _petSize.Y);
            _pets[PetSlotType.Terrestrial_2].PawRegion = new(xOffset + 34 + 125, 40, 58, 58);
            _pets[PetSlotType.Terrestrial_2].FallbackBounds = new(xOffset + 34 + 125, 40, 58, 58);

            xOffset = 60 + 34 + 125 + 220;
            _pets[PetSlotType.Aquatic_1].Bounds = new(xOffset, 0, _petSize.X, _petSize.Y);
            _pets[PetSlotType.Aquatic_1].PawRegion = new(xOffset + 34, 40, 58, 58);
            _pets[PetSlotType.Aquatic_1].FallbackBounds = new(xOffset + 34, 40, 58, 58);

            _pets[PetSlotType.Aquatic_2].Bounds = new(xOffset + 125, 0, _petSize.X, _petSize.Y);
            _pets[PetSlotType.Aquatic_2].PawRegion = new(xOffset + 34 + 125, 40, 58, 58);
            _pets[PetSlotType.Aquatic_2].FallbackBounds = new(xOffset + 34 + 125, 40, 58, 58);

            _slotRectangle = new(xOffset, 5, 188, 18);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (TemplatePresenter is not null)
            {                
                foreach (var pet in _pets.Values)
                {
                    pet.Pet ??= TemplatePresenter?.Template?.Pets?[pet.PetSlot];
                    pet.Draw(this, spriteBatch, RelativeMousePosition);
                }

                applied = true;

                Color borderColor = Color.Black;
                Rectangle b = _healthRectangle;

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
                    var flag = _selectorAnchor.PetSlot is PetSlotType.Terrestrial_1 or PetSlotType.Terrestrial_2 ? Enviroment.Terrestrial : Enviroment.Aquatic;

                    if (s.Hovered && s.Pet.Enviroment.HasFlag(flag))
                    {
                        var otherSlot = _selectorAnchor.PetSlot switch
                        {
                            PetSlotType.Aquatic_1 => PetSlotType.Aquatic_2,
                            PetSlotType.Aquatic_2 => PetSlotType.Aquatic_1,
                            PetSlotType.Terrestrial_1 => PetSlotType.Terrestrial_2,
                            _ => PetSlotType.Terrestrial_1,
                        };

                        if (TemplatePresenter.Template.Pets[otherSlot] != s.Pet)
                        {
                            _selectorAnchor.Pet = s.Pet;
                            TemplatePresenter.Template.Pets[_selectorAnchor.PetSlot] = s.Pet;
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

            foreach (var pet in _pets.Values)
            {
                if (pet.Hovered)
                {
                    if (!_selectorOpen)
                    {
                        _selectorAnchor = pet;
                        GetSelectablePets();
                    }

                    _selectorOpen = !_selectorOpen;
                }
            }
        }

        protected override void ApplyTemplate()
        {
            if (TemplatePresenter is not null)
            {
                foreach (var pet in _pets.Values)
                {
                    pet.Pet = TemplatePresenter?.Template?.Pets?[pet.PetSlot];
                }
            }

            base.ApplyTemplate();
        }

        private void DrawSelector(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_selectorAnchor is not null)
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
                    var flag = _selectorAnchor.PetSlot is PetSlotType.Terrestrial_1 or PetSlotType.Terrestrial_2 ? Enviroment.Terrestrial : Enviroment.Aquatic;

                    if (s.Pet.Enviroment.HasFlag(flag))
                    {
                        s.Draw(this, spriteBatch, RelativeMousePosition);
                    }
                }

                spriteBatch.DrawStringOnCtrl(this, string.Format("{0} Pets", _selectorAnchor.PetSlot is PetSlotType.Terrestrial_1 or PetSlotType.Terrestrial_2 ? "Terrestrial" : "Aquatic"), Content.DefaultFont18, new Rectangle(_selectorBounds.Left, _selectorBounds.Bottom - 12 - Content.DefaultFont18.LineHeight, _selectorBounds.Width, Content.DefaultFont18.LineHeight), Color.White, false, HorizontalAlignment.Center);
            }
        }

        private void GetSelectablePets()
        {
            if (TemplatePresenter is not null)
            {
                var flag = _selectorAnchor.PetSlot is PetSlotType.Terrestrial_1 or PetSlotType.Terrestrial_2 ? Enviroment.Terrestrial : Enviroment.Aquatic;
                var pets = _selectablePets.Where(e => e.Pet.Enviroment.HasFlag(flag));

                int columns = Math.Min(pets.Count(), 6);
                int rows = (int)Math.Ceiling(pets.Count() / (double)columns);

                int column = 0;
                int row = 0;
                int petSize = (int)(_petSize.X / 1.25);

                _selectorBounds = new(_selectorAnchor.Bounds.X - ((((petSize - 28) * columns) + 8) / 2 - ((petSize - 28) / 2)), _selectorAnchor.Bounds.Bottom - 32, ((petSize - 28) * columns) + 4 + 28, ((petSize - 28) * rows) + 28 + 40);

                foreach (var p in pets)
                {
                    p.Bounds = new(_selectorBounds.Left + (column * (petSize - 28)), _selectorBounds.Top + (row * (petSize - 28)), petSize, petSize);
                    p.PawRegion = new(_selectorBounds.Left + (column * (petSize - 28)) + (int)(34 / 1.25), _selectorBounds.Top + (row * (petSize - 28)) + (int)(40 / 1.25), (int)(58 / 1.25), (int)(58 / 1.25));

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
