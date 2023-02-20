using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using static Blish_HUD.ContentService;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.DataModels;
using Blish_HUD;

namespace Kenedia.Modules.BuildsManager.Controls.SkillConnectionEdit
{
    public class SkillConnectionControl : Blish_HUD.Controls.Control
    {
        private Rectangle _iconBounds;
        private Rectangle _idBounds;
        private Rectangle _nameBounds;
        private SkillConnection _skillConnection;

        public SkillConnection SkillConnection { get => _skillConnection; set => Common.SetProperty(ref _skillConnection, value, ApplyConnection); }

        public object Entry { get; set; }

        public string Name { get; set; }

        public int? Id { get; set; }

        public AsyncTexture2D Icon { get; set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int padding = 2;
            _iconBounds = new(padding, padding, Height - (padding * 2), Height - (padding * 2));
            _idBounds = new(_iconBounds.Right + 5, 0, 75, Height - padding);
            _nameBounds = new(_idBounds.Right + 5, 0, Width - _idBounds.Right + 5, Height - padding);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var color = MouseOver ? Colors.ColonialWhite : Color.White;

            if (MouseOver)
            {
                var borderColor = Colors.ColonialWhite;
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, bounds, Color.Black * 0.3F);

                // Top
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, borderColor * 0.8f);

                // Bottom
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, borderColor * 0.8f);

                // Left
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, borderColor * 0.8f);

                // Right
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, borderColor * 0.8f);
            }

            if (Icon != null) spriteBatch.DrawOnCtrl(this, Icon, _iconBounds, Color.White);
            if (Id != null) spriteBatch.DrawStringOnCtrl(this, $"{Id}", Content.DefaultFont16, _idBounds, color);
            if (Name != null) spriteBatch.DrawStringOnCtrl(this, Name, Content.DefaultFont16, _nameBounds, color);
        }

        protected virtual void ApplyConnection()
        {

        }

        protected virtual void ApplyItem()
        {

        }
    }
}
