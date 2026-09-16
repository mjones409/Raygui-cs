using System.Diagnostics.CodeAnalysis;

namespace RayGui_cs
{
    /// <summary>Prompts the user to select one or more folders, modeled on the WinForms class of the same name.</summary>
    public sealed class FolderBrowserDialog : CommonDialog
    {
        private string description = string.Empty;
        private string initialDirectory = string.Empty;
        private string[] selectedPaths = [];
        private Environment.SpecialFolder rootFolder;

        public FolderBrowserDialog()
        {
            Reset();
        }

        /// <summary>Gets or sets whether the dialog reopens in the last folder accepted by a dialog with the same GUID.</summary>
        public Guid? ClientGuid { get; set; }

        /// <summary>Gets or sets the text shown above the folder list, or in the title bar when <see cref="UseDescriptionForTitle"/> is true.</summary>
        [AllowNull]
        public string Description
        {
            get => description;
            set => description = value ?? string.Empty;
        }

        /// <summary>Gets or sets the folder the dialog opens in when <see cref="SelectedPath"/> is empty.</summary>
        [AllowNull]
        public string InitialDirectory
        {
            get => initialDirectory;
            set => initialDirectory = value ?? string.Empty;
        }

        /// <summary>Gets or sets whether the user can select more than one folder.</summary>
        public bool Multiselect { get; set; }

        /// <summary>Gets or sets whether the OK button stays disabled until the user interacts with the dialog.</summary>
        public bool OkRequiresInteraction { get; set; }

        /// <summary>Gets or sets the folder the dialog opens in when neither <see cref="SelectedPath"/> nor <see cref="InitialDirectory"/> is set.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is not a defined special folder.</exception>
        public Environment.SpecialFolder RootFolder
        {
            get => rootFolder;
            set
            {
                if (!Enum.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The value is not a defined special folder.");
                }
                rootFolder = value;
            }
        }

        /// <summary>Gets or sets the selected folder. When set before the dialog opens, the dialog opens next to it with it selected.</summary>
        [AllowNull]
        public string SelectedPath
        {
            get => selectedPaths.Length > 0 ? selectedPaths[0] : string.Empty;
            set => selectedPaths = string.IsNullOrEmpty(value) ? [] : [value];
        }

        /// <summary>Gets the full paths of all selected folders.</summary>
        public string[] SelectedPaths => (string[])selectedPaths.Clone();

        /// <summary>Gets or sets whether hidden and system folders are shown when the dialog opens. The user can toggle it.</summary>
        public bool ShowHiddenFiles { get; set; }

        /// <summary>Gets or sets whether the dialog has a button for creating folders.</summary>
        public bool ShowNewFolderButton { get; set; }

        /// <summary>Gets or sets whether the places list (known folders and drives) is shown.</summary>
        public bool ShowPinnedPlaces { get; set; }

        /// <summary>Gets or sets whether <see cref="Description"/> is used as the dialog title.</summary>
        public bool UseDescriptionForTitle { get; set; }

        public override void Reset()
        {
            ClientGuid = null;
            Description = null;
            InitialDirectory = null;
            Multiselect = false;
            OkRequiresInteraction = false;
            RootFolder = Environment.SpecialFolder.Desktop;
            SelectedPath = null;
            ShowHiddenFiles = false;
            ShowNewFolderButton = true;
            ShowPinnedPlaces = true;
            UseDescriptionForTitle = false;
        }

        public override string ToString()
        {
            return $"{base.ToString()}: Description: {Description}, SelectedPath: {SelectedPath}";
        }

        internal override FileBrowserOptions CreateOptions()
        {
            // Open next to the selected folder with it selected, like the Windows dialog.
            string? selectedParent = null;
            string selectedName = string.Empty;
            string selected = SelectedPath;
            if (selected.Length > 0)
            {
                try
                {
                    string full = Path.GetFullPath(selected);
                    selectedParent = Path.GetDirectoryName(full);
                    selectedName = selectedParent is null ? full : Path.GetFileName(full);
                }
                catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
                {
                }
            }

            // A selected drive root opens the drive list with the drive selected.
            bool selectedIsDrive = OperatingSystem.IsWindows() && selected.Length > 0 && selectedParent is null && selectedName.Length > 0;
            bool descriptionIsTitle = UseDescriptionForTitle && Description.Length > 0;
            return new FileBrowserOptions
            {
                Mode = FileBrowserMode.Folder,
                Title = descriptionIsTitle ? Description : "Select Folder",
                Description = descriptionIsTitle || Description.Length == 0 ? null : Description,
                OkText = Multiselect ? "Select Folders" : "Select Folder",
                Multiselect = Multiselect,
                InitialDirectory = selectedIsDrive ? null : ResolveInitialDirectory(selectedParent, InitialDirectory, GetRecentDirectory(ClientGuid), Environment.GetFolderPath(RootFolder)),
                FileName = selectedName,
                SelectName = selectedName,
                ShowHiddenFiles = ShowHiddenFiles,
                ShowNewFolderButton = ShowNewFolderButton,
                ShowPinnedPlaces = ShowPinnedPlaces,
                OkRequiresInteraction = OkRequiresInteraction,
            };
        }

        internal override void OnAccept(FileBrowser browser)
        {
            // The text box holds the selected folders' names, or what the user typed over them.
            string text = browser.FileNameText.Trim();
            List<string> paths;

            if (text.Length > 0)
            {
                List<string>? names = FileDialog.SplitFileNames(text);
                if (names is null || (names.Count > 1 && !Multiselect))
                {
                    browser.ShowError($"{text}\nThe folder name is not valid.");
                    return;
                }

                paths = [];
                foreach (string name in names)
                {
                    string? full = browser.ResolvePath(name);
                    if (full is null || !FileDialog.IsValidPathText(name))
                    {
                        browser.ShowError($"{name}\nThe folder name is not valid.");
                        return;
                    }
                    if (!Directory.Exists(full))
                    {
                        browser.ShowError($"{full}\nThe folder doesn't exist.\nCheck the name and try again.");
                        return;
                    }
                    paths.Add(Path.TrimEndingDirectorySeparator(full));
                }
            }
            else if (browser.CurrentDirectory is { } current)
            {
                paths = [current];
            }
            else
            {
                // The drive list has no folder of its own to select.
                return;
            }

            selectedPaths = [.. paths];
            browser.Close(DialogResult.OK);
        }

        internal override void OnClosed(FileBrowser browser, DialogResult result)
        {
            if (result == DialogResult.OK)
            {
                SetRecentDirectory(ClientGuid, browser.CurrentDirectory);
            }
        }
    }
}
