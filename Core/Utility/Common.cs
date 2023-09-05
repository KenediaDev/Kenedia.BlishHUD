using Blish_HUD;
using Blish_HUD.Content;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Kenedia.Modules.Core.Utility
{
    public static class Common
    {
        public static double Now => GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;

        public static bool SetProperty<T>(ref T property, T newValue, ValueChangedEventHandler<T> OnUpdated, bool triggerOnUpdate = true)
        {
            var temp = property;
            if (SetProperty(ref property, newValue))
            {
                if (triggerOnUpdate) OnUpdated?.Invoke(property, new(temp, newValue));

                return true;
            }

            return false;
        }

        public static bool SetProperty<T>(ref T property, T newValue, PropertyChangedEventHandler OnUpdated, bool triggerOnUpdate = true, [CallerMemberName] string propName = null)
        {
            if (SetProperty(ref property, newValue))
            {
                if (triggerOnUpdate) OnUpdated?.Invoke(property, new(propName));

                return true;
            }

            return false;
        }

        public static bool SetProperty<T>(ref T property, T newValue, Action OnUpdated, bool triggerOnUpdate = true)
        {
            if(SetProperty(ref property, newValue))
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

        public static int GetAssetIdFromRenderUrl(this string s)
        {
            int pos = s.LastIndexOf("/") + 1;

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

        static char[] s_invalids;

        /// <summary>Replaces characters in <c>text</c> that are not allowed in 
        /// file names with the specified replacement character.</summary>
        /// <param name="text">Text to make into a valid filename. The same string is returned if it is valid already.</param>
        /// <param name="replacement">Replacement character, or null to simply remove bad characters.</param>
        /// <param name="fancy">Whether to replace quotes and slashes with the non-ASCII characters ” and ⁄.</param>
        /// <returns>A string that can be used as a filename. If the output string would otherwise be empty, returns "_".</returns>
        public static string MakeValidFileName(string text, char? replacement = '_', bool fancy = true)
        {
            var sb = new StringBuilder(text.Length);
            char[] invalids = s_invalids ??= Path.GetInvalidFileNameChars();
            bool changed = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (invalids.Contains(c))
                {
                    changed = true;
                    char repl = replacement ?? '\0';
                    if (fancy)
                    {
                        if (c == '"') repl = '”'; // U+201D right double quotation mark
                        else if (c == '\'') repl = '’'; // U+2019 right single quotation mark
                        else if (c == '/') repl = '⁄'; // U+2044 fraction slash
                    }

                    if (repl != '\0')
                        _ = sb.Append(repl);
                }
                else
                {
                    _ = sb.Append(c);
                }
            }

            return sb.Length == 0 ? "_" : changed ? sb.ToString() : text;
        }

        public static double CalculateDistance(Vector3 vector1, Vector3 vector2)
        {
            double squaredDistance =
                Math.Pow(vector2.X - vector1.X, 2) +
                Math.Pow(vector2.Y - vector1.Y, 2) +
                Math.Pow(vector2.Z - vector1.Z, 2);
            
            double distance = Math.Sqrt(squaredDistance);

            return distance;
        }
    }
}
