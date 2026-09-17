using Raylib_cs;

namespace RayGui_cs
{
    /// <summary>
    /// A dialog for choosing files to open, drawn by <see cref="Gui.OpenFileDialog"/>. Its options are modeled on the
    /// WinForms OpenFileDialog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Create one when the dialog should open, keep it in a nullable field, and pass it to <see cref="Gui.OpenFileDialog"/>
    /// every frame, storing what it returns. Check <see cref="Accepted"/> for the chosen files, and set the field to null
    /// once <see cref="Closed"/> is true.
    /// </para>
    /// <code>
    /// OpenFileDialog? openDialog;
    ///
    /// if (Gui.Button(openButton, "Open...")) openDialog = new OpenFileDialog { Filter = "Text files|*.txt" };
    ///
    /// if (openDialog is OpenFileDialog dialog)
    /// {
    ///     dialog = Gui.OpenFileDialog(dialog);
    ///     if (dialog.Accepted) Load(dialog.FileName);
    ///     openDialog = dialog.Closed ? null : dialog;
    /// }
    /// </code>
    /// <para>The options can change between frames. <c>default</c> has the same defaults as <c>new()</c>.</para>
    /// </remarks>
    public struct OpenFileDialog
    {
        /// <summary>
        /// The dialog's working state, such as its folder, selection and prompts. Most programs never touch it;
        /// see <see cref="FileDialogState"/>.
        /// </summary>
        public FileDialogState State;

        // Options that default to true are stored inverted, so default(OpenFileDialog) has the documented defaults.
        private bool noAddExtension;
        private bool noCheckFileExists;
        private bool noCheckPathExists;
        private bool noDereferenceLinks;
        private bool noDimBackground;
        private bool noSelectReadOnlyFiles;
        private bool noShowPinnedPlaces;
        private bool noValidateNames;
        private string? defaultExt;
        private string? filter;
        private int filterIndex;
        private IReadOnlyList<string>? fileNames;

        /// <summary>Creates a dialog at the given bounds.</summary>
        /// <param name="bounds">The dialog bounds; empty bounds center the dialog.</param>
        /// <param name="title">The title, or null for "Open".</param>
        public OpenFileDialog(Rectangle bounds, string? title = null)
        {
            Bounds = bounds;
            Title = title;
        }

        /// <summary>
        /// Gets or sets the dialog's position and size, clamped to the screen and a minimum size.
        /// Empty bounds center the dialog at a size that suits the style. Updated when the user moves or resizes the dialog.
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>Gets or sets the title, or null for "Open".</summary>
        public string? Title { get; set; }

        /// <summary>Gets or sets the folder the dialog opens in; the current directory when null or not an existing folder.</summary>
        public string? InitialDirectory { get; set; }

        /// <summary>
        /// Gets or sets the file name. When the dialog opens, it is the name initially typed, and a folder in it takes precedence
        /// over <see cref="InitialDirectory"/>. When the user accepts, it becomes the full path of the first chosen file.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>Gets the full paths of the files the user accepted last, or an empty list before the user accepts.</summary>
        public IReadOnlyList<string> FileNames
        {
            readonly get => fileNames ?? [];
            private set => fileNames = value;
        }

        /// <summary>
        /// Gets or sets the file filters, as pairs of descriptions and semicolon separated patterns, all separated by '|',
        /// or null for none.
        /// </summary>
        /// <example><c>"Images (*.png, *.jpg)|*.png;*.jpg|All files (*.*)|*.*"</c></example>
        /// <exception cref="ArgumentException">The value doesn't have a pattern for every description.</exception>
        public string? Filter
        {
            readonly get => filter;
            set
            {
                FileFilter.Parse(value);
                filter = value;
            }
        }

        /// <summary>Gets or sets the zero based index of the selected filter. The dialog clamps it to the filters, and the user can change it.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
        public int FilterIndex
        {
            readonly get => filterIndex;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value);
                filterIndex = value;
            }
        }

        /// <summary>
        /// Gets or sets the extension added by <see cref="AddExtension"/> when the selected filter doesn't name one,
        /// without the leading dot, or null for none.
        /// </summary>
        public string? DefaultExt
        {
            readonly get => defaultExt;
            set => defaultExt = value?.TrimStart('.') is { Length: > 0 } extension ? extension : null;
        }

        /// <summary>Gets or sets the folders shown at the top of the places list, or null for none.</summary>
        public IReadOnlyList<FileDialogCustomPlace>? CustomPlaces { get; set; }

        /// <summary>Gets or sets whether the user can select more than one file.</summary>
        public bool Multiselect { get; set; }

        /// <summary>
        /// Gets or sets whether a chosen folder is accepted instead of opened, so one dialog can return both files and folders.
        /// Folders are still opened by double clicking them or pressing Enter on them.
        /// </summary>
        public bool AllowFolders { get; set; }

        /// <summary>Gets or sets whether the dialog has an "Open as read-only" check box, kept in <see cref="ReadOnlyChecked"/>.</summary>
        public bool ShowReadOnly { get; set; }

        /// <summary>Gets or sets whether the "Open as read-only" check box is checked. The user can change it.</summary>
        public bool ReadOnlyChecked { get; set; }

        /// <summary>Gets or sets whether the dialog has a Help button, reported by <see cref="HelpClicked"/>.</summary>
        public bool ShowHelp { get; set; }

        /// <summary>Gets or sets whether hidden and system items are shown. The user can change it.</summary>
        public bool ShowHiddenFiles { get; set; }

        /// <summary>Gets or sets whether the Open button stays disabled until the user interacts with the dialog.</summary>
        public bool OkRequiresInteraction { get; set; }

        /// <summary>Gets or sets whether extensions with more than one dot, such as ".tar.gz", are added whole by <see cref="AddExtension"/>.</summary>
        public bool SupportMultiDottedExtensions { get; set; }

        /// <summary>Gets or sets whether an extension is added to a file name typed without one. Defaults to true.</summary>
        /// <remarks>The extension comes from the selected filter, or <see cref="DefaultExt"/> when the filter doesn't name one.</remarks>
        public bool AddExtension { readonly get => !noAddExtension; set => noAddExtension = !value; }

        /// <summary>Gets or sets whether the dialog shows an error when the user names a file that doesn't exist. Defaults to true.</summary>
        public bool CheckFileExists { readonly get => !noCheckFileExists; set => noCheckFileExists = !value; }

        /// <summary>Gets or sets whether the dialog shows an error when the user names a folder that doesn't exist. Defaults to true.</summary>
        public bool CheckPathExists { readonly get => !noCheckPathExists; set => noCheckPathExists = !value; }

        /// <summary>Gets or sets whether a selected symbolic link returns the path of its target instead of the link. Defaults to true.</summary>
        /// <remarks>Windows shortcut (.lnk) files are not resolved.</remarks>
        public bool DereferenceLinks { readonly get => !noDereferenceLinks; set => noDereferenceLinks = !value; }

        /// <summary>Gets or sets whether the rest of the screen is dimmed behind the dialog. Defaults to true.</summary>
        public bool DimBackground { readonly get => !noDimBackground; set => noDimBackground = !value; }

        /// <summary>Gets or sets whether the user can select files that have the read-only attribute. Defaults to true.</summary>
        public bool SelectReadOnlyFiles { readonly get => !noSelectReadOnlyFiles; set => noSelectReadOnlyFiles = !value; }

        /// <summary>Gets or sets whether the places list (custom places, known folders and drives) is shown. Defaults to true.</summary>
        public bool ShowPinnedPlaces { readonly get => !noShowPinnedPlaces; set => noShowPinnedPlaces = !value; }

        /// <summary>Gets or sets whether the dialog rejects file names that contain invalid characters. Defaults to true.</summary>
        public bool ValidateNames { readonly get => !noValidateNames; set => noValidateNames = !value; }

        /// <summary>Gets whether the user accepted files this frame; they are in <see cref="FileNames"/>.</summary>
        public bool Accepted { get; private set; }

        /// <summary>Gets whether the user canceled the dialog this frame, with Cancel, the close button or Escape.</summary>
        public bool Canceled { get; private set; }

        /// <summary>Gets whether the Help button was clicked this frame.</summary>
        public bool HelpClicked { get; private set; }

        /// <summary>
        /// Gets whether the user accepted or canceled the dialog this frame, so it should close. The dialog stays open for as
        /// long as it is drawn, so drawing it again, for example to reject the chosen files, keeps it open.
        /// </summary>
        public readonly bool Closed => Accepted || Canceled;

        // Draws this frame and takes in what the user did.
        internal void Update()
        {
            FileBrowserResult result = FileBrowser.Draw(FileBrowserMode.Open, ref State, new FileBrowserOptions
            {
                Bounds = Bounds,
                Title = Title ?? "Open",
                OkText = "Open",
                InitialDirectory = InitialDirectory,
                InitialFileName = FileName,
                Filter = Filter,
                FilterIndex = FilterIndex,
                Multiselect = Multiselect,
                AllowFolders = AllowFolders,
                ShowReadOnly = ShowReadOnly,
                ReadOnlyChecked = ReadOnlyChecked,
                ShowHelp = ShowHelp,
                ShowHiddenFiles = ShowHiddenFiles,
                DimBackground = DimBackground,
                ShowPinnedPlaces = ShowPinnedPlaces,
                OkRequiresInteraction = OkRequiresInteraction,
                CustomPlaces = CustomPlaces,
                AddExtension = AddExtension,
                CheckFileExists = CheckFileExists,
                CheckPathExists = CheckPathExists,
                DefaultExt = DefaultExt,
                DereferenceLinks = DereferenceLinks,
                SupportMultiDottedExtensions = SupportMultiDottedExtensions,
                ValidateNames = ValidateNames,
                SelectReadOnlyFiles = SelectReadOnlyFiles,
            });

            Bounds = result.Bounds;
            filterIndex = result.FilterIndex;
            ReadOnlyChecked = result.ReadOnlyChecked;
            ShowHiddenFiles = result.ShowHiddenFiles;
            Accepted = result.Paths is not null;
            Canceled = result.Canceled;
            HelpClicked = result.HelpClicked;
            if (result.Paths is { } paths)
            {
                FileNames = paths;
                FileName = paths[0];
            }
        }
    }
}
