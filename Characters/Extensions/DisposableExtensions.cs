using System;
using System.Collections.Generic;

namespace Kenedia.Modules.Characters.Extensions
{
    internal static class DisposableExtensions
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
