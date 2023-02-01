using Kenedia.Modules.Core.Services;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.Core.Models
{
    public class ServiceCollection
    {
        public ServiceCollection(GameState gameState, ClientWindowService clientWindowService, SharedSettings sharedSettings)
        {
            GameState = gameState;
            ClientWindowService = clientWindowService;
            SharedSettings = sharedSettings;

            States[typeof(GameState)] = true;
            States[typeof(ClientWindowService)] = true;
            States[typeof(SharedSettings)] = true;
        }

        public Dictionary<Type, bool> States { get; } = new();

        public GameState GameState { get; }

        public ClientWindowService ClientWindowService { get; }

        public SharedSettings SharedSettings { get; }
    }
}
