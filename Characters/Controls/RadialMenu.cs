using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Characters.Controls
{
    public class RadialMenu : Control
    {
        private readonly SettingsModel _settings;
        private readonly ObservableCollection<Character_Model> _characters;
        private List<Character_Model> _displayedCharacters;
        private Point _iconSize;
        private  readonly List<Point> _points = new();
        private  readonly List<Rectangle> _recs = new();

        public RadialMenu(SettingsModel settings, ObservableCollection<Character_Model> characters, Container parent)
        {
            _settings = settings;
            _characters = characters;
            Parent = parent;
            BackgroundColor = Color.Orange * 0.2f;

            foreach (Character_Model c in _characters)
            {
                c.Updated += Character_Updated;
            }

            Parent.Resized += Parent_Resized;
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

                _iconSize = new Point(size, size);

                var center = new Point(Parent.Width / 2, Parent.Height / 2);
                int amount = 72;
                double adiv = Math.PI * 2 / amount;
                _points.Clear();
                _recs.Clear();

                int imgSize = (int) (size * 0.175 / amount);
                Point p;
                for (int i = 0; i < amount; i++)
                {
                    _points.Add(p = new(
                       (int)(center.X + (size / 2 * Math.Sin(i * adiv))),
                       (int)(center.Y + (size / 2 * Math.Cos(i * adiv)))));

                    _recs.Add(new(
                        p.X - (imgSize / 2),
                        p.Y - (imgSize / 2),
                        imgSize,
                        imgSize
                        ));
                }
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            foreach (var p in _points)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    ContentService.Textures.Pixel,
                    new(p.X - 5, p.Y - 5, 10, 10),
                    Rectangle.Empty,
                    Color.Red);
            }

            foreach (var r in _recs)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    ContentService.Textures.Pixel,
                    r,
                    Rectangle.Empty,
                    Color.Green);
            }
        }

        protected override void DisposeControl()
        {
            if(Parent != null ) Parent.Resized -= Parent_Resized;

            foreach (Character_Model c in _characters)
            {
                c.Updated -= Character_Updated;
            }

            base.DisposeControl();
        }
    }
}
