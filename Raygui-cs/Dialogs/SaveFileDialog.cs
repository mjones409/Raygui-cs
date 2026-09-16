namespace RayGui_cs
{
    /// <summary>Prompts the user to select a location for saving a file, modeled on the WinForms class of the same name.</summary>
    public sealed class SaveFileDialog : FileDialog
    {
        public SaveFileDialog()
        {
            Reset();
        }

        /// <summary>Gets or sets whether the dialog asks for permission to create a file that doesn't exist.</summary>
        public bool CreatePrompt { get; set; }

        /// <summary>Gets or sets whether the dialog asks for permission to replace a file that exists.</summary>
        public bool OverwritePrompt { get; set; }

        /// <summary>Gets or sets whether the dialog rejects existing files that can't be written to.</summary>
        public bool CheckWriteAccess { get; set; }

        private protected override FileBrowserMode Mode => FileBrowserMode.Save;

        private protected override string DefaultTitle => "Save As";

        public override void Reset()
        {
            base.Reset();
            CreatePrompt = false;
            OverwritePrompt = true;
            CheckWriteAccess = true;
        }

        /// <summary>Creates, or truncates, the selected file and opens it for reading and writing.</summary>
        /// <exception cref="InvalidOperationException">No file is selected.</exception>
        public Stream OpenFile()
        {
            string fileName = FileName;
            if (fileName.Length == 0)
            {
                throw new InvalidOperationException("No file is selected.");
            }
            return new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
        }

        internal override FileBrowserOptions CreateOptions()
        {
            FileBrowserOptions options = base.CreateOptions();
            options.OkText = "Save";
            options.ShowNewFolderButton = true;
            return options;
        }

        private protected override string? ValidateFile(FileBrowser browser, string path)
        {
            if (!CheckWriteAccess || !File.Exists(path))
            {
                return null;
            }

            if (new FileInfo(path).IsReadOnly)
            {
                return $"{Path.GetFileName(path)}\nThis file is set to read-only.\nTry again with a different file name.";
            }
            try
            {
                using (new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                {
                }
            }
            catch (UnauthorizedAccessException)
            {
                return $"{Path.GetFileName(path)}\nYou don't have permission to save to this file.";
            }
            catch (IOException ex)
            {
                return $"{Path.GetFileName(path)}\n{ex.Message}";
            }
            return null;
        }

        private protected override void ConfirmFiles(FileBrowser browser, List<string> paths)
        {
            string path = paths[0];
            string name = Path.GetFileName(path);
            if (File.Exists(path))
            {
                if (OverwritePrompt)
                {
                    browser.Confirm("Confirm Save As", $"{name} already exists.\nDo you want to replace it?", () => AcceptFiles(browser, paths));
                    return;
                }
            }
            else if (CreatePrompt)
            {
                browser.Confirm("Create File", $"{name} doesn't exist.\nDo you want to create it?", () => AcceptFiles(browser, paths));
                return;
            }

            AcceptFiles(browser, paths);
        }
    }
}
