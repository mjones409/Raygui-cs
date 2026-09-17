namespace RayGui_cs
{
    // raygui has no file dialogs, so these are built from raygui controls.
    public static partial class Gui
    {
        /// <summary>Draws a dialog for choosing files to open.</summary>
        /// <param name="dialog">The dialog to draw, created when it opened and passed back in every frame.</param>
        /// <returns>The dialog after this frame's input, to keep and pass back in on the next frame.</returns>
        /// <remarks>
        /// <para>
        /// Draw the dialog after the rest of the UI, and lock the rest of the UI while it is shown so it ignores input meant
        /// for the dialog. The dialog doesn't close itself: stop drawing it, usually by setting the field it is kept in to null,
        /// once <see cref="RayGui_cs.OpenFileDialog.Closed"/> is true.
        /// </para>
        /// <para>
        /// Escape cancels the dialog, but raylib also closes the window when its exit key (Escape by default) is pressed.
        /// Call <c>Raylib.SetExitKey(KeyboardKey.Null)</c> to keep Escape from closing the window.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The raylib window is not initialized.</exception>
        public static OpenFileDialog OpenFileDialog(OpenFileDialog dialog)
        {
            dialog.Update();
            return dialog;
        }

        /// <summary>Draws a dialog for choosing where to save a file.</summary>
        /// <inheritdoc cref="OpenFileDialog(RayGui_cs.OpenFileDialog)" path="/param"/>
        /// <inheritdoc cref="OpenFileDialog(RayGui_cs.OpenFileDialog)" path="/returns"/>
        /// <inheritdoc cref="OpenFileDialog(RayGui_cs.OpenFileDialog)" path="/remarks"/>
        /// <inheritdoc cref="OpenFileDialog(RayGui_cs.OpenFileDialog)" path="/exception"/>
        public static SaveFileDialog SaveFileDialog(SaveFileDialog dialog)
        {
            dialog.Update();
            return dialog;
        }

        /// <summary>Draws a dialog for choosing folders.</summary>
        /// <inheritdoc cref="OpenFileDialog(RayGui_cs.OpenFileDialog)" path="/param"/>
        /// <inheritdoc cref="OpenFileDialog(RayGui_cs.OpenFileDialog)" path="/returns"/>
        /// <inheritdoc cref="OpenFileDialog(RayGui_cs.OpenFileDialog)" path="/remarks"/>
        /// <inheritdoc cref="OpenFileDialog(RayGui_cs.OpenFileDialog)" path="/exception"/>
        public static FolderBrowserDialog FolderBrowserDialog(FolderBrowserDialog dialog)
        {
            dialog.Update();
            return dialog;
        }
    }
}
