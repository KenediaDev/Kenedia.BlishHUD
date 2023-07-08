using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static Blish_HUD.ContentService;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillsBar : Control
    {
        private readonly DetailedTexture _selectingFrame = new(157147);
        private readonly DetailedTexture _aquaticTexture = new(1988170);
        private readonly DetailedTexture _terrestrialTexture = new(1988171);

        private readonly List<DetailedTexture> _terrestrialSelectors = new()
        {
            new(157138, 157140),
            new(157138, 157140),
            new(157138, 157140),
            new(157138, 157140),
            new(157138, 157140),
        };

        private readonly List<DetailedTexture> _aquaticSelectors = new()
        {
            new(157138, 157140),
            new(157138, 157140),
            new(157138, 157140),
            new(157138, 157140),
            new(157138, 157140),
        };

        private Template _template;

        private readonly SkillIconCollection _aquaticSkills = new();
        private readonly SkillIconCollection _inactiveAquaticSkills = new();
        private readonly SkillIconCollection _terrestrialSkills = new();
        private readonly SkillIconCollection _inactiveTerrestrialSkills = new();

        private readonly SelectableSkillIconCollection _selectableSkills = new();
        private SkillSlot _selectedSkillSlot;
        private Rectangle _selectorBounds;
        private SkillIcon _selectorAnchor;

        private bool _selectorOpen = false;
        private double _lastOpen;
        private int _skillSize;

        public SkillsBar()
        {
            Height = 125;
            ClipsBounds = false;
            ZIndex = int.MaxValue / 2;

            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        private bool Terrestrial => Template != null && Template.BuildTemplate != null && Template.Terrestrial;

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

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, BuildTemplate_Loaded))
                {
                    if (temp != null) temp.BuildTemplate.Loaded -= BuildTemplate_Loaded;
                    if (_template != null) _template.BuildTemplate.Loaded += BuildTemplate_Loaded;

                    if (temp != null) temp.BuildTemplate.LegendChanged -= BuildTemplate_LegendChanged;
                    if (_template != null) _template.BuildTemplate.LegendChanged += BuildTemplate_LegendChanged;

                    if (temp != null) temp.BuildTemplate.ProfessionChanged -= Template_ProfessionChanged;
                    if (_template != null) _template.BuildTemplate.ProfessionChanged += Template_ProfessionChanged;

                    if (temp != null) temp.BuildTemplate.EliteSpecChanged -= BuildTemplate_EliteSpecChanged;
                    if (_template != null) _template.BuildTemplate.EliteSpecChanged += BuildTemplate_EliteSpecChanged; ;
                }
            }
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
            for (int i = 0; i < _aquaticSkills.Count; i++)
            {
                _aquaticSkills[(BuildSkillSlot)i].Skill = null;
                _inactiveAquaticSkills[(BuildSkillSlot)i].Skill = null;
                _terrestrialSkills[(BuildSkillSlot)i].Skill = null;
                _inactiveTerrestrialSkills[(BuildSkillSlot)i].Skill = null;
            }

            if (Template == null)
            {
                return;
            }

            foreach (var spair in Template.BuildTemplate.AquaticSkills)
            {
                _aquaticSkills[spair.Key].Skill = spair.Value?.GetEffectiveSkill(Template);
                _aquaticSkills[spair.Key].Slot = spair.Key;
            }

            foreach (var spair in Template.BuildTemplate.InactiveAquaticSkills)
            {
                _inactiveAquaticSkills[spair.Key].Skill = spair.Value?.GetEffectiveSkill(Template);
                _inactiveAquaticSkills[spair.Key].Slot = spair.Key;
            }

            foreach (var spair in Template.BuildTemplate.TerrestrialSkills)
            {
                _terrestrialSkills[spair.Key].Skill = spair.Value?.GetEffectiveSkill(Template);
                _terrestrialSkills[spair.Key].Slot = spair.Key;
            }

            foreach (var spair in Template.BuildTemplate.InactiveTerrestrialSkills)
            {
                _inactiveTerrestrialSkills[spair.Key].Skill = spair.Value?.GetEffectiveSkill(Template);
                _inactiveTerrestrialSkills[spair.Key].Slot = spair.Key;
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

            int offsetX = 90;
            int offsetY = 5;
            int secondRowY = 64;
            _skillSize = 64;
            int weponSize = 52;

            _terrestrialTexture.Bounds = new Rectangle(4, ((Height - _skillSize - 14) / 2) + offsetY, 42, 42);
            offsetX = _terrestrialTexture.Bounds.Right + 5;

            for (int i = 0; i < _terrestrialSkills.Count; i++)
            {
                var terrestrial = _terrestrialSkills[(BuildSkillSlot)i];
                terrestrial.Bounds = new Rectangle(offsetX, ((Height - _skillSize - 14) / 2) + offsetY, _skillSize, _skillSize);

                var terrestrialInactive = _inactiveTerrestrialSkills[(BuildSkillSlot)i];
                terrestrialInactive.Bounds = new Rectangle(offsetX, ((Height - _skillSize - 14) / 2) + offsetY, _skillSize, _skillSize);

                _terrestrialSelectors[i].Bounds = new Rectangle(offsetX, ((Height - _skillSize - 14) / 2) - 14 + offsetY, _skillSize, 15);

                offsetX += _skillSize;
            }

            _aquaticTexture.Bounds = new Rectangle(_terrestrialSkills[(BuildSkillSlot)4].Bounds.Right + 15, ((Height - _skillSize - 14) / 2) + offsetY, 42, 42);
            offsetX = _aquaticTexture.Bounds.Right + 5;

            for (int i = 0; i < _terrestrialSkills.Count; i++)
            {
                var aquatic = _aquaticSkills[(BuildSkillSlot)i];
                aquatic.Bounds = new Rectangle(offsetX, ((Height - _skillSize - 14) / 2) + offsetY, _skillSize, _skillSize);

                var aquaticInactive = _inactiveAquaticSkills[(BuildSkillSlot)i];
                aquaticInactive.Bounds = new Rectangle(offsetX, ((Height - _skillSize - 14) / 2) + offsetY, _skillSize, _skillSize);

                _aquaticSelectors[i].Bounds = new Rectangle(offsetX, ((Height - _skillSize - 14) / 2) - 14 + offsetY, _skillSize, 15);

                offsetX += _skillSize;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _terrestrialTexture.Draw(this, spriteBatch, RelativeMousePosition, Color.White);
            _aquaticTexture.Draw(this, spriteBatch, RelativeMousePosition, Color.White);

            var slot = Template?.LegendSlot;
            string txt = string.Empty;

            switch (slot)
            {
                case LegendSlot.AquaticActive:
                case LegendSlot.TerrestrialActive:

                    foreach (var s in _terrestrialSkills)
                    {
                        s.Value.Draw(this, spriteBatch, true, RelativeMousePosition);
                        if (!SeletorOpen && s.Value.Hovered && s.Value.Skill != null)
                        {
                            txt = s.Value.Skill.Name;
                        }
                    }

                    foreach (var s in _aquaticSkills)
                    {
                        s.Value.Draw(this, spriteBatch, false, RelativeMousePosition);
                        if (!SeletorOpen && s.Value.Hovered && s.Value.Skill != null)
                        {
                            txt = s.Value.Skill.Name;
                        }
                    }
                    break;

                case LegendSlot.TerrestrialInactive:
                case LegendSlot.AquaticInactive:
                    foreach (var s in _inactiveTerrestrialSkills)
                    {
                        s.Value.Draw(this, spriteBatch, true, RelativeMousePosition);
                        if (!SeletorOpen && s.Value.Hovered && s.Value.Skill != null)
                        {
                            txt = s.Value.Skill.Name;
                        }
                    }

                    foreach (var s in _inactiveAquaticSkills)
                    {
                        s.Value.Draw(this, spriteBatch, false, RelativeMousePosition);
                        if (!SeletorOpen && s.Value.Hovered && s.Value.Skill != null)
                        {
                            txt = s.Value.Skill.Name;
                        }
                    }
                    break;
            };

            foreach (var s in _aquaticSelectors)
            {
                s.Draw(this, spriteBatch, RelativeMousePosition);
            }

            foreach (var s in _terrestrialSelectors)
            {
                s.Draw(this, spriteBatch, RelativeMousePosition);
            }

            if (SeletorOpen)
            {
                DrawSelector(spriteBatch, bounds);
            }

            BasicTooltipText = txt;
        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e)
        {
            base.OnRightMouseButtonPressed(e);

            for (int i = 0; i < _terrestrialSkills.Count; i++)
            {
                SkillIcon skill = null;
                LegendSlot slot = LegendSlot.TerrestrialActive;

                switch (Template.LegendSlot)
                {
                    case LegendSlot.TerrestrialActive:
                    case LegendSlot.AquaticActive:
                        skill = _aquaticSkills[(BuildSkillSlot)i].Hovered ? _aquaticSkills[(BuildSkillSlot)i] : _terrestrialSkills[(BuildSkillSlot)i];
                        slot = _aquaticSkills[(BuildSkillSlot)i].Hovered ? LegendSlot.AquaticActive : LegendSlot.TerrestrialActive;
                        break;
                    case LegendSlot.TerrestrialInactive:
                    case LegendSlot.AquaticInactive:
                        skill = _inactiveAquaticSkills[(BuildSkillSlot)i].Hovered ? _inactiveAquaticSkills[(BuildSkillSlot)i] : _inactiveTerrestrialSkills[(BuildSkillSlot)i];
                        slot = _inactiveAquaticSkills[(BuildSkillSlot)i].Hovered ? LegendSlot.AquaticInactive : LegendSlot.TerrestrialInactive;
                        break;
                }

                if (skill.Hovered)
                {
                    if (Template.Profession != Gw2Sharp.Models.ProfessionType.Revenant || Template.BuildTemplate.Legends[slot] != null)
                    {
                        var skillSlot = i == 0 ? SkillSlot.Heal : i == 4 ? SkillSlot.Elite : SkillSlot.Utility;
                        _selectedSkillSlot = skillSlot;
                        SeletorOpen = _selectorAnchor != skill || !SeletorOpen;
                        _selectorAnchor = skill;
                        GetSelectableSkills(skillSlot, slot);
                        return;
                    }
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            for (int i = 0; i < _terrestrialSkills.Count; i++)
            {
                var skill = _terrestrialSkills[(BuildSkillSlot)i];
                var terrestrialSelector = _terrestrialSelectors[i];

                if (skill != null && ((skill.Hovered && GameService.Input.Mouse.State.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) || terrestrialSelector.Hovered))
                {
                    if (Template.Profession != Gw2Sharp.Models.ProfessionType.Revenant || Template.BuildTemplate.Legends[Template.LegendSlot] != null)
                    {
                        var skillSlot = i == 0 ? SkillSlot.Heal : i == 4 ? SkillSlot.Elite : SkillSlot.Utility;
                        _selectedSkillSlot = skillSlot;
                        SeletorOpen = _selectorAnchor != skill || !SeletorOpen;
                        _selectorAnchor = skill;
                        GetSelectableSkills(skillSlot);
                        return;
                    }
                }

                var aquaticSelector = _aquaticSelectors[i];
                skill = _aquaticSkills[(BuildSkillSlot)i];
                if (skill != null && ((skill.Hovered && GameService.Input.Mouse.State.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) || aquaticSelector.Hovered))
                {
                    if (Template.Profession != Gw2Sharp.Models.ProfessionType.Revenant || Template.BuildTemplate.Legends[Template.LegendSlot] != null)
                    {
                        var skillSlot = i == 0 ? SkillSlot.Heal : i == 4 ? SkillSlot.Elite : SkillSlot.Utility;
                        _selectedSkillSlot = skillSlot;
                        SeletorOpen = _selectorAnchor != skill || !SeletorOpen;
                        _selectorAnchor = skill;
                        GetSelectableSkills(skillSlot);
                        return;
                    }
                }
            }
        }

        private void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (SeletorOpen)
            {
                foreach (var s in _selectableSkills[_selectedSkillSlot])
                {
                    if (s.Hovered)
                    {
                        if (Terrestrial || Template.Profession == Gw2Sharp.Models.ProfessionType.Revenant || !s.Skill.Flags.HasFlag(Gw2Sharp.SkillFlag.NoUnderwater))
                        {
                            var targetSkills = Template.BuildTemplate.TerrestrialSkills;
                            var skillIcons = _terrestrialSkills;

                            switch (Template.LegendSlot)
                            {
                                case LegendSlot.AquaticActive:
                                    targetSkills = Template.BuildTemplate.AquaticSkills;
                                    skillIcons = _aquaticSkills;
                                    break;
                                case LegendSlot.TerrestrialActive:
                                    targetSkills = Template.BuildTemplate.TerrestrialSkills;
                                    skillIcons = _terrestrialSkills;
                                    break;
                                case LegendSlot.TerrestrialInactive:
                                    targetSkills = Template.BuildTemplate.InactiveTerrestrialSkills;
                                    skillIcons = _inactiveTerrestrialSkills;
                                    break;
                                case LegendSlot.AquaticInactive:
                                    targetSkills = Template.BuildTemplate.InactiveAquaticSkills;
                                    skillIcons = _inactiveAquaticSkills;
                                    break;
                            };

                            if (targetSkills.HasSkill(s.Skill))
                            {
                                var slot = targetSkills.GetSkillSlot(s.Skill);
                                targetSkills[slot] = _selectorAnchor.Skill;
                                skillIcons[slot].Skill = _selectorAnchor.Skill;
                            }

                            _selectorAnchor.Skill = s.Skill;
                            targetSkills[_selectorAnchor.Slot] = s.Skill;
                        }
                    }
                }
            }

            SeletorOpen = false;
        }

        private void GetSelectableSkills(SkillSlot skillSlot, LegendSlot legendSlot = LegendSlot.TerrestrialActive)
        {
            if (Template != null)
            {
                _selectableSkills[skillSlot].Clear();
                Template.LegendSlot = legendSlot;

                if (Template.Profession != Gw2Sharp.Models.ProfessionType.Revenant)
                {
                    var skills = BuildsManager.Data.Professions[Template.Profession].Skills;

                    var filteredSkills = skills.Where(e => e.Value.PaletteId > 0 && e.Value.Slot != null && e.Value.Slot == skillSlot && (e.Value.Specialization == 0 || Template.BuildTemplate.HasSpecialization(e.Value.Specialization))).ToList();
                    var racialSkills = Template.Race != Core.DataModels.Races.None ? BuildsManager.Data.Races[Template.Race]?.Skills.Where(e => e.Value.PaletteId > 0 && e.Value.Slot != null && e.Value.Slot == skillSlot).ToList() : new();
                    if (racialSkills != null) filteredSkills.AddRange(racialSkills);

                    int columns = Math.Min(filteredSkills.Count(), 4);
                    int rows = (int)Math.Ceiling(filteredSkills.Count() / (double)columns);
                    _selectorBounds = new(_selectorAnchor.Bounds.X - (((_skillSize * columns) + 8) / 2 - (_skillSize / 2)), _selectorAnchor.Bounds.Bottom, (_skillSize * columns) + 4, (_skillSize * rows) + 40);

                    int column = 0;
                    int row = 0;
                    foreach (var skill in filteredSkills.OrderBy(e => e.Value.Categories).ToList())
                    {
                        _selectableSkills[skillSlot].Add(new() { Skill = skill.Value, Bounds = new(_selectorBounds.Left + 4 + (column * _skillSize), _selectorBounds.Top + 4 + (row * _skillSize), _skillSize - 4, _skillSize - 4) });
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
                    if (Template.BuildTemplate.Legends[legendSlot] != null)
                    {
                        var skills = Template.BuildTemplate.Legends[legendSlot];

                        List<Skill> filteredSkills = new();
                        switch (skillSlot)
                        {
                            case SkillSlot.Heal:
                                filteredSkills.Add(Template.BuildTemplate.Legends[legendSlot].Heal);
                                break;
                            case SkillSlot.Elite:
                                filteredSkills.Add(Template.BuildTemplate.Legends[legendSlot].Elite);
                                break;
                            case SkillSlot.Utility:
                                filteredSkills.AddRange(Template.BuildTemplate.Legends[legendSlot].Utilities.Select(e => e.Value));
                                break;
                        }

                        int columns = 4;
                        int rows = (int)Math.Ceiling(filteredSkills.Count() / (double)columns);
                        _selectorBounds = new(_selectorAnchor.Bounds.X - (((_skillSize * columns) + 8) / 2 - (_skillSize / 2)), _selectorAnchor.Bounds.Bottom, (_skillSize * columns) + 4, (_skillSize * rows) + 40);

                        int column = 0;
                        int row = 0;
                        foreach (var skill in filteredSkills)
                        {
                            _selectableSkills[skillSlot].Add(new() { Skill = skill, Bounds = new(_selectorBounds.Left + 4 + (column * _skillSize), _selectorBounds.Top + 4 + (row * _skillSize), _skillSize - 4, _skillSize - 4) });
                            column++;

                            if (column > 3)
                            {
                                column = 0;
                                row++;
                            }
                        }
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

            foreach (var s in _selectableSkills[_selectedSkillSlot])
            {
                s.Draw(this, spriteBatch, Terrestrial, RelativeMousePosition);
            }

            spriteBatch.DrawStringOnCtrl(this, string.Format("{0} Skills", _selectedSkillSlot), Content.DefaultFont18, new Rectangle(_selectorBounds.Left, _selectorBounds.Bottom - 12 - Content.DefaultFont18.LineHeight, _selectorBounds.Width, Content.DefaultFont18.LineHeight), Color.White, false, HorizontalAlignment.Center);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Input.Mouse.LeftMouseButtonPressed -= Mouse_LeftMouseButtonPressed;
        }
    }
}
