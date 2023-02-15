using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static Blish_HUD.ContentService;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using System.Diagnostics;
using SharpDX.MediaFoundation;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{

    public class ProfessionSpecifics : Control
    {
        private readonly AsyncTexture2D _pet = AsyncTexture2D.FromAssetId(156797);
        private readonly AsyncTexture2D _petClicked = AsyncTexture2D.FromAssetId(156796);

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }
    }

    public class SkillIcon : DetailedTexture
    {
        private Skill _skill;

        public SkillIcon()
        {
            FallBackTexture = AsyncTexture2D.FromAssetId(157154);
            HoveredFrameTexture = AsyncTexture2D.FromAssetId(157143);

            TextureRegion = new(14, 14, 100, 100);
            HoveredFrameTextureRegion = new(8, 8, 112, 112);

            AutoCastTextureRegion = new Rectangle(6, 6, 52, 52);
        }

        public Skill Skill { get => _skill; set => Common.SetProperty(ref _skill, value, ApplyTrait); }

        public AsyncTexture2D HoveredFrameTexture { get; private set; }

        public AsyncTexture2D AutoCastTexture { get; set; }

        public Rectangle HoveredFrameTextureRegion { get; }

        public Rectangle AutoCastTextureRegion { get; }

        private void ApplyTrait()
        {
            Texture = Skill?.Icon;

        }

        public override void Draw(Control ctrl, SpriteBatch spriteBatch, Point? mousePos = null, Color? color = null, Color? bgColor = null, bool? forceHover = null, float rotation = 0, Vector2? origin = null)
        {
            base.Draw(ctrl, spriteBatch, mousePos, color, bgColor, forceHover, rotation, origin);

            color ??= Color.White;
            origin ??= Vector2.Zero;
            Color borderColor = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Bottom - 1, Bounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Top, 1, Bounds.Height), Rectangle.Empty, borderColor * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Right - 1, Bounds.Top, 1, Bounds.Height), Rectangle.Empty, borderColor * 0.6f);

            if (AutoCastTexture != null)
            {
                spriteBatch.DrawOnCtrl(
                    ctrl,
                    AutoCastTexture,
                    Bounds.Add(-4, -4, 8, 8),
                    AutoCastTextureRegion,
                    (Color)color,
                    rotation,
                    (Vector2)origin);
            }

            if (Hovered)
            {
                spriteBatch.DrawOnCtrl(
                    ctrl,
                    HoveredFrameTexture,
                    Bounds,
                    HoveredFrameTextureRegion,
                    (Color)color,
                    rotation,
                    (Vector2)origin);
            }
        }
    }

    public class WeaponSkillIconCollection : Dictionary<WeaponSkillSlot, SkillIcon>
    {
        public WeaponSkillIconCollection()
        {
            foreach (WeaponSkillSlot e in Enum.GetValues(typeof(WeaponSkillSlot)))
            {
                Add(e, new());
            }
        }
    }

    public class SkillIconCollection : Dictionary<BuildSkillSlot, SkillIcon>
    {
        public SkillIconCollection()
        {
            foreach (BuildSkillSlot e in Enum.GetValues(typeof(BuildSkillSlot)))
            {
                Add(e, new());
            }
        }
    }

    public class SelectableSkillIconCollection : Dictionary<SkillSlot, List<SkillIcon>>
    {
        public SelectableSkillIconCollection()
        {
            Add(SkillSlot.Heal, new());
            Add(SkillSlot.Utility, new());
            Add(SkillSlot.Elite, new());
        }
    }

    public class SkillsBar : Control
    {
        private readonly DetailedTexture _aquaticTexture = new(1988170);
        private readonly DetailedTexture _terrestrialTexture = new(1988171);
        private readonly DetailedTexture _noAquaticFlagTexture = new(157145);

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

        private bool _terrestrial = true;
        private bool _seletorOpen = false;
        private double _lastOpen;
        private int _skillSize;

        public SkillsBar()
        {
            Height = 150;
            ClipsBounds = false;
            ZIndex = int.MaxValue / 2;

            _terrestrialWeaponSkills[WeaponSkillSlot.Weapon_1].AutoCastTexture = AsyncTexture2D.FromAssetId(157150);
            _aquaticWeaponSkills[WeaponSkillSlot.Weapon_1].AutoCastTexture = AsyncTexture2D.FromAssetId(157150);
            _aquaticInactiveWeaponSkills[WeaponSkillSlot.Weapon_1].AutoCastTexture = AsyncTexture2D.FromAssetId(157150);
            _terrestrialInactiveWeaponSkills[WeaponSkillSlot.Weapon_1].AutoCastTexture = AsyncTexture2D.FromAssetId(157150);

            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        private bool AnyHovered
        {
            get
            {
                bool hovered = _seletorOpen && _selectorBounds.Contains(RelativeMousePosition);
                hovered = hovered || AbsoluteBounds.Contains(Input.Mouse.Position);

                return hovered;
            }
        }

        public bool IsSelecting
        {
            get
            {
                double timeSince = Common.Now() - _lastOpen;
                return _seletorOpen || timeSince <= 200;
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
                        _selectorAnchor.Skill = s.Skill;
                    }
                }
            }

            SeletorOpen = false;
        }

        public Template Template { get => _template; set => Common.SetProperty(ref _template, value, ApplyTemplate, value != null); }

        private bool SeletorOpen
        {
            get => _seletorOpen;
            set
            {
                if (AnyHovered || _seletorOpen)
                {
                    _lastOpen = Common.Now();
                }

                _seletorOpen = value;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int offsetX = 90;
            int offsetY = 26;
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

                var aquatic = _aquaticSkills[(BuildSkillSlot)i];
                aquatic.Bounds = new Rectangle(offsetX, ((Height - _skillSize - 14) / 2) + offsetY, _skillSize, _skillSize);

                _selectors[i].Bounds = new Rectangle(offsetX, ((Height - _skillSize - 14) / 2) - 14 + offsetY, _skillSize, 15);

                offsetX += _skillSize;
            }

            _terrestrialTexture.Bounds = new Rectangle(4, _terrestrialWeaponSkills[WeaponSkillSlot.Weapon_1].Bounds.Bottom - 42, 42, 42);
            _aquaticTexture.Bounds = new Rectangle(4, _terrestrialInactiveWeaponSkills[WeaponSkillSlot.Weapon_1].Bounds.Top, 42, 42);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _terrestrialTexture.Draw(this, spriteBatch, RelativeMousePosition, _terrestrial ? Color.White : _terrestrialTexture.Hovered ? Color.White * 0.7F : Color.Gray * 0.5F);
            _aquaticTexture.Draw(this, spriteBatch, RelativeMousePosition, !_terrestrial ? Color.White : _aquaticTexture.Hovered ? Color.White * 0.7F : Color.Gray * 0.5F);

            foreach (var s in _terrestrial ? _terrestrialWeaponSkills : _aquaticWeaponSkills)
            {
                s.Value.Draw(this, spriteBatch, RelativeMousePosition);
                if (!SeletorOpen && s.Value.Hovered && s.Value.Skill != null)
                {
                    BasicTooltipText = s.Value.Skill.Name;
                }
            }

            foreach (var s in _terrestrial ? _terrestrialSkills : _aquaticSkills)
            {
                s.Value.Draw(this, spriteBatch, RelativeMousePosition);
                if (!SeletorOpen && s.Value.Hovered && s.Value.Skill != null)
                {
                    BasicTooltipText = s.Value.Skill.Name;
                }
            }
            foreach (var s in _selectors)
            {
                s.Draw(this, spriteBatch, RelativeMousePosition);
            }

            foreach (var s in _terrestrial ? _terrestrialInactiveWeaponSkills : _aquaticInactiveWeaponSkills)
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
                var skill = _terrestrial ? _terrestrialSkills[(BuildSkillSlot)i] : _aquaticSkills[(BuildSkillSlot)i];

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

            if (_terrestrialTexture.Hovered)
            {
                _terrestrial = true;
                return;
            }

            if (_aquaticTexture.Hovered)
            {
                _terrestrial = false;
                return;
            }

            for (int i = 0; i < _terrestrialSkills.Count; i++)
            {
                var skill = _terrestrial ? _terrestrialSkills[(BuildSkillSlot)i] : _aquaticSkills[(BuildSkillSlot)i];
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

                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && e.Value.Slot != null && (int)e.Value.Slot <= 3 && e.Value.PrevChain == null))
                {
                    _terrestrialWeaponSkills[s.Value.Slot.GetSkillSlot()].Skill = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.OffHand] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.OffHand].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && (int)e.Value.Slot > 3 && (int)e.Value.Slot <= 5 && e.Value.PrevChain == null))
                {
                    _terrestrialWeaponSkills[s.Value.Slot.GetSkillSlot()].Skill = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.AltMainHand] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.AltMainHand].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && (int)e.Value.Slot <= 3))
                {
                    _terrestrialInactiveWeaponSkills[s.Value.Slot.GetSkillSlot()].Skill = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.AltOffHand] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.AltOffHand].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && (int)e.Value.Slot > 3))
                {
                    _terrestrialInactiveWeaponSkills[s.Value.Slot.GetSkillSlot()].Skill = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.Aquatic] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.Aquatic].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon))
                {
                    _aquaticWeaponSkills[s.Value.Slot.GetSkillSlot()].Skill = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.AltAquatic] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.AltAquatic].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon))
                {
                    _aquaticInactiveWeaponSkills[s.Value.Slot.GetSkillSlot()].Skill = s.Value;
                }
            }

            foreach (var spair in Template.BuildTemplate.AquaticSkills)
            {
                _aquaticSkills[spair.Key].Skill = spair.Value;
            }

            foreach (var spair in Template.BuildTemplate.InactiveAquaticSkills)
            {
                _inactiveAquaticSkills[spair.Key].Skill = spair.Value;
            }

            foreach (var spair in Template.BuildTemplate.TerrestrialSkills)
            {
                _terrestrialSkills[spair.Key].Skill = spair.Value;
            }

            foreach (var spair in Template.BuildTemplate.InactiveTerrestrialSkills)
            {
                _inactiveTerrestrialSkills[spair.Key].Skill = spair.Value;
            }
        }

        private void GetSelectableSkills(SkillSlot skillType)
        {
            if (Template != null)
            {
                _selectableSkills[skillType].Clear();

                var skills = BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills;
                var filteredSkills = skills.Where(e => e.Value.Slot != null && e.Value.Slot == skillType && e.Value.Categories != null && (e.Value.Specialization == 0 || Template.BuildTemplate.HasSpecialization(e.Value.Specialization))).OrderBy(e => e.Value.Categories != null ? e.Value.Categories[0] : SkillCategory.None);

                int columns = Math.Min(filteredSkills.Count(), 4);
                int rows = (int)Math.Ceiling(filteredSkills.Count() / (double)4);
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
                s.Draw(this, spriteBatch, RelativeMousePosition);
            }

            spriteBatch.DrawStringOnCtrl(this, string.Format("{0} Skills", _selectedSkillSlot), Content.DefaultFont18, new Rectangle(_selectorBounds.Left, _selectorBounds.Bottom - 12 - Content.DefaultFont18.LineHeight, _selectorBounds.Width, Content.DefaultFont18.LineHeight), Color.White, false, HorizontalAlignment.Center);
        }
    }

    public class BuildPage : Container
    {
        private readonly DetailedTexture _specsBackground = new(993592);
        private readonly DetailedTexture _skillsBackground = new(155960);
        private readonly DetailedTexture _skillsBackgroundBottomBorder = new(155987);
        private readonly DetailedTexture _skillsBackgroundTopBorder = new(155989);
        private readonly AsyncTexture2D _pve_Toggle = AsyncTexture2D.FromAssetId(2229699);
        private readonly AsyncTexture2D _pve_Toggle_Hovered = AsyncTexture2D.FromAssetId(2229700);
        private readonly AsyncTexture2D _pvp_Toggle = AsyncTexture2D.FromAssetId(2229701);
        private readonly AsyncTexture2D _pvp_Toggle_Hovered = AsyncTexture2D.FromAssetId(2229702);
        private readonly AsyncTexture2D _editFeather = AsyncTexture2D.FromAssetId(2175780);
        private readonly AsyncTexture2D _editFeatherHovered = AsyncTexture2D.FromAssetId(2175779);

        private Template _template;
        private readonly FlowPanel _specializationsPanel;
        private readonly ProfessionSpecifics _professionSpecifics;
        private readonly SkillsBar _skillbar;
        private readonly Dummy _dummy;
        private readonly Dictionary<SpecializationSlot, SpecLine> _specializations = new()
        {
            {SpecializationSlot.Line_1,  new SpecLine(){ Line = SpecializationSlot.Line_1, } },
            {SpecializationSlot.Line_2,  new SpecLine() {Line = SpecializationSlot.Line_2, } },
            {SpecializationSlot.Line_3,  new SpecLine() {Line = SpecializationSlot.Line_3, } },
        };

        public BuildPage()
        {
            WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.AutoSize;

            _professionSpecifics = new()
            {
                Parent = this,
                Location = new(5, 5),
                Width = Width,
                Height = 80,
            };

            _skillbar = new SkillsBar()
            {
                Parent = this,
                Location = new(5, _professionSpecifics.Bottom),
                Width = Width,
            };

            _dummy = new Dummy()
            {
                Parent = this,
                Location = new(0, _skillbar.Bottom),
                Width = Width,
                Height = 20,
                //Height = 45,
            };

            _specializationsPanel = new()
            {
                Parent = this,
                Location = new(0, _dummy.Bottom),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new(1),
                BackgroundColor= Color.Black * 0.8F,
                AutoSizePadding = new(1),
            };

            _specializations.ToList().ForEach(l =>
            {
                l.Value.Parent = _specializationsPanel;
                l.Value.TraitsChanged += OnBuildAdjusted;
                l.Value.SpeclineSwapped += SpeclineSwapped;
                l.Value.CanInteract = () => !_skillbar.IsSelecting;
            });
        }

        private void SpeclineSwapped(object sender, EventArgs e)
        {
            ApplyTemplate();
        }

        public event EventHandler BuildAdjusted;

        private void OnBuildAdjusted(object sender = null, EventArgs e = null)
        {
            BuildAdjusted?.Invoke(sender ?? this, e);
        }

        public Template Template { get => _template; set => Common.SetProperty(ref _template, value, ApplyTemplate, value != null); }

        public void ApplyTemplate()
        {
            _specializations[SpecializationSlot.Line_1].BuildSpecialization = Template.BuildTemplate.Specializations[SpecializationSlot.Line_1];
            _specializations[SpecializationSlot.Line_1].Profession = BuildsManager.Data.Professions[Template.BuildTemplate.Profession];
            _specializations[SpecializationSlot.Line_1].Template = Template.BuildTemplate;
            _specializations[SpecializationSlot.Line_1].ApplyTemplate();

            _specializations[SpecializationSlot.Line_2].BuildSpecialization = Template.BuildTemplate.Specializations[SpecializationSlot.Line_2];
            _specializations[SpecializationSlot.Line_2].Profession = BuildsManager.Data.Professions[Template.BuildTemplate.Profession];
            _specializations[SpecializationSlot.Line_2].Template = Template.BuildTemplate;
            _specializations[SpecializationSlot.Line_2].ApplyTemplate();

            _specializations[SpecializationSlot.Line_3].BuildSpecialization = Template.BuildTemplate.Specializations[SpecializationSlot.Line_3];
            _specializations[SpecializationSlot.Line_3].Profession = BuildsManager.Data.Professions[Template.BuildTemplate.Profession];
            _specializations[SpecializationSlot.Line_3].Template = Template.BuildTemplate;
            _specializations[SpecializationSlot.Line_3].ApplyTemplate();

            _skillbar.Template = Template;
            _skillbar.ApplyTemplate();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_specializationsPanel != null)
            {
                _skillbar.Width = _specializationsPanel.Width - 10;
                _professionSpecifics.Width = _specializationsPanel.Width - 10;
                _dummy.Width = _specializationsPanel.Width;
                _specsBackground.Bounds = new(0, _dummy.Bottom - 55, Width + 15, _dummy.Height + _specializationsPanel.Height + 34);
                _specsBackground.TextureRegion = new(0, 0, 650, 450);

                _skillsBackground.Bounds = new(_professionSpecifics.Left, _professionSpecifics.Top, _professionSpecifics.Width, _professionSpecifics.Height + _skillbar.Height);
                _skillsBackground.TextureRegion = new(20, 20, _professionSpecifics.Width, _professionSpecifics.Height + _skillbar.Height);

                _skillsBackgroundTopBorder.Bounds = new(_professionSpecifics.Left - 5, _professionSpecifics.Top - 8, _professionSpecifics.Width / 2, _professionSpecifics.Height + _skillbar.Height + 8);
                _skillsBackgroundTopBorder.TextureRegion = new(35, 15, _professionSpecifics.Width / 2, _professionSpecifics.Height + _skillbar.Height);

                _skillsBackgroundBottomBorder.Bounds = new(_professionSpecifics.Right - (_professionSpecifics.Width / 2), _professionSpecifics.Top, (_professionSpecifics.Width / 2) + 16, _professionSpecifics.Height + _skillbar.Height + 12);
                _skillsBackgroundBottomBorder.TextureRegion = new(108, 275, _professionSpecifics.Width / 2, _professionSpecifics.Height + _skillbar.Height);
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
            _skillsBackground?.Draw(this, spriteBatch);
            _skillsBackgroundBottomBorder?.Draw(this, spriteBatch);
            _skillsBackgroundTopBorder?.Draw(this, spriteBatch);

            //_specsBackground?.Draw(this, spriteBatch);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _specializations.ToList().ForEach(l =>
            {
                l.Value.TraitsChanged -= OnBuildAdjusted;
                l.Value.SpeclineSwapped -= SpeclineSwapped;
                l.Value.Dispose();
            });
        }
    }
}
