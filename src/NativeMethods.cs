using System;
using System.Runtime.InteropServices;

namespace RMDarkHelper
{
    internal static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MAGTRANSFORM
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
            public float[] v;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MAGCOLOREFFECT
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
            public float[] transform;
        }

        public delegate bool EnumWindowsProc(
            IntPtr hWnd,
            IntPtr lParam);

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagInitialize();

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagUninitialize();

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagSetWindowSource(
            IntPtr hwnd,
            RECT rect);

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagSetWindowTransform(
            IntPtr hwnd,
            ref MAGTRANSFORM pTransform);

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagSetColorEffect(
            IntPtr hwnd,
            ref MAGCOLOREFFECT pEffect);

        [DllImport("Magnification.dll", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool MagSetWindowFilterList(
            IntPtr hwnd,
            uint dwFilterMode,
            int count,
            [In] IntPtr[] pHWND);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(
            EnumWindowsProc lpEnumFunc,
            IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(
            IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(
            IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(
            IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(
            IntPtr hWnd,
            out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(
            IntPtr hWnd,
            out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateWindowEx(
            int dwExStyle,
            string lpClassName,
            string lpWindowName,
            int dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(
            IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetLayeredWindowAttributes(
            IntPtr hwnd,
            uint crKey,
            byte bAlpha,
            uint dwFlags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        public static extern IntPtr GetWindowLongPtr(
            IntPtr hWnd,
            int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr(
            IntPtr hWnd,
            int nIndex,
            IntPtr dwNewLong);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(
            IntPtr hWnd,
            int id,
            uint fsModifiers,
            uint vk);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(
            IntPtr hWnd,
            int id);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetProcessDPIAware();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetProcessDpiAwarenessContext(
            IntPtr value);

        public static MAGTRANSFORM IdentityTransform()
        {
            MAGTRANSFORM transform = new MAGTRANSFORM();

            transform.v = new float[]
            {
                1f, 0f, 0f,
                0f, 1f, 0f,
                0f, 0f, 1f
            };

            return transform;
        }

        public static MAGCOLOREFFECT CreateColorEffect(
            params float[] values)
        {
            if (values == null || values.Length != 25)
                throw new ArgumentException(
                    "Color effect requires exactly 25 values.");

            MAGCOLOREFFECT effect = new MAGCOLOREFFECT();
            effect.transform = values;

            return effect;
        }
    }
}
