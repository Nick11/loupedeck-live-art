namespace Loupedeck.ArtRawEditorPlugin
{
    using System;

    // Turn = zoom in/out (keypad +/−, editorpanel.cc), press = zoom to fit.
    public class ArtZoomAdjustment : PluginDynamicAdjustment
    {
        private const Int32 MaxStepsPerTick = 3;

        public ArtZoomAdjustment()
            : base(displayName: "ART Zoom", description: "Zooms the ART preview in and out", groupName: "View", hasReset: true)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            var zoom = ArtShortcuts.Find(diff > 0 ? "view.zoomin" : "view.zoomout");
            for (var i = 0; i < Math.Min(Math.Abs(diff), MaxStepsPerTick); i++)
            {
                ArtRawEditorPlugin.KeySender?.EnqueueShortcut(zoom);
            }
        }

        protected override void RunCommand(String actionParameter) =>
            ArtRawEditorPlugin.KeySender?.EnqueueShortcut(ArtShortcuts.Find("view.fit"));

        protected override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize) => "Zoom";

        protected override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize) =>
            ArtIcons.Load("zoom.wheel");
    }
}
