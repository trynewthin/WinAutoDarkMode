using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WinAutoDarkMode.Core
{
    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
        ACCENT_INVALID_STATE = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public uint AccentFlags;
        public uint GradientColor;
        public uint AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }

    public static class WindowHelper
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        public static void EnableBlur(Window window, bool isDark)
        {
            var windowHelper = new WindowInteropHelper(window);
            
            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
            
            // 设置一点背景色覆盖，增加亚克力质感 (AABBGGRR)
            // 深色模式下用深灰透明，浅色模式用白色透明
            accent.GradientColor = isDark ? 0x99000000 : 0x99FFFFFF;

            var accentStructSize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
    }
}
