using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD;
using static Blish_HUD.ContentService;
using Kenedia.Modules.Core.Extensions;
using MonoGame.Extended;
using Blish_HUD.Controls;
using System.Diagnostics;
using MonoGame.Extended.BitmapFonts;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagGroupPanel : Blish_HUD.Controls.FlowPanel
    {
        public static int ControlPaddingY = 4;
        public static int OuterControlPaddingY = 30;
        public static int MaxTags = 6;

        private Rectangle _textBorder;
        private Rectangle _tagBorder;
        private Rectangle _bgBorder;
        private BitmapFont _textFont = Content.DefaultFont12;

        public TagGroupPanel(TagGroup tagGroup, Container container)
        {
            TagGroup = tagGroup;
            FlowDirection = Blish_HUD.Controls.ControlFlowDirection.LeftToRight;
            WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            Parent = container;

            OuterControlPadding = new(4, OuterControlPaddingY);
            ControlPadding = new(ControlPaddingY, ControlPaddingY);            
        }

        public TagGroup TagGroup { get; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            OuterControlPadding = new(4, 22);

            _textBorder = new Rectangle(0, 0, Width, _textFont.LineHeight);

            _tagBorder = new Rectangle(0, (int)_textBorder.Bottom + 5, Width, Height - _textBorder.Bottom);
            _bgBorder = new Rectangle(AbsoluteBounds.X + _tagBorder.X, AbsoluteBounds.Y + _tagBorder.Y, _tagBorder.Width, _tagBorder.Height);
        }

        protected override void OnMoved(MovedEventArgs e)
        {
            base.OnMoved(e);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, TagGroup?.Name, _textFont, _textBorder, Colors.DullColor);

            //spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(0, font.LineHeight + 2, bounds.Width, 2), StandardColors.Default * 0.5F);

            _bgBorder = new Rectangle(AbsoluteBounds.X + _tagBorder.X, AbsoluteBounds.Y + _tagBorder.Y, _tagBorder.Width, _tagBorder.Height);

            spriteBatch.FillRectangle(_bgBorder, Color.Black * 0.4F);
            spriteBatch.DrawFrame(this, _tagBorder, Color.Black, 2);
        }
    }
}
