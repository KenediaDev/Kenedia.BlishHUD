using System;

namespace Kenedia.Modules.Core.Models
{
    public class BaseSettingsModel : IDisposable
    {
        private bool _isDisposed;

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                OnDispose();
            }
        }

        protected virtual void OnDispose()
        {
        }
    }
}
