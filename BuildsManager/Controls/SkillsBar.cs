using Blish_HUD.Controls;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Controls_Old.BuildPage;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using System.Text.RegularExpressions;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Blish_HUD.Input;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class SkillsBar : Container
    {
        private readonly int _skillSize = 64;
        private readonly DetailedTexture _selectingFrame = new(157147);
        private readonly DetailedTexture _aquaticTexture = new(1988170);
        private readonly DetailedTexture _terrestrialTexture = new(1988171);

        private readonly SkillSelector _skillSelector;
        private SkillSlotControl _selectorAnchor;

        public Dictionary<SkillSlotType, SkillSlotControl> Skills { get; } = [];

        public TemplatePresenter TemplatePresenter { get; }

        public Data Data { get; }

        public SkillsBar(TemplatePresenter templatePresenter, Data data)
        {
            TemplatePresenter = templatePresenter;
            Data = data;
            Height = 80;
            Width = 500;

            var enviroments = new[] { SkillSlotType.Terrestrial, SkillSlotType.Aquatic };
            var states = new[] { SkillSlotType.Active, SkillSlotType.Inactive };
            var slots = new[] { SkillSlotType.Heal, SkillSlotType.Utility_1, SkillSlotType.Utility_2, SkillSlotType.Utility_3, SkillSlotType.Elite };

            foreach (var state in states)
            {
                foreach (var enviroment in enviroments)
                {
                    foreach (var slot in slots)
                    {
                        var skillSlot = slot | state | enviroment;
                        Skills[skillSlot] = new SkillSlotControl(skillSlot, templatePresenter) { Parent = this, ShowSelector = true, };
                    }
                }
            }

            _skillSelector = new()
            {
                Parent = Graphics.SpriteScreen,
                Visible = false,
                OnClickAction = (skill) =>
                {
                    TemplatePresenter?.Template.SelectSkill(_selectorAnchor.SkillSlot, skill);
                    _skillSelector?.Hide();
                    return;
                }
            };

            TemplatePresenter.ProfessionChanged += TemplatePresenter_ProfessionChanged;
            TemplatePresenter.TemplateChanged += TemplatePresenter_TemplateChanged;
            TemplatePresenter.LegendSlotChanged += TemplatePresenter_LegendSlotChanged;
        }

        private void TemplatePresenter_LegendSlotChanged(object sender, ValueChangedEventArgs<LegendSlotType> e)
        {
            SetSkillsVisibility();
        }

        private void TemplatePresenter_TemplateChanged(object sender, ValueChangedEventArgs<Template> e)
        {
            SetSkillsVisibility();
        }

        private void TemplatePresenter_ProfessionChanged(object sender, ValueChangedEventArgs<ProfessionType> e)
        {
            SetSkillsVisibility();
        }

        private void SetSkillsVisibility()
        {
            var state = TemplatePresenter.Template?.Profession is ProfessionType.Revenant && TemplatePresenter.LegendSlot is LegendSlotType.TerrestrialInactive or LegendSlotType.AquaticInactive ? SkillSlotType.Inactive : SkillSlotType.Active;

            foreach (var skill in Skills)
            {
                skill.Value.Visible = skill.Key.HasFlag(state);
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _terrestrialTexture.Bounds = new Rectangle(5, 2, 42, 42);
            _aquaticTexture.Bounds = new Rectangle(_terrestrialTexture.Bounds.Right + (_skillSize * 5) + 20, 2, 42, 42);

            var size = new Point(_skillSize, _skillSize + 15);

            foreach (var spair in Skills)
            {
                int left = (spair.Key.IsTerrestrial() ? _terrestrialTexture.Bounds.Right : _aquaticTexture.Bounds.Right) + 5;
                int xOffset = spair.Key.GetSlotPosition() * size.X;

                Skills[spair.Key].SetBounds(new(left + xOffset, 0, size.X, size.Y));
            }
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

            foreach (var skillIcon in Skills)
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

            foreach (var skillIcon in Skills)
            {
                if (skillIcon.Value.IsSelectorHovered)
                {
                    SetSelector(skillIcon);
                }
            }
        }
        private void SetSelector(KeyValuePair<SkillSlotType, SkillSlotControl> skillCtrl)
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
            if (TemplatePresenter?.Template?.Profession is null)
                return;

            var slot = skillSlot.HasFlag(SkillSlotType.Utility_1) ? SkillSlot.Utility :
            skillSlot.HasFlag(SkillSlotType.Utility_2) ? SkillSlot.Utility :
            skillSlot.HasFlag(SkillSlotType.Utility_3) ? SkillSlot.Utility :
            skillSlot.HasFlag(SkillSlotType.Heal) ? SkillSlot.Heal :
            SkillSlot.Elite;

            if (TemplatePresenter?.Template?.Profession != ProfessionType.Revenant)
            {
                var skills = Data.Professions[TemplatePresenter.Template.Profession].Skills;
                var filteredSkills = skills.Where(e => e.Value.PaletteId > 0 && e.Value.Slot is not null && e.Value.Slot == slot && (e.Value.Specialization == 0 || TemplatePresenter.Template.HasSpecialization(e.Value.Specialization))).ToList();

                var racialSkills = TemplatePresenter.Template.Race != Core.DataModels.Races.None ? Data.Races[TemplatePresenter.Template.Race]?.Skills.Where(e => e.Value.PaletteId > 0 && e.Value.Slot is not null && e.Value.Slot == slot).ToList() : new();
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

            Skills.Values?.DisposeAll();
        }
    }
}
