namespace Loupedeck.ArtRawEditorPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // https://artraweditor.github.io/Shortcuts
    // Short (≤ 8 chars) is shown on the device and must stay unambiguous.
    internal sealed record ArtShortcut(String Id, String Group, String Label, String Short, VirtualKeyCode Key, ModifierKey Modifiers);

    internal static class ArtShortcuts
    {
        // ART uses Ctrl on macOS too (Cmd is ignored).
        private static readonly ModifierKey Primary = ModifierKey.Control;
        private static readonly ModifierKey Alt = OperatingSystem.IsMacOS() ? ModifierKey.Option : ModifierKey.Alt;

        public static readonly IReadOnlyList<ArtShortcut> All = new[]
        {
            new ArtShortcut("history.undo", "History", "Undo", "Undo", VirtualKeyCode.KeyZ, Primary),
            new ArtShortcut("history.redo", "History", "Redo", "Redo", VirtualKeyCode.KeyZ, Primary | ModifierKey.Shift),
            new ArtShortcut("history.snapshot", "History", "New Snapshot", "Snapshot", VirtualKeyCode.KeyS, Alt),

            new ArtShortcut("view.beforeafter", "View", "Before/After", "Bef/Aft", VirtualKeyCode.KeyA, ModifierKey.Shift),
            new ArtShortcut("view.zoom100", "View", "Zoom 100%", "Zoom 1:1", VirtualKeyCode.KeyZ, ModifierKey.None),
            new ArtShortcut("view.zoomin", "View", "Zoom In", "Zoom +", VirtualKeyCode.Add, ModifierKey.None),
            new ArtShortcut("view.zoomout", "View", "Zoom Out", "Zoom −", VirtualKeyCode.Subtract, ModifierKey.None),
            new ArtShortcut("view.fit", "View", "Fit to Screen", "Zoom Fit", VirtualKeyCode.KeyF, ModifierKey.None),
            new ArtShortcut("view.hidepanels", "View", "Hide Panels", "Panels", VirtualKeyCode.KeyM, ModifierKey.None),
            new ArtShortcut("view.grid", "View", "Grid", "Grid", VirtualKeyCode.KeyG, Primary),
            // ART: '<' / '>' (editorpanel.cc). Sent as the ISO key next to left Shift (Oem102): '<' and Shift '>' on
            // German/Swiss layouts; keyboards without that key (US ANSI) can't trigger them this way.
            new ArtShortcut("view.clipshadows", "View", "Clipped Shadows", "Clip Blk", VirtualKeyCode.Oem102, ModifierKey.None),
            new ArtShortcut("view.cliphighlights", "View", "Clipped Highlights", "Clip Wht", VirtualKeyCode.Oem102, ModifierKey.Shift),
            new ArtShortcut("view.focusmask", "View", "Focus Mask", "Focus", VirtualKeyCode.KeyF, ModifierKey.Shift),
            // Ctrl+M is handled by the local editing masks panel only while it has keyboard focus (maskspanel.cc).
            new ArtShortcut("view.localmask", "View", "Local Editing Mask", "Mask", VirtualKeyCode.KeyM, Primary),
            new ArtShortcut("view.falsecolor", "View", "False Color", "False Col", VirtualKeyCode.KeyE, ModifierKey.Shift),

            new ArtShortcut("navigate.previous", "Navigate", "Previous Image", "Prev Img", VirtualKeyCode.F3, ModifierKey.Shift),
            new ArtShortcut("navigate.next", "Navigate", "Next Image", "Next Img", VirtualKeyCode.F4, ModifierKey.Shift),

            new ArtShortcut("tab.exposure", "Tool Tabs", "Exposure", "Tab Exp", VirtualKeyCode.KeyE, Alt),
            new ArtShortcut("tab.color", "Tool Tabs", "Color", "Tab Col", VirtualKeyCode.KeyC, Alt),
            new ArtShortcut("tab.detail", "Tool Tabs", "Detail", "Tab Det", VirtualKeyCode.KeyD, Alt),
            new ArtShortcut("tab.effects", "Tool Tabs", "Special effects", "Tab FX", VirtualKeyCode.KeyF, Alt),
            new ArtShortcut("tab.transform", "Tool Tabs", "Transform", "Tab Trf", VirtualKeyCode.KeyT, Alt),
            new ArtShortcut("tab.raw", "Tool Tabs", "Raw", "Tab Raw", VirtualKeyCode.KeyR, Alt),
            new ArtShortcut("tab.local", "Tool Tabs", "Local editing", "Tab Loc", VirtualKeyCode.KeyX, Alt),
            new ArtShortcut("tab.favorites", "Tool Tabs", "Favorites", "Tab Fav", VirtualKeyCode.KeyU, Alt),
            new ArtShortcut("tab.metadata", "Tool Tabs", "Metadata", "Tab Meta", VirtualKeyCode.KeyM, Alt),

            new ArtShortcut("tool.crop", "Tools", "Crop", "Crop", VirtualKeyCode.KeyC, ModifierKey.Shift),
            new ArtShortcut("tool.straighten", "Tools", "Straighten", "Straight", VirtualKeyCode.KeyS, ModifierKey.Shift),
            new ArtShortcut("tool.spotwb", "Tools", "Spot White Balance", "Spot WB", VirtualKeyCode.KeyW, ModifierKey.Shift),

            new ArtShortcut("output.save", "Output", "Save", "Save", VirtualKeyCode.KeyS, Primary),
            new ArtShortcut("output.queue", "Output", "Add to Queue", "Queue", VirtualKeyCode.KeyB, Primary),
            new ArtShortcut("output.external", "Output", "External Editor", "Ext Edit", VirtualKeyCode.KeyE, Primary),
        };

        private static readonly Dictionary<String, ArtShortcut> ById = All.ToDictionary(s => s.Id);

        public static ArtShortcut Find(String id) => id != null && ById.TryGetValue(id, out var shortcut) ? shortcut : null;
    }
}
