using Control = Blish_HUD.Controls.Control;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Blish_HUD.Input;
using ICheckable = Blish_HUD.Controls.ICheckable;

namespace Kenedia.Modules.Core.Controls
{
    public class HotbarButton : Control, ICheckable
    {
        private readonly DetailedTexture _active = new(157336);
        private readonly DetailedTexture _inactive = new(102538);

        public int Index;
        private bool _checked;

        public event EventHandler<Blish_HUD.Controls.CheckChangedEvent> CheckedChanged;

        public HotbarButton()
        {
            ClipsBounds = true;
        }


        public DetailedTexture Icon { get; set; }

        public bool Checked { get => _checked; set => Common.SetProperty(ref _checked, value, On_CheckChanged); }

        public Action<bool> OnCheckChanged { get; set; }

        private void On_CheckChanged(object sender, Models.ValueChangedEventArgs<bool> e)
        {
            OnCheckChanged?.Invoke(e.NewValue);
            CheckedChanged?.Invoke(this, new Blish_HUD.Controls.CheckChangedEvent(e.NewValue));
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            Checked = !Checked;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (Icon is not null)
            {
                Icon.Bounds = new(0, 0, Width, Height);
            }

            double offset = 0.7;
            _active.Bounds = new(Width - (int)(Width * offset), Height - (int)(Height * offset), Width, Height);
            _inactive.Bounds = new(Width - (int)(Width * offset), Height - (int)(Height * offset), Width, Height);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            RecalculateLayout();
            Icon?.Draw(this, spriteBatch, RelativeMousePosition);

            (Checked ? _active : _inactive).Draw(this, spriteBatch);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            if (Icon is not null)
            {
                Icon.Texture = null;
                Icon.FallBackTexture = null;
                Icon.HoveredTexture = null;
                Icon = null;
            }
        }
    }
}
