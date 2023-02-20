using Blish_HUD;
using Blish_HUD.Content;
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

        public static bool SetProperty<T>(ref T property, T newValue, PropertyChangedEventHandler OnUpdated, bool triggerOnUpdate = true, [CallerMemberName] string propName = null)
        {
            if (SetProperty<T>(ref property, newValue))
            {
                if (triggerOnUpdate) OnUpdated?.Invoke(property, new(propName));

                return true;
            }

            return false;
        }

        public static bool SetProperty<T>(ref T property, T newValue, Action OnUpdated, bool triggerOnUpdate = true)
        {
            if(SetProperty<T>(ref property, newValue))
            {
                if (triggerOnUpdate) OnUpdated?.Invoke();

                return true;
            }

            return false;
        }

        public static bool SetProperty<T>(ref T property, T newValue)
        {
            if (Equals(property, newValue))
            {
                return false;
            }

            property = newValue;

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

        public static AsyncTexture2D GetAssetFromRenderUrl(this RenderUrl? url)
        {
            if (url == null) return null;

            string s = url.ToString();
            int pos = url.ToString().LastIndexOf("/") + 1;

            if(int.TryParse(s.Substring(pos, s.Length - pos - 4), out int id))
            {
                return AsyncTexture2D.FromAssetId(id);
            }

            return null;
        }
    }
}
