using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Blish_HUD.ContentService;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{

    public class ProfessionSpecifics : Control
    {
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
        }

        public Skill Skill { get => _skill; set => Common.SetProperty(ref _skill, value, ApplyTrait); }

        private void ApplyTrait()
        {
            Texture = Skill?.Icon;

        }

        public override void Draw(Control ctrl, SpriteBatch spriteBatch, Point? mousePos = null, Color? color = null, Color? bgColor = null, bool? forceHover = null, float rotation = 0, Vector2? origin = null)
        {
            base.Draw(ctrl, spriteBatch, mousePos, color, bgColor, forceHover, rotation, origin);

            Color borderColor = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Bottom - 1, Bounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Top, 1, Bounds.Height), Rectangle.Empty, borderColor * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Right - 1, Bounds.Top, 1, Bounds.Height), Rectangle.Empty, borderColor * 0.6f);
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
        private readonly AsyncTexture2D _selector = AsyncTexture2D.FromAssetId(157138);
        private readonly AsyncTexture2D _selectorHovered = AsyncTexture2D.FromAssetId(157140);

        private readonly AsyncTexture2D _weaponSwapDisabled = AsyncTexture2D.FromAssetId(156580);
        private readonly AsyncTexture2D _weaponSwap = AsyncTexture2D.FromAssetId(156583);
        private readonly AsyncTexture2D _weaponSwapHovered = AsyncTexture2D.FromAssetId(156584);
        private readonly AsyncTexture2D _pet = AsyncTexture2D.FromAssetId(156797);
        private readonly AsyncTexture2D _petClicked = AsyncTexture2D.FromAssetId(156796);

        private readonly DetailedTexture _terrestrialAutoCast = new(157150);
        private readonly DetailedTexture _terrestrialAltAutoCast = new(157150);
        private readonly DetailedTexture _aquaticAutoCast = new(157150);
        private readonly DetailedTexture _aquaticAltAutoCast = new(157150);

        private readonly AsyncTexture2D _dropBundle = AsyncTexture2D.FromAssetId(156581);

        private Template _template;
        private readonly WeaponSkillIconCollection _terrestrialWeaponSkills = new();
        private readonly WeaponSkillIconCollection _terrestrialInactiveWeaponSkills = new();
        private readonly WeaponSkillIconCollection _aquaticWeaponSkills = new();
        private readonly WeaponSkillIconCollection _aquaticInactiveWeaponSkills = new();

        private readonly SkillIconCollection _aquaticSkills = new();
        private readonly SkillIconCollection _inactiveAquaticSkills = new();
        private readonly SkillIconCollection _terrestrialSkills = new();
        private readonly SkillIconCollection _inactiveTerrestrialSkills = new();

        private bool _terrestrial = true;

        public SkillsBar()
        {
            //BackgroundColor = Color.White * 0.2F;
            Height = 150;
        }

        public Template Template { get => _template; set => Common.SetProperty(ref _template, value, ApplyTemplate, value != null); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int offsetX = 90;
            int offsetY = 26;
            int secondRowY = 64;
            int size = 64;
            int weponSize = 52;
            var skillBounds = new Rectangle(14, 14, 100, 100);

            _terrestrialTexture.Bounds = new Rectangle(4, 18, 48, 48);
            _aquaticTexture.Bounds = new Rectangle(4, 70, 48, 48);

            for (int i = 0; i < _terrestrialWeaponSkills.Count; i++)
            {
                var terrestrial = _terrestrialWeaponSkills[(WeaponSkillSlot)i];
                terrestrial.Bounds = new Rectangle(offsetX, offsetY, weponSize, weponSize);
                terrestrial.TextureRegion = skillBounds;

                if (i == 0)
                {
                    var textBounds = new Rectangle(6, 6, 52, 52);
                    _terrestrialAutoCast.Bounds = new Rectangle(offsetX, offsetY - 4, weponSize + 8, weponSize + 8);
                    _terrestrialAutoCast.TextureRegion = textBounds;

                    _terrestrialAltAutoCast.Bounds = new Rectangle(offsetX, secondRowY + offsetY - 4, weponSize + 8, weponSize + 8);
                    _terrestrialAltAutoCast.TextureRegion = textBounds;

                    _aquaticAutoCast.Bounds = new Rectangle(offsetX, offsetY - 4, weponSize + 8, weponSize + 8);
                    _aquaticAutoCast.TextureRegion = textBounds;

                    _aquaticAltAutoCast.Bounds = new Rectangle(offsetX, secondRowY + offsetY - 4, weponSize + 8, weponSize + 8);
                    _aquaticAltAutoCast.TextureRegion = textBounds;
                }

                var terrestrialAlt = _terrestrialInactiveWeaponSkills[(WeaponSkillSlot)i];
                terrestrialAlt.Bounds = new Rectangle(offsetX, offsetY + secondRowY, weponSize, weponSize);
                terrestrialAlt.TextureRegion = skillBounds;

                var aquatic = _aquaticWeaponSkills[(WeaponSkillSlot)i];
                aquatic.Bounds = new Rectangle(offsetX, offsetY, weponSize, weponSize);
                aquatic.TextureRegion = skillBounds;

                var aquaticAlt = _aquaticInactiveWeaponSkills[(WeaponSkillSlot)i];
                aquaticAlt.Bounds = new Rectangle(offsetX, offsetY + secondRowY, weponSize, weponSize);
                aquaticAlt.TextureRegion = skillBounds;

                offsetX += weponSize;
            }

            offsetX += 10;

            for (int i = 0; i < _terrestrialSkills.Count; i++)
            {
                var terrestrial = _terrestrialSkills[(BuildSkillSlot)i];
                terrestrial.Bounds = new Rectangle(offsetX, (Height - size - 14) / 2 + offsetY, size, size);
                terrestrial.TextureRegion = skillBounds;

                var aquatic = _aquaticSkills[(BuildSkillSlot)i];
                aquatic.Bounds = new Rectangle(offsetX, (Height - size - 14) / 2 + offsetY, size, size);
                aquatic.TextureRegion = skillBounds;

                _selectors[i].Bounds = new Rectangle(offsetX, (Height - size - 14) / 2 - 14 + offsetY, size, 15);

                offsetX += size;
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
                if (s.Value.Hovered && s.Value.Skill != null)
                {
                    BasicTooltipText = s.Value.Skill.Name;
                }
            }

            foreach (var s in _terrestrial ? _terrestrialSkills : _aquaticSkills)
            {
                s.Value.Draw(this, spriteBatch, RelativeMousePosition);
                if (s.Value.Hovered && s.Value.Skill != null)
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
                if (s.Value.Hovered && s.Value.Skill != null)
                {
                    BasicTooltipText = s.Value.Skill.Name;
                }
            }

            (_terrestrial ? _terrestrialAutoCast : _aquaticAutoCast).Draw(this, spriteBatch);
            (_terrestrial ? _terrestrialAltAutoCast : _aquaticAltAutoCast).Draw(this, spriteBatch);
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
            };

            _specializations.ToList().ForEach(l => l.Value.Parent = _specializationsPanel);
            _specializations.ToList().ForEach(l => l.Value.TraitsChanged += OnBuildAdjusted);
            _specializations.ToList().ForEach(l => l.Value.SpeclineSwapped += SpeclineSwapped);
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
