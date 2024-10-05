using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Gw2Mumble;
using Blish_HUD.Input;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Blish_HUD.ContentService;
using Label = Kenedia.Modules.Core.Controls.Label;

namespace Kenedia.Modules.BuildsManager.Controls
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
        private readonly TraitTooltip _traitTooltip;
        private readonly Tooltip _basicTooltip;
        private readonly Label _basicTooltipLabel;

        public bool SelectorOpen { get => _selectorOpen; set => Common.SetProperty(ref _selectorOpen, value, OnSelectorToggled); }

        private void OnSelectorToggled(object sender, Core.Models.ValueChangedEventArgs<bool> e)
        {
            if (Tooltip is TraitTooltip tooltip)
            {
                tooltip.Trait = null;
            }
        }

        private Rectangle _specSelectorBounds;

        private readonly List<(Specialization spec, Rectangle bounds)> _specBounds = [];

        public SpecLine(SpecializationSlotType line, TemplatePresenter templatePresenter, Data data)
        {
            TemplatePresenter = templatePresenter;
            Data = data;

            Tooltip = _traitTooltip = new TraitTooltip();
            _basicTooltip = new()
            {

            };
            _basicTooltipLabel = new Label()
            {
                Parent = _basicTooltip,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
            };

            SpecializationSlot = line;
            Height = 165;

            BackgroundColor = new Color(48, 48, 48);
            Input.Mouse.LeftMouseButtonPressed += MouseMouseButtonPressed;
            Input.Mouse.RightMouseButtonPressed += MouseMouseButtonPressed;

            int size = Scale(72);
            int offset = 40;

            for (int i = 0; i < (SpecializationSlot == SpecializationSlotType.Line_3 ? 8 : 5); i++)
            {
                _specBounds.Add(new(null, new(offset, (Height - size) / 2, size, size)));
                offset += size + Scale(10);
            }

            TemplatePresenter.SpecializationChanged += OnSpecializationChanged;
            TemplatePresenter.TemplateChanged += TemplatePresenter_TemplateChanged;
            TemplatePresenter.TraitChanged += TemplatePresenter_TraitChanged;
        }

        private void TemplatePresenter_TraitChanged(object sender, TraitChangedEventArgs e)
        {
            if (e.SpecSlot == SpecializationSlot)
            {
                UpdateTraitsForSpecialization();
            }
        }

        private void UpdateTraitsForSpecialization()
        {
            _minorsTraits = BuildSpecialization.Specialization?.MinorTraits.ToDictionary(e => e.Value.Index, e => e.Value);
            _majorTraits = BuildSpecialization.Specialization?.MajorTraits.ToDictionary(e => e.Value.Index, e => e.Value);

            for (int i = 0; i < _minors.Count; i++)
            {
                _minors[i].Trait = _minorsTraits?.TryGetValue(i, out Trait trait) is true ? trait : null;
            }

            for (int i = 0; i < _majors.Count; i++)
            {
                _majors[i].Trait = _majorTraits?.TryGetValue(i, out Trait trait) is true ? trait : null;
                _majors[i].Selected = _majors[i].Trait is not null && BuildSpecialization.Traits[_majors[i].Trait.Tier] == _majors[i].Trait;
            }
        }

        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<Template> e)
        {
            SetSpecialization();
        }

        private void OnSpecializationChanged(object sender, SpecializationChangedEventArgs e)
        {
            if (e.Slot == SpecializationSlot)
            {
                SetSpecialization();
            }
        }

        private new string BasicTooltipText { get => _basicTooltipLabel.Text; set => _basicTooltipLabel.Text = value; }

        private void MouseMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (!MouseOver)
            {
                SelectorOpen = false;
            }
        }

        public Func<bool> CanInteract { get; set; } = () => true;

        public TemplatePresenter TemplatePresenter { get; }

        public Data Data { get; }

        public BuildSpecialization? BuildSpecialization => TemplatePresenter?.Template?[SpecializationSlot];

        public SpecializationSlotType SpecializationSlot { get; private set; }

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

        private void SetSpecialization()
        {
            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            if (TemplatePresenter?.Template?.Profession is not ProfessionType templateProfession || Data?.Professions?.TryGetValue(templateProfession, out Profession profession) is not true)
                return;

            int j = 0;
            bool isEliteSpecLine = SpecializationSlot == SpecializationSlotType.Line_3;
            var specializations = profession.Specializations.Values.Where(x => !x.Elite || isEliteSpecLine).OrderBy(x => x.Elite).ThenBy(x => x.Id);

            foreach (var s in specializations)
            {
                if (!s.Elite || SpecializationSlot == SpecializationSlotType.Line_3)
                {
                    _specBounds[j] = new(s, _specBounds[j].bounds);
                    j++;
                }
            }

            _weaponTrait.Texture = BuildSpecialization?.Specialization?.WeaponTrait?.Icon;
            _specializationBackground.Texture = BuildSpecialization?.Specialization?.Background;

            UpdateTraitsForSpecialization();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string txt = string.Empty;
            bool canInteract = CanInteract?.Invoke() is true or null;
            bool hasSpec = BuildSpecialization is not null && BuildSpecialization.Specialization is not null;

            Point? hoverPos = canInteract ? RelativeMousePosition : null;

            //_background.Draw(this, spriteBatch);
            BasicTooltipText = null;

            if (BuildSpecialization is not null && BuildSpecialization.Specialization is not null && !SelectorOpen)
            {
                _traitTooltip.Trait = null;

                _specializationBackground.Draw(this, spriteBatch);

                var minor = _minors[0].Bounds;
                spriteBatch.DrawLine(new(_hexagon.Bounds.Right - Scale(18) + AbsoluteBounds.X, _hexagon.Bounds.Center.Y + AbsoluteBounds.Y), new(minor.Left + Scale(3) + AbsoluteBounds.X, minor.Center.Y + AbsoluteBounds.Y), Colors.ColonialWhite * 0.8f, Scale(3));

                for (int i = 0; i < _majors.Count; i++)
                {
                    var major = _majors[i].Bounds;

                    if (_majors[i].Trait is not null)
                    {
                        minor = _minors[(int)_majors[i].Trait.Tier - 1].Bounds;
                        if (_majors[i].Selected)
                        {
                            Rectangle? minorNext = _minors.ContainsKey((int)_majors[i].Trait.Tier) ? _minors[(int)_majors[i].Trait.Tier].Bounds : null;

                            spriteBatch.DrawLine(new(minor.Right - Scale(2) + AbsoluteBounds.X, minor.Center.Y + AbsoluteBounds.Y), new(major.Left + Scale(2) + AbsoluteBounds.X, major.Center.Y + AbsoluteBounds.Y), Colors.ColonialWhite * 0.8f, Scale(2));
                            if (minorNext is not null) spriteBatch.DrawLine(new(major.Right - Scale(2) + AbsoluteBounds.X, major.Center.Y + AbsoluteBounds.Y), new(minorNext.Value.Left + Scale(2) + AbsoluteBounds.X, minorNext.Value.Center.Y + AbsoluteBounds.Y), Colors.ColonialWhite * 0.8f, Scale(2));
                        }
                    }
                }

                for (int i = 0; i < _minors.Count; i++)
                {
                    _minors[i].Draw(this, spriteBatch, hoverPos, null, null, SelectorOpen ? false : null);
                    if (_minors[i].Hovered)
                        _traitTooltip.Trait = _minors[i].Trait;
                }

                for (int i = 0; i < _majors.Count; i++)
                {
                    _majors[i].Draw(this, spriteBatch, hoverPos, _majors[i].Selected ? Color.White : _majors[i].Hovered ? Color.DarkGray : Color.White * 0.6f, _majors[i].Selected ? null : _majors[i].Hovered ? Color.Gray * 0.1f : Color.Black * 0.5f, SelectorOpen ? false : null);

                    if (_majors[i].Hovered)
                        _traitTooltip.Trait = _majors[i].Trait;
                }
            }

            _baseFrame.Draw(this, spriteBatch);
            _selector.Draw(this, spriteBatch, hoverPos, null, null, SelectorOpen ? true : null);
            if (_selector.Hovered) BasicTooltipText = "Change Specialization";
            (hasSpec ? _hexagon : _noSpecHexagon).Draw(this, spriteBatch, hoverPos);
            if (SpecializationSlot == SpecializationSlotType.Line_3) _eliteFrame.Draw(this, spriteBatch);

            _weaponTrait.Draw(this, spriteBatch, hoverPos, null, null, SelectorOpen ? false : null);
            if (_weaponTrait.Hovered)
                _traitTooltip.Trait = _weaponTrait.Trait;

            if (SelectorOpen)
            {
                BasicTooltipText = DrawSelector(spriteBatch, bounds) ?? BasicTooltipText;
            }

            Tooltip = SelectorOpen ? _basicTooltip : _traitTooltip;
            _basicTooltip.Opacity = string.IsNullOrEmpty(BasicTooltipText) ? 0F : 1F;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (CanInteract?.Invoke() == true)
            {
                if (!SelectorOpen)
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
                        TemplatePresenter.Template?.SetTrait(SpecializationSlot, trait.Trait, trait.Trait.Tier);
                        return;
                    }
                }
                else
                {
                    SpecializationSlotType slot = SpecializationSlotType.Line_1;
                    BuildSpecialization temp = null;

                    try
                    {
                        foreach (var spec in _specBounds.ToList())
                        {
                            if (spec.bounds.Contains(RelativeMousePosition) && spec.spec is not null)
                            {
                                TemplatePresenter.Template.SetSpecialization(SpecializationSlot, spec.spec);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        BuildsManager.Logger.Warn($"{ex}");
                    }
                }

                SelectorOpen = (_hexagon.Hovered || _noSpecHexagon.Hovered || _selector.Hovered) && !SelectorOpen;
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Input.Mouse.LeftMouseButtonPressed -= MouseMouseButtonPressed;
            Input.Mouse.RightMouseButtonPressed -= MouseMouseButtonPressed;

            TemplatePresenter.SpecializationChanged -= OnSpecializationChanged;
            TemplatePresenter.TemplateChanged -= TemplatePresenter_TemplateChanged;
            TemplatePresenter.TraitChanged -= TemplatePresenter_TraitChanged;

            _baseFrame?.Dispose();
            _eliteFrame?.Dispose();
            _background?.Dispose();
            _specializationBackground?.Dispose();
            _selector?.Dispose();
            _hexagon?.Dispose();
            _noSpecHexagon?.Dispose();
            _weaponTrait?.Dispose();

            _minors?.Values?.DisposeAll();
            _majors?.Values?.DisposeAll();
        }

        private int Scale(int input)
        {
            return (int)Math.Ceiling(input * _scale);
        }

        private string? DrawSelector(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string txt = null;

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
                bool hasSpec = TemplatePresenter?.Template?.HasSpecialization(spec.spec, out _) == true;

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