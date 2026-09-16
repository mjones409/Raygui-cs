using Raylib_cs;
using System.Numerics;
using System.Runtime.InteropServices;
using static RayGui_cs.Utf8Marshal;

namespace RayGui_cs
{
    public static class Gui
    {
        // raygui keeps the tooltip pointer, so the string must stay alive until it is replaced.
        private static IntPtr tooltip;

        // Must match RAYGUI_VALUEBOX_MAX_CHARS in the native build.
        private const int ValueBoxMaxChars = 32;

        #region Global gui state control functions

        [Reviewed]
        public static void GuiEnable()
        {
            RayguiNative.GuiEnable();
        }

        [Reviewed]
        public static void GuiDisable()
        {
            RayguiNative.GuiDisable();
        }

        [Reviewed]
        public static void GuiLock()
        {
            RayguiNative.GuiLock();
        }

        [Reviewed]
        public static void GuiUnlock()
        {
            RayguiNative.GuiUnlock();
        }

        [Reviewed]
        public static bool GuiIsLocked()
        {
            return RayguiNative.GuiIsLocked();
        }

        public static void GuiSetAlpha(float alpha)
        {
            RayguiNative.GuiSetAlpha(alpha);
        }

        [Reviewed]
        public static void GuiSetState(int state)
        {
            RayguiNative.GuiSetState(state);
        }

        [Reviewed]
        public static int GuiGetState()
        {
            return RayguiNative.GuiGetState();
        }

        #endregion

        #region Font and style functions

        [Reviewed]
        public static void GuiSetFont(Font font)
        {
            RayguiNative.GuiSetFont(font);
        }

        [Reviewed]
        public static Font GuiGetFont()
        {
            return RayguiNative.GuiGetFont();
        }

        [Reviewed]
        public static void GuiSetStyle(int control, int property, int value)
        {
            RayguiNative.GuiSetStyle(control, property, value);
        }

        [Reviewed]
        public static int GuiGetStyle(int control, int property)
        {
            return RayguiNative.GuiGetStyle(control, property);
        }

        public static void GuiLoadStyle(string? fileName)
        {
            unsafe
            {
                fixed (byte* fileNamePtr = ToUtf8(fileName ?? string.Empty))
                {
                    RayguiNative.GuiLoadStyle((sbyte*)fileNamePtr);
                }
            }
        }

        public static void GuiLoadStyleFromMemory(ReadOnlySpan<byte> fileData)
        {
            unsafe
            {
                fixed (byte* fileDataPtr = fileData)
                {
                    RayguiNative.GuiLoadStyleFromMemory(fileDataPtr, fileData.Length);
                }
            }
        }

        [Reviewed]
        public static void GuiLoadStyleDefault()
        {
            RayguiNative.GuiLoadStyleDefault();
        }

        #endregion

        #region Tooltip functions

        public static void GuiEnableTooltip()
        {
            RayguiNative.GuiEnableTooltip();
        }

        public static void GuiDisableTooltip()
        {
            RayguiNative.GuiDisableTooltip();
        }

        public static void GuiSetTooltip(string? text)
        {
            IntPtr previous = tooltip;
            tooltip = text is null ? IntPtr.Zero : Marshal.StringToCoTaskMemUTF8(text);
            unsafe
            {
                RayguiNative.GuiSetTooltip((sbyte*)tooltip);
            }
            Marshal.FreeCoTaskMem(previous);
        }

        #endregion

        #region Icon functions

        public static string GuiIconText(int iconId, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return FromUtf8(RayguiNative.GuiIconText(iconId, (sbyte*)textPtr));
                }
            }
        }

        public static void GuiSetIconScale(int scale)
        {
            RayguiNative.GuiSetIconScale(scale);
        }

        // GuiGetIcons is intentionally not exposed: it returns a raw pointer into raygui's icon table.

        public static void GuiLoadIcons(string? fileName)
        {
            unsafe
            {
                fixed (byte* fileNamePtr = ToUtf8(fileName ?? string.Empty))
                {
                    RayguiNative.GuiLoadIcons((sbyte*)fileNamePtr, false);
                }
            }
        }

        public static void GuiLoadIconsFromMemory(ReadOnlySpan<byte> fileData)
        {
            unsafe
            {
                fixed (byte* fileDataPtr = fileData)
                {
                    RayguiNative.GuiLoadIconsFromMemory(fileDataPtr, fileData.Length, false);
                }
            }
        }

        [Reviewed]
        public static void GuiDrawIcon(int iconId, int posX, int posY, int pixelSize, Color color)
        {
            RayguiNative.GuiDrawIcon(iconId, posX, posY, pixelSize, color);
        }

        #endregion

        #region Utility functions

        public static int GuiGetTextWidth(string? text)
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

        public static int GuiWindowBox(Rectangle bounds, string? title)
        {
            unsafe
            {
                fixed (byte* titlePtr = ToUtf8(title))
                {
                    return RayguiNative.GuiWindowBox(bounds, (sbyte*)titlePtr);
                }
            }
        }

        public static int GuiGroupBox(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiGroupBox(bounds, (sbyte*)textPtr);
                }
            }
        }

        public static int GuiLine(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiLine(bounds, (sbyte*)textPtr);
                }
            }
        }

        public static int GuiPanel(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiPanel(bounds, (sbyte*)textPtr);
                }
            }
        }

        public static int GuiScrollPanel(Rectangle bounds, string? text, Rectangle content, ref Vector2 scroll, out Rectangle view)
        {
            view = default;
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (Vector2* scrollPtr = &scroll)
                fixed (Rectangle* viewPtr = &view)
                {
                    return RayguiNative.GuiScrollPanel(bounds, (sbyte*)textPtr, content, scrollPtr, viewPtr);
                }
            }
        }

        #endregion

        #region Basic controls

        public static int GuiLabel(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiLabel(bounds, (sbyte*)textPtr);
                }
            }
        }

        public static int GuiButton(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiButton(bounds, (sbyte*)textPtr);
                }
            }
        }

        public static int GuiLabelButton(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiLabelButton(bounds, (sbyte*)textPtr);
                }
            }
        }

        public static int GuiStatusBar(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiStatusBar(bounds, (sbyte*)textPtr);
                }
            }
        }

        public static int GuiDummyRec(Rectangle bounds, string? text)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    return RayguiNative.GuiDummyRec(bounds, (sbyte*)textPtr);
                }
            }
        }

        public static int GuiGrid(Rectangle bounds, string? text, float spacing, int subdivs, out Vector2 mouseCell)
        {
            mouseCell = default;
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (Vector2* mouseCellPtr = &mouseCell)
                {
                    return RayguiNative.GuiGrid(bounds, (sbyte*)textPtr, spacing, subdivs, mouseCellPtr);
                }
            }
        }

        #endregion

        #region Selection controls

        public static int GuiToggle(Rectangle bounds, string? text, ref bool active)
        {
            CBool nativeActive = active;
            int result;
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    result = RayguiNative.GuiToggle(bounds, (sbyte*)textPtr, &nativeActive);
                }
            }
            active = nativeActive;
            return result;
        }

        public static int GuiToggleGroup(Rectangle bounds, string? text, ref int active)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text ?? string.Empty))
                fixed (int* activePtr = &active)
                {
                    return RayguiNative.GuiToggleGroup(bounds, (sbyte*)textPtr, activePtr);
                }
            }
        }

        public static int GuiToggleSlider(Rectangle bounds, string? text, ref int active)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (int* activePtr = &active)
                {
                    return RayguiNative.GuiToggleSlider(bounds, (sbyte*)textPtr, activePtr);
                }
            }
        }

        public static int GuiCheckBox(Rectangle bounds, string? text, ref bool @checked)
        {
            CBool nativeChecked = @checked;
            int result;
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                {
                    result = RayguiNative.GuiCheckBox(bounds, (sbyte*)textPtr, &nativeChecked);
                }
            }
            @checked = nativeChecked;
            return result;
        }

        public static int GuiComboBox(Rectangle bounds, string? text, ref int active)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text ?? string.Empty))
                fixed (int* activePtr = &active)
                {
                    return RayguiNative.GuiComboBox(bounds, (sbyte*)textPtr, activePtr);
                }
            }
        }

        public static int GuiDropdownBox(Rectangle bounds, string? text, ref int active, bool editMode)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text ?? string.Empty))
                fixed (int* activePtr = &active)
                {
                    return RayguiNative.GuiDropdownBox(bounds, (sbyte*)textPtr, activePtr, editMode);
                }
            }
        }

        public static int GuiSpinner(Rectangle bounds, string? text, ref int value, int minValue, int maxValue, bool editMode)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (int* valuePtr = &value)
                {
                    return RayguiNative.GuiSpinner(bounds, (sbyte*)textPtr, valuePtr, minValue, maxValue, editMode);
                }
            }
        }

        public static int GuiValueBox(Rectangle bounds, string? text, ref int value, int minValue, int maxValue, bool editMode)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (int* valuePtr = &value)
                {
                    return RayguiNative.GuiValueBox(bounds, (sbyte*)textPtr, valuePtr, minValue, maxValue, editMode);
                }
            }
        }

        public static int GuiValueBoxFloat(Rectangle bounds, string? text, ref string? textValue, ref float value, bool editMode)
        {
            byte[] textValueBuffer = ToUtf8Buffer(textValue, ValueBoxMaxChars + 1);
            int result;
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (byte* textValuePtr = textValueBuffer)
                fixed (float* valuePtr = &value)
                {
                    result = RayguiNative.GuiValueBoxFloat(bounds, (sbyte*)textPtr, (sbyte*)textValuePtr, valuePtr, editMode);
                }
            }
            textValue = FromUtf8Buffer(textValueBuffer);
            return result;
        }

        #endregion

        #region Text and value controls

        public static int GuiTextBox(Rectangle bounds, ref string? text, int textSize, bool editMode)
        {
            byte[] textBuffer = ToUtf8Buffer(text, textSize);
            int result;
            unsafe
            {
                fixed (byte* textPtr = textBuffer)
                {
                    result = RayguiNative.GuiTextBox(bounds, (sbyte*)textPtr, textSize, editMode);
                }
            }
            text = FromUtf8Buffer(textBuffer);
            return result;
        }

        // GuiTextBoxMulti was removed in raygui 5.0, which has no multiline text editing control.

        public static int GuiSlider(Rectangle bounds, string? textLeft, string? textRight, ref float value, float minValue, float maxValue)
        {
            unsafe
            {
                fixed (byte* textLeftPtr = ToUtf8(textLeft))
                fixed (byte* textRightPtr = ToUtf8(textRight))
                fixed (float* valuePtr = &value)
                {
                    return RayguiNative.GuiSlider(bounds, (sbyte*)textLeftPtr, (sbyte*)textRightPtr, valuePtr, minValue, maxValue);
                }
            }
        }

        public static int GuiSliderBar(Rectangle bounds, string? textLeft, string? textRight, ref float value, float minValue, float maxValue)
        {
            unsafe
            {
                fixed (byte* textLeftPtr = ToUtf8(textLeft))
                fixed (byte* textRightPtr = ToUtf8(textRight))
                fixed (float* valuePtr = &value)
                {
                    return RayguiNative.GuiSliderBar(bounds, (sbyte*)textLeftPtr, (sbyte*)textRightPtr, valuePtr, minValue, maxValue);
                }
            }
        }

        public static int GuiProgressBar(Rectangle bounds, string? textLeft, string? textRight, ref float value, float minValue, float maxValue)
        {
            unsafe
            {
                fixed (byte* textLeftPtr = ToUtf8(textLeft))
                fixed (byte* textRightPtr = ToUtf8(textRight))
                fixed (float* valuePtr = &value)
                {
                    return RayguiNative.GuiProgressBar(bounds, (sbyte*)textLeftPtr, (sbyte*)textRightPtr, valuePtr, minValue, maxValue);
                }
            }
        }

        #endregion

        #region Advanced controls

        public static int GuiListView(Rectangle bounds, string? text, ref int scrollIndex, ref int active)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (int* scrollIndexPtr = &scrollIndex)
                fixed (int* activePtr = &active)
                {
                    return RayguiNative.GuiListView(bounds, (sbyte*)textPtr, scrollIndexPtr, activePtr);
                }
            }
        }

        public static int GuiListViewEx(Rectangle bounds, string[] textArray, ref int scrollIndex, ref int active, ref int focus)
        {
            unsafe
            {
                sbyte** textArrayPtr = AllocUtf8Array(textArray);
                try
                {
                    fixed (int* scrollIndexPtr = &scrollIndex)
                    fixed (int* activePtr = &active)
                    fixed (int* focusPtr = &focus)
                    {
                        return RayguiNative.GuiListViewEx(bounds, textArrayPtr, textArray.Length, scrollIndexPtr, activePtr, focusPtr);
                    }
                }
                finally
                {
                    FreeUtf8Array(textArrayPtr, textArray.Length);
                }
            }
        }

        public static int GuiTabBar(Rectangle bounds, string? text, ref int hscroll, ref int active)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (int* hscrollPtr = &hscroll)
                fixed (int* activePtr = &active)
                {
                    return RayguiNative.GuiTabBar(bounds, (sbyte*)textPtr, hscrollPtr, activePtr);
                }
            }
        }

        public static int GuiTabBarEx(Rectangle bounds, string[] textArray, ref int hscroll, ref int active, ref int focus)
        {
            unsafe
            {
                sbyte** textArrayPtr = AllocUtf8Array(textArray);
                try
                {
                    fixed (int* hscrollPtr = &hscroll)
                    fixed (int* activePtr = &active)
                    fixed (int* focusPtr = &focus)
                    {
                        return RayguiNative.GuiTabBarEx(bounds, textArrayPtr, textArray.Length, hscrollPtr, activePtr, focusPtr);
                    }
                }
                finally
                {
                    FreeUtf8Array(textArrayPtr, textArray.Length);
                }
            }
        }

        public static int GuiMessageBox(Rectangle bounds, string? title, string? message, string? buttons, ref int btnActive)
        {
            unsafe
            {
                fixed (byte* titlePtr = ToUtf8(title))
                fixed (byte* messagePtr = ToUtf8(message))
                fixed (byte* buttonsPtr = ToUtf8(buttons ?? string.Empty))
                fixed (int* btnActivePtr = &btnActive)
                {
                    return RayguiNative.GuiMessageBox(bounds, (sbyte*)titlePtr, (sbyte*)messagePtr, (sbyte*)buttonsPtr, btnActivePtr);
                }
            }
        }

        public static int GuiTextInputBox(Rectangle bounds, string? title, string? message, ref string? text, int textMaxSize, string? buttons, ref int btnActive)
        {
            unsafe
            {
                return TextInputBox(bounds, title, message, ref text, textMaxSize, buttons, ref btnActive, null);
            }
        }

        public static int GuiTextInputBox(Rectangle bounds, string? title, string? message, ref string? text, int textMaxSize, string? buttons, ref int btnActive, ref bool secretViewActive)
        {
            CBool nativeSecretViewActive = secretViewActive;
            int result;
            unsafe
            {
                result = TextInputBox(bounds, title, message, ref text, textMaxSize, buttons, ref btnActive, &nativeSecretViewActive);
            }
            secretViewActive = nativeSecretViewActive;
            return result;
        }

        private static unsafe int TextInputBox(Rectangle bounds, string? title, string? message, ref string? text, int textMaxSize, string? buttons, ref int btnActive, CBool* secretViewActive)
        {
            byte[] textBuffer = ToUtf8Buffer(text, textMaxSize);
            int result;
            fixed (byte* titlePtr = ToUtf8(title))
            fixed (byte* messagePtr = ToUtf8(message))
            fixed (byte* textPtr = textBuffer)
            fixed (byte* buttonsPtr = ToUtf8(buttons ?? string.Empty))
            fixed (int* btnActivePtr = &btnActive)
            {
                result = RayguiNative.GuiTextInputBox(bounds, (sbyte*)titlePtr, (sbyte*)messagePtr, (sbyte*)textPtr, textMaxSize, (sbyte*)buttonsPtr, btnActivePtr, secretViewActive);
            }
            text = FromUtf8Buffer(textBuffer);
            return result;
        }

        #endregion

        #region Color controls

        public static int GuiColorPicker(Rectangle bounds, string? text, ref Color color)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (Color* colorPtr = &color)
                {
                    return RayguiNative.GuiColorPicker(bounds, (sbyte*)textPtr, colorPtr);
                }
            }
        }

        public static int GuiColorPanel(Rectangle bounds, string? text, ref Color color)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (Color* colorPtr = &color)
                {
                    return RayguiNative.GuiColorPanel(bounds, (sbyte*)textPtr, colorPtr);
                }
            }
        }

        public static int GuiColorBarAlpha(Rectangle bounds, string? text, ref float alpha)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (float* alphaPtr = &alpha)
                {
                    return RayguiNative.GuiColorBarAlpha(bounds, (sbyte*)textPtr, alphaPtr);
                }
            }
        }

        public static int GuiColorBarHue(Rectangle bounds, string? text, ref float value)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (float* valuePtr = &value)
                {
                    return RayguiNative.GuiColorBarHue(bounds, (sbyte*)textPtr, valuePtr);
                }
            }
        }

        public static int GuiColorPickerHSV(Rectangle bounds, string? text, ref Vector3 colorHsv)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (Vector3* colorHsvPtr = &colorHsv)
                {
                    return RayguiNative.GuiColorPickerHSV(bounds, (sbyte*)textPtr, colorHsvPtr);
                }
            }
        }

        public static int GuiColorPanelHSV(Rectangle bounds, string? text, ref Vector3 colorHsv)
        {
            unsafe
            {
                fixed (byte* textPtr = ToUtf8(text))
                fixed (Vector3* colorHsvPtr = &colorHsv)
                {
                    return RayguiNative.GuiColorPanelHSV(bounds, (sbyte*)textPtr, colorHsvPtr);
                }
            }
        }

        #endregion
    }
}
