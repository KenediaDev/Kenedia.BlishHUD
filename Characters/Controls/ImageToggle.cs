using Blish_HUD.Input;
using System;

namespace Kenedia.Modules.Characters.Controls
{
    public class ImageToggle : ImageGrayScaled
    {
        private bool _checked;

        public bool Checked
        {
            get => _checked;
            set
            {
                _checked = value;
                Active = value;
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler CheckedChanged;

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            Checked = !Checked;
        }
    }
}
