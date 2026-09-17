using Raylib_cs;

namespace RayGui_cs
{
    /// <summary>
    /// A dialog for choosing folders, drawn by <see cref="Gui.FolderBrowserDialog"/>. Its options are modeled on the
    /// WinForms FolderBrowserDialog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Create one when the dialog should open, keep it in a nullable field, and pass it to <see cref="Gui.FolderBrowserDialog"/>
    /// every frame, storing what it returns. Check <see cref="Accepted"/> for the chosen folders, and set the field to null
    /// once <see cref="Closed"/> is true.
    /// </para>
    /// <code>
    /// FolderBrowserDialog? folderDialog;
    ///
    /// if (Gui.Button(browseButton, "Browse...")) folderDialog = new FolderBrowserDialog { Description = "Choose the output folder" };
    ///
    /// if (folderDialog is FolderBrowserDialog dialog)
    /// {
    ///     dialog = Gui.FolderBrowserDialog(dialog);
    ///     if (dialog.Accepted) outputFolder = dialog.SelectedPath;
    ///     folderDialog = dialog.Closed ? null : dialog;
    /// }
    /// </code>
    /// <para>The options can change between frames. <c>default</c> has the same defaults as <c>new()</c>.</para>
    /// </remarks>
    public struct FolderBrowserDialog
    {
        /// <inheritdoc cref="OpenFileDialog.State"/>
        public FileDialogState State;

        // Options that default to true are stored inverted, so default(FolderBrowserDialog) has the documented defaults.
        private bool noDimBackground;
        private bool noShowNewFolderButton;
        private bool noShowPinnedPlaces;
        private IReadOnlyList<string>? selectedPaths;

        /// <summary>Creates a dialog at the given bounds.</summary>
        /// <param name="bounds">The dialog bounds; empty bounds center the dialog.</param>
        /// <param name="title">The title, or null for "Select Folder".</param>
        public FolderBrowserDialog(Rectangle bounds, string? title = null)
        {
            Bounds = bounds;
            Title = title;
        }

        /// <inheritdoc cref="OpenFileDialog.Bounds"/>
        public Rectangle Bounds { get; set; }

        /// <summary>Gets or sets the title, or null for "Select Folder".</summary>
        public string? Title { get; set; }

        /// <summary>Gets or sets the text shown above the folder list, or null for none.</summary>
        public string? Description { get; set; }

        /// <inheritdoc cref="OpenFileDialog.InitialDirectory"/>
        public string? InitialDirectory { get; set; }

        /// <summary>
        /// Gets or sets the selected folder. When the dialog opens, it is the folder initially selected, and its parent takes
        /// precedence over <see cref="InitialDirectory"/>. When the user accepts, it becomes the full path of the first chosen folder.
        /// </summary>
        public string? SelectedPath { get; set; }

        /// <summary>Gets the full paths of the folders the user accepted last, or an empty list before the user accepts.</summary>
        public IReadOnlyList<string> SelectedPaths
        {
            readonly get => selectedPaths ?? [];
            private set => selectedPaths = value;
        }

        /// <summary>Gets or sets whether the user can select more than one folder.</summary>
        public bool Multiselect { get; set; }

        /// <inheritdoc cref="OpenFileDialog.ShowHiddenFiles"/>
        public bool ShowHiddenFiles { get; set; }

        /// <summary>Gets or sets whether the Select Folder button stays disabled until the user interacts with the dialog.</summary>
        public bool OkRequiresInteraction { get; set; }

        /// <inheritdoc cref="OpenFileDialog.DimBackground"/>
        public bool DimBackground { readonly get => !noDimBackground; set => noDimBackground = !value; }

        /// <summary>Gets or sets whether the dialog has a button for creating folders. Defaults to true.</summary>
        public bool ShowNewFolderButton { readonly get => !noShowNewFolderButton; set => noShowNewFolderButton = !value; }

        /// <summary>Gets or sets whether the places list (known folders and drives) is shown. Defaults to true.</summary>
        public bool ShowPinnedPlaces { readonly get => !noShowPinnedPlaces; set => noShowPinnedPlaces = !value; }

        /// <summary>Gets whether the user accepted folders this frame; they are in <see cref="SelectedPaths"/>.</summary>
        public bool Accepted { get; private set; }

        /// <inheritdoc cref="OpenFileDialog.Canceled"/>
        public bool Canceled { get; private set; }

        /// <summary>
        /// Gets whether the user accepted or canceled the dialog this frame, so it should close. The dialog stays open for as
        /// long as it is drawn, so drawing it again, for example to reject the chosen folders, keeps it open.
        /// </summary>
        public readonly bool Closed => Accepted || Canceled;

        // Draws this frame and takes in what the user did.
        internal void Update()
        {
            FileBrowserResult result = FileBrowser.Draw(FileBrowserMode.Folder, ref State, new FileBrowserOptions
            {
                Bounds = Bounds,
                Title = Title ?? "Select Folder",
                Description = string.IsNullOrEmpty(Description) ? null : Description,
                OkText = Multiselect ? "Select Folders" : "Select Folder",
                InitialDirectory = InitialDirectory,
                InitialFileName = SelectedPath,
                Multiselect = Multiselect,
                ShowHiddenFiles = ShowHiddenFiles,
                DimBackground = DimBackground,
                ShowPinnedPlaces = ShowPinnedPlaces,
                ShowNewFolderButton = ShowNewFolderButton,
                OkRequiresInteraction = OkRequiresInteraction,
            });

            Bounds = result.Bounds;
            ShowHiddenFiles = result.ShowHiddenFiles;
            Accepted = result.Paths is not null;
            Canceled = result.Canceled;
            if (result.Paths is { } paths)
            {
                SelectedPaths = paths;
                SelectedPath = paths[0];
            }
        }
    }
}
