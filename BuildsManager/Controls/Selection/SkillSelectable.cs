using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class SkillSelectable : Panel, INotifyPropertyChanged
    {
        private readonly AsyncTexture2D _textureVignette = AsyncTexture2D.FromAssetId(605003);
        private Rectangle _vignetteBounds;
        private readonly bool _created;

        public SkillSelectable()
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
                Enabled = false,
            };

            _skillSummary = new()
            {
                Parent = this,
                VerticalAlignment = Blish_HUD.Controls.VerticalAlignment.Top,
                AutoSizeHeight = true,
                //BackgroundColor = Color.White * 0.2F,
                Enabled = false,
            };

            _icon = new()
            {
                Parent = this,
                Size = new(48),
                Location = new(2, 2),
                Enabled = false,
            };

            _created = true;
        }

        public Template Template { get; set; }

        private readonly Image _icon;
        private readonly Label _name;
        private readonly Label _skillSummary;

        private Skill _skill;

        public Skill Skill { get => _skill; set => Common.SetProperty(ref _skill, value, OnSkillChanged); }

        public Func<RotationElement> GetRotationElement { get; set; }

        private void OnSkillChanged()
        {
            _name.Text = _skill?.Name;
            _skillSummary.Text = _skill?.Description;

            _icon.Texture = _skill?.Icon;

            OnPropertyChanged(nameof(Skill));
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(this, Textures.Pixel, _vignetteBounds, Rectangle.Empty, Color.Gray * 0.3F, 0F, Vector2.Zero);
            spriteBatch.DrawOnCtrl(this, _textureVignette, _vignetteBounds, _textureVignette.Bounds, Color.Black, 0F, Vector2.Zero);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (!_created) return;

            _name?.SetSize(ContentRegion.Width - _icon.Width - 10, _name.Font.LineHeight);
            _name?.SetLocation(_icon.Right + 10, _icon.Top);

            _skillSummary?.SetLocation(_name.Left, _name.Bottom);
            _skillSummary?.SetSize(_name.Width, ContentRegion.Height - _name.Height);

            _vignetteBounds = _icon.LocalBounds.Add(0, 0, 10, 10);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            var element = GetRotationElement();

            if (element != null)
            {
                element.Skill = Skill;
            }
        }
    }
}
