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

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillsBar : Control
    {
        private readonly DetailedTexture _selectingFrame = new(157147);
        private readonly DetailedTexture _aquaticTexture = new(1988170);
        private readonly DetailedTexture _terrestrialTexture = new(1988171);

        private readonly List<DetailedTexture> _selectors = new()
        {
            new(157138, 157140),
            new(157138, 157140),
            new(157138, 157140),
            new(157138, 157140),
            new(157138, 157140),
        };

        private Template _template;
        private readonly WeaponSkillIconCollection _terrestrialWeaponSkills = new();
        private readonly WeaponSkillIconCollection _terrestrialInactiveWeaponSkills = new();
        private readonly WeaponSkillIconCollection _aquaticWeaponSkills = new();
        private readonly WeaponSkillIconCollection _aquaticInactiveWeaponSkills = new();

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

            _terrestrialWeaponSkills[WeaponSkillSlot.Weapon_1].AutoCastTexture = AsyncTexture2D.FromAssetId(157150);
            _aquaticWeaponSkills[WeaponSkillSlot.Weapon_1].AutoCastTexture = AsyncTexture2D.FromAssetId(157150);
            _aquaticInactiveWeaponSkills[WeaponSkillSlot.Weapon_1].AutoCastTexture = AsyncTexture2D.FromAssetId(157150);
            _terrestrialInactiveWeaponSkills[WeaponSkillSlot.Weapon_1].AutoCastTexture = AsyncTexture2D.FromAssetId(157150);

            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        private bool Terrestrial => Template != null && Template.BuildTemplate != null && Template.BuildTemplate.Terrestrial;

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
                if (Common.SetProperty(ref _template, value, ApplyTemplate, value != null))
                {
                    if (temp != null) temp.Changed -= TemplateChanged;

                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        private void TemplateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyTemplate();
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

        public void ApplyTemplate()
        {
            for (int i = 0; i < _terrestrialWeaponSkills.Count; i++)
            {
                _terrestrialWeaponSkills[(WeaponSkillSlot)i].Skill = null;
                _terrestrialInactiveWeaponSkills[(WeaponSkillSlot)i].Skill = null;
                _aquaticWeaponSkills[(WeaponSkillSlot)i].Skill = null;
                _aquaticInactiveWeaponSkills[(WeaponSkillSlot)i].Skill = null;
            }

            for (int i = 0; i < _aquaticSkills.Count; i++)
            {
                _aquaticSkills[(BuildSkillSlot)i].Skill = null;
                _inactiveAquaticSkills[(BuildSkillSlot)i].Skill = null;
                _terrestrialSkills[(BuildSkillSlot)i].Skill = null;
                _inactiveTerrestrialSkills[(BuildSkillSlot)i].Skill = null;
            }

            if (Template.GearTemplate.Gear[GearSlot.MainHand] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.MainHand].WeaponType;

                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && e.Value.Slot != null && (int)e.Value.Slot <= 3 && e.Value.PrevChain == null && e.Value.Specialization == 0))
                {
                    _terrestrialWeaponSkills[s.Value.Slot.GetSkillSlot()].Skill = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.OffHand] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.OffHand].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && (int)e.Value.Slot > 3 && (int)e.Value.Slot <= 5 && e.Value.PrevChain == null && e.Value.Specialization == 0))
                {
                    _terrestrialWeaponSkills[s.Value.Slot.GetSkillSlot()].Skill = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.AltMainHand] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.AltMainHand].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && (int)e.Value.Slot <= 3 && e.Value.Specialization == 0))
                {
                    _terrestrialInactiveWeaponSkills[s.Value.Slot.GetSkillSlot()].Skill = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.AltOffHand] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.AltOffHand].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && (int)e.Value.Slot > 3 && e.Value.Specialization == 0))
                {
                    _terrestrialInactiveWeaponSkills[s.Value.Slot.GetSkillSlot()].Skill = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.Aquatic] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.Aquatic].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && e.Value.Specialization == 0))
                {
                    _aquaticWeaponSkills[s.Value.Slot.GetSkillSlot()].Skill = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.AltAquatic] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.AltAquatic].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && e.Value.Specialization == 0))
                {
                    _aquaticInactiveWeaponSkills[s.Value.Slot.GetSkillSlot()].Skill = s.Value;
                }
            }

            foreach (var spair in Template.BuildTemplate.AquaticSkills)
            {
                _aquaticSkills[spair.Key].Skill = spair.Value;
                _aquaticSkills[spair.Key].Slot = spair.Key;
            }

            foreach (var spair in Template.BuildTemplate.InactiveAquaticSkills)
            {
                _inactiveAquaticSkills[spair.Key].Skill = spair.Value;
                _inactiveAquaticSkills[spair.Key].Slot = spair.Key;
            }

            foreach (var spair in Template.BuildTemplate.TerrestrialSkills)
            {
                _terrestrialSkills[spair.Key].Skill = spair.Value;
                _terrestrialSkills[spair.Key].Slot = spair.Key;
            }

            foreach (var spair in Template.BuildTemplate.InactiveTerrestrialSkills)
            {
                _inactiveTerrestrialSkills[spair.Key].Skill = spair.Value;
                _inactiveTerrestrialSkills[spair.Key].Slot = spair.Key;
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

            _terrestrialTexture.Bounds = new Rectangle(4, 18, 48, 48);
            _aquaticTexture.Bounds = new Rectangle(4, 70, 48, 48);

            for (int i = 0; i < _terrestrialWeaponSkills.Count; i++)
            {
                var terrestrial = _terrestrialWeaponSkills[(WeaponSkillSlot)i];
                terrestrial.Bounds = new Rectangle(offsetX, offsetY, weponSize, weponSize);

                var terrestrialAlt = _terrestrialInactiveWeaponSkills[(WeaponSkillSlot)i];
                terrestrialAlt.Bounds = new Rectangle(offsetX, offsetY + secondRowY, weponSize, weponSize);

                var aquatic = _aquaticWeaponSkills[(WeaponSkillSlot)i];
                aquatic.Bounds = new Rectangle(offsetX, offsetY, weponSize, weponSize);

                var aquaticAlt = _aquaticInactiveWeaponSkills[(WeaponSkillSlot)i];
                aquaticAlt.Bounds = new Rectangle(offsetX, offsetY + secondRowY, weponSize, weponSize);

                offsetX += weponSize;
            }

            offsetX += 10;

            for (int i = 0; i < _terrestrialSkills.Count; i++)
            {
                var terrestrial = _terrestrialSkills[(BuildSkillSlot)i];
                terrestrial.Bounds = new Rectangle(offsetX, ((Height - _skillSize - 14) / 2) + offsetY, _skillSize, _skillSize);

                var terrestrialInactive = _inactiveTerrestrialSkills[(BuildSkillSlot)i];
                terrestrialInactive.Bounds = new Rectangle(offsetX, ((Height - _skillSize - 14) / 2) + offsetY, _skillSize, _skillSize);

                var aquatic = _aquaticSkills[(BuildSkillSlot)i];
                aquatic.Bounds = new Rectangle(offsetX, ((Height - _skillSize - 14) / 2) + offsetY, _skillSize, _skillSize);

                var aquaticInactive = _inactiveAquaticSkills[(BuildSkillSlot)i];
                aquaticInactive.Bounds = new Rectangle(offsetX, ((Height - _skillSize - 14) / 2) + offsetY, _skillSize, _skillSize);

                _selectors[i].Bounds = new Rectangle(offsetX, ((Height - _skillSize - 14) / 2) - 14 + offsetY, _skillSize, 15);

                offsetX += _skillSize;
            }

            _terrestrialTexture.Bounds = new Rectangle(4, _terrestrialWeaponSkills[WeaponSkillSlot.Weapon_1].Bounds.Bottom - 42, 42, 42);
            _aquaticTexture.Bounds = new Rectangle(4, _terrestrialInactiveWeaponSkills[WeaponSkillSlot.Weapon_1].Bounds.Top, 42, 42);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _terrestrialTexture.Draw(this, spriteBatch, RelativeMousePosition, Terrestrial ? Color.White : _terrestrialTexture.Hovered ? Color.White * 0.7F : Color.Gray * 0.5F);
            _aquaticTexture.Draw(this, spriteBatch, RelativeMousePosition, !Terrestrial ? Color.White : _aquaticTexture.Hovered ? Color.White * 0.7F : Color.Gray * 0.5F);

            foreach (var s in Terrestrial ? _terrestrialWeaponSkills : _aquaticWeaponSkills)
            {
                s.Value.Draw(this, spriteBatch, RelativeMousePosition);
                if (!SeletorOpen && s.Value.Hovered && s.Value.Skill != null)
                {
                    BasicTooltipText = s.Value.Skill.Name;
                }
            }

            var slot = Template.BuildTemplate.LegendSlot;
            var skills = _terrestrialSkills;

            switch (slot)
            {
                case LegendSlot.AquaticActive:
                    skills = _aquaticSkills;
                    break;
                case LegendSlot.TerrestrialActive:
                    skills = _terrestrialSkills;
                    break;
                case LegendSlot.TerrestrialInactive:
                    skills = _inactiveTerrestrialSkills;
                    break;
                case LegendSlot.AquaticInactive:
                    skills = _inactiveAquaticSkills;
                    break;
            };

            foreach (var s in skills)
            {
                s.Value.Draw(this, spriteBatch, Template.BuildTemplate.Terrestrial, RelativeMousePosition);
                if (!SeletorOpen && s.Value.Hovered && s.Value.Skill != null)
                {
                    BasicTooltipText = s.Value.Skill.Name;
                }
            }

            foreach (var s in _selectors)
            {
                s.Draw(this, spriteBatch, RelativeMousePosition);
            }

            foreach (var s in Terrestrial ? _terrestrialInactiveWeaponSkills : _aquaticInactiveWeaponSkills)
            {
                s.Value.Draw(this, spriteBatch, RelativeMousePosition);
                if (!SeletorOpen && s.Value.Hovered && s.Value.Skill != null)
                {
                    BasicTooltipText = s.Value.Skill.Name;
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

            for (int i = 0; i < _terrestrialSkills.Count; i++)
            {
                SkillIcon skill = null;

                switch (Template.BuildTemplate.LegendSlot)
                {
                    case LegendSlot.TerrestrialActive:
                        skill = _terrestrialSkills[(BuildSkillSlot)i];
                        break;
                    case LegendSlot.TerrestrialInactive:
                        skill = _inactiveTerrestrialSkills[(BuildSkillSlot)i];
                        break;
                    case LegendSlot.AquaticActive:
                        skill = _aquaticSkills[(BuildSkillSlot)i];
                        break;
                    case LegendSlot.AquaticInactive:
                        skill = _inactiveAquaticSkills[(BuildSkillSlot)i];
                        break;
                }

                if (skill != null && skill.Hovered)
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

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (Template != null && Template.BuildTemplate != null)
            {
                var slot = Template.BuildTemplate.LegendSlot;
                if (_terrestrialTexture.Hovered)
                {
                    Template.BuildTemplate.Terrestrial = true;
                    Template.BuildTemplate.LegendSlot = slot is LegendSlot.AquaticActive ? LegendSlot.TerrestrialActive : LegendSlot.TerrestrialInactive;
                    return;
                }

                if (_aquaticTexture.Hovered)
                {
                    Template.BuildTemplate.Terrestrial = false;
                    Template.BuildTemplate.LegendSlot = slot is LegendSlot.TerrestrialActive ? LegendSlot.AquaticActive : LegendSlot.AquaticInactive;
                    return;
                }
            }

            for (int i = 0; i < _terrestrialSkills.Count; i++)
            {
                var skill = Terrestrial ? _terrestrialSkills[(BuildSkillSlot)i] : _aquaticSkills[(BuildSkillSlot)i];
                var selector = _selectors[i];

                if (skill != null && ((skill.Hovered && GameService.Input.Mouse.State.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) || selector.Hovered))
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

                            switch (Template.BuildTemplate.LegendSlot)
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

        private void GetSelectableSkills(SkillSlot skillType)
        {
            if (Template != null)
            {
                _selectableSkills[skillType].Clear();

                if (Template.Profession != Gw2Sharp.Models.ProfessionType.Revenant)
                {
                    var skills = BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills;
                    var filteredSkills = skills.Where(e => e.Value.Slot != null && e.Value.Slot == skillType && e.Value.Categories != null && (e.Value.Specialization == 0 || Template.BuildTemplate.HasSpecialization(e.Value.Specialization))).OrderBy(e => e.Value.Categories != null ? e.Value.Categories[0] : SkillCategory.None);

                    int columns = Math.Min(filteredSkills.Count(), 4);
                    int rows = (int)Math.Ceiling(filteredSkills.Count() / (double)columns);
                    _selectorBounds = new(_selectorAnchor.Bounds.X - (((_skillSize * columns) + 8) / 2 - (_skillSize / 2)), _selectorAnchor.Bounds.Bottom, (_skillSize * columns) + 4, (_skillSize * rows) + 40);

                    int column = 0;
                    int row = 0;
                    foreach (var skill in filteredSkills)
                    {
                        _selectableSkills[skillType].Add(new() { Skill = skill.Value, Bounds = new(_selectorBounds.Left + 4 + (column * _skillSize), _selectorBounds.Top + 4 + (row * _skillSize), _skillSize - 4, _skillSize - 4) });
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
                    var skills = Template.BuildTemplate.Legends[Template.BuildTemplate.LegendSlot];
                    List<Skill> filteredSkills = new();
                    switch (skillType)
                    {
                        case SkillSlot.Heal:
                            filteredSkills.Add(Template.BuildTemplate.Legends[Template.BuildTemplate.LegendSlot].Heal);
                            break;
                        case SkillSlot.Elite:
                            filteredSkills.Add(Template.BuildTemplate.Legends[Template.BuildTemplate.LegendSlot].Elite);
                            break;
                        case SkillSlot.Utility:
                            filteredSkills.AddRange(Template.BuildTemplate.Legends[Template.BuildTemplate.LegendSlot].Utilities.Select(e => e.Value));
                            break;
                    }

                    int columns = Math.Min(filteredSkills.Count(), 4);
                    int rows = (int)Math.Ceiling(filteredSkills.Count() / (double)columns);
                    _selectorBounds = new(_selectorAnchor.Bounds.X - (((_skillSize * columns) + 8) / 2 - (_skillSize / 2)), _selectorAnchor.Bounds.Bottom, (_skillSize * columns) + 4, (_skillSize * rows) + 40);

                    int column = 0;
                    int row = 0;
                    foreach (var skill in filteredSkills)
                    {
                        _selectableSkills[skillType].Add(new() { Skill = skill, Bounds = new(_selectorBounds.Left + 4 + (column * _skillSize), _selectorBounds.Top + 4 + (row * _skillSize), _skillSize - 4, _skillSize - 4) });
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
