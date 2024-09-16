using System;

namespace Kenedia.Modules.Core.Controls
{
    public class CornerElements : IDisposable
    {
        public CornerIcon CornerIcon { get; } = new();

        public CornerNotificationBadge CornerNotificationBadge { get; } = new();

        public CornerLoadingSpinner CornerLoadingSpinner { get; } = new();

        public CornerElements()
        {

        }

        public void Dispose()
        {
            CornerIcon?.Dispose();
            CornerNotificationBadge?.Dispose();
            CornerLoadingSpinner?.Dispose();
        }
    }
}
