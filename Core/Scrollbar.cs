using System;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Kenedia.Modules.Core.Controls
{
    public class Scrollbar : Control
    {
        private const int s_control_width = 12;
        private const int s_min_length = 32;
        private const int s_cap_slack = 6;
        private const int s_scroll_arrow = 50;
        private const int s_scroll_cont_arrow = 10;
        private const int s_scroll_cont_track = 15;
        private const int s_scroll_wheel = 30;

        #region Load Static

        private static readonly TextureRegion2D s_textureTrack = Blish_HUD.Controls.Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-track");
        private static readonly TextureRegion2D s_textureUpArrow = Blish_HUD.Controls.Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-arrow-up");
        private static readonly TextureRegion2D s_textureDownArrow = Blish_HUD.Controls.Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-arrow-down");
        private static readonly TextureRegion2D s_textureBar = Blish_HUD.Controls.Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-bar-active");
        private static readonly TextureRegion2D s_textureThumb = Blish_HUD.Controls.Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-thumb");
        private static readonly TextureRegion2D s_textureTopCap = Blish_HUD.Controls.Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-cap-top");
        private static readonly TextureRegion2D s_textureBottomCap = Blish_HUD.Controls.Resources.Control.TextureAtlasControl.GetRegion("scrollbar/sb-cap-bottom");

        #endregion

        private enum ClickFocus
        {
            None,
            UpArrow,
            DownArrow,
            AboveBar,
            BelowBar,
            Bar
        };

        private ClickFocus _scrollFocus;
        private ClickFocus ScrollFocus
        {
            get => _scrollFocus;
            set
            {
                _scrollFocus = value;
                HandleClickScroll(true);
            }
        }

        private Tween _targetScrollDistanceAnim = null;

        private float _targetScrollDistance;
        private float TargetScrollDistance
        {
            get => _targetScrollDistanceAnim == null ? _scrollDistance : _targetScrollDistance;
            set
            {
                float aVal = MathHelper.Clamp(value, 0f, 1f);
                if (_associatedContainer != null && _targetScrollDistance != aVal)
                {
                    _targetScrollDistance = aVal;
                }
            }
        }

        private float _scrollDistance = 0f;
        public float ScrollDistance
        {
            get => _scrollDistance;
            set
            {
                if (SetProperty(ref _scrollDistance, MathHelper.Clamp(value, 0f, 1f), true))
                {
                    _targetScrollDistance = _scrollDistance;
                }

                UpdateAssocContainer();
            }
        }

        private int _scrollbarHeight = s_min_length;
        private int ScrollbarHeight
        {
            get => _scrollbarHeight;
            set
            {
                if (!SetProperty(ref _scrollbarHeight, value, true)) return;

                // Reclamps the scrolling content
                RecalculateScrollbarSize();
                UpdateAssocContainer();
            }
        }

        private double _scrollbarPercent = 1.0;

        /// <summary>
        /// See if the <see cref="Scrollbar"/> is beeing drawn.
        /// </summary>
        public bool Drawn => Visible && _scrollbarPercent < 0.99;

        /// <summary>
        /// Width of the <see cref="Scrollbar"/>.
        /// </summary>
        public int ScrollbarWidth => _barBounds.Width;

        private Container _associatedContainer;
        public Container AssociatedContainer
        {
            get => _associatedContainer;
            set => SetProperty(ref _associatedContainer, value);
        }

        private int ContainerContentDiff => _containerLowestContent - _associatedContainer.ContentRegion.Height;
        private int TrackLength => _size.Y - s_textureUpArrow.Height - s_textureDownArrow.Height;

        private int _scrollingOffset = 0;

        private Rectangle _upArrowBounds;
        private Rectangle _downArrowBounds;
        private Rectangle _barBounds;
        private Rectangle _trackBounds;

        public Scrollbar(Container container)
        {
            _associatedContainer = container;

            _upArrowBounds = Rectangle.Empty;
            _downArrowBounds = Rectangle.Empty;
            _barBounds = Rectangle.Empty;
            _trackBounds = Rectangle.Empty;

            Width = s_control_width;

            Input.Mouse.LeftMouseButtonReleased += MouseOnLeftMouseButtonReleased;
            _associatedContainer.MouseWheelScrolled += HandleWheelScroll;
        }

        private double _lastClickTime;

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Input.Mouse.LeftMouseButtonReleased -= MouseOnLeftMouseButtonReleased;
            _associatedContainer.MouseWheelScrolled -= HandleWheelScroll;
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);

            ScrollFocus = GetScrollFocus(Input.Mouse.Position - AbsoluteBounds.Location);
            _lastClickTime = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
        }

        private void MouseOnLeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            ScrollFocus = ClickFocus.None;
        }

        protected override void OnMouseWheelScrolled(MouseEventArgs e)
        {
            HandleWheelScroll(this, e);

            base.OnMouseWheelScrolled(e);
        }

        private void HandleWheelScroll(object sender, MouseEventArgs e)
        {
            // Don't scroll if the scrollbar isn't visible
            if (!Visible || _scrollbarPercent > 0.99) return;

            // Avoid scrolling nested panels
            var ctrl = (Control)sender;
            while (ctrl != _associatedContainer && ctrl != null)
            {
                if (ctrl is Panel) return;
                ctrl = ctrl.Parent;
            }

            if (GameService.Input.Mouse.State.ScrollWheelValue == 0) return;

            float normalScroll = Math.Sign(GameService.Input.Mouse.State.ScrollWheelValue);
            ScrollAnimated((int)normalScroll * -s_scroll_wheel * System.Windows.Forms.SystemInformation.MouseWheelScrollLines);
        }

        private ClickFocus GetScrollFocus(Point mousePos)
        {
            return mousePos switch
            {
                var point when _trackBounds.Contains(point) && !_barBounds.Contains(point) && _barBounds.Y < point.Y => ClickFocus.AboveBar,
                var point when _trackBounds.Contains(point) && !_barBounds.Contains(point) && _barBounds.Y > point.Y => ClickFocus.BelowBar,
                var point when _barBounds.Contains(point) => ClickFocus.Bar,
                var point when _upArrowBounds.Contains(point) => ClickFocus.UpArrow,
                var point when _downArrowBounds.Contains(point) => ClickFocus.DownArrow,
                _ => ClickFocus.None
            };
        }

        private void HandleClickScroll(bool clicked)
        {
            void scroll(int pixels)
            {
                ScrollDistance = ((ContainerContentDiff * ScrollDistance) + pixels) / ContainerContentDiff;
            }

            Action<int> getScrollAction(bool c)
            {
                return c ? ScrollAnimated : scroll;
            }

            var relMousePos = Input.Mouse.Position - AbsoluteBounds.Location;

            if (ScrollFocus == ClickFocus.None)
            {
                return;
            }
            else if (ScrollFocus == ClickFocus.BelowBar)
            {
                if (GetScrollFocus(relMousePos) == ClickFocus.BelowBar)
                    getScrollAction(clicked)(clicked ? -ScrollbarHeight : -s_scroll_cont_track);
            }
            else if (ScrollFocus == ClickFocus.AboveBar)
            {
                if (GetScrollFocus(relMousePos) == ClickFocus.AboveBar)
                    getScrollAction(clicked)(clicked ? ScrollbarHeight : s_scroll_cont_track);
            }
            else if (ScrollFocus == ClickFocus.UpArrow)
            {
                getScrollAction(clicked)(clicked ? -s_scroll_arrow : -s_scroll_cont_arrow);
            }
            else if (ScrollFocus == ClickFocus.DownArrow)
            {
                getScrollAction(clicked)(clicked ? s_scroll_arrow : s_scroll_cont_arrow);
            }
            else if (ScrollFocus == ClickFocus.Bar)
            {
                if (clicked)
                    _scrollingOffset = relMousePos.Y - _barBounds.Y;

                relMousePos = relMousePos - new Point(0, _scrollingOffset) - _trackBounds.Location;
                ScrollDistance = relMousePos.Y / (float)(TrackLength - ScrollbarHeight);
                TargetScrollDistance = ScrollDistance;
            }
        }

        private void ScrollAnimated(int pixels)
        {
            TargetScrollDistance = ((ContainerContentDiff * ScrollDistance) + pixels) / ContainerContentDiff;
            _targetScrollDistanceAnim = Animation.Tweener
                     .Tween(this, new { ScrollDistance = TargetScrollDistance }, 0f, overwrite: true).Ease(Ease.QuadOut);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse | CaptureType.MouseWheel;
        }

        private void UpdateAssocContainer()
        {
            AssociatedContainer.VerticalScrollOffset = (int)Math.Floor((_containerLowestContent - AssociatedContainer.ContentRegion.Height) * ScrollDistance);
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            double timeDiff = gameTime.TotalGameTime.TotalMilliseconds - _lastClickTime;

            if (ScrollFocus == ClickFocus.Bar)
                HandleClickScroll(false);
            else if (timeDiff > 200)
                HandleClickScroll(false);

            Invalidate();
        }

        public override void RecalculateLayout()
        {
            double lastVal = _scrollbarPercent;
            RecalculateScrollbarSize();

            if (lastVal != _scrollbarPercent && _associatedContainer != null)
            {
                ScrollDistance = 0;
                TargetScrollDistance = 0;
            }

            _upArrowBounds = new Rectangle((Width / 2) - (s_textureUpArrow.Width / 2), 0, s_textureUpArrow.Width, s_textureUpArrow.Height);
            _downArrowBounds = new Rectangle((Width / 2) - (s_textureDownArrow.Width / 2), Height - s_textureDownArrow.Height, s_textureDownArrow.Width, s_textureDownArrow.Height);
            _barBounds = new Rectangle((Width / 2) - (s_textureBar.Width / 2), (int)(ScrollDistance * (TrackLength - ScrollbarHeight)) + s_textureUpArrow.Height, s_textureBar.Width, ScrollbarHeight);
            _trackBounds = new Rectangle((Width / 2) - (s_textureTrack.Width / 2), _upArrowBounds.Bottom, s_textureTrack.Width, TrackLength);
        }

        private int _containerLowestContent;

        private void RecalculateScrollbarSize()
        {
            if (_associatedContainer == null) return;

            var tempContainerChidlren = _associatedContainer.Children.ToArray();

            _containerLowestContent = 0;

            for (int i = 0; i < tempContainerChidlren.Length; i++)
            {
                ref var child = ref tempContainerChidlren[i];

                if (child.Visible)
                {
                    _containerLowestContent = Math.Max(_containerLowestContent, child.Bottom);
                }
            }

            _containerLowestContent = Math.Max(_containerLowestContent, _associatedContainer.ContentRegion.Height);

            _scrollbarPercent = _associatedContainer.ContentRegion.Height / (double)_containerLowestContent;

            ScrollbarHeight = (int)Math.Max(Math.Floor(TrackLength * _scrollbarPercent) - 1, s_min_length);

            UpdateAssocContainer();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Don't show the scrollbar if there is nothing to scroll
            if (_scrollbarPercent > 0.99) return;

            var drawTint = (ScrollFocus == ClickFocus.None && MouseOver) || (_associatedContainer != null && _associatedContainer.MouseOver)
                               ? Color.White
                               : ContentService.Colors.Darkened(0.6f);

            drawTint = ScrollFocus != ClickFocus.None
                           ? ContentService.Colors.Darkened(0.9f)
                           : drawTint;

            spriteBatch.DrawOnCtrl(this, s_textureTrack, _trackBounds);

            spriteBatch.DrawOnCtrl(this, s_textureUpArrow, _upArrowBounds, drawTint);
            spriteBatch.DrawOnCtrl(this, s_textureDownArrow, _downArrowBounds, drawTint);

            spriteBatch.DrawOnCtrl(this, s_textureBar, _barBounds, drawTint);
            spriteBatch.DrawOnCtrl(this, s_textureTopCap, new Rectangle((Width / 2) - (s_textureTopCap.Width / 2), _barBounds.Top - s_cap_slack, s_textureTopCap.Width, s_textureTopCap.Height));
            spriteBatch.DrawOnCtrl(this, s_textureBottomCap, new Rectangle((Width / 2) - (s_textureBottomCap.Width / 2), _barBounds.Bottom - s_textureBottomCap.Height + s_cap_slack, s_textureBottomCap.Width, s_textureBottomCap.Height));
            spriteBatch.DrawOnCtrl(this, s_textureThumb, new Rectangle((Width / 2) - (s_textureThumb.Width / 2), _barBounds.Top + ((ScrollbarHeight / 2) - (s_textureThumb.Height / 2)), s_textureThumb.Width, s_textureThumb.Height), drawTint);
        }
    }
}