using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Models
{
    public class BaseSettingsModel : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                OnDispose();
            }
        }

        protected virtual void OnDispose()
        {
        }
    }
}
