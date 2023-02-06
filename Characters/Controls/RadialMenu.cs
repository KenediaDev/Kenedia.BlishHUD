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
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Characters.Controls
{
    public struct Triangle
    {
        public static Triangle Empty = new(new(0), new(0), new(0));

        public Triangle()
        {

        }

        public Triangle(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
        }

        public Vector2 Point1 { get; set; }

        public Vector2 Point2 { get; set; }

        public Vector2 Point3 { get; set; }

        public bool CompareTo(Triangle t)
        {
            return Point1.Equals(t.Point1) && Point2.Equals(t.Point2) && Point3.Equals(t.Point3);
        }

        public bool IsEmpty()
        {
            return Point1.Equals(Empty.Point1) && Point2.Equals(Empty.Point2) && Point3.Equals(Empty.Point3);
        }

        public List<Vector2> ToVectorList()
        {
            return new List<Vector2>()
                {
                    Point1,
                    Point2,
                    Point3,
                };
        }

        public bool Contains(Vector2 pt)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, Point1, Point2);
            d2 = sign(pt, Point2, Point3);
            d3 = sign(pt, Point3, Point1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        float sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        public Point LowestRectPoint()
        {
            var max = new Vector2(Math.Max(Point1.X, Math.Max(Point2.X, Point3.X)), Math.Max(Point1.Y, Math.Max(Point2.Y, Point3.Y)));
            var min = new Vector2(Math.Min(Point1.X, Math.Min(Point2.X, Point3.X)), Math.Min(Point1.Y, Math.Min(Point2.Y, Point3.Y)));

            return new((int)min.X, (int)min.Y);
        }

        public List<PointF> DrawingPoints()
        {
            float diff_X = Point2.X - Point1.X;
            float diff_Y = Point2.Y - Point1.Y;
            int pointNum = Point2.ToPoint().Distance2D(Point1.ToPoint());

            float interval_X = diff_X / (pointNum + 1);
            float interval_Y = diff_Y / (pointNum + 1);

            var pointList = new List<PointF>();
            for (int i = 1; i <= pointNum; i++)
            {
                pointList.Add(new PointF(Point1.X + (interval_X * i), Point1.Y + (interval_Y * i)));
            }

            return pointList;
        }
    }

    public class RadialMenuSection
    {
        public Triangle Triangle { get; set; }

        public Vector2 IconPos { get; set; }

        public Rectangle Rectangle { get; set; }

        public Rectangle IconRectangle { get; set; }

        public List<PointF> Lines { get; set; }

        public Character_Model Character { get; set; }
    }

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

                        _iconSize = (int) Math.Min(c.ToPoint().Distance2D(v.ToPoint()) * 0.65, 128);

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

        public async Task<bool> IsNoKeyPressed()
        {
            while (GameService.Input.Keyboard.KeysDown.Count > 0)
            {
                await Task.Delay(250);
            }

            await Task.Delay(25);
            return true;
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
                    else if(section.Rectangle != null)
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
                        _settings.Radial_UseProfessionIcons.Value ? section.Character.Profession.GetData(_data.Professions).Color : Color.White,
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
                    _settings.Radial_UseProfessionIcons.Value ? _selected.Character.Profession.GetData(_data.Professions).Color : Color.White,
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
                _ = await IsNoKeyPressed();
                _selected?.Character.Swap();
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
