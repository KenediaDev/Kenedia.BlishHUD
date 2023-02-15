using Blish_HUD;
using Gw2Sharp.WebApi;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Kenedia.Modules.Core.Utility
{
    public static class Common
    {
        public static double Now()
        {
            return GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;
        }

        public static bool SetProperty<T>(ref T property, T newValue, PropertyChangedEventHandler OnUpdated = null, bool triggerOnUpdate = true, [CallerMemberName] string propName = null)
        {
            if (Equals(property, newValue))
            {
                return false;
            }

            property = newValue;

            Debug.WriteLine($"propName {propName}");
            if (triggerOnUpdate) OnUpdated?.Invoke(property, new(propName));

            return true;
        }

        public static bool SetProperty<T>(ref T property, T newValue, Action OnUpdated = null, bool triggerOnUpdate = true)
        {
            if (Equals(property, newValue))
            {
                return false;
            }

            property = newValue;
            if (triggerOnUpdate) OnUpdated?.Invoke();

            return true;
        }

        public static T GetPropertyValue<T>(object obj, string propName)
        {
            var p = obj.GetType().GetProperty(propName);

            if (p == null)
            {
                return default;
            };

            object o = p.GetValue(obj, null);

            if (o == null)
            {
                return default;
            };

            if (o.GetType() == typeof(T))
            {
                return (T)o;
            }

            return default;
        }

        public static string GetPropertyValueAsString(object obj, string propName)
        {
            var p = obj.GetType().GetProperty(propName);

            if (p == null)
            {
                return default;
            };

            object o = p.GetValue(obj, null);

            if (o == null)
            {
                return default;
            };

            return o.ToString();
        }

        public static int GetAssetIdFromRenderUrl(this RenderUrl? url)
        {
            if (url == null) return 0;

            string s = url.ToString();
            int pos = s.LastIndexOf("/") + 1;

            return int.TryParse(s.Substring(pos, s.Length - pos - 4), out int id) ? id : 0;
        }

        public static int GetAssetIdFromRenderUrl(this RenderUrl url)
        {
            string s = url.ToString();
            int pos = s.LastIndexOf("/") + 1;

            return int.TryParse(s.Substring(pos, s.Length - pos - 4), out int id) ? id : 0;
        }

        public static int GetAssetIdFromRenderUrl(string s)
        {
            int pos = s.ToString().LastIndexOf("/") + 1;

            return int.TryParse(s.Substring(pos, s.Length - pos - 4), out int id) ? id : 0;
        }
    }
}
