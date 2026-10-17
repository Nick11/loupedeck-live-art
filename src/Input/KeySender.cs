namespace Loupedeck.ArtRawEditorPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    // Sends keystrokes to ART on one worker thread, only while ART is in front and no modifier is held.
    // Dial steps are accumulated and clamped instead of queued, so fast turns never lag behind.
    internal sealed class KeySender : IDisposable
    {
        private const Int32 KeyDelayMs = 4;
        private const Int32 MaxPendingSteps = 5;
        private const Int32 MaxPendingWheelTicks = 1000;
        private static readonly TimeSpan HoldTimeout = TimeSpan.FromMilliseconds(150);
        private static readonly TimeSpan MaxCommandAge = TimeSpan.FromMilliseconds(500);

        private readonly ArtRawEditorPlugin _plugin;
        private readonly Queue<(Action Send, Boolean Pointer, Int64 Timestamp)> _commands = new();
        private readonly Thread _worker;
        private VirtualKeyCode? _stepKey;
        private Int32 _steps;
        private Int32 _wheelTicks;
        private VirtualKeyCode? _held;
        private Boolean _stopping;
        private String _blocked;

        public KeySender(ArtRawEditorPlugin plugin)
        {
            this._plugin = plugin;
            this._worker = new Thread(this.Run) { IsBackground = true, Name = "ART KeySender" };
            this._worker.Start();
        }

        public void EnqueueSteps(ArtSlider slider, Int32 steps)
        {
            lock (this._commands)
            {
                if (this._stepKey != slider.Key)
                {
                    this._stepKey = slider.Key;
                    this._steps = 0;
                }

                this._steps = Math.Clamp(this._steps + steps, -MaxPendingSteps, MaxPendingSteps);
                Monitor.Pulse(this._commands);
            }
        }

        // Shift+mouse wheel at the pointer: ART changes the slider under it (guiutils.cc, MyHScale).
        public void EnqueueWheel(Int32 ticks)
        {
            lock (this._commands)
            {
                this._wheelTicks = Math.Clamp(this._wheelTicks + ticks, -MaxPendingWheelTicks, MaxPendingWheelTicks);
                Monitor.Pulse(this._commands);
            }
        }

        // Double-click on an ART slider resets it (adjuster.cc).
        public void EnqueueDoubleClick() => this.Enqueue(() =>
        {
            this.Release();
            this._plugin.NativeApi.GetNativeMethods().MouseDoubleClick(MouseButton.Left);
        }, pointer: true);

        public void EnqueueReset(ArtSlider slider) => this.Enqueue(() =>
        {
            this.Hold(slider.Key);
            this.Tap(VirtualKeyCode.Multiply);
        }, slider.Key);

        public void EnqueueShortcut(ArtShortcut shortcut) => this.Enqueue(() =>
        {
            this.Release();
            this.SendShortcut(shortcut.Key, shortcut.Modifiers);
        });

        // SendKeyboardShortcut maps keys positionally: on QWERTZ, KeyZ arrives as 'y' (= Redo in ART).
        // SendKeyboardEvent takes the active keyboard layout, like the hold-key path, but Option shortcuts
        // (ART tool tabs) stopped working with it; they only use keys that are identical on QWERTZ.
        private void SendShortcut(VirtualKeyCode key, ModifierKey modifiers)
        {
            if ((modifiers & (ModifierKey.Alt | ModifierKey.Option)) != 0)
            {
                this._plugin.ClientApplication.SendKeyboardShortcut(key, modifiers);
                return;
            }

            var keyboard = this._plugin.NativeApi.GetNativeKeyboard();
            var layout = keyboard.GetActiveKeyboardLayout();
            keyboard.SendKeyboardEvent(key, modifiers, layout, true);
            Thread.Sleep(KeyDelayMs);
            keyboard.SendKeyboardEvent(key, modifiers, layout, false);
        }

        public void Dispose()
        {
            lock (this._commands)
            {
                this._stopping = true;
                Monitor.Pulse(this._commands);
            }

            this._worker.Join(TimeSpan.FromSeconds(2));
        }

        private void Enqueue(Action send, VirtualKeyCode? cancelsStepsOf = null, Boolean pointer = false)
        {
            lock (this._commands)
            {
                if (cancelsStepsOf != null && cancelsStepsOf == this._stepKey)
                {
                    this._steps = 0;
                }

                this._commands.Enqueue((send, pointer, Stopwatch.GetTimestamp()));
                Monitor.Pulse(this._commands);
            }
        }

        private void Run()
        {
            this.TrySend(this.ReleaseSliderKeys);

            while (true)
            {
                Action send;
                lock (this._commands)
                {
                    if (this._commands.Count == 0 && this._steps == 0 && this._wheelTicks == 0 && !this._stopping)
                    {
                        Monitor.Wait(this._commands, HoldTimeout);
                    }

                    if (this._stopping)
                    {
                        break;
                    }

                    send = this.NextAction();
                }

                this.TrySend(send);
            }

            this.TrySend(this.Release);
            this.TrySend(this.ReleaseSliderKeys);
        }

        // Called under lock: a queued command, else one dial step, else pending wheel ticks, else release the held key (idle).
        private Action NextAction()
        {
            if (this._commands.TryDequeue(out var command))
            {
                return () =>
                {
                    if (Stopwatch.GetElapsedTime(command.Timestamp) <= MaxCommandAge && this.CanSend(command.Pointer))
                    {
                        command.Send();
                    }
                };
            }

            if (this._steps != 0 && this._stepKey is { } key)
            {
                var direction = Math.Sign(this._steps);
                this._steps -= direction;
                return () =>
                {
                    if (this.CanSend())
                    {
                        this.Hold(key);
                        this.Tap(direction > 0 ? VirtualKeyCode.Add : VirtualKeyCode.Subtract);
                    }
                    else
                    {
                        this.Release();
                    }
                };
            }

            if (this._wheelTicks != 0)
            {
                var ticks = this._wheelTicks;
                this._wheelTicks = 0;
                return () =>
                {
                    if (this.CanSend(pointer: true))
                    {
                        this.Release();
                        ShiftScroll.Send(ticks);
                    }
                };
            }

            return this.Release;
        }

        // Pointer input (wheel, click) goes to the window under the pointer instead of the frontmost app.
        // Only pointer input may ignore our own Shift echo; a physically held Shift must still block keys (Shift+E etc.).
        private Boolean CanSend(Boolean pointer = false)
        {
            var problem = InputGuard.PermissionProblem();
            this._plugin.ReportInputProblem(problem);

            var blocked = problem != null ? "permission"
                : !this._plugin.ClientApplication.IsActive() ? "ART not in front"
                : InputGuard.AnyModifierPressed(ignoreShift: pointer && ShiftScroll.RecentlySent) ? "modifier key held"
                : pointer && !InputGuard.PointerOverProcess(this._plugin.NativeApi.GetNativeMethods().GetActiveProcessId()) ? "pointer not over ART"
                : null;
            if (blocked != this._blocked)
            {
                PluginLog.Info(blocked == null ? "Sending to ART resumed" : $"Not sending to ART: {blocked}");
                this._blocked = blocked;
            }

            return blocked == null;
        }

        private void Hold(VirtualKeyCode key)
        {
            if (this._held != key)
            {
                this.Release();
                this.Send(key, true);
                this._held = key;
            }
        }

        private void Release()
        {
            if (this._held is { } key)
            {
                this.Send(key, false);
                this._held = null;
            }
        }

        private void Tap(VirtualKeyCode key)
        {
            this.Send(key, true);
            this.Send(key, false);
        }

        private void Send(VirtualKeyCode key, Boolean pressed)
        {
            var keyboard = this._plugin.NativeApi.GetNativeKeyboard();
            keyboard.SendKeyboardEvent(key, ModifierKey.None, keyboard.GetActiveKeyboardLayout(), pressed);
            Thread.Sleep(KeyDelayMs);
        }

        // A crash between key-down and key-up would leave an ART slider armed.
        private void ReleaseSliderKeys()
        {
            foreach (var slider in ArtSliders.All)
            {
                this.Send(slider.Key, false);
            }
        }

        private void TrySend(Action send)
        {
            try
            {
                send();
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Failed to send keystrokes to ART");
            }
        }
    }
}
