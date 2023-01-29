using System;
using System.Collections.Generic;

namespace Kenedia.Modules.Core.Extensions
{
    public static class DisposableExtensions
    {
        public static void DisposeAll(this IEnumerable<IDisposable> disposables)
        {
            foreach (IDisposable d in disposables)
            {
                d?.Dispose();
            }
        }
    }
}
