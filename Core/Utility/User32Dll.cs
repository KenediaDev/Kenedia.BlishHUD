using System;
using System.Runtime.InteropServices;

namespace Kenedia.Modules.Core.Utility.WindowsUtil
{
    public class User32Dll
    {
        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        public const uint WM_COMMAND = 0x0111;
        public const uint WM_PASTE = 0x0302;

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

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
    }
}
