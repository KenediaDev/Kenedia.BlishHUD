using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Kenedia.Modules.Core.Utility.WindowsUtil
{
    public class User32Dll
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        public const uint WM_COMMAND = 0x0111;
        public const uint WM_PASTE = 0x0302;

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition(IntPtr hWnd)
        {
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

        public static Point GetScreenPositionFromUi(IntPtr hWnd, POINT pos)
        {
            return !ClientToScreen(hWnd, ref pos) ? Point.Empty : new Point(pos.X, pos.Y);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public bool Matches(RECT rect)
            {
                return rect.Left == Left && rect.Top == Top && rect.Right == Right && rect.Bottom == Bottom;
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);
    }
}
