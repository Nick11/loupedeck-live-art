namespace Loupedeck.ArtRawEditorPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // ART hold-key slider (src/gui/shortcutmanager.cc): hold Key, then keypad +/- steps, keypad * resets.
    // Tool and Label follow ART's UI texts; Short (≤ 8 chars) is shown on the device and must stay unambiguous.
    internal sealed record ArtSlider(String Id, String Tool, String Label, String Short, VirtualKeyCode Key);

    internal static class ArtSliders
    {
        // ART 1.26.10, registerShortcuts() in src/gui/*.cc. WB tint ('i') is shadowed by L*a*b* lightness.
        public static readonly IReadOnlyList<ArtSlider> All = new[]
        {
            new ArtSlider("exposure.compensation", "Exposure", "Compensation", "Exp Comp", VirtualKeyCode.KeyE),
            new ArtSlider("exposure.black", "Exposure", "Black point", "Black P.", VirtualKeyCode.KeyQ),
            new ArtSlider("whitebalance.temperature", "White Balance", "Temperature", "WB Temp", VirtualKeyCode.KeyT),
            new ArtSlider("lab.lightness", "L*a*b* Adjustments", "Lightness", "Lab L", VirtualKeyCode.KeyI),
            new ArtSlider("lab.contrast", "L*a*b* Adjustments", "Contrast", "Lab Con", VirtualKeyCode.KeyJ),
            new ArtSlider("lab.chromaticity", "L*a*b* Adjustments", "Chromaticity", "Lab Chr", VirtualKeyCode.KeyN),
            new ArtSlider("tonecurve.contrast", "Tone Curves", "Contrast", "TC Con", VirtualKeyCode.KeyC),
            new ArtSlider("saturation.saturation", "Saturation/Vibrance", "Saturation", "Sat", VirtualKeyCode.KeyS),
            new ArtSlider("saturation.vibrance", "Saturation/Vibrance", "Vibrance", "Vib", VirtualKeyCode.KeyV),
            new ArtSlider("softlight.strength", "Soft Light", "Strength", "Soft L.", VirtualKeyCode.KeyO),
            new ArtSlider("logencoding.gain", "Log Tone Mapping", "Gain", "Log Gain", VirtualKeyCode.KeyG),
            new ArtSlider("logencoding.targetgray", "Log Tone Mapping", "Target gray point", "Log Gray", VirtualKeyCode.KeyB),
            new ArtSlider("logencoding.whiteev", "Log Tone Mapping", "White Ev", "Log WEv", VirtualKeyCode.KeyW),
            new ArtSlider("logencoding.blackev", "Log Tone Mapping", "Black Ev", "Log BEv", VirtualKeyCode.KeyD),
            new ArtSlider("logencoding.hlprecompression", "Log Tone Mapping", "Highlights precompression", "Log HLC", VirtualKeyCode.KeyK),
            new ArtSlider("toneequalizer.blacks", "Tone Equalizer", "Blacks", "Blacks", VirtualKeyCode.Key1),
            new ArtSlider("toneequalizer.shadows", "Tone Equalizer", "Shadows", "Shadows", VirtualKeyCode.Key2),
            new ArtSlider("toneequalizer.midtones", "Tone Equalizer", "Midtones", "Midtones", VirtualKeyCode.Key3),
            new ArtSlider("toneequalizer.highlights", "Tone Equalizer", "Highlights", "Highlights", VirtualKeyCode.Key4),
            new ArtSlider("toneequalizer.whites", "Tone Equalizer", "Whites", "Whites", VirtualKeyCode.Key5),
        };

        private static readonly Dictionary<String, ArtSlider> ById = All.ToDictionary(s => s.Id);

        public static ArtSlider Find(String id) => id != null && ById.TryGetValue(id, out var slider) ? slider : null;
    }
}
