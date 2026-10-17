namespace Loupedeck.ArtRawEditorPlugin
{
    using System;

    // Button variant of ArtSliderAdjustment for devices without dials (MX Creative Keypad, touch keys).
    // Parameter = "<slider id>.<op>", identical to the icon file name in icons/buttons.
    public class ArtSliderCommand : PluginDynamicCommand
    {
        private const String Increase = "plus", Decrease = "minus", Reset = "reset";

        public ArtSliderCommand()
        {
            this.DisplayName = "ART Slider Step";
            this.GroupName = "Sliders";

            foreach (var slider in ArtSliders.All)
            {
                foreach (var (op, label) in new[] { (Increase, "+"), (Decrease, "−"), (Reset, "Reset") })
                {
                    this.AddParameter($"{slider.Id}.{op}", $"{slider.Tool}: {slider.Label} {label}", slider.Tool);
                }
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            var dot = actionParameter?.LastIndexOf('.') ?? -1;
            if (dot < 0 || ArtSliders.Find(actionParameter[..dot]) is not { } slider)
            {
                return;
            }

            var sender = ArtRawEditorPlugin.KeySender;
            switch (actionParameter[(dot + 1)..])
            {
                case Increase: sender?.EnqueueSteps(slider, 1); break;
                case Decrease: sender?.EnqueueSteps(slider, -1); break;
                case Reset: sender?.EnqueueReset(slider); break;
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) =>
            ArtIcons.Load(actionParameter);
    }
}
