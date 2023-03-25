using Microsoft.Toolkit.HighPerformance.Buffers;
using Raylib_cs;
using Raylib_CsLo.InternalHelpers;
using System.Numerics;
using System.Text;
using static RaylibBindingsConverter;

namespace RayGui_cs
{
    public static class Gui
    {
        [Reviewed]
        public static void GuiEnable()
        {
            Raylib_CsLo.RayGui.GuiEnable();
        }

        [Reviewed]
        public static void GuiDisable()
        {
            Raylib_CsLo.RayGui.GuiDisable();
        }

        [Reviewed]
        public static void GuiLock()
        {
            Raylib_CsLo.RayGui.GuiLock();
        }

        [Reviewed]
        public static void GuiUnlock()
        {
            Raylib_CsLo.RayGui.GuiUnlock();
        }

        [Reviewed]
        public static bool GuiIsLocked()
        {
            return Raylib_CsLo.RayGui.GuiIsLocked();
        }


        [Reviewed]
        public static void GuiFade(float alpha)
        {
            Raylib_CsLo.RayGui.GuiFade(alpha);
        }

        [Reviewed]
        public static void GuiSetState(int state)
        {
            Raylib_CsLo.RayGui.GuiSetState(state);
        }

        [Reviewed]
        public static int GuiGetState()
        {
            return Raylib_CsLo.RayGui.GuiGetState();
        }

        [Reviewed]
        public static void GuiSetFont(Font font)
        {
            Raylib_CsLo.RayGui.GuiSetFont(ConvertFont(font));
        }

        [Reviewed]
        public static Font GuiGetFont()
        {
            return ConvertFont(Raylib_CsLo.RayGui.GuiGetFont());
        }

        [Reviewed]
        public static void GuiSetStyle(int control, int property, int value)

        {
            Raylib_CsLo.RayGui.GuiSetStyle(control, property, value);
        }

        [Reviewed]
        public static int GuiGetStyle(int control, int property)
        {
            return Raylib_CsLo.RayGui.GuiGetStyle(control, property);
        }

        [Reviewed]
        public static void GuiPanel(Rectangle bounds, string? text)
        {
            unsafe
            {
                Raylib_CsLo.RayGui.GuiPanel(ConvertRectangle(bounds), ConvertString(text));
            }
        }

        [Reviewed]
        public static Rectangle GuiScrollPanel(Rectangle bounds, string? text, Rectangle content, ref Vector2 scroll)
        {
            unsafe
            {
                fixed (Vector2* fixedScroll = &scroll)
                {
                    return ConvertRectangle(Raylib_CsLo.RayGui.GuiScrollPanel(ConvertRectangle(bounds), ConvertString(text), ConvertRectangle(content), fixedScroll));
                }

            }
        }

        [Reviewed]
        public static Vector2 GuiGrid(Rectangle bounds, string? text, float spacing, int subdivs)
        {
            unsafe
            {
                return Raylib_CsLo.RayGui.GuiGrid(ConvertRectangle(bounds), ConvertString(text), spacing, subdivs);
            }
        }

        [Reviewed]
        public static Color GuiColorPicker(Rectangle bounds, string? text, Color color)
        {
            unsafe
            {
                return ConvertColor(Raylib_CsLo.RayGui.GuiColorPicker(ConvertRectangle(bounds), ConvertString(text), ConvertColor(color)));
            }
        }

        [Reviewed]
        public static Color GuiColorPanel(Rectangle bounds, string? text, Color color)
        {
            unsafe
            {
                return ConvertColor(Raylib_CsLo.RayGui.GuiColorPanel(ConvertRectangle(bounds), ConvertString(text), ConvertColor(color)));
            }
        }

        [Reviewed]
        public static float GuiColorBarAlpha(Rectangle bounds, string? text, float alpha)
        {
            unsafe
            {
                return Raylib_CsLo.RayGui.GuiColorBarAlpha(ConvertRectangle(bounds), ConvertString(text), alpha);
            }
        }

        [Reviewed]
        public static float GuiColorBarHue(Rectangle bounds, string? text, float value)
        {
            unsafe
            {
                return Raylib_CsLo.RayGui.GuiColorBarHue(ConvertRectangle(bounds), ConvertString(text), value);
            }
        }

        [Reviewed]
        public static void GuiLoadStyleDefault()
        {
            Raylib_CsLo.RayGui.GuiLoadStyleDefault();
        }

        [Reviewed]
        public static void GuiDrawIcon(int iconId, int posX, int posY, int pixelSize, Color color)
        {
            Raylib_CsLo.RayGui.GuiDrawIcon(iconId, posX, posY, pixelSize, ConvertColor(color));
        }
        /*
       [FailedReview("Returns a memory address.")]
       public static uint GuiGetIcons()
       {
           throw new NotImplementedException();
           unsafe
           {
               return (uint)Raylib_CsLo.RayGui.GuiGetIcons();
           }
       }

       [FailedReview("Returns a memory address.")]
       public static uint GuiGetIconData(int iconId)
       {
           throw new NotImplementedException();
           unsafe
           {
               return (uint)Raylib_CsLo.RayGui.GuiGetIconData(iconId);
           }
       }

       [FailedReview("Requires a memory address.")]
       public static void GuiSetIconData(int iconId, uint data)
       {
           throw new NotImplementedException();
           unsafe
           {
               Raylib_CsLo.RayGui.GuiSetIconData(iconId, (uint*)data); 
           }
       } 
        */
        [Reviewed]
        public static void GuiSetIconScale(uint scale)
        {
            Raylib_CsLo.RayGui.GuiSetIconScale(scale);
        }

        [Reviewed]
        public static void GuiSetIconPixel(int iconId, int x, int y)
        {
            Raylib_CsLo.RayGui.GuiSetIconPixel(iconId, x, y);
        }

        [Reviewed]
        public static void GuiClearIconPixel(int iconId, int x, int y)
        {
            Raylib_CsLo.RayGui.GuiClearIconPixel(iconId, x, y);
        }

        [Reviewed]
        public static bool GuiCheckIconPixel(int iconId, int x, int y)

        {
            return Raylib_CsLo.RayGui.GuiCheckIconPixel(iconId, x, y);
        }

        [Reviewed]
        public static bool GuiWindowBox(Rectangle bounds, string? title)
        {
            return Raylib_CsLo.RayGui.GuiWindowBox(ConvertRectangle(bounds), title);
        }

        [Reviewed]
        public static void GuiGroupBox(Rectangle bounds, string? text)
        {
            Raylib_CsLo.RayGui.GuiGroupBox(ConvertRectangle(bounds), text);
        }

        [Reviewed]
        public static void GuiLine(Rectangle bounds, string? text)
        {
            Raylib_CsLo.RayGui.GuiLine(ConvertRectangle(bounds), text);
        }

        [Reviewed]
        public static void GuiLabel(Rectangle bounds, string? text)
        {
            Raylib_CsLo.RayGui.GuiLabel(ConvertRectangle(bounds), text);
        }

        [Reviewed]
        public static bool GuiButton(Rectangle bounds, string? text)
        {
            return Raylib_CsLo.RayGui.GuiButton(ConvertRectangle(bounds), text);

        }

        [Reviewed]
        public static bool GuiLabelButton(Rectangle bounds, string? text)
        {
            return Raylib_CsLo.RayGui.GuiLabelButton(ConvertRectangle(bounds), text);
        }

        [Reviewed]
        public static bool GuiToggle(Rectangle bounds, string? text, bool active)
        {
            return Raylib_CsLo.RayGui.GuiToggle(ConvertRectangle(bounds), text, active);

        }

        [Reviewed]
        public static int GuiToggleGroup(Rectangle bounds, string? text, int active)
        {
            text ??= string.Empty;
            return Raylib_CsLo.RayGui.GuiToggleGroup(ConvertRectangle(bounds), text, active);
        }

        [Reviewed]
        public static bool GuiCheckBox(Rectangle bounds, string? text, bool @checked)
        {
            return Raylib_CsLo.RayGui.GuiCheckBox(ConvertRectangle(bounds), text, @checked);
        }

        [Reviewed]
        public static int GuiComboBox(Rectangle bounds, string? text, int active)
        {
            text ??= string.Empty;
            return Raylib_CsLo.RayGui.GuiComboBox(ConvertRectangle(bounds), text, active);
        }

        [Reviewed]
        public static bool GuiDropdownBox(Rectangle bounds, string? text, ref int active, bool editMode)
        {
            text ??= string.Empty;
            unsafe
            {
                fixed (int* activePtr = &active)
                {
                    return Raylib_CsLo.RayGui.GuiDropdownBox(ConvertRectangle(bounds), text, activePtr, editMode);
                }
            }
        }

        [Reviewed]
        public static bool GuiSpinner(Rectangle bounds, string? text, ref int value, int minValue, int maxValue, bool editMode)
        {
            unsafe
            {
                fixed (int* valuePtr = &value)
                {
                    return Raylib_CsLo.RayGui.GuiSpinner(ConvertRectangle(bounds), text, valuePtr, minValue, maxValue, editMode);
                }
            }
        }

        [Reviewed]
        public static bool GuiValueBox(Rectangle bounds, string? text, ref int value, int minValue, int maxValue, bool editMode)
        {
            unsafe
            {
                fixed (int* valuePtr = &value)
                {
                    return Raylib_CsLo.RayGui.GuiValueBox(ConvertRectangle(bounds), text, valuePtr, minValue, maxValue, editMode);
                }
            }
        }

        [Reviewed]
        public static bool GuiTextBox(Rectangle bounds, ref string? text, int textSize, bool editMode)
        {
            unsafe
            {
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(text ?? "");
                if (utf8Bytes.Length == 0)
                {
                    utf8Bytes = new byte[] { 0 }; // create a byte array with a single null terminator
                }
                var signed = Array.ConvertAll(utf8Bytes, x => unchecked((sbyte)x));
                fixed (sbyte* ptr = signed)
                {
                    bool returnValue = Raylib_CsLo.RayGui.GuiTextBox(ConvertRectangle(bounds), ptr, textSize, editMode);
                    text = ConvertSbyte(ptr, textSize);

                    return returnValue;
                }
            }
        }

        [Reviewed]
        public static bool GuiTextBoxMulti(Rectangle bounds, ref string? text, int textSize, bool editMode)
        {
            unsafe
            {
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(text ?? "");
                if (utf8Bytes.Length == 0)
                {
                    utf8Bytes = new byte[] { 0 }; // create a byte array with a single null terminator
                }
                var signed = Array.ConvertAll(utf8Bytes, x => unchecked((sbyte)x));
                fixed (sbyte* ptr = signed)
                {
                    bool returnValue = Raylib_CsLo.RayGui.GuiTextBoxMulti(ConvertRectangle(bounds), ptr, textSize, editMode);
                    text = ConvertSbyte(ptr, textSize);

                    return returnValue;
                }
            }
        }

        [Reviewed]
        public static float GuiSlider(Rectangle bounds, string? textLeft, string? textRight, float value, float minValue, float maxValue)
        {
            return Raylib_CsLo.RayGui.GuiSlider(ConvertRectangle(bounds), textLeft, textRight, value, minValue, maxValue);

        }

        [Reviewed]
        public static float GuiSliderBar(Rectangle bounds, string? leftText, string? rightText, float value, float minValue, float maxValue)
        {
            return Raylib_CsLo.RayGui.GuiSliderBar(ConvertRectangle(bounds), leftText, rightText, value, minValue, maxValue);
        }

        [Reviewed]
        public static float GuiProgressBar(Rectangle bounds, string? textLeft, string? textRight, float value, float minValue, float maxValue)
        {
            return Raylib_CsLo.RayGui.GuiProgressBar(ConvertRectangle(bounds), textLeft, textRight, value, minValue, maxValue);

        }

        [Reviewed]
        public static void GuiStatusBar(Rectangle bounds, string? text)
        {
            Raylib_CsLo.RayGui.GuiStatusBar(ConvertRectangle(bounds), text);
        }

        [Reviewed]
        public static void GuiDummyRec(Rectangle bounds, string? text)
        {
            Raylib_CsLo.RayGui.GuiDummyRec(ConvertRectangle(bounds), text);
        }

        [Reviewed]
        public static int GuiListView(Rectangle bounds, string? text, ref int scrollIndex, int active)
        {
            unsafe
            {
                fixed (int* fixedScrollIndex = &scrollIndex)
                {
                    return Raylib_CsLo.RayGui.GuiListView(ConvertRectangle(bounds), text, fixedScrollIndex, active);
                }
            }
        }

        [Reviewed]
        public static int GuiListViewEx(Rectangle bounds, string[] textArray, int count, ref int focus, ref int scrollIndex, int active)
        {
            unsafe
            {
                fixed (int* fixedFocus = &focus)
                {
                    fixed (int* fixedScrollIndex = &scrollIndex)
                    {
                        return Raylib_CsLo.RayGui.GuiListViewEx(ConvertRectangle(bounds), textArray, count, fixedFocus, fixedScrollIndex, active);
                    }
                }
            }
        }

        [Reviewed]
        public static int GuiMessageBox(Rectangle bounds, string? title, string? message, string? buttons)
        {
            title ??= string.Empty;
            message ??= string.Empty;
            buttons ??= string.Empty;
            unsafe
            {
                SpanOwner<sbyte> spanOwner = title.MarshalUtf8();
                try
                {
                    SpanOwner<sbyte> spanOwner2 = message.MarshalUtf8();
                    try
                    {
                        SpanOwner<sbyte> spanOwner3 = buttons.MarshalUtf8();
                        try
                        {
                            return Raylib_CsLo.RayGui.GuiMessageBox(ConvertRectangle(bounds), spanOwner.AsPtr(), spanOwner2.AsPtr(), spanOwner3.AsPtr());
                        }
                        finally
                        {
                            spanOwner3.Dispose();
                        }
                    }
                    finally
                    {
                        spanOwner2.Dispose();
                    }
                }
                finally
                {
                    spanOwner.Dispose();
                }
            }
        }

        [Reviewed]
        public static int GuiTextInputBox(Rectangle bounds, string? title, string? message, string? buttons, ref string? text, int textMaxSize = 255)
        {
            title ??= string.Empty;
            message ??= string.Empty;
            buttons ??= string.Empty;
            text ??= string.Empty;
            unsafe
            {
                SpanOwner<sbyte> spanOwner = title.MarshalUtf8();
                try
                {
                    SpanOwner<sbyte> spanOwner2 = message.MarshalUtf8();
                    try
                    {
                        SpanOwner<sbyte> spanOwner3 = buttons.MarshalUtf8();
                        try
                        {
                            SpanOwner<sbyte> spanOwner4 = text.MarshalUtf8();
                            try
                            {
                                var textBytes = spanOwner4.AsPtr();
                                int buttonClicked = Raylib_CsLo.RayGui.GuiTextInputBox(ConvertRectangle(bounds), spanOwner.AsPtr(), spanOwner2.AsPtr(), spanOwner3.AsPtr(), textBytes, textMaxSize, null);
                                text = ConvertSbyte(textBytes, textMaxSize);
                                return buttonClicked;
                            }
                            finally
                            {
                                spanOwner4.Dispose();
                            }
                        }
                        finally
                        {
                            spanOwner3.Dispose();
                        }
                    }
                    finally
                    {
                        spanOwner2.Dispose();
                    }
                }
                finally
                {
                    spanOwner.Dispose();
                }
            }
        }

        [Reviewed]
        public static void GuiLoadStyle(string? fileName)
        {
            fileName ??= string.Empty;
            Raylib_CsLo.RayGui.GuiLoadStyle(fileName);
        }

        [Reviewed]
        public static string GuiIconText(int iconId, string? text)
        {
            return Raylib_CsLo.RayGui.GuiIconText(iconId, text);
        }
    }
}