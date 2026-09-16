namespace RayGui_cs
{
    /// <summary>Options for <see cref="Gui.FolderBrowserDialog"/>, modeled on the properties of the WinForms FolderBrowserDialog.</summary>
    /// <remarks><c>default</c> has the same defaults as <c>new()</c>.</remarks>
    public readonly record struct FolderBrowserDialogOptions
    {
        // Options that default to true are stored inverted, so default(FolderBrowserDialogOptions) has the documented defaults.
        private readonly bool noDimBackground;
        private readonly bool noShowNewFolderButton;
        private readonly bool noShowPinnedPlaces;

        /// <summary>The text shown above the folder list, or null for none.</summary>
        public string? Description { get; init; }

        /// <inheritdoc cref="OpenFileDialogOptions.DimBackground"/>
        public bool DimBackground { get => !noDimBackground; init => noDimBackground = !value; }

        /// <summary>Whether the user can select more than one folder.</summary>
        public bool Multiselect { get; init; }

        /// <summary>Whether the Select Folder button stays disabled until the user interacts with the dialog.</summary>
        public bool OkRequiresInteraction { get; init; }

        /// <summary>Whether the dialog has a button for creating folders. Defaults to true.</summary>
        public bool ShowNewFolderButton { get => !noShowNewFolderButton; init => noShowNewFolderButton = !value; }

        /// <summary>Whether the places list (known folders and drives) is shown. Defaults to true.</summary>
        public bool ShowPinnedPlaces { get => !noShowPinnedPlaces; init => noShowPinnedPlaces = !value; }

        internal FileBrowserOptions ToBrowserOptions(string title) => new()
        {
            Title = title,
            Description = string.IsNullOrEmpty(Description) ? null : Description,
            OkText = Multiselect ? "Select Folders" : "Select Folder",
            Multiselect = Multiselect,
            DimBackground = DimBackground,
            ShowPinnedPlaces = ShowPinnedPlaces,
            ShowNewFolderButton = ShowNewFolderButton,
            OkRequiresInteraction = OkRequiresInteraction,
        };
    }
}
