using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static Blish_HUD.ContentService;
using Kenedia.Modules.BuildsManager.Models;
using Gw2Sharp;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{

    public class SkillsBar : Control
    {
        private readonly DetailedTexture _selectingFrame = new(157147);
        private readonly DetailedTexture _aquaticTexture = new(1988170);
        private readonly DetailedTexture _terrestrialTexture = new(1988171);

        private TemplatePresenter _templatePresenter;

        private readonly SkillIconCollection _skillIcons = new(true);

        private readonly List<SkillIcon> _selectableSkills = new();
        private SkillSlot _selectedSkillSlot;
        private Rectangle _selectorBounds;
        private SkillIcon _selectorAnchor;
        private bool _selectorOpen = false;
        private double _lastOpen;
        private int _skillSize;

        public SkillsBar(TemplatePresenter templatePresenter)
        {
            TemplatePresenter = templatePresenter;
            Tooltip = new SkillTooltip();

            Height = 125;
            ClipsBounds = false;
            ZIndex = int.MaxValue / 2;

            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        private bool AnyHovered
        {
            get
            {
                bool hovered = _selectorOpen && _selectorBounds.Contains(RelativeMousePosition);
                hovered = hovered || AbsoluteBounds.Contains(Input.Mouse.Position);

                return hovered;
            }
        }

        public bool IsSelecting
        {
            get
            {
                double timeSince = Common.Now() - _lastOpen;
                return _selectorOpen || timeSince <= 200;
            }
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
            }

            if (e.NewValue is not null)
            {
                e.NewValue.ProfessionChanged += Template_ProfessionChanged;
                e.NewValue.RaceChanged += Template_RaceChanged;
                e.NewValue.EliteSpecializationChanged += BuildTemplate_EliteSpecChanged;
                e.NewValue.LegendChanged += On_LegendChanged;
                e.NewValue.LoadedBuildFromCode += OnLoaded;
                e.NewValue.TemplateChanged += On_TemplateChanged;
            }
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

            foreach (var spair in TemplatePresenter.Template?.Skills)
            {
                _skillIcons[spair.Key].Skill = spair.Value;
                _skillIcons[spair.Key].Slot = spair.Key;
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

        private bool SeletorOpen
        {
            get => _selectorOpen;
            set
            {
                if (AnyHovered || _selectorOpen)
                {
                    _lastOpen = Common.Now();
                }

                _selectorOpen = value;
            }
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

                _skillIcons[spair.Key].Bounds = new((spair.Key.HasFlag(SkillSlotType.Terrestrial) ? _terrestrialTexture.Bounds.Right : _aquaticTexture.Bounds.Right) + 5 + pos, ((Height - _skillSize - 14) / 2) + offsetY, _skillSize, _skillSize);
                _skillIcons[spair.Key].Selector.Bounds = new((spair.Key.HasFlag(SkillSlotType.Terrestrial) ? _terrestrialTexture.Bounds.Right : _aquaticTexture.Bounds.Right) + 5 + pos, ((Height - _skillSize - 14) / 2) + offsetY - 13, _skillSize, 15);
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _terrestrialTexture.Draw(this, spriteBatch, RelativeMousePosition, Color.White);
            _aquaticTexture.Draw(this, spriteBatch, RelativeMousePosition, Color.White);

            SkillSlotType state = TemplatePresenter.LegendSlot == LegendSlotType.TerrestrialActive ? SkillSlotType.Active : SkillSlotType.Inactive;
            string txt = string.Empty;

            foreach (var spair in TemplatePresenter.Template.Skills)
            {
                if (spair.Key.HasFlag(state))
                {
                    _skillIcons[spair.Key].Draw(this, spriteBatch, true, RelativeMousePosition);
                    if (!SeletorOpen && _skillIcons[spair.Key].Hovered && _skillIcons[spair.Key].Skill is not null)
                    {
                        if (Tooltip is SkillTooltip tooltip)
                        {
                            tooltip.Skill = _skillIcons[spair.Key].Skill;
                        }
                    }
                }
            }

            if (SeletorOpen)
            {
                DrawSelector(spriteBatch, bounds);
            }

        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e)
        {
            base.OnRightMouseButtonPressed(e);

            foreach (var skillIcon in _skillIcons)
            {
                if (skillIcon.Value.Hovered)
                {
                    SeletorOpen = _selectorAnchor != skillIcon.Value || !SeletorOpen;
                    _selectorAnchor = skillIcon.Value;
                    GetSelectableSkills(skillIcon.Key);
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            foreach (var skillIcon in _skillIcons)
            {
                if (skillIcon.Value.Selector.Hovered)
                {
                    SeletorOpen = _selectorAnchor != skillIcon.Value || !SeletorOpen;
                    _selectorAnchor = skillIcon.Value;
                    GetSelectableSkills(skillIcon.Key);
                }
            }
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (SeletorOpen)
            {
                SkillSlotType enviromentState = _selectorAnchor.Slot.GetEnviromentState();
                bool terrestrial = _selectorAnchor.Slot.HasFlag(SkillSlotType.Terrestrial);

                foreach (var s in _selectableSkills)
                {
                    if (s.Hovered)
                    {
                        if (terrestrial || TemplatePresenter.Template.Profession == Gw2Sharp.Models.ProfessionType.Revenant || !s.Skill.Flags.HasFlag(SkillFlag.NoUnderwater))
                        {
                            if (TemplatePresenter.Template.Skills.HasSkill(s.Skill, enviromentState))
                            {
                                var slot = TemplatePresenter.Template.Skills.GetSkillSlot(s.Skill, enviromentState);
                                TemplatePresenter.Template.Skills[slot] = _selectorAnchor.Skill;
                                _skillIcons[slot].Skill = _selectorAnchor.Skill;
                            }

                            _selectorAnchor.Skill = s.Skill;
                            _skillIcons[_selectorAnchor.Slot].Skill = s.Skill;
                            TemplatePresenter.Template.Skills[_selectorAnchor.Slot] = s.Skill;

                            SeletorOpen = false;
                            return;
                        }
                    }
                }
            }

            SeletorOpen = false;
        }

        private void GetSelectableSkills(SkillSlotType skillSlot)
        {
            _selectableSkills.Clear();

            var slot = skillSlot.HasFlag(SkillSlotType.Utility_1) ? SkillSlot.Utility :
            skillSlot.HasFlag(SkillSlotType.Utility_2) ? SkillSlot.Utility :
            skillSlot.HasFlag(SkillSlotType.Utility_3) ? SkillSlot.Utility :
            skillSlot.HasFlag(SkillSlotType.Heal) ? SkillSlot.Heal :
            SkillSlot.Elite;

            if (TemplatePresenter.Template.Profession != Gw2Sharp.Models.ProfessionType.Revenant)
            {
                var skills = BuildsManager.Data.Professions[TemplatePresenter.Template.Profession].Skills;
                var filteredSkills = skills.Where(e => e.Value.PaletteId > 0 && e.Value.Slot is not null && e.Value.Slot == slot && (e.Value.Specialization == 0 || TemplatePresenter.Template.HasSpecialization(e.Value.Specialization))).ToList();
                var racialSkills = TemplatePresenter.Template.Race != Core.DataModels.Races.None ? BuildsManager.Data.Races[TemplatePresenter.Template.Race]?.Skills.Where(e => e.Value.PaletteId > 0 && e.Value.Slot is not null && e.Value.Slot == slot).ToList() : new();
                if (racialSkills is not null) filteredSkills.AddRange(racialSkills);

                int columns = Math.Min(filteredSkills.Count(), 4);
                int rows = (int)Math.Ceiling(filteredSkills.Count() / (double)columns);
                _selectorBounds = new(_selectorAnchor.Bounds.X - (((_skillSize * columns) + 8) / 2 - (_skillSize / 2)), _selectorAnchor.Bounds.Bottom, (_skillSize * columns) + 4, (_skillSize * rows) + 40);
                _selectedSkillSlot = slot;
                int column = 0;
                int row = 0;

                foreach (var skill in filteredSkills.OrderBy(e => e.Value.Categories).ToList())
                {
                    _selectableSkills.Add(new() { Skill = skill.Value, Bounds = new(_selectorBounds.Left + 4 + (column * _skillSize), _selectorBounds.Top + 4 + (row * _skillSize), _skillSize - 4, _skillSize - 4) });
                    column++;

                    if (column > 3)
                    {
                        column = 0;
                        row++;
                    }
                }
            }
            else
            {
                List<Skill> filteredSkills = new();
                LegendSlotType legendSlot = LegendSlotType.TerrestrialActive;

                switch (skillSlot.GetEnviromentState())
                {
                    case SkillSlotType.Active | SkillSlotType.Aquatic:
                        legendSlot = LegendSlotType.AquaticActive;
                        break;

                    case SkillSlotType.Inactive | SkillSlotType.Aquatic:
                        legendSlot = LegendSlotType.AquaticInactive;
                        break;

                    case SkillSlotType.Active | SkillSlotType.Terrestrial:
                        legendSlot = LegendSlotType.TerrestrialActive;
                        break;

                    case SkillSlotType.Inactive | SkillSlotType.Terrestrial:
                        legendSlot = LegendSlotType.TerrestrialInactive;
                        break;
                }

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

                int columns = 4;
                int rows = (int)Math.Ceiling(filteredSkills.Count() / (double)columns);
                _selectorBounds = new(_selectorAnchor.Bounds.X - (((_skillSize * columns) + 8) / 2 - (_skillSize / 2)), _selectorAnchor.Bounds.Bottom, (_skillSize * columns) + 4, (_skillSize * rows) + 40);
                _selectedSkillSlot = slot;

                int column = 0;
                int row = 0;
                foreach (var skill in filteredSkills)
                {
                    _selectableSkills.Add(new() { Skill = skill, Bounds = new(_selectorBounds.Left + 4 + (column * _skillSize), _selectorBounds.Top + 4 + (row * _skillSize), _skillSize - 4, _skillSize - 4) });
                    column++;

                    if (column > 3)
                    {
                        column = 0;
                        row++;
                    }
                }
            }
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

            if (_selectableSkills is not null)
            {
                foreach (var s in _selectableSkills)
                {
                    s.Draw(this, spriteBatch, _selectorAnchor.Slot.HasFlag(SkillSlotType.Terrestrial), RelativeMousePosition);
                }
            }

            spriteBatch.DrawStringOnCtrl(this, string.Format("{0} Skills", _selectedSkillSlot), Content.DefaultFont18, new Rectangle(_selectorBounds.Left, _selectorBounds.Bottom - 12 - Content.DefaultFont18.LineHeight, _selectorBounds.Width, Content.DefaultFont18.LineHeight), Color.White, false, HorizontalAlignment.Center);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Input.Mouse.LeftMouseButtonPressed -= Mouse_LeftMouseButtonPressed;
            _selectorAnchor?.Dispose();

            _aquaticTexture?.Dispose();
            _terrestrialTexture?.Dispose();
            _selectingFrame?.Dispose();

            foreach (var c in _skillIcons.Values)
            {
                c?.Dispose();
            }

            TemplatePresenter = null;
        }
    }
}
