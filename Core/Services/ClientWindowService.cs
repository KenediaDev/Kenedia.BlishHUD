using Blish_HUD;
using Microsoft.Xna.Framework;
using System;
using static Kenedia.Modules.Core.Utility.WindowsUtil.User32Dll;

namespace Kenedia.Modules.Core.Services
{
    public class ClientWindowService
    {
        private double _resolutionTick;

        public bool Enabled { get; set; } = true;

        public int TitleBarHeight { get; set; }

        public int SideBarWidth { get; set; }

        public RECT ClientBounds { get; set; }

        public RECT WindowBounds { get; set; }

        public void Run(GameTime gameTime)
        {
            if (!Enabled) return;

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
