using Kenedia.Modules.Characters.Services;
using System;

namespace Kenedia.Modules.Characters.Models
{
    public class GameStateChangedEventArgs : EventArgs
    {
        public GameStatus OldStatus;
        public GameStatus Status;
    }
}
