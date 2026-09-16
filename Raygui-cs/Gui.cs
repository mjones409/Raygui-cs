using Raylib_cs;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static RayGui_cs.Utf8Marshal;

namespace RayGui_cs
{
    public static class Gui
    {
        // GuiResult values returned by raygui controls.
        private const int ResultPressed = 1;
        private const int ResultTabClose = 4;

        // Must match RAYGUI_ICON_SIZE, RAYGUI_ICON_MAX_ICONS and RAYGUI_ICON_MAX_NAME_LENGTH in the native build.
        private const int IconSize = 16;
        private const int IconDataElements = IconSize * IconSize / 32;
        private const int IconMaxIcons = 512;
        private const int IconMaxNameLength = 32;

        // Must match RAYGUI_TEXTSPLIT_MAX_ITEMS and RAYGUI_TEXTSPLIT_MAX_TEXT_SIZE in the native build.
        private const int TextSplitMaxItems = 128;
        private const int TextSplitMaxTextSize = 1024;

        // Must match RAYGUI_VALUEBOX_MAX_CHARS in the native build.
        private const int ValueBoxMaxChars = 32;

        // Mirrors of native state that raygui has no getters for.
        private static float globalAlpha = 1f;
        private static bool tooltipsEnabled;
        private static string? tooltip;
        private static IntPtr tooltipPtr;
        private static int iconScale = 1;

        // Number of icons in raygui's current icon table, which shrinks when a smaller icon set is loaded.
        private static int iconCount = IconMaxIcons;

        #region Global gui state control functions

        /// <summary>Enables gui controls if they are currently disabled.</summary>
        [Reviewed]
        public static void Enable()
        {
            RayguiNative.GuiEnable();
        }

        /// <summary>Disables gui controls if they are currently in the normal state.</summary>
        [Reviewed]
        public static void Disable()
        {
            RayguiNative.GuiDisable();
        }

        /// <summary>Gets or sets whether gui controls are locked and ignore input.</summary>
        public static bool IsLocked
        {
            get => RayguiNative.GuiIsLocked();
            set
            {
                if (value)
                {
                    RayguiNative.GuiLock();
                }
                else
                {
                    RayguiNative.GuiUnlock();
                }
            }
        }

        /// <summary>Gets or sets the gui controls transparency, clamped to the range 0 to 1.</summary>
        public static float Alpha
        {
            // raygui has no getter, and only GuiSetAlpha writes the native value, so it is mirrored here.
            get => globalAlpha;
            set
            {
                globalAlpha = Math.Clamp(value, 0f, 1f);
                RayguiNative.GuiSetAlpha(globalAlpha);
            }
        }

        /// <summary>Gets or sets the state applied to the gui controls drawn next.</summary>
        public static GuiState State
        {
            get => (GuiState)RayguiNative.GuiGetState();
            set => RayguiNative.GuiSetState((int)value);
        }

        /// <summary>Gets or sets the font used by gui controls.</summary>
        public static Font Font
        {
            get => RayguiNative.GuiGetFont();
            set => RayguiNative.GuiSetFont(value);
        }

        #endregion

        #region Tooltip functions

        // raygui has no tooltip getters, and only these properties write the native values, so they are mirrored here.

        /// <summary>Gets or sets whether tooltips are shown for the gui controls drawn next.</summary>
        public static bool TooltipsEnabled
        {
            get => tooltipsEnabled;
            set
            {
                tooltipsEnabled = value;
                if (value)
                {
                    RayguiNative.GuiEnableTooltip();
                }
                else
                {
                    RayguiNative.GuiDisableTooltip();
                }
            }
        }

        /// <summary>Gets or sets the tooltip text shown for the gui controls drawn next, or null for none.</summary>
        public static string? Tooltip
        {
            get => tooltip;
            set
            {
                // raygui keeps the pointer, so the native string must stay alive until it is replaced.
                IntPtr previous = tooltipPtr;
                tooltipPtr = value is null ? IntPtr.Zero : Marshal.StringToCoTaskMemUTF8(value);
                unsafe
                {
                    RayguiNative.GuiSetTooltip((sbyte*)tooltipPtr);
                }
                Marshal.FreeCoTaskMem(previous);
                tooltip = value;
            }
        }

        #endregion

        #region Icon functions

        /// <summary>Returns text prefixed with an icon, for use as any control's text.</summary>
        public static string IconText(GuiIconName icon, string? text = null)
        {
            // Same "#NNN#" format GuiIconText produces, without its static buffer and 1024 byte limit.
            return $"#{(int)icon:D3}#{text}";
        }

        /// <summary>Gets or sets the scale icons are drawn at inside control text.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is less than 1.</exception>
        public static int IconScale
        {
            // raygui has no getter, and only GuiSetIconScale writes the native value, so it is mirrored here.
            get => iconScale;
            set
            {
                // raygui silently ignores values below 1.
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
                iconScale = value;
                RayguiNative.GuiSetIconScale(value);
            }
        }

        // GuiGetIcons is intentionally not exposed: it returns a raw pointer into raygui's icon table.

        /// <summary>Loads a raygui icons file (.rgi), replacing the current icons.</summary>
        /// <returns>The name of each loaded icon, in icon id order.</returns>
        /// <exception cref="FileNotFoundException">The file does not exist.</exception>
        /// <exception cref="ArgumentException">The file is not a compatible raygui icons file.</exception>
        public static IReadOnlyList<string> LoadIcons(string fileName)
        {
            // Read here rather than through GuiLoadIcons, which leaks its file buffer.
            return LoadIcons(File.ReadAllBytes(fileName));
        }

        /// <summary>Loads raygui icons (.rgi) from memory, replacing the current icons.</summary>
        /// <returns>The name of each loaded icon, in icon id order.</returns>
        /// <exception cref="ArgumentException">The data is not a compatible raygui icons file.</exception>
        public static IReadOnlyList<string> LoadIcons(ReadOnlySpan<byte> data)
        {
            // raygui reads the data without bounds checks and draws icons assuming its compiled icon size.
            const int HeaderSize = 12;
            if (data.Length < HeaderSize || !data[..4].SequenceEqual("rGI "u8))
            {
                throw new ArgumentException("Data is not a raygui icons file.", nameof(data));
            }

            int count = BitConverter.ToInt16(data[8..10]);
            int size = BitConverter.ToInt16(data[10..12]);
            if (size != IconSize)
            {
                throw new ArgumentException($"Icons must be {IconSize}x{IconSize} pixels, but are {size}x{size}.", nameof(data));
            }

            if (count <= 0)
            {
                throw new ArgumentException("Icons file contains no icons.", nameof(data));
            }

            int iconDataSize = IconDataElements * sizeof(uint);
            if (HeaderSize + (long)count * (IconMaxNameLength + iconDataSize) > data.Length)
            {
                throw new ArgumentException("Icons data is truncated.", nameof(data));
            }

            // Names are read here, since raygui returns them in memory the caller cannot safely free.
            string[] names = new string[count];
            for (int i = 0; i < count; i++)
            {
                ReadOnlySpan<byte> name = data.Slice(HeaderSize + i * IconMaxNameLength, IconMaxNameLength);
                int length = name.IndexOf((byte)0);
                names[i] = Encoding.UTF8.GetString(length < 0 ? name : name[..length]);
            }

            unsafe
            {
                fixed (byte* dataPtr = data)
                {
                    RayguiNative.GuiLoadIconsFromMemory(dataPtr, data.Length, false);
                }
            }
            iconCount = count;
            return names;
        }

        /// <summary>Draws an icon, with each icon pixel drawn as a pixelSize square.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The icon is not in the loaded icon set.</exception>
        public static void DrawIcon(GuiIconName icon, int posX, int posY, int pixelSize, Color color)
        {
            // raygui indexes its icon table without bounds checks.
            ArgumentOutOfRangeException.ThrowIfNegative((int)icon, nameof(icon));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((int)icon, iconCount, nameof(icon));
            RayguiNative.GuiDrawIcon((int)icon, posX, posY, pixelSize, color);
        }

        /// <inheritdoc cref="DrawIcon(GuiIconName, int, int, int, Color)"/>
        public static void DrawIcon(GuiIconName icon, Vector2 position, int pixelSize, Color color)
        {
            DrawIcon(icon, (int)position.X, (int)position.Y, pixelSize, color);
        }

        #endregion

        #region Utility functions

        /// <summary>Gets the width of text as gui controls draw it, including any icon prefix.</summary>
        public static int GetTextWidth(string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiGetTextWidth((sbyte*)textPtr);
                }
            }
        }

        #endregion

        #region Container/separator controls

        /// <summary>Draws a window box.</summary>
        /// <returns>True when the close button was clicked.</returns>
        public static bool WindowBox(Rectangle bounds, string? title)
        {
            unsafe
            {
                fixed (byte* titlePtr = ToUtf8(title))
                {
                    return RayguiNative.GuiWindowBox(bounds, (sbyte*)titlePtr) == ResultPressed;
                }
            }
        }

        /// <summary>Draws a group box with an optional name.</summary>
        public static void GroupBox(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiGroupBox(bounds, (sbyte*)textPtr);
                }
            }
        }

        /// <summary>Draws a line separator with optional text.</summary>
        public static void Line(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiLine(bounds, (sbyte*)textPtr);
                }
            }
        }

        /// <summary>Draws a panel, with a header when text is not null.</summary>
        /// <returns>True when the header was clicked.</returns>
        public static bool Panel(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiPanel(bounds, (sbyte*)textPtr) == ResultPressed;
                }
            }
        }

        /// <summary>Draws a scroll panel, with a header when text is not null.</summary>
        /// <param name="content">The full size of the scrollable content.</param>
        /// <param name="scroll">The scroll offset returned by the previous frame.</param>
        public static ScrollPanelResult ScrollPanel(Rectangle bounds, string? text, Rectangle content, Vector2 scroll)
        {
            Rectangle view;
            int result;
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    result = RayguiNative.GuiScrollPanel(bounds, (sbyte*)textPtr, content, &scroll, &view);
                }
            }
            return new ScrollPanelResult(scroll, view) { HeaderClicked = result == ResultPressed };
        }

        #endregion

        #region Basic controls

        /// <summary>Draws a text label.</summary>
        public static void Label(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiLabel(bounds, (sbyte*)textPtr);
                }
            }
        }

        /// <summary>Draws a button.</summary>
        /// <returns>True when the button was clicked.</returns>
        public static bool Button(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiButton(bounds, (sbyte*)textPtr) == ResultPressed;
                }
            }
        }

        /// <summary>Draws a label that behaves as a button.</summary>
        /// <returns>True when the label was clicked.</returns>
        public static bool LabelButton(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiLabelButton(bounds, (sbyte*)textPtr) == ResultPressed;
                }
            }
        }

        /// <summary>Draws a status bar.</summary>
        /// <returns>True when the status bar was clicked.</returns>
        public static bool StatusBar(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiStatusBar(bounds, (sbyte*)textPtr) == ResultPressed;
                }
            }
        }

        /// <summary>Draws a placeholder rectangle.</summary>
        /// <returns>True when the placeholder was clicked.</returns>
        public static bool DummyRec(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiDummyRec(bounds, (sbyte*)textPtr) == ResultPressed;
                }
            }
        }

        /// <summary>Draws a grid.</summary>
        /// <param name="spacing">The size of each cell.</param>
        /// <param name="subdivs">The number of subdivision lines inside each cell.</param>
        /// <returns>The column and row of the cell under the mouse, or null when the mouse is not over the grid.</returns>
        public static Vector2? Grid(Rectangle bounds, string? text, float spacing, int subdivs)
        {
            Vector2 mouseCell;
            int result;
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    result = RayguiNative.GuiGrid(bounds, (sbyte*)textPtr, spacing, subdivs, &mouseCell);
                }
            }
            // raygui reports the mouse being over the grid as "pressed".
            return result == ResultPressed ? mouseCell : null;
        }

        #endregion

        #region Selection controls

        /// <summary>Draws a toggle button.</summary>
        /// <param name="active">Whether the toggle is currently on.</param>
        /// <returns>Whether the toggle is on after this frame's input.</returns>
        public static bool Toggle(Rectangle bounds, string? text, bool active)
        {
            CBool nativeActive = active;
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiToggle(bounds, (sbyte*)textPtr, &nativeActive);
                }
            }
            return nativeActive;
        }

        /// <summary>Draws a group of toggle buttons, where only one can be on.</summary>
        /// <param name="bounds">The bounds of the first toggle.</param>
        /// <param name="text">The toggle labels, separated by ';' for columns and '\n' for rows.</param>
        /// <param name="active">The index of the toggle that is currently on.</param>
        /// <returns>The index of the toggle that is on after this frame's input.</returns>
        public static int ToggleGroup(Rectangle bounds, string text, int active)
        {
            ArgumentNullException.ThrowIfNull(text);
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiToggleGroup(bounds, (sbyte*)textPtr, &active);
                }
            }
            return active;
        }

        /// <summary>Draws a row of toggle buttons, where only one can be on.</summary>
        /// <param name="bounds">The bounds of the first toggle.</param>
        /// <param name="items">The toggle labels.</param>
        /// <param name="active">The index of the toggle that is currently on.</param>
        /// <returns>The index of the toggle that is on after this frame's input.</returns>
        public static int ToggleGroup(Rectangle bounds, IEnumerable<string> items, int active)
        {
            return ToggleGroup(bounds, JoinItems(items), active);
        }

        /// <summary>Draws a slider that toggles between items when clicked.</summary>
        /// <param name="text">The item labels, separated by ';'.</param>
        /// <param name="active">The index of the current item.</param>
        /// <returns>The index of the current item after this frame's input.</returns>
        public static int ToggleSlider(Rectangle bounds, string text, int active)
        {
            // raygui indexes the item list without checking for negative values.
            CountItems(text);
            ArgumentOutOfRangeException.ThrowIfNegative(active);
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiToggleSlider(bounds, (sbyte*)textPtr, &active);
                }
            }
            return active;
        }

        /// <summary>Draws a slider that toggles between items when clicked.</summary>
        /// <param name="items">The item labels.</param>
        /// <param name="active">The index of the current item.</param>
        /// <returns>The index of the current item after this frame's input.</returns>
        public static int ToggleSlider(Rectangle bounds, IEnumerable<string> items, int active)
        {
            return ToggleSlider(bounds, JoinItems(items), active);
        }

        /// <summary>Draws a check box with an optional label.</summary>
        /// <param name="isChecked">Whether the box is currently checked.</param>
        /// <returns>Whether the box is checked after this frame's input.</returns>
        public static bool CheckBox(Rectangle bounds, string? text, bool isChecked)
        {
            CBool nativeChecked = isChecked;
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiCheckBox(bounds, (sbyte*)textPtr, &nativeChecked);
                }
            }
            return nativeChecked;
        }

        /// <summary>Draws a combo box that cycles through items when clicked.</summary>
        /// <param name="text">The item labels, separated by ';'.</param>
        /// <param name="active">The index of the selected item; out of range values are clamped.</param>
        /// <returns>The index of the selected item after this frame's input.</returns>
        public static int ComboBox(Rectangle bounds, string text, int active)
        {
            CountItems(text);
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiComboBox(bounds, (sbyte*)textPtr, &active);
                }
            }
            return active;
        }

        /// <summary>Draws a combo box that cycles through items when clicked.</summary>
        /// <param name="items">The item labels.</param>
        /// <param name="active">The index of the selected item; out of range values are clamped.</param>
        /// <returns>The index of the selected item after this frame's input.</returns>
        public static int ComboBox(Rectangle bounds, IEnumerable<string> items, int active)
        {
            return ComboBox(bounds, JoinItems(items), active);
        }

        /// <summary>Draws a dropdown box.</summary>
        /// <param name="text">The item labels, separated by ';'.</param>
        /// <param name="active">The index of the selected item.</param>
        /// <param name="isOpen">Whether the item list is currently open.</param>
        /// <returns>The selected item and open state after this frame's input.</returns>
        public static DropdownBoxResult DropdownBox(Rectangle bounds, string text, int active, bool isOpen)
        {
            // raygui indexes the item list without range checks.
            int count = CountItems(text);
            ArgumentOutOfRangeException.ThrowIfNegative(active);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(active, count);
            int result;
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    result = RayguiNative.GuiDropdownBox(bounds, (sbyte*)textPtr, &active, isOpen);
                }
            }
            // raygui returns non-zero when the list should open or close, including when an item is selected.
            return new DropdownBoxResult(active, result != 0 ? !isOpen : isOpen);
        }

        /// <inheritdoc cref="DropdownBox(Rectangle, string, int, bool)"/>
        /// <param name="items">The item labels.</param>
        public static DropdownBoxResult DropdownBox(Rectangle bounds, IEnumerable<string> items, int active, bool isOpen)
        {
            return DropdownBox(bounds, JoinItems(items), active, isOpen);
        }

        /// <summary>Draws an integer value box with decrement and increment buttons.</summary>
        /// <param name="value">The current value.</param>
        /// <param name="isEditing">Whether the value is currently being typed in.</param>
        /// <returns>The value and editing state after this frame's input.</returns>
        public static ValueBoxResult Spinner(Rectangle bounds, string? text, int value, int minValue, int maxValue, bool isEditing)
        {
            ValidateRange(minValue, maxValue);
            int result;
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    result = RayguiNative.GuiSpinner(bounds, (sbyte*)textPtr, &value, minValue, maxValue, isEditing);
                }
            }
            return new ValueBoxResult(value, ToggleIfPressed(result, isEditing));
        }

        /// <summary>Draws an integer value box that can be typed in.</summary>
        /// <param name="value">The current value.</param>
        /// <param name="isEditing">Whether the value is currently being typed in.</param>
        /// <returns>The value and editing state after this frame's input.</returns>
        public static ValueBoxResult ValueBox(Rectangle bounds, string? text, int value, int minValue, int maxValue, bool isEditing)
        {
            ValidateRange(minValue, maxValue);
            int result;
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    result = RayguiNative.GuiValueBox(bounds, (sbyte*)textPtr, &value, minValue, maxValue, isEditing);
                }
            }
            return new ValueBoxResult(value, ToggleIfPressed(result, isEditing));
        }

        /// <summary>Draws a floating point value box that can be typed in.</summary>
        /// <param name="value">The current value.</param>
        /// <param name="valueText">The text being typed, from the previous frame's result; ignored when not editing.</param>
        /// <param name="isEditing">Whether the value is currently being typed in.</param>
        /// <returns>The value, typed text and editing state after this frame's input.</returns>
        public static FloatValueBoxResult FloatValueBox(Rectangle bounds, string? text, float value, string? valueText, bool isEditing)
        {
            // raygui never formats the value itself, so the text is regenerated whenever it is not being typed.
            if (!isEditing || valueText is null)
            {
                valueText = value.ToString("0.######", CultureInfo.InvariantCulture);
            }

            byte[] valueTextBuffer = ToUtf8Buffer(valueText, ValueBoxMaxChars + 1);
            int result;
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (byte* valueTextPtr = valueTextBuffer)
                {
                    result = RayguiNative.GuiValueBoxFloat(bounds, (sbyte*)textPtr, (sbyte*)valueTextPtr, &value, isEditing);
                }
            }
            return new FloatValueBoxResult(value, FromUtf8Buffer(valueTextBuffer), ToggleIfPressed(result, isEditing));
        }

        #endregion

        #region Text and value controls

        /// <summary>Draws a single line text box that can be typed in.</summary>
        /// <param name="text">The current text.</param>
        /// <param name="maxByteCount">The maximum length of the text in UTF-8 bytes.</param>
        /// <param name="isEditing">Whether the text is currently being typed in.</param>
        /// <returns>The text and editing state after this frame's input.</returns>
        public static TextBoxResult TextBox(Rectangle bounds, string? text, int maxByteCount, bool isEditing)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxByteCount);
            // raygui's size includes the null terminator.
            int textSize = maxByteCount + 1;
            byte[] textBuffer = ToUtf8Buffer(text, textSize);
            int result;
            unsafe
            {
                fixed (byte* textPtr = textBuffer)
                {
                    result = RayguiNative.GuiTextBox(bounds, (sbyte*)textPtr, textSize, isEditing);
                }
            }
            return new TextBoxResult(FromUtf8Buffer(textBuffer), ToggleIfPressed(result, isEditing));
        }

        // GuiTextBoxMulti was removed in raygui 5.0, which has no multiline text editing control.

        /// <summary>Draws a slider with optional labels on each side.</summary>
        /// <param name="value">The current value.</param>
        /// <returns>The value after this frame's input.</returns>
        public static float Slider(Rectangle bounds, string? textLeft, string? textRight, float value, float minValue, float maxValue)
        {
            ValidateRange(minValue, maxValue);
            unsafe
            {
                fixed (byte* textLeftPtr = ToUtf8(textLeft))
                fixed (byte* textRightPtr = ToUtf8(textRight))
                {
                    RayguiNative.GuiSlider(bounds, (sbyte*)textLeftPtr, (sbyte*)textRightPtr, &value, minValue, maxValue);
                }
            }
            return value;
        }

        /// <summary>Draws a filled slider bar with optional labels on each side.</summary>
        /// <param name="value">The current value.</param>
        /// <returns>The value after this frame's input.</returns>
        public static float SliderBar(Rectangle bounds, string? textLeft, string? textRight, float value, float minValue, float maxValue)
        {
            ValidateRange(minValue, maxValue);
            unsafe
            {
                fixed (byte* textLeftPtr = ToUtf8(textLeft))
                fixed (byte* textRightPtr = ToUtf8(textRight))
                {
                    RayguiNative.GuiSliderBar(bounds, (sbyte*)textLeftPtr, (sbyte*)textRightPtr, &value, minValue, maxValue);
                }
            }
            return value;
        }

        /// <summary>Draws a progress bar with optional labels on each side.</summary>
        /// <param name="value">The progress to show, between minValue and maxValue.</param>
        public static void ProgressBar(Rectangle bounds, string? textLeft, string? textRight, float value, float minValue, float maxValue)
        {
            ValidateRange(minValue, maxValue);
            unsafe
            {
                fixed (byte* textLeftPtr = ToUtf8(textLeft))
                fixed (byte* textRightPtr = ToUtf8(textRight))
                {
                    // raygui only clamps the value for drawing.
                    RayguiNative.GuiProgressBar(bounds, (sbyte*)textLeftPtr, (sbyte*)textRightPtr, &value, minValue, maxValue);
                }
            }
        }

        #endregion

        #region Advanced controls

        /// <summary>Draws a scrollable list of selectable items.</summary>
        /// <param name="text">The item labels, separated by ';' or '\n'.</param>
        /// <param name="scrollIndex">The index of the first visible item.</param>
        /// <param name="active">The index of the selected item, or null for none.</param>
        /// <returns>The scroll position and selection after this frame's input.</returns>
        public static ListViewResult ListView(Rectangle bounds, string? text, int scrollIndex, int? active)
        {
            // Split here like GuiListView does, but without GuiTextSplit's item count and text size limits.
            string[] items = text?.Split([';', '\n']) ?? [];
            return ListView(bounds, items, scrollIndex, active);
        }

        /// <summary>Draws a scrollable list of selectable items.</summary>
        /// <param name="items">The item labels.</param>
        /// <param name="scrollIndex">The index of the first visible item.</param>
        /// <param name="active">The index of the selected item, or null for none.</param>
        /// <returns>The scroll position and selection after this frame's input.</returns>
        public static ListViewResult ListView(Rectangle bounds, IReadOnlyList<string> items, int scrollIndex, int? active)
        {
            ArgumentNullException.ThrowIfNull(items);
            // raygui uses -1 for no selection and no focused item.
            int nativeActive = active ?? -1;
            int focus = -1;
            unsafe
            {
                sbyte** itemsPtr = AllocUtf8Array(items);
                try
                {
                    RayguiNative.GuiListViewEx(bounds, itemsPtr, items.Count, &scrollIndex, &nativeActive, &focus);
                }
                finally
                {
                    FreeUtf8Array(itemsPtr, items.Count);
                }
            }
            return new ListViewResult(scrollIndex, nativeActive >= 0 ? nativeActive : null)
            {
                Focused = focus >= 0 ? focus : null,
            };
        }

        /// <summary>Draws a bar of tabs, where one tab is active.</summary>
        /// <param name="bounds">The bounds of the bar; each tab is as wide as <see cref="GuiTabBarProperty.TabItemsWidth"/>.</param>
        /// <param name="text">The tab labels, separated by ';' or '\n'.</param>
        /// <param name="active">The index of the active tab; out of range values are clamped.</param>
        /// <returns>The active tab and any tab the user asked to close after this frame's input.</returns>
        public static TabBarResult TabBar(Rectangle bounds, string? text, int active)
        {
            // Split here like GuiTabBar does, but without GuiTextSplit's item count and text size limits.
            string[] items = text?.Split([';', '\n']) ?? [];
            return TabBar(bounds, items, active);
        }

        /// <inheritdoc cref="TabBar(Rectangle, string?, int)"/>
        /// <param name="items">The tab labels.</param>
        public static TabBarResult TabBar(Rectangle bounds, IReadOnlyList<string> items, int active)
        {
            ArgumentNullException.ThrowIfNull(items);
            int count = items.Count;
            // raygui clamps the active tab before laying the tabs out.
            int layoutActive = count > 0 ? Math.Clamp(active, 0, count - 1) : -1;
            // raygui 5.0 ignores the scroll and focus arguments.
            int hscroll = 0;
            int focus = -1;
            int result;
            unsafe
            {
                sbyte** itemsPtr = AllocUtf8Array(items);
                try
                {
                    result = RayguiNative.GuiTabBarEx(bounds, itemsPtr, count, &hscroll, &active, &focus);
                }
                finally
                {
                    FreeUtf8Array(itemsPtr, count);
                }
            }

            int? closedTab = result == ResultTabClose ? FindTabUnderPointer(bounds, count, layoutActive) ?? active : null;
            return new TabBarResult(Math.Max(active, 0), closedTab);
        }

        // raygui reports a close request without saying which tab it is for, but the pointer is always over that tab,
        // so find it using the same layout GuiTabBarEx uses.
        private static int? FindTabUnderPointer(Rectangle bounds, int count, int active)
        {
            int tabItemsWidth = RayguiNative.GuiGetStyle((int)GuiControl.TabBar, (int)GuiTabBarProperty.TabItemsWidth);
            int screenWidth = Raylib.GetScreenWidth();
            int offsetX = Math.Max(active * tabItemsWidth - screenWidth, 0);
            Vector2 pointer = Raylib.GetMousePosition();
            for (int i = 0; i < count; i++)
            {
                Rectangle tabBounds = new(bounds.X + (tabItemsWidth + 4) * i + offsetX, bounds.Y, tabItemsWidth, bounds.Height);
                if (tabBounds.X < screenWidth && Raylib.CheckCollisionPointRec(pointer, tabBounds))
                {
                    return i;
                }
            }
            return null;
        }

        /// <summary>Draws a message box with a row of buttons.</summary>
        /// <param name="buttons">The button labels, separated by ';'.</param>
        /// <returns>Which button was clicked, or whether the box was closed, this frame.</returns>
        public static MessageBoxResult MessageBox(Rectangle bounds, string? title, string? message, string buttons)
        {
            CountItems(buttons);
            // raygui writes 0 for the close button and 1 based indexes for the buttons, and only when one was clicked.
            int btnActive = -1;
            unsafe
            {
                fixed (byte* titlePtr = ToUtf8(title))
                fixed (byte* messagePtr = ToUtf8(message))
                fixed (byte* buttonsPtr = ToUtf8(buttons))
                {
                    RayguiNative.GuiMessageBox(bounds, (sbyte*)titlePtr, (sbyte*)messagePtr, (sbyte*)buttonsPtr, &btnActive);
                }
            }
            return ToDialogResult(btnActive);
        }

        /// <inheritdoc cref="MessageBox(Rectangle, string?, string?, string)"/>
        /// <param name="buttons">The button labels.</param>
        public static MessageBoxResult MessageBox(Rectangle bounds, string? title, string? message, IEnumerable<string> buttons)
        {
            return MessageBox(bounds, title, message, JoinItems(buttons));
        }

        private static MessageBoxResult ToDialogResult(int btnActive)
        {
            return new MessageBoxResult(btnActive > 0 ? btnActive - 1 : null, btnActive == 0);
        }

        /// <summary>Draws a message box with a text box and a row of buttons.</summary>
        /// <param name="text">The current text.</param>
        /// <param name="maxByteCount">The maximum length of the text in UTF-8 bytes.</param>
        /// <param name="buttons">The button labels, separated by ';'.</param>
        /// <param name="secretViewActive">
        /// For secret input, whether the text is currently shown instead of hidden; null for plain input without the show/hide toggle.
        /// </param>
        /// <returns>The text, and which button was clicked or whether the box was closed, after this frame's input.</returns>
        public static TextInputBoxResult TextInputBox(Rectangle bounds, string? title, string? message, string? text, int maxByteCount, string buttons, bool? secretViewActive = null)
        {
            CountItems(buttons);
            ArgumentOutOfRangeException.ThrowIfNegative(maxByteCount);
            // raygui's size includes the null terminator.
            int textSize = maxByteCount + 1;
            byte[] textBuffer = ToUtf8Buffer(text, textSize);
            // raygui writes 0 for the close button and 1 based indexes for the buttons, and only when one was clicked.
            int btnActive = -1;
            CBool nativeSecretViewActive = secretViewActive ?? false;
            unsafe
            {
                fixed (byte* titlePtr = ToUtf8(title))
                fixed (byte* messagePtr = ToUtf8(message))
                fixed (byte* textPtr = textBuffer)
                fixed (byte* buttonsPtr = ToUtf8(buttons))
                {
                    RayguiNative.GuiTextInputBox(bounds, (sbyte*)titlePtr, (sbyte*)messagePtr, (sbyte*)textPtr, textSize, (sbyte*)buttonsPtr, &btnActive,
                        secretViewActive.HasValue ? &nativeSecretViewActive : null);
                }
            }

            MessageBoxResult dialog = ToDialogResult(btnActive);
            return new TextInputBoxResult(FromUtf8Buffer(textBuffer), dialog.ClickedButton, dialog.Closed)
            {
                SecretViewActive = secretViewActive.HasValue ? (bool)nativeSecretViewActive : null,
            };
        }

        /// <inheritdoc cref="TextInputBox(Rectangle, string?, string?, string?, int, string, bool?)"/>
        /// <param name="buttons">The button labels.</param>
        public static TextInputBoxResult TextInputBox(Rectangle bounds, string? title, string? message, string? text, int maxByteCount, IEnumerable<string> buttons, bool? secretViewActive = null)
        {
            return TextInputBox(bounds, title, message, text, maxByteCount, JoinItems(buttons), secretViewActive);
        }

        #endregion

        #region Color controls

        /// <summary>Draws a color picker: a color panel with a hue bar.</summary>
        /// <param name="color">The current color.</param>
        /// <returns>The color after this frame's input.</returns>
        public static Color ColorPicker(Rectangle bounds, string? text, Color color)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiColorPicker(bounds, (sbyte*)textPtr, &color);
                }
            }
            return color;
        }

        /// <summary>Draws a color panel for picking saturation and value.</summary>
        /// <param name="color">The current color.</param>
        /// <returns>The color after this frame's input.</returns>
        public static Color ColorPanel(Rectangle bounds, string? text, Color color)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiColorPanel(bounds, (sbyte*)textPtr, &color);
                }
            }
            return color;
        }

        /// <summary>Draws a horizontal bar for picking an alpha value.</summary>
        /// <param name="alpha">The current alpha, from 0 to 1.</param>
        /// <returns>The alpha after this frame's input.</returns>
        public static float ColorBarAlpha(Rectangle bounds, string? text, float alpha)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiColorBarAlpha(bounds, (sbyte*)textPtr, &alpha);
                }
            }
            return alpha;
        }

        /// <summary>Draws a vertical bar for picking a hue.</summary>
        /// <param name="hue">The current hue, in degrees from 0 to 359.</param>
        /// <returns>The hue after this frame's input.</returns>
        public static float ColorBarHue(Rectangle bounds, string? text, float hue)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiColorBarHue(bounds, (sbyte*)textPtr, &hue);
                }
            }
            return hue;
        }

        /// <summary>Draws a color picker that works in hue, saturation and value.</summary>
        /// <param name="colorHsv">The current color: X is hue in degrees from 0 to 359, Y is saturation and Z is value, both from 0 to 1.</param>
        /// <returns>The color after this frame's input.</returns>
        public static Vector3 ColorPickerHsv(Rectangle bounds, string? text, Vector3 colorHsv)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiColorPickerHSV(bounds, (sbyte*)textPtr, &colorHsv);
                }
            }
            return colorHsv;
        }

        /// <summary>Draws a color panel for picking saturation and value, working in hue, saturation and value.</summary>
        /// <param name="colorHsv">The current color: X is hue in degrees from 0 to 359, Y is saturation and Z is value, both from 0 to 1.</param>
        /// <returns>The color after this frame's input.</returns>
        public static Vector3 ColorPanelHsv(Rectangle bounds, string? text, Vector3 colorHsv)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    RayguiNative.GuiColorPanelHSV(bounds, (sbyte*)textPtr, &colorHsv);
                }
            }
            return colorHsv;
        }

        #endregion

        // Joins items into raygui's ';' separated list format.
        private static string JoinItems(IEnumerable<string> items)
        {
            ArgumentNullException.ThrowIfNull(items);
            return string.Join(';', items);
        }

        private static void ValidateRange<T>(T minValue, T maxValue, [CallerArgumentExpression(nameof(minValue))] string? paramName = null)
            where T : IComparable<T>
        {
            if (minValue.CompareTo(maxValue) > 0)
            {
                throw new ArgumentException($"{paramName} must not be greater than the maximum.", paramName);
            }
        }

        // raygui returns RESULT_PRESSED when an editable control should enter or leave edit mode.
        private static bool ToggleIfPressed(int result, bool isEditing) => result == ResultPressed ? !isEditing : isEditing;

        // Validates text that raygui splits into items, and returns the number of items raygui produces from it.
        private static int CountItems(string text, [CallerArgumentExpression(nameof(text))] string? paramName = null)
        {
            // raygui splits without checking for NULL, and reads past its split buffer when the text does not fit.
            ArgumentNullException.ThrowIfNull(text, paramName);
            if (Encoding.UTF8.GetByteCount(text) >= TextSplitMaxTextSize)
            {
                throw new ArgumentException($"Item text must be shorter than {TextSplitMaxTextSize} UTF-8 bytes.", paramName);
            }

            int count = 1 + text.AsSpan().Count(';') + text.AsSpan().Count('\n');
            return Math.Min(count, TextSplitMaxItems);
        }
    }
}
