using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.Core.Extensions;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Utility;
using static Blish_HUD.ContentService;
using System;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public abstract class Selectable<T> : Blish_HUD.Controls.Control
    {
        protected Rectangle ContentBounds;
        protected Rectangle IconBounds;
        protected Rectangle TextBounds;
        private bool _selected;

        public Selectable(T item, Blish_HUD.Controls.Container parent)
        {
            Item = item;
            Parent = parent;
            Height = 30;

            Width = Parent.Width - 25;
            Parent.Resized += Parent_Resized;
        }

        private void Parent_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            Width = Parent.Width - 25;
        }

        public T Item { get; }

        public Action<T>? OnClickAction { get; set; } = null;

        public bool Selected { get => _selected; set => Common.SetProperty(ref _selected, value, OnSelectedChanged); }

        private void OnSelectedChanged(object sender, Core.Models.ValueChangedEventArgs<bool> e)
        {
            BackgroundColor = Selected ? Colors.ColonialWhite * 0.1F : Color.Transparent;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (MouseOver)
            {
                spriteBatch.DrawFrame(this, bounds, Colors.ColonialWhite, 2);
            }

            if (Selected)
            {
                spriteBatch.DrawFrame(this, bounds, Colors.ColonialWhite * 0.5F, 2);
            }

            DrawItem(spriteBatch, bounds);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (OnClickAction is not null)
            {
                OnClickAction(Item);
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            ContentBounds = new(5, 2, Width - 30, Height - 4);
            IconBounds = new(ContentBounds.Left, ContentBounds.Top, ContentBounds.Height, ContentBounds.Height);
            TextBounds = new(IconBounds.Right + 5, ContentBounds.Top, ContentBounds.Width - IconBounds.Width - 5, ContentBounds.Height);
        }

        protected abstract void DrawItem(SpriteBatch spriteBatch, Rectangle bounds);
    }
}
