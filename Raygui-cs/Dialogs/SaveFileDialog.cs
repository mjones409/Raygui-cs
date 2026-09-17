using Raylib_cs;

namespace RayGui_cs
{
    /// <summary>
    /// A dialog for choosing where to save a file, drawn by <see cref="Gui.SaveFileDialog"/>. Its options are modeled on the
    /// WinForms SaveFileDialog.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Create one when the dialog should open, keep it in a nullable field, and pass it to <see cref="Gui.SaveFileDialog"/>
    /// every frame, storing what it returns. Check <see cref="Accepted"/> for the chosen file, and set the field to null
    /// once <see cref="Closed"/> is true.
    /// </para>
    /// <code>
    /// SaveFileDialog? saveDialog;
    ///
    /// if (Gui.Button(saveButton, "Save as...")) saveDialog = new SaveFileDialog { Filter = "Text files|*.txt", FileName = "notes.txt" };
    ///
    /// if (saveDialog is SaveFileDialog dialog)
    /// {
    ///     dialog = Gui.SaveFileDialog(dialog);
    ///     if (dialog.Accepted) Save(dialog.FileName!);
    ///     saveDialog = dialog.Closed ? null : dialog;
    /// }
    /// </code>
    /// <para>The options can change between frames. <c>default</c> has the same defaults as <c>new()</c>.</para>
    /// </remarks>
    public struct SaveFileDialog
    {
        /// <inheritdoc cref="OpenFileDialog.State"/>
        public FileDialogState State;

        // Options that default to true are stored inverted, so default(SaveFileDialog) has the documented defaults.
        private bool noAddExtension;
        private bool noCheckPathExists;
        private bool noCheckWriteAccess;
        private bool noDereferenceLinks;
        private bool noDimBackground;
        private bool noOverwritePrompt;
        private bool noShowPinnedPlaces;
        private bool noValidateNames;
        private string? defaultExt;
        private string? filter;
        private int filterIndex;

        /// <summary>Creates a dialog at the given bounds.</summary>
        /// <param name="bounds">The dialog bounds; empty bounds center the dialog.</param>
        /// <param name="title">The title, or null for "Save As".</param>
        public SaveFileDialog(Rectangle bounds, string? title = null)
        {
            Bounds = bounds;
            Title = title;
        }

        /// <inheritdoc cref="OpenFileDialog.Bounds"/>
        public Rectangle Bounds { get; set; }

        /// <summary>Gets or sets the title, or null for "Save As".</summary>
        public string? Title { get; set; }

        /// <inheritdoc cref="OpenFileDialog.InitialDirectory"/>
        public string? InitialDirectory { get; set; }

        /// <summary>
        /// Gets or sets the file name. When the dialog opens, it is the name initially typed, and a folder in it takes precedence
        /// over <see cref="InitialDirectory"/>. When the user accepts, it becomes the full path of the chosen file.
        /// </summary>
        public string? FileName { get; set; }

        /// <inheritdoc cref="OpenFileDialog.Filter"/>
        public string? Filter
        {
            readonly get => filter;
            set
            {
                FileFilter.Parse(value);
                filter = value;
            }
        }

        /// <inheritdoc cref="OpenFileDialog.FilterIndex"/>
        public int FilterIndex
        {
            readonly get => filterIndex;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegative(value);
                filterIndex = value;
            }
        }

        /// <inheritdoc cref="OpenFileDialog.DefaultExt"/>
        public string? DefaultExt
        {
            readonly get => defaultExt;
            set => defaultExt = value?.TrimStart('.') is { Length: > 0 } extension ? extension : null;
        }

        /// <inheritdoc cref="OpenFileDialog.CustomPlaces"/>
        public IReadOnlyList<FileDialogCustomPlace>? CustomPlaces { get; set; }

        /// <inheritdoc cref="OpenFileDialog.ShowHelp"/>
        public bool ShowHelp { get; set; }

        /// <inheritdoc cref="OpenFileDialog.ShowHiddenFiles"/>
        public bool ShowHiddenFiles { get; set; }

        /// <summary>Gets or sets whether the Save button stays disabled until the user interacts with the dialog.</summary>
        public bool OkRequiresInteraction { get; set; }

        /// <inheritdoc cref="OpenFileDialog.SupportMultiDottedExtensions"/>
        public bool SupportMultiDottedExtensions { get; set; }

        /// <summary>Gets or sets whether the dialog shows an error when the user names a file that doesn't exist.</summary>
        public bool CheckFileExists { get; set; }

        /// <summary>Gets or sets whether the dialog asks for permission to create a file that doesn't exist.</summary>
        public bool CreatePrompt { get; set; }

        /// <inheritdoc cref="OpenFileDialog.AddExtension"/>
        public bool AddExtension { readonly get => !noAddExtension; set => noAddExtension = !value; }

        /// <inheritdoc cref="OpenFileDialog.CheckPathExists"/>
        public bool CheckPathExists { readonly get => !noCheckPathExists; set => noCheckPathExists = !value; }

        /// <summary>Gets or sets whether the dialog rejects existing files that can't be written to. Defaults to true.</summary>
        public bool CheckWriteAccess { readonly get => !noCheckWriteAccess; set => noCheckWriteAccess = !value; }

        /// <inheritdoc cref="OpenFileDialog.DereferenceLinks"/>
        public bool DereferenceLinks { readonly get => !noDereferenceLinks; set => noDereferenceLinks = !value; }

        /// <inheritdoc cref="OpenFileDialog.DimBackground"/>
        public bool DimBackground { readonly get => !noDimBackground; set => noDimBackground = !value; }

        /// <summary>Gets or sets whether the dialog asks for permission to replace a file that exists. Defaults to true.</summary>
        public bool OverwritePrompt { readonly get => !noOverwritePrompt; set => noOverwritePrompt = !value; }

        /// <inheritdoc cref="OpenFileDialog.ShowPinnedPlaces"/>
        public bool ShowPinnedPlaces { readonly get => !noShowPinnedPlaces; set => noShowPinnedPlaces = !value; }

        /// <inheritdoc cref="OpenFileDialog.ValidateNames"/>
        public bool ValidateNames { readonly get => !noValidateNames; set => noValidateNames = !value; }

        /// <summary>Gets whether the user accepted a file this frame; it is in <see cref="FileName"/>.</summary>
        public bool Accepted { get; private set; }

        /// <inheritdoc cref="OpenFileDialog.Canceled"/>
        public bool Canceled { get; private set; }

        /// <inheritdoc cref="OpenFileDialog.HelpClicked"/>
        public bool HelpClicked { get; private set; }

        /// <summary>
        /// Gets whether the user accepted or canceled the dialog this frame, so it should close. The dialog stays open for as
        /// long as it is drawn, so drawing it again, for example to reject the chosen file, keeps it open.
        /// </summary>
        public readonly bool Closed => Accepted || Canceled;

        // Draws this frame and takes in what the user did.
        internal void Update()
        {
            FileBrowserResult result = FileBrowser.Draw(FileBrowserMode.Save, ref State, new FileBrowserOptions
            {
                Bounds = Bounds,
                Title = Title ?? "Save As",
                OkText = "Save",
                InitialDirectory = InitialDirectory,
                InitialFileName = FileName,
                Filter = Filter,
                FilterIndex = FilterIndex,
                ShowHelp = ShowHelp,
                ShowHiddenFiles = ShowHiddenFiles,
                ShowNewFolderButton = true,
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
                CheckWriteAccess = CheckWriteAccess,
                OverwritePrompt = OverwritePrompt,
                CreatePrompt = CreatePrompt,
            });

            Bounds = result.Bounds;
            filterIndex = result.FilterIndex;
            ShowHiddenFiles = result.ShowHiddenFiles;
            Accepted = result.Paths is not null;
            Canceled = result.Canceled;
            HelpClicked = result.HelpClicked;
            if (result.Paths is { } paths)
            {
                FileName = paths[0];
            }
        }
    }
}
