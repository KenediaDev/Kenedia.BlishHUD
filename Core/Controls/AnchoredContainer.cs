﻿using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Structs;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using System;

namespace Kenedia.Modules.Core.Controls
{
    public class CornerIconContainer : AnchoredContainer
    {

    }

    public class CornerNotificationBadge : NotificationBadge
    {

    }
    public class CornerLoadingSpinner : LoadingSpinner
    {

    }

    public class AnchoredContainer : FramedContainer
    {
        private Control _anchor;

        public enum AnchorPos
        {
            None,
            Left,
            Top,
            Right,
            Bottom,
            AutoHorizontal,
            AutoVertical,
        }

        public CaptureType? CaptureInput { get; set; } = null;

        public Control Anchor {
            get => _anchor;
            set => Common.SetProperty(ref _anchor, value, OnAnchorChanged);
        }

        private void OnAnchorChanged(object sender, Models.ValueChangedEventArgs<Control> e)
        {
            if (e.OldValue is not null)
            {
                e.OldValue.Moved -= Anchor_Moved;
                e.OldValue.Resized -= Anchor_Moved;
            }

            if (e.NewValue is not null)
            {
                e.NewValue.Moved += Anchor_Moved;
                e.NewValue.Resized += Anchor_Moved;
            }
        }

        private void Anchor_Moved(object sender, EventArgs e)
        {
            Location = GetPosition();
        }

        public AnchorPos AnchorPosition { get; set; }

        public RectangleDimensions RelativePosition { get; set; } = new(0);

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (Anchor is not null)
            {
                Location = GetPosition();
            }
        }

        private Point GetPosition()
        {
            Rectangle anchorBounds = Anchor.AbsoluteBounds;

            switch (AnchorPosition)
            {
                case AnchorPos.Left:
                    return new(anchorBounds.Left - Width + RelativePosition.Left, anchorBounds.Top + RelativePosition.Top);

                case AnchorPos.Top:
                    return new(anchorBounds.Left + RelativePosition.Left, anchorBounds.Top - Height + RelativePosition.Top);

                case AnchorPos.Right:
                    return new(anchorBounds.Right + RelativePosition.Right, anchorBounds.Top + RelativePosition.Top);

                case AnchorPos.Bottom:
                    return new(anchorBounds.Left + RelativePosition.Left, anchorBounds.Bottom + RelativePosition.Bottom);

                case AnchorPos.AutoHorizontal:
                    bool left = anchorBounds.Left + (anchorBounds.Width / 2) > GameService.Graphics.SpriteScreen.Right / 2;
                    return left ? new(anchorBounds.Left - Width + RelativePosition.Left, anchorBounds.Top + RelativePosition.Top) : new(anchorBounds.Right + RelativePosition.Right, anchorBounds.Top + RelativePosition.Top);

                case AnchorPos.AutoVertical:
                    bool top = anchorBounds.Top + (anchorBounds.Height / 2) > GameService.Graphics.SpriteScreen.Bottom / 2;
                    return top ? new(anchorBounds.Left + RelativePosition.Left, anchorBounds.Top - Height + RelativePosition.Top) : new(anchorBounds.Left + RelativePosition.Left, anchorBounds.Bottom + RelativePosition.Bottom);
                case AnchorPos.None:
                    return Location;
            }

            return Location;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

        }

        protected override CaptureType CapturesInput()
        {
            return CaptureInput ?? base.CapturesInput();
        }
    }
}
