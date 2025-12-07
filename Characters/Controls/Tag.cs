using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Label = Kenedia.Modules.Core.Controls.Label;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class Tag : Blish_HUD.Controls.FlowPanel, IFontControl
    {
        private readonly Label _text;
        private readonly ImageButton _delete;
        private readonly ImageButton _dummy;

        private Color _disabledColor = new(156, 156, 156);
        private Texture2D _disabledBackground;

        public Tag()
        {
            Background = AsyncTexture2D.FromAssetId(1620622);
            FlowDirection = ControlFlowDirection.SingleLeftToRight;
            OuterControlPadding = new Vector2(3, 3);
            ControlPadding = new Vector2(4, 0);
            AutoSizePadding = new Point(5, 0);

            _delete = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(156012),
                HoveredTexture = AsyncTexture2D.FromAssetId(156011),
                TextureRectangle = new Rectangle(4, 4, 24, 24),
                Size = new Point(20, 20),
                BasicTooltipText = string.Format(strings.DeleteItem, strings.Tag),
            };
            _delete.Click += Delete_Click;

            _dummy = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(156025),
                TextureRectangle = new Rectangle(44, 48, 43, 46),
                Size = new Point(20, 20),
                Visible = false,
            };

            _text = new Label()
            {
                Parent = this,
                Height = Math.Max(20, Content.DefaultFont14.LineHeight + 4),
                VerticalAlignment = VerticalAlignment.Middle,
                Text = "Tag",
            };

            Height = Math.Max(20, Font.LineHeight + 4) + 5;
            Width = (int)Font.MeasureString("Tag").Width + _delete.Width + (int) OuterControlPadding.X + (int)AutoSizePadding.X + (int)ControlPadding.X;
            _text.Width = (int)Font.MeasureString("Tag").Width + 4;
        }

        public int TagPanelIndex { get; set; }

        public Point DesiredSize => new((int)Font.MeasureString(Text).Width + 30, Math.Max(20, Font.LineHeight + 4) + 5);

        public Action OnDeleteAction { get; set; }

        public Action OnClickAction { get; set; }

        public event EventHandler Deleted;

        public event EventHandler ActiveChanged;

        public BitmapFont Font
        {
            get => _text.Font;
            set
            {
                if (value is not null && _text.Font != value)
                {
                    _dummy.Size = new Point(value.LineHeight, value.LineHeight);
                    _delete.Size = new Point(value.LineHeight, value.LineHeight);
                    _text.Font = value;
                    _text.Height = Math.Max(20, value.LineHeight + 4);
                    Height = Math.Max(20, value.LineHeight + 4) + 5;
                }
            }
        }

        private bool _active = false;

        public void SetActive(bool active)
        {
            _active = active;
        }

        public bool Active
        {
            get => _active;
            set
            {
                if (_active != value)
                {
                    _active = value;
                    ActiveChanged?.Invoke(this, null);
                }
            }
        }

        public bool CanInteract { get; set; } = true;

        public AsyncTexture2D Background
        {
            get;
            set
            {
                field = value;
                if (value is not null)
                {
                    CreateDisabledBackground(null, null);
                    field.TextureSwapped += CreateDisabledBackground;
                }
            }
        }

        public bool ShowDelete
        {
            get => _delete.Visible;
            set
            {
                if (_delete is not null)
                {
                    _delete.Visible = value;
                    _dummy.Visible = !value;
                    Invalidate();
                }
            }
        }

        public string Text
        {
            get => _text?.Text;
            set
            {
                if (_text is not null)
                {
                    _text.Text = value;
                    _text.Width = (int)Font.MeasureString(value).Width + 4;
                    Width = (int)Font.MeasureString(value).Width + _delete.Width + (int)OuterControlPadding.X + (int)AutoSizePadding.X + (int)ControlPadding.X;
                }
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Background is not null)
            {
                AsyncTexture2D texture = Active ? Background : _disabledBackground is not null ? _disabledBackground : Background;

                spriteBatch.DrawOnCtrl(this, texture, bounds, bounds, Active ? Color.White * 0.98f : _disabledColor * 0.8f);
            }

            Color color = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (!_delete.MouseOver)
            {
                base.OnClick(e);

                if (CanInteract)
                {
                    Active = !Active;
                }

                OnClickAction?.Invoke();
            }
        }

        private void Delete_Click(object sender, MouseEventArgs e)
        {
            Deleted?.Invoke(this, EventArgs.Empty);
            OnDeleteAction?.Invoke();
            Dispose();
        }

        private void CreateDisabledBackground(object sender, ValueChangedEventArgs<Texture2D> e)
        {
            _disabledBackground = Background.Texture.ToGrayScaledPalettable();
            Background.TextureSwapped -= CreateDisabledBackground;
        }
    }
}
