namespace Loupedeck.ArtRawEditorPlugin
{
    using System;
    using System.IO;

    // Finished button images from icons/buttons/<action id>.svg (embedded). The icon files are the
    // single source of truth; SVG scales to every device size.
    internal static class ArtIcons
    {
        public static BitmapImage Load(String name)
        {
            using var stream = typeof(ArtIcons).Assembly.GetManifestResourceStream($"icons/buttons/{name}.svg");
            if (stream == null)
            {
                return null;
            }

            using var reader = new StreamReader(stream);
            return BitmapImage.FromSvg(reader.ReadToEnd());
        }
    }
}
