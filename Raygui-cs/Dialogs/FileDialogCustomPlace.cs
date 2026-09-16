using System.Collections.ObjectModel;

namespace RayGui_cs
{
    /// <summary>A folder shown in the places list of a file dialog.</summary>
    public sealed class FileDialogCustomPlace
    {
        public FileDialogCustomPlace(string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            Path = path;
        }

        /// <summary>Creates a place for a special folder, such as <see cref="Environment.SpecialFolder.MyPictures"/>.</summary>
        public FileDialogCustomPlace(Environment.SpecialFolder folder)
            : this(Environment.GetFolderPath(folder))
        {
        }

        /// <summary>Gets or sets the folder path. Places whose folder doesn't exist are not shown.</summary>
        public string Path { get; set; }

        /// <summary>Gets or sets the name shown for the place, or null to show the folder name.</summary>
        public string? Name { get; set; }

        public override string ToString() => $"{GetType().Name} Path: {Path}";
    }

    /// <summary>The custom places of a file dialog.</summary>
    public sealed class FileDialogCustomPlacesCollection : Collection<FileDialogCustomPlace>
    {
        public void Add(string path) => Add(new FileDialogCustomPlace(path));

        public void Add(Environment.SpecialFolder folder) => Add(new FileDialogCustomPlace(folder));
    }
}
