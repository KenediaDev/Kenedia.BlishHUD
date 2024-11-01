using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Core.Controls
{
    public class Blocker : Control
    {
        private List<(Rectangle r, float opacity, int thickness)> _borders = [];

        public Blocker()
        {

        }

        private Control _coveredControl;

        public Control CoveredControl
        {
            get { return _coveredControl; }
            set => Common.SetProperty(ref _coveredControl, value, OnCoveredControlChanged);
        }

        private void OnCoveredControlChanged(object sender, Models.ValueChangedEventArgs<Control> e)
        {
            if (e.OldValue is not null)
                e.OldValue.Resized -= CoveredControl_Resized;

            if (e.NewValue is not null)
                e.NewValue.Resized += CoveredControl_Resized;
        }

        private void CoveredControl_Resized(object sender, ResizedEventArgs e)
        {
            Size = e.CurrentSize;
        }

        public int BorderWidth { get; set; } = 3;

        public string Text { get; set; } = string.Empty;

        public Color TextColor { get; set; } = Color.White;

        public BitmapFont TextFont { get; set; } = Content.DefaultFont18;

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _borders.Clear();
            ZIndex = (Parent?.ZIndex ?? 0) + 25;

            int strength = BorderWidth;
            int fadeLines = Math.Max(0, Math.Min(strength - 1, 4));

            for (int i = fadeLines - 1; i >= 0; i--)
            {
                _borders.Add((new Rectangle(i, i, Width - (i * 2), Height - (i * 2)), GetFadeValue(i), 1));
            }

            if (fadeLines < strength)
            {
                _borders.Add((new Rectangle(fadeLines, fadeLines, Width - (fadeLines * 2), Height - (fadeLines * 2)), GetFadeValue(int.MaxValue), strength - fadeLines));
            }
        }

        private float GetFadeValue(int i)
        {
            return
                i == 0 ? 0.25f :
                i == 1 ? 0.5f :
                i == 2 ? 0.75f :
                1f;

        }
        private float GetFadeValue(int i, int fadeLines, int strength)
        {
            float fadeLineOpacity = strength / (float)fadeLines;
            return fadeLineOpacity * i;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            foreach (var (r, opacity, thickness) in _borders)
            {
                spriteBatch.DrawFrame(this, r, Color.Black * opacity, thickness);
            }

            if (!string.IsNullOrEmpty(Text))
            {
                spriteBatch.DrawStringOnCtrl(this, Text, TextFont, bounds, TextColor, false, HorizontalAlignment.Center, VerticalAlignment.Middle);
            }

            //spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.5f);
        }
    }
}
