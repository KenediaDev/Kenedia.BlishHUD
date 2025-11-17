using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Control = Blish_HUD.Controls.Control;

namespace Kenedia.Modules.Core.Controls
{
    public class NotificationBadge : Control
    {
        private readonly List<ConditionalNotification> _removeNotifications = [];
        private DetailedTexture _badge = new(222246);
        private Control _anchor;
        private float _opcacity = 1f;

        private double _lastChecked;
        private bool _deleting = false;

        private string _message;

        public NotificationBadge()
        {
            Size = new Point(32);
            Services.LocalizingService.LocaleChanged += UserLocale_SettingChanged;

            Notifications.CollectionChanged += Notifications_CollectionChanged; ;
        }

        public CaptureType? CaptureInput { get; set; } = null;

        public ObservableCollection<ConditionalNotification> Notifications { get; } = [];

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

        public Action ClickAction { get; set; }

        private void Notifications_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_deleting)
                return;

            e.OldItems?.Cast<ConditionalNotification>().ForEach(n => n.ConditionMatched -= Notification_ConditionMatched);
            e.NewItems?.Cast<ConditionalNotification>().ForEach(n => n.ConditionMatched += Notification_ConditionMatched);

            _message = Notifications.Count > 0 ? string.Join(Environment.NewLine, Notifications.Select(e => e.NotificationText).Distinct().Enumerate(Environment.NewLine, "[{0}]: ")) : string.Empty;
            BasicTooltipText = _message;

            Visible = Notifications.Count > 0;
        }

        private void Notification_ConditionMatched(object sender, EventArgs e)
        {
            _removeNotifications.Add(sender as ConditionalNotification);
        }

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
            _message = Notifications.Count > 0 ? string.Join(Environment.NewLine, Notifications.Select(e => e.NotificationText).Distinct().Enumerate(Environment.NewLine, "[{0}]: ")) : string.Empty;
            BasicTooltipText = _message;
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
            _badge = null;
            Notifications?.Clear();
            Services.LocalizingService.LocaleChanged -= UserLocale_SettingChanged;
        }

        protected override CaptureType CapturesInput()
        {
            return !MouseOver ? CaptureInput ?? base.CapturesInput() : base.CapturesInput();
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);
            
            if (gameTime.TotalGameTime.TotalMilliseconds - _lastChecked < 1000 || Notifications.Count <= 0)
            {
                return;
            }

            _lastChecked = gameTime.TotalGameTime.TotalMilliseconds;

            foreach (var notification in Notifications)
            {
                notification.CheckCondition();
            }

            for (int i = 0; i < _removeNotifications.Count; i++)
            {
                _deleting = i < _removeNotifications.Count - 1;
                _ = Notifications.Remove(_removeNotifications[i]);
            }
        }

        public void AddNotification(ConditionalNotification notification)
        {
            if (notification is null)
            {
                return;
            }

            notification.ConditionMatched += Notification_ConditionMatched;
            Notifications.Add(notification);
        }
    }
}