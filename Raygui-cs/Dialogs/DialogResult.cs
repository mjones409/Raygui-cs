namespace RayGui_cs
{
    /// <summary>How a dialog was closed.</summary>
    public enum DialogResult
    {
        /// <summary>The dialog has not been closed.</summary>
        None = 0,
        /// <summary>The user accepted the dialog.</summary>
        OK = 1,
        /// <summary>The user cancelled or closed the dialog.</summary>
        Cancel = 2,
    }
}
