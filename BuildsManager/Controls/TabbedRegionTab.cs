﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class TabbedRegionTab
    {
        private readonly DetailedTexture _inactiveHeader = new(2200567);
        private readonly DetailedTexture _activeHeader = new(2200566);
        private AsyncTexture2D _icon;
        private RectangleDimensions _padding = new(5, 2);
        private Rectangle _bounds;
        private Rectangle _iconBounds;
        private Rectangle _textBounds;
        private string _header;
        private BitmapFont _font = GameService.Content.DefaultFont18;

        public TabbedRegionTab(Container container)
        {
            Container = container;
        }

        public Container Container { get; set; }

        public bool IsActive { get; set; }

        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }

        public Rectangle Bounds
        {
            get => _bounds;
            set
            {
                _bounds = value;
                _inactiveHeader.Bounds = value;
                _activeHeader.Bounds = value;
                RecalculateLayout();
            }
        }

        public bool IsHovered(Point p)
        {
            return Bounds.Contains(p);
        }

        public AsyncTexture2D Icon { get => _icon; set => _icon = value; }
        public BitmapFont Font { get => _font; set => _font = value; }

        public void DrawHeader(Control ctrl, SpriteBatch spriteBatch, Point mousePos)
        {
            Color color = IsActive ? Color.White : Color.White * (IsHovered(mousePos) ? 0.9F : 0.6F);
            (!IsActive ? _activeHeader : _inactiveHeader).Draw(ctrl, spriteBatch, mousePos, color);

            if (!string.IsNullOrEmpty(_header))
            {
                spriteBatch.DrawStringOnCtrl(ctrl, _header, _font, _textBounds, Color.White);
            }

            if (Icon != null)
            {
                spriteBatch.DrawOnCtrl(ctrl, Icon, _iconBounds, Color.White);
            }
        }

        private void RecalculateLayout()
        {
            _iconBounds = new(Bounds.Left + _padding.Left, Bounds.Top + _padding.Top, Bounds.Height - _padding.Vertical, Bounds.Height - _padding.Vertical);
            _textBounds = new(_iconBounds.Right + 10, _padding.Top, Bounds.Width - _iconBounds.Width - 10 - _padding.Right, Bounds.Height - _padding.Vertical);
        }
    }
}
