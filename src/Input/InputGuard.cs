namespace Loupedeck.ArtRawEditorPlugin
{
    using System;
    using System.Runtime.InteropServices;

    internal static class InputGuard
    {
        // Returns a user-facing message if the OS blocks synthetic keystrokes, otherwise null.
        public static String PermissionProblem()
        {
            if (!OperatingSystem.IsMacOS())
            {
                return null;
            }

            if (!AXIsProcessTrusted())
            {
                return "Enable Logi Plugin Service in System Settings → Privacy & Security → Accessibility (remove and re-add it if already enabled).";
            }

            return IsSecureEventInputEnabled()
                ? "Secure keyboard input is active (password field or terminal), keystrokes are blocked."
                : null;
        }

        // Physically held modifiers would turn slider keys into ART shortcuts (e.g. Ctrl+S, Cmd+Q).
        public static Boolean AnyModifierPressed(Boolean ignoreShift = false) =>
            OperatingSystem.IsMacOS() ? AnyModifierPressedMac(ignoreShift) : AnyModifierPressedWindows(ignoreShift);

        // Synthetic wheel and click events go to the window under the pointer, which need not belong to the frontmost app.
        public static Boolean PointerOverProcess(Int32 processId) =>
            (OperatingSystem.IsMacOS() ? WindowOwnerAtPointerMac() : WindowOwnerAtPointerWindows()) == processId;

        private static Boolean AnyModifierPressedMac(Boolean ignoreShift)
        {
            const UInt64 shift = 0x20000, control = 0x40000, option = 0x80000, command = 0x100000;
            const Int32 hidSystemState = 1;
            return (CGEventSourceFlagsState(hidSystemState) & ((ignoreShift ? 0 : shift) | control | option | command)) != 0;
        }

        private static Boolean AnyModifierPressedWindows(Boolean ignoreShift)
        {
            const Int32 shift = 0x10, control = 0x11, alt = 0x12, leftWin = 0x5B, rightWin = 0x5C;
            foreach (var key in ignoreShift ? new[] { control, alt, leftWin, rightWin } : new[] { shift, control, alt, leftWin, rightWin })
            {
                if ((GetAsyncKeyState(key) & 0x8000) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        // The window list is ordered front to back; fully transparent windows (overlays) don't receive events.
        private static Int32 WindowOwnerAtPointerMac()
        {
            const UInt32 onScreenOnly = 1, excludeDesktopElements = 16;
            var location = CGEventCreate(IntPtr.Zero);
            if (location == IntPtr.Zero)
            {
                return -1;
            }

            var pointer = CGEventGetLocation(location);
            CFRelease(location);

            var windows = CGWindowListCopyWindowInfo(onScreenOnly | excludeDesktopElements, 0);
            if (windows == IntPtr.Zero)
            {
                return -1;
            }

            try
            {
                for (var i = 0L; i < CFArrayGetCount(windows); i++)
                {
                    var window = CFArrayGetValueAtIndex(windows, i);
                    if (CGRectMakeWithDictionaryRepresentation(CFDictionaryGetValue(window, WindowBounds.Value), out var bounds)
                        && pointer.X >= bounds.X && pointer.X < bounds.X + bounds.Width
                        && pointer.Y >= bounds.Y && pointer.Y < bounds.Y + bounds.Height
                        && Number(window, WindowAlpha.Value) > 0)
                    {
                        return (Int32)Number(window, WindowOwnerPid.Value);
                    }
                }

                return -1;
            }
            finally
            {
                CFRelease(windows);
            }
        }

        private static Double Number(IntPtr dictionary, IntPtr key)
        {
            const Int32 doubleType = 13;
            return CFNumberGetValue(CFDictionaryGetValue(dictionary, key), doubleType, out var value) ? value : -1;
        }

        private static Lazy<IntPtr> CFString(String value) => new(() => CFStringCreateWithCString(IntPtr.Zero, value, 0x08000100));

        private static readonly Lazy<IntPtr> WindowBounds = CFString("kCGWindowBounds");
        private static readonly Lazy<IntPtr> WindowAlpha = CFString("kCGWindowAlpha");
        private static readonly Lazy<IntPtr> WindowOwnerPid = CFString("kCGWindowOwnerPID");

        private static Int32 WindowOwnerAtPointerWindows()
        {
            GetCursorPos(out var pointer);
            GetWindowThreadProcessId(WindowFromPoint(pointer), out var processId);
            return (Int32)processId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CGPoint
        {
            public Double X, Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CGRect
        {
            public Double X, Y, Width, Height;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Point
        {
            public Int32 X, Y;
        }

        private const String CoreGraphics = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
        private const String CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

        [DllImport(CoreGraphics)]
        private static extern IntPtr CGEventCreate(IntPtr source);

        [DllImport(CoreGraphics)]
        private static extern CGPoint CGEventGetLocation(IntPtr evt);

        [DllImport(CoreGraphics)]
        private static extern IntPtr CGWindowListCopyWindowInfo(UInt32 option, UInt32 relativeToWindow);

        [DllImport(CoreGraphics)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern Boolean CGRectMakeWithDictionaryRepresentation(IntPtr dictionary, out CGRect rect);

        [DllImport(CoreFoundation)]
        private static extern Int64 CFArrayGetCount(IntPtr array);

        [DllImport(CoreFoundation)]
        private static extern IntPtr CFArrayGetValueAtIndex(IntPtr array, Int64 index);

        [DllImport(CoreFoundation)]
        private static extern IntPtr CFDictionaryGetValue(IntPtr dictionary, IntPtr key);

        [DllImport(CoreFoundation)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern Boolean CFNumberGetValue(IntPtr number, Int32 type, out Double value);

        [DllImport(CoreFoundation)]
        private static extern IntPtr CFStringCreateWithCString(IntPtr allocator, String value, UInt32 encoding);

        [DllImport(CoreFoundation)]
        private static extern void CFRelease(IntPtr obj);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean GetCursorPos(out Point point);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll")]
        private static extern UInt32 GetWindowThreadProcessId(IntPtr window, out UInt32 processId);

        [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern Boolean AXIsProcessTrusted();

        [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern Boolean IsSecureEventInputEnabled();

        [DllImport(CoreGraphics)]
        private static extern UInt64 CGEventSourceFlagsState(Int32 stateId);

        [DllImport("user32.dll")]
        private static extern Int16 GetAsyncKeyState(Int32 virtualKey);
    }
}
