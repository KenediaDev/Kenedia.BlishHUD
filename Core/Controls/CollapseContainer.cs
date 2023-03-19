using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Structs;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class CollapseContainer : Container
    {
        protected bool _canCollapse = true;
        private Glide.Tween _collapseAnim;
        private string _title;
        protected bool _showBorder;
        protected bool _collapsed;

        private readonly AsyncTexture2D _texturePanelHeader = AsyncTexture2D.FromAssetId(1032325);
        private readonly AsyncTexture2D _texturePanelHeaderActive = AsyncTexture2D.FromAssetId(1032324);
        private readonly AsyncTexture2D _textureCornerAccent = AsyncTexture2D.FromAssetId(1002144);
        private readonly AsyncTexture2D _textureLeftSideAccent = AsyncTexture2D.FromAssetId(605025);
        private readonly AsyncTexture2D _textureAccordionArrow = AsyncTexture2D.FromAssetId(155953);

        private readonly BasicTooltip _tooltip = new()
        {
            Parent = Graphics.SpriteScreen,
            ZIndex = int.MaxValue / 2,
            Visible = false,
        };

        private Vector2 _layoutAccordionArrowOrigin;
        private Rectangle _layoutTopLeftAccentBounds;
        private Rectangle _layoutBottomRightAccentBounds;
        private Rectangle _layoutCornerAccentSrc;
        private Rectangle _layoutLeftAccentBounds;
        private Rectangle _layoutLeftAccentSrc;
        private Rectangle _layoutHeaderBounds;
        private Rectangle _layoutHeaderTextBounds;
        private Rectangle _layoutHeaderIconBounds;
        private Rectangle _layoutAccordionArrowBounds;
        private RectangleDimensions _contentPadding = new(0);

        private RectangleDimensions _titleIconPadding = new(3, 3, 5, 3);
        private int _titleBarHeight = 36;

        [JsonIgnore] public float ArrowRotation { get; set; } = 0f;
        [JsonIgnore] public float AccentOpacity { get; set; } = 1f;

        public CollapseContainer()
        {
        }

        public RectangleDimensions TitleIconPadding
        {
            get => _titleIconPadding;
            set
            {
                _titleIconPadding = value;
                RecalculateLayout();
            }
        }

        public int TitleBarHeight
        {
            get => _titleBarHeight;
            set
            {
                _titleBarHeight = value;
                RecalculateLayout();
            }
        }

        public RectangleDimensions ContentPadding
        {
            get => _contentPadding;
            set
            {
                _contentPadding = value;
                RecalculateLayout();
            }
        }

        private Func<string> _setLocalizedTitleTooltip;

        public string TitleTooltipText
        {
            get => _tooltip.Text;
            set => _tooltip.Text = value;
        }

        public Func<string> SetLocalizedTooltip
        {
            get => _setLocalizedTooltip;
            set
            {
                _setLocalizedTooltip = value;
                BasicTooltipText = value?.Invoke();
            }
        }

        public Func<string> SetLocalizedTitleTooltip
        {
            get => _setLocalizedTitleTooltip;
            set
            {
                _setLocalizedTitleTooltip = value;
                TitleTooltipText = value?.Invoke();
            }
        }

        public Func<string> SetLocalizedTitle
        {
            get => _setLocalizedTitle;
            set
            {
                _setLocalizedTitle = value;
                Title = value?.Invoke();
            }
        }

        public string Title { get => _title; set => Common.SetProperty(ref _title, value, RecalculateLayout); }

        public AsyncTexture2D TitleIcon { get; set; }

        public Point MaxSize { get; set; } = Point.Zero;

        [JsonIgnore]
        public bool Collapsed
        {
            get => _collapsed;
            set
            {
                if (value)
                {
                    Collapse();
                }
                else
                {
                    Expand();
                }
            }
        }
        /// <inheritdoc />
        public void Expand()
        {
            if (!_collapsed) return;

            _collapseAnim?.CancelAndComplete();
            _ = SetProperty(ref _collapsed, false);

            var bounds = SizeDeterminingChild != null ? ControlUtil.GetControlBounds(new Control[] { SizeDeterminingChild }) : Point.Zero;
            int height = MaxSize != Point.Zero ? Math.Min(MaxSize.Y, bounds.Y + ContentPadding.Vertical + _titleBarHeight) : bounds.Y + ContentPadding.Vertical + _titleBarHeight;

            _collapseAnim = Animation.Tweener
                                     .Tween(this,
                                            new
                                            {
                                                Height = height,
                                                ArrowRotation = 0f,
                                                AccentOpacity = 1f
                                            },
                                            0.15f)
                                     .Ease(Glide.Ease.QuadOut);
        }

        private Func<string> _setLocalizedTitle;
        private Func<string> _setLocalizedTooltip;
        private Control _sizeDeterminingChild;
        private Rectangle _panelBounds;

        /// <inheritdoc />
        public void Collapse()
        {
            if (_collapsed) return;

            // Prevent us from setting the _preCollapseHeight midtransition by accident
            if (_collapseAnim != null && _collapseAnim.Completion < 1)
            {
                _collapseAnim.CancelAndComplete();
            }

            _ = SetProperty(ref _canCollapse, true);
            _ = SetProperty(ref _collapsed, true);

            _collapseAnim = Animation.Tweener
                                     .Tween(this,
                                            new
                                            {
                                                Height = _titleBarHeight,
                                                ArrowRotation = -MathHelper.PiOver2,
                                                AccentOpacity = 0f,
                                            },
                                            0.15f)
                                     .Ease(Glide.Ease.QuadOut);
        }

        public bool CanCollapse
        {
            get => _canCollapse;
            set => SetProperty(ref _canCollapse, value, true);
        }

        public bool ShowBorder
        {
            get => _showBorder;
            set => SetProperty(ref _showBorder, value, true);
        }

        public Control SizeDeterminingChild
        {
            get => _sizeDeterminingChild; set
            {
                var temp = _sizeDeterminingChild;
                if (Common.SetProperty(ref _sizeDeterminingChild, value, OnChildSet))
                {
                    if (temp != null) temp.Resized -= SizeDeterminingChild_Resized;
                }
            }
        }

        private void OnChildSet()
        {
            if (_sizeDeterminingChild != null)
            {
                _sizeDeterminingChild.Resized += SizeDeterminingChild_Resized;
                _sizeDeterminingChild.Hidden += SizeDeterminingChild_Hidden;
            }
        }

        private void SizeDeterminingChild_Hidden(object sender, EventArgs e)
        {
            Hide();
        }

        private void SizeDeterminingChild_Resized(object sender, ResizedEventArgs e)
        {
            SetHeight();
        }

        private void SetHeight()
        {
            var bounds = _sizeDeterminingChild != null ? ControlUtil.GetControlBounds(new Control[] { _sizeDeterminingChild }) : Point.Zero;
            int height = MaxSize != Point.Zero ? Math.Min(MaxSize.Y, bounds.Y + ContentPadding.Vertical + _titleBarHeight) : bounds.Y + ContentPadding.Vertical + _titleBarHeight;
            Height = Collapsed ? _titleBarHeight : height;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (_canCollapse && _layoutHeaderBounds.Contains(RelativeMousePosition))
            {
                _ = ToggleAccordionState();
            }

            base.OnClick(e);
        }

        private bool ToggleAccordionState()
        {
            Collapsed = !_collapsed;

            return _collapsed;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int num = (!string.IsNullOrEmpty(_title) || TitleIcon != null) ? _titleBarHeight : 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            if (ShowBorder)
            {
                num = Math.Max(7, num);
                num2 = 4;
                num3 = 7;
                num4 = 4;
                int num5 = Math.Min(_size.X, 256);
                _layoutTopLeftAccentBounds = new Rectangle(-2, num - 12, num5, _textureCornerAccent.Height);
                _layoutBottomRightAccentBounds = new Rectangle(_size.X - num5 + 2, _size.Y - 59, num5, _textureCornerAccent.Height);
                _layoutCornerAccentSrc = new Rectangle(256 - num5, 0, num5, _textureCornerAccent.Height);
                _layoutLeftAccentBounds = new Rectangle(num4 - 7, num, _textureLeftSideAccent.Width, Math.Min(_size.Y - num - num3, _textureLeftSideAccent.Height));
                _layoutLeftAccentSrc = new Rectangle(0, 0, _textureLeftSideAccent.Width, _layoutLeftAccentBounds.Height);
            }

            _panelBounds = new Rectangle(num4, num, _size.X - num4 - num2, _size.Y - num - num3);
            ContentRegion = new Rectangle(num4 + ContentPadding.Left, num + ContentPadding.Top, _size.X - num4 - num2 - ContentPadding.Horizontal, _size.Y - num - num3 - ContentPadding.Vertical);
            _layoutHeaderBounds = new Rectangle(0, 0, Width, num);
            _layoutHeaderIconBounds = TitleIcon != null ? new Rectangle(_layoutHeaderBounds.Left + _titleIconPadding.Left, _titleIconPadding.Top, num - _titleIconPadding.Vertical, num - _titleIconPadding.Vertical) : Rectangle.Empty;

            _layoutHeaderTextBounds = new Rectangle(_layoutHeaderIconBounds.Right + _titleIconPadding.Right, 0, _layoutHeaderBounds.Width - _layoutHeaderIconBounds.Width, num);

            int arrowSize = num - 4;

            _layoutAccordionArrowOrigin = new Vector2(16F, 16F);
            _layoutAccordionArrowBounds = new Rectangle(_layoutHeaderBounds.Right - arrowSize, (num - arrowSize) / 2, arrowSize, arrowSize).OffsetBy(new(arrowSize / 2, arrowSize / 2));
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _tooltip.Visible = false;

            if (!string.IsNullOrEmpty(_title))
            {
                spriteBatch.DrawOnCtrl(this, _texturePanelHeader, _layoutHeaderBounds);
                if (_canCollapse && _mouseOver && RelativeMousePosition.Y <= 36)
                {
                    _tooltip.Visible = true;
                    spriteBatch.DrawOnCtrl(this, _texturePanelHeaderActive, _layoutHeaderBounds);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(this, _texturePanelHeader, _layoutHeaderBounds);
                }

                spriteBatch.DrawStringOnCtrl(this, _title, Content.DefaultFont16, _layoutHeaderTextBounds, Color.White);
                if (TitleIcon != null) spriteBatch.DrawOnCtrl(this, TitleIcon, _layoutHeaderIconBounds, TitleIcon.Bounds, Color.White);
                if (_canCollapse)
                {
                    spriteBatch.DrawOnCtrl(this, _textureAccordionArrow, _layoutAccordionArrowBounds, null, Color.White, ArrowRotation, _layoutAccordionArrowOrigin);
                }
            }

            if (ShowBorder)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, _panelBounds, Color.Black * (0.1f * AccentOpacity));
                spriteBatch.DrawOnCtrl(this, _textureCornerAccent, _layoutTopLeftAccentBounds, _layoutCornerAccentSrc, Color.White * AccentOpacity, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally);
                spriteBatch.DrawOnCtrl(this, _textureCornerAccent, _layoutBottomRightAccentBounds, _layoutCornerAccentSrc, Color.White * AccentOpacity, 0f, Vector2.Zero, SpriteEffects.FlipVertically);
                spriteBatch.DrawOnCtrl(this, _textureLeftSideAccent, _layoutLeftAccentBounds, _layoutLeftAccentSrc, Color.Black * AccentOpacity, 0f, Vector2.Zero, SpriteEffects.FlipVertically);
            }

            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, _panelBounds, Color.Black * (0.3f * AccentOpacity));
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _sizeDeterminingChild.Resized -= SizeDeterminingChild_Resized;
            _sizeDeterminingChild = null;
        }
    }
}
