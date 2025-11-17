using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Gw2Mumble;
using Blish_HUD.Input;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.BuildPage
{
    public class SpecLine : Control
    {
        private readonly double _ratio = 647 / (double)135;
        private readonly DetailedTexture _baseFrame = new(993595)
        {
            TextureRegion = new(0, 0, 647, 135),
        };
        private readonly DetailedTexture _eliteFrame = new(993596)
        {
            TextureRegion = new(0, 0, 647, 135),
        };
        private readonly DetailedTexture _background = new(993593)
        {
            TextureRegion = new(0, 0, 647, 135),
        };
        private readonly DetailedTexture _specializationBackground = new(993593)
        {
            TextureRegion = new(0, 120, 647, 135),
        };
        private readonly DetailedTexture _selector = new(993583, 993584);
        private readonly DetailedTexture _hexagon = new(993598);
        private readonly DetailedTexture _noSpecHexagon = new(993597);

        private readonly TraitIcon _weaponTrait = new();

        //To Do - Implement Masks for minor traits 
        private readonly Dictionary<int, TraitIcon> _minors = new()
        {
            { 0, new()},
            { 1, new()},
            { 2, new()},
        };
        private readonly Dictionary<int, TraitIcon> _majors = new()
        {
            { 0, new()},
            { 1, new()},
            { 2, new()},
            { 3, new()},
            { 4, new()},
            { 5, new()},
            { 6, new()},
            { 7, new()},
            { 8, new()},
        };

        private double _scale = 647 / (double)135;

        private Dictionary<int, Trait> _minorsTraits = [];
        private Dictionary<int, Trait> _majorTraits = [];
        private bool _selectorOpen = false;
        private Rectangle _specSelectorBounds;
        private Template _template;
        private readonly List<(Specialization spec, Rectangle bounds)> _specBounds = [];

        public SpecLine(SpecializationSlot line)
        {
            Line = line;
            Height = 158;

            BackgroundColor = new Color(48, 48, 48);
            Input.Mouse.LeftMouseButtonPressed += MouseMouseButtonPressed;
            Input.Mouse.RightMouseButtonPressed += MouseMouseButtonPressed;

            int size = Scale(72);
            int offset = 40;

            for (int i = 0; i < (Line == SpecializationSlot.Line_3 ? 8 : 5); i++)
            {
                _specBounds.Add(new(null, new(offset, (Height - size) / 2, size, size)));
                offset += size + Scale(10);
            }
        }

        private void MouseMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (!MouseOver)
            {
                _selectorOpen = false;
            }
        }

        public Func<bool> CanInteract { get; set; } = () => true;

        private BuildTemplate Build => Template?.BuildTemplate;

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, SelectedTemplateChanged))
                {
                    if (temp is not null) temp.PropertyChanged -= OnProfessionChanged;
                    if (temp is not null) temp.PropertyChanged -= ApplyTemplate;

                    if (_template is not null) _template.PropertyChanged += OnProfessionChanged;
                    if (_template is not null) _template.PropertyChanged += ApplyTemplate;
                }
            }
        }

        private void SelectedTemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            OnProfessionChanged(sender, e);
            ApplyTemplate(sender, e);
        }

        public BuildSpecialization BuildSpecialization => Template?.BuildTemplate?.Specializations[Line];

        private void OnProfessionChanged(object sender, PropertyChangedEventArgs e)
        {
            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

            var profession = AdvancedBuildsManager.Data.Professions[Template?.Profession ?? player?.Profession ?? Gw2Sharp.Models.ProfessionType.Guardian];
            int i = 0;
            foreach (var s in profession.Specializations.Values)
            {
                if (!s.Elite || Line == SpecializationSlot.Line_3)
                {
                    _specBounds[i] = new(s, _specBounds[i].bounds);
                    i++;
                }
            }
        }

        public SpecializationSlot Line { get; private set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int ratioWidth = (int)Math.Ceiling(Height * _ratio);
            int ratioHeight = (int)Math.Ceiling(Width / _ratio);

            if (ratioWidth != Width)
            {
                Width = ratioWidth;
            }
            else if (ratioHeight != Height)
            {
                Height = ratioHeight;
            }

            _scale = Height / (double)149;

            _baseFrame.Bounds = new(0, 0, Width, Height);
            _eliteFrame.Bounds = new(0, 0, Width, Height);
            _background.Bounds = new(0, 0, Width, Height);
            _specializationBackground.Bounds = new(0, 0, Width, Height);
            _hexagon.Bounds = new(Scale(64), Scale(4), Height - Scale(8), Height - Scale(8));
            _noSpecHexagon.Bounds = new(Scale(64), Scale(4), Height - Scale(8), Height - Scale(8));
            _weaponTrait.Bounds = new(_hexagon.Bounds.Right - Scale(46) - Scale(20), _hexagon.Bounds.Bottom - Scale(46) - Scale(8), Scale(46), Scale(46));
            _selector.Bounds = new(0, 0, Scale(18), Height);

            for (int i = 0; i < _minors.Count; i++)
            {
                _minors[i].Bounds = new(Scale(225) + (i * Scale(160)), LocalBounds.Center.Y - (Scale(42) / 2), Scale(42), Scale(42));
            }
            for (int i = 0; i < _majors.Count; i++)
            {
                int row = i - ((int)Math.Floor(i / (double)3) * 3);
                _majors[i].Bounds = new(Scale(300) + ((int)Math.Floor(i / (double)3) * Scale(160)), Scale(8) + (row * Scale(42 + 4)), Scale(42), Scale(42));
            }

            _specSelectorBounds = new(_selector.Bounds.Right, 0, Width - _selector.Bounds.Right, Height);
        }

        public void ApplyTemplate(object sender = null, PropertyChangedEventArgs e = null)
        {
            _weaponTrait.Texture = BuildSpecialization?.Specialization?.WeaponTrait?.Icon;
            _specializationBackground.Texture = BuildSpecialization?.Specialization?.Background;

            if (BuildSpecialization is not null && BuildSpecialization.Specialization is not null)
            {
                _minorsTraits = BuildSpecialization.Specialization.MinorTraits.ToDictionary(e => e.Value.Index, e => e.Value);
                _majorTraits = BuildSpecialization.Specialization.MajorTraits.ToDictionary(e => e.Value.Index, e => e.Value);

                for (int i = 0; i < _minors.Count; i++)
                {
                    _minors[i].Trait = _minorsTraits.TryGetValue(i, out Trait trait) ? trait : null;
                }

                for (int i = 0; i < _majors.Count; i++)
                {
                    _majors[i].Trait = _majorTraits.TryGetValue(i, out Trait trait) ? trait : null;
                    _majors[i].Selected = trait is not null && BuildSpecialization.Traits[trait.Tier] == trait;
                }

                if (Line == SpecializationSlot.Line_3 && Template is not null && Template.BuildTemplate is not null)
                {
                    foreach (var s in Template.BuildTemplate.AquaticSkills.ToList())
                    {
                        if (s.Value is not null && s.Value.Specialization != 0 && s.Value.Specialization != BuildSpecialization.Specialization.Id)
                        {
                            Template.BuildTemplate.AquaticSkills[s.Key] = null;
                        }
                    }

                    foreach (var s in Template.BuildTemplate.InactiveAquaticSkills.ToList())
                    {
                        if (s.Value is not null && s.Value.Specialization != 0 && s.Value.Specialization != BuildSpecialization.Specialization.Id)
                        {
                            Template.BuildTemplate.InactiveAquaticSkills[s.Key] = null;
                        }
                    };

                    foreach (var s in Template.BuildTemplate.TerrestrialSkills.ToList())
                    {
                        if (s.Value is not null && s.Value.Specialization != 0 && s.Value.Specialization != BuildSpecialization.Specialization.Id)
                        {
                            Template.BuildTemplate.TerrestrialSkills[s.Key] = null;
                        }
                    }

                    foreach (var s in Template.BuildTemplate.InactiveTerrestrialSkills.ToList())
                    {
                        if (s.Value is not null && s.Value.Specialization != 0 && s.Value.Specialization != BuildSpecialization.Specialization.Id)
                        {
                            Template.BuildTemplate.InactiveTerrestrialSkills[s.Key] = null;
                        }
                    }
                }
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string txt = string.Empty;
            bool canInteract = CanInteract?.Invoke() is true or null;
            bool hasSpec = BuildSpecialization is not null && BuildSpecialization.Specialization is not null;

            Point? hoverPos = canInteract ? RelativeMousePosition : null;

            //_background.Draw(this, spriteBatch);

            if (BuildSpecialization is not null && BuildSpecialization.Specialization is not null)
            {
                _specializationBackground.Draw(this, spriteBatch);

                var minor = _minors[0].Bounds;
                spriteBatch.DrawLine(new(_hexagon.Bounds.Right - Scale(18) + AbsoluteBounds.X, _hexagon.Bounds.Center.Y + AbsoluteBounds.Y), new(minor.Left + Scale(3) + AbsoluteBounds.X, minor.Center.Y + AbsoluteBounds.Y), Colors.ColonialWhite * 0.8f, Scale(3));

                for (int i = 0; i < _majors.Count; i++)
                {
                    var major = _majors[i].Bounds;
                    minor = _minors[(int)_majors[i].Trait.Tier - 1].Bounds;
                    if (_majors[i].Selected)
                    {
                        Rectangle? minorNext = _minors.ContainsKey((int)_majors[i].Trait.Tier) ? _minors[(int)_majors[i].Trait.Tier].Bounds : null;

                        spriteBatch.DrawLine(new(minor.Right - Scale(2) + AbsoluteBounds.X, minor.Center.Y + AbsoluteBounds.Y), new(major.Left + Scale(2) + AbsoluteBounds.X, major.Center.Y + AbsoluteBounds.Y), Colors.ColonialWhite * 0.8f, Scale(2));
                        if (minorNext is not null) spriteBatch.DrawLine(new(major.Right - Scale(2) + AbsoluteBounds.X, major.Center.Y + AbsoluteBounds.Y), new(minorNext.Value.Left + Scale(2) + AbsoluteBounds.X, minorNext.Value.Center.Y + AbsoluteBounds.Y), Colors.ColonialWhite * 0.8f, Scale(2));
                    }
                }

                for (int i = 0; i < _minors.Count; i++)
                {
                    _minors[i].Draw(this, spriteBatch, hoverPos, null, null, _selectorOpen ? false : null);
                    if (_minors[i].Hovered) txt = _minors[i].Trait.Name + Environment.NewLine + _minors[i].Trait.Description;
                }

                for (int i = 0; i < _majors.Count; i++)
                {
                    _majors[i].Draw(this, spriteBatch, hoverPos, _majors[i].Selected ? Color.White : _majors[i].Hovered ? Color.DarkGray : Color.White * 0.6f, _majors[i].Selected ? null : _majors[i].Hovered ? Color.Gray * 0.1f : Color.Black * 0.5f, _selectorOpen ? false : null);
                    if (_majors[i].Hovered) txt = _majors[i].Trait.Name + Environment.NewLine + _majors[i].Trait.Description;
                }
            }

            _baseFrame.Draw(this, spriteBatch);
            _selector.Draw(this, spriteBatch, hoverPos, null, null, _selectorOpen ? true : null);
            if (_selector.Hovered) txt = "Change Specialization";
            (hasSpec ? _hexagon : _noSpecHexagon).Draw(this, spriteBatch, hoverPos);
            if (Line == SpecializationSlot.Line_3) _eliteFrame.Draw(this, spriteBatch);

            _weaponTrait.Draw(this, spriteBatch, hoverPos, null, null, _selectorOpen ? false : null);
            if (_weaponTrait.Hovered) txt = _weaponTrait.Trait?.Description;

            if (_selectorOpen)
            {
                txt = DrawSelector(spriteBatch, bounds);
            }

            BasicTooltipText = txt;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (CanInteract?.Invoke() == true)
            {
                if (!_selectorOpen)
                {
                    TraitIcon trait = _majors.FirstOrDefault(e => e.Value.Hovered).Value;

                    for (int i = 0; i < _majors.Count; i++)
                    {
                        if (trait is not null && _majors[i].Trait.Tier == trait.Trait.Tier)
                        {
                            _majors[i].Selected = trait == _majors[i] && !_majors[i].Selected;
                        }
                    }

                    if (trait is not null)
                    {
                        BuildSpecialization.Traits[trait.Trait.Tier] = trait.Selected ? trait.Trait : null;
                        return;
                    }
                }
                else
                {
                    SpecializationSlot slot = SpecializationSlot.Line_1;
                    BuildSpecialization temp = null;

                    // tied to removing skills
                    try
                    {
                        foreach (var spec in _specBounds.ToList())
                        {
                            if (spec.bounds.Contains(RelativeMousePosition))
                            {
                                bool hasSpec = Build?.HasSpecialization(spec.spec) == true;
                                slot = (SpecializationSlot)(hasSpec ? Build?.GetSpecializationSlot(spec.spec) : SpecializationSlot.Line_1);

                                if (BuildSpecialization is not null && (BuildSpecialization?.Specialization == null || BuildSpecialization.Specialization != spec.spec))
                                {
                                    if (hasSpec)
                                    {
                                        temp = new()
                                        {
                                            Specialization = BuildSpecialization.Specialization
                                        };
                                        temp.Traits[TraitTier.Adept] = BuildSpecialization.Traits[TraitTier.Adept];
                                        temp.Traits[TraitTier.Master] = BuildSpecialization.Traits[TraitTier.Master];
                                        temp.Traits[TraitTier.GrandMaster] = BuildSpecialization.Traits[TraitTier.GrandMaster];

                                        BuildSpecialization.Specialization = Build.Specializations[slot].Specialization;
                                        BuildSpecialization.Traits[TraitTier.Adept] = Build.Specializations[slot].Traits[TraitTier.Adept];
                                        BuildSpecialization.Traits[TraitTier.Master] = Build.Specializations[slot].Traits[TraitTier.Master];
                                        BuildSpecialization.Traits[TraitTier.GrandMaster] = Build.Specializations[slot].Traits[TraitTier.GrandMaster];

                                        if (temp.Specialization == null || !temp.Specialization.Elite)
                                        {
                                            Build.Specializations[slot].Specialization = temp.Specialization;
                                            Build.Specializations[slot].Traits[TraitTier.Adept] = temp.Traits[TraitTier.Adept];
                                            Build.Specializations[slot].Traits[TraitTier.Master] = temp.Traits[TraitTier.Master];
                                            Build.Specializations[slot].Traits[TraitTier.GrandMaster] = temp.Traits[TraitTier.GrandMaster];
                                        }
                                        else
                                        {
                                            Build.Specializations[slot].Specialization = null;
                                            Build.Specializations[slot].Traits[TraitTier.Adept] = null;
                                            Build.Specializations[slot].Traits[TraitTier.Master] = null;
                                            Build.Specializations[slot].Traits[TraitTier.GrandMaster] = null;
                                        }

                                        _selectorOpen = !_selectorOpen;
                                        return;
                                    }
                                    else
                                    {
                                        BuildSpecialization.Specialization = spec.spec;
                                        BuildSpecialization.Traits[TraitTier.Adept] = null;
                                        BuildSpecialization.Traits[TraitTier.Master] = null;
                                        BuildSpecialization.Traits[TraitTier.GrandMaster] = null;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AdvancedBuildsManager.Logger.Warn($"{ex}");
                    }
                }

                _selectorOpen = (_hexagon.Hovered || _noSpecHexagon.Hovered || _selector.Hovered) && !_selectorOpen;
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Input.Mouse.LeftMouseButtonPressed -= MouseMouseButtonPressed;
            Input.Mouse.RightMouseButtonPressed -= MouseMouseButtonPressed;
        }

        private int Scale(int input)
        {
            return (int)Math.Ceiling(input * _scale);
        }

        private string DrawSelector(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string txt = string.Empty;

            spriteBatch.DrawOnCtrl(
            this,
            Textures.Pixel,
            _specSelectorBounds,
            Rectangle.Empty,
            Color.Black * 0.8F,
            0F,
            Vector2.Zero);

            foreach (var spec in _specBounds)
            {
                bool hovered = spec.bounds.Contains(RelativeMousePosition);
                bool hasSpec = Build?.HasSpecialization(spec.spec) == true;

                if (spec.spec is not null)
                {
                    spriteBatch.DrawOnCtrl(
                    this,
                    spec.spec.Icon,
                    spec.bounds,
                    spec.spec.Icon.Bounds,
                    hasSpec ? Colors.Chardonnay : hovered ? Color.White : Color.White * 0.8F,
                    0F,
                    Vector2.Zero);

                    if (hovered) txt = spec.spec.Name;

                    if (hasSpec)
                    {
                        spriteBatch.DrawOnCtrl(
                        this,
                        spec.spec.Icon,
                        spec.bounds,
                        spec.spec.Icon.Bounds.Add(-4, -4, 8, 8),
                        Color.Black * 0.7F,
                        0F,
                        Vector2.Zero);
                    }
                }
            }

            return txt;
        }
    }
}
