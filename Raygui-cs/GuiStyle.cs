using Raylib_cs;

namespace RayGui_cs
{
    /// <summary>Reads, writes and loads raygui style properties.</summary>
    public static class GuiStyle
    {
        // Must match RAYGUI_MAX_CONTROLS, RAYGUI_MAX_PROPS_BASE and RAYGUI_MAX_PROPS_EXTENDED in the native build.
        private const int MaxControls = 16;
        private const int MaxProperties = 16 + 8;

        #region Base properties

        /// <summary>Sets a base property; setting it on <see cref="GuiControl.Default"/> applies it to every control.</summary>
        public static void Set(GuiControl control, GuiControlProperty property, int value) => SetRaw(control, (int)property, value);

        /// <summary>Sets an enum-valued base property, such as <see cref="GuiControlProperty.TextAlignment"/>.</summary>
        public static void Set<TValue>(GuiControl control, GuiControlProperty property, TValue value) where TValue : struct, Enum => SetRaw(control, (int)property, Convert.ToInt32(value));

        /// <summary>Sets a color base property; setting it on <see cref="GuiControl.Default"/> applies it to every control.</summary>
        public static void SetColor(GuiControl control, GuiControlProperty property, Color value) => SetRaw(control, (int)property, ToStyleValue(value));

        public static int Get(GuiControl control, GuiControlProperty property) => GetRaw(control, (int)property);

        public static Color GetColor(GuiControl control, GuiControlProperty property) => ToColor(GetRaw(control, (int)property));

        #endregion

        #region Default (global) properties

        public static void Set(GuiDefaultProperty property, int value) => SetRaw(GuiControl.Default, (int)property, value);

        /// <summary>Sets an enum-valued global property, such as <see cref="GuiDefaultProperty.TextWrapMode"/>.</summary>
        public static void Set<TValue>(GuiDefaultProperty property, TValue value) where TValue : struct, Enum => SetRaw(GuiControl.Default, (int)property, Convert.ToInt32(value));

        public static void SetColor(GuiDefaultProperty property, Color value) => SetRaw(GuiControl.Default, (int)property, ToStyleValue(value));

        public static int Get(GuiDefaultProperty property) => GetRaw(GuiControl.Default, (int)property);

        public static Color GetColor(GuiDefaultProperty property) => ToColor(GetRaw(GuiControl.Default, (int)property));

        #endregion

        #region Control specific properties

        public static void Set(GuiToggleProperty property, int value) => SetRaw(GuiControl.Toggle, (int)property, value);

        public static void Set(GuiToggleProperty property, bool value) => Set(property, value ? 1 : 0);

        public static int Get(GuiToggleProperty property) => GetRaw(GuiControl.Toggle, (int)property);

        public static void Set(GuiSliderProperty property, int value) => SetRaw(GuiControl.Slider, (int)property, value);

        public static int Get(GuiSliderProperty property) => GetRaw(GuiControl.Slider, (int)property);

        public static void Set(GuiProgressBarProperty property, int value) => SetRaw(GuiControl.ProgressBar, (int)property, value);

        public static int Get(GuiProgressBarProperty property) => GetRaw(GuiControl.ProgressBar, (int)property);

        public static void Set(GuiScrollBarProperty property, int value) => SetRaw(GuiControl.ScrollBar, (int)property, value);

        public static void Set(GuiScrollBarProperty property, bool value) => Set(property, value ? 1 : 0);

        public static int Get(GuiScrollBarProperty property) => GetRaw(GuiControl.ScrollBar, (int)property);

        public static void Set(GuiCheckBoxProperty property, int value) => SetRaw(GuiControl.CheckBox, (int)property, value);

        public static int Get(GuiCheckBoxProperty property) => GetRaw(GuiControl.CheckBox, (int)property);

        public static void Set(GuiComboBoxProperty property, int value) => SetRaw(GuiControl.ComboBox, (int)property, value);

        public static int Get(GuiComboBoxProperty property) => GetRaw(GuiControl.ComboBox, (int)property);

        public static void Set(GuiDropdownBoxProperty property, int value) => SetRaw(GuiControl.DropdownBox, (int)property, value);

        public static void Set(GuiDropdownBoxProperty property, bool value) => Set(property, value ? 1 : 0);

        public static int Get(GuiDropdownBoxProperty property) => GetRaw(GuiControl.DropdownBox, (int)property);

        public static void Set(GuiTextBoxProperty property, int value) => SetRaw(GuiControl.TextBox, (int)property, value);

        public static void Set(GuiTextBoxProperty property, bool value) => Set(property, value ? 1 : 0);

        public static int Get(GuiTextBoxProperty property) => GetRaw(GuiControl.TextBox, (int)property);

        public static void Set(GuiValueBoxProperty property, int value) => SetRaw(GuiControl.ValueBox, (int)property, value);

        public static int Get(GuiValueBoxProperty property) => GetRaw(GuiControl.ValueBox, (int)property);

        public static void Set(GuiTabBarProperty property, int value) => SetRaw(GuiControl.TabBar, (int)property, value);

        public static void Set(GuiTabBarProperty property, bool value) => Set(property, value ? 1 : 0);

        public static int Get(GuiTabBarProperty property) => GetRaw(GuiControl.TabBar, (int)property);

        public static void Set(GuiListViewProperty property, int value) => SetRaw(GuiControl.ListView, (int)property, value);

        public static void Set(GuiListViewProperty property, bool value) => Set(property, value ? 1 : 0);

        public static void Set(GuiListViewProperty property, GuiScrollBarSide value) => Set(property, (int)value);

        public static int Get(GuiListViewProperty property) => GetRaw(GuiControl.ListView, (int)property);

        public static void Set(GuiColorPickerProperty property, int value) => SetRaw(GuiControl.ColorPicker, (int)property, value);

        public static int Get(GuiColorPickerProperty property) => GetRaw(GuiControl.ColorPicker, (int)property);

        #endregion

        #region Custom properties

        /// <summary>Sets a property by index, for custom controls or properties without an enum.</summary>
        public static void Set(GuiControl control, int property, int value) => SetRaw(control, property, value);

        /// <summary>Gets a property by index, for custom controls or properties without an enum.</summary>
        public static int Get(GuiControl control, int property) => GetRaw(control, property);

        #endregion

        #region Loading

        /// <summary>Loads a raygui style file (.rgs, text or binary) over the current style.</summary>
        /// <exception cref="FileNotFoundException">The file does not exist.</exception>
        public static void Load(string fileName)
        {
            ArgumentNullException.ThrowIfNull(fileName);
            // raygui silently ignores files it cannot open.
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("Style file not found.", fileName);
            }

            unsafe
            {
                fixed (byte* fileNamePtr = Utf8Marshal.ToUtf8(fileName))
                {
                    RayguiNative.GuiLoadStyle((sbyte*)fileNamePtr);
                }
            }
        }

        /// <summary>Loads a binary raygui style (.rgs) from memory over the current style.</summary>
        /// <exception cref="ArgumentException">The data is not a binary raygui style.</exception>
        public static void Load(ReadOnlySpan<byte> data)
        {
            // raygui reads the header and property table without checking the data size.
            const int HeaderSize = 12;
            const int PropertySize = 8;
            if (data.Length < HeaderSize || !data[..4].SequenceEqual("rGS "u8))
            {
                throw new ArgumentException("Data is not a binary raygui style.", nameof(data));
            }

            int propertyCount = BitConverter.ToInt32(data[8..12]);
            if (propertyCount < 0 || propertyCount > (data.Length - HeaderSize) / PropertySize)
            {
                throw new ArgumentException("Style data is truncated.", nameof(data));
            }

            unsafe
            {
                fixed (byte* dataPtr = data)
                {
                    RayguiNative.GuiLoadStyleFromMemory(dataPtr, data.Length);
                }
            }
        }

        /// <summary>Restores the default raygui style.</summary>
        [Reviewed]
        public static void LoadDefault()
        {
            RayguiNative.GuiLoadStyleDefault();
        }

        #endregion

        private static void SetRaw(GuiControl control, int property, int value)
        {
            ValidateSlot(control, property);
            RayguiNative.GuiSetStyle((int)control, property, value);
        }

        private static int GetRaw(GuiControl control, int property)
        {
            ValidateSlot(control, property);
            return RayguiNative.GuiGetStyle((int)control, property);
        }

        // raygui indexes its style array without bounds checks.
        private static void ValidateSlot(GuiControl control, int property)
        {
            ArgumentOutOfRangeException.ThrowIfNegative((int)control, nameof(control));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((int)control, MaxControls, nameof(control));
            ArgumentOutOfRangeException.ThrowIfNegative(property, nameof(property));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(property, MaxProperties, nameof(property));
        }

        // raygui packs colors as 0xRRGGBBAA.
        private static int ToStyleValue(Color color) => (color.R << 24) | (color.G << 16) | (color.B << 8) | color.A;

        private static Color ToColor(int value) => new((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
    }
}
