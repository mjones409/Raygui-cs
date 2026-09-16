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

## Differences from raygui 3.x

Earlier versions of Raygui-cs wrapped raygui 3.x through Raylib-CsLo. raygui 5.0 changed its API, and the wrapper now follows it:

- Controls return an `int` result code (`0` none, `1` pressed, `2` changed) instead of `bool` or the new value.
- Control state is passed by `ref`, for example `GuiCheckBox(bounds, text, ref isChecked)` and `GuiSlider(bounds, left, right, ref value, min, max)`.
- `GuiScrollPanel` and `GuiGrid` return their view and mouse cell through `out` parameters.
- `GuiMessageBox` and `GuiTextInputBox` write the clicked button to `ref int btnActive`. `GuiTextInputBox` now takes its parameters in raygui 5.0's order.
- `GuiFade` has been replaced by `GuiSetAlpha`. `GuiTextBoxMulti`, `GuiSetIconPixel`, `GuiClearIconPixel` and `GuiCheckIconPixel` no longer exist in raygui.
- New in raygui 5.0: `GuiToggleSlider`, `GuiValueBoxFloat`, `GuiTabBar`, `GuiTabBarEx`, `GuiColorPickerHSV`, `GuiColorPanelHSV`, tooltips, `GuiGetTextWidth`, `GuiLoadStyleFromMemory`, `GuiLoadIcons` and `GuiLoadIconsFromMemory`.
