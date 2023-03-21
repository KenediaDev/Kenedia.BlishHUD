using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class OLDNourishmentControl : Blish_HUD.Controls.Control
    {
        private Template _template;
        private readonly DetailedTexture _nourishment = new() { TextureRegion = new(36, 36, 56, 56) };
        private readonly DetailedTexture _identifier = new(436367) { };

        public OLDNourishmentControl()
        {
            _nourishment.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\foodslot.png");
            Size = new(45, 45);
        }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (temp != null) temp.Changed -= TemplateChanged;
                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        public Action ClickAction { get; internal set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            //_identifier.Draw(this, spriteBatch, RelativeMousePosition);
            _nourishment.Draw(this, spriteBatch, RelativeMousePosition);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);

            _identifier.Bounds = new(0, 0, size / 2, size / 2);
            _nourishment.Bounds = new(0, 0, size, size);
        }

        public void ApplyTemplate()
        {
        }

        private void TemplateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            ClickAction?.Invoke();
        }
    }
}
