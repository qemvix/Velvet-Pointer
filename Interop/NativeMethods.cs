using System.Runtime.InteropServices;
namespace VelvetPointer.Interop;
internal static class NativeMethods
{
    internal const int WH_MOUSE_LL = 14, WM_MOUSEMOVE = 0x0200, WM_INPUT = 0x00FF, WM_HOTKEY = 0x0312;
    internal const uint RID_INPUT = 0x10000003, RIDEV_INPUTSINK = 0x00000100, RIM_TYPEMOUSE = 0;
    internal const uint LLMHF_INJECTED = 0x00000001;
    internal const uint MOUSEEVENTF_MOVE = 0x0001, MOUSEEVENTF_ABSOLUTE = 0x8000, MOUSEEVENTF_VIRTUALDESK = 0x4000;
    internal const uint MOD_ALT = 0x0001, MOD_CONTROL = 0x0002;
    [StructLayout(LayoutKind.Sequential)] internal struct POINT { public int x, y; }
    [StructLayout(LayoutKind.Sequential)] internal struct MSLLHOOKSTRUCT { public POINT pt; public uint mouseData, flags, time; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)] internal struct RAWINPUTDEVICE { public ushort usUsagePage, usUsage; public uint dwFlags; public IntPtr hwndTarget; }
    [StructLayout(LayoutKind.Sequential)] internal struct RAWINPUTHEADER { public uint dwType, dwSize; public IntPtr hDevice, wParam; }
    [StructLayout(LayoutKind.Sequential)] internal struct RAWMOUSE { public ushort usFlags; public uint ulButtons; public ushort usButtonFlags, usButtonData; public uint ulRawButtons; public int lLastX, lLastY; public uint ulExtraInformation; }
    [StructLayout(LayoutKind.Sequential)] internal struct RAWINPUT { public RAWINPUTHEADER header; public RAWMOUSE mouse; }
    [StructLayout(LayoutKind.Sequential)] internal struct INPUT { public uint type; public MOUSEINPUT mi; }
    [StructLayout(LayoutKind.Sequential)] internal struct MOUSEINPUT { public int dx, dy; public uint mouseData, dwFlags, time; public IntPtr dwExtraInfo; }
    internal delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", SetLastError=true)] internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc callback, IntPtr hMod, uint threadId);
    [DllImport("user32.dll")] internal static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")] internal static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", SetLastError=true)] internal static extern uint GetRawInputData(IntPtr hRawInput, uint command, IntPtr data, ref uint size, uint headerSize);
    [DllImport("user32.dll", SetLastError=true)] internal static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] devices, uint count, uint size);
    [DllImport("user32.dll", SetLastError=true)] internal static extern uint SendInput(uint count, INPUT[] inputs, int size);
    [DllImport("user32.dll")] internal static extern bool GetCursorPos(out POINT point);
    [DllImport("user32.dll")] internal static extern int GetSystemMetrics(int index);
    [DllImport("user32.dll", SetLastError=true)] internal static extern bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] internal static extern bool UnregisterHotKey(IntPtr hwnd, int id);
}
