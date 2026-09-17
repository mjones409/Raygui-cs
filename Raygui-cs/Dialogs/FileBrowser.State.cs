using Raylib_cs;
using System.Collections.Immutable;
using SortColumn = RayGui_cs.FileDialogState.SortColumn;

namespace RayGui_cs
{
    // How the browser's state gets in and out of the caller's dialog struct: everything it keeps between frames is loaded
    // into fields at the start of the frame and stored back at the end.
    internal sealed partial class FileBrowser
    {
        // What the visible list was built from, so a caller that changes the state, such as the folder or the sort order,
        // gets a rebuilt view on the next frame.
        internal readonly record struct ViewKey(
            List<FileEntry> Entries,
            FileFilter[] Filters,
            int FilterIndex,
            string? Pattern,
            bool ShowHidden,
            string Search,
            SortColumn Sort,
            bool Descending);

        private FileBrowser(FileBrowserMode mode, in FileDialogState state, in FileBrowserOptions options)
        {
            Mode = mode;
            this.options = options;
            Load(state);
        }

        // Draws the dialog for one frame: the state and the options go in, what the user did comes out.
        public static FileBrowserResult Draw(FileBrowserMode mode, ref FileDialogState state, in FileBrowserOptions options)
        {
            if (!Raylib.IsWindowReady())
            {
                throw new InvalidOperationException("The raylib window must be initialized before a dialog is drawn.");
            }

            var browser = new FileBrowser(mode, state, options);
            FileBrowserResult result = browser.DrawFrame();
            browser.Store(ref state);
            return result;
        }

        private void Load(in FileDialogState state)
        {
            initialized = state.Initialized;
            frame = state.Frame;
            CurrentDirectory = state.CurrentDirectory;
            backHistory = state.BackHistory;
            forwardHistory = state.ForwardHistory;

            selection = state.Selection.WithComparer(PathComparer);
            focusPath = state.FocusPath;
            anchorPath = state.AnchorPath;
            scrollToPath = state.ScrollToPath;
            lastClickPath = state.LastClickPath;
            lastClickTime = state.LastClickTime;
            typeAhead = state.TypeAhead;
            typeAheadTime = state.TypeAheadTime;
            interacted = state.Interacted;
            acceptOnRelease = state.AcceptOnRelease;

            fileNameText = state.FileNameText;
            fileNameEditing = state.FileNameEditing;
            addressText = state.AddressText;
            addressEditing = state.AddressEditing;
            searchText = state.SearchText;
            searchEditing = state.SearchEditing;
            pattern = state.Pattern;
            filterOpen = state.FilterDropdownOpen;
            sortColumn = state.SortBy;
            sortDescending = state.SortDescending;
            listScroll = state.ListScroll;
            placesScroll = state.PlacesScroll;
            listView = state.ListView;
            prompt = state.Prompt;

            drag = state.Drag;
            dragOffset = state.DragOffset;
            cursorChanged = state.CursorChanged;
            lastTooltip = state.LastTooltip;
            tooltipStart = state.TooltipStart;

            // The values the caller owns but the user can change.
            filterIndex = options.FilterIndex;
            showHidden = options.ShowHiddenFiles;
            readOnlyChecked = options.ReadOnlyChecked;

            loadedDirectory = state.LoadedDirectory;
            entries = state.Entries ?? [];
            loadError = state.LoadError;
            loadedWriteTime = state.LoadedWriteTime;
            nextRefreshCheck = state.NextRefreshCheck;
            visible = state.Visible ?? [];
            viewKey = state.ViewKey;
            loadedFilter = state.LoadedFilter;
            filters = state.Filters ?? [];
            loadedCustomPlaces = state.LoadedCustomPlaces ?? [];
            places = state.Places;
        }

        private void Store(ref FileDialogState state)
        {
            state.Initialized = initialized;
            state.Frame = frame;
            state.CurrentDirectory = CurrentDirectory;
            state.BackHistory = backHistory;
            state.ForwardHistory = forwardHistory;

            state.Selection = selection;
            state.FocusPath = focusPath;
            state.AnchorPath = anchorPath;
            state.ScrollToPath = scrollToPath;
            state.LastClickPath = lastClickPath;
            state.LastClickTime = lastClickTime;
            state.TypeAhead = typeAhead;
            state.TypeAheadTime = typeAheadTime;
            state.Interacted = interacted;
            state.AcceptOnRelease = acceptOnRelease;

            state.FileNameText = fileNameText;
            state.FileNameEditing = fileNameEditing;
            state.AddressText = addressText;
            state.AddressEditing = addressEditing;
            state.SearchText = searchText;
            state.SearchEditing = searchEditing;
            state.Pattern = pattern;
            state.FilterDropdownOpen = filterOpen;
            state.SortBy = sortColumn;
            state.SortDescending = sortDescending;
            state.ListScroll = listScroll;
            state.PlacesScroll = placesScroll;
            state.ListView = listView;
            state.Prompt = prompt;

            state.Drag = drag;
            state.DragOffset = dragOffset;
            state.CursorChanged = cursorChanged;
            state.LastTooltip = lastTooltip;
            state.TooltipStart = tooltipStart;

            state.LoadedDirectory = loadedDirectory;
            state.Entries = entries;
            state.LoadError = loadError;
            state.LoadedWriteTime = loadedWriteTime;
            state.NextRefreshCheck = nextRefreshCheck;
            state.Visible = visible;
            state.ViewKey = viewKey;
            state.LoadedFilter = loadedFilter;
            state.Filters = filters;
            state.LoadedCustomPlaces = loadedCustomPlaces;
            state.Places = places;
        }
    }
}
