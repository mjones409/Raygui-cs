using Raylib_cs;
using System.Globalization;

namespace RayGui_cs
{
    internal enum FileBrowserMode
    {
        Open,
        Save,
        Folder,
    }

    // What the browser reads from the public dialog struct each frame: the options, and the values the user can change.
    internal readonly struct FileBrowserOptions
    {
        public Rectangle Bounds { get; init; }
        public string? InitialDirectory { get; init; }
        public string? InitialFileName { get; init; }
        public int FilterIndex { get; init; }
        public bool ReadOnlyChecked { get; init; }
        public bool ShowHiddenFiles { get; init; }

        public required string Title { get; init; }
        public string? Description { get; init; }
        public required string OkText { get; init; }
        public bool Multiselect { get; init; }
        // Whether the file dialogs accept a chosen folder instead of only opening it.
        public bool AllowFolders { get; init; }
        public string? Filter { get; init; }
        public bool DimBackground { get; init; }
        public bool ShowNewFolderButton { get; init; }
        public bool ShowPinnedPlaces { get; init; }
        public bool ShowReadOnly { get; init; }
        public bool ShowHelp { get; init; }
        public bool OkRequiresInteraction { get; init; }
        public IReadOnlyList<FileDialogCustomPlace>? CustomPlaces { get; init; }

        // Checks on accepted file names, which the folder dialog doesn't use.
        public bool AddExtension { get; init; }
        public bool CheckFileExists { get; init; }
        public bool CheckPathExists { get; init; }
        public string? DefaultExt { get; init; }
        public bool DereferenceLinks { get; init; }
        public bool SupportMultiDottedExtensions { get; init; }
        public bool ValidateNames { get; init; }
        public bool SelectReadOnlyFiles { get; init; }
        public bool CheckWriteAccess { get; init; }
        public bool OverwritePrompt { get; init; }
        public bool CreatePrompt { get; init; }
    }

    // What the browser gives back to the public dialog struct each frame.
    internal readonly struct FileBrowserResult
    {
        public required Rectangle Bounds { get; init; }
        public int FilterIndex { get; init; }
        public bool ReadOnlyChecked { get; init; }
        public bool ShowHiddenFiles { get; init; }

        // The paths accepted this frame, or null for none.
        public string[]? Paths { get; init; }
        public bool Canceled { get; init; }
        public bool HelpClicked { get; init; }
    }

    // A file, folder or drive shown in the browser.
    internal sealed record FileEntry(string Name, string FullPath, bool IsDirectory, long? Size, DateTime? Modified, string TypeName, GuiIconName Icon)
    {
        public bool IsDrive { get; init; }
        public bool IsHidden { get; init; }

        private static readonly Dictionary<string, GuiIconName> IconsByExtension = new(StringComparer.OrdinalIgnoreCase)
        {
            [".txt"] = GuiIconName.FiletypeText, [".md"] = GuiIconName.FiletypeText, [".log"] = GuiIconName.FiletypeText,
            [".ini"] = GuiIconName.FiletypeText, [".cfg"] = GuiIconName.FiletypeText, [".csv"] = GuiIconName.FiletypeText,
            [".rtf"] = GuiIconName.FiletypeText, [".doc"] = GuiIconName.FiletypeText, [".docx"] = GuiIconName.FiletypeText,
            [".pdf"] = GuiIconName.FiletypeText,
            [".png"] = GuiIconName.FiletypeImage, [".jpg"] = GuiIconName.FiletypeImage, [".jpeg"] = GuiIconName.FiletypeImage,
            [".bmp"] = GuiIconName.FiletypeImage, [".gif"] = GuiIconName.FiletypeImage, [".tga"] = GuiIconName.FiletypeImage,
            [".psd"] = GuiIconName.FiletypeImage, [".hdr"] = GuiIconName.FiletypeImage, [".qoi"] = GuiIconName.FiletypeImage,
            [".dds"] = GuiIconName.FiletypeImage, [".ktx"] = GuiIconName.FiletypeImage, [".webp"] = GuiIconName.FiletypeImage,
            [".svg"] = GuiIconName.FiletypeImage, [".tif"] = GuiIconName.FiletypeImage, [".tiff"] = GuiIconName.FiletypeImage,
            [".ico"] = GuiIconName.FiletypeIcon,
            [".wav"] = GuiIconName.FiletypeAudio, [".mp3"] = GuiIconName.FiletypeAudio, [".ogg"] = GuiIconName.FiletypeAudio,
            [".flac"] = GuiIconName.FiletypeAudio, [".qoa"] = GuiIconName.FiletypeAudio, [".xm"] = GuiIconName.FiletypeAudio,
            [".mod"] = GuiIconName.FiletypeAudio, [".m4a"] = GuiIconName.FiletypeAudio, [".aac"] = GuiIconName.FiletypeAudio,
            [".mp4"] = GuiIconName.FiletypeVideo, [".mkv"] = GuiIconName.FiletypeVideo, [".avi"] = GuiIconName.FiletypeVideo,
            [".mov"] = GuiIconName.FiletypeVideo, [".webm"] = GuiIconName.FiletypeVideo, [".wmv"] = GuiIconName.FiletypeVideo,
            [".ttf"] = GuiIconName.FiletypeFont, [".otf"] = GuiIconName.FiletypeFont, [".fnt"] = GuiIconName.FiletypeFont,
            [".obj"] = GuiIconName.Filetype3D, [".gltf"] = GuiIconName.Filetype3D, [".glb"] = GuiIconName.Filetype3D,
            [".fbx"] = GuiIconName.Filetype3D, [".iqm"] = GuiIconName.Filetype3D, [".m3d"] = GuiIconName.Filetype3D,
            [".vox"] = GuiIconName.Filetype3D,
            [".xml"] = GuiIconName.FiletypeCodeXml, [".html"] = GuiIconName.FiletypeCodeXml, [".htm"] = GuiIconName.FiletypeCodeXml,
            [".json"] = GuiIconName.FiletypeCodeXml, [".yaml"] = GuiIconName.FiletypeCodeXml, [".yml"] = GuiIconName.FiletypeCodeXml,
            [".csproj"] = GuiIconName.FiletypeCodeXml, [".slnx"] = GuiIconName.FiletypeCodeXml, [".rgs"] = GuiIconName.FiletypeCodeXml,
            [".c"] = GuiIconName.FiletypeCodeC, [".h"] = GuiIconName.FiletypeCodeC, [".cpp"] = GuiIconName.FiletypeCodeC,
            [".hpp"] = GuiIconName.FiletypeCodeC, [".cs"] = GuiIconName.FiletypeCodeC, [".rs"] = GuiIconName.FiletypeCodeC,
            [".go"] = GuiIconName.FiletypeCodeC, [".java"] = GuiIconName.FiletypeCodeC,
            [".py"] = GuiIconName.FiletypeCodePython,
            [".js"] = GuiIconName.FiletypeCodeJs, [".ts"] = GuiIconName.FiletypeCodeJs,
            [".exe"] = GuiIconName.FiletypeBinary, [".dll"] = GuiIconName.FiletypeBinary, [".so"] = GuiIconName.FiletypeBinary,
            [".dylib"] = GuiIconName.FiletypeBinary, [".bin"] = GuiIconName.FiletypeBinary, [".dat"] = GuiIconName.FiletypeBinary,
            [".rgi"] = GuiIconName.FiletypeBinary,
            [".zip"] = GuiIconName.SuitcaseZip, [".7z"] = GuiIconName.SuitcaseZip, [".rar"] = GuiIconName.SuitcaseZip,
            [".gz"] = GuiIconName.SuitcaseZip, [".tar"] = GuiIconName.SuitcaseZip, [".xz"] = GuiIconName.SuitcaseZip,
        };

        public static FileEntry FromInfo(FileSystemInfo info)
        {
            bool isHidden = (info.Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0
                || (!OperatingSystem.IsWindows() && info.Name.StartsWith('.'));

            if (info is FileInfo file)
            {
                string extension = file.Extension;
                string typeName = extension.Length > 1 ? $"{extension[1..].ToUpperInvariant()} File" : "File";
                return new FileEntry(file.Name, file.FullName, false, file.Length, file.LastWriteTime, typeName,
                    IconsByExtension.GetValueOrDefault(extension, GuiIconName.File))
                {
                    IsHidden = isHidden,
                };
            }

            return new FileEntry(info.Name, info.FullName, true, null, info.LastWriteTime, "File folder", GuiIconName.Folder)
            {
                IsHidden = isHidden,
            };
        }

        // Returns null for drives that aren't ready.
        public static FileEntry? FromDrive(DriveInfo drive)
        {
            try
            {
                if (!drive.IsReady)
                {
                    return null;
                }

                string letter = drive.Name.TrimEnd(Path.DirectorySeparatorChar);
                string label = drive.VolumeLabel;
                if (string.IsNullOrWhiteSpace(label))
                {
                    label = DriveTypeName(drive.DriveType);
                }
                return new FileEntry($"{label} ({letter})", drive.RootDirectory.FullName, true, drive.TotalSize, null,
                    DriveTypeName(drive.DriveType), GuiIconName.Rom)
                {
                    IsDrive = true,
                };
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
            {
                return null;
            }
        }

        public static string FormatSize(long bytes)
        {
            const double KB = 1024, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
            return bytes switch
            {
                1 => "1 byte",
                < 1024 => $"{bytes} bytes",
                < 1024 * 1024 => $"{Math.Ceiling(bytes / KB).ToString("N0", CultureInfo.CurrentCulture)} KB",
                < 1024L * 1024 * 1024 => $"{(bytes / MB).ToString("N1", CultureInfo.CurrentCulture)} MB",
                < 1024L * 1024 * 1024 * 1024 => $"{(bytes / GB).ToString("N1", CultureInfo.CurrentCulture)} GB",
                _ => $"{(bytes / TB).ToString("N1", CultureInfo.CurrentCulture)} TB",
            };
        }

        private static string DriveTypeName(DriveType type) => type switch
        {
            DriveType.Fixed => "Local Disk",
            DriveType.Removable => "Removable Disk",
            DriveType.Network => "Network Drive",
            DriveType.CDRom => "CD Drive",
            DriveType.Ram => "RAM Disk",
            _ => "Drive",
        };
    }
}
