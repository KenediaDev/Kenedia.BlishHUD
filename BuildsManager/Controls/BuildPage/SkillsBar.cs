using Blish_HUD.Controls;
using Kenedia.Modules.Core.DataModels;
using Blish_HUD.Input;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Kenedia.Modules.BuildsManager.Models;
using Gw2Sharp;
using System.Text.RegularExpressions;
using Kenedia.Modules.BuildsManager.Res;
using Gw2Sharp.Models;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{

    public class SkillsBar : Container
    {
        private readonly DetailedTexture _selectingFrame = new(157147);
        private readonly DetailedTexture _aquaticTexture = new(1988170);
        private readonly DetailedTexture _terrestrialTexture = new(1988171);
        private readonly SkillSelector _skillSelector;

        private TemplatePresenter _templatePresenter;

        private readonly SkillControlCollection _skillIcons = new(true);

        private readonly List<SkillControl> _selectableSkills = new();

        private SkillControl _selectorAnchor;

        private int _skillSize;

        public SkillsBar(TemplatePresenter templatePresenter)
        {
            TemplatePresenter = templatePresenter;

            Height = 125;
            ClipsBounds = false;
            ZIndex = int.MaxValue / 2;

            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;

            _skillSelector = new()
            {
                Parent = Graphics.SpriteScreen,
                Visible = false,
                OnClickAction = (skill) =>
                {
                    SkillSlotType enviromentState = _selectorAnchor.Slot.GetEnviromentState();
                    bool terrestrial = _selectorAnchor.Slot.HasFlag(SkillSlotType.Terrestrial);

                    if (terrestrial || TemplatePresenter.Template.Profession == ProfessionType.Revenant || !skill.Flags.HasFlag(SkillFlag.NoUnderwater))
                    {
                        if (TemplatePresenter.Template.Skills.HasSkill(skill, enviromentState))
                        {
                            var slot = TemplatePresenter.Template.Skills.GetSkillSlot(skill, enviromentState);
                            TemplatePresenter.Template.Skills[slot] = _selectorAnchor.Skill;
                            _skillIcons[slot].Skill = _selectorAnchor.Skill;
                        }

                        _selectorAnchor.Skill = skill;
                        _skillIcons[_selectorAnchor.Slot].Skill = skill;
                        TemplatePresenter.Template.Skills[_selectorAnchor.Slot] = skill;

                        _skillSelector?.Hide();
                        return;
                    }
                }
            };
        }

        public TemplatePresenter TemplatePresenter
        {
            get => _templatePresenter; set => Common.SetProperty(ref _templatePresenter, value, SetTemplatePresenter);
        }

        private void SetTemplatePresenter(object sender, Core.Models.ValueChangedEventArgs<TemplatePresenter> e)
        {
            if (e.OldValue is not null)
            {
                e.OldValue.ProfessionChanged -= Template_ProfessionChanged;
                e.OldValue.RaceChanged -= Template_RaceChanged;
                e.OldValue.EliteSpecializationChanged -= BuildTemplate_EliteSpecChanged;
                e.OldValue.LegendChanged -= On_LegendChanged;
                e.OldValue.LoadedBuildFromCode -= OnLoaded;
                e.OldValue.TemplateChanged -= On_TemplateChanged;
                e.OldValue.LegendSlotChanged -= On_LegendSlotChanged;
            }

            if (e.NewValue is not null)
            {
                e.NewValue.ProfessionChanged += Template_ProfessionChanged;
                e.NewValue.RaceChanged += Template_RaceChanged;
                e.NewValue.EliteSpecializationChanged += BuildTemplate_EliteSpecChanged;
                e.NewValue.LegendChanged += On_LegendChanged;
                e.NewValue.LoadedBuildFromCode += OnLoaded;
                e.NewValue.TemplateChanged += On_TemplateChanged;
                e.NewValue.LegendSlotChanged += On_LegendSlotChanged;
            }
        }

        private void On_LegendSlotChanged(object sender, Core.Models.ValueChangedEventArgs<LegendSlotType> e)
        {
            SetSkills();
        }

        private void On_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<Template> e)
        {
            SetSkills();
        }

        private void Template_RaceChanged(object sender, Core.Models.ValueChangedEventArgs<Core.DataModels.Races> e)
        {
            SetSkills();
        }

        private void On_LegendChanged(object sender, DictionaryItemChangedEventArgs<LegendSlotType, Legend> e)
        {
            SetSkills();
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            SetSkills();
        }

        private void BuildTemplate_EliteSpecChanged(object sender, EventArgs e)
        {
            SetSkills();
        }

        private void Template_ProfessionChanged(object sender, EventArgs e)
        {
            SetSkills();
        }

        private void SetSkills()
        {
            _skillIcons.Wipe();

            if (TemplatePresenter?.Template == null)
            {
                return;
            }

            var prof = TemplatePresenter.Template.Profession;
            bool active = TemplatePresenter.LegendSlot is LegendSlotType.TerrestrialActive or LegendSlotType.AquaticActive;

            foreach (var spair in TemplatePresenter?.Template?.Skills)
            {
                _skillIcons[spair.Key].Parent = this;
                _skillIcons[spair.Key].Skill = spair.Value;
                _skillIcons[spair.Key].Slot = spair.Key;
                _skillIcons[spair.Key].Visible  = 
                    prof == ProfessionType.Revenant  ? active  ? spair.Key.HasFlag(SkillSlotType.Active) : spair.Key.HasFlag(SkillSlotType.Inactive) : spair.Key.HasFlag(SkillSlotType.Active);
            }
        }

        private void BuildTemplate_LegendChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetSkills();
        }

        private void BuildTemplate_Loaded(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetSkills();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int offsetY = 5;
            _skillSize = 64;

            _terrestrialTexture.Bounds = new Rectangle(5, ((Height - _skillSize - 14) / 2) + offsetY, 42, 42);
            _aquaticTexture.Bounds = new Rectangle(_terrestrialTexture.Bounds.Right + (_skillSize * 5) + 20, ((Height - _skillSize - 14) / 2) + offsetY, 42, 42);

            foreach (var spair in _skillIcons)
            {
                var key = spair.Key;

                int pos = key.HasFlag(SkillSlotType.Heal) ? _skillSize * 0 : key.HasFlag(SkillSlotType.Utility_1) ? _skillSize * 1 : key.HasFlag(SkillSlotType.Utility_2) ? _skillSize * 2 : key.HasFlag(SkillSlotType.Utility_3) ? _skillSize * 3 : _skillSize * 4;

                _skillIcons[spair.Key].SetBounds(new((spair.Key.HasFlag(SkillSlotType.Terrestrial) ? _terrestrialTexture.Bounds.Right : _aquaticTexture.Bounds.Right) + 5 + pos, ((Height - _skillSize - 14) / 2) + offsetY, _skillSize, _skillSize + 15));
                //_skillIcons[spair.Key].Selector.Bounds = new((spair.Key.HasFlag(SkillSlotType.Terrestrial) ? _terrestrialTexture.Bounds.Right : _aquaticTexture.Bounds.Right) + 5 + pos, ((Height - _skillSize - 14) / 2) + offsetY - 13, _skillSize, 15);
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            _terrestrialTexture.Draw(this, spriteBatch, RelativeMousePosition, Color.White);
            _aquaticTexture.Draw(this, spriteBatch, RelativeMousePosition, Color.White);
        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e)
        {
            base.OnRightMouseButtonPressed(e);

            foreach (var skillIcon in _skillIcons)
            {
                if (skillIcon.Value.MouseOver)
                {
                    SetSelector(skillIcon);
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            foreach (var skillIcon in _skillIcons)
            {
                if (skillIcon.Value.IsSelectorHovered)
                {
                    SetSelector(skillIcon);
                }
            }
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {

        }

        private void SetSelector(KeyValuePair<SkillSlotType, SkillControl> skillCtrl)
        {
            _selectorAnchor = skillCtrl.Value;

            _skillSelector.Anchor = skillCtrl.Value;
            _skillSelector.AnchorOffset = new(-2, 10);
            _skillSelector.ZIndex = ZIndex + 100;
            _skillSelector.SelectedItem = skillCtrl.Value.Skill;

            var slot = skillCtrl.Key;

            slot &= ~(SkillSlotType.Aquatic | SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Active);
            _skillSelector.Label = strings.ResourceManager.GetString($"{Regex.Replace($"{slot.ToString().Trim()}", @"[_0-9]", "")}Skills");
            _skillSelector.Enviroment = skillCtrl.Key.HasFlag(SkillSlotType.Aquatic) ? Enviroment.Aquatic : Enviroment.Terrestrial;

            GetSelectableSkills(skillCtrl.Key);
        }

        private void GetSelectableSkills(SkillSlotType skillSlot)
        {
            _selectableSkills.Clear();

            if (TemplatePresenter?.Template?.Profession is null)
                return;

            var slot = skillSlot.HasFlag(SkillSlotType.Utility_1) ? SkillSlot.Utility :
            skillSlot.HasFlag(SkillSlotType.Utility_2) ? SkillSlot.Utility :
            skillSlot.HasFlag(SkillSlotType.Utility_3) ? SkillSlot.Utility :
            skillSlot.HasFlag(SkillSlotType.Heal) ? SkillSlot.Heal :
            SkillSlot.Elite;

            if (TemplatePresenter?.Template?.Profession != ProfessionType.Revenant)
            {
                var skills = BuildsManager.Data.Professions[TemplatePresenter.Template.Profession].Skills;
                var filteredSkills = skills.Where(e => e.Value.PaletteId > 0 && e.Value.Slot is not null && e.Value.Slot == slot && (e.Value.Specialization == 0 || TemplatePresenter.Template.HasSpecialization(e.Value.Specialization))).ToList();

                var racialSkills = TemplatePresenter.Template.Race != Core.DataModels.Races.None ? BuildsManager.Data.Races[TemplatePresenter.Template.Race]?.Skills.Where(e => e.Value.PaletteId > 0 && e.Value.Slot is not null && e.Value.Slot == slot).ToList() : new();
                if (racialSkills is not null) filteredSkills.AddRange(racialSkills);

                _skillSelector.SetItems(filteredSkills.OrderBy(e => e.Value.Categories).Select(e => e.Value));
            }
            else
            {
                List<Skill> filteredSkills = new();
                LegendSlotType legendSlot = skillSlot.GetEnviromentState() switch
                {
                    SkillSlotType.Active | SkillSlotType.Aquatic => LegendSlotType.AquaticActive,
                    SkillSlotType.Inactive | SkillSlotType.Aquatic => LegendSlotType.AquaticInactive,
                    SkillSlotType.Active | SkillSlotType.Terrestrial => LegendSlotType.TerrestrialActive,
                    SkillSlotType.Inactive | SkillSlotType.Terrestrial => LegendSlotType.TerrestrialInactive,
                    _ => LegendSlotType.TerrestrialActive,
                };

                var skills = TemplatePresenter.Template?.Legends[legendSlot];

                if (skills is not null)
                {
                    switch (slot)
                    {
                        case SkillSlot.Heal:
                            filteredSkills.Add(skills.Heal);
                            break;
                        case SkillSlot.Elite:
                            filteredSkills.Add(skills.Elite);
                            break;
                        case SkillSlot.Utility:
                            filteredSkills.AddRange(skills.Utilities.Select(e => e.Value));
                            break;
                    }
                }

                _skillSelector.SetItems(filteredSkills);
            }

        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Input.Mouse.LeftMouseButtonPressed -= Mouse_LeftMouseButtonPressed;
            _selectorAnchor?.Dispose();

            _aquaticTexture?.Dispose();
            _terrestrialTexture?.Dispose();
            _selectingFrame?.Dispose();
            _skillSelector?.Dispose();

            foreach (var c in _skillIcons.Values)
            {
                c?.Dispose();
            }

            TemplatePresenter = null;
        }
    }
}
