using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Control = Blish_HUD.Controls.Control;

namespace Kenedia.Modules.Core.Controls
{
    public class NotificationBadge : Control, ILocalizable
    {
        private readonly DetailedTexture _badge = new(222246);
        private Func<string> _setLocalizedText;
        private Control _anchor;
        private float _opcacity = 1f;

        public NotificationBadge()
        {
            Size = new Point(32);
            Services.LocalizingService.LocaleChanged += UserLocale_SettingChanged;
        }

        public CaptureType? CaptureInput { get; set; } = null;

        public Func<string> SetLocalizedText { get => _setLocalizedText; set => Common.SetProperty(ref _setLocalizedText, value, () => UserLocale_SettingChanged()); }

        public float HoveredOpacity { get; set; } = 1f;

        public new float Opacity
        {
            get => _opcacity; set
            {
                _opcacity = value;
                SetOpacity();
            }
        }

        public Control Anchor
        {
            get => _anchor;
            set => Common.SetProperty(ref _anchor, value, OnAnchorChanged);
        }

        public Func<string> SetLocalizedTooltip { get; set; }
        
        public Action ClickAction { get; set; }

        private void OnAnchorChanged(object sender, ValueChangedEventArgs<Control> e)
        {
            if (e.OldValue is not null)
            {
                e.OldValue.MouseEntered -= SetHoveredOpacity;
                e.OldValue.MouseLeft -= SetOpacity;
            }

            if (e.NewValue is not null)
            {
                e.NewValue.MouseEntered += SetHoveredOpacity;
                e.NewValue.MouseLeft += SetOpacity;
            }
        }

        private void SetOpacity(object sender = null, EventArgs e = null)
        {
            base.Opacity = Opacity;
        }

        private void SetHoveredOpacity(object sender = null, EventArgs e = null)
        {
            base.Opacity = HoveredOpacity;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            _badge.Bounds = new Rectangle(Point.Zero, Size);
        }

        public void UserLocale_SettingChanged(object sender = null, Blish_HUD.ValueChangedEventArgs<Locale> e = null)
        {
            BasicTooltipText = SetLocalizedText?.Invoke();
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            SetHoveredOpacity();
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            base.OnMouseLeft(e);
            SetOpacity();
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);
            SetHoveredOpacity();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            ClickAction?.Invoke();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _badge.Draw(this, spriteBatch);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Anchor = null;
            Services.LocalizingService.LocaleChanged -= UserLocale_SettingChanged;
        }

        protected override CaptureType CapturesInput()
        {
            return !MouseOver ? CaptureInput ?? base.CapturesInput() : base.CapturesInput();
        }
    }
}