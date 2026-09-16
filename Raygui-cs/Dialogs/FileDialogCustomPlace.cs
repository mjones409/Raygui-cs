namespace RayGui_cs
{
    /// <summary>A folder shown at the top of the places list of a file dialog.</summary>
    /// <param name="Path">The folder path. Places whose folder doesn't exist are not shown.</param>
    /// <param name="Name">The name shown for the place, or null to show the folder name.</param>
    public readonly record struct FileDialogCustomPlace(string Path, string? Name = null)
    {
        /// <summary>Creates a place for a special folder, such as <see cref="Environment.SpecialFolder.MyPictures"/>.</summary>
        public FileDialogCustomPlace(Environment.SpecialFolder folder, string? name = null)
            : this(Environment.GetFolderPath(folder), name)
        {
        }
    }
}
