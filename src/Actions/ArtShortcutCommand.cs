namespace Loupedeck.ArtRawEditorPlugin
{
    using System;

    public class ArtShortcutCommand : PluginDynamicCommand
    {
        public ArtShortcutCommand()
        {
            this.DisplayName = "ART Shortcut";
            this.GroupName = "Shortcuts";

            foreach (var shortcut in ArtShortcuts.All)
            {
                this.AddParameter(shortcut.Id, shortcut.Label, shortcut.Group);
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            if (ArtShortcuts.Find(actionParameter) is { } shortcut)
            {
                ArtRawEditorPlugin.KeySender?.EnqueueShortcut(shortcut);
            }
        }

        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize) =>
            ArtIcons.Load(actionParameter);
    }
}
