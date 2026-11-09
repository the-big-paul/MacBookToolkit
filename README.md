# MacBookToolkit

A tiny, portable **Windows laptop utility** born on a MacBook Pro (Boot Camp).
It bundles a few small quality-of-life tweaks that Windows lacks out of the box:
automatic screen dimming, touchpad blocking while typing, and key-chatter
suppression for the infamous butterfly keyboard. Everything is configurable in
a simple GUI and can be reset to sane defaults at any time.

![icon](app.ico)

## Features

| Feature | What it does |
|---|---|
| **Screen auto-dim** | After N seconds of inactivity the screen brightness drops to a set level; any input restores the normal brightness. |
| **Touchpad guard** | While you type, touchpad input is ignored for a configurable time after each keypress. No more accidental cursor jumps from your palms. |
| **Key debounce** | Suppresses butterfly-keyboard chatter (one key registering twice) without breaking normal typing, auto-repeat or third-party tools like layout switchers. |
| **Screen timeout** | Sets the Windows "turn off display after" timeout directly from the GUI. |
| **Tray + autostart** | Lives in the system tray (optionally hidden), starts with Windows. |
| **3 languages** | Русский · Українська · English — switchable on the fly, tooltips included. |
| **Reset to defaults** | One click restores the tuned defaults below. |

## Defaults (the "tuned" profile)

- Screen: **15 s** idle → **10 %** brightness, restore to **100 %** on activity
- Touchpad: block for **700 ms** after each keypress
- Keyboard: debounce windows **70 ms** after release / **25 ms** between presses
- Power: turn off display after **2 min**

## Requirements

- Windows 7 / 8 / 10 / 11 (x86 or x64)
- .NET Framework 4.x (preinstalled on Windows 10/11)
- A laptop with WMI-controllable brightness for the screen feature
  (most laptops work; desktop PCs simply won't have the dim feature)

## Build

No toolchain required — `csc.exe` ships with the .NET Framework:

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

Or manually:

```bat
csc /nologo /target:winexe /win32icon:app.ico /out:MacBookToolkit.exe ^
    /reference:System.dll /reference:System.Windows.Forms.dll ^
    /reference:System.Drawing.dll /reference:System.Management.dll ^
    MacBookToolkit.cs
```

Output: a single portable `MacBookToolkit.exe` (~20 KB). No installer, no
dependencies — copy it anywhere and run it.

## Usage

1. Run `MacBookToolkit.exe`.
2. Configure the tabs, press **Save & apply**.
3. Close the window to send it to the tray (or disable the tray icon).
4. Settings are stored in `%APPDATA%\MacBookToolkit\settings.ini`.

## How it works (short version)

- **Low-level hooks** (`WH_KEYBOARD_LL`, `WH_MOUSE_LL`) are used for the
  touchpad guard and the key debounce. Injected input (`LLKHF_INJECTED`) and
  service keys (Backspace, Delete, arrows, Enter, Tab, Esc, modifiers) are
  excluded, so fast deletion, auto-repeat and tools like Caramba Switcher keep
  working.
- **Screen brightness** is set through the WMI class
  `root\wmi:WmiMonitorBrightnessMethods`.
- **Idle detection** uses `GetLastInputInfo`.
- **Screen timeout** is applied with `powercfg`.

## License

MIT — free to use, modify, merge, publish, distribute, sublicense and sell.
See [LICENSE](LICENSE).

## Disclaimer

Keyboard and mouse hooks affect system-wide input. The program is provided
"as is", without warranty of any kind. Use at your own risk.

---

## Русский

**MacBookToolkit** — небольшая портативная утилита для ноутбуков Windows,
созданная на MacBook Pro (Boot Camp). Даёт то, чего не хватает в Windows
«из коробки»: автоматическое затемнение экрана, блокировку тачпада во время
печати и подавление дребезга клавиш butterfly-клавиатуры. Всё настраивается в
простом графическом интерфейсе и в один клик сбрасывается к значениям
по умолчанию.

- Языки интерфейса: **русский, украинский, английский**.
- Сборка: `powershell -ExecutionPolicy Bypass -File build.ps1`.
- Требования: Windows 7+ и .NET Framework 4.x.
- Результат: один файл `MacBookToolkit.exe` (~20 КБ), без установки.

Настройки по умолчанию: экран 15 с → 10 %, активность → 100 %; тачпад
700 мс; дебаунс 70/25 мс; выключение экрана через 2 мин.

Лицензия: **MIT** — можно использовать, изменять, распространять и продавать.
