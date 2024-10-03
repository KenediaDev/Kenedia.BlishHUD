using Microsoft.Xna.Framework;
using Kenedia.Modules.BuildsManager.Models;
using System;
using Microsoft.Xna.Framework.Graphics;
using Blish_HUD.Input;
using Blish_HUD;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class TagToggle : Blish_HUD.Controls.Control
    {
        public TagToggle()
        {
            Size = new(25);
        }

        public TemplateTag Tag { get; internal set; }

        public Action<TemplateTag> OnClicked { get; internal set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Tag is not null && Tag.Icon is not null && Tag.Icon.Texture is var texture)
            {
                spriteBatch.DrawOnCtrl(this, texture, bounds);
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (OnClicked is not null)
                OnClicked(Tag);
        }
    }
}
