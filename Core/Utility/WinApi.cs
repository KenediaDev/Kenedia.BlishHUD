using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kenedia.Modules.Core.Utility.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Input
    {
        internal InputType _type;
        internal InputUnion _u;
        internal static int Size => Marshal.SizeOf(typeof(Input));
    }

    internal enum InputType : uint
    {
        MOUSE = 0,
        KEYBOARD = 1,
        HARDWARE = 2
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct InputUnion
    {
        [FieldOffset(0)]
        internal MouseUtil.MouseInput _mi;
        [FieldOffset(0)]
        internal KeyboardUtil.KeybdInput _ki;
        [FieldOffset(0)]
        internal HardwareInput _hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HardwareInput
    {
        internal int _uMsg;
        internal short _wParamL;
        internal short _wParamH;
    }
}
