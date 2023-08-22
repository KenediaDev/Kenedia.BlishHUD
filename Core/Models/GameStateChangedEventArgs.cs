using Kenedia.Modules.Core.Services;
using System;

namespace Kenedia.Modules.Core.Models
{
    public class GameStateChangedEventArgs : EventArgs
    {
        public GameStatusType OldStatus;
        public GameStatusType Status;
    }
}
