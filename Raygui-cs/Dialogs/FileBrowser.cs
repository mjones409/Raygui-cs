using Raylib_cs;
using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using System.Security;
using DragMode = RayGui_cs.FileDialogState.DragMode;
using PromptKind = RayGui_cs.FileDialogState.PromptKind;
using PromptState = RayGui_cs.FileDialogState.PromptState;
using SortColumn = RayGui_cs.FileDialogState.SortColumn;

namespace RayGui_cs
{
    // The window shared by the file and folder dialogs: toolbar, places, file list, file name row and buttons.
    // Kept in a FileDialogState and drawn every frame with that frame's bounds and options.
    internal sealed partial class FileBrowser
    {
        // Must match RAYGUI_WINDOWBOX_STATUSBAR_HEIGHT and RAYGUI_WINDOWBOX_CLOSEBUTTON_HEIGHT in the native build.
        private const int TitleBarHeight = 24;
        private const int CloseButtonSize = 18;

        private const int TextMaxBytes = 4096;
        private const double DoubleClickSeconds = 0.5;
        private const double TypeAheadSeconds = 1.0;
        private const double RefreshSeconds = 1.5;
        private const double TooltipSeconds = 0.5;
        private const string Ellipsis = "...";

        private static readonly StringComparer PathComparer =
            OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        private readonly FileBrowserOptions options;
        private readonly Cache cache;
        private bool initialized;

        // Loaded from the options, and given back so the caller sees what the user changed.
        private int filterIndex;
        private bool showHidden;
        private bool readOnlyChecked;

        // This frame's result.
        private List<string>? acceptedPaths;
        private bool canceled;
        private bool helpClicked;

        // The parsed filters and the places, kept until the options they were built from change.
        private string? loadedFilter;
        private FileFilter[] filters = [];
        private FileDialogCustomPlace[] loadedCustomPlaces = [];
        private List<Place>? places;

        // Navigation. The listing and the view are kept until the folder, the filters or the sort order change.
        private ImmutableStack<string?> backHistory = ImmutableStack<string?>.Empty;
        private ImmutableStack<string?> forwardHistory = ImmutableStack<string?>.Empty;
        private List<FileEntry> entries = [];
        private List<FileEntry> visible = [];
        private ViewKey viewKey;
        private string? loadedDirectory;
        private string? loadError;
        private DateTime loadedWriteTime;
        private double nextRefreshCheck;

        // Selection, by full path so it survives sorting and refreshing.
        private ImmutableHashSet<string> selection = ImmutableHashSet.Create(PathComparer);
        private string? focusPath;
        private string? anchorPath;
        private string? lastClickPath;
        private double lastClickTime;
        private string typeAhead = string.Empty;
        private double typeAheadTime;
        private bool interacted;
        // A double-clicked file is accepted when the button is released, so the release doesn't reach the UI behind the dialog.
        private bool acceptOnRelease;

        // Controls.
        private string fileNameText = string.Empty;
        private bool fileNameEditing;
        private string addressText = string.Empty;
        private bool addressEditing;
        private string searchText = string.Empty;
        private bool searchEditing;
        private bool filterOpen;
        // A wildcard pattern typed in the file name box, such as "*.log", which replaces the selected filter.
        private string? pattern;
        private SortColumn sortColumn = SortColumn.Name;
        private bool sortDescending;
        private Vector2 listScroll;
        private Vector2 placesScroll;
        private Rectangle listView;
        private string? scrollToPath;
        private PromptState? prompt;
        private long frame;

        // Window. The dialog is drawn at bounds, and nextBounds is where the user moved or resized it to.
        private Rectangle bounds;
        private Rectangle nextBounds;
        private bool callerLocked;
        private bool locked;
        private DragMode drag;
        private Vector2 dragOffset;
        private bool cursorChanged;
        private string? tooltip;
        private string? lastTooltip;
        private double tooltipStart;

        // Metrics and colors, read from the style each frame.
        private Font font;
        private int textSize;
        private int textSpacing;
        private int borderWidth;
        private int iconScale;
        private int iconSize;
        private int rowHeight;
        private int controlHeight;
        private int padding;
        private float alpha;

        public FileBrowserMode Mode { get; }

        // The folder shown, or null for the drive list.
        public string? CurrentDirectory { get; private set; }

        public string FileNameText
        {
            get => fileNameText;
            set => fileNameText = value;
        }

        // The filter chosen in the dropdown, ignoring any typed wildcard pattern.
        public FileFilter? SelectedFilter => filters.Length > 0 ? filters[filterIndex] : null;

        public List<FileEntry> SelectedEntries => visible.Where(e => selection.Contains(e.FullPath)).ToList();

        private bool FolderMode => Mode == FileBrowserMode.Folder;

        private bool AnyEditing => fileNameEditing || addressEditing || searchEditing;

        #region Public operations

        // Shows a folder, or the drive list when path is null. Returns false, after showing an error, when it can't be read.
        public bool Navigate(string? path, bool record = true)
        {
            string? full = null;
            List<FileEntry> loaded;
            try
            {
                if (path is not null)
                {
                    full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
                }
                loaded = ReadDirectory(full);
            }
            catch (Exception ex) when (IsFileSystemError(ex))
            {
                ShowError($"{full ?? path}\n{DescribeError(ex)}");
                return false;
            }

            if (record && !SamePath(CurrentDirectory, full))
            {
                backHistory = backHistory.Push(CurrentDirectory);
                forwardHistory = ImmutableStack<string?>.Empty;
            }

            CurrentDirectory = full;
            loadedDirectory = full;
            entries = loaded;
            loadError = null;
            loadedWriteTime = GetWriteTime(full);
            nextRefreshCheck = Raylib.GetTime() + RefreshSeconds;
            selection = selection.Clear();
            focusPath = null;
            anchorPath = null;
            listScroll = default;
            searchText = string.Empty;
            typeAhead = string.Empty;
            addressEditing = false;
            addressText = full ?? string.Empty;
            if (FolderMode && !fileNameEditing)
            {
                fileNameText = string.Empty;
            }
            RefreshView();
            return true;
        }

        // Filters the list with a typed wildcard pattern, such as "*.log" or "logs\*.txt".
        public void ApplyPattern(string text)
        {
            string? directory = Path.GetDirectoryName(text);
            string name = Path.GetFileName(text);
            if (!string.IsNullOrEmpty(directory))
            {
                string? full = ResolvePath(directory);
                if (full is null || !Navigate(full))
                {
                    if (full is null)
                    {
                        ShowError($"{directory}\nThe folder name is not valid.");
                    }
                    return;
                }
            }

            pattern = name;
            fileNameText = name;
            RefreshView();
        }

        // Resolves a typed name against the current folder, expanding environment variables and "~". Null when it isn't a valid path.
        public string? ResolvePath(string name)
        {
            name = Environment.ExpandEnvironmentVariables(name.Trim());
            if (name == "~" || name.StartsWith("~/", StringComparison.Ordinal) || name.StartsWith("~\\", StringComparison.Ordinal))
            {
                name = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + name[1..];
            }

            try
            {
                if (!Path.IsPathFullyQualified(name))
                {
                    if (CurrentDirectory is null)
                    {
                        // In the drive list only drive roots, such as "C:" or "C:\", mean something.
                        return Path.IsPathRooted(name) ? Path.GetFullPath(name) : null;
                    }
                    name = Path.Combine(CurrentDirectory, name);
                }
                return Path.GetFullPath(name);
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or SecurityException)
            {
                return null;
            }
        }

        public void ShowError(string message)
        {
            ShowPrompt(new PromptState { Kind = PromptKind.Message, Title = options.Title, Message = message });
        }

        // Asks a yes/no question, and accepts the paths when the user answers yes.
        // No is the default, so Enter doesn't overwrite by accident.
        public void ConfirmAccept(string title, string message, List<string> paths)
        {
            ShowPrompt(new PromptState
            {
                Kind = PromptKind.ConfirmAccept,
                Title = title,
                Message = message,
                DefaultButton = 1,
                PendingPaths = [.. paths],
            });
        }

        public void ReleaseCursor()
        {
            if (cursorChanged)
            {
                Raylib.SetMouseCursor(MouseCursor.Default);
                cursorChanged = false;
            }
        }

        #endregion

        #region Frame

        private FileBrowserResult DrawFrame()
        {
            frame++;
            tooltip = null;
            acceptedPaths = null;
            canceled = false;
            helpClicked = false;
            // A locked gui locks the dialog too, and the input that opened the dialog must not also act on it.
            callerLocked = Gui.IsLocked;
            locked = callerLocked || frame == 1;

            UpdateMetrics();
            // Empty bounds, from a dialog the caller didn't place, open it centered on the screen.
            bounds = ClampBounds(options.Bounds is { Width: > 0, Height: > 0 } placed ? placed : DefaultBounds());
            nextBounds = bounds;
            ApplyOptions();
            if (!initialized)
            {
                Initialize();
            }

            try
            {
                DrawWindow();
            }
            finally
            {
                Gui.IsLocked = callerLocked;
            }

            if (acceptedPaths is not null || canceled)
            {
                ReleaseCursor();
            }
            return new FileBrowserResult
            {
                Bounds = nextBounds,
                FilterIndex = filterIndex,
                ReadOnlyChecked = readOnlyChecked,
                ShowHiddenFiles = showHidden,
                Paths = acceptedPaths?.ToArray(),
                Canceled = canceled,
                HelpClicked = helpClicked,
            };
        }

        private void DrawWindow()
        {
            if (!locked && prompt is null && !filterOpen)
            {
                HandleWindowDrag();
                HandleKeyboard();
            }
            else if (!locked && filterOpen && Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                filterOpen = false;
            }
            AutoRefresh();

            if (acceptOnRelease && !Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                acceptOnRelease = false;
                if (!locked && prompt is null)
                {
                    Accept();
                }
            }

            bool modal = prompt is not null;
            bool blockMain = locked || modal || filterOpen;

            if (options.DimBackground)
            {
                Raylib.DrawRectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), Fade(Color.Black, 0.35f));
            }

            Gui.IsLocked = blockMain;
            if (Gui.WindowBox(bounds, options.Title))
            {
                canceled = true;
            }

            // Layout, top to bottom: toolbar, description, places and list, file name row, button row.
            float x = bounds.X + padding;
            float width = bounds.Width - padding * 2;
            float y = bounds.Y + TitleBarHeight + padding;

            DrawToolbar(new Rectangle(x, y, width, controlHeight), !blockMain);
            y += controlHeight + padding;

            if (options.Description is { } description)
            {
                float height = textSize + 4;
                DrawText(Fit(description, width), x, y + 2, StyleColor(GuiControl.Label, GuiControlProperty.TextColorNormal));
                y += height + padding;
            }

            float buttonRowY = bounds.Y + bounds.Height - padding - controlHeight;
            float nameRowY = buttonRowY - padding - controlHeight;
            float mainHeight = nameRowY - padding - y;

            float listX = x;
            float placesWidth = Math.Max(textSize * 11, 120);
            if (options.ShowPinnedPlaces && width >= placesWidth * 2.5f)
            {
                DrawPlaces(new Rectangle(x, y, placesWidth, mainHeight), !blockMain);
                listX += placesWidth + padding;
            }
            DrawFileList(new Rectangle(listX, y, x + width - listX, mainHeight), !blockMain);

            Rectangle filterBounds = DrawNameRow(new Rectangle(x, nameRowY, width, controlHeight));
            DrawButtonRow(new Rectangle(x, buttonRowY, width, controlHeight));
            DrawResizeGrip();

            // Drawn last so its open list covers the other controls.
            Gui.IsLocked = locked || modal;
            if (filterBounds.Width > 0)
            {
                DrawFilterDropdown(filterBounds);
            }

            Gui.IsLocked = locked;
            if (prompt is { } current)
            {
                DrawPrompt(current);
            }
            DrawTooltip();
        }

        // Rebuilds whatever this frame's options and state no longer match: the filters, the places, the listing and the view.
        private void ApplyOptions()
        {
            if (options.Filter != loadedFilter)
            {
                filters = FileFilter.Parse(options.Filter);
                loadedFilter = options.Filter;
            }
            filterIndex = filters.Length > 0 ? Math.Min(filterIndex, filters.Length - 1) : 0;
            if (filterIndex != viewKey.FilterIndex)
            {
                // A filter chosen by the caller replaces a typed wildcard pattern.
                pattern = null;
            }

            IReadOnlyList<FileDialogCustomPlace> customPlaces = options.CustomPlaces ?? [];
            if (options.ShowPinnedPlaces && (places is null || !customPlaces.SequenceEqual(loadedCustomPlaces)))
            {
                loadedCustomPlaces = [.. customPlaces];
                LoadPlaces();
            }

            if (!initialized)
            {
                return;
            }
            if (!string.Equals(loadedDirectory, CurrentDirectory, StringComparison.Ordinal))
            {
                // The caller navigated by setting the folder in the state.
                Refresh();
            }
            else if (ViewKeyNow != viewKey)
            {
                RefreshView();
            }
        }

        // What the visible list should be built from this frame.
        private ViewKey ViewKeyNow => new(entries, filters, filterIndex, pattern, showHidden, searchText, sortColumn, sortDescending);

        // Opens the state's initial folder on the first frame, once the options are known.
        private void Initialize()
        {
            initialized = true;
            LoadPlaces();

            // A folder in the initial file name takes precedence over the initial directory.
            string? fileName = options.InitialFileName;
            string? nameDirectory = null;
            string name = string.Empty;
            if (!string.IsNullOrEmpty(fileName))
            {
                nameDirectory = Path.GetDirectoryName(fileName);
                name = Path.GetFileName(fileName);
                if (nameDirectory is null && FolderMode)
                {
                    // A root, such as "C:\" or "/", is selected as it is.
                    name = fileName;
                }
            }

            // A drive selected in the folder dialog is shown in the drive list.
            bool driveList = OperatingSystem.IsWindows() && FolderMode && nameDirectory is null && name.Length > 0;
            if (!Navigate(driveList ? null : ResolveInitialDirectory(nameDirectory, options.InitialDirectory), record: false))
            {
                // Dismiss the error for the initial folder and fall back to one that can be read.
                prompt = null;
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!Navigate(home, record: false))
                {
                    prompt = null;
                    Navigate(OperatingSystem.IsWindows() ? null : "/", record: false);
                }
            }

            if (name.Length > 0)
            {
                FileEntry? entry = visible.FirstOrDefault(e => PathComparer.Equals(e.Name, name) || PathComparer.Equals(e.FullPath, name));
                if (entry is not null)
                {
                    SelectOnly(entry.FullPath);
                    EnsureVisible(IndexOf(entry.FullPath));
                }
            }
            // Navigate clears the folder name in folder mode.
            fileNameText = name;
        }

        // Returns the first candidate that is an existing folder, falling back to the current directory.
        private static string ResolveInitialDirectory(params string?[] candidates)
        {
            foreach (string? candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }
                try
                {
                    string full = Path.GetFullPath(Environment.ExpandEnvironmentVariables(candidate));
                    if (Directory.Exists(full))
                    {
                        return full;
                    }
                }
                catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException or UnauthorizedAccessException)
                {
                }
            }
            return Environment.CurrentDirectory;
        }

        private void UpdateMetrics()
        {
            font = Gui.Font;
            textSize = StyleTextSize();
            textSpacing = GuiStyle.Get(GuiDefaultProperty.TextSpacing);
            borderWidth = GuiStyle.Get(GuiControl.Default, GuiControlProperty.BorderWidth);
            iconScale = Gui.IconScale;
            iconSize = 16 * iconScale;
            rowHeight = Math.Max(iconSize, textSize) + 8;
            controlHeight = Math.Max(24, Math.Max(iconSize, textSize) + 10);
            padding = Math.Max(8, textSize / 2 + 2);
            alpha = Gui.Alpha;
        }

        // Bounds centered on the screen and sized for the current style.
        public static Rectangle DefaultBounds()
        {
            int size = StyleTextSize();
            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();
            float width = MathF.Min(screenWidth * 0.9f, MathF.Max(MinWidth(size), size * 80));
            float height = MathF.Min(screenHeight * 0.9f, MathF.Max(MinHeight(size), size * 52));
            return new Rectangle(MathF.Round((screenWidth - width) / 2), MathF.Round((screenHeight - height) / 2), width, height);
        }

        private static int StyleTextSize() => Math.Max(1, GuiStyle.Get(GuiDefaultProperty.TextSize));

        private static float MinWidth(int size) => Math.Max(360, size * 36);

        private static float MinHeight(int size) => Math.Max(260, size * 22);

        // Keeps the dialog on the screen and at least its minimum size.
        private Rectangle ClampBounds(Rectangle rect)
        {
            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();
            rect.Width = Math.Clamp(rect.Width, Math.Min(MinWidth(textSize), screenWidth), screenWidth);
            rect.Height = Math.Clamp(rect.Height, Math.Min(MinHeight(textSize), screenHeight), screenHeight);
            rect.X = MathF.Round(Math.Clamp(rect.X, 0, screenWidth - rect.Width));
            rect.Y = MathF.Round(Math.Clamp(rect.Y, 0, screenHeight - rect.Height));
            return rect;
        }

        private Rectangle GripBounds => new(bounds.X + bounds.Width - padding, bounds.Y + bounds.Height - padding, padding, padding);

        private void HandleWindowDrag()
        {
            Vector2 mouse = Raylib.GetMousePosition();
            Rectangle titleBar = new(bounds.X, bounds.Y, bounds.Width - CloseButtonSize - 6, TitleBarHeight);

            if (drag == DragMode.None && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (Raylib.CheckCollisionPointRec(mouse, GripBounds))
                {
                    drag = DragMode.Resize;
                    dragOffset = new Vector2(bounds.X + bounds.Width, bounds.Y + bounds.Height) - mouse;
                }
                else if (Raylib.CheckCollisionPointRec(mouse, titleBar))
                {
                    drag = DragMode.Move;
                    dragOffset = mouse - new Vector2(bounds.X, bounds.Y);
                }
            }

            if (drag != DragMode.None && !Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                drag = DragMode.None;
            }

            // The dialog stays where it is drawn this frame; the caller passes nextBounds back in to move it.
            if (drag == DragMode.Move)
            {
                nextBounds.X = mouse.X - dragOffset.X;
                nextBounds.Y = mouse.Y - dragOffset.Y;
            }
            else if (drag == DragMode.Resize)
            {
                nextBounds.Width = mouse.X + dragOffset.X - bounds.X;
                nextBounds.Height = mouse.Y + dragOffset.Y - bounds.Y;
            }
            nextBounds = ClampBounds(nextBounds);

            bool resizeCursor = drag == DragMode.Resize || (drag == DragMode.None && Raylib.CheckCollisionPointRec(mouse, GripBounds));
            if (resizeCursor)
            {
                Raylib.SetMouseCursor(MouseCursor.ResizeNwse);
                cursorChanged = true;
            }
            else
            {
                ReleaseCursor();
            }
        }

        private void DrawResizeGrip()
        {
            Rectangle grip = GripBounds;
            Color color = GuiStyle.GetColor(GuiDefaultProperty.LineColor);
            for (int i = 1; i <= 3; i++)
            {
                float offset = grip.Width * i / 4f;
                Raylib.DrawLineV(
                    new Vector2(grip.X + grip.Width - 1, grip.Y + offset),
                    new Vector2(grip.X + offset, grip.Y + grip.Height - 1),
                    Fade(color, 1f));
            }
        }

        private void AutoRefresh()
        {
            double now = Raylib.GetTime();
            if (now < nextRefreshCheck || CurrentDirectory is null)
            {
                return;
            }

            nextRefreshCheck = now + RefreshSeconds;
            if (GetWriteTime(CurrentDirectory) != loadedWriteTime)
            {
                Refresh();
            }
        }

        #endregion

        #region Keyboard

        private void HandleKeyboard()
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                if (AnyEditing)
                {
                    StopEditing();
                    addressText = CurrentDirectory ?? string.Empty;
                }
                else
                {
                    canceled = true;
                }
                return;
            }
            if (AnyEditing)
            {
                return;
            }

            bool ctrl = IsDown(KeyboardKey.LeftControl, KeyboardKey.RightControl) || (OperatingSystem.IsMacOS() && IsDown(KeyboardKey.LeftSuper, KeyboardKey.RightSuper));
            bool shift = IsDown(KeyboardKey.LeftShift, KeyboardKey.RightShift);
            bool alt = IsDown(KeyboardKey.LeftAlt, KeyboardKey.RightAlt);

            if (alt)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.Left)) GoBack();
                else if (Raylib.IsKeyPressed(KeyboardKey.Right)) GoForward();
                else if (Raylib.IsKeyPressed(KeyboardKey.Up)) GoUp();
                else if (Raylib.IsKeyPressed(KeyboardKey.D)) StartEditing(ref addressEditing);
                DrainChars();
                return;
            }

            if (ctrl)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.A) && options.Multiselect) SelectAll();
                else if (Raylib.IsKeyPressed(KeyboardKey.L)) StartEditing(ref addressEditing);
                else if (Raylib.IsKeyPressed(KeyboardKey.F) || Raylib.IsKeyPressed(KeyboardKey.E)) StartEditing(ref searchEditing);
                else if (Raylib.IsKeyPressed(KeyboardKey.H)) ToggleHidden();
                else if (Raylib.IsKeyPressed(KeyboardKey.R)) Refresh();
                else if (Raylib.IsKeyPressed(KeyboardKey.N) && shift && CanCreateFolder) PromptNewFolder();
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.F4)) StartEditing(ref addressEditing);
            else if (Raylib.IsKeyPressed(KeyboardKey.F3)) StartEditing(ref searchEditing);
            else if (Raylib.IsKeyPressed(KeyboardKey.F5)) { LoadPlaces(); Refresh(); }
            else if (Raylib.IsKeyPressed(KeyboardKey.Tab)) StartEditing(ref fileNameEditing);
            else if (Pressed(KeyboardKey.Backspace)) GoUp();
            else if (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.KpEnter)) Activate();

            int count = visible.Count;
            if (count > 0)
            {
                int focus = IndexOf(focusPath);
                int page = Math.Max(1, (int)(listView.Height / rowHeight) - 1);
                int? target = null;
                if (Pressed(KeyboardKey.Down)) target = focus < 0 ? 0 : focus + 1;
                else if (Pressed(KeyboardKey.Up)) target = focus < 0 ? 0 : focus - 1;
                else if (Pressed(KeyboardKey.PageDown)) target = focus < 0 ? 0 : focus + page;
                else if (Pressed(KeyboardKey.PageUp)) target = focus < 0 ? 0 : focus - page;
                else if (Raylib.IsKeyPressed(KeyboardKey.Home)) target = 0;
                else if (Raylib.IsKeyPressed(KeyboardKey.End)) target = count - 1;

                if (target is { } index)
                {
                    MoveFocus(Math.Clamp(index, 0, count - 1), shift, ctrl);
                }
            }

            // Typing selects the first item whose name starts with the typed text.
            int codepoint;
            while ((codepoint = Raylib.GetCharPressed()) > 0)
            {
                if (ctrl || codepoint < 32)
                {
                    continue;
                }

                double now = Raylib.GetTime();
                if (now - typeAheadTime > TypeAheadSeconds)
                {
                    typeAhead = string.Empty;
                }
                typeAheadTime = now;
                typeAhead += char.ConvertFromUtf32(codepoint);

                int start = Math.Max(0, IndexOf(focusPath));
                // A repeated first letter cycles through the items that start with it.
                bool cycle = typeAhead.Length > 1 && typeAhead.All(c => char.ToUpperInvariant(c) == char.ToUpperInvariant(typeAhead[0]));
                string prefix = cycle ? typeAhead[..1] : typeAhead;
                int offset = cycle || typeAhead.Length == 1 ? 1 : 0;
                for (int i = 0; i < count; i++)
                {
                    int index = (start + offset + i) % count;
                    if (visible[index].Name.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
                    {
                        MoveFocus(index, false, false);
                        break;
                    }
                }
            }
        }

        private static bool IsDown(KeyboardKey left, KeyboardKey right) => Raylib.IsKeyDown(left) || Raylib.IsKeyDown(right);

        private static bool Pressed(KeyboardKey key) => Raylib.IsKeyPressed(key) || Raylib.IsKeyPressedRepeat(key);

        private static void DrainChars()
        {
            while (Raylib.GetCharPressed() > 0)
            {
            }
        }

        private void StartEditing(ref bool editing)
        {
            StopEditing();
            editing = true;
            interacted = true;
        }

        private void StopEditing()
        {
            fileNameEditing = false;
            addressEditing = false;
            searchEditing = false;
        }

        // Enter in the list: opens the selected folder, or accepts.
        private void Activate()
        {
            if (!FolderMode && SelectedEntries is [{ IsDirectory: true } folder])
            {
                Navigate(folder.FullPath);
                return;
            }
            Accept();
        }

        private void Accept()
        {
            if (options.OkRequiresInteraction && !interacted)
            {
                return;
            }
            StopEditing();
            OnAccept();
        }

        #endregion

        #region Toolbar

        private bool CanCreateFolder => options.ShowNewFolderButton && CurrentDirectory is not null;

        private void DrawToolbar(Rectangle row, bool input)
        {
            float size = row.Height;
            float gap = Math.Max(2, padding / 2);
            float x = row.X;

            if (ToolButton(new Rectangle(x, row.Y, size, size), GuiIconName.ArrowLeft, "Back (Alt+Left)", !backHistory.IsEmpty, input))
            {
                GoBack();
            }
            x += size + gap;
            if (ToolButton(new Rectangle(x, row.Y, size, size), GuiIconName.ArrowRight, "Forward (Alt+Right)", !forwardHistory.IsEmpty, input))
            {
                GoForward();
            }
            x += size + gap;
            if (ToolButton(new Rectangle(x, row.Y, size, size), GuiIconName.ArrowUp, "Up (Alt+Up)", CanGoUp, input))
            {
                GoUp();
            }
            x += size + gap;
            if (ToolButton(new Rectangle(x, row.Y, size, size), GuiIconName.Restart, "Refresh (F5)", true, input))
            {
                LoadPlaces();
                Refresh();
            }
            x += size + padding;

            float right = row.X + row.Width;
            if (options.ShowNewFolderButton)
            {
                right -= size;
                if (ToolButton(new Rectangle(right, row.Y, size, size), GuiIconName.FolderAdd, "New folder (Ctrl+Shift+N)", CanCreateFolder, input))
                {
                    PromptNewFolder();
                }
                right -= gap;
            }

            right -= size;
            Rectangle hiddenBounds = new(right, row.Y, size, size);
            SetTooltip(hiddenBounds, showHidden ? "Hide hidden items (Ctrl+H)" : "Show hidden items (Ctrl+H)", input);
            if (Gui.Toggle(hiddenBounds, Gui.IconText(showHidden ? GuiIconName.EyeOn : GuiIconName.EyeOff), showHidden) != showHidden)
            {
                ToggleHidden();
            }
            right -= padding;

            float searchWidth = Math.Clamp(row.Width * 0.22f, textSize * 6, textSize * 16);
            right -= searchWidth;
            Rectangle searchBounds = new(right, row.Y, searchWidth, size);
            string previousSearch = searchText;
            TextBox(searchBounds, ref searchText, ref searchEditing);
            if (searchText.Length == 0 && !searchEditing)
            {
                DrawPlaceholder(searchBounds, "Search");
            }
            if (searchText != previousSearch)
            {
                RefreshView();
            }
            right -= gap;

            Rectangle addressBounds = new(x, row.Y, right - x, size);
            string display = FitStart(CurrentDirectory ?? (OperatingSystem.IsWindows() ? "This PC" : string.Empty), addressBounds.Width - padding * 2);
            if (TextBox(addressBounds, ref addressText, ref addressEditing, display))
            {
                NavigateToAddress();
            }
            else if (!addressEditing)
            {
                addressText = CurrentDirectory ?? string.Empty;
            }
        }

        private bool ToolButton(Rectangle rect, GuiIconName icon, string tip, bool enabled, bool input)
        {
            SetTooltip(rect, tip, input);
            GuiState previous = Gui.State;
            if (!enabled)
            {
                Gui.State = GuiState.Disabled;
            }
            bool pressed = Gui.Button(rect, Gui.IconText(icon));
            Gui.State = previous;
            return pressed && enabled;
        }

        private void SetTooltip(Rectangle rect, string tip, bool input)
        {
            if (input && Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect))
            {
                tooltip = tip;
            }
        }

        private void DrawTooltip()
        {
            if (tooltip != lastTooltip)
            {
                lastTooltip = tooltip;
                tooltipStart = Raylib.GetTime();
            }
            if (tooltip is null || Raylib.GetTime() - tooltipStart < TooltipSeconds || Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                return;
            }

            Vector2 mouse = Raylib.GetMousePosition();
            float width = Measure(tooltip) + padding * 2;
            float height = textSize + padding;
            float x = Math.Clamp(mouse.X, 0, Raylib.GetScreenWidth() - width);
            float y = mouse.Y + 20;
            if (y + height > Raylib.GetScreenHeight())
            {
                y = mouse.Y - height - 4;
            }

            Rectangle box = new(x, y, width, height);
            Raylib.DrawRectangleRec(box, Fade(GuiStyle.GetColor(GuiDefaultProperty.BackgroundColor), 1f));
            Raylib.DrawRectangleLinesEx(box, Math.Max(1, borderWidth), StyleColor(GuiControl.Default, GuiControlProperty.BorderColorNormal));
            DrawText(tooltip, x + padding, y + padding / 2f, StyleColor(GuiControl.Default, GuiControlProperty.TextColorNormal));
        }

        private void NavigateToAddress()
        {
            string text = addressText.Trim();
            addressText = CurrentDirectory ?? string.Empty;
            if (text.Length == 0)
            {
                return;
            }
            if (OperatingSystem.IsWindows() && text.Equals("This PC", StringComparison.OrdinalIgnoreCase))
            {
                Navigate(null);
                return;
            }

            string? full = ResolvePath(text);
            if (full is not null && Directory.Exists(full))
            {
                Navigate(full);
            }
            else if (full is not null && !FolderMode && File.Exists(full) && Navigate(Path.GetDirectoryName(full)))
            {
                SelectOnly(full);
                fileNameText = Path.GetFileName(full);
                Accept();
            }
            else
            {
                ShowError($"Can't find '{text}'.\nCheck the spelling and try again.");
            }
        }

        private bool CanGoUp => CurrentDirectory is not null && (Path.GetDirectoryName(CurrentDirectory) is not null || OperatingSystem.IsWindows());

        private void GoUp()
        {
            if (!CanGoUp)
            {
                return;
            }

            string child = CurrentDirectory!;
            if (Navigate(Path.GetDirectoryName(child)))
            {
                SelectOnly(child);
                EnsureVisible(IndexOf(child));
            }
        }

        private void GoBack()
        {
            if (backHistory.IsEmpty)
            {
                return;
            }

            string? current = CurrentDirectory;
            backHistory = backHistory.Pop(out string? target);
            if (Navigate(target, record: false))
            {
                forwardHistory = forwardHistory.Push(current);
            }
        }

        private void GoForward()
        {
            if (forwardHistory.IsEmpty)
            {
                return;
            }

            string? current = CurrentDirectory;
            forwardHistory = forwardHistory.Pop(out string? target);
            if (Navigate(target, record: false))
            {
                backHistory = backHistory.Push(current);
            }
        }

        private void ToggleHidden()
        {
            showHidden = !showHidden;
            interacted = true;
            RefreshView();
        }

        private void PromptNewFolder()
        {
            if (!CanCreateFolder)
            {
                return;
            }

            string name = "New folder";
            for (int i = 2; Path.Exists(Path.Combine(CurrentDirectory!, name)); i++)
            {
                name = $"New folder ({i})";
            }

            ShowPrompt(new PromptState
            {
                Kind = PromptKind.NewFolder,
                Title = "New Folder",
                Message = "Folder name:",
                Text = name,
                TextEditing = true,
            });
        }

        private void CreateFolder(string name)
        {
            name = name.Trim();
            if (name.Length == 0 || name is "." or ".." || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || !IsValidPathText(name))
            {
                ShowError($"{name}\nThe folder name is not valid.");
                return;
            }

            string path = Path.Combine(CurrentDirectory!, name);
            if (Path.Exists(path))
            {
                ShowError($"{name}\nA file or folder with this name already exists.");
                return;
            }

            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex) when (IsFileSystemError(ex))
            {
                ShowError($"{name}\n{DescribeError(ex)}");
                return;
            }

            Refresh();
            interacted = true;
            SelectOnly(Path.GetFullPath(path));
            EnsureVisible(IndexOf(focusPath));
            SyncFileNameText();
        }

        #endregion

        #region Places

        internal sealed record Place(string Name, string? Path, GuiIconName Icon);

        private void LoadPlaces()
        {
            var list = new List<Place>();
            var seen = new HashSet<string>(PathComparer);

            void AddFolder(string name, string path, GuiIconName icon)
            {
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path) && seen.Add(Path.TrimEndingDirectorySeparator(path)))
                {
                    list.Add(new Place(name, path, icon));
                }
            }

            foreach (FileDialogCustomPlace place in loadedCustomPlaces)
            {
                try
                {
                    string path = Path.GetFullPath(place.Path);
                    AddFolder(place.Name ?? (Path.GetFileName(Path.TrimEndingDirectorySeparator(path)) is { Length: > 0 } n ? n : path), path, GuiIconName.Star);
                }
                catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
                {
                }
            }

            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            AddFolder("Home", home, GuiIconName.House);
            AddFolder("Desktop", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), GuiIconName.Monitor);
            AddFolder("Documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), GuiIconName.TextNotes);
            if (home.Length > 0)
            {
                AddFolder("Downloads", Path.Combine(home, "Downloads"), GuiIconName.ArrowDownFill);
            }
            AddFolder("Pictures", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), GuiIconName.FiletypeImage);
            AddFolder("Music", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), GuiIconName.FiletypeAudio);
            AddFolder("Videos", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), GuiIconName.FiletypeVideo);

            if (OperatingSystem.IsWindows())
            {
                list.Add(new Place("This PC", null, GuiIconName.Cpu));
                foreach (DriveInfo drive in GetDrives())
                {
                    if (FileEntry.FromDrive(drive) is { } entry)
                    {
                        list.Add(new Place(entry.Name, entry.FullPath, GuiIconName.Rom));
                    }
                }
            }
            else
            {
                list.Add(new Place("File System", "/", GuiIconName.Rom));
                foreach (DriveInfo drive in GetDrives().Where(d => d.DriveType == DriveType.Removable))
                {
                    if (FileEntry.FromDrive(drive) is { } entry)
                    {
                        AddFolder(entry.Name, entry.FullPath, GuiIconName.Rom);
                    }
                }
            }

            places = list;
        }

        private void DrawPlaces(Rectangle area, bool input)
        {
            List<Place> places = this.places ?? [];
            float contentHeight = places.Count * rowHeight;
            Rectangle content = ScrollContent(area, contentHeight, out float viewX);
            ScrollPanelResult panel = Gui.ScrollPanel(area, null, content, placesScroll);
            placesScroll = panel.Scroll;
            Rectangle view = panel.View;

            Vector2 mouse = Raylib.GetMousePosition();
            bool mouseInView = input && Raylib.CheckCollisionPointRec(mouse, view);

            Raylib.BeginScissorMode((int)view.X, (int)view.Y, (int)view.Width, (int)view.Height);
            for (int i = 0; i < places.Count; i++)
            {
                Rectangle rowBounds = new(viewX + placesScroll.X, view.Y + placesScroll.Y + i * rowHeight, content.Width, rowHeight);
                if (rowBounds.Y + rowHeight < view.Y || rowBounds.Y > view.Y + view.Height)
                {
                    continue;
                }

                Place place = places[i];
                bool active = SamePath(place.Path, CurrentDirectory);
                bool hovered = mouseInView && Raylib.CheckCollisionPointRec(mouse, rowBounds);
                Color textColor = DrawRowBackground(rowBounds, active, hovered);
                float textX = rowBounds.X + padding / 2f;
                Gui.DrawIcon(place.Icon, (int)textX, (int)(rowBounds.Y + (rowHeight - iconSize) / 2), iconScale, textColor);
                textX += iconSize + padding / 2f;
                DrawText(Fit(place.Name, rowBounds.X + rowBounds.Width - textX - 2), textX, CenterTextY(rowBounds), textColor);

                if (hovered)
                {
                    SetTooltip(rowBounds, place.Path ?? place.Name, input);
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        interacted = true;
                        StopEditing();
                        Navigate(place.Path);
                    }
                }
            }
            Raylib.EndScissorMode();
        }

        private static DriveInfo[] GetDrives()
        {
            try
            {
                return DriveInfo.GetDrives();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return [];
            }
        }

        #endregion

        #region File list

        private readonly record struct Column(SortColumn Kind, string Title, float X, float Width, bool AlignRight);

        private void DrawFileList(Rectangle area, bool input)
        {
            Rectangle header = new(area.X, area.Y, area.Width, rowHeight + 2);
            Rectangle panelBounds = new(area.X, area.Y + header.Height - borderWidth, area.Width, area.Height - header.Height + borderWidth);

            Rectangle content = ScrollContent(panelBounds, visible.Count * rowHeight, out float viewX);
            List<Column> columns = LayoutColumns(viewX, content.Width);
            DrawHeader(header, columns, input);

            ScrollPanelResult panel = Gui.ScrollPanel(panelBounds, null, content, listScroll);
            listScroll = panel.Scroll;
            listView = panel.View;
            if (scrollToPath is not null)
            {
                EnsureVisible(IndexOf(scrollToPath));
                scrollToPath = null;
            }

            Vector2 mouse = Raylib.GetMousePosition();
            bool mouseInView = input && Raylib.CheckCollisionPointRec(mouse, listView);
            int focusIndex = AnyEditing ? -1 : IndexOf(focusPath);

            Raylib.BeginScissorMode((int)listView.X, (int)listView.Y, (int)listView.Width, (int)listView.Height);
            int first = Math.Max(0, (int)(-listScroll.Y / rowHeight));
            int last = Math.Min(visible.Count - 1, (int)((-listScroll.Y + listView.Height) / rowHeight));
            for (int i = first; i <= last; i++)
            {
                Rectangle rowBounds = new(viewX + listScroll.X, listView.Y + listScroll.Y + i * rowHeight, content.Width, rowHeight);
                FileEntry entry = visible[i];
                bool hovered = mouseInView && Raylib.CheckCollisionPointRec(mouse, rowBounds);
                Color textColor = DrawRowBackground(rowBounds, selection.Contains(entry.FullPath), hovered);
                if (i == focusIndex && !selection.Contains(entry.FullPath))
                {
                    Raylib.DrawRectangleLinesEx(rowBounds, 1, StyleColor(GuiControl.ListView, GuiControlProperty.BorderColorFocused));
                }
                if (entry.IsHidden)
                {
                    textColor = ColorAlpha(textColor, 0.55f);
                }
                DrawRow(rowBounds, entry, columns, textColor);
            }

            if (visible.Count == 0)
            {
                string message = loadError
                    ?? (searchText.Length > 0 ? "No items match your search."
                    : entries.Count > 0 ? "No items match the filter."
                    : "This folder is empty.");
                float messageWidth = Measure(message);
                DrawText(message, listView.X + (listView.Width - messageWidth) / 2, listView.Y + padding * 2,
                    StyleColor(GuiControl.ListView, GuiControlProperty.TextColorDisabled));
            }
            Raylib.EndScissorMode();

            if (mouseInView && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                int index = (int)MathF.Floor((mouse.Y - listView.Y - listScroll.Y) / rowHeight);
                if (index >= 0 && index < visible.Count)
                {
                    ClickEntry(index);
                }
                else
                {
                    interacted = true;
                    StopEditing();
                    selection = selection.Clear();
                    SyncFileNameText();
                }
            }
        }

        // The content of a scroll panel as wide as its view, so it only scrolls vertically.
        private Rectangle ScrollContent(Rectangle panel, float contentHeight, out float viewX)
        {
            int scrollBarWidth = GuiStyle.Get(GuiListViewProperty.ScrollBarWidth);
            bool verticalBar = contentHeight > panel.Height - borderWidth * 2;
            float width = panel.Width - borderWidth * 2 - (verticalBar ? scrollBarWidth : 0);
            bool barOnLeft = GuiStyle.Get(GuiListViewProperty.ScrollBarSide) == (int)GuiScrollBarSide.Left;
            viewX = panel.X + borderWidth + (verticalBar && barOnLeft ? scrollBarWidth : 0);
            return new Rectangle(0, 0, width, contentHeight);
        }

        private List<Column> LayoutColumns(float x, float width)
        {
            float cellPadding = padding * 2;
            // Digit widths vary in proportional fonts, so the sample date gets some slack.
            float dateWidth = Measure(new DateTime(2000, 12, 28, 23, 59, 0).ToString("g", CultureInfo.CurrentCulture)) + cellPadding + textSize;
            float typeWidth = Math.Max(Measure("File folder"), textSize * 7) + cellPadding;
            float sizeWidth = Measure("999,999 KB") + cellPadding;
            float minNameWidth = textSize * 14;

            var optional = new List<(SortColumn Kind, string Title, float Width, bool AlignRight)>
            {
                (SortColumn.Modified, "Date modified", dateWidth, false),
            };
            if (!FolderMode)
            {
                optional.Add((SortColumn.Type, "Type", typeWidth, false));
                optional.Add((SortColumn.Size, "Size", sizeWidth, true));
            }
            // Drop the right-most columns until the name column fits.
            while (optional.Count > 0 && width - optional.Sum(c => c.Width) < minNameWidth)
            {
                optional.RemoveAt(optional.Count - 1);
            }

            float nameWidth = width - optional.Sum(c => c.Width);
            var columns = new List<Column> { new(SortColumn.Name, "Name", x, nameWidth, false) };
            float columnX = x + nameWidth;
            foreach (var column in optional)
            {
                columns.Add(new Column(column.Kind, column.Title, columnX, column.Width, column.AlignRight));
                columnX += column.Width;
            }
            return columns;
        }

        private void DrawHeader(Rectangle header, List<Column> columns, bool input)
        {
            Raylib.DrawRectangleRec(header, StyleColor(GuiControl.Default, GuiControlProperty.BaseColorNormal));
            Raylib.DrawRectangleLinesEx(header, borderWidth, StyleColor(GuiControl.Default, GuiControlProperty.BorderColorNormal));

            Vector2 mouse = Raylib.GetMousePosition();
            Color lineColor = Fade(GuiStyle.GetColor(GuiDefaultProperty.LineColor), 1f);
            foreach (Column column in columns)
            {
                Rectangle cell = new(column.X, header.Y, column.Width, header.Height);
                bool hovered = input && Raylib.CheckCollisionPointRec(mouse, cell);
                if (hovered)
                {
                    Raylib.DrawRectangleRec(new Rectangle(cell.X, cell.Y + borderWidth, cell.Width, cell.Height - borderWidth * 2),
                        StyleColor(GuiControl.Default, GuiControlProperty.BaseColorFocused));
                }
                Color textColor = StyleColor(GuiControl.Default, hovered ? GuiControlProperty.TextColorFocused : GuiControlProperty.TextColorNormal);

                float arrowSpace = sortColumn == column.Kind ? iconSize + 2 : 0;
                string title = Fit(column.Title, column.Width - padding * 2 - arrowSpace);
                float titleWidth = Measure(title);
                float textX = column.AlignRight ? cell.X + cell.Width - padding - titleWidth : cell.X + padding;
                DrawText(title, textX, CenterTextY(cell), textColor);

                if (sortColumn == column.Kind)
                {
                    float arrowX = column.AlignRight ? textX - iconSize - 2 : textX + titleWidth + 2;
                    Gui.DrawIcon(sortDescending ? GuiIconName.ArrowDownFill : GuiIconName.ArrowUpFill,
                        (int)arrowX, (int)(cell.Y + (cell.Height - iconSize) / 2), iconScale, textColor);
                }
                if (column.X > header.X + borderWidth)
                {
                    Raylib.DrawLineV(new Vector2(column.X, cell.Y + 4), new Vector2(column.X, cell.Y + cell.Height - 4), lineColor);
                }

                if (hovered && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    sortDescending = sortColumn == column.Kind && !sortDescending;
                    sortColumn = column.Kind;
                    interacted = true;
                    RefreshView();
                }
            }
        }

        private void DrawRow(Rectangle rowBounds, FileEntry entry, List<Column> columns, Color textColor)
        {
            float textY = CenterTextY(rowBounds);
            foreach (Column column in columns)
            {
                float cellX = column.X + listScroll.X;
                if (column.Kind == SortColumn.Name)
                {
                    float iconX = cellX + padding / 2f;
                    Gui.DrawIcon(entry.Icon, (int)iconX, (int)(rowBounds.Y + (rowHeight - iconSize) / 2), iconScale, textColor);
                    float textX = iconX + iconSize + padding / 2f;
                    DrawText(Fit(entry.Name, cellX + column.Width - textX - padding / 2f), textX, textY, textColor);
                    continue;
                }

                string text = column.Kind switch
                {
                    SortColumn.Modified => entry.Modified?.ToString("g", CultureInfo.CurrentCulture) ?? string.Empty,
                    SortColumn.Type => entry.TypeName,
                    SortColumn.Size => entry.Size is { } size ? FileEntry.FormatSize(size) : string.Empty,
                    _ => string.Empty,
                };
                text = Fit(text, column.Width - padding * 2);
                float x = column.AlignRight ? cellX + column.Width - padding - Measure(text) : cellX + padding;
                DrawText(text, x, textY, textColor);
            }
        }

        // Draws a list row's background for its state and returns the color for its text.
        private Color DrawRowBackground(Rectangle rowBounds, bool selected, bool hovered)
        {
            if (selected)
            {
                Raylib.DrawRectangleRec(rowBounds, StyleColor(GuiControl.ListView, GuiControlProperty.BaseColorPressed));
                Raylib.DrawRectangleLinesEx(rowBounds, 1, StyleColor(GuiControl.ListView, GuiControlProperty.BorderColorPressed));
                return StyleColor(GuiControl.ListView, GuiControlProperty.TextColorPressed);
            }
            if (hovered)
            {
                Raylib.DrawRectangleRec(rowBounds, StyleColor(GuiControl.ListView, GuiControlProperty.BaseColorFocused));
                Raylib.DrawRectangleLinesEx(rowBounds, 1, StyleColor(GuiControl.ListView, GuiControlProperty.BorderColorFocused));
                return StyleColor(GuiControl.ListView, GuiControlProperty.TextColorFocused);
            }
            return StyleColor(GuiControl.ListView, GuiControlProperty.TextColorNormal);
        }

        private void ClickEntry(int index)
        {
            interacted = true;
            StopEditing();
            FileEntry entry = visible[index];
            bool ctrl = IsDown(KeyboardKey.LeftControl, KeyboardKey.RightControl) || (OperatingSystem.IsMacOS() && IsDown(KeyboardKey.LeftSuper, KeyboardKey.RightSuper));
            bool shift = IsDown(KeyboardKey.LeftShift, KeyboardKey.RightShift);

            double now = Raylib.GetTime();
            bool isDoubleClick = !ctrl && !shift && lastClickPath is not null
                && PathComparer.Equals(lastClickPath, entry.FullPath) && now - lastClickTime <= DoubleClickSeconds;
            lastClickPath = isDoubleClick ? null : entry.FullPath;
            lastClickTime = now;

            if (isDoubleClick)
            {
                SelectOnly(entry.FullPath);
                if (entry.IsDirectory)
                {
                    Navigate(entry.FullPath);
                }
                else
                {
                    fileNameText = entry.Name;
                    acceptOnRelease = true;
                }
                return;
            }

            if (options.Multiselect && shift && IndexOf(anchorPath) is var anchor and >= 0)
            {
                SelectRange(anchor, index, keep: ctrl);
            }
            else if (options.Multiselect && ctrl)
            {
                selection = selection.Contains(entry.FullPath)
                    ? selection.Remove(entry.FullPath)
                    : selection.Add(entry.FullPath);
                anchorPath = entry.FullPath;
            }
            else
            {
                SelectOnly(entry.FullPath);
            }
            focusPath = entry.FullPath;
            SyncFileNameText();
        }

        private void MoveFocus(int index, bool shift, bool ctrl)
        {
            interacted = true;
            string path = visible[index].FullPath;
            if (options.Multiselect && shift && IndexOf(anchorPath) is var anchor and >= 0)
            {
                SelectRange(anchor, index, keep: false);
            }
            else if (!(options.Multiselect && ctrl))
            {
                SelectOnly(path);
            }
            focusPath = path;
            EnsureVisible(index);
            SyncFileNameText();
        }

        private void SelectOnly(string path)
        {
            selection = selection.Clear().Add(path);
            focusPath = path;
            anchorPath = path;
        }

        private void SelectRange(int from, int to, bool keep)
        {
            if (!keep)
            {
                selection = selection.Clear();
            }
            int first = Math.Min(from, to);
            selection = selection.Union(visible.GetRange(first, Math.Max(from, to) - first + 1).Select(e => e.FullPath));
        }

        private void SelectAll()
        {
            interacted = true;
            selection = selection.Union(visible.Select(e => e.FullPath));
            SyncFileNameText();
        }

        private void EnsureVisible(int index)
        {
            if (index < 0)
            {
                return;
            }
            if (listView.Height <= 0)
            {
                // The list hasn't been laid out yet; scroll once it has.
                scrollToPath = visible[index].FullPath;
                return;
            }

            float top = index * rowHeight;
            if (top < -listScroll.Y)
            {
                listScroll.Y = -top;
            }
            else if (top + rowHeight > -listScroll.Y + listView.Height)
            {
                listScroll.Y = -(top + rowHeight - listView.Height);
            }
        }

        private int IndexOf(string? path)
        {
            return path is null ? -1 : visible.FindIndex(e => PathComparer.Equals(e.FullPath, path));
        }

        // Shows the selected items' names in the file name box, like the Windows dialog.
        private void SyncFileNameText()
        {
            if (fileNameEditing)
            {
                return;
            }

            List<string> names = SelectedEntries
                .Where(e => e.IsDirectory == FolderMode)
                .Select(e => e.IsDrive ? e.FullPath : e.Name)
                .ToList();
            if (names.Count == 0)
            {
                // In file mode, selecting folders keeps what the user typed.
                if (FolderMode)
                {
                    fileNameText = string.Empty;
                }
                return;
            }
            fileNameText = names.Count == 1 ? names[0] : string.Join(" ", names.Select(n => $"\"{n}\""));
        }

        #endregion

        #region Loading and sorting

        private static List<FileEntry> ReadDirectory(string? directory)
        {
            if (directory is null)
            {
                return GetDrives().Select(FileEntry.FromDrive).OfType<FileEntry>().ToList();
            }

            var options = new EnumerationOptions
            {
                AttributesToSkip = 0,
                IgnoreInaccessible = false,
                ReturnSpecialDirectories = false,
            };
            return new DirectoryInfo(directory).EnumerateFileSystemInfos("*", options).Select(FileEntry.FromInfo).ToList();
        }

        private void Refresh()
        {
            try
            {
                entries = ReadDirectory(CurrentDirectory);
                loadError = null;
            }
            catch (Exception ex) when (IsFileSystemError(ex))
            {
                entries = [];
                loadError = DescribeError(ex);
            }
            loadedDirectory = CurrentDirectory;
            loadedWriteTime = GetWriteTime(CurrentDirectory);
            nextRefreshCheck = Raylib.GetTime() + RefreshSeconds;
            RefreshView();
        }

        // Rebuilds the visible list from the loaded entries, the filters and the sort order.
        private void RefreshView()
        {
            FileFilter? filter = pattern is null ? SelectedFilter : new FileFilter(pattern, pattern);
            visible = entries.Where(e =>
                    (e.IsDrive || showHidden || !e.IsHidden)
                    && (!FolderMode || e.IsDirectory)
                    && (e.IsDirectory || filter is null || filter.Matches(e.Name))
                    && (searchText.Length == 0 || e.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)))
                .ToList();
            visible.Sort(CompareEntries);
            viewKey = ViewKeyNow;

            var visiblePaths = new HashSet<string>(visible.Select(e => e.FullPath), PathComparer);
            selection = selection.Intersect(visiblePaths);
            if (focusPath is not null && !visiblePaths.Contains(focusPath))
            {
                focusPath = null;
            }
            if (anchorPath is not null && !visiblePaths.Contains(anchorPath))
            {
                anchorPath = null;
            }
        }

        private int CompareEntries(FileEntry x, FileEntry y)
        {
            // Folders stay above files in both directions, like Explorer.
            if (x.IsDirectory != y.IsDirectory)
            {
                return x.IsDirectory ? -1 : 1;
            }

            int result = sortColumn switch
            {
                SortColumn.Modified => Nullable.Compare(x.Modified, y.Modified),
                SortColumn.Type => string.Compare(x.TypeName, y.TypeName, StringComparison.CurrentCultureIgnoreCase),
                SortColumn.Size => Nullable.Compare(x.Size, y.Size),
                _ => 0,
            };
            if (result == 0)
            {
                result = x.IsDrive && y.IsDrive ? string.Compare(x.FullPath, y.FullPath, StringComparison.OrdinalIgnoreCase) : CompareNatural(x.Name, y.Name);
            }
            return sortDescending ? -result : result;
        }

        // Compares names with digit runs compared by value, so "file2" sorts before "file10".
        private static int CompareNatural(string a, string b)
        {
            int i = 0;
            int j = 0;
            while (i < a.Length && j < b.Length)
            {
                if (char.IsAsciiDigit(a[i]) && char.IsAsciiDigit(b[j]))
                {
                    int startA = i;
                    int startB = j;
                    while (i < a.Length && char.IsAsciiDigit(a[i])) i++;
                    while (j < b.Length && char.IsAsciiDigit(b[j])) j++;
                    ReadOnlySpan<char> digitsA = a.AsSpan(startA, i - startA).TrimStart('0');
                    ReadOnlySpan<char> digitsB = b.AsSpan(startB, j - startB).TrimStart('0');
                    if (digitsA.Length != digitsB.Length)
                    {
                        return digitsA.Length.CompareTo(digitsB.Length);
                    }
                    int digits = digitsA.CompareTo(digitsB, StringComparison.Ordinal);
                    if (digits != 0)
                    {
                        return digits;
                    }
                }
                else
                {
                    int chars = string.Compare(a, i, b, j, 1, StringComparison.CurrentCultureIgnoreCase);
                    if (chars != 0)
                    {
                        return chars;
                    }
                    i++;
                    j++;
                }
            }
            return (a.Length - i).CompareTo(b.Length - j);
        }

        private static DateTime GetWriteTime(string? directory)
        {
            if (directory is null)
            {
                return default;
            }
            try
            {
                return Directory.GetLastWriteTimeUtc(directory);
            }
            catch (Exception ex) when (IsFileSystemError(ex))
            {
                return default;
            }
        }

        private static bool IsFileSystemError(Exception ex)
        {
            return ex is IOException or UnauthorizedAccessException or SecurityException or ArgumentException or NotSupportedException;
        }

        private static string DescribeError(Exception ex) => ex switch
        {
            UnauthorizedAccessException or SecurityException => "Access is denied.",
            DirectoryNotFoundException => "The folder doesn't exist.",
            FileNotFoundException => "The file doesn't exist.",
            PathTooLongException => "The path is too long.",
            ArgumentException or NotSupportedException => "The path is not valid.",
            _ => ex.Message,
        };

        private bool SamePath(string? a, string? b)
        {
            if (a is null || b is null)
            {
                return a is null && b is null;
            }
            return PathComparer.Equals(Path.TrimEndingDirectorySeparator(a), Path.TrimEndingDirectorySeparator(b));
        }

        #endregion

        #region Bottom rows

        // Draws the file name row and returns the bounds for the filter dropdown, which is drawn last.
        private Rectangle DrawNameRow(Rectangle row)
        {
            string label = FolderMode ? "Folder:" : "File name:";
            float labelWidth = Measure(label);
            DrawText(label, row.X, CenterTextY(row), StyleColor(GuiControl.Label, GuiControlProperty.TextColorNormal));

            float x = row.X + labelWidth + padding;
            float right = row.X + row.Width;
            Rectangle filterBounds = default;
            if (filters.Length > 0)
            {
                float longest = filters.Max(f => Measure(f.Description));
                float filterWidth = Math.Clamp(longest + controlHeight + padding * 2, textSize * 10, row.Width * 0.4f);
                right -= filterWidth;
                filterBounds = new Rectangle(right, row.Y, filterWidth, row.Height);
                right -= padding;
            }

            Rectangle textBounds = new(x, row.Y, right - x, row.Height);
            string display = Fit(fileNameText, textBounds.Width - padding * 2);
            if (TextBox(textBounds, ref fileNameText, ref fileNameEditing, display))
            {
                Accept();
            }
            return filterBounds;
        }

        private void DrawFilterDropdown(Rectangle filterBounds)
        {
            // raygui splits items on ';' and limits their total size, so descriptions are sanitized and shortened.
            int maxLength = Math.Max(8, 900 / filters.Length);
            IEnumerable<string> items = filters.Select(f =>
            {
                string text = f.Description.Replace(';', ',').Replace('\n', ' ');
                return text.Length > maxLength ? text[..(maxLength - Ellipsis.Length)] + Ellipsis : text;
            }).Take(128);

            int previousRollUp = GuiStyle.Get(GuiDropdownBoxProperty.DropdownRollUp);
            GuiStyle.Set(GuiDropdownBoxProperty.DropdownRollUp, true);
            DropdownBoxResult result;
            try
            {
                result = Gui.DropdownBox(filterBounds, items, Math.Min(filterIndex, 127), filterOpen);
            }
            finally
            {
                GuiStyle.Set(GuiDropdownBoxProperty.DropdownRollUp, previousRollUp);
            }

            if (result.IsOpen != filterOpen)
            {
                interacted = true;
                StopEditing();
            }
            filterOpen = result.IsOpen;
            if (result.Active != filterIndex)
            {
                filterIndex = result.Active;
                pattern = null;
                if (Mode == FileBrowserMode.Save)
                {
                    ChangeTypedExtension();
                }
                RefreshView();
            }
        }

        // Switches the typed file name to the extension of the newly selected filter.
        private void ChangeTypedExtension()
        {
            string? extension = SelectedFilter?.Extension;
            string name = fileNameText.Trim();
            if (extension is null || name.Length == 0 || name.StartsWith('"') || !Path.HasExtension(name))
            {
                return;
            }
            fileNameText = Path.ChangeExtension(name, extension);
        }

        private void DrawButtonRow(Rectangle row)
        {
            float x = row.X;
            if (options.ShowReadOnly)
            {
                const string Label = "Open as read-only";
                float boxSize = Math.Min(row.Height - 6, textSize + 6);
                Rectangle box = new(x, row.Y + (row.Height - boxSize) / 2, boxSize, boxSize);
                bool isChecked = Gui.CheckBox(box, Label, readOnlyChecked);
                if (isChecked != readOnlyChecked)
                {
                    interacted = true;
                }
                readOnlyChecked = isChecked;
                x += boxSize + padding + Measure(Label) + padding * 2;
            }

            float right = row.X + row.Width;
            float cancelWidth = ButtonWidth("Cancel");
            right -= cancelWidth;
            if (Gui.Button(new Rectangle(right, row.Y, cancelWidth, row.Height), "Cancel"))
            {
                canceled = true;
            }

            right -= padding;
            float okWidth = ButtonWidth(options.OkText);
            right -= okWidth;
            GuiState previous = Gui.State;
            bool okEnabled = !options.OkRequiresInteraction || interacted;
            if (!okEnabled)
            {
                Gui.State = GuiState.Disabled;
            }
            if (Gui.Button(new Rectangle(right, row.Y, okWidth, row.Height), options.OkText) && okEnabled)
            {
                Accept();
            }
            Gui.State = previous;

            if (options.ShowHelp)
            {
                right -= padding;
                float helpWidth = ButtonWidth("Help");
                right -= helpWidth;
                if (Gui.Button(new Rectangle(right, row.Y, helpWidth, row.Height), "Help"))
                {
                    helpClicked = true;
                }
            }

            int selected = selection.Count;
            string items = visible.Count == 1 ? "1 item" : $"{visible.Count} items";
            string status = selected > 0 ? $"{items}    {selected} selected" : items;
            float statusWidth = right - padding - x;
            if (statusWidth > textSize * 4)
            {
                DrawText(Fit(status, statusWidth), x, CenterTextY(row), StyleColor(GuiControl.Label, GuiControlProperty.TextColorNormal));
            }
        }

        private float ButtonWidth(string text) => Math.Max(Measure(text) + padding * 3, textSize * 8);

        #endregion

        #region Prompts

        // Opens a small modal window over the browser, which blocks it until it is answered.
        private void ShowPrompt(PromptState newPrompt)
        {
            prompt = newPrompt with { OpenedFrame = frame };
            filterOpen = false;
            drag = DragMode.None;
            StopEditing();
            ReleaseCursor();
        }

        // Answers the prompt: button is the index of the clicked button, or -1 when the prompt was closed.
        private void ClosePrompt(int button)
        {
            if (prompt is not { } current)
            {
                return;
            }
            // Cleared first so answering it can open another prompt.
            prompt = null;
            switch (current.Kind)
            {
                case PromptKind.ConfirmAccept when button == 0:
                    acceptedPaths = [.. current.PendingPaths];
                    break;
                case PromptKind.NewFolder when button == 0:
                    CreateFolder(current.Text ?? string.Empty);
                    break;
            }
        }

        private void DrawPrompt(PromptState current)
        {
            // The input that opened the prompt must not also answer it.
            bool acceptInput = frame > current.OpenedFrame && !locked;
            Gui.IsLocked = !acceptInput;

            Raylib.DrawRectangleRec(bounds, Fade(GuiStyle.GetColor(GuiDefaultProperty.BackgroundColor), 0.6f));

            int screenWidth = Raylib.GetScreenWidth();
            string[] lines = current.Message.Split('\n');
            float lineHeight = textSize + Math.Max(2, GuiStyle.Get(GuiDefaultProperty.TextLineSpacing) - textSize + 2);
            float iconArea = iconSize * 2 + padding * 2;
            float buttonsWidth = current.Buttons.Sum(ButtonWidth) + padding * (current.Buttons.Length - 1);
            float maxWidth = Math.Min(screenWidth - padding * 2, Math.Max(bounds.Width, textSize * 50));
            float width = Math.Clamp(Math.Max(lines.Max(Measure) + iconArea + padding * 2, buttonsWidth + padding * 2), textSize * 26, maxWidth);
            float messageHeight = Math.Max(lines.Length * lineHeight, iconSize * 2);
            float height = TitleBarHeight + padding * 2 + messageHeight + (current.Text is null ? 0 : controlHeight + padding) + padding + controlHeight + padding;

            Rectangle box = new(
                MathF.Round(Math.Clamp(bounds.X + (bounds.Width - width) / 2, 0, Math.Max(0, screenWidth - width))),
                MathF.Round(Math.Clamp(bounds.Y + (bounds.Height - height) / 2, 0, Math.Max(0, Raylib.GetScreenHeight() - height))),
                width, height);

            if (Gui.WindowBox(box, current.Title))
            {
                ClosePrompt(-1);
                return;
            }

            float x = box.X + padding * 2;
            float y = box.Y + TitleBarHeight + padding * 2;
            Color textColor = StyleColor(GuiControl.Label, GuiControlProperty.TextColorNormal);
            Gui.DrawIcon(current.Icon, (int)x, (int)y, iconScale * 2, textColor);
            float textX = x + iconSize * 2 + padding * 2;
            for (int i = 0; i < lines.Length; i++)
            {
                DrawText(Fit(lines[i], box.X + box.Width - padding * 2 - textX), textX, y + i * lineHeight, textColor);
            }
            y += messageHeight + padding;

            bool submitted = false;
            if (current.Text is not null)
            {
                string text = current.Text;
                bool editing = current.TextEditing && acceptInput;
                bool wasEditing = editing;
                TextBoxResult result = Gui.TextBox(new Rectangle(textX, y, box.X + box.Width - padding * 2 - textX, controlHeight), text, TextMaxBytes, editing);
                if (editing)
                {
                    current = current with { Text = result.Text };
                }
                if (acceptInput)
                {
                    current = current with { TextEditing = result.IsEditing };
                    submitted = wasEditing && !result.IsEditing && (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.KpEnter));
                }
                prompt = current;
                y += controlHeight + padding;
            }

            float buttonX = box.X + box.Width - padding * 2 - buttonsWidth;
            float buttonY = box.Y + box.Height - padding - controlHeight;
            for (int i = 0; i < current.Buttons.Length; i++)
            {
                float buttonWidth = ButtonWidth(current.Buttons[i]);
                Rectangle buttonBounds = new(buttonX, buttonY, buttonWidth, controlHeight);
                if (i == current.DefaultButton)
                {
                    Raylib.DrawRectangleLinesEx(
                        new Rectangle(buttonBounds.X - 2, buttonBounds.Y - 2, buttonBounds.Width + 4, buttonBounds.Height + 4),
                        1, StyleColor(GuiControl.Button, GuiControlProperty.BorderColorFocused));
                }
                if (Gui.Button(buttonBounds, current.Buttons[i]))
                {
                    ClosePrompt(i);
                    return;
                }
                buttonX += buttonWidth + padding;
            }

            if (!acceptInput)
            {
                DrainChars();
                return;
            }

            if (submitted || (!current.TextEditing && (Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.KpEnter))))
            {
                ClosePrompt(current.DefaultButton);
            }
            else if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            {
                ClosePrompt(-1);
            }
            else if (current.Buttons.Length > 1 && Raylib.IsKeyPressed(KeyboardKey.Tab) && !current.TextEditing)
            {
                // raygui has no keyboard focus, so Tab moves the default between buttons.
                prompt = current with { DefaultButton = (current.DefaultButton + 1) % current.Buttons.Length };
            }
        }

        #endregion

        #region Drawing helpers

        // Draws a text box whose text only changes while it is edited; returns true when editing ends with Enter.
        private bool TextBox(Rectangle rect, ref string text, ref bool editing, string? displayText = null)
        {
            bool wasEditing = editing;
            TextBoxResult result = Gui.TextBox(rect, wasEditing ? text : displayText ?? text, TextMaxBytes, wasEditing);
            if (wasEditing)
            {
                text = result.Text;
            }
            if (result.IsEditing == wasEditing)
            {
                return false;
            }

            if (result.IsEditing)
            {
                StartEditing(ref editing);
                return false;
            }
            editing = false;
            return Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.KpEnter);
        }

        private void DrawPlaceholder(Rectangle rect, string text)
        {
            int textPadding = GuiStyle.Get(GuiControl.TextBox, GuiControlProperty.TextPadding);
            DrawText(Fit(text, rect.Width - padding * 2), rect.X + borderWidth + textPadding + 2, CenterTextY(rect),
                StyleColor(GuiControl.TextBox, GuiControlProperty.TextColorDisabled));
        }

        private float CenterTextY(Rectangle rect) => rect.Y + (rect.Height - textSize) / 2;

        private void DrawText(string text, float x, float y, Color color)
        {
            if (text.Length > 0)
            {
                Raylib.DrawTextEx(font, text, new Vector2(MathF.Round(x), MathF.Round(y)), textSize, textSpacing, color);
            }
        }

        private float Measure(string text)
        {
            return text.Length == 0 ? 0 : Raylib.MeasureTextEx(font, text, textSize, textSpacing).X;
        }

        // Shortens text from the end with an ellipsis until it fits.
        private string Fit(string text, float maxWidth)
        {
            if (maxWidth <= 0)
            {
                return string.Empty;
            }
            if (Measure(text) <= maxWidth)
            {
                return text;
            }

            int low = 0;
            int high = text.Length;
            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                if (Measure(string.Concat(text.AsSpan(0, mid), Ellipsis)) <= maxWidth)
                {
                    low = mid;
                }
                else
                {
                    high = mid - 1;
                }
            }
            if (low > 0 && char.IsHighSurrogate(text[low - 1]))
            {
                low--;
            }
            return low == 0 ? string.Empty : string.Concat(text.AsSpan(0, low), Ellipsis);
        }

        // Shortens text from the start with an ellipsis until it fits, keeping the end of a path visible.
        private string FitStart(string text, float maxWidth)
        {
            if (maxWidth <= 0)
            {
                return string.Empty;
            }
            if (Measure(text) <= maxWidth)
            {
                return text;
            }

            int low = 0;
            int high = text.Length;
            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                if (Measure(string.Concat(Ellipsis, text.AsSpan(text.Length - mid))) <= maxWidth)
                {
                    low = mid;
                }
                else
                {
                    high = mid - 1;
                }
            }
            if (low > 0 && char.IsLowSurrogate(text[text.Length - low]))
            {
                low--;
            }
            return string.Concat(Ellipsis, text.AsSpan(text.Length - low));
        }

        private Color StyleColor(GuiControl control, GuiControlProperty property)
        {
            return Fade(GuiStyle.GetColor(control, property), 1f);
        }

        // Applies Gui.Alpha, like raygui does for its own controls.
        private Color Fade(Color color, float amount)
        {
            return ColorAlpha(color, amount * alpha);
        }

        private static Color ColorAlpha(Color color, float amount)
        {
            return new Color(color.R, color.G, color.B, (byte)Math.Clamp(color.A * amount, 0, 255));
        }

        #endregion
    }
}
