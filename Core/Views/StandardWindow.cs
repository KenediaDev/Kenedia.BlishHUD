using Blish_HUD.Content;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.Core.Views
{
    public class StandardWindow : Blish_HUD.Controls.StandardWindow
    {
        private readonly List<AnchoredContainer> _attachedContainers = new();

        public StandardWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
        }

        public bool IsActive => ActiveWindow == this;

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
            
            foreach(var container in _attachedContainers)
            {
                if(container.ZIndex != ZIndex) container.ZIndex = ZIndex;
            }
        }

        public void ShowAttached(AnchoredContainer container = null)
        {
            foreach (var c in _attachedContainers)
            {
                if (container != c)
                {
                    if(c.Visible) c.Hide();
                }
            }

            container?.Show();
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
                if(container.Parent == Graphics.SpriteScreen) container.Hide();
            }
        }
    }
}
