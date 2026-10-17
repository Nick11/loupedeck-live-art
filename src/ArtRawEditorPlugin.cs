namespace Loupedeck.ArtRawEditorPlugin
{
    using System;

    public class ArtRawEditorPlugin : Plugin
    {
        private String _inputProblem = String.Empty;

        public override Boolean UsesApplicationApiOnly => false;

        public override Boolean HasNoApplication => false;

        internal static KeySender KeySender { get; private set; }

        public ArtRawEditorPlugin() => PluginLog.Init(this.Log);

        public override void Load()
        {
            KeySender = new KeySender(this);
            this.ReportInputProblem(InputGuard.PermissionProblem());
        }

        public override void Unload()
        {
            KeySender?.Dispose();
            KeySender = null;
        }

        // Shows (or clears) a warning in the Loupedeck software when keystrokes can't be delivered.
        internal void ReportInputProblem(String problem)
        {
            if (problem != this._inputProblem)
            {
                this._inputProblem = problem;
                this.OnPluginStatusChanged(problem == null ? Loupedeck.PluginStatus.Normal : Loupedeck.PluginStatus.Warning, problem);
            }
        }
    }
}
