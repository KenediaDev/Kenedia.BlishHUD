using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Blish_HUD.ContentService;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillsBar : Control
    {
        private Template _template;
        private WeaponSkillCollection _terrestrialWeaponSkills = new();
        private WeaponSkillCollection _terrestrialInactiveWeaponSkills = new();
        private WeaponSkillCollection _aquaticWeaponSkills = new();
        private WeaponSkillCollection _aquaticInactiveWeaponSkills = new();

        private SkillCollection _aquaticSkills = new();
        private SkillCollection _inactiveAquaticSkills = new();
        private SkillCollection _terrestrialSkills = new();
        private SkillCollection _inactiveTerrestrialSkills = new();

        public SkillsBar()
        {
            BackgroundColor = Color.White * 0.2F;
            Height = 64;
        }

        public Template Template { get => _template; set => Common.SetProperty(ref _template, value, ApplyTemplate, value != null); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            int offsetX = 65;
            int size = 64;
            foreach (var s in _terrestrialWeaponSkills)
            {
                var b = new Rectangle(offsetX, 0, size, size);
                if (s.Value != null)
                {
                    spriteBatch.DrawOnCtrl(
                    this,
                    s.Value.Icon,
                    b,
                    new(8, 8, 112, 112),
                    Color.White,
                    0F,
                    Vector2.Zero);

                    if (b.Contains(RelativeMousePosition))
                    {
                        BasicTooltipText = s.Value.Name + " " + (int) s.Value.Slot;
                    }
                }

                offsetX += size + 4;
            }
            offsetX += 26;
            foreach (var s in _terrestrialSkills)
            {
                var b = new Rectangle(offsetX, 0, size, size);
                if (s.Value != null)
                {
                    spriteBatch.DrawOnCtrl(
                    this,
                    s.Value.Icon,
                    b,
                    new(8, 8, 112, 112),
                    Color.White,
                    0F,
                    Vector2.Zero);

                    if (b.Contains(RelativeMousePosition))
                    {
                        BasicTooltipText = s.Value.Name + " " + (int) s.Value.Slot;
                    }
                }

                offsetX += (size + 4);
            }
        }

        private void ApplyTemplate()
        {
            if (Template.GearTemplate.Gear[GearSlot.MainHand] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.MainHand].WeaponType;

                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && e.Value.Slot != null && (int)e.Value.Slot <= 3 && e.Value.PrevChain == null))
                {
                    _terrestrialWeaponSkills[s.Value.Slot.GetSkillSlot()] = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.OffHand] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.OffHand].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && (int)e.Value.Slot > 3 && (int)e.Value.Slot <= 5 && e.Value.PrevChain == null))
                {
                    _terrestrialWeaponSkills[s.Value.Slot.GetSkillSlot()] = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.AltMainHand] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.AltMainHand].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && (int)e.Value.Slot <= 3))
                {
                    _terrestrialInactiveWeaponSkills[s.Value.Slot.GetSkillSlot()] = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.AltOffHand] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.AltOffHand].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon && (int)e.Value.Slot > 3))
                {
                    _terrestrialInactiveWeaponSkills[s.Value.Slot.GetSkillSlot()] = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.Aquatic] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.Aquatic].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon))
                {
                    _aquaticWeaponSkills[s.Value.Slot.GetSkillSlot()] = s.Value;
                }
            }

            if (Template.GearTemplate.Gear[GearSlot.AltAquatic] != null)
            {
                var weapon = Template.GearTemplate.Gear[GearSlot.AltAquatic].WeaponType;
                foreach (var s in BuildsManager.Data.Professions[Template.BuildTemplate.Profession].Skills.Where(e => e.Value.WeaponType == weapon))
                {
                    _aquaticInactiveWeaponSkills[s.Value.Slot.GetSkillSlot()] = s.Value;
                }
            }

            foreach (var spair in Template.BuildTemplate.AquaticSkills)
            {
                _aquaticSkills[spair.Key] = spair.Value;
            }

            foreach (var spair in Template.BuildTemplate.InactiveAquaticSkills)
            {
                _inactiveAquaticSkills[spair.Key] = spair.Value;
            }

            foreach (var spair in Template.BuildTemplate.TerrestrialSkills)
            {
                _terrestrialSkills[spair.Key] = spair.Value;
            }

            foreach (var spair in Template.BuildTemplate.InactiveTerrestrialSkills)
            {
                _inactiveTerrestrialSkills[spair.Key] = spair.Value;
            }
        }
    }

    public class BuildPage : Container
    {
        private Template _template;
        private readonly FlowPanel _specializationsPanel;
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

            _specializationsPanel = new()
            {
                Parent = this,
                Location = new(0, 5),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            _skillbar = new SkillsBar()
            {
                Parent = _specializationsPanel,
                Width = _specializationsPanel.Width,
            };

            _dummy = new Dummy()
            {
                Parent = _specializationsPanel,
                Width = _specializationsPanel.Width,
                Height = 15,
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
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_skillbar != null) _skillbar.Width = _specializationsPanel.Width;
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
