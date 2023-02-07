using Kenedia.Modules.Core.Services;
using System;
using System.Collections.Generic;

namespace Kenedia.Modules.Core.Models
{
    public class ServiceCollection : IDisposable
    {
        private bool _disposed;

        public ServiceCollection(GameState gameState, ClientWindowService clientWindowService, SharedSettings sharedSettings, TexturesService texturesService, InputDetectionService inputDetectionService)
        {
            GameState = gameState;
            ClientWindowService = clientWindowService;
            SharedSettings = sharedSettings;
            TexturesService = texturesService;
            InputDetectionService = inputDetectionService;

            States[typeof(GameState)] = true;
            States[typeof(ClientWindowService)] = true;
            States[typeof(SharedSettings)] = true;
            States[typeof(TexturesService)] = true;
            States[typeof(InputDetectionService)] = true;
        }

        public Dictionary<Type, bool> States { get; } = new();

        public GameState GameState { get; }

        public ClientWindowService ClientWindowService { get; }

        public SharedSettings SharedSettings { get; }

        public TexturesService TexturesService { get; }

        public InputDetectionService InputDetectionService { get; }

        public void Dispose()
        {
            if(_disposed ) return;
            _disposed = true;

            GameState.Dispose();
            TexturesService.Dispose();
        }
    }
}
