using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using System;

namespace Kenedia.Modules.Core.Views
{
    public partial class SettingsWindow
    {
        public class BaseSettingsTab : Tab
        {
            public BaseSettingsTab(AsyncTexture2D icon, Func<IView> view, string name = null, int? priority = null) : base(icon, view, name, priority)
            {

            }            
        }
    }
}
