using Blish_HUD;
using Kenedia.Modules.Core.Utility.WinApi;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Kenedia.Modules.Core.Utility
{
    public static class KeyboardUtil
    {
        const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_CHAR = 0x0102;
        private const uint WM_PASTE = 0x0302;

        private const uint MAPVK_VK_TO_VSC = 0x00;
        private const uint MAPVK_VSC_TO_VK = 0x01;
        private const uint MAPVK_VK_TO_CHAR = 0x02;
        private const uint MAPVK_VSC_TO_VK_EX = 0x03;
        private const uint MAPVK_VK_TO_VSC_EX = 0x04;

        private const uint KEY_PRESSED = 0x8000;

        private const uint VK_LSHIFT = 0xA0;
        private const uint VK_RSHIFT = 0xA1;

        private const uint VK_LCONTROL = 0xA2;
        private const uint VK_RCONTROL = 0xA3;

        private const uint VK_CONTROL = 0x11;
        private const uint VK_SHIFT = 0x10;

        [Flags]
        internal enum KeyEventF : uint
        {
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            SCANCODE = 0x0008,
            UNICODE = 0x0004
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KeybdInput
        {
            internal short _wVk;
            internal short _wScan;
            internal KeyEventF _dwFlags;
            internal int _time;
            internal UIntPtr _dwExtraInfo;
        }

        private static readonly List<int> s_extendedKeys = new() {
            0x2D, 0x24, 0x22,
            0x2E, 0x23, 0x21,
            0xA5, 0xA1, 0xA3,
            0x26, 0x28, 0x25,
            0x27, 0x90, 0x2A
        };

        [DllImport("USER32.dll")]
        private static extern short GetKeyState(uint vk);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        /// <summary>
        /// Inserts inputs into the input stream.
        /// </summary>
        /// <returns>How many inputs were successfully inserted into the input stream.</returns>
        /// <remarks>If blocked by UIPI neither the return value nor <seealso cref="GetLastError"/> will indicate that it was blocked.</remarks>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] WinApi.Input[] pInputs, int cbSize);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SendMessage(IntPtr hWnd, uint msg, uint wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, uint wParam, int lParam); // sends a message asynchronously.

        /// <summary>
        /// Presses a key.
        /// </summary>
        /// <param name="keyCode">Virtual key code of the key to press.</param>
        /// <param name="sendToSystem">Set if key message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static bool Press(int keyCode, bool sendToSystem = true)
        {
            return EmulateInput(keyCode, true, sendToSystem);
        }

        /// <summary>
        /// Releases a key.
        /// </summary>
        /// <param name="keyCode">Virtual key code of the key to release.</param>
        /// <param name="sendToSystem">Set if key message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static bool Release(int keyCode, bool sendToSystem = true)
        {
            return EmulateInput(keyCode, false, sendToSystem);
        }

        private static bool EmulateInput(int keyCode, bool pressed, bool sendToSystem = false)
        {
            var waitTil = DateTime.UtcNow.AddMilliseconds(500);
            while (DateTime.UtcNow < waitTil)
            {
                if (sendToSystem)
                {
                    if (SendInput(keyCode, pressed))
                    {
                        return true;
                    }

                    continue;
                }

                if (PostMessage(keyCode, pressed))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool PostMessage(int keyCode, bool pressed)
        {
            uint vkCode = (uint)keyCode;
            var lParam = new ExtraKeyInfo
            {
                ScanCode = (char)MapVirtualKey(vkCode, MAPVK_VK_TO_VSC)
            };

            if (s_extendedKeys.Contains(keyCode))
            {
                lParam.ExtendedKey = 1;
            }

            uint msg = WM_KEYDOWN;

            if (!pressed)
            {
                msg = WM_KEYUP;
                lParam.RepeatCount = 1;
                lParam.PrevKeyState = 1;
                lParam.TransitionState = 1;
            }

            return PostMessage(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, msg, vkCode, lParam.GetInt());
        }

        private static bool SendInput(int keyCode, bool pressed)
        {
            WinApi.Input[] nInputs;

            var dwFlags = pressed ? default : KeyEventF.KEYUP;
            nInputs = s_extendedKeys.Contains(keyCode)
                ? (new[]
                {
                        new WinApi.Input
                        {
                            _type = InputType.KEYBOARD,
                            _u = new InputUnion
                            {
                                _ki = new KeybdInput
                                {
                                    _wScan = 224,
                                    _wVk = 0,
                                    _dwFlags = 0
                                }
                            }
                        },
                        new WinApi.Input
                        {
                            _type = InputType.KEYBOARD,
                            _u = new InputUnion
                            {
                                _ki = new KeybdInput
                                {
                                    _wScan   = (short)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC),
                                    _wVk     = (short)keyCode,
                                    _dwFlags = KeyEventF.EXTENDEDKEY | dwFlags
                                }
                            }
                        }
                    })
                : (new[]
                {
                        new WinApi.Input
                        {
                            _type = InputType.KEYBOARD,
                            _u = new InputUnion
                            {
                                _ki = new KeybdInput
                                {
                                    _wScan   = (short)MapVirtualKey((uint)keyCode, MAPVK_VK_TO_VSC),
                                    _wVk     = (short)keyCode,
                                    _dwFlags = dwFlags
                                }
                            }
                        }
                    });
            return SendInput((uint)nInputs.Length, nInputs, WinApi.Input.Size) > 0;
        }
        public static bool Paste(bool sendToSystem = true)
        {
            if (!Press(162, sendToSystem))
            { // LControl
                return false;
            }

            if (!Stroke(86, sendToSystem))
            { // V
                return false;
            }

            Thread.Sleep(50);
            return Release(162, sendToSystem); // LControl
        }

        public static bool Clear(bool sendToSystem = true)
        {
            if (!SelectAll(sendToSystem))
            {
                return false;
            }

            return Stroke(46, sendToSystem); // Del; 
        }

        public static bool SelectAll(bool sendToSystem = true)
        {
            if (!Press(162, sendToSystem))
            { // LControl
                return false;
            }

            if (!Stroke(65, sendToSystem))
            { // A
                return false;
            }

            Thread.Sleep(50);
            return Release(162, sendToSystem); // LControl
        }

        /// <summary>
        /// Performs a keystroke in which a key is pressed and immediately released once.
        /// </summary>
        /// <param name="keyCode">Virtual key code of the key to stroke.</param>
        /// <param name="sendToSystem">Set if key message (or a combination of such) cannot be correctly interpreted by the game client.</param>
        public static bool Stroke(int keyCode, bool sendToSystem = true)
        {
            return Press(keyCode, sendToSystem) && Release(keyCode, sendToSystem);
        }

        /// <summary>
        /// Checks if the specified key is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if being pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsPressed(uint keyCode)
        {
            return Convert.ToBoolean(GetKeyState(keyCode) & KEY_PRESSED);
        }

        /// <summary>
        /// Checks if the left Control key is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if LCtrl is being pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsLCtrlPressed()
        {
            return Convert.ToBoolean(GetKeyState(VK_LCONTROL) & KEY_PRESSED);
        }

        /// <summary>
        /// Checks if the right Control key is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if RCtrl is being pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsRCtrlPressed()
        {
            return Convert.ToBoolean(GetKeyState(VK_RCONTROL) & KEY_PRESSED);
        }

        /// <summary>
        /// Checks if the left Shift key is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if LShift is being pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsLShiftPressed()
        {
            return Convert.ToBoolean(GetKeyState(VK_LSHIFT) & KEY_PRESSED);
        }

        /// <summary>
        /// Checks if the right Shift key is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if RShift is being pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsRShiftPressed()
        {
            return Convert.ToBoolean(GetKeyState(VK_RSHIFT) & KEY_PRESSED);
        }

        /// <summary>
        /// Checks if any Control key (left or right) is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if ctrl is being pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsCtrlPressed()
        {
            return Convert.ToBoolean(GetKeyState(VK_CONTROL) & KEY_PRESSED);
        }

        /// <summary>
        /// Checks if any Shift key (left or right) is being pressed.
        /// </summary>
        /// <returns><see langword="True"/> if shift is pressed; otherwise <see langword="false"/>.</returns>
        public static bool IsShiftPressed()
        {
            return Convert.ToBoolean(GetKeyState(VK_SHIFT) & KEY_PRESSED);
        }

        private class ExtraKeyInfo
        {
            public ushort RepeatCount;
            public char ScanCode;
            public ushort ExtendedKey, PrevKeyState, TransitionState;

            public int GetInt()
            {
                return RepeatCount | (ScanCode << 16) | (ExtendedKey << 24) |
                       (PrevKeyState << 30) | (TransitionState << 31);
            }
        }
    }
}
