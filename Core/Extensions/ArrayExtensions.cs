using System;
using System.Collections.Generic;
using System.Text;
using Gw2Sharp.WebApi;

namespace Kenedia.Modules.Core.Extensions
{
    public static class ArrayExtensions
    {
        public static List<T> ToList<T>(this Array array)
        {
            var list = new List<T>();

            foreach (T item in array)
            {
                list.Add(item);
            }

            return list;
        }
    }
}
