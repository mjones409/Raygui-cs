using Raylib_cs;

namespace RayGui_cs
{
    // raygui has no file dialogs, so these are built from raygui controls.
    public static partial class Gui
    {
        /// <summary>Gets bounds centered on the screen and sized for the current style, to open a file or folder dialog with.</summary>
        public static Rectangle GetFileDialogBounds()
        {
            return FileBrowser.DefaultBounds();
        }

        /// <summary>Draws a dialog for choosing files to open.</summary>
        /// <param name="bounds">The dialog bounds, clamped to the screen and a minimum size.</param>
        /// <param name="title">The title, or null for "Open".</param>
        /// <param name="state">The dialog state; create one when the dialog opens and pass it in every frame.</param>
        /// <param name="options">The dialog options, usually kept in a field; they can change from frame to frame.</param>
        /// <returns>The bounds after this frame's input, and the files the user accepted or whether the user canceled this frame.</returns>
        /// <remarks>
        /// <para>
        /// Draw the dialog after the rest of the UI, and lock the rest of the UI while the dialog is shown so it ignores input
        /// meant for the dialog. The dialog doesn't close itself: stop drawing it once the result is dismissed, or keep drawing
        /// it to reject the accepted paths.
        /// </para>
        /// <para>
        /// Escape cancels the dialog, but raylib also closes the window when its exit key (Escape by default) is pressed.
        /// Call <c>Raylib.SetExitKey(KeyboardKey.Null)</c> to keep Escape from closing the window.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="state"/> was drawn with a different kind of dialog.</exception>
        /// <exception cref="InvalidOperationException">The raylib window is not initialized.</exception>
        public static FileDialogResult OpenFileDialog(Rectangle bounds, string? title, FileDialogState state, OpenFileDialogOptions options = default)
        {
            ArgumentNullException.ThrowIfNull(state);
            return state.GetBrowser(FileBrowserMode.Open, nameof(state)).Draw(bounds, options.ToBrowserOptions(title ?? "Open"));
        }

        /// <summary>Draws a dialog for choosing where to save a file.</summary>
        /// <inheritdoc cref="OpenFileDialog" path="/param[@name!='title']"/>
        /// <param name="title">The title, or null for "Save As".</param>
        /// <returns>The bounds after this frame's input, and the file the user accepted or whether the user canceled this frame.</returns>
        /// <inheritdoc cref="OpenFileDialog" path="/remarks"/>
        /// <inheritdoc cref="OpenFileDialog" path="/exception"/>
        public static FileDialogResult SaveFileDialog(Rectangle bounds, string? title, FileDialogState state, SaveFileDialogOptions options = default)
        {
            ArgumentNullException.ThrowIfNull(state);
            return state.GetBrowser(FileBrowserMode.Save, nameof(state)).Draw(bounds, options.ToBrowserOptions(title ?? "Save As"));
        }

        /// <summary>Draws a dialog for choosing folders.</summary>
        /// <inheritdoc cref="OpenFileDialog" path="/param[@name!='title']"/>
        /// <param name="title">The title, or null for "Select Folder".</param>
        /// <returns>The bounds after this frame's input, and the folders the user accepted or whether the user canceled this frame.</returns>
        /// <inheritdoc cref="OpenFileDialog" path="/remarks"/>
        /// <inheritdoc cref="OpenFileDialog" path="/exception"/>
        public static FileDialogResult FolderBrowserDialog(Rectangle bounds, string? title, FileDialogState state, FolderBrowserDialogOptions options = default)
        {
            ArgumentNullException.ThrowIfNull(state);
            return state.GetBrowser(FileBrowserMode.Folder, nameof(state)).Draw(bounds, options.ToBrowserOptions(title ?? "Select Folder"));
        }
    }
}
