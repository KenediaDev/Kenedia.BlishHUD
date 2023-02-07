using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Controls;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class RadialMenu : Control
    {
        private readonly Data _data;
        private readonly AsyncTexture2D _dummy = AsyncTexture2D.FromAssetId(1128572);
        private readonly CharacterTooltip _tooltip;

        private readonly SettingsModel _settings;
        private readonly ObservableCollection<Character_Model> _characters;
        private readonly Func<Character_Model> _currentCharacter;
        private List<Character_Model> _displayedCharacters;
        private int _iconSize;

        private readonly List<RadialMenuSection> _sections = new();
        private RadialMenuSection? _selected;

        private Point _center;

        public RadialMenu(SettingsModel settings, ObservableCollection<Character_Model> characters, Container parent, Func<Character_Model> currentCharacter, Data data, TextureManager textureManager)
        {
            _settings = settings;
            _characters = characters;
            _currentCharacter = currentCharacter;
            _data = data;
            Parent = parent;

            _tooltip = new(currentCharacter, textureManager, data, settings)
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = int.MaxValue / 2 + 1,
                Size = new Point(300, 50),
                Visible = false,
            };

            foreach (Character_Model c in _characters)
            {
                c.Updated += Character_Updated;
            }

            Parent.Resized += Parent_Resized;
            Input.Keyboard.KeyPressed += Keyboard_KeyPressed;
        }

        private Character_Model CurrentCharacter => _currentCharacter?.Invoke();

        public bool HasDisplayedCharacters()
        {
            if (_characters != null) _displayedCharacters = _characters.Count > 0 ? _characters.Where(e => e.ShowOnRadial).ToList() : new();
            return _displayedCharacters.Count() > 0;
        }

        private void Keyboard_KeyPressed(object sender, KeyboardEventArgs e)
        {
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.Escape)
            {
                Hide();
            }
        }

        private void Parent_Resized(object sender, ResizedEventArgs e)
        {
            RecalculateLayout();
        }

        private void Character_Updated(object sender, EventArgs e)
        {
            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (Parent != null)
            {
                _displayedCharacters = _characters.Count > 0 ? _characters.Where(e => e.ShowOnRadial).ToList() : new();
                _displayedCharacters.Sort((a, b) => a.Name.CompareTo(b.Name));

                int size = (int)(Math.Min(Parent.Width, Parent.Height) * _settings.Radial_Scale.Value);
                Size = new(Parent.Width, Parent.Height);

                _sections.Clear();

                _center = RelativeMousePosition;
                int amount = _displayedCharacters.Count;
                double adiv = Math.PI * 2 / amount;

                int radius = size / 2;

                _iconSize = Math.Min(105, ((int)(radius * (Math.Sqrt(2) / amount))) * 3);

                if (amount <= 2)
                {
                    //var points = new List<Vector2>();
                    //var c = _center.ToVector2();

                    //for (int i = 0; i < amount; i++)
                    //{
                    //    var p = new Vector2(
                    //        x: _center.X - (int)(radius * Math.Sin(2 * Math.PI * i / amount)),
                    //        y: _center.Y - (int)(radius * Math.Cos(2 * Math.PI * i / amount))
                    //        );

                    //    points.Add(p);
                    //}

                    //for (int i = 0; i < points.Count; i++)
                    //{
                    //    Vector2 a = points[i];
                    //    var p = a.ToPoint();
                    //    var s = _center.Distance2D(p);

                    //    _sections.Add(new()
                    //    {
                    //        Character = _displayedCharacters[i],
                    //        IconRectangle = new(p.X - (_iconSize / 2), p.Y - (_iconSize / 2), _iconSize, _iconSize)
                    //    });
                    //}
                }
                else
                {
                    var points = new List<Vector2>();
                    var c = _center.ToVector2();

                    for (int i = 0; i < amount; i++)
                    {
                        var p = new Vector2(
                            x: _center.X - (int)(radius * Math.Sin(2 * Math.PI * i / amount)),
                            y: _center.Y - (int)(radius * Math.Cos(2 * Math.PI * i / amount))
                            );

                        points.Add(p);
                    }

                    for (int i = 0; i < points.Count; i++)
                    {
                        Vector2 a = points[i];
                        Vector2 b = i == points.Count - 1 ? points[0] : points[i + 1];
                        var t = new Triangle(a, b, c);
                        var v = new Vector2((a.X + b.X + c.X) / 3, (a.Y + b.Y + c.Y) / 3);
                        var p = v.ToPoint();

                        var dis = Math.Min(a.ToPoint().Distance2D(v.ToPoint()),
                            Math.Min(b.ToPoint().Distance2D(v.ToPoint()),
                            c.ToPoint().Distance2D(v.ToPoint())));

                        _sections.Add(new()
                        {
                            Character = _displayedCharacters[i],
                            Triangle = t,
                            Lines = t.DrawingPoints(),
                            IconPos = v,
                            IconRectangle = new(p.X - (_iconSize / 2), p.Y - (_iconSize / 2), _iconSize, _iconSize)
                        });
                    }
                }
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            _selected = null;
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            if (!Input.Keyboard.KeysDown.Contains(_settings.RadialKey.Value.PrimaryKey))
            {
                Hide();
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var mouse = RelativeMousePosition.ToVector2();
            var txt = string.Empty;

            foreach (var section in _sections)
            {
                _selected ??= section.Triangle.Contains(mouse) ? section : null;

                if (RelativeMousePosition == _center || _selected != null || !section.Triangle.Contains(mouse))
                {
                    if (section.Lines != null)
                    {
                        foreach (var line in section.Lines)
                        {
                            spriteBatch.DrawLine(section.Triangle.Point3, new(line.X, line.Y), _settings.Radial_UseProfessionColor.Value ? section.Character.Profession.GetData(_data.Professions).Color * 0.7f : _settings.Radial_IdleColor.Value);
                        }

                        spriteBatch.DrawPolygon(Vector2.Zero, section.Triangle.ToVectorList(), _settings.Radial_IdleBorderColor.Value, 1);
                    }
                    else if (section.Rectangle != null)
                    {

                        spriteBatch.DrawOnCtrl(
                            this,
                            ContentService.Textures.Pixel,
                            section.Rectangle,
                            Rectangle.Empty,
                            _settings.Radial_IdleColor.Value,
                            0f,
                            default);
                    }

                    spriteBatch.DrawOnCtrl(
                        this,
                        _settings.Radial_UseProfessionIcons.Value ? section.Character.ProfessionIcon : section.Character.Icon,
                        section.IconRectangle,
                        _settings.Radial_UseProfessionIcons.Value ? section.Character.ProfessionIcon.Bounds : section.Character.Icon.Bounds,
                        _settings.Radial_UseProfessionIconsColor.Value ? section.Character.Profession.GetData(_data.Professions).Color : Color.White,
                        0f,
                        default);
                }
            }

            if (_selected != null)
            {
                if (_settings.Radial_ShowAdvancedTooltip.Value)
                {
                    _tooltip.Character = _selected.Character;
                    _tooltip.Show();
                }
                else
                {
                    txt = _selected.Character.Name;
                }

                if (_selected.Lines != null)
                {
                    foreach (var line in _selected.Lines)
                    {
                        spriteBatch.DrawLine(_selected.Triangle.Point3, new(line.X, line.Y), _settings.Radial_UseProfessionColor.Value ? _selected.Character.Profession.GetData(_data.Professions).Color : _settings.Radial_HoveredColor.Value);
                    }

                    spriteBatch.DrawPolygon(Vector2.Zero, _selected.Triangle.ToVectorList(), _settings.Radial_HoveredBorderColor.Value, 1);
                }
                else if (_selected.Rectangle != null)
                {

                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        _selected.Rectangle,
                        Rectangle.Empty,
                        _settings.Radial_HoveredColor.Value,
                        0f,
                        default);
                }

                spriteBatch.DrawOnCtrl(
                    this,
                    _settings.Radial_UseProfessionIcons.Value ? _selected.Character.ProfessionIcon : _selected.Character.Icon,
                    _selected.IconRectangle,
                    _settings.Radial_UseProfessionIcons.Value ? _selected.Character.ProfessionIcon.Bounds : _selected.Character.Icon.Bounds,
                    _settings.Radial_UseProfessionIconsColor.Value ? _selected.Character.Profession.GetData(_data.Professions).Color : Color.White,
                    0f,
                    default);
            }
            else
            {
                _tooltip.Character = null;
                _tooltip.Hide();
            }

            if (!_settings.Radial_ShowAdvancedTooltip.Value) BasicTooltipText = txt;
        }

        protected override async void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_selected != null)
            {
                Hide();

                if(await ExtendedInputService.WaitForNoKeyPressed())
                {
                    _selected?.Character.Swap();
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            RecalculateLayout();
            _selected = null;
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);

            _tooltip.Character = null;
            _tooltip.Hide();
        }

        protected override void DisposeControl()
        {
            if (Parent != null) Parent.Resized -= Parent_Resized;

            foreach (Character_Model c in _characters)
            {
                c.Updated -= Character_Updated;
            }
            _tooltip?.Dispose();
            base.DisposeControl();
        }
    }
}
