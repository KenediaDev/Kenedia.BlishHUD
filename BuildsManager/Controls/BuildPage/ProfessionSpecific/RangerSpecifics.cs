using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class RangerSpecifics : ProfessionSpecifics
    {
        private readonly PetSelector _petSelector;
        private readonly Dictionary<PetSlotType, PetControl> _pets = new()
        {
            {PetSlotType.Terrestrial_1, new() { PetSlot = PetSlotType.Terrestrial_1 }},
            {PetSlotType.Terrestrial_2, new() { PetSlot = PetSlotType.Terrestrial_2 }},
            {PetSlotType.Aquatic_1, new() { PetSlot = PetSlotType.Aquatic_1 }},
            {PetSlotType.Aquatic_2, new() { PetSlot = PetSlotType.Aquatic_2 }},
        };
        private Point _petSize = new(120);
        private PetControl _selectorAnchor;

        public RangerSpecifics(TemplatePresenter template) : base(template)
        {
            foreach (var pet in _pets)
            {
                pet.Value.Parent = this;
                pet.Value.RightClickAction = Pet_Click;
                pet.Value.LeftClickAction = Pet_Click;
            }

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

                    _petSelector?.Hide();
                }
            };

        }

        private void Pet_Click(PetControl sender)
        {
            SetSelector(sender);
        }

        private void Mouse_RightMouseButtonPressed(object sender, MouseEventArgs e)
        {

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int xOffset = 60;
            _pets[PetSlotType.Terrestrial_1].SetBounds(new(xOffset, 0, _petSize.X, _petSize.Y));
            _pets[PetSlotType.Terrestrial_2].SetBounds(new(xOffset + 125, 0, _petSize.X, _petSize.Y));

            xOffset = 60 + 34 + 125 + 220;
            _pets[PetSlotType.Aquatic_1].SetBounds(new(xOffset, 0, _petSize.X, _petSize.Y));
            _pets[PetSlotType.Aquatic_2].SetBounds(new(xOffset + 125, 0, _petSize.X, _petSize.Y));
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

        }

        private void SetSelector(PetControl skillIcon)
        {
            _selectorAnchor = skillIcon;

            _petSelector.Anchor = skillIcon;
            _petSelector.ZIndex = ZIndex + 1000;
            _petSelector.SelectedItem = skillIcon.Pet;
            _petSelector.AnchorOffset = new(0, 20);
            _petSelector.Label = skillIcon.PetSlot switch
            {
                PetSlotType.Terrestrial_1 or PetSlotType.Terrestrial_2 => strings.TerrestrialPets,
                PetSlotType.Aquatic_1 or PetSlotType.Aquatic_2 => strings.AquaticPets,
                _ => string.Empty,
            };

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

                _petSelector.SetItems(BuildsManager.Data.Pets.Values.Where(e => e.Enviroment.HasFlag(flag)).OrderBy(e => e.Order));
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _petSelector?.Dispose();

            Input.Mouse.RightMouseButtonPressed -= Mouse_RightMouseButtonPressed;
        }
    }
}
