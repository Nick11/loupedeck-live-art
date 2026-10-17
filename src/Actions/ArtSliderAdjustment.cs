namespace Loupedeck.ArtRawEditorPlugin
{
    using System;

    // Turn = step the ART slider, press = reset it to its default.
    public class ArtSliderAdjustment : PluginDynamicAdjustment
    {
        private readonly Acceleration _acceleration = new();

        public ArtSliderAdjustment()
            : base(hasReset: true)
        {
            this.DisplayName = "ART Slider";
            this.GroupName = "Sliders";

            foreach (var slider in ArtSliders.All)
            {
                this.AddParameter(slider.Id, $"{slider.Tool}: {slider.Label}", slider.Tool);
            }
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            if (ArtSliders.Find(actionParameter) is { } slider)
            {
                ArtRawEditorPlugin.KeySender?.EnqueueSteps(slider, this._acceleration.Steps(slider.Id, diff));
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            if (ArtSliders.Find(actionParameter) is { } slider)
            {
                ArtRawEditorPlugin.KeySender?.EnqueueReset(slider);
            }
        }

        protected override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize) =>
            ArtSliders.Find(actionParameter)?.Short;

        protected override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize) =>
            ArtIcons.Load(actionParameter);
    }
}
