using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class Selectable : Blish_HUD.Controls.Control
    {
        public Action OnClickAction { get; set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {

        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            OnClickAction?.Invoke();
        }
    }
}
