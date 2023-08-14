using Control = Blish_HUD.Controls.Control;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Blish_HUD.Content;
using Blish_HUD;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Blish_HUD.Input;
using ICheckable = Blish_HUD.Controls.ICheckable;

namespace Kenedia.Modules.Core.Controls
{
    public class HotbarButton : Control, ICheckable
    {
        public int Index;
        private bool _checked;

        public event EventHandler<Blish_HUD.Controls.CheckChangedEvent> CheckedChanged;

        public HotbarButton()
        {
            ClipsBounds = true;
        }

        public DetailedTexture Icon { get; set; }

        public bool Checked { get => _checked; set => Common.SetProperty(ref _checked, value, OnCheckChanged); }

        private void OnCheckChanged(object sender, Models.ValueChangedEventArgs<bool> e)
        {
            CheckedChanged?.Invoke(this, new Blish_HUD.Controls.CheckChangedEvent(e.NewValue));
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Icon != null)
            {
                Icon.Draw(this, spriteBatch, RelativeMousePosition);
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            Icon?.Dispose();
        }
    }
}
