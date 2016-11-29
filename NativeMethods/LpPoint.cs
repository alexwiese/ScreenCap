using System.Runtime.InteropServices;

namespace ScreenCap.NativeMethods
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LpPoint
    {
        public int X;
        public int Y;
    }
}