namespace Loupedeck.ArtRawEditorPlugin
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    // Shift+mouse wheel at the current pointer position. The SDK's MouseWheelMove carries no modifier state,
    // so ART would scroll the tool panel instead of changing the slider. Positive ticks = increase
    // (on macOS that is wheel down for ART, verified; Windows direction untested).
    internal static class ShiftScroll
    {
        private static readonly TimeSpan ShiftEcho = TimeSpan.FromMilliseconds(300);
        private static Int64 _lastSent;

        // Our own Shift stays visible in the system modifier state for a moment after sending (plugin log).
        public static Boolean RecentlySent => System.Diagnostics.Stopwatch.GetElapsedTime(Interlocked.Read(ref _lastSent)) < ShiftEcho;

        public static void Send(Int32 ticks)
        {
            Interlocked.Exchange(ref _lastSent, System.Diagnostics.Stopwatch.GetTimestamp());
            if (OperatingSystem.IsMacOS())
            {
                SendMac(ticks);
            }
            else
            {
                SendWindows(ticks);
            }
        }

        // A private event source keeps the Shift flag on this event only; with the default source macOS
        // keeps reporting Shift as held system-wide afterwards, which blocks InputGuard.AnyModifierPressed.
        private static readonly Lazy<IntPtr> PrivateSource = new(() => CGEventSourceCreate(-1));

        // Non-continuous wheel events (classic mouse wheel): GTK on macOS then steps by the slider's
        // page increment (proportional to its range); continuous events use absolute units (gtkrange.c).
        // In that mode GTK only uses the direction, so one event is sent per tick.
        private static void SendMac(Int32 ticks)
        {
            const Int32 lineUnits = 1, hidEventTap = 0, isContinuousField = 88;
            const UInt64 shiftFlag = 0x20000;

            for (var i = 0; i < Math.Abs(ticks); i++)
            {
                var scroll = CGEventCreateScrollWheelEvent2(PrivateSource.Value, lineUnits, 1, -Math.Sign(ticks), 0, 0);
                try
                {
                    CGEventSetIntegerValueField(scroll, isContinuousField, 0);
                    CGEventSetFlags(scroll, shiftFlag);
                    CGEventPost(hidEventTap, scroll);
                }
                finally
                {
                    CFRelease(scroll);
                }
            }

            // Discrete scroll events with the Shift flag leave Shift set in the system-wide modifier state
            // (verified in the plugin log); a flags-changed event without flags clears it again.
            const Int32 flagsChanged = 12;
            var clear = CGEventCreate(PrivateSource.Value);
            try
            {
                CGEventSetType(clear, flagsChanged);
                CGEventSetFlags(clear, 0);
                CGEventPost(hidEventTap, clear);
            }
            finally
            {
                CFRelease(clear);
            }
        }

        private static void SendWindows(Int32 ticks)
        {
            const UInt32 keyboard = 1, mouse = 0, keyUp = 0x2, wheel = 0x0800;
            const UInt16 shift = 0x10;
            const Int32 wheelDelta = 120;

            var inputs = new[]
            {
                new Input { Type = keyboard, Keyboard = new KeyboardInput { VirtualKey = shift } },
                new Input { Type = mouse, Mouse = new MouseInput { Data = ticks * wheelDelta, Flags = wheel } },
                new Input { Type = keyboard, Keyboard = new KeyboardInput { VirtualKey = shift, Flags = keyUp } },
            };
            SendInput((UInt32)inputs.Length, inputs, Marshal.SizeOf<Input>());
        }

        private const String CoreGraphics = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

        [DllImport(CoreGraphics)]
        private static extern IntPtr CGEventSourceCreate(Int32 stateId);

        [DllImport(CoreGraphics)]
        private static extern IntPtr CGEventCreateScrollWheelEvent2(IntPtr source, Int32 units, UInt32 wheelCount, Int32 wheel1, Int32 wheel2, Int32 wheel3);

        [DllImport(CoreGraphics)]
        private static extern IntPtr CGEventCreate(IntPtr source);

        [DllImport(CoreGraphics)]
        private static extern void CGEventSetType(IntPtr evt, Int32 type);

        [DllImport(CoreGraphics)]
        private static extern void CGEventSetIntegerValueField(IntPtr evt, Int32 field, Int64 value);

        [DllImport(CoreGraphics)]
        private static extern void CGEventSetFlags(IntPtr evt, UInt64 flags);

        [DllImport(CoreGraphics)]
        private static extern void CGEventPost(Int32 tap, IntPtr evt);

        [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        private static extern void CFRelease(IntPtr obj);

        [DllImport("user32.dll")]
        private static extern UInt32 SendInput(UInt32 count, Input[] inputs, Int32 size);

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseInput
        {
            public Int32 Dx, Dy, Data;
            public UInt32 Flags, Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardInput
        {
            public UInt16 VirtualKey, ScanCode;
            public UInt32 Flags, Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Input
        {
            [FieldOffset(0)] public UInt32 Type;
            [FieldOffset(8)] public MouseInput Mouse;
            [FieldOffset(8)] public KeyboardInput Keyboard;
        }
    }
}
