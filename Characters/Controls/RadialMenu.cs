using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Characters.Controls
{
    public class RadialMenu : Control
    {
        private readonly SettingsModel _settings;
        private readonly ObservableCollection<Character_Model> _characters;
        private List<Character_Model> _displayedCharacters;
        private int _iconSize;
        private readonly List<Point> _points = new();
        private readonly List<(Rectangle, Character_Model)> _recs = new();
        private (Rectangle, Character_Model)? _nearest = null;

        public RadialMenu(SettingsModel settings, ObservableCollection<Character_Model> characters, Container parent)
        {
            _settings = settings;
            _characters = characters;
            Parent = parent;
            BackgroundColor = Color.Black * 0.7f;

            foreach (Character_Model c in _characters)
            {
                c.Updated += Character_Updated;
            }

            Parent.Resized += Parent_Resized;
            Input.Keyboard.KeyPressed += Keyboard_KeyPressed;
        }

        public bool HasDisplayedCharacters()
        {
            return _characters != null && _characters.Where(e => e.ShowOnRadial).Count() > 0;
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
                Size = new(size, size);
                Size = new(Parent.Width, Parent.Height);
                //Location = new((Parent.Width - size) / 2, (Parent.Height - size) / 2);

                _points.Clear();
                _recs.Clear();

                var center = new Point(Size.X / 2, Size.Y / 2);
                int amount = _displayedCharacters.Count;
                double adiv = Math.PI * 2 / amount;

                int radius = size / 2;

                _iconSize = Math.Min(128, ((int)(radius * (Math.Sqrt(2) / amount))) * 3);

                Point p;
                for (int i = 0; i < amount; i++)
                {
                    p = new(
                       (int)(center.X + ((size - _iconSize - 2) / 2 * Math.Sin(i * adiv))),
                       (int)(center.Y + ((size - _iconSize - 2) / 2 * Math.Cos(i * adiv))));

                    _recs.Add(new(new(
                        p.X - (_iconSize / 2),
                        p.Y - (_iconSize / 2),
                        _iconSize,
                        _iconSize
                        ), _displayedCharacters[i]));
                }
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);

            var copy = new List<(Rectangle, Character_Model)>(_recs);
            _nearest = copy.OrderBy(e => e.Item1.Center.Distance2D(RelativeMousePosition)).FirstOrDefault();
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
            foreach (var r in _recs)
            {
                if (r.Item2.Icon != null)
                {
                    bool mouseOver = r == _nearest;

                    if (mouseOver)
                    {
                        txt = r.Item2.Name;
                        spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        r.Item1.Add(-2, -2, 4, 4),
                        Rectangle.Empty,
                        mouseOver ? ContentService.Colors.ColonialWhite : Color.White);
                    }

                    spriteBatch.DrawOnCtrl(
                        this,
                        r.Item2.Icon,
                        r.Item1,
                        r.Item2.Icon.Bounds,
                        Color.White);
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
