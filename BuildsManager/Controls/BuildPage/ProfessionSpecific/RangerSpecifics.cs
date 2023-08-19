using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.DataModels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class RangerSpecifics : ProfessionSpecifics
    {
        private readonly PetSelector _petSelector;
        private readonly Dictionary<PetSlotType, PetIcon> _pets = new()
        {
            {PetSlotType.Terrestrial_1, new() { PetSlot = PetSlotType.Terrestrial_1 }},
            {PetSlotType.Terrestrial_2, new() { PetSlot = PetSlotType.Terrestrial_2 }},
            {PetSlotType.Aquatic_1, new() { PetSlot = PetSlotType.Aquatic_1 }},
            {PetSlotType.Aquatic_2, new() { PetSlot = PetSlotType.Aquatic_2 }},
        };
        private readonly List<PetIcon> _selectablePets = new();
        private Point _petSize = new(120);
        private PetIcon _selectorAnchor;

        public RangerSpecifics(TemplatePresenter template) : base(template)
        {
            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
            _petSelector = new()
            {
                Parent = Graphics.SpriteScreen,
                Visible = false,
                OnClickAction = (pet) =>
                {
                    var otherSlot = _selectorAnchor.PetSlot switch
                    {
                        PetSlotType.Aquatic_1 => PetSlotType.Aquatic_2,
                        PetSlotType.Aquatic_2 => PetSlotType.Aquatic_1,
                        PetSlotType.Terrestrial_1 => PetSlotType.Terrestrial_2,
                        _ => PetSlotType.Terrestrial_1,
                    };

                    if (TemplatePresenter.Template.Pets[otherSlot] != pet)
                    {
                        _selectorAnchor.Pet = pet;
                        TemplatePresenter.Template.Pets[_selectorAnchor.PetSlot] = pet;
                    };
                }
            };

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
            }
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {

        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            foreach (var pet in _pets)
            {
                if (pet.Value.Hovered)
                {
                    SetSelector(pet);
                }
            }
        }

        private void SetSelector(KeyValuePair<PetSlotType, PetIcon> skillIcon)
        {
            _selectorAnchor = skillIcon.Value;
            _petSelector.ZIndex = int.MaxValue;
            _petSelector.Anchor = this;
            //_petSelector.SelectedItem = skillIcon.Value.Pet;

            GetSelectablePets();
        }

        protected override void ApplyTemplate()
        {
            base.ApplyTemplate();
            if (TemplatePresenter is not null)
            {
                foreach (var pet in _pets.Values)
                {
                    pet.Pet = TemplatePresenter?.Template?.Pets?[pet.PetSlot];
                }
            }
        }

        private void GetSelectablePets()
        {
            if (TemplatePresenter is not null)
            {
                var flag = _selectorAnchor.PetSlot is PetSlotType.Terrestrial_1 or PetSlotType.Terrestrial_2 ? Enviroment.Terrestrial : Enviroment.Aquatic;
                var pets = _selectablePets.OrderBy(e => e.Pet.Order).Where(e => e.Pet.Enviroment.HasFlag(flag));
                _petSelector.SetItems(pets.Select(e => e.Pet));
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _petSelector?.Dispose();

            Input.Mouse.LeftMouseButtonPressed -= Mouse_LeftMouseButtonPressed;
        }
    }
}
