using Raylib_cs;
using System.Collections.Immutable;
using System.Numerics;

namespace RayGui_cs
{
    /// <summary>
    /// The working state of a file or folder dialog: its folder, history, selection, scroll positions, text boxes and prompt.
    /// Kept in the <c>State</c> of an <see cref="OpenFileDialog"/>, <see cref="SaveFileDialog"/> or <see cref="FolderBrowserDialog"/>.
    /// </summary>
    /// <remarks>
    /// Most programs never touch this. It is public for programs that want fine control, such as saving and restoring a
    /// dialog's folder and history, or answering a prompt from code. The dialog checks these values every frame, so it
    /// copes with any of them changing, but it may adjust values that don't fit, such as selected items that aren't shown.
    /// </remarks>
    public struct FileDialogState
    {
        private ImmutableStack<string?>? backHistory;
        private ImmutableStack<string?>? forwardHistory;
        private ImmutableHashSet<string>? selection;
        private string? typeAhead;
        private string? fileNameText;
        private string? addressText;
        private string? searchText;

        /// <summary>The kind of prompt shown over the dialog.</summary>
        public enum PromptKind
        {
            /// <summary>An error or notice with an OK button.</summary>
            Message,

            /// <summary>
            /// A Yes/No question before accepting files, such as whether to replace a file.
            /// Yes accepts <see cref="PromptState.PendingPaths"/>.
            /// </summary>
            ConfirmAccept,

            /// <summary>Asks for a folder name, and Create makes that folder in the current folder.</summary>
            NewFolder,
        }

        /// <summary>The column the file list is sorted by.</summary>
        public enum SortColumn
        {
            /// <summary>Sorts by name, with numbers in names compared by value.</summary>
            Name,

            /// <summary>Sorts by the date modified.</summary>
            Modified,

            /// <summary>Sorts by the type description.</summary>
            Type,

            /// <summary>Sorts by size.</summary>
            Size,
        }

        /// <summary>What the mouse is dragging.</summary>
        public enum DragMode
        {
            /// <summary>Nothing is dragged.</summary>
            None,

            /// <summary>The title bar is dragged to move the dialog.</summary>
            Move,

            /// <summary>The corner grip is dragged to resize the dialog.</summary>
            Resize,
        }

        /// <summary>A small window shown over the dialog, which blocks the dialog until it is answered.</summary>
        public readonly record struct PromptState
        {
            private static readonly string[] OkButtons = ["OK"];
            private static readonly string[] YesNoButtons = ["Yes", "No"];
            private static readonly string[] CreateButtons = ["Create", "Cancel"];

            private readonly string? title;
            private readonly string? message;
            private readonly ImmutableArray<string> pendingPaths;

            /// <summary>Gets what the prompt asks, which decides its buttons and what they do.</summary>
            public PromptKind Kind { get; init; }

            /// <summary>Gets the title.</summary>
            public string Title { get => title ?? string.Empty; init => title = value; }

            /// <summary>Gets the message; lines are separated by '\n'.</summary>
            public string Message { get => message ?? string.Empty; init => message = value; }

            /// <summary>Gets the text in the prompt's text box, or null when it has none.</summary>
            public string? Text { get; init; }

            /// <summary>Gets whether the text box is being typed in.</summary>
            public bool TextEditing { get; init; }

            /// <summary>Gets the zero based index of the button that Enter clicks.</summary>
            public int DefaultButton { get; init; }

            /// <summary>Gets the dialog frame the prompt opened on; it ignores input on that frame.</summary>
            public long OpenedFrame { get; init; }

            /// <summary>Gets the paths a <see cref="PromptKind.ConfirmAccept"/> prompt accepts when the user answers Yes.</summary>
            public ImmutableArray<string> PendingPaths
            {
                get => pendingPaths.IsDefault ? [] : pendingPaths;
                init => pendingPaths = value;
            }

            internal string[] Buttons => Kind switch
            {
                PromptKind.ConfirmAccept => YesNoButtons,
                PromptKind.NewFolder => CreateButtons,
                _ => OkButtons,
            };

            internal GuiIconName Icon => Kind == PromptKind.NewFolder ? GuiIconName.FolderAdd : GuiIconName.Warning;
        }

        /// <summary>
        /// Gets or sets whether the dialog has opened its initial folder and selected its initial item.
        /// Set it to false to do that again on the next frame.
        /// </summary>
        public bool Initialized { get; set; }

        /// <summary>Gets or sets the number of frames the dialog has been drawn. It ignores input on its first frame.</summary>
        public long Frame { get; set; }

        /// <summary>Gets or sets the folder shown, or null for the drive list. The list reloads when it changes.</summary>
        public string? CurrentDirectory { get; set; }

        /// <summary>Gets or sets the folders the Back button returns to, most recent on top; null stands for the drive list.</summary>
        public ImmutableStack<string?> BackHistory
        {
            readonly get => backHistory ?? ImmutableStack<string?>.Empty;
            set => backHistory = value;
        }

        /// <summary>Gets or sets the folders the Forward button goes to, next on top; null stands for the drive list.</summary>
        public ImmutableStack<string?> ForwardHistory
        {
            readonly get => forwardHistory ?? ImmutableStack<string?>.Empty;
            set => forwardHistory = value;
        }

        /// <summary>Gets or sets the full paths of the selected items. Paths that aren't shown are dropped.</summary>
        public ImmutableHashSet<string> Selection
        {
            readonly get => selection ?? ImmutableHashSet<string>.Empty;
            set => selection = value;
        }

        /// <summary>Gets or sets the full path of the item with the keyboard focus, or null for none.</summary>
        public string? FocusPath { get; set; }

        /// <summary>Gets or sets the full path of the item a Shift selection extends from, or null for none.</summary>
        public string? AnchorPath { get; set; }

        /// <summary>Gets or sets the full path of an item to scroll into view on the next frame, or null for none.</summary>
        public string? ScrollToPath { get; set; }

        /// <summary>Gets or sets the full path of the item last clicked, to detect double clicks.</summary>
        public string? LastClickPath { get; set; }

        /// <summary>Gets or sets when the item was last clicked, in seconds since the window was initialized.</summary>
        public double LastClickTime { get; set; }

        /// <summary>Gets or sets the letters typed so far to jump to an item by name.</summary>
        public string TypeAhead
        {
            readonly get => typeAhead ?? string.Empty;
            set => typeAhead = value;
        }

        /// <summary>Gets or sets when a letter was last typed to jump to an item, in seconds since the window was initialized.</summary>
        public double TypeAheadTime { get; set; }

        /// <summary>Gets or sets whether the user has used the dialog, which enables OK when it requires interaction.</summary>
        public bool Interacted { get; set; }

        /// <summary>Gets or sets whether the dialog accepts once the left mouse button is released, after a double click.</summary>
        public bool AcceptOnRelease { get; set; }

        /// <summary>Gets or sets the text in the file name box (the folder box in a folder dialog).</summary>
        public string FileNameText
        {
            readonly get => fileNameText ?? string.Empty;
            set => fileNameText = value;
        }

        /// <summary>Gets or sets whether the file name box is being typed in.</summary>
        public bool FileNameEditing { get; set; }

        /// <summary>Gets or sets the text in the address box.</summary>
        public string AddressText
        {
            readonly get => addressText ?? string.Empty;
            set => addressText = value;
        }

        /// <summary>Gets or sets whether the address box is being typed in.</summary>
        public bool AddressEditing { get; set; }

        /// <summary>Gets or sets the text in the search box, which shows only items whose names contain it.</summary>
        public string SearchText
        {
            readonly get => searchText ?? string.Empty;
            set => searchText = value;
        }

        /// <summary>Gets or sets whether the search box is being typed in.</summary>
        public bool SearchEditing { get; set; }

        /// <summary>
        /// Gets or sets a wildcard pattern typed in the file name box, such as "*.log", which replaces the selected filter;
        /// null for none. Choosing a filter clears it.
        /// </summary>
        public string? Pattern { get; set; }

        /// <summary>Gets or sets whether the filter dropdown is open.</summary>
        public bool FilterDropdownOpen { get; set; }

        /// <summary>Gets or sets the column the file list is sorted by.</summary>
        public SortColumn SortBy { get; set; }

        /// <summary>Gets or sets whether the file list is sorted in descending order.</summary>
        public bool SortDescending { get; set; }

        /// <summary>Gets or sets the scroll offset of the file list.</summary>
        public Vector2 ListScroll { get; set; }

        /// <summary>Gets or sets the scroll offset of the places list.</summary>
        public Vector2 PlacesScroll { get; set; }

        /// <summary>Gets or sets the visible area of the file list on the last frame, used to page and scroll before it is drawn.</summary>
        public Rectangle ListView { get; set; }

        /// <summary>Gets or sets the prompt shown over the dialog, or null for none.</summary>
        public PromptState? Prompt { get; set; }

        /// <summary>Gets or sets what the mouse is dragging.</summary>
        public DragMode Drag { get; set; }

        /// <summary>Gets or sets the offset from the dragged corner of the dialog to the mouse.</summary>
        public Vector2 DragOffset { get; set; }

        /// <summary>Gets or sets whether the dialog changed the mouse cursor and must restore it.</summary>
        public bool CursorChanged { get; set; }

        /// <summary>Gets or sets the tooltip of the control under the mouse on the last frame, or null for none.</summary>
        public string? LastTooltip { get; set; }

        /// <summary>Gets or sets when the mouse moved onto the control with <see cref="LastTooltip"/>, in seconds since the window was initialized.</summary>
        public double TooltipStart { get; set; }

        // What the dialog loaded from the disk and the options: the folder listing, the sorted and filtered view, the parsed
        // filters and the places, with what each was built from so the dialog can tell when to load or rebuild it. These are
        // internal because a caller changes the values above instead. The lists are replaced, never changed, so copies of a
        // state stay independent.
        internal string? LoadedDirectory;
        internal List<FileEntry>? Entries;
        internal string? LoadError;
        internal DateTime LoadedWriteTime;
        internal double NextRefreshCheck;
        internal List<FileEntry>? Visible;
        internal FileBrowser.ViewKey ViewKey;
        internal string? LoadedFilter;
        internal FileFilter[]? Filters;
        internal FileDialogCustomPlace[]? LoadedCustomPlaces;
        internal List<FileBrowser.Place>? Places;
    }
}
