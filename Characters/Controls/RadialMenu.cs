using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Characters.Controls
{
    public class RadialMenu : Control
    {
        //public AsyncTexture2D _backgroundRaw = AsyncTexture2D.FromAssetId(156003);
        public AsyncTexture2D _backgroundRaw = AsyncTexture2D.FromAssetId(1965546);
        public AsyncTexture2D _background;

        private readonly SettingsModel _settings;
        private readonly ObservableCollection<Character_Model> _characters;
        private readonly Func<Character_Model> _currentCharacter;
        private readonly Data _data;
        private List<Character_Model> _displayedCharacters;
        private int _radius;
        private int _iconSize;
        private int _idleIconSize;
        private int _hoveredIconSize;
        private readonly List<Point> _points = new();
        private readonly List<(Point, Character_Model)> _recs = new();
        private (Point, Character_Model)? _nearest = null;

        private CircleF? _circle;
        private CircleF? _outterCircle;
        private CircleF? _innerCircle;
        private Rectangle _backgroundRectangle;

        public RadialMenu(SettingsModel settings, ObservableCollection<Character_Model> characters, Container parent, Func<Character_Model> currentCharacter, Data data)
        {
            _settings = settings;
            _characters = characters;
            _currentCharacter = currentCharacter;
            _data = data;
            Parent = parent;

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

                int size = (int)(Math.Min(Parent.Width, Parent.Height) * 0.66);
                //Size = new(size, size);
                Size = new(Parent.Width, Parent.Height);
                //Location = new((Parent.Width - size) / 2, (Parent.Height - size) / 2);

                _points.Clear();
                _recs.Clear();

                var center = new Point(Size.X / 2, Size.Y / 2);
                int amount = _displayedCharacters.Count;
                double adiv = Math.PI * 2 / amount;

                int radius = size / 2;

                _iconSize = Math.Min(128, ((int)(radius * (Math.Sqrt(2) / amount))) * 3);
                _idleIconSize = (int)(_iconSize * 0.75);
                _hoveredIconSize = _iconSize;

                Point p;
                if (_displayedCharacters.Count == 1)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        _recs.Add(new(new(
                            center.X,
                            center.Y),
                            _displayedCharacters[i]));
                    }

                    _circle = null;
                }
                else
                {
                    for (int i = 0; i < amount; i++)
                    {
                        p = new Point(
                            x: center.X - (int)(radius * Math.Sin(2 * Math.PI * i / amount)),
                            y: center.Y - (int)(radius * Math.Cos(2 * Math.PI * i / amount))
                            );

                        _recs.Add(new(new(
                            p.X,
                            p.Y),
                            _displayedCharacters[i]));

                        _circle = new(center, radius + ((_iconSize / 2) + 35));
                        _innerCircle = new(center, radius - ((_iconSize / 2) + 30));
                        _outterCircle = new(center, radius + ((_iconSize / 2) + 35));
                    }
                }

                if (true || _radius != radius)
                {
                    Point min = new(
                        _recs.Select(e => e.Item1.X).Min(),
                        _recs.Select(e => e.Item1.Y).Min()
                        );

                    Point max = new(
                        _recs.Select(e => e.Item1.X).Max(),
                        _recs.Select(e => e.Item1.Y).Max()
                        );

                    //_background = _backgroundRaw.Texture.Duplicate().GetRegion(new Rectangle(40, 25, _backgroundRaw.Width - 100, _backgroundRaw.Height - 85));
                    //_backgroundRectangle = new(min.X - _iconSize, min.Y - _iconSize, size + (_iconSize * 2), size + (_iconSize * 2));
                    //_background = CreateTextureRing(_background, radius, (int)(_iconSize * 1.3), 20);
                }

                _radius = radius;
            }
        }

        private AsyncTexture2D CreateTextureRing(AsyncTexture2D texture, int radius, int thickness, int bordersize = 0)
        {
            Point center = new(texture.Width / 2, texture.Height / 2);
            radius = Math.Min(texture.Width / 2, texture.Height / 2);
            var orgColorMap = new Color[texture.Width * texture.Height];
            texture.Texture.GetData(orgColorMap);

            var colorMap = new Color[texture.Width * texture.Height];

            for (int i = 0; i < texture.Width; i++)
            {
                for (int j = 0; j < texture.Height; j++)
                {
                    int index = i + (j * texture.Width);
                    Color org = orgColorMap[index];

                    int dist = center.Distance2D(new(i, j));

                    float alpha = 1f;
                    if (dist >= radius - bordersize)
                    {
                        alpha = (1F - ((float)dist / (float)radius)) * (float)bordersize;
                    }
                    else if (dist <= radius - thickness && dist >= radius - thickness - bordersize)
                    {
                        alpha = 1F - ((radius - thickness - dist) * (1F / (float)bordersize));
                    }

                    colorMap[index] = dist > radius || dist < radius - thickness - bordersize ? Color.Transparent : org * alpha;
                }
            }
            texture.Texture.SetData(colorMap);

            return texture;
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);

            var copy = new List<(Point, Character_Model)>(_recs);
            _nearest = copy.OrderBy(e => e.Item1.Distance2D(RelativeMousePosition)).FirstOrDefault();
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
            string txt = string.Empty;

            if (_circle != null) spriteBatch.DrawCircle(_circle.Value, 360 * 2, Color.Black * 0.8f, _iconSize + 70);

            //if (_background != null)
            //{
            //    spriteBatch.DrawOnCtrl(
            //    this,
            //                ContentService.Textures.Pixel,
            //    _backgroundRectangle,
            //    _background.Bounds,
            //    Color.Transparent);

            //    spriteBatch.DrawOnCtrl(
            //    this,
            //    _background,
            //    _backgroundRectangle,
            //    _background.Bounds,
            //    Color.White * 0.9f);
            //}

            if (_innerCircle != null) spriteBatch.DrawCircle(_innerCircle.Value, 360 * 2, ContentService.Colors.ColonialWhite, 5);
            if (_outterCircle != null) spriteBatch.DrawCircle(_outterCircle.Value, 360 * 2, ContentService.Colors.ColonialWhite, 5);

            foreach (var r in _recs)
            {
                if (r.Item2.Icon != null)
                {
                    bool mouseOver = r == _nearest;
                    int iconSize = mouseOver ? _hoveredIconSize : _idleIconSize;

                    if (mouseOver)
                    {
                        txt = r.Item2.Name;

                        if (!r.Item2.HasDefaultIcon)
                        {
                            spriteBatch.DrawOnCtrl(
                            this,
                            ContentService.Textures.Pixel,
                            new(new(r.Item1.X - (iconSize / 2) - 2, r.Item1.Y - (iconSize / 2) - 2), new(iconSize + 4)),
                            Rectangle.Empty,
                            mouseOver ? ContentService.Colors.ColonialWhite : Color.White);
                        }
                    }

                    spriteBatch.DrawOnCtrl(
                        this,
                        r.Item2.Icon,
                        new(r.Item1.X - (iconSize / 2), r.Item1.Y - (iconSize / 2), iconSize, iconSize),
                        r.Item2.Icon.Bounds,
                        mouseOver && r.Item2.HasDefaultIcon ? r.Item2.Profession.GetData(_data.Professions).Color : Color.White);
                }
            }

            BasicTooltipText = txt;
        }

        protected override async void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_nearest != null)
            {
                Hide();
                _ = await IsNoKeyPressed();
                _nearest?.Item2.Swap();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _nearest = null;
            RecalculateLayout();
        }

        protected override void DisposeControl()
        {
            if (Parent != null) Parent.Resized -= Parent_Resized;

            foreach (Character_Model c in _characters)
            {
                c.Updated -= Character_Updated;
            }

            base.DisposeControl();
        }
    }
}
