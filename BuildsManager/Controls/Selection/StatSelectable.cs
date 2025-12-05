using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class StatSelectable : Panel
    {
        private readonly AsyncTexture2D _textureVignette = AsyncTexture2D.FromAssetId(605003);
        private Rectangle _vignetteBounds;
        private readonly bool _created;
        private readonly Image _icon;
        private readonly Label _name;
        private readonly Label _statSummary;

        private double _attributeAdjustment;
        private Stat _stat;

        public StatSelectable()
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            BorderWidth = new(2);
            BorderColor = Color.Black;
            BackgroundColor = Color.Black * 0.4F;
            ContentPadding = new(5);

            _name = new()
            {
                Parent = this,
                WrapText = false,
                AutoSizeHeight = true,
                //BackgroundColor = Color.Blue * 0.2F,
                Font = Content.DefaultFont16,
                TextColor = Colors.ColonialWhite,
            };

            _statSummary = new()
            {
                Parent = this,
                AutoSizeHeight = true,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Top,
                //BackgroundColor = Color.White * 0.2F,
            };

            _icon = new()
            {
                Parent = this,
                Size = new(48),
                Location = new(2, 2),
            };

            _created = true;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            OnClickAction?.Invoke();
        }

        public Stat Stat { get => _stat; set => Common.SetProperty(ref _stat, value, OnStatChanged); }

        public double AttributeAdjustment { get => _attributeAdjustment; set => Common.SetProperty(ref _attributeAdjustment, value, OnMultiplierChanged); }

        public Action OnClickAction { get; set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (!_created) return;

            _name?.SetSize(ContentRegion.Width - _icon.Width - 10, _name.Font.LineHeight);
            _name?.SetLocation(_icon.Right + 10, _icon.Top);

            _statSummary?.SetLocation(_name.Left, _name.Bottom);
            _statSummary?.SetSize(_name.Width, ContentRegion.Height - _name.Height);

            _vignetteBounds = _icon.LocalBounds.Add(0, 0, 10, 10);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(this, Textures.Pixel, _vignetteBounds, Rectangle.Empty, Color.Gray * 0.3F, 0F, Vector2.Zero);
            spriteBatch.DrawOnCtrl(this, _textureVignette, _vignetteBounds, _textureVignette.Bounds, Color.Black, 0F, Vector2.Zero);
        }

        private void OnStatChanged()
        {
            _name.SetLocalizedText = () => _stat?.Name;
            _statSummary.SetLocalizedText = () => _stat?.Attributes.ToString(AttributeAdjustment);
            
            _icon.Texture = _stat?.Icon.Texture;
            _icon.SourceRectangle = _stat?.Icon.TextureRegion ?? Rectangle.Empty;
        }

        private void OnMultiplierChanged()
        {
            _statSummary.SetLocalizedText = () => _stat?.Attributes.ToString(AttributeAdjustment);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Stat = null;
            _textureVignette.Dispose();
            base.DisposeControl();
        }
    }
}
