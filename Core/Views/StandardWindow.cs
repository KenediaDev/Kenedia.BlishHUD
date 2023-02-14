using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.Core.Views
{
    public class StandardWindow : Blish_HUD.Controls.StandardWindow
    {
        private readonly BitmapFont _titleFont = GameService.Content.DefaultFont32;
        private Rectangle _subEmblemRectangle;
        private Rectangle _mainEmblemRectangle;
        private Rectangle _titleRectangle;
        private string _name;
        private readonly List<AnchoredContainer> _attachedContainers = new();

        public StandardWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
        }

        public bool IsActive => ActiveWindow == this;

        public SemVer.Version Version { get; set; }

        public string Name { get => _name; set => Common.SetProperty(ref _name, value, RecalculateLayout); }

        public AsyncTexture2D MainWindowEmblem { get; set; }

        public AsyncTexture2D SubWindowEmblem { get; set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _subEmblemRectangle = new(-43 + 64, -58 + 64, 64, 64);
            _mainEmblemRectangle = new(-43, -58, 128, 128);

            if (!string.IsNullOrEmpty(Name))
            {
                MonoGame.Extended.RectangleF titleBounds = _titleFont.GetStringRectangle(Name);
                _titleRectangle = new(80, 2, (int)titleBounds.Width, Math.Max(30, (int)titleBounds.Height));
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            foreach (var container in _attachedContainers)
            {
                if (container.ZIndex != ZIndex) container.ZIndex = ZIndex;
            }
        }

        public void ShowAttached(AnchoredContainer container = null)
        {
            foreach (var c in _attachedContainers)
            {
                if (container != c)
                {
                    if (c.Visible) c.Hide();
                }
            }

            container?.Show();
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);

            if (MainWindowEmblem != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    MainWindowEmblem,
                    _mainEmblemRectangle,
                    MainWindowEmblem.Bounds,
                    Color.White,
                    0f,
                    default);
            }

            if (SubWindowEmblem != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    SubWindowEmblem,
                    _subEmblemRectangle,
                    SubWindowEmblem.Bounds,
                    Color.White,
                    0f,
                    default);
            }

            if (_titleRectangle.Width < bounds.Width - (_subEmblemRectangle.Width - 20) && !string.IsNullOrEmpty(Name))
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    Name,
                    _titleFont,
                    _titleRectangle,
                    Colors.ColonialWhite,
                    false,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Bottom);
            }

            if (Version != null) spriteBatch.DrawStringOnCtrl(this, $"v. {Version}", Content.DefaultFont16, new(bounds.Right - 150, bounds.Top + 10, 100, 30), Color.White, false, true, 1, HorizontalAlignment.Right, VerticalAlignment.Top);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Children.DisposeAll();

            SubWindowEmblem?.Dispose();
            SubWindowEmblem = null;

            MainWindowEmblem?.Dispose();
            MainWindowEmblem = null;
        }

        protected virtual void AttachContainer(AnchoredContainer container)
        {
            _attachedContainers.Add(container);
        }

        protected virtual void UnAttachContainer(AnchoredContainer container)
        {
            _ = _attachedContainers.Remove(container);
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);

            foreach (var container in _attachedContainers)
            {
                if (container.Parent == Graphics.SpriteScreen) container.Hide();
            }
        }
    }
}
