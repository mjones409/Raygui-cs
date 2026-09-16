using Raylib_cs;

namespace RayGui_cs
{
    /// <summary>Base class for dialogs drawn with raygui inside the application window.</summary>
    /// <remarks>
    /// <para>A dialog can be shown in two ways:</para>
    /// <list type="bullet">
    /// <item><see cref="ShowDialog"/> runs its own frame loop and returns when the dialog closes, like WinForms.</item>
    /// <item>
    /// <see cref="Open"/> opens the dialog without blocking. Call <see cref="Draw"/> every frame after drawing the rest of the UI;
    /// it returns the result on the frame the user closes the dialog. Lock the rest of the UI while
    /// <see cref="IsBlockingInput"/> is true so it ignores input meant for the dialog.
    /// </item>
    /// </list>
    /// <para>
    /// Escape cancels the dialog, but raylib also closes the window when its exit key (Escape by default) is pressed.
    /// <see cref="ShowDialog"/> turns the exit key off while it runs; when using <see cref="Open"/>, call
    /// <c>Raylib.SetExitKey(KeyboardKey.Null)</c> to keep Escape from closing the window.
    /// </para>
    /// </remarks>
    public abstract class CommonDialog
    {
        // The last folder each dialog was accepted in, keyed by ClientGuid (Guid.Empty when it is not set).
        private static readonly Dictionary<Guid, string> RecentDirectories = [];

        private FileBrowser? browser;
        private bool drawing;
        private bool swallowMouse;
        private DialogResult lastResult;

        private protected CommonDialog()
        {
        }

        /// <summary>Gets or sets an object that contains data about the dialog.</summary>
        public object? Tag { get; set; }

        /// <summary>Gets or sets the bounds of the dialog window, or null to center it on the screen.</summary>
        /// <remarks>Updated when the dialog closes, so the dialog reopens where the user left it.</remarks>
        public Rectangle? Bounds { get; set; }

        /// <summary>Gets or sets whether the rest of the screen is dimmed while the dialog is open.</summary>
        public bool DimBackground { get; set; } = true;

        /// <summary>Gets whether the dialog is open.</summary>
        public bool IsOpen => browser is not null;

        /// <summary>
        /// Gets whether the rest of the UI should ignore input: true while the dialog is open, and after it closes
        /// until the mouse button that closed it is released.
        /// </summary>
        /// <example><code>Gui.IsLocked = dialog.IsBlockingInput;</code></example>
        public bool IsBlockingInput => browser is not null || swallowMouse;

        /// <summary>Occurs when the user clicks the Help button of the dialog.</summary>
        public event EventHandler? HelpRequest;

        /// <summary>Resets all properties to their default values.</summary>
        public abstract void Reset();

        /// <summary>Opens the dialog without blocking; call <see cref="Draw"/> every frame to show it.</summary>
        /// <exception cref="InvalidOperationException">The dialog is already open, or the raylib window is not initialized.</exception>
        public void Open()
        {
            if (browser is not null)
            {
                throw new InvalidOperationException("The dialog is already open.");
            }
            if (!Raylib.IsWindowReady())
            {
                throw new InvalidOperationException("The raylib window must be initialized before a dialog is opened.");
            }

            lastResult = DialogResult.None;
            browser = new FileBrowser(this, CreateOptions(), Bounds);
        }

        /// <summary>Draws the dialog and handles its input. Call it every frame, after drawing the rest of the UI.</summary>
        /// <returns>The result on the frame the user closes the dialog; otherwise null.</returns>
        public DialogResult? Draw()
        {
            if (browser is null)
            {
                if (swallowMouse && !AnyMouseButtonDown())
                {
                    swallowMouse = false;
                }
                return null;
            }

            FileBrowser current = browser;
            bool wasLocked = Gui.IsLocked;
            drawing = true;
            try
            {
                current.Draw();
            }
            finally
            {
                drawing = false;
                Gui.IsLocked = wasLocked;
            }

            if (current.Result is not { } result)
            {
                return null;
            }
            if (browser == current)
            {
                Finish(result);
            }
            return result;
        }

        /// <summary>Closes the dialog without the user's input.</summary>
        public void Close(DialogResult result = DialogResult.Cancel)
        {
            if (browser is null)
            {
                return;
            }

            browser.Close(result);
            // While drawing, Draw finishes the close so it can return the result.
            if (!drawing)
            {
                Finish(result);
            }
        }

        /// <summary>Shows the dialog and runs its own frame loop until it closes.</summary>
        /// <param name="drawBackground">
        /// Draws the rest of the application behind the dialog each frame, between BeginDrawing and EndDrawing.
        /// The gui is locked while it runs. When null, only the background color is drawn.
        /// </param>
        /// <param name="exitKey">The raylib exit key to restore afterwards; raylib has no getter for it.</param>
        /// <returns><see cref="DialogResult.OK"/> when the user accepted the dialog; otherwise <see cref="DialogResult.Cancel"/>.</returns>
        /// <remarks>
        /// Can be called from inside a frame, such as when a button is clicked; the loop draws complete frames of its own
        /// and the calling frame continues afterwards. Closing the window cancels the dialog.
        /// </remarks>
        public DialogResult ShowDialog(Action? drawBackground = null, KeyboardKey exitKey = KeyboardKey.Escape)
        {
            Open();
            Raylib.SetExitKey(KeyboardKey.Null);
            try
            {
                while (browser is not null)
                {
                    if (Raylib.WindowShouldClose())
                    {
                        Close(DialogResult.Cancel);
                        break;
                    }

                    Raylib.BeginDrawing();
                    DrawModalBackground(drawBackground);
                    Draw();
                    Raylib.EndDrawing();
                }

                // Keep drawing until the mouse button that closed the dialog is released, so the release
                // doesn't click whatever the caller draws under the mouse later in this frame.
                while (!Raylib.WindowShouldClose() && (AnyMouseButtonDown() || AnyMouseButtonReleased()))
                {
                    Raylib.BeginDrawing();
                    DrawModalBackground(drawBackground);
                    Raylib.EndDrawing();
                }
                swallowMouse = false;
            }
            finally
            {
                if (browser is not null)
                {
                    Close(DialogResult.Cancel);
                }
                Raylib.SetExitKey(exitKey);
            }
            return lastResult == DialogResult.OK ? DialogResult.OK : DialogResult.Cancel;
        }

        /// <summary>Raises the <see cref="HelpRequest"/> event.</summary>
        protected virtual void OnHelpRequest(EventArgs e)
        {
            HelpRequest?.Invoke(this, e);
        }

        internal void RaiseHelpRequest() => OnHelpRequest(EventArgs.Empty);

        // Snapshots the dialog's properties for the browser.
        internal abstract FileBrowserOptions CreateOptions();

        // Called when the user accepts the browser; closes it with browser.Close(DialogResult.OK) when the input is valid.
        internal abstract void OnAccept(FileBrowser browser);

        internal virtual void OnClosed(FileBrowser browser, DialogResult result)
        {
        }

        private protected static string? GetRecentDirectory(Guid? clientGuid)
        {
            return RecentDirectories.GetValueOrDefault(clientGuid ?? Guid.Empty);
        }

        private protected static void SetRecentDirectory(Guid? clientGuid, string? directory)
        {
            if (directory is not null)
            {
                RecentDirectories[clientGuid ?? Guid.Empty] = directory;
            }
        }

        // Returns the first candidate that is an existing folder, falling back to the current directory.
        private protected static string ResolveInitialDirectory(params string?[] candidates)
        {
            foreach (string? candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }
                try
                {
                    string full = Path.GetFullPath(Environment.ExpandEnvironmentVariables(candidate));
                    if (Directory.Exists(full))
                    {
                        return full;
                    }
                }
                catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException or UnauthorizedAccessException)
                {
                }
            }
            return Environment.CurrentDirectory;
        }

        private void Finish(DialogResult result)
        {
            FileBrowser closing = browser!;
            browser = null;
            lastResult = result;
            Bounds = closing.Bounds;
            swallowMouse = AnyMouseButtonDown();
            closing.ReleaseCursor();
            OnClosed(closing, result);
        }

        private static void DrawModalBackground(Action? drawBackground)
        {
            Raylib.ClearBackground(GuiStyle.GetColor(GuiDefaultProperty.BackgroundColor));
            if (drawBackground is null)
            {
                return;
            }

            bool wasLocked = Gui.IsLocked;
            Gui.IsLocked = true;
            try
            {
                drawBackground();
            }
            finally
            {
                Gui.IsLocked = wasLocked;
            }
        }

        private static bool AnyMouseButtonDown()
        {
            return Raylib.IsMouseButtonDown(MouseButton.Left) || Raylib.IsMouseButtonDown(MouseButton.Right) || Raylib.IsMouseButtonDown(MouseButton.Middle);
        }

        private static bool AnyMouseButtonReleased()
        {
            return Raylib.IsMouseButtonReleased(MouseButton.Left) || Raylib.IsMouseButtonReleased(MouseButton.Right) || Raylib.IsMouseButtonReleased(MouseButton.Middle);
        }
    }
}
