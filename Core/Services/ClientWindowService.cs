using Blish_HUD;
using Microsoft.Xna.Framework;
using System;
using static Kenedia.Modules.Core.Utility.WindowsUtil.User32Dll;

namespace Kenedia.Modules.Core.Services
{
    public static class ClientWindowService
    {
        private static double _resolutionTick;

        public static int TitleBarHeight { get; set; }

        public static int SideBarWidth { get; set; }

        public static RECT ClientBounds { get; set; }

        public static RECT WindowBounds { get; set; }

        public static void Run(GameTime gameTime)
        {
            if (gameTime.TotalGameTime.TotalMilliseconds - _resolutionTick >= 50)
            {
                _resolutionTick = gameTime.TotalGameTime.TotalMilliseconds;

                IntPtr hWnd = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;

                RECT newRect = new();
                if (GetWindowRect(hWnd, ref newRect) && !WindowBounds.Matches(newRect))
                {
                    WindowBounds = newRect;

                    if (GetClientRect(hWnd, out RECT rect))
                    {
                        ClientBounds = rect;
                    }

                    TitleBarHeight = WindowBounds.Bottom - WindowBounds.Top - (ClientBounds.Bottom - ClientBounds.Top);
                    SideBarWidth = WindowBounds.Right - WindowBounds.Left - (ClientBounds.Right - ClientBounds.Left);
                }
            }
        }
    }
}
