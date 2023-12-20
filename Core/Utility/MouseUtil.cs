using Blish_HUD;
using Kenedia.Modules.Core.Utility.WinApi;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text;
using System.Diagnostics;

namespace Kenedia.Modules.Core.Utility
{
    internal class MouseUtil
    {
        public enum MouseInputType
        {
            System = 0,
            Background = 1,
        }

        public enum MouseButton
        {
            LEFT,
            RIGHT,
            MIDDLE,
            XBUTTON
        }

        [Flags]
        internal enum MouseEventF : uint
        {
            ABSOLUTE = 0x8000,
            HWHEEL = 0x01000,
            MOVE = 0x0001,
            MOVE_NOCOALESCE = 0x2000,
            LEFTDOWN = 0x0002,
            LEFTUP = 0x0004,
            RIGHTDOWN = 0x0008,
            RIGHTUP = 0x0010,
            MIDDLEDOWN = 0x0020,
            MIDDLEUP = 0x0040,
            VIRTUALDESK = 0x4000,
            WHEEL = 0x0800,
            XDOWN = 0x0080,
            XUP = 0x0100
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MouseInput
        {
            internal int _dx;
            internal int _dy;
            internal int _mouseData;
            internal MouseEventF _dwFlags;
            internal uint _time;
            internal UIntPtr _dwExtraInfo;
        }

        private const uint WM_MOUSEWHEEL = 0x020A;
        private const uint WM_MOUSEHWHEEL = 0x020E;
        private const int WHEEL_DELTA = 120;
        private const uint WM_MOUSEMOVE = 0x0200;

#pragma warning disable IDE1006 // Naming Styles
        private static readonly Dictionary<MouseButton, MouseEventF> ButtonPress = new()
        {
            { MouseButton.LEFT, MouseEventF.LEFTDOWN },
            { MouseButton.RIGHT, MouseEventF.RIGHTDOWN },
            { MouseButton.MIDDLE, MouseEventF.MIDDLEDOWN },
            { MouseButton.XBUTTON, MouseEventF.XDOWN }
        };

        private static readonly Dictionary<MouseButton, MouseEventF> ButtonRelease = new()
        {
            { MouseButton.LEFT, MouseEventF.LEFTUP },
            { MouseButton.RIGHT, MouseEventF.RIGHTUP },
            { MouseButton.MIDDLE, MouseEventF.MIDDLEUP },
            { MouseButton.XBUTTON, MouseEventF.XUP }
        };

        private static readonly Dictionary<MouseButton, short> VirtualButtonShort = new()
        {
            { MouseButton.LEFT, 0x01 },
            { MouseButton.RIGHT, 0x02 },
            { MouseButton.MIDDLE, 0x04 },
            { MouseButton.XBUTTON, 0x05 }
        };

        private static readonly Dictionary<MouseButton, uint> WM_BUTTONDOWN = new()
        {
            { MouseButton.LEFT, 0x0201 },
            { MouseButton.RIGHT, 0x0204 },
            { MouseButton.MIDDLE, 0x0207 },
            { MouseButton.XBUTTON, 0x020B }
        };

        private static readonly Dictionary<MouseButton, uint> WM_BUTTONUP = new()
        {
            { MouseButton.LEFT, 0x0202 },
            { MouseButton.RIGHT, 0x0205 },
            { MouseButton.MIDDLE, 0x0208 },
            { MouseButton.XBUTTON, 0x020C }
        };

        private static readonly Dictionary<MouseButton, uint> WM_BUTTONDBLCLK = new()
        {
            { MouseButton.LEFT, 0x0203 },
            { MouseButton.RIGHT, 0x0206 },
            { MouseButton.MIDDLE, 0x0209 },
            { MouseButton.XBUTTON, 0x020D }
        };
#pragma warning restore IDE1006 // Naming Styles

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [DllImport("user32.Dll", SetLastError = true)]
        private static extern long SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] WinApi.Input[] pInputs, int cbSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, uint wParam, int lParam); // sends a message asynchronously.

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// Presses a mouse button.
        /// </summary>
        /// <param name="button">The mouse button to press.</param>
        /// <param name="xPos">The X coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current X coordinate.</param>
        /// <param name="yPos">The Y coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current Y coordinate.</param>
        /// <param name="sendToSystem">Set if button message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Press(MouseButton button, int xPos = -1, int yPos = -1, bool sendToSystem = false)
        {
            if (xPos == -1 || yPos == -1)
            {
                var pos = GetPosition();
                xPos = pos.X;
                yPos = pos.Y;
            }

            if (!GameService.GameIntegration.Gw2Instance.Gw2IsRunning || sendToSystem)
            {
                var nInputs = new[]
                {
                    new WinApi.Input
                    {
                        _type = InputType.MOUSE,
                        _u = new InputUnion
                        {
                            _mi = new MouseInput
                            {
                                _dx = xPos,
                                _dy = yPos,
                                _mouseData = 0,
                                _dwFlags = ButtonPress[button],
                                _time = 0,
                            }
                        }
                    }
                };
                SendInput((uint)nInputs.Length, nInputs, WinApi.Input.Size);
            }
            else
            {
                uint wParam = (uint)VirtualButtonShort[button];
                int lParam = xPos | (yPos << 16);
                PostMessage(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, WM_BUTTONDOWN[button], wParam, lParam);
            }
        }

        /// <summary>
        /// Releases a mouse button.
        /// </summary>
        /// <param name="button">The mouse button to release.</param>
        /// <param name="xPos">The X coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current X coordinate.</param>
        /// <param name="yPos">The Y coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current Y coordinate.</param>
        /// <param name="sendToSystem">Set if button message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Release(MouseButton button, int xPos = -1, int yPos = -1, bool sendToSystem = false)
        {
            if (xPos == -1 || yPos == -1)
            {
                var pos = GetPosition();
                xPos = pos.X;
                yPos = pos.Y;
            }

            if (!GameService.GameIntegration.Gw2Instance.Gw2IsRunning || sendToSystem)
            {
                var nInputs = new[]
                {
                    new WinApi.Input
                    {
                        _type = InputType.MOUSE,
                        _u = new InputUnion
                        {
                            _mi = new MouseInput
                            {
                                _dx = xPos,
                                _dy = yPos,
                                _mouseData = 0,
                                _dwFlags = ButtonRelease[button],
                                _time = 0
                            }
                        }
                    }
                };
                SendInput((uint)nInputs.Length, nInputs, WinApi.Input.Size);
            }
            else
            {
                uint wParam = (uint)VirtualButtonShort[button];
                int lParam = xPos | (yPos << 16);
                PostMessage(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, WM_BUTTONUP[button], wParam, lParam);
            }
        }

        /// <summary>
        /// Rotates the mouse wheel.
        /// </summary>
        /// <param name="wheelDistance">Distance of movement by multiples or divisions of 120 (WHEEL_DELTA). A positive value indicates the wheel to rotate forward, away from the user; a negative value indicates the wheel to rotate backward, toward the user.</param>
        /// <param name="horizontalWheel">Indicates the wheel to rotate horizontally.</param>
        /// <param name="xPos">The X coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current X coordinate.</param>
        /// <param name="yPos">The Y coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current Y coordinate.</param>
        /// <param name="sendToSystem">Set if button message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void RotateWheel(int wheelDistance, bool horizontalWheel = false, int xPos = -1, int yPos = -1, bool sendToSystem = false)
        {
            wheelDistance %= WHEEL_DELTA;
            if (wheelDistance == 0) return;

            if (xPos == -1 || yPos == -1)
            {
                var pos = GetPosition();
                xPos = pos.X;
                yPos = pos.Y;
            }

            if (!GameService.GameIntegration.Gw2Instance.Gw2IsRunning || sendToSystem)
            {
                var nInputs = new[]
                {
                    new WinApi.Input
                    {
                        _type = InputType.MOUSE,
                        _u = new InputUnion
                        {
                            _mi = new MouseInput
                            {
                                _dx = xPos,
                                _dy = yPos,
                                _mouseData = wheelDistance,
                                _dwFlags = horizontalWheel ? MouseEventF.HWHEEL : MouseEventF.WHEEL,
                                _time = 0
                            }
                        }
                    }
                };
                SendInput((uint)nInputs.Length, nInputs, WinApi.Input.Size);
            }
            else
            {
                uint wParam = (uint)(0 | (wheelDistance << 16));
                int lParam = xPos | (yPos << 16);
                PostMessage(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, horizontalWheel ? WM_MOUSEHWHEEL : WM_MOUSEWHEEL, wParam, lParam);
            }
        }

        /// <summary>
        /// Sets the cursors absolute screen position.
        /// </summary>
        /// <param name="xPos">The X coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current X coordinate.</param>
        /// <param name="yPos">The Y coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current Y coordinate.</param>
        /// <param name="sendToSystem">Set if button message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void SetPosition(int xPos, int yPos, bool sendToSystem = false)
        {
            if (!GameService.GameIntegration.Gw2Instance.Gw2IsRunning || sendToSystem)
            {
                SetCursorPos(xPos, yPos);
            }
            else
            {
                int lParam = xPos | (yPos << 16);
                
                PostMessage(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, WM_MOUSEMOVE, 0, lParam);
            }
        }

        /// <summary>
        /// Sets the cursors absolute screen position.
        /// </summary>
        /// <param name="point">The position to set the cursor to.</param>
        /// <param name="sendToSystem">Set if button message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void SetPosition(Microsoft.Xna.Framework.Point point, bool sendToSystem = false)
        {
            SetPosition(point.X, point.Y, sendToSystem);
        }

        /// <summary>
        /// Gets the cursors screen position relative to the window.
        /// </summary>
        public static Point GetPosition()
        {
            var hWnd = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;

            if (!GetCursorPos(out var pos)
                || !ScreenToClient(hWnd, ref pos)
                || !GetWindowRect(hWnd, out var wndBounds)
                || !GetClientRect(hWnd, out var clientBounds))
            {
                return Point.Empty;
            }

            // Border thickness
            int widthOffset = wndBounds.Right - wndBounds.Left - (clientBounds.Right - clientBounds.Left);
            // Titlebar height + Border thickness
            int heightOffset = wndBounds.Bottom - wndBounds.Top - (clientBounds.Bottom - clientBounds.Top);
            pos.X -= wndBounds.Left + widthOffset;
            pos.Y -= wndBounds.Top + heightOffset;

            return !ClientToScreen(hWnd, ref pos) ? Point.Empty : new Point(pos.X, pos.Y);
        }

        /// <summary>
        /// Presses and immediately releases a mouse button ONCE.
        /// </summary>
        /// <param name="button">The mouse button to click.</param>
        /// <param name="xPos">The X coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current X coordinate.</param>
        /// <param name="yPos">The Y coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current Y coordinate.</param>
        /// <param name="sendToSystem">Set if button message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void Click(MouseButton button, int xPos = -1, int yPos = -1, bool sendToSystem = false)
        {
            Press(button, xPos, yPos, sendToSystem);
            Release(button, xPos, yPos, sendToSystem);
        }

        public static void Click(MouseButton button, Microsoft.Xna.Framework.Point point, bool sendToSystem)
        {
            Click(button, point.X, point.Y, sendToSystem);
        }

        /// <summary>
        /// Performs a double click of a mouse button.
        /// </summary>
        /// <param name="button">The mouse button to click.</param>
        /// <param name="xPos">The X coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current X coordinate.</param>
        /// <param name="yPos">The Y coodinate where this action takes place. Relative to the game client window if sendToSystem is not set. Default: current Y coordinate.</param>
        /// <param name="sendToSystem">Set if button message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static void DoubleClick(MouseButton button, int xPos = -1, int yPos = -1, bool sendToSystem = false)
        {
            if (!GameService.GameIntegration.Gw2Instance.Gw2IsRunning || sendToSystem)
            {
                for (int i = 0; i <= 1; i++)
                {
                    Press(button, xPos, yPos, sendToSystem);
                    Release(button, xPos, yPos, sendToSystem);
                }
            }
            else
            {
                if (xPos == -1 || yPos == -1)
                {
                    var pos = GetPosition();
                    xPos = pos.X;
                    yPos = pos.Y;
                }

                uint wParam = (uint)VirtualButtonShort[button];
                int lParam = xPos | (yPos << 16);

                for (int i = 0; i <= 1; i++)
                {
                    Press(button, xPos, yPos);
                    Release(button, xPos, yPos);
                }

                PostMessage(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, WM_BUTTONUP[button], wParam, lParam);
            }
        }

        public static void DoubleClick(MouseButton button, Microsoft.Xna.Framework.Point point, bool sendToSystem)
        {
            DoubleClick(button, point.X, point.Y, sendToSystem);
        }
    }
}
