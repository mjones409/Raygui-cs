using System.Runtime.InteropServices;
using System.Text;

namespace RayGui_cs
{
    internal static class Utf8Marshal
    {
        // Returns a null-terminated UTF-8 copy of text, or null so raygui receives NULL.
        internal static byte[]? ToUtf8(string? text)
        {
            if (text is null)
            {
                return null;
            }

            byte[] buffer = new byte[Encoding.UTF8.GetByteCount(text) + 1];
            Encoding.UTF8.GetBytes(text, 0, text.Length, buffer, 0);
            return buffer;
        }

        // Returns a mutable null-terminated UTF-8 buffer that raygui can edit in place, never smaller than bufferSize.
        internal static byte[] ToUtf8Buffer(string? text, int bufferSize)
        {
            text ??= string.Empty;
            byte[] buffer = new byte[Math.Max(bufferSize, Encoding.UTF8.GetByteCount(text) + 1)];
            Encoding.UTF8.GetBytes(text, 0, text.Length, buffer, 0);
            return buffer;
        }

        internal static string FromUtf8Buffer(byte[] buffer)
        {
            int length = Array.IndexOf(buffer, (byte)0);
            return Encoding.UTF8.GetString(buffer, 0, length < 0 ? buffer.Length : length);
        }

        internal static unsafe string FromUtf8(sbyte* text)
        {
            return Marshal.PtrToStringUTF8((IntPtr)text) ?? string.Empty;
        }

        // Allocates a native array of null-terminated UTF-8 strings; release it with FreeUtf8Array.
        internal static unsafe sbyte** AllocUtf8Array(string[] items)
        {
            sbyte** array = (sbyte**)NativeMemory.AllocZeroed((nuint)items.Length, (nuint)sizeof(sbyte*));
            for (int i = 0; i < items.Length; i++)
            {
                array[i] = (sbyte*)Marshal.StringToCoTaskMemUTF8(items[i] ?? string.Empty);
            }

            return array;
        }

        internal static unsafe void FreeUtf8Array(sbyte** array, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Marshal.FreeCoTaskMem((IntPtr)array[i]);
            }

            NativeMemory.Free(array);
        }
    }
}
