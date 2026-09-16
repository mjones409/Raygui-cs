namespace RayGui_cs
{
    /// <summary>Options for <see cref="Gui.OpenFileDialog"/>, modeled on the properties of the WinForms OpenFileDialog.</summary>
    /// <remarks><c>default</c> has the same defaults as <c>new()</c>.</remarks>
    public readonly record struct OpenFileDialogOptions
    {
        // Options that default to true are stored inverted, so default(OpenFileDialogOptions) has the documented defaults.
        private readonly bool noAddExtension;
        private readonly bool noCheckFileExists;
        private readonly bool noCheckPathExists;
        private readonly bool noDereferenceLinks;
        private readonly bool noDimBackground;
        private readonly bool noSelectReadOnlyFiles;
        private readonly bool noShowPinnedPlaces;
        private readonly bool noValidateNames;
        private readonly string? defaultExt;
        private readonly string? filter;

        /// <summary>Whether an extension is added to a file name typed without one. Defaults to true.</summary>
        /// <remarks>The extension comes from the selected filter, or <see cref="DefaultExt"/> when the filter doesn't name one.</remarks>
        public bool AddExtension { get => !noAddExtension; init => noAddExtension = !value; }

        /// <summary>Whether the dialog shows an error when the user names a file that doesn't exist. Defaults to true.</summary>
        public bool CheckFileExists { get => !noCheckFileExists; init => noCheckFileExists = !value; }

        /// <summary>Whether the dialog shows an error when the user names a folder that doesn't exist. Defaults to true.</summary>
        public bool CheckPathExists { get => !noCheckPathExists; init => noCheckPathExists = !value; }

        /// <summary>The folders shown at the top of the places list, or null for none.</summary>
        public IReadOnlyList<FileDialogCustomPlace>? CustomPlaces { get; init; }

        /// <summary>
        /// The extension added by <see cref="AddExtension"/> when the selected filter doesn't name one, without the leading dot,
        /// or null for none.
        /// </summary>
        public string? DefaultExt
        {
            get => defaultExt;
            init => defaultExt = value?.TrimStart('.') is { Length: > 0 } extension ? extension : null;
        }

        /// <summary>Whether a selected symbolic link returns the path of its target instead of the link. Defaults to true.</summary>
        /// <remarks>Windows shortcut (.lnk) files are not resolved.</remarks>
        public bool DereferenceLinks { get => !noDereferenceLinks; init => noDereferenceLinks = !value; }

        /// <summary>Whether the rest of the screen is dimmed behind the dialog. Defaults to true.</summary>
        public bool DimBackground { get => !noDimBackground; init => noDimBackground = !value; }

        /// <summary>
        /// The file filters, as pairs of descriptions and semicolon separated patterns, all separated by '|', or null for none.
        /// </summary>
        /// <example><c>"Images (*.png, *.jpg)|*.png;*.jpg|All files (*.*)|*.*"</c></example>
        /// <exception cref="ArgumentException">The value doesn't have a pattern for every description.</exception>
        public string? Filter
        {
            get => filter;
            init
            {
                FileFilter.Parse(value);
                filter = value;
            }
        }

        /// <summary>Whether the user can select more than one file.</summary>
        public bool Multiselect { get; init; }

        /// <summary>Whether the Open button stays disabled until the user interacts with the dialog.</summary>
        public bool OkRequiresInteraction { get; init; }

        /// <summary>Whether the user can select files that have the read-only attribute. Defaults to true.</summary>
        public bool SelectReadOnlyFiles { get => !noSelectReadOnlyFiles; init => noSelectReadOnlyFiles = !value; }

        /// <summary>Whether the dialog has a Help button, reported by <see cref="FileDialogResult.HelpClicked"/>.</summary>
        public bool ShowHelp { get; init; }

        /// <summary>Whether the places list (custom places, known folders and drives) is shown. Defaults to true.</summary>
        public bool ShowPinnedPlaces { get => !noShowPinnedPlaces; init => noShowPinnedPlaces = !value; }

        /// <summary>Whether the dialog has an "Open as read-only" check box, kept in <see cref="FileDialogState.ReadOnlyChecked"/>.</summary>
        public bool ShowReadOnly { get; init; }

        /// <summary>Whether extensions with more than one dot, such as ".tar.gz", are added whole by <see cref="AddExtension"/>.</summary>
        public bool SupportMultiDottedExtensions { get; init; }

        /// <summary>Whether the dialog rejects file names that contain invalid characters. Defaults to true.</summary>
        public bool ValidateNames { get => !noValidateNames; init => noValidateNames = !value; }

        internal FileBrowserOptions ToBrowserOptions(string title) => new()
        {
            Title = title,
            OkText = "Open",
            Multiselect = Multiselect,
            Filter = Filter,
            DimBackground = DimBackground,
            ShowPinnedPlaces = ShowPinnedPlaces,
            ShowReadOnly = ShowReadOnly,
            ShowHelp = ShowHelp,
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
        };
    }
}
