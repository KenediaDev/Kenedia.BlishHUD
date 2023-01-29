using Kenedia.Modules.Core.Services;
using System;

namespace Kenedia.Modules.Core.Models
{
    public class GameStateChangedEventArgs : EventArgs
    {
        public GameStatus OldStatus;
        public GameStatus Status;
    }
}
