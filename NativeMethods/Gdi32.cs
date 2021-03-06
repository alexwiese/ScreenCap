﻿using System;
using System.Runtime.InteropServices;

namespace ScreenCap.NativeMethods
{
    /// <summary>
    ///  Helper class containing Gdi32 API functions
    /// </summary>
    internal static class Gdi32
    {

        public const int Srccopy = 0x00CC0020; // BitBlt dwRop parameter 
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
            int nWidth, int nHeight, IntPtr hObjectSource,
            int nXSrc, int nYSrc, int dwRop);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDc, int nWidth,
            int nHeight);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDc, IntPtr hObject);
    }
}