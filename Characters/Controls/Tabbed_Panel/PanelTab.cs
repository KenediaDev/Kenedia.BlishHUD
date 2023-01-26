using Blish_HUD.Content;
using Blish_HUD.Controls;
using System;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class PanelTab : Panel
    {
        private AsyncTexture2D _icon;
        private Rectangle _textureRectangle = Rectangle.Empty;
        private bool _active;
        private string _name;

        public PanelTab()
        {
            TabButton = new TabButton()
            {
                BasicTooltipText = Name,
            };
        }

        private event EventHandler Activated;

        private event EventHandler TextureRectangleChanged;

        private event EventHandler Deactivated;

        private event EventHandler IconChanged;

        public TabButton TabButton { get; private set; }

        public AsyncTexture2D Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                TabButton.Icon = Icon;
                IconChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Rectangle TextureRectangle
        {
            get => _textureRectangle;
            set
            {
                _textureRectangle = value;
                TabButton.TextureRectangle = value;
                TextureRectangleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                TabButton.BasicTooltipText = value;
            }
        }

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                TabButton.Active = value;

                if (value)
                {
                    OnActivated();
                }
                else
                {
                    OnDeactivated();
                }
            }
        }

        protected void OnActivated()
        {
            Show();
            Activated?.Invoke(this, EventArgs.Empty);
        }

        protected void OnDeactivated()
        {
            Hide();
            Deactivated?.Invoke(this, EventArgs.Empty);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            TabButton?.Dispose();
            _icon = null;
        }
    }
}
