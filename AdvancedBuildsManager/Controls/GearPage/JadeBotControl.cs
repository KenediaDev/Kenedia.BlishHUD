using Blish_HUD.Input;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.GearPage
{
    public class OLDJadeBotControl : Blish_HUD.Controls.Control
    {
        private readonly DetailedTexture _jadebotcore = new(2630946) { TextureRegion = new(36, 36, 56, 56) };
        private readonly DetailedTexture _identifier = new(436368) { };

        public OLDJadeBotControl()
        {
            Size = new(45, 45);
        }

        public Template Template
        {
            get; set
            {
                var temp = field;
                if (Common.SetProperty(ref field, value, ApplyTemplate))
                {
                    if (temp is not null) temp.PropertyChanged -= TemplateChanged;
                    if (field is not null) field.PropertyChanged += TemplateChanged;
                }
            }
        }

        public Action ClickAction { get; internal set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _jadebotcore.Draw(this, spriteBatch, RelativeMousePosition);
            //_identifier.Draw(this, spriteBatch, RelativeMousePosition);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int size = Math.Min(Width, Height);

            _identifier.Bounds = new(0, 0, size / 2, size / 2);
            _jadebotcore.Bounds = new(0, 0, size, size);
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
