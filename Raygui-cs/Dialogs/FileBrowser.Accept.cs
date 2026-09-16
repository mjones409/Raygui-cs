namespace RayGui_cs
{
    // What happens when the user accepts the dialog: the typed names are checked, and valid paths go into this frame's result.
    internal sealed partial class FileBrowser
    {
        private static readonly string[] WindowsDeviceNames =
        [
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
        ];

        // Accepts the typed or selected names when they are valid; otherwise shows why they aren't.
        private void OnAccept()
        {
            if (FolderMode)
            {
                AcceptFolders();
            }
            else
            {
                AcceptFiles();
            }
        }

        private void AcceptFiles()
        {
            string text = fileNameText.Trim();
            if (text.Length == 0)
            {
                // With nothing typed, accepting opens the selected folder.
                if (SelectedEntries is [{ IsDirectory: true } folder])
                {
                    Navigate(folder.FullPath);
                }
                return;
            }

            List<string>? names = SplitFileNames(text);
            if (names is null || (names.Count > 1 && !options.Multiselect))
            {
                ShowError($"{text}\nThe file name is not valid.");
                return;
            }

            if (names.Count == 1)
            {
                string name = names[0];
                if (name.IndexOfAny(['*', '?']) >= 0)
                {
                    ApplyPattern(name);
                    return;
                }

                string? full = ResolvePath(name);
                if (full is not null && Directory.Exists(full))
                {
                    fileNameText = string.Empty;
                    Navigate(full);
                    return;
                }
            }

            var paths = new List<string>(names.Count);
            foreach (string name in names)
            {
                string? error = ResolveFile(name, out string path);
                if (error is not null)
                {
                    ShowError(error);
                    return;
                }
                paths.Add(path);
            }

            ConfirmFiles(paths);
        }

        private void AcceptFolders()
        {
            // The text box holds the selected folders' names, or what the user typed over them.
            string text = fileNameText.Trim();
            List<string> paths;

            if (text.Length > 0)
            {
                List<string>? names = SplitFileNames(text);
                if (names is null || (names.Count > 1 && !options.Multiselect))
                {
                    ShowError($"{text}\nThe folder name is not valid.");
                    return;
                }

                paths = [];
                foreach (string name in names)
                {
                    string? full = ResolvePath(name);
                    if (full is null || !IsValidPathText(name))
                    {
                        ShowError($"{name}\nThe folder name is not valid.");
                        return;
                    }
                    if (!Directory.Exists(full))
                    {
                        ShowError($"{full}\nThe folder doesn't exist.\nCheck the name and try again.");
                        return;
                    }
                    paths.Add(Path.TrimEndingDirectorySeparator(full));
                }
            }
            else if (CurrentDirectory is { } current)
            {
                paths = [current];
            }
            else
            {
                // The drive list has no folder of its own to select.
                return;
            }

            acceptedPaths = paths;
        }

        // Asks the user to confirm the files when the options say so, then accepts them.
        private void ConfirmFiles(List<string> paths)
        {
            if (Mode == FileBrowserMode.Save)
            {
                string name = Path.GetFileName(paths[0]);
                if (File.Exists(paths[0]))
                {
                    if (options.OverwritePrompt)
                    {
                        Confirm("Confirm Save As", $"{name} already exists.\nDo you want to replace it?", () => acceptedPaths = paths);
                        return;
                    }
                }
                else if (options.CreatePrompt)
                {
                    Confirm("Create File", $"{name} doesn't exist.\nDo you want to create it?", () => acceptedPaths = paths);
                    return;
                }
            }

            acceptedPaths = paths;
        }

        // Returns an error message when a resolved file can't be accepted.
        private string? ValidateFile(string path)
        {
            if (Mode == FileBrowserMode.Open)
            {
                if (!options.SelectReadOnlyFiles && File.Exists(path) && new FileInfo(path).IsReadOnly)
                {
                    return $"{Path.GetFileName(path)}\nThis file is read-only.\nSelect a different file.";
                }
                return null;
            }

            if (!options.CheckWriteAccess || !File.Exists(path))
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

        // Splits "a.txt" or "\"a.txt\" \"b.txt\"" into names; null when the quotes don't pair up.
        private static List<string>? SplitFileNames(string text)
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
        private static bool IsValidPathText(string text)
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

        // Resolves a typed file name to a full path and applies the options' checks; returns an error message when it fails them.
        private string? ResolveFile(string name, out string path)
        {
            path = string.Empty;
            if (options.ValidateNames && !IsValidPathText(name))
            {
                return $"{name}\nThe file name is not valid.";
            }

            string? full = ResolvePath(name);
            if (full is null)
            {
                return $"{name}\nThe file name is not valid.";
            }

            string? directory = Path.GetDirectoryName(full);
            if (options.CheckPathExists && (directory is null || !Directory.Exists(directory)))
            {
                return $"{directory ?? full}\nPath does not exist.\nCheck the path and try again.";
            }
            if (Directory.Exists(full))
            {
                return $"{Path.GetFileName(full)}\nThis is a folder. Select a file instead.";
            }

            if (options.AddExtension)
            {
                full = AddDefaultExtension(full, name);
            }
            // A trailing dot means "no extension", as in Windows.
            if (full.EndsWith('.') && !full.EndsWith("..", StringComparison.Ordinal))
            {
                full = full[..^1];
            }

            if (options.DereferenceLinks && File.Exists(full))
            {
                try
                {
                    full = new FileInfo(full).ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? full;
                }
                catch (IOException)
                {
                }
            }

            if (options.CheckFileExists && !File.Exists(full))
            {
                return $"{Path.GetFileName(full)}\nFile not found.\nCheck the file name and try again.";
            }

            path = full;
            return ValidateFile(full);
        }

        private string AddDefaultExtension(string full, string typedName)
        {
            string? extension = SelectedFilter?.Extension ?? options.DefaultExt;
            if (extension is null || typedName.EndsWith('.'))
            {
                return full;
            }
            if (!options.SupportMultiDottedExtensions)
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
