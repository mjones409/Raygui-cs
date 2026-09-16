using Raylib_cs;

namespace RayGui_cs
{
    /// <summary>
    /// The state a file or folder dialog keeps between frames: its folder, history, selection, scroll position and prompts.
    /// </summary>
    /// <remarks>
    /// Create a state when the dialog opens and pass it to the dialog every frame. A state belongs to the kind of dialog
    /// it is first drawn with. Drop it once the dialog is dismissed, and create a new one to open the dialog again.
    /// </remarks>
    public sealed class FileDialogState
    {
        private FileBrowser? browser;
        private int filterIndex;

        /// <param name="directory">The folder the dialog opens in; the current directory when null or not an existing folder.</param>
        /// <param name="fileName">
        /// For file dialogs, the file name initially typed; for folder dialogs, the folder initially selected.
        /// A folder in it takes precedence over <paramref name="directory"/>, and the named item is selected when it exists.
        /// </param>
        public FileDialogState(string? directory = null, string? fileName = null)
        {
            InitialDirectory = directory;
            InitialFileName = fileName;
        }

        /// <summary>Gets the folder the dialog shows, or null while it shows the drive list or before it is first drawn.</summary>
        public string? CurrentDirectory => browser?.CurrentDirectory;

        /// <summary>Gets or sets the zero based index of the selected filter; the dialog clamps it to its filters.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
        public int FilterIndex
        {
            get => filterIndex;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value);
                filterIndex = value;
            }
        }

        /// <summary>Gets or sets whether the "Open as read-only" check box is checked.</summary>
        public bool ReadOnlyChecked { get; set; }

        /// <summary>Gets or sets whether hidden and system items are shown. The user can toggle it.</summary>
        public bool ShowHiddenFiles { get; set; }

        internal string? InitialDirectory { get; }

        internal string? InitialFileName { get; }

        // Returns the browser for the kind of dialog being drawn, creating it on the first frame.
        internal FileBrowser GetBrowser(FileBrowserMode mode, string paramName)
        {
            if (browser is null)
            {
                if (!Raylib.IsWindowReady())
                {
                    throw new InvalidOperationException("The raylib window must be initialized before a dialog is drawn.");
                }
                browser = new FileBrowser(mode, this);
            }
            else if (browser.Mode != mode)
            {
                throw new ArgumentException("The state belongs to a different kind of dialog.", paramName);
            }
            return browser;
        }
    }
}
