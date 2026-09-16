using Raylib_cs;
using System.Numerics;
using System.Runtime.InteropServices;

namespace RayGui_cs
{
    /// <summary>
    /// Raw bindings to the native raygui 5.0 shared library.
    /// The native library must be built with RAYGUI_IMPLEMENTATION and linked against the same raylib shared library that Raylib-cs loads.
    /// </summary>
    internal static unsafe class RayguiNative
    {
        internal const string NativeLibName = "raygui";

        // Global gui state control functions
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiEnable();

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiDisable();

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiLock();

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiUnlock();

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern CBool GuiIsLocked();

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiSetAlpha(float alpha);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiSetState(int state);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiGetState();

        // Font set/get functions
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiSetFont(Font font);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern Font GuiGetFont();

        // Style set/get functions
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiSetStyle(int control, int property, int value);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiGetStyle(int control, int property);

        // Styles loading functions
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiLoadStyle(sbyte* fileName);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiLoadStyleFromMemory(byte* fileData, int dataSize);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiLoadStyleDefault();

        // Tooltips management functions
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiEnableTooltip();

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiDisableTooltip();

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiSetTooltip(sbyte* tooltip);

        // Icons functionality
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern sbyte* GuiIconText(int iconId, sbyte* text);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiSetIconScale(int scale);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint* GuiGetIcons();

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern sbyte** GuiLoadIcons(sbyte* fileName, CBool loadIconsName);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern sbyte** GuiLoadIconsFromMemory(byte* fileData, int dataSize, CBool loadIconsName);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GuiDrawIcon(int iconId, int posX, int posY, int pixelSize, Color color);

        // Utility functions
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiGetTextWidth(sbyte* text);

        // Container/separator controls, useful for controls organization
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiWindowBox(Rectangle bounds, sbyte* title);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiGroupBox(Rectangle bounds, sbyte* text);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiLine(Rectangle bounds, sbyte* text);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiPanel(Rectangle bounds, sbyte* text);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiScrollPanel(Rectangle bounds, sbyte* text, Rectangle content, Vector2* scroll, Rectangle* view);

        // Basic controls set
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiLabel(Rectangle bounds, sbyte* text);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiButton(Rectangle bounds, sbyte* text);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiLabelButton(Rectangle bounds, sbyte* text);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiToggle(Rectangle bounds, sbyte* text, CBool* active);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiToggleGroup(Rectangle bounds, sbyte* text, int* active);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiToggleSlider(Rectangle bounds, sbyte* text, int* active);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiCheckBox(Rectangle bounds, sbyte* text, CBool* @checked);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiComboBox(Rectangle bounds, sbyte* text, int* active);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiDropdownBox(Rectangle bounds, sbyte* text, int* active, CBool editMode);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiSpinner(Rectangle bounds, sbyte* text, int* value, int minValue, int maxValue, CBool editMode);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiValueBox(Rectangle bounds, sbyte* text, int* value, int minValue, int maxValue, CBool editMode);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiValueBoxFloat(Rectangle bounds, sbyte* text, sbyte* textValue, float* value, CBool editMode);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiTextBox(Rectangle bounds, sbyte* text, int textSize, CBool editMode);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiSlider(Rectangle bounds, sbyte* textLeft, sbyte* textRight, float* value, float minValue, float maxValue);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiSliderBar(Rectangle bounds, sbyte* textLeft, sbyte* textRight, float* value, float minValue, float maxValue);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiProgressBar(Rectangle bounds, sbyte* textLeft, sbyte* textRight, float* value, float minValue, float maxValue);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiStatusBar(Rectangle bounds, sbyte* text);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiDummyRec(Rectangle bounds, sbyte* text);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiGrid(Rectangle bounds, sbyte* text, float spacing, int subdivs, Vector2* mouseCell);

        // Advance controls set
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiListView(Rectangle bounds, sbyte* text, int* scrollIndex, int* active);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiListViewEx(Rectangle bounds, sbyte** text, int count, int* scrollIndex, int* active, int* focus);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiTabBar(Rectangle bounds, sbyte* text, int* hscroll, int* active);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiTabBarEx(Rectangle bounds, sbyte** text, int count, int* hscroll, int* active, int* focus);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiMessageBox(Rectangle bounds, sbyte* title, sbyte* message, sbyte* btnText, int* btnActive);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiTextInputBox(Rectangle bounds, sbyte* title, sbyte* message, sbyte* text, int textSize, sbyte* btnText, int* btnActive, CBool* secretViewActive);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiColorPicker(Rectangle bounds, sbyte* text, Color* color);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiColorPanel(Rectangle bounds, sbyte* text, Color* color);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiColorBarAlpha(Rectangle bounds, sbyte* text, float* alpha);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiColorBarHue(Rectangle bounds, sbyte* text, float* value);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiColorPickerHSV(Rectangle bounds, sbyte* text, Vector3* colorHsv);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GuiColorPanelHSV(Rectangle bounds, sbyte* text, Vector3* colorHsv);
    }
}
