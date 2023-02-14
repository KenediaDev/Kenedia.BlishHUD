﻿using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
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
        private readonly DetailedTexture _hexagon = new(993598)
        {
            //TextureRegion = new(12, 5, 103, 116),
        };
        private readonly DetailedTexture _weaponTrait = new()
        {
            //TextureRegion = new(12, 5, 103, 116),
        };
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
        private readonly Dictionary<Specialization, Rectangle> _specBounds = new();

        private double _scale = 647 / (double)135;
        private BuildSpecialization _buildSpecialization;

        private Dictionary<int, Trait> _minorsTraits = new();
        private Dictionary<int, Trait> _majorTraits = new();
        private bool _selectorOpen = false;
        private Rectangle _specSelectorBounds;
        private Profession _profession;
        private BuildTemplate _template;

        public SpecLine()
        {
            Height = 158;

            BackgroundColor = Color.Black * 0.4f;
        }

        public event EventHandler TraitsChanged;
        public event EventHandler SpeclineSwapped;

        public BuildTemplate Template { get => _template; set => Common.SetProperty(ref _template, value, ApplyTemplate, value != null); }

        public BuildSpecialization BuildSpecialization { get => _buildSpecialization; set => Common.SetProperty(ref _buildSpecialization, value, ApplySpecialization); }

        public Profession Profession { get => _profession; set => Common.SetProperty(ref _profession, value, ApplyProfession); }

        public SpecializationSlot Line { get; internal set; }

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

            ApplyProfession();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string txt = string.Empty;
            if (_buildSpecialization != null && _buildSpecialization.Specialization != null)
            {
                _background.Draw(this, spriteBatch);
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
                        if (minorNext != null) spriteBatch.DrawLine(new(major.Right - Scale(2) + AbsoluteBounds.X, major.Center.Y + AbsoluteBounds.Y), new(minorNext.Value.Left + Scale(2) + AbsoluteBounds.X, minorNext.Value.Center.Y + AbsoluteBounds.Y), Colors.ColonialWhite * 0.8f, Scale(2));
                    }
                }

                for (int i = 0; i < _minors.Count; i++)
                {
                    _minors[i].Draw(this, spriteBatch, RelativeMousePosition, null, null, _selectorOpen ? false : null);
                    if (_minors[i].Hovered) txt = _minors[i].Trait.Description;
                }

                for (int i = 0; i < _majors.Count; i++)
                {
                    _majors[i].Draw(this, spriteBatch, RelativeMousePosition, _majors[i].Selected ? Color.White : _majors[i].Hovered ? Color.DarkGray : Color.White * 0.6f, _majors[i].Selected ? null : _majors[i].Hovered ? Color.Gray * 0.1f : Color.Black * 0.5f, _selectorOpen ? false : null);
                    if (_majors[i].Hovered) txt = _majors[i].Trait.Description;
                }

                _baseFrame.Draw(this, spriteBatch);
                _hexagon.Draw(this, spriteBatch);
                _weaponTrait.Draw(this, spriteBatch, RelativeMousePosition, null, null, _selectorOpen ? false : null);
                if (_weaponTrait.Hovered) txt = BuildSpecialization.Specialization.WeaponTrait?.Description;
                _selector.Draw(this, spriteBatch, RelativeMousePosition, null, null, _selectorOpen ? true : null);
                if (_selector.Hovered) txt = "Change Specialization";
                if (_buildSpecialization.Specialization.Elite) _eliteFrame.Draw(this, spriteBatch);

                if (_selectorOpen && Profession != null)
                {
                    txt = DrawSelector(spriteBatch, bounds);
                }
            }

            BasicTooltipText = txt;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            if (!_selectorOpen)
            {
                TraitIcon trait = _majors.FirstOrDefault(e => e.Value.Hovered).Value;

                for (int i = 0; i < _majors.Count; i++)
                {
                    if (trait != null && _majors[i].Trait.Tier == trait.Trait.Tier)
                    {
                        _majors[i].Selected = trait == _majors[i] && !_majors[i].Selected;
                    }
                }

                if (trait != null)
                {
                    BuildSpecialization.Traits[trait.Trait.Tier] = trait.Selected ? trait.Trait : null;
                    TraitsChanged?.Invoke(this, EventArgs.Empty);

                    return;
                }
            }
            else
            {

                SpecializationSlot slot = SpecializationSlot.Line_1;
                BuildSpecialization temp = null;

                foreach (var spec in _specBounds)
                {
                    if (spec.Value.Contains(RelativeMousePosition))
                    {
                        if (BuildSpecialization.Specialization != spec.Key)
                        {
                            bool hasSpec = Template?.HasSpecialization(spec.Key) == true;
                            slot = (SpecializationSlot)(hasSpec ? Template?.GetSpecializationSlot(spec.Key) : SpecializationSlot.Line_1);

                            if(hasSpec)
                            {
                                temp = new()
                                {
                                    Specialization = BuildSpecialization.Specialization
                                };
                                temp.Traits[TraitTier.Adept] = BuildSpecialization.Traits[TraitTier.Adept];
                                temp.Traits[TraitTier.Master] = BuildSpecialization.Traits[TraitTier.Master];
                                temp.Traits[TraitTier.GrandMaster] = BuildSpecialization.Traits[TraitTier.GrandMaster];

                                BuildSpecialization.Specialization = Template.Specializations[slot].Specialization;
                                BuildSpecialization.Traits[TraitTier.Adept] = Template.Specializations[slot].Traits[TraitTier.Adept];
                                BuildSpecialization.Traits[TraitTier.Master] = Template.Specializations[slot].Traits[TraitTier.Master];
                                BuildSpecialization.Traits[TraitTier.GrandMaster] = Template.Specializations[slot].Traits[TraitTier.GrandMaster];

                                Template.Specializations[slot].Specialization = temp.Specialization;
                                Template.Specializations[slot].Traits[TraitTier.Adept] = temp.Traits[TraitTier.Adept];
                                Template.Specializations[slot].Traits[TraitTier.Master] = temp.Traits[TraitTier.Master];
                                Template.Specializations[slot].Traits[TraitTier.GrandMaster] = temp.Traits[TraitTier.GrandMaster];

                                ApplySpecialization();
                                _selectorOpen = !_selectorOpen;
                                OnSpecLineSwapped();
                                return;
                            }
                            else
                            {
                                BuildSpecialization.Specialization = spec.Key;
                                BuildSpecialization.Traits[TraitTier.Adept] = null;
                                BuildSpecialization.Traits[TraitTier.Master] = null;
                                BuildSpecialization.Traits[TraitTier.GrandMaster] = null;
                            }

                            ApplySpecialization();
                        }
                    }
                }
            }

            _selectorOpen = !_selectorOpen;
        }

        private void OnSpecLineSwapped()
        {
            SpeclineSwapped?.Invoke(this, null);
            TraitsChanged?.Invoke(this, null);
        }

        public void ApplyProfession()
        {
            if (Profession != null)
            {
                _specBounds.Clear();

                int size = Scale(72);
                int offset = 40;

                foreach (var spec in Profession.Specializations)
                {
                    if (Line == SpecializationSlot.Line_3 || !spec.Value.Elite)
                    {
                        _specBounds[spec.Value] = new(offset, (Height - size) / 2, size, size);
                        offset += size + Scale(10);
                    }
                }
            }
        }

        public void ApplySpecialization()
        {
            if (_buildSpecialization != null)
            {
                _specializationBackground.Texture = _buildSpecialization.Specialization.Background;
                _weaponTrait.Texture = _buildSpecialization.Specialization.WeaponTrait?.Icon;

                _minorsTraits = _buildSpecialization.Specialization.MinorTraits.ToDictionary(e => e.Value.Index, e => e.Value);
                _majorTraits = _buildSpecialization.Specialization.MajorTraits.ToDictionary(e => e.Value.Index, e => e.Value);

                for (int i = 0; i < _minors.Count; i++)
                {
                    _minors[i].Trait = _minorsTraits.TryGetValue(i, out Trait trait) ? trait : null;
                    _minors[i].TextureRegion = new(4, 4, 58, 58);
                }

                for (int i = 0; i < _majors.Count; i++)
                {
                    _majors[i].Trait = _majorTraits.TryGetValue(i, out Trait trait) ? trait : null;
                    _majors[i].TextureRegion = new(4, 4, 58, 58);
                    _majors[i].Selected = trait != null && BuildSpecialization.Traits[trait.Tier] == trait;
                }
            }
        }

        public void ApplyTemplate()
        {
            ApplyProfession();
            ApplySpecialization();
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
                bool hovered = spec.Value.Contains(RelativeMousePosition);
                bool hasSpec = Template?.HasSpecialization(spec.Key) == true;

                spriteBatch.DrawOnCtrl(
                this,
                spec.Key.Icon,
                spec.Value,
                spec.Key.Icon.Bounds,
                hasSpec ? Colors.Chardonnay : hovered ? Color.White : Color.White * 0.8F,
                0F,
                Vector2.Zero);

                if (hovered) txt = spec.Key.Name;

                if (hasSpec)
                {
                    spriteBatch.DrawOnCtrl(
                    this,
                    spec.Key.Icon,
                    spec.Value,
                    spec.Key.Icon.Bounds.Add(-4, -4, 8, 8),
                    Color.Black * 0.7F,
                    0F,
                    Vector2.Zero);
                }
            }

            return txt;
        }
    }
}
