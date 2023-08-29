using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Gw2Mumble;
using Blish_HUD.Input;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;
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
using Label = Kenedia.Modules.Core.Controls.Label;

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

        private Dictionary<int, Trait> _minorsTraits = new();
        private Dictionary<int, Trait> _majorTraits = new();

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
        private TemplatePresenter _templatePresenter;
        private readonly List<(Specialization spec, Rectangle bounds)> _specBounds = new();

        public SpecLine(SpecializationSlotType line, TemplatePresenter template)
        {
            TemplatePresenter = template;
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

            Line = line;
            Height = 158;

            BackgroundColor = new Color(48, 48, 48);
            Input.Mouse.LeftMouseButtonPressed += MouseMouseButtonPressed;
            Input.Mouse.RightMouseButtonPressed += MouseMouseButtonPressed;

            int size = Scale(72);
            int offset = 40;

            for (int i = 0; i < (Line == SpecializationSlotType.Line_3 ? 8 : 5); i++)
            {
                _specBounds.Add(new(null, new(offset, (Height - size) / 2, size, size)));
                offset += size + Scale(10);
            }
        }

        private string _basicTooltipText { get => _basicTooltipLabel.Text; set => _basicTooltipLabel.Text = value; }

        private void MouseMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (!MouseOver)
            {
                SelectorOpen = false;
            }
        }

        public Func<bool> CanInteract { get; set; } = () => true;

        public TemplatePresenter TemplatePresenter
        {
            get => _templatePresenter; set => Common.SetProperty(ref _templatePresenter, value, OnTemplatePresenterChanged);
        }

        private void OnTemplatePresenterChanged(object sender, Core.Models.ValueChangedEventArgs<TemplatePresenter> e)
        {
            if (e.OldValue is not null)
            {
                e.OldValue.ProfessionChanged -= OnProfessionChanged;
                e.OldValue.EliteSpecializationChanged -= BuildTemplate_EliteSpecializationChanged;
                e.OldValue.SpecializationChanged -= OnSpecializationChanged;
                e.OldValue.LoadedBuildFromCode -= BuildTemplate_Loaded;
                e.OldValue.TemplateChanged -= TemplatePresenter_TemplateChanged;
            }

            if (e.NewValue is not null)
            {
                e.NewValue.ProfessionChanged += OnProfessionChanged; ;
                e.NewValue.EliteSpecializationChanged += BuildTemplate_EliteSpecializationChanged;
                e.NewValue.SpecializationChanged += OnSpecializationChanged;
                e.NewValue.LoadedBuildFromCode += BuildTemplate_Loaded;
                e.NewValue.TemplateChanged += TemplatePresenter_TemplateChanged;
            }
        }

        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<Template> e)
        {
            ApplyTemplate();
        }

        private void OnSpecializationChanged(object sender, DictionaryItemChangedEventArgs<SpecializationSlotType, Specialization> e)
        {
            ApplyTemplate();
        }

        private void BuildTemplate_EliteSpecializationChanged(object sender, Core.Models.ValueChangedEventArgs<Specialization> e)
        {
            ApplyTemplate();
        }

        private void BuildTemplate_Loaded(object sender, EventArgs e)
        {
            ApplyTemplate();
        }

        public BuildSpecialization BuildSpecialization => TemplatePresenter?.Template?.Specializations[Line];

        private void OnProfessionChanged(object sender, Core.Models.ValueChangedEventArgs<ProfessionType> e)
        {
            ApplyTemplate();
        }

        public SpecializationSlotType Line { get; private set; }

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

        public void ApplyTemplate()
        {
            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            if (BuildsManager.Data?.Professions?.TryGetValue(TemplatePresenter?.Template?.Profession ?? player?.Profession ?? ProfessionType.Guardian, out Profession profession) != true)
                return;

            int j = 0;
            foreach (var s in profession.Specializations.Values)
            {
                if (!s.Elite || Line == SpecializationSlotType.Line_3)
                {
                    _specBounds[j] = new(s, _specBounds[j].bounds);
                    j++;
                }
            }

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

                if (Line == SpecializationSlotType.Line_3 && TemplatePresenter?.Template is not null)
                {
                    // Remove invalid skills
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
            _basicTooltipText = null;

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
            if (_selector.Hovered) _basicTooltipText = "Change Specialization";
            (hasSpec ? _hexagon : _noSpecHexagon).Draw(this, spriteBatch, hoverPos);
            if (Line == SpecializationSlotType.Line_3) _eliteFrame.Draw(this, spriteBatch);

            _weaponTrait.Draw(this, spriteBatch, hoverPos, null, null, SelectorOpen ? false : null);
            if (_weaponTrait.Hovered)
                _traitTooltip.Trait = _weaponTrait.Trait;

            if (SelectorOpen)
            {
                _basicTooltipText = DrawSelector(spriteBatch, bounds) ?? _basicTooltipText;
            }

            Tooltip = SelectorOpen ? _basicTooltip : _traitTooltip;
            _basicTooltip.Opacity = string.IsNullOrEmpty(_basicTooltipText) ? 0F : 1F;
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
                        BuildSpecialization.Traits[trait.Trait.Tier] = trait.Selected ? trait.Trait : null;
                        return;
                    }
                }
                else
                {
                    SpecializationSlotType slot = SpecializationSlotType.Line_1;
                    BuildSpecialization temp = null;

                    // TODO Figure out why it modifies the collection
                    // tied to removing skills
                    try
                    {
                        foreach (var spec in _specBounds.ToList())
                        {
                            if (spec.bounds.Contains(RelativeMousePosition))
                            {
                                bool hasSpec = TemplatePresenter?.Template?.HasSpecialization(spec.spec) == true;
                                slot = (SpecializationSlotType)(hasSpec ? TemplatePresenter?.Template?.GetSpecializationSlot(spec.spec) : SpecializationSlotType.Line_1);

                                if (BuildSpecialization is not null && (BuildSpecialization?.Specialization == null || BuildSpecialization.Specialization != spec.spec))
                                {
                                    if (hasSpec)
                                    {

                                        TemplatePresenter.Template.SwapSpecializations(Line, slot);
                                        SelectorOpen = !SelectorOpen;
                                        return;
                                    }
                                    else
                                    {
                                        TemplatePresenter.Template.SetSpecialization(Line, spec.spec, null, null, null);
                                    }
                                }
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

            TemplatePresenter = null;
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
                bool hasSpec = TemplatePresenter?.Template?.HasSpecialization(spec.spec) == true;

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