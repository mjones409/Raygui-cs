using Raylib_cs;
using System.Numerics;

namespace RayGui_cs
{
    /// <summary>Result of <see cref="Gui.ScrollPanel"/>.</summary>
    /// <param name="Scroll">The scroll offset to pass back in on the next frame.</param>
    /// <param name="View">The visible area of the content, typically used as a scissor rectangle.</param>
    public readonly record struct ScrollPanelResult(Vector2 Scroll, Rectangle View)
    {
        /// <summary>True when the panel header was clicked.</summary>
        public bool HeaderClicked { get; init; }
    }

    /// <summary>Result of <see cref="Gui.DropdownBox(Rectangle, string, int, bool)"/>.</summary>
    /// <param name="Active">The index of the selected item.</param>
    /// <param name="IsOpen">Whether the item list is open, to pass back in on the next frame.</param>
    public readonly record struct DropdownBoxResult(int Active, bool IsOpen);

    /// <summary>Result of <see cref="Gui.Spinner"/> and <see cref="Gui.ValueBox"/>.</summary>
    /// <param name="Value">The current value.</param>
    /// <param name="IsEditing">Whether the value is being typed in, to pass back in on the next frame.</param>
    public readonly record struct ValueBoxResult(int Value, bool IsEditing);

    /// <summary>Result of <see cref="Gui.FloatValueBox"/>.</summary>
    /// <param name="Value">The current value.</param>
    /// <param name="ValueText">The typed text, to pass back in on the next frame.</param>
    /// <param name="IsEditing">Whether the value is being typed in, to pass back in on the next frame.</param>
    public readonly record struct FloatValueBoxResult(float Value, string ValueText, bool IsEditing);

    /// <summary>Result of <see cref="Gui.TextBox"/>.</summary>
    /// <param name="Text">The current text.</param>
    /// <param name="IsEditing">Whether the text is being typed in, to pass back in on the next frame.</param>
    public readonly record struct TextBoxResult(string Text, bool IsEditing);

    /// <summary>Result of <see cref="Gui.ListView(Rectangle, IReadOnlyList{string}, int, int?)"/>.</summary>
    /// <param name="ScrollIndex">The index of the first visible item, to pass back in on the next frame.</param>
    /// <param name="Active">The index of the selected item, or null for none.</param>
    public readonly record struct ListViewResult(int ScrollIndex, int? Active)
    {
        /// <summary>The index of the item under the mouse, or null for none.</summary>
        public int? Focused { get; init; }
    }

    /// <summary>Result of <see cref="Gui.TabBar(Rectangle, IReadOnlyList{string}, int)"/>.</summary>
    /// <param name="Active">The index of the active tab, to pass back in on the next frame.</param>
    /// <param name="ClosedTab">
    /// The index of the tab the user asked to close, with its close button or a middle click, or null for none.
    /// Removing the tab is up to the caller.
    /// </param>
    public readonly record struct TabBarResult(int Active, int? ClosedTab);

    /// <summary>Result of <see cref="Gui.MessageBox(Rectangle, string?, string?, string)"/>.</summary>
    /// <param name="ClickedButton">The zero based index of the button clicked this frame, or null for none.</param>
    /// <param name="Closed">True when the window close button was clicked this frame.</param>
    public readonly record struct MessageBoxResult(int? ClickedButton, bool Closed)
    {
        /// <summary>True when the user clicked a button or closed the box.</summary>
        public bool IsDismissed => ClickedButton.HasValue || Closed;
    }

    /// <summary>Result of <see cref="Gui.TextInputBox(Rectangle, string?, string?, string?, int, string, bool?)"/>.</summary>
    /// <param name="Text">The current text, to pass back in on the next frame.</param>
    /// <param name="ClickedButton">The zero based index of the button clicked this frame, or null for none.</param>
    /// <param name="Closed">True when the window close button was clicked this frame.</param>
    public readonly record struct TextInputBoxResult(string Text, int? ClickedButton, bool Closed)
    {
        /// <summary>Whether secret text is shown, to pass back in on the next frame; null when the box has no show/hide toggle.</summary>
        public bool? SecretViewActive { get; init; }

        /// <summary>True when the user clicked a button or closed the box.</summary>
        public bool IsDismissed => ClickedButton.HasValue || Closed;
    }

    /// <summary>Result of <see cref="Gui.OpenFileDialog"/>, <see cref="Gui.SaveFileDialog"/> and <see cref="Gui.FolderBrowserDialog"/>.</summary>
    /// <param name="Bounds">The dialog bounds after the user moved or resized it, to pass back in on the next frame.</param>
    /// <param name="Paths">The full paths of the files or folders the user accepted this frame, or null for none.</param>
    /// <param name="Canceled">True when the user canceled the dialog this frame, with Cancel, the close button or Escape.</param>
    public readonly record struct FileDialogResult(Rectangle Bounds, IReadOnlyList<string>? Paths, bool Canceled)
    {
        /// <summary>True when the Help button was clicked this frame.</summary>
        public bool HelpClicked { get; init; }

        /// <summary>True when the user accepted or canceled the dialog.</summary>
        public bool IsDismissed => Paths is not null || Canceled;
    }
}
