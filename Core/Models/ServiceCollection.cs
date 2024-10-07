using Kenedia.Modules.Core.Services;
using System;

namespace Kenedia.Modules.Core.Models
{
    public class CoreServiceCollection : IDisposable
    {
        private bool _isDisposed;

        public CoreServiceCollection(GameStateDetectionService gameState, ClientWindowService clientWindowService, SharedSettings sharedSettings, InputDetectionService inputDetectionService)
        {
            GameStateDetectionService = gameState;
            ClientWindowService = clientWindowService;
            SharedSettings = sharedSettings;
            InputDetectionService = inputDetectionService;            
        }

        public GameStateDetectionService GameStateDetectionService { get; }

        public ClientWindowService ClientWindowService { get; }

        public SharedSettings SharedSettings { get; }

        public InputDetectionService InputDetectionService { get; }

        public void Dispose()
        {
            if(_isDisposed ) return;
            _isDisposed = true;

            GameStateDetectionService.Dispose();
            TexturesService.Dispose();
        }
    }
}
