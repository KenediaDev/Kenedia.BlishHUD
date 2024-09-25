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
    //TODO fix the version in title header
    public class StandardWindow : Blish_HUD.Controls.StandardWindow
    {
        private Rectangle _subTitleRectangle;
        protected BitmapFont TitleFont = Content.DefaultFont32;
        protected BitmapFont SubTitleFont = Content.DefaultFont18;
        private Rectangle _subEmblemRectangle;
        private Rectangle _mainEmblemRectangle;
        private Rectangle _titleTextRegion;
        private Rectangle _titleRectangle;
        private Rectangle _versionRectangle;
        private string _name;
        protected BitmapFont VersionFont = Content.DefaultFont14;
        private SemVer.Version _version;
        private string _subName;
        private readonly List<AnchoredContainer> _attachedContainers = [];

        public StandardWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
        }

        public bool IsActive => ActiveWindow == this;

        public SemVer.Version Version { get => _version; set => Common.SetProperty(ref _version, value, RecalculateLayout); }

        public string Name { get => _name; set => Common.SetProperty(ref _name, value, RecalculateLayout); }

        public string SubName { get => _subName; set => Common.SetProperty(ref _subName, value, RecalculateLayout); }

        public AsyncTexture2D MainWindowEmblem { get; set; }

        public AsyncTexture2D SubWindowEmblem { get; set; }

        public Color NameColor { get; set; } = Colors.ColonialWhite;

        public Color SubNameColor { get; set; } = Color.White;

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _subEmblemRectangle = new(-43 + 64, -58 + 64, 64, 64);
            _mainEmblemRectangle = new(-43, -58, 128, 128);

            _titleTextRegion = new(Math.Max(Math.Max(MainWindowEmblem == null ? 0 : _mainEmblemRectangle.Right, SubWindowEmblem == null ? 0 : _subEmblemRectangle.Right) - 16, 0), 5, Width - Math.Max(_mainEmblemRectangle.Right, _subEmblemRectangle.Right) - 30, 30);

            _versionRectangle = Rectangle.Empty;
            if (Version is not null && !string.IsNullOrEmpty($"v. {Version}"))
            {
                MonoGame.Extended.RectangleF versionBounds = VersionFont.GetStringRectangle($"v. {Version}");
                _versionRectangle = new(_titleTextRegion.Right - (int)versionBounds.Width, _titleTextRegion.Top, (int)versionBounds.Width, _titleTextRegion.Height - 3);
            }

            if (!string.IsNullOrEmpty(Name))
            {
                List<BitmapFont> fontList =
                [
                    Content.DefaultFont32,
                    Content.DefaultFont18,
                    Content.DefaultFont16,
                ];

                foreach (BitmapFont font in fontList)
                {
                    var titleBounds = font.GetStringRectangle(Name);
                    Rectangle titleRectangle = new(_titleTextRegion.Left, _titleTextRegion.Top, (int)titleBounds.Width, _titleTextRegion.Height);

                    if (_titleTextRegion.Width >= titleBounds.Width + 10 + _versionRectangle.Width)
                    {
                        _titleRectangle = titleRectangle;
                        TitleFont = font;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(SubName))
            {
                var titleBounds = SubTitleFont.GetStringRectangle(SubName);
                Rectangle subTitleRectangle = new(_titleRectangle.Right + 25, _titleRectangle.Top, (int)titleBounds.Width, _titleRectangle.Height);

                _subTitleRectangle = subTitleRectangle;
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

            if (MainWindowEmblem is not null)
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

            if (SubWindowEmblem is not null)
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

            if (_titleRectangle.Width <= _titleTextRegion.Width && !string.IsNullOrEmpty(Name))
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    Name,
                    TitleFont,
                    _titleRectangle,
                    NameColor,
                    false,
                    true,
                    1,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Middle);
            }

            if (_subTitleRectangle.Width <= _titleTextRegion.Width && !string.IsNullOrEmpty(SubName))
            {
                spriteBatch.DrawStringOnCtrl(
                    this,
                    SubName,
                    SubTitleFont,
                    _subTitleRectangle,
                    SubNameColor,
                    false,
                    true,
                    1,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Middle);
            }

            if (Version is not null && (_titleTextRegion.Width >= _titleRectangle.Width + 10 + _versionRectangle.Width))
            {
                spriteBatch.DrawStringOnCtrl(this, $"v. {Version}", VersionFont, _versionRectangle, Color.White, false, true, 1, HorizontalAlignment.Right, VerticalAlignment.Bottom);
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            Children.DisposeAll();

            SubWindowEmblem = null;
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
