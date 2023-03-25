using Microsoft.Toolkit.HighPerformance.Buffers;
using Raylib_cs;
using Raylib_CsLo.InternalHelpers;
using System.Numerics;
using System.Runtime.InteropServices;

internal static class RaylibBindingsConverter
{

    internal static Raylib_CsLo.Rectangle ConvertRectangle(Rectangle rectangle)
    {
        return new Raylib_CsLo.Rectangle(rectangle.x, rectangle.y, rectangle.width, rectangle.height);
    }
    internal static Rectangle ConvertRectangle(Raylib_CsLo.Rectangle rectangle)
    {
        return new Rectangle(rectangle.x, rectangle.y, rectangle.width, rectangle.height);
    }

    internal static Raylib_CsLo.Font ConvertFont(Font font)
    {
        unsafe
        {
            Raylib_CsLo.Font csLoFont = new Raylib_CsLo.Font
            {
                baseSize = font.baseSize,
                glyphCount = font.glyphCount,
                glyphPadding = font.glyphPadding,
                texture = ConvertTexture(font.texture),
                recs = (Raylib_CsLo.Rectangle*)font.recs,
            };
            csLoFont.glyphs = ConvertGlyphInfo(font.glyphs, csLoFont.glyphs);
            return csLoFont;
        }
    }
    internal static Font ConvertFont(Raylib_CsLo.Font font)
    {
        unsafe
        {
            Font csLoFont = new Font
            {
                baseSize = font.baseSize,
                glyphCount = font.glyphCount,
                glyphPadding = font.glyphPadding,
                texture = ConvertTexture(font.texture),
                recs = (Rectangle*)font.recs,
            };
            csLoFont.glyphs = ConvertGlyphInfo(font.glyphs, csLoFont.glyphs);
            return csLoFont;
        }
    }
    internal static Raylib_CsLo.Texture ConvertTexture(Texture2D texture)
    {
        return new Raylib_CsLo.Texture
        {
            id = texture.id,
            width = texture.width,
            height = texture.height,
            mipmaps = texture.mipmaps,
            format = (int)texture.format
        };

    }
    internal static Texture2D ConvertTexture(Raylib_CsLo.Texture texture)
    {
        return new Texture2D
        {
            id = texture.id,
            width = texture.width,
            height = texture.height,
            mipmaps = texture.mipmaps,
            format = (PixelFormat)texture.format
        };

    }
    private unsafe static Raylib_CsLo.GlyphInfo* ConvertGlyphInfo(GlyphInfo* convertFrom, Raylib_CsLo.GlyphInfo* convertTo)
    {
        convertTo->value = convertFrom->value;
        convertTo->offsetX = convertFrom->offsetX;
        convertTo->offsetY = convertFrom->offsetY;
        convertTo->advanceX = convertFrom->advanceX;
        convertTo->image = ConvertImage(convertFrom->image);
        return convertTo;
    }
    private unsafe static GlyphInfo* ConvertGlyphInfo(Raylib_CsLo.GlyphInfo* convertFrom, GlyphInfo* convertTo)
    {
        convertTo->value = convertFrom->value;
        convertTo->offsetX = convertFrom->offsetX;
        convertTo->offsetY = convertFrom->offsetY;
        convertTo->advanceX = convertFrom->advanceX;
        convertTo->image = ConvertImage(convertFrom->image);
        return convertTo;
    }
    internal static Raylib_CsLo.Image ConvertImage(Image image)
    {
        unsafe
        {
            return new Raylib_CsLo.Image
            {
                data = image.data,
                width = image.width,
                height = image.height,
                mipmaps = image.mipmaps,
                format = (int)image.format
            };
        }
    }
    internal static Image ConvertImage(Raylib_CsLo.Image image)
    {
        unsafe
        {
            return new Image
            {
                data = image.data,
                width = image.width,
                height = image.height,
                mipmaps = image.mipmaps,
                format = (PixelFormat)image.format
            };
        }
    }
    internal unsafe static sbyte* ConvertString(string? text)
    {
        SpanOwner<sbyte> spanOwner = text.MarshalUtf8();
        try
        {
            return spanOwner.AsPtr();
        }
        finally
        {
            spanOwner.Dispose();
        }
    }

    internal unsafe static string ConvertSbyte(sbyte* sbytes, int maxLength)
    {
        if (sbytes is null)
        {
            return string.Empty;
        }

        int length = 0;

        // Find the length of the string, up to maxLength
        while (length < maxLength && sbytes[length] != 0)
        {
            length++;
        }

        // Create a byte array and copy the sbytes into it
        byte[] bytes = new byte[length];
        Marshal.Copy((IntPtr)sbytes, bytes, 0, length);

        // Convert the byte array to a string
        string str = System.Text.Encoding.UTF8.GetString(bytes);

        return str;
    }

    internal unsafe static Vector2* ConvertVector2(Vector2 vector)
    {
        return &vector;
    }

    internal static Raylib_CsLo.Color ConvertColor(Color color)
    {
        return new Raylib_CsLo.Color
        {
            a = color.a,
            r = color.r,
            g = color.g,
            b = color.b,
        };
    }
    internal static Color ConvertColor(Raylib_CsLo.Color color)
    {
        return new Color
        {
            a = color.a,
            r = color.r,
            g = color.g,
            b = color.b,
        };
    }

}

