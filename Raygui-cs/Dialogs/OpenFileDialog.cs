namespace RayGui_cs
{
    /// <summary>Prompts the user to open one or more files, modeled on the WinForms class of the same name.</summary>
    public sealed class OpenFileDialog : FileDialog
    {
        public OpenFileDialog()
        {
            Reset();
        }

        /// <summary>Gets or sets whether the user can select more than one file.</summary>
        public bool Multiselect { get; set; }

        /// <summary>Gets or sets whether the read-only check box is checked. Updated when the user accepts the dialog.</summary>
        public bool ReadOnlyChecked { get; set; }

        /// <summary>Gets or sets whether the dialog has an "Open as read-only" check box.</summary>
        public bool ShowReadOnly { get; set; }

        /// <summary>Gets or sets whether the user can select files that have the read-only attribute.</summary>
        public bool SelectReadOnlyFiles { get; set; }

        /// <summary>Gets the file name and extension of the first selected file, without its folder.</summary>
        public string SafeFileName => Path.GetFileName(FileName);

        /// <summary>Gets the file names and extensions of all selected files, without their folders.</summary>
        public string[] SafeFileNames => FileNames.Select(Path.GetFileName).ToArray()!;

        private protected override FileBrowserMode Mode => FileBrowserMode.Open;

        private protected override string DefaultTitle => "Open";

        private protected override bool AllowsMultipleFiles => Multiselect;

        public override void Reset()
        {
            base.Reset();
            CheckFileExists = true;
            Multiselect = false;
            ReadOnlyChecked = false;
            ShowReadOnly = false;
            SelectReadOnlyFiles = true;
        }

        /// <summary>Opens the first selected file for reading.</summary>
        /// <exception cref="InvalidOperationException">No file is selected.</exception>
        public Stream OpenFile()
        {
            string fileName = FileName;
            if (fileName.Length == 0)
            {
                throw new InvalidOperationException("No file is selected.");
            }
            return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        internal override FileBrowserOptions CreateOptions()
        {
            FileBrowserOptions options = base.CreateOptions();
            options.OkText = "Open";
            options.ShowReadOnly = ShowReadOnly;
            options.ReadOnlyChecked = ReadOnlyChecked;
            return options;
        }

        internal override void OnClosed(FileBrowser browser, DialogResult result)
        {
            base.OnClosed(browser, result);
            if (result == DialogResult.OK && ShowReadOnly)
            {
                ReadOnlyChecked = browser.ReadOnlyChecked;
            }
        }

        private protected override string? ValidateFile(FileBrowser browser, string path)
        {
            if (!SelectReadOnlyFiles && File.Exists(path) && new FileInfo(path).IsReadOnly)
            {
                return $"{Path.GetFileName(path)}\nThis file is read-only.\nSelect a different file.";
            }
            return null;
        }
    }
}
