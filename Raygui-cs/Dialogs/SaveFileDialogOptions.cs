namespace RayGui_cs
{
    /// <summary>Options for <see cref="Gui.SaveFileDialog"/>, modeled on the properties of the WinForms SaveFileDialog.</summary>
    /// <remarks><c>default</c> has the same defaults as <c>new()</c>.</remarks>
    public readonly record struct SaveFileDialogOptions
    {
        // Options that default to true are stored inverted, so default(SaveFileDialogOptions) has the documented defaults.
        private readonly bool noAddExtension;
        private readonly bool noCheckPathExists;
        private readonly bool noCheckWriteAccess;
        private readonly bool noDereferenceLinks;
        private readonly bool noDimBackground;
        private readonly bool noOverwritePrompt;
        private readonly bool noShowPinnedPlaces;
        private readonly bool noValidateNames;
        private readonly string? defaultExt;
        private readonly string? filter;

        /// <inheritdoc cref="OpenFileDialogOptions.AddExtension"/>
        public bool AddExtension { get => !noAddExtension; init => noAddExtension = !value; }

        /// <summary>Whether the dialog shows an error when the user names a file that doesn't exist.</summary>
        public bool CheckFileExists { get; init; }

        /// <inheritdoc cref="OpenFileDialogOptions.CheckPathExists"/>
        public bool CheckPathExists { get => !noCheckPathExists; init => noCheckPathExists = !value; }

        /// <summary>Whether the dialog rejects existing files that can't be written to. Defaults to true.</summary>
        public bool CheckWriteAccess { get => !noCheckWriteAccess; init => noCheckWriteAccess = !value; }

        /// <summary>Whether the dialog asks for permission to create a file that doesn't exist.</summary>
        public bool CreatePrompt { get; init; }

        /// <inheritdoc cref="OpenFileDialogOptions.CustomPlaces"/>
        public IReadOnlyList<FileDialogCustomPlace>? CustomPlaces { get; init; }

        /// <inheritdoc cref="OpenFileDialogOptions.DefaultExt"/>
        public string? DefaultExt
        {
            get => defaultExt;
            init => defaultExt = value?.TrimStart('.') is { Length: > 0 } extension ? extension : null;
        }

        /// <inheritdoc cref="OpenFileDialogOptions.DereferenceLinks"/>
        public bool DereferenceLinks { get => !noDereferenceLinks; init => noDereferenceLinks = !value; }

        /// <inheritdoc cref="OpenFileDialogOptions.DimBackground"/>
        public bool DimBackground { get => !noDimBackground; init => noDimBackground = !value; }

        /// <inheritdoc cref="OpenFileDialogOptions.Filter"/>
        public string? Filter
        {
            get => filter;
            init
            {
                FileFilter.Parse(value);
                filter = value;
            }
        }

        /// <summary>Whether the Save button stays disabled until the user interacts with the dialog.</summary>
        public bool OkRequiresInteraction { get; init; }

        /// <summary>Whether the dialog asks for permission to replace a file that exists. Defaults to true.</summary>
        public bool OverwritePrompt { get => !noOverwritePrompt; init => noOverwritePrompt = !value; }

        /// <inheritdoc cref="OpenFileDialogOptions.ShowHelp"/>
        public bool ShowHelp { get; init; }

        /// <inheritdoc cref="OpenFileDialogOptions.ShowPinnedPlaces"/>
        public bool ShowPinnedPlaces { get => !noShowPinnedPlaces; init => noShowPinnedPlaces = !value; }

        /// <inheritdoc cref="OpenFileDialogOptions.SupportMultiDottedExtensions"/>
        public bool SupportMultiDottedExtensions { get; init; }

        /// <inheritdoc cref="OpenFileDialogOptions.ValidateNames"/>
        public bool ValidateNames { get => !noValidateNames; init => noValidateNames = !value; }

        internal FileBrowserOptions ToBrowserOptions(string title) => new()
        {
            Title = title,
            OkText = "Save",
            Filter = Filter,
            DimBackground = DimBackground,
            ShowPinnedPlaces = ShowPinnedPlaces,
            ShowNewFolderButton = true,
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
            CheckWriteAccess = CheckWriteAccess,
            OverwritePrompt = OverwritePrompt,
            CreatePrompt = CreatePrompt,
        };
    }
}
