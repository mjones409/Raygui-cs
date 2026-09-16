# Raygui-cs

Lets you use [raygui](https://github.com/raysan5/raygui) with [Raylib-cs](https://github.com/raylib-cs/raylib-cs). Raygui-cs binds directly to a native raygui shared library, and its API uses the Raylib-cs types (`Rectangle`, `Color`, `Font`, ...). Code that calls it doesn't need `unsafe`.

| Dependency | Version |
|------------|---------|
| Raylib-cs (NuGet) | 8.1.0 (raylib 6.0) |
| raygui (native) | 5.0 |
| .NET | 10.0 |

## Native raygui library

raygui only ships as a C header, and Raylib-cs's native raylib library doesn't include it. You need to provide a raygui shared library next to your application:

| Platform | File name |
|----------|-----------|
| Windows | `raygui.dll` |
| Linux | `libraygui.so` |
| macOS | `libraygui.dylib` |

The library **must link dynamically to raylib 6.0**, so it uses the same raylib instance (window, input, and GL context) that Raylib-cs loads. If raylib is linked statically into raygui, the result is a second raylib instance that can't see your window.

Start by creating `raygui.c` next to raygui 5.0's `src/raygui.h`:

```c
#define RAYGUI_IMPLEMENTATION
#include "raygui.h"
```

Then build it against the matching [raylib 6.0 release](https://github.com/raysan5/raylib/releases/tag/6.0) package.

**Windows (x64 Native Tools Command Prompt, `raylib-6.0_win64_msvc16`):**

```bat
cl /LD /O2 /D"RAYGUIAPI=__declspec(dllexport)" /I raylib-6.0_win64_msvc16\include raygui.c raylib-6.0_win64_msvc16\lib\raylibdll.lib /Fe:raygui.dll
```

**Linux (`raylib-6.0_linux_amd64`):**

```sh
gcc -shared -fPIC -O2 -Iraylib-6.0_linux_amd64/include raygui.c -Lraylib-6.0_linux_amd64/lib -lraylib -Wl,-rpath,'$ORIGIN' -o libraygui.so
```

**macOS (`raylib-6.0_macos`):**

```sh
clang -dynamiclib -O2 -Iraylib-6.0_macos/include raygui.c -Lraylib-6.0_macos/lib -lraylib -Wl,-rpath,@loader_path -o libraygui.dylib
```

## Usage

```csharp
using Raylib_cs;
using RayGui_cs;

Raylib.InitWindow(800, 450, "Raygui-cs");

float volume = 0.5f;
int quality = 0;
bool qualityOpen = false;
bool showMessage = false;

while (!Raylib.WindowShouldClose())
{
    Raylib.BeginDrawing();
    Raylib.ClearBackground(GuiStyle.GetColor(GuiDefaultProperty.BackgroundColor));

    if (Gui.Button(new Rectangle(24, 24, 160, 30), Gui.IconText(GuiIconName.Info, "Show message")))
    {
        showMessage = true;
    }

    volume = Gui.Slider(new Rectangle(80, 70, 200, 20), "Volume", null, volume, 0, 1);

    (quality, qualityOpen) = Gui.DropdownBox(new Rectangle(24, 110, 160, 30), ["Low", "Medium", "High"], quality, qualityOpen);

    if (showMessage && Gui.MessageBox(new Rectangle(250, 150, 300, 120), "Hello", "raygui from C#", "OK;Cancel").IsDismissed)
    {
        showMessage = false;
    }

    Raylib.EndDrawing();
}

Raylib.CloseWindow();
```

## API conventions

raygui is an immediate mode library: you call each control every frame, pass in its current state, and keep the state it gives back.

- **Controls are methods on `Gui`**, without raygui's `Gui` prefix: `GuiButton` is `Gui.Button`.
- **Single values go in and come back out.** `Toggle`, `CheckBox`, `ComboBox`, the sliders and the color controls take the current value and return the new one.
- **Multiple values come back as a result struct** that can be deconstructed, for example `(scroll, view) = Gui.ScrollPanel(...)` or `(text, isEditing) = Gui.TextBox(...)`. Controls with an edit mode return the next edit state, so you never toggle it yourself.
- **Clicks are `bool`.** `Button`, `LabelButton`, `WindowBox` (close button), `Panel` (header), `StatusBar` and `DummyRec` return `true` when clicked. Display only controls return nothing.
- **"None" is `null`**, never `-1`: `Grid` returns `Vector2?`, `ListView` takes and returns `int? active`, and dialogs report `ClickedButton` as a zero based `int?`.
- **Lists can be collections.** Controls that take `;` separated text also accept a collection of strings.
- **Global state is properties:** `Gui.IsLocked`, `Gui.Alpha`, `Gui.State`, `Gui.Font`, `Gui.TooltipsEnabled`, `Gui.Tooltip` and `Gui.IconScale`.
- **Styles use enums**, through `GuiStyle`: `GuiStyle.Set(GuiControl.Button, GuiControlProperty.BorderWidth, 2)`, `GuiStyle.SetColor(GuiDefaultProperty.BackgroundColor, Color.Black)` or `GuiStyle.Set(GuiDropdownBoxProperty.DropdownRollUp, true)`.
- **Text length limits are in UTF-8 bytes** and don't include the null terminator (`maxByteCount`).
- **Invalid arguments throw** instead of reaching raygui, which doesn't check its inputs. For example, an out of range style property or icon throws `ArgumentOutOfRangeException`, and a missing style file throws `FileNotFoundException`.

## File and folder dialogs

raygui has no file dialog, so Raygui-cs includes one written in C# on top of raygui controls. `OpenFileDialog`, `SaveFileDialog` and `FolderBrowserDialog` follow the WinForms classes of the same names. They have the same properties (`Filter`, `FilterIndex`, `InitialDirectory`, `FileName(s)`, `Multiselect`, `DefaultExt`, `AddExtension`, `CheckFileExists`, `OverwritePrompt`, `CreatePrompt`, `ShowReadOnly`, `ShowHiddenFiles`, `ShowPinnedPlaces`, `CustomPlaces`, `ClientGuid`, `OkRequiresInteraction`, `SelectedPath(s)`, `Description`, `ShowNewFolderButton`, ...) and events (`FileOk`, `HelpRequest`). They draw in the current raygui style and work on Windows, Linux and macOS.

The dialog window can be moved and resized. It has back, forward, up and refresh buttons, an editable address bar, search, a hidden files toggle and a new folder button. The file list can be sorted by column. The places list shows known folders, custom places and drives. The dialogs also have filter and read-only controls, confirmation prompts and keyboard support: arrows, Enter, Backspace, Alt+arrows, Ctrl+A, Ctrl+L, Ctrl+F, Ctrl+H, F5, Ctrl+Shift+N, type-ahead and Escape.

**Blocking**, like WinForms. `ShowDialog` runs its own frame loop until the dialog closes, so it can be called from a button handler:

```csharp
var dialog = new OpenFileDialog { Filter = "Images (*.png, *.jpg)|*.png;*.jpg|All files (*.*)|*.*", Multiselect = true };
if (Gui.Button(bounds, "Open...") && dialog.ShowDialog(DrawMyUi) == DialogResult.OK)
{
    Load(dialog.FileNames);
}
```

`DrawMyUi` is optional. It redraws your UI behind the dialog each frame, with the gui locked.

**Non-blocking.** Open the dialog, then call `Draw` every frame after the rest of your UI:

```csharp
var folderDialog = new FolderBrowserDialog { Description = "Select the source folder" };
Raylib.SetExitKey(KeyboardKey.Null); // otherwise Escape also closes the window

while (!Raylib.WindowShouldClose())
{
    Raylib.BeginDrawing();
    Gui.IsLocked = folderDialog.IsBlockingInput;
    if (Gui.Button(new Rectangle(10, 10, 120, 30), "Browse..."))
    {
        folderDialog.Open();
    }
    Gui.IsLocked = false;

    if (folderDialog.Draw() == DialogResult.OK)
    {
        source = folderDialog.SelectedPath;
    }
    Raylib.EndDrawing();
}
```

Differences from WinForms: `.lnk` shortcuts aren't resolved (symbolic links are), `RestoreDirectory` only restores a current directory changed by your own `FileOk` handler, `ClientGuid` remembers the last folder only for the lifetime of the process, and `AddToRecent` and `AutoUpgradeEnabled` don't exist.

## Migrating from earlier versions

Earlier versions of Raygui-cs wrapped raygui 3.x through Raylib-CsLo, using raygui's C function names.

| Earlier | Now |
|---------|-----|
| `Gui.GuiButton(...)` and other controls | `Gui.Button(...)`, and so on |
| `GuiLock`, `GuiUnlock`, `GuiIsLocked` | `Gui.IsLocked` |
| `GuiFade(alpha)` | `Gui.Alpha = alpha` |
| `GuiSetState`, `GuiGetState` | `Gui.State` (`GuiState`) |
| `GuiSetFont`, `GuiGetFont` | `Gui.Font` |
| `GuiSetIconScale` | `Gui.IconScale` |
| `GuiSetStyle`, `GuiGetStyle` | `GuiStyle.Set`, `GuiStyle.Get`, `GuiStyle.SetColor`, `GuiStyle.GetColor` |
| `GuiLoadStyle`, `GuiLoadStyleDefault` | `GuiStyle.Load`, `GuiStyle.LoadDefault` |
| `GuiIconText(int, text)`, `GuiDrawIcon(int, ...)` | `Gui.IconText(GuiIconName, text)`, `Gui.DrawIcon(GuiIconName, ...)` |
| `GuiScrollPanel(..., ref scroll)` returning the view | `(scroll, view) = Gui.ScrollPanel(..., scroll)` |
| `GuiDropdownBox(..., ref active, editMode)` returning a toggle flag | `(active, isOpen) = Gui.DropdownBox(..., active, isOpen)` |
| `GuiSpinner`, `GuiValueBox` with `ref value` | `(value, isEditing) = Gui.Spinner(...)` / `Gui.ValueBox(...)` |
| `GuiTextBox(bounds, ref text, textSize, editMode)` | `(text, isEditing) = Gui.TextBox(bounds, text, maxByteCount, isEditing)` |
| `GuiListView(..., ref scrollIndex, active)`, `GuiListViewEx` | `(scrollIndex, active) = Gui.ListView(..., scrollIndex, active)` |
| `GuiMessageBox` returning -1, 0 or a 1 based button | `Gui.MessageBox` returning `ClickedButton` and `Closed` |
| `GuiTextInputBox(bounds, title, message, buttons, ref text, textMaxSize)` | `Gui.TextInputBox(bounds, title, message, text, maxByteCount, buttons)` |

`GuiTextBoxMulti`, `GuiSetIconPixel`, `GuiClearIconPixel` and `GuiCheckIconPixel` were removed from raygui 5.0 and have no replacement. New in this version: `ToggleSlider`, `FloatValueBox`, `TabBar`, `ColorPickerHsv`, `ColorPanelHsv`, tooltips, `GetTextWidth`, `GuiStyle.Load(ReadOnlySpan<byte>)` and `LoadIcons`.
