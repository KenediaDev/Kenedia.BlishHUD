using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using static Kenedia.Modules.Core.Utility.WindowsUtil.User32Dll;

namespace Kenedia.Modules.Core.Services
{
    public class ClientWindowService
    {
        private double _resolutionTick;

        public bool Enabled { get; set; } = true;

        public int TitleBarHeight { get; set; }

        public int SideBarWidth { get; set; }

        public Point Resolution { get; private set; }

        public RECT ClientBounds { get; set; }

        public RECT WindowBounds { get; set; }

        public event EventHandler<ValueChangedEventArgs<Point>> ResolutionChanged;

        public void Run(GameTime gameTime)
        {
            if (!Enabled) return;

            if (gameTime.TotalGameTime.TotalMilliseconds - _resolutionTick >= 50)
            {
                _resolutionTick = gameTime.TotalGameTime.TotalMilliseconds;

                if (GameService.Input.Mouse.State.LeftButton != Microsoft.Xna.Framework.Input.ButtonState.Pressed && !Resolution.Equals(GameService.Graphics.Resolution))
                {
                    var prev = Resolution;
                    Resolution = GameService.Graphics.Resolution;

                    ResolutionChanged?.Invoke(this, new(prev, Resolution));
                }

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
