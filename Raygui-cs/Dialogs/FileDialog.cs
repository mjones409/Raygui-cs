using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace RayGui_cs
{
    /// <summary>Base class for <see cref="OpenFileDialog"/> and <see cref="SaveFileDialog"/>, modeled on the WinForms class of the same name.</summary>
    public abstract class FileDialog : CommonDialog
    {
        private static readonly string[] WindowsDeviceNames =
        [
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
        ];

        private string title = string.Empty;
        private string initialDirectory = string.Empty;
        private string defaultExt = string.Empty;
        private string? filter;
        private FileFilter[] filters = [];
        private string[] fileNames = [];
        private string? savedCurrentDirectory;

        private protected FileDialog()
        {
        }

        /// <summary>Gets or sets whether an extension is added to a file name typed without one.</summary>
        /// <remarks>The extension comes from the selected filter, or <see cref="DefaultExt"/> when the filter doesn't name one.</remarks>
        public bool AddExtension { get; set; }

        /// <summary>Gets or sets whether the dialog shows an error when the user names a file that doesn't exist.</summary>
        public bool CheckFileExists { get; set; }

        /// <summary>Gets or sets whether the dialog shows an error when the user names a folder that doesn't exist.</summary>
        public bool CheckPathExists { get; set; }

        /// <summary>
        /// Gets or sets the GUID that identifies this dialog's persisted state. Dialogs with the same GUID reopen in the
        /// last folder one of them was accepted in, for the lifetime of the process.
        /// </summary>
        public Guid? ClientGuid { get; set; }

        /// <summary>Gets the folders shown at the top of the places list.</summary>
        public FileDialogCustomPlacesCollection CustomPlaces { get; } = [];

        /// <summary>Gets or sets the extension added by <see cref="AddExtension"/> when the selected filter doesn't name one, without the leading dot.</summary>
        [AllowNull]
        public string DefaultExt
        {
            get => defaultExt;
            set => defaultExt = value is null ? string.Empty : value.TrimStart('.');
        }

        /// <summary>Gets or sets whether a selected symbolic link returns the path of its target instead of the link.</summary>
        /// <remarks>Windows shortcut (.lnk) files are not resolved.</remarks>
        public bool DereferenceLinks { get; set; }

        /// <summary>Gets or sets the first selected file, or the file name initially shown in the dialog.</summary>
        /// <remarks>
        /// When it includes an existing folder, the dialog opens in that folder. Set it to an empty string to clear the selection.
        /// </remarks>
        [AllowNull]
        public string FileName
        {
            get => fileNames.Length > 0 ? fileNames[0] : string.Empty;
            set => fileNames = string.IsNullOrEmpty(value) ? [] : [value];
        }

        /// <summary>Gets the full paths of all selected files.</summary>
        public string[] FileNames => (string[])fileNames.Clone();

        /// <summary>
        /// Gets or sets the file filters, as pairs of descriptions and semicolon separated patterns, all separated by '|'.
        /// </summary>
        /// <example><c>"Images (*.png, *.jpg)|*.png;*.jpg|All files (*.*)|*.*"</c></example>
        /// <exception cref="ArgumentException">The value doesn't have a pattern for every description.</exception>
        [AllowNull]
        public string Filter
        {
            get => filter ?? string.Empty;
            set
            {
                filters = FileFilter.Parse(value);
                filter = value;
            }
        }

        /// <summary>Gets or sets the one based index of the selected filter. Updated when the user accepts the dialog.</summary>
        public int FilterIndex { get; set; }

        /// <summary>Gets or sets the folder the dialog opens in.</summary>
        [AllowNull]
        public string InitialDirectory
        {
            get => initialDirectory;
            set => initialDirectory = value ?? string.Empty;
        }

        /// <summary>Gets or sets whether the OK button stays disabled until the user interacts with the dialog.</summary>
        public bool OkRequiresInteraction { get; set; }

        /// <summary>Gets or sets whether <see cref="Environment.CurrentDirectory"/> is restored when the dialog closes.</summary>
        /// <remarks>The dialog never changes the current directory itself, but a <see cref="FileOk"/> handler might.</remarks>
        public bool RestoreDirectory { get; set; }

        /// <summary>Gets or sets whether the dialog has a Help button, which raises <see cref="CommonDialog.HelpRequest"/>.</summary>
        public bool ShowHelp { get; set; }

        /// <summary>Gets or sets whether hidden and system files are shown when the dialog opens. The user can toggle it.</summary>
        public bool ShowHiddenFiles { get; set; }

        /// <summary>Gets or sets whether the places list (known folders, custom places and drives) is shown.</summary>
        public bool ShowPinnedPlaces { get; set; }

        /// <summary>Gets or sets whether extensions with more than one dot, such as ".tar.gz", are added whole by <see cref="AddExtension"/>.</summary>
        public bool SupportMultiDottedExtensions { get; set; }

        /// <summary>Gets or sets the dialog title, or an empty string for the default title.</summary>
        [AllowNull]
        public string Title
        {
            get => title;
            set => title = value ?? string.Empty;
        }

        /// <summary>Gets or sets whether the dialog rejects file names that contain invalid characters.</summary>
        public bool ValidateNames { get; set; }

        /// <summary>Occurs when the user accepts the dialog with valid files; cancel it to keep the dialog open.</summary>
        /// <remarks><see cref="FileNames"/> holds the accepted files while the event runs.</remarks>
        public event CancelEventHandler? FileOk;

        private protected abstract FileBrowserMode Mode { get; }

        private protected abstract string DefaultTitle { get; }

        private protected virtual bool AllowsMultipleFiles => false;

        public override void Reset()
        {
            AddExtension = true;
            CheckFileExists = false;
            CheckPathExists = true;
            ClientGuid = null;
            CustomPlaces.Clear();
            DefaultExt = null;
            DereferenceLinks = true;
            FileName = null;
            Filter = null;
            FilterIndex = 1;
            InitialDirectory = null;
            OkRequiresInteraction = false;
            RestoreDirectory = false;
            ShowHelp = false;
            ShowHiddenFiles = false;
            ShowPinnedPlaces = true;
            SupportMultiDottedExtensions = false;
            Title = null;
            ValidateNames = true;
        }

        public override string ToString()
        {
            return $"{base.ToString()}: Title: {Title}, FileName: {FileName}";
        }

        /// <summary>Raises the <see cref="FileOk"/> event.</summary>
        protected virtual void OnFileOk(CancelEventArgs e)
        {
            FileOk?.Invoke(this, e);
        }

        internal override FileBrowserOptions CreateOptions()
        {
            savedCurrentDirectory = Environment.CurrentDirectory;

            // A folder in FileName takes precedence, like the Windows dialog.
            string? fileDirectory = null;
            string fileName = string.Empty;
            if (FileName.Length > 0)
            {
                try
                {
                    fileDirectory = Path.GetDirectoryName(FileName);
                    fileName = Path.GetFileName(FileName);
                }
                catch (ArgumentException)
                {
                    fileName = FileName;
                }
            }

            return new FileBrowserOptions
            {
                Mode = Mode,
                Title = Title.Length > 0 ? Title : DefaultTitle,
                Multiselect = AllowsMultipleFiles,
                Filters = filters,
                FilterIndex = Math.Clamp(FilterIndex - 1, 0, Math.Max(0, filters.Length - 1)),
                InitialDirectory = ResolveInitialDirectory(fileDirectory, InitialDirectory, GetRecentDirectory(ClientGuid)),
                FileName = fileName,
                SelectName = fileName,
                ShowHiddenFiles = ShowHiddenFiles,
                ShowPinnedPlaces = ShowPinnedPlaces,
                ShowHelp = ShowHelp,
                OkRequiresInteraction = OkRequiresInteraction,
                CustomPlaces = [.. CustomPlaces],
            };
        }

        internal override void OnAccept(FileBrowser browser)
        {
            string text = browser.FileNameText.Trim();
            if (text.Length == 0)
            {
                // With nothing typed, accepting opens the selected folder.
                if (browser.SelectedEntries is [{ IsDirectory: true } folder])
                {
                    browser.Navigate(folder.FullPath);
                }
                return;
            }

            List<string>? names = SplitFileNames(text);
            if (names is null || (names.Count > 1 && !AllowsMultipleFiles))
            {
                browser.ShowError($"{text}\nThe file name is not valid.");
                return;
            }

            if (names.Count == 1)
            {
                string name = names[0];
                if (name.IndexOfAny(['*', '?']) >= 0)
                {
                    browser.ApplyPattern(name);
                    return;
                }

                string? full = browser.ResolvePath(name);
                if (full is not null && Directory.Exists(full))
                {
                    browser.FileNameText = string.Empty;
                    browser.Navigate(full);
                    return;
                }
            }

            var paths = new List<string>(names.Count);
            foreach (string name in names)
            {
                string? error = ResolveFile(browser, name, out string path);
                if (error is not null)
                {
                    browser.ShowError(error);
                    return;
                }
                paths.Add(path);
            }

            ConfirmFiles(browser, paths);
        }

        internal override void OnClosed(FileBrowser browser, DialogResult result)
        {
            if (result == DialogResult.OK)
            {
                SetRecentDirectory(ClientGuid, browser.CurrentDirectory);
            }
            if (RestoreDirectory && savedCurrentDirectory is not null)
            {
                try
                {
                    Environment.CurrentDirectory = savedCurrentDirectory;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                }
            }
        }

        // Asks the user to confirm the accepted files, if needed, then calls AcceptFiles.
        private protected virtual void ConfirmFiles(FileBrowser browser, List<string> paths)
        {
            AcceptFiles(browser, paths);
        }

        // Returns an error message when the resolved file can't be accepted.
        private protected virtual string? ValidateFile(FileBrowser browser, string path)
        {
            return null;
        }

        private protected void AcceptFiles(FileBrowser browser, List<string> paths)
        {
            string[] previous = fileNames;
            fileNames = [.. paths];
            var e = new CancelEventArgs();
            OnFileOk(e);
            if (e.Cancel)
            {
                fileNames = previous;
                return;
            }

            if (filters.Length > 0)
            {
                FilterIndex = browser.FilterIndex + 1;
            }
            browser.Close(DialogResult.OK);
        }

        // Splits "a.txt" or "\"a.txt\" \"b.txt\"" into names; null when the quotes don't pair up.
        internal static List<string>? SplitFileNames(string text)
        {
            if (!text.StartsWith('"'))
            {
                return [text];
            }

            var names = new List<string>();
            int i = 0;
            while (i < text.Length)
            {
                if (char.IsWhiteSpace(text[i]))
                {
                    i++;
                    continue;
                }
                if (text[i] != '"')
                {
                    return null;
                }

                int end = text.IndexOf('"', i + 1);
                if (end < 0)
                {
                    return null;
                }
                string name = text[(i + 1)..end].Trim();
                if (name.Length > 0)
                {
                    names.Add(name);
                }
                i = end + 1;
            }
            return names.Count > 0 ? names : null;
        }

        // Checks each folder and file name in a typed path for characters and names the file system doesn't allow.
        internal static bool IsValidPathText(string text)
        {
            string[] segments = text.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
            char[] invalid = Path.GetInvalidFileNameChars();
            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                if (segment.Length == 0 || segment is "." or "..")
                {
                    continue;
                }
                // Drive letter, such as "C:".
                if (i == 0 && OperatingSystem.IsWindows() && segment.Length == 2 && segment[1] == ':' && char.IsAsciiLetter(segment[0]))
                {
                    continue;
                }
                if (segment.IndexOfAny(invalid) >= 0)
                {
                    return false;
                }
                if (OperatingSystem.IsWindows())
                {
                    string stem = segment.Split('.')[0].TrimEnd();
                    if (WindowsDeviceNames.Contains(stem, StringComparer.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private string? ResolveFile(FileBrowser browser, string name, out string path)
        {
            path = string.Empty;
            if (ValidateNames && !IsValidPathText(name))
            {
                return $"{name}\nThe file name is not valid.";
            }

            string? full = browser.ResolvePath(name);
            if (full is null)
            {
                return $"{name}\nThe file name is not valid.";
            }

            string? directory = Path.GetDirectoryName(full);
            if (CheckPathExists && (directory is null || !Directory.Exists(directory)))
            {
                return $"{directory ?? full}\nPath does not exist.\nCheck the path and try again.";
            }
            if (Directory.Exists(full))
            {
                return $"{Path.GetFileName(full)}\nThis is a folder. Select a file instead.";
            }

            if (AddExtension)
            {
                full = AddDefaultExtension(full, name, browser.SelectedFilter);
            }
            // A trailing dot means "no extension", as in Windows.
            if (full.EndsWith('.') && !full.EndsWith("..", StringComparison.Ordinal))
            {
                full = full[..^1];
            }

            if (DereferenceLinks && File.Exists(full))
            {
                try
                {
                    full = new FileInfo(full).ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? full;
                }
                catch (IOException)
                {
                }
            }

            if (CheckFileExists && !File.Exists(full))
            {
                return $"{Path.GetFileName(full)}\nFile not found.\nCheck the file name and try again.";
            }

            path = full;
            return ValidateFile(browser, full);
        }

        private string AddDefaultExtension(string full, string typedName, FileFilter? selectedFilter)
        {
            string? extension = selectedFilter?.Extension ?? (DefaultExt.Length > 0 ? DefaultExt : null);
            if (extension is null || typedName.EndsWith('.'))
            {
                return full;
            }
            if (!SupportMultiDottedExtensions)
            {
                extension = extension[(extension.LastIndexOf('.') + 1)..];
            }

            string fileName = Path.GetFileName(full);
            if (fileName.EndsWith("." + extension, StringComparison.OrdinalIgnoreCase))
            {
                return full;
            }

            if (Mode == FileBrowserMode.Open)
            {
                // Open the file as typed when it exists, otherwise try it with the extension.
                if (File.Exists(full) || Path.HasExtension(fileName))
                {
                    return full;
                }
                string withExtension = full + "." + extension;
                return File.Exists(withExtension) ? withExtension : full;
            }

            return Path.HasExtension(fileName) ? full : full + "." + extension;
        }
    }
}
