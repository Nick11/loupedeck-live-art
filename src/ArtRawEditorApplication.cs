namespace Loupedeck.ArtRawEditorPlugin
{
    using System;

    public class ArtRawEditorApplication : ClientApplication
    {
        protected override String GetProcessName() => "ART";

        protected override String GetBundleName() => "us.pixls.art.ART";
    }
}
