using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

public class Settings
{
    public bool ScreenEnabled = true;
    public int IdleSec = 15;
    public int DimPct = 10;
    public int NormalPct = 100;
    public bool TouchEnabled = true;
    public int BlockMs = 700;
    public bool KeyEnabled = true;
    public int DebounceUpMs = 70;
    public int DebounceDownMs = 25;
    public int ScreenOffMin = 2;
    public bool AutoStart = true;
    public bool ShowTray = true;
    public bool TopMost = false;
    public string Lang = "ru";
    public string Theme = "system";

    public const int MaxDebounceDownMs = 30;

    public bool Existed = false;
    public bool ParseFailed = false;

    public static string Dir()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MacBookToolkit");
    }

    public static string FilePath()
    {
        return Path.Combine(Dir(), "settings.ini");
    }

    public static Settings Load()
    {
        Settings s = new Settings();
        string fp = FilePath();
        if (!File.Exists(fp)) return s;
        try
        {
            s = FromLines(File.ReadAllLines(fp));
        }
        catch (Exception ex)
        {
            s = new Settings();
            s.ParseFailed = true;
            Program.LogError("settings load", ex);
        }
        s.Existed = true;
        return s;
    }

    public static Settings FromLines(string[] lines)
    {
        Settings s = new Settings();
        if (lines == null) return s;
        foreach (string line in lines)
        {
            string[] p = line.Split('=');
            if (p.Length != 2) continue;
            string k = p[0].Trim();
            string v = p[1].Trim();
            bool b; int i;
            switch (k)
            {
                case "ScreenEnabled": if (bool.TryParse(v, out b)) s.ScreenEnabled = b; break;
                case "IdleSec": if (int.TryParse(v, out i)) s.IdleSec = i; break;
                case "DimPct": if (int.TryParse(v, out i)) s.DimPct = i; break;
                case "NormalPct": if (int.TryParse(v, out i)) s.NormalPct = i; break;
                case "TouchEnabled": if (bool.TryParse(v, out b)) s.TouchEnabled = b; break;
                case "BlockMs": if (int.TryParse(v, out i)) s.BlockMs = i; break;
                case "KeyEnabled": if (bool.TryParse(v, out b)) s.KeyEnabled = b; break;
                case "DebounceUpMs": if (int.TryParse(v, out i)) s.DebounceUpMs = i; break;
                case "DebounceDownMs": if (int.TryParse(v, out i)) s.DebounceDownMs = i; break;
                case "ScreenOffMin": if (int.TryParse(v, out i)) s.ScreenOffMin = i; break;
                case "AutoStart": if (bool.TryParse(v, out b)) s.AutoStart = b; break;
                case "ShowTray": if (bool.TryParse(v, out b)) s.ShowTray = b; break;
                case "TopMost": if (bool.TryParse(v, out b)) s.TopMost = b; break;
                case "Lang": s.Lang = v; break;
                case "Theme": s.Theme = v; break;
            }
        }
        s.Normalize();
        return s;
    }

    static int ClampInt(int v, int min, int max)
    {
        return v < min ? min : (v > max ? max : v);
    }

    public void Normalize()
    {
        IdleSec = ClampInt(IdleSec, 1, 300);
        DimPct = ClampInt(DimPct, 1, 100);
        NormalPct = ClampInt(NormalPct, 1, 100);
        BlockMs = ClampInt(BlockMs, 50, 2000);
        DebounceUpMs = ClampInt(DebounceUpMs, 0, 200);
        DebounceDownMs = ClampInt(DebounceDownMs, 0, MaxDebounceDownMs);
        ScreenOffMin = ClampInt(ScreenOffMin, 1, 60);
        if (DimPct > NormalPct) DimPct = NormalPct;
        if (Lang != "ru" && Lang != "uk" && Lang != "en") Lang = "ru";
        if (Theme != "system" && Theme != "light" && Theme != "dark") Theme = "system";
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Dir());
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ScreenEnabled=" + ScreenEnabled.ToString());
            sb.AppendLine("IdleSec=" + IdleSec.ToString());
            sb.AppendLine("DimPct=" + DimPct.ToString());
            sb.AppendLine("NormalPct=" + NormalPct.ToString());
            sb.AppendLine("TouchEnabled=" + TouchEnabled.ToString());
            sb.AppendLine("BlockMs=" + BlockMs.ToString());
            sb.AppendLine("KeyEnabled=" + KeyEnabled.ToString());
            sb.AppendLine("DebounceUpMs=" + DebounceUpMs.ToString());
            sb.AppendLine("DebounceDownMs=" + DebounceDownMs.ToString());
            sb.AppendLine("ScreenOffMin=" + ScreenOffMin.ToString());
            sb.AppendLine("AutoStart=" + AutoStart.ToString());
            sb.AppendLine("ShowTray=" + ShowTray.ToString());
            sb.AppendLine("TopMost=" + TopMost.ToString());
            sb.AppendLine("Lang=" + Lang.ToString());
            sb.AppendLine("Theme=" + Theme.ToString());
            string fp = FilePath();
            string tmp = fp + ".tmp";
            File.WriteAllText(tmp, sb.ToString());
            if (File.Exists(fp)) File.Replace(tmp, fp, null);
            else File.Move(tmp, fp);
        }
        catch (Exception ex) { Program.LogError("settings save", ex); }
    }
}

public class KeyDebouncer
{
    readonly bool[] _hasUp = new bool[256];
    readonly bool[] _hasDown = new bool[256];
    readonly uint[] _lastUp = new uint[256];
    readonly uint[] _lastDown = new uint[256];

    public void NoteKeyUp(int vk, uint now)
    {
        int key = vk & 0xFF;
        _lastUp[key] = now;
        _hasUp[key] = true;
    }

    public bool ShouldDropKeyDown(int vk, uint now, int upMs, int downMs)
    {
        int key = vk & 0xFF;
        uint dtUp = _hasUp[key] ? unchecked(now - _lastUp[key]) : uint.MaxValue;
        uint dtDown = _hasDown[key] ? unchecked(now - _lastDown[key]) : uint.MaxValue;
        if (dtUp < (uint)upMs || dtDown < (uint)downMs) return true;
        _lastDown[key] = now;
        _hasDown[key] = true;
        return false;
    }
}

public static class L10n
{
    public static string Lang = "ru";

    static Dictionary<string, string> ru = new Dictionary<string, string>()
    {
        {"sec_screen","Экран"}, {"sec_touch","Тачпад"}, {"sec_key","Клавиатура"}, {"sec_power","Питание"},
        {"sec_window","Окно и система"},
        {"chk_screen","Затемнять при простое"},
        {"lbl_idle","Простой, с"}, {"lbl_dim","Тускло, %"}, {"lbl_normal","Ярко, %"},
        {"chk_touch","Блокировать при печати"}, {"lbl_block","Блок, мс"},
        {"chk_key","Убирать дребезг клавиш"}, {"lbl_up","Отпуск, мс"}, {"lbl_down","Между, мс"},
        {"lbl_screenoff","Экран, мин"},
        {"chk_topmost","Поверх всех окон"},
        {"lbl_theme","Тема"}, {"theme_system","Как в Windows"}, {"theme_light","Светлая"}, {"theme_dark","Тёмная"},
        {"chk_autostart","Автозапуск"}, {"chk_tray","Иконка в трее"}, {"lbl_lang","Язык"},
        {"btn_save","Сохранить"}, {"btn_reset","Сброс"},
        {"reset_title","Сброс"},
        {"reset_confirm","Вернуть все настройки (кроме языка и темы) к подобранным значениям?\n\nЭкран: 15 с / 10 % / 100 %\nТачпад: 700 мс\nКлавиатура: 70 / 25 мс\nПитание: экран через 2 мин"},
        {"reset_done","Готово — настройки сброшены к умолчанию."},
        {"tray_show","Открыть"}, {"tray_topmost","Поверх всех окон"}, {"tray_autostart","Автозагрузка"}, {"tray_exit","Выход"},
        {"tray_min","Свёрнуто в трей. Настройки работают."},
        {"already_running","MacBookToolkit уже запущен — окно восстановлено."},
        {"hung_title","Приложение не отвечает"},
        {"hung_body","Основной экземпляр MacBookToolkit не отвечает.\n\nЗавершить его и запустить заново?"},
        {"tip_chk_screen","Когда вы не трогаете мышь/клавиатуру заданное время, яркость экрана снижается. При активности возвращается обычная яркость."},
        {"tip_idle","Через сколько секунд бездействия включать затемнение. Минимум 1 сек."},
        {"tip_dim","До какой яркости (в %) затемнять экран. 10 % — очень тускло, но читаемо."},
        {"tip_normal","Яркость при активности. 100 % — максимум."},
        {"tip_chk_touch","Пока вы печатаете, тачпад не двигает курсор. Защищает от случайных касаний ладонью."},
        {"tip_block","Сколько миллисекунд после последнего нажатия клавиши тачпад игнорирует касания и клики."},
        {"tip_chk_key","Убирает «дребезг» butterfly-клавиатуры — когда одна клавиша срабатывает дважды."},
        {"tip_up","Повторное нажатие той же клавиши быстрее этого времени считается дребезгом и отбрасывается."},
        {"tip_down","Окно между двумя нажатиями одной клавиши без отпускания. Не ставьте больше 30 мс — иначе перестанет работать авто-повтор."},
        {"tip_screenoff","Через сколько минут простоя Windows выключает экран."},
        {"tip_topmost","Окно программы всегда будет поверх остальных окон."},
        {"tip_theme","Оформление: следовать теме Windows, светлая или тёмная. Акцентный цвет берётся из системы."},
        {"tip_autostart","Запускать программу автоматически при входе в Windows."},
        {"tip_tray","Показывать иконку программы в системном трее. Клик — открыть, правый клик — меню."},
        {"tip_lang","Язык интерфейса."},
        {"tip_save","Сохранить настройки и применить их сразу."},
        {"tip_reset","Вернуть все настройки к подобранным значениям по умолчанию."}
    };

    static Dictionary<string, string> uk = new Dictionary<string, string>()
    {
        {"sec_screen","Екран"}, {"sec_touch","Тачпад"}, {"sec_key","Клавіатура"}, {"sec_power","Живлення"},
        {"sec_window","Вікно і система"},
        {"chk_screen","Затемнювати при простої"},
        {"lbl_idle","Простій, с"}, {"lbl_dim","Тьмяно, %"}, {"lbl_normal","Яскраво, %"},
        {"chk_touch","Блокувати під час друку"}, {"lbl_block","Блок, мс"},
        {"chk_key","Прибирати брязкіт клавіш"}, {"lbl_up","Відпуск, мс"}, {"lbl_down","Між, мс"},
        {"lbl_screenoff","Екран, хв"},
        {"chk_topmost","Поверх усіх вікон"},
        {"lbl_theme","Тема"}, {"theme_system","Як у Windows"}, {"theme_light","Світла"}, {"theme_dark","Темна"},
        {"chk_autostart","Автозапуск"}, {"chk_tray","Іконка в треї"}, {"lbl_lang","Мова"},
        {"btn_save","Зберегти"}, {"btn_reset","Скинути"},
        {"reset_title","Скидання"},
        {"reset_confirm","Повернути всі налаштування (крім мови та теми) до підібраних значень?\n\nЕкран: 15 с / 10 % / 100 %\nТачпад: 700 мс\nКлавіатура: 70 / 25 мс\nЖивлення: екран через 2 хв"},
        {"reset_done","Готово — налаштування скинуті до стандартних."},
        {"tray_show","Відкрити"}, {"tray_topmost","Поверх усіх вікон"}, {"tray_autostart","Автозапуск"}, {"tray_exit","Вихід"},
        {"tray_min","Згорнуто в трей. Налаштування працюють."},
        {"already_running","MacBookToolkit вже запущено — вікно відновлено."},
        {"hung_title","Програма не відповідає"},
        {"hung_body","Основний екземпляр MacBookToolkit не відповідає.\n\nЗавершити його та запустити заново?"},
        {"tip_chk_screen","Коли ви не торкаєтесь миші/клавіатури заданий час, яскравість екрана знижується. При активності повертається звичайна."},
        {"tip_idle","Через скільки секунд простою вмикати затемнення. Мінімум 1 сек."},
        {"tip_dim","До якої яскравості (у %) затемнювати екран. 10 % — дуже тьмяно, але читабельно."},
        {"tip_normal","Яскравість при активності. 100 % — максимум."},
        {"tip_chk_touch","Поки ви друкуєте, тачпад не рухає курсор. Захищає від випадкових торкань долонею."},
        {"tip_block","Скільки мілісекунд після останнього натискання клавіші тачпад ігнорує торкання та кліки."},
        {"tip_chk_key","Прибирає «брязкіт» butterfly-клавіатури — коли одна клавіша спрацьовує двічі."},
        {"tip_up","Повторне натискання тієї ж клавіші швидше цього часу вважається брязкотом і відкидається."},
        {"tip_down","Вікно між двома натисканнями однієї клавіші без відпускання. Не ставте більше 30 мс — інакше перестане працювати автоповтор."},
        {"tip_screenoff","Через скільки хвилин простою Windows вимикає екран."},
        {"tip_topmost","Вікно програми завжди буде поверх інших вікон."},
        {"tip_theme","Оформлення: слідувати темі Windows, світла або темна. Акцентний колір береться з системи."},
        {"tip_autostart","Запускати програму автоматично при вході у Windows."},
        {"tip_tray","Показувати іконку програми в системному треї. Клік — відкрити, правий клік — меню."},
        {"tip_lang","Мова інтерфейсу."},
        {"tip_save","Зберегти налаштування та застосувати їх одразу."},
        {"tip_reset","Повернути всі налаштування до підібраних значень за замовчуванням."}
    };

    static Dictionary<string, string> en = new Dictionary<string, string>()
    {
        {"sec_screen","Screen"}, {"sec_touch","Touchpad"}, {"sec_key","Keyboard"}, {"sec_power","Power"},
        {"sec_window","Window & system"},
        {"chk_screen","Dim when idle"},
        {"lbl_idle","Idle, s"}, {"lbl_dim","Dim, %"}, {"lbl_normal","Bright, %"},
        {"chk_touch","Block while typing"}, {"lbl_block","Block, ms"},
        {"chk_key","Suppress key chatter"}, {"lbl_up","Release, ms"}, {"lbl_down","Between, ms"},
        {"lbl_screenoff","Screen, min"},
        {"chk_topmost","Always on top"},
        {"lbl_theme","Theme"}, {"theme_system","Follow Windows"}, {"theme_light","Light"}, {"theme_dark","Dark"},
        {"chk_autostart","Autostart"}, {"chk_tray","Tray icon"}, {"lbl_lang","Lang"},
        {"btn_save","Save"}, {"btn_reset","Reset"},
        {"reset_title","Reset to defaults"},
        {"reset_confirm","Restore all settings (language and theme are kept) to the tuned values?\n\nScreen: 15 s / 10 % / 100 %\nTouchpad: 700 ms\nKeyboard: 70 / 25 ms\nPower: screen off in 2 min"},
        {"reset_done","Done — settings restored to defaults."},
        {"tray_show","Open"}, {"tray_topmost","Always on top"}, {"tray_autostart","Autostart"}, {"tray_exit","Exit"},
        {"tray_min","Minimized to tray. Settings are running."},
        {"already_running","MacBookToolkit is already running — window restored."},
        {"hung_title","Application is not responding"},
        {"hung_body","The main MacBookToolkit instance is not responding.\n\nTerminate it and start again?"},
        {"tip_chk_screen","When you do not touch the mouse/keyboard for the set time, screen brightness is reduced. It restores on activity."},
        {"tip_idle","Seconds of inactivity before dimming starts. Minimum 1 s."},
        {"tip_dim","Brightness (%) to dim to. 10 % is very dim but readable."},
        {"tip_normal","Brightness (%) when active. 100 % is maximum."},
        {"tip_chk_touch","While typing, the touchpad will not move the cursor. Protects from accidental palm touches."},
        {"tip_block","Milliseconds after the last keypress during which the touchpad ignores touches and clicks."},
        {"tip_chk_key","Removes butterfly-keyboard chatter (a key firing twice)."},
        {"tip_up","A repeat press of the same key within this time is treated as chatter and dropped."},
        {"tip_down","Window between two presses of one key without release. Do not exceed 30 ms — otherwise auto-repeat stops."},
        {"tip_screenoff","Minutes of inactivity before Windows turns the screen off."},
        {"tip_topmost","Keep the program window above all other windows."},
        {"tip_theme","Appearance: follow Windows, light or dark. The accent colour is taken from the system."},
        {"tip_autostart","Start the app automatically at Windows login."},
        {"tip_tray","Show the app icon in the system tray. Click to open, right-click for the menu."},
        {"tip_lang","Interface language."},
        {"tip_save","Save settings and apply them immediately."},
        {"tip_reset","Restore all settings to the tuned defaults."}
    };

    public static string T(string key)
    {
        Dictionary<string, string> d = ru;
        if (Lang == "en") d = en;
        else if (Lang == "uk") d = uk;
        string v;
        if (d.TryGetValue(key, out v)) return v;
        return key;
    }
}

public class MainForm : Form
{
    delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll")]
    static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll")]
    static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("user32.dll")]
    static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);

    [StructLayout(LayoutKind.Sequential)]
    struct LASTINPUTINFO { public uint cbSize; public uint dwTime; }
    [StructLayout(LayoutKind.Sequential)]
    struct KBDLLHOOKSTRUCT { public int vkCode; public int scanCode; public int flags; public int time; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)]
    struct MSLLHOOKSTRUCT { public int ptX; public int ptY; public uint mouseData; public uint flags; public uint time; public IntPtr dwExtraInfo; }

    const int WH_KEYBOARD_LL = 13;
    const int WH_MOUSE_LL = 14;
    const int WM_KEYDOWN = 0x0100;
    const int WM_SYSKEYDOWN = 0x0104;
    const int WM_KEYUP = 0x0101;
    const int WM_SYSKEYUP = 0x0105;
    const int LLKHF_INJECTED = 0x10;
    const uint LLMHF_INJECTED = 0x01;
    const int WM_MOUSEMOVE = 0x0200;
    const int WM_LBUTTONDOWN = 0x0201;
    const int WM_RBUTTONDOWN = 0x0204;
    const int WM_MBUTTONDOWN = 0x0207;
    const int WM_XBUTTONDOWN = 0x020B;

    static LowLevelProc _kbProc;
    static LowLevelProc _msProc;
    static IntPtr _kbHook = IntPtr.Zero;
    static IntPtr _msHook = IntPtr.Zero;
    static uint _lastKeyTick = 0;
    static bool _haveKeyTick = false;
    static readonly KeyDebouncer _debounce = new KeyDebouncer();
    static Settings _s;
    static bool _dimmed = false;
    static bool _brightnessErrorLogged = false;
    static bool _systemApplied = false;

    ThemeColors _theme;
    Dictionary<Control, string> _textKeys = new Dictionary<Control, string>();
    Dictionary<Control, string> _tipKeys = new Dictionary<Control, string>();
    ToolTip tip;

    ThemedCard cardScreen, cardKey, cardTouch, cardPower, cardWindow;
    ThemedCaption caption;
    ThemedLabel lblIdle, lblDim, lblNormal;
    ThemedLabel lblBlock, lblUp, lblDown, lblScreenOff, lblTheme, lblLang;
    ThemedCheckBox chkScreen, chkTouch, chkKey, chkTopMost, chkAutoStart, chkTray;
    ThemedSpinner numIdle, numDim, numNormal, numBlock, numUp, numDown, numScreenOff;
    ThemedSelect cbLang, cbTheme;
    ThemedButton btnSave, btnReset;
    NotifyIcon tray;
    MenuItem miShow, miTopMost, miAutoStart, miExit;
    System.Windows.Forms.Timer idleTimer;
    bool _loading = false;
    bool _brightnessOk = false;
    int _brightnessTries = 0;

    public MainForm()
    {
        _s = Settings.Load();
        L10n.Lang = _s.Lang;
        AutoScaleMode = AutoScaleMode.None;
        Font = new Font("Segoe UI", 9f);
        BuildUi();
        ApplyLanguage();
        ApplyThemeNow();
        ApplyTopMost(_s.TopMost, false);
        InstallHooks();
        idleTimer = new System.Windows.Forms.Timer();
        idleTimer.Interval = 500;
        idleTimer.Tick += IdleTick;
        idleTimer.Start();
    }

    // ---------- UI building ----------
    void Add(Control parent, Control child)
    {
        parent.Controls.Add(child);
    }

    ThemedCard Card(string key, double x, double y, double w, double h)
    {
        ThemedCard c = new ThemedCard();
        c.TitleKey = key;
        c.Title = L10n.T(key);
        c.SetBounds(Ui.Px(x), Ui.Px(y), Ui.Px(w), Ui.Px(h));
        _textKeys[c] = key;
        Controls.Add(c);
        return c;
    }

    ThemedLabel Lbl(string key, double x, double y, double w, double h, Control parent, bool muted, bool bold)
    {
        ThemedLabel l = new ThemedLabel();
        l.Muted = muted;
        l.Bold = bold;
        l.Text = L10n.T(key);
        l.SetBounds(Ui.Px(x), Ui.Px(y), Ui.Px(w), Ui.Px(h));
        l.TextAlign = ContentAlignment.MiddleLeft;
        _textKeys[l] = key;
        Add(parent, l);
        return l;
    }

    ThemedCheckBox Chk(string key, double x, double y, double w, Control parent, bool val)
    {
        ThemedCheckBox c = new ThemedCheckBox();
        c.Text = L10n.T(key);
        c.Checked = val;
        c.SetBounds(Ui.Px(x), Ui.Px(y), Ui.Px(w), Ui.Px(22));
        _textKeys[c] = key;
        Add(parent, c);
        return c;
    }

    ThemedSpinner Spn(double x, double y, double w, int min, int max, int step, int val, Control parent)
    {
        ThemedSpinner s = new ThemedSpinner();
        s.Minimum = min;
        s.Maximum = max;
        s.Increment = step;
        s.Value = Math.Max(min, Math.Min(max, val));
        s.SetBounds(Ui.Px(x), Ui.Px(y), Ui.Px(w), Ui.Px(26));
        Add(parent, s);
        return s;
    }

    ThemedSelect Sel(double x, double y, double w, string[] items, Control parent)
    {
        ThemedSelect s = new ThemedSelect();
        s.SetBounds(Ui.Px(x), Ui.Px(y), Ui.Px(w), Ui.Px(26));
        s.Items = items;
        Add(parent, s);
        return s;
    }

    ThemedButton Btn(string key, double x, double y, double w, double h)
    {
        ThemedButton b = new ThemedButton();
        b.Text = L10n.T(key);
        b.SetBounds(Ui.Px(x), Ui.Px(y), Ui.Px(w), Ui.Px(h));
        _textKeys[b] = key;
        Controls.Add(b);
        return b;
    }

    void Tip(Control c, string key)
    {
        _tipKeys[c] = key;
        if (tip != null) tip.SetToolTip(c, L10n.T(key));
    }

    void BuildUi()
    {
        Text = "MacBookToolkit";
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(Ui.Px(456), Ui.Px(472));
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        tip = new ToolTip();
        tip.AutoPopDelay = 15000;
        tip.InitialDelay = 350;
        tip.ReshowDelay = 80;

        caption = new ThemedCaption();
        caption.Title = "MacBookToolkit";
        caption.VersionText = "v" + (string.IsNullOrEmpty(Application.ProductVersion) ? "1.2" : Application.ProductVersion);
        caption.SetBounds(0, 0, Ui.Px(456), Ui.Px(36));
        caption.CloseClick += delegate { Close(); };
        caption.MinimizeClick += delegate { WindowState = FormWindowState.Minimized; };
        Controls.Add(caption);

        cardScreen = Card("sec_screen", 14, 48, 212, 146);
        chkScreen = Chk("chk_screen", 12, 26, 190, cardScreen, _s.ScreenEnabled);
        lblIdle = Lbl("lbl_idle", 12, 58, 112, 22, cardScreen, false, false);
        numIdle = Spn(130, 54, 66, 1, 300, 1, _s.IdleSec, cardScreen);
        lblDim = Lbl("lbl_dim", 12, 88, 112, 22, cardScreen, false, false);
        numDim = Spn(130, 84, 66, 1, 100, 1, _s.DimPct, cardScreen);
        lblNormal = Lbl("lbl_normal", 12, 118, 112, 22, cardScreen, false, false);
        numNormal = Spn(130, 114, 66, 1, 100, 1, _s.NormalPct, cardScreen);

        cardKey = Card("sec_key", 240, 48, 202, 146);
        chkKey = Chk("chk_key", 12, 26, 180, cardKey, _s.KeyEnabled);
        lblUp = Lbl("lbl_up", 12, 58, 86, 22, cardKey, false, false);
        numUp = Spn(104, 54, 66, 0, 200, 5, _s.DebounceUpMs, cardKey);
        lblDown = Lbl("lbl_down", 12, 88, 86, 22, cardKey, false, false);
        numDown = Spn(104, 84, 66, 0, Settings.MaxDebounceDownMs, 5, _s.DebounceDownMs, cardKey);

        cardTouch = Card("sec_touch", 14, 204, 212, 82);
        chkTouch = Chk("chk_touch", 12, 26, 190, cardTouch, _s.TouchEnabled);
        lblBlock = Lbl("lbl_block", 12, 54, 112, 22, cardTouch, false, false);
        numBlock = Spn(130, 50, 66, 50, 2000, 50, _s.BlockMs, cardTouch);

        cardPower = Card("sec_power", 240, 204, 202, 82);
        lblScreenOff = Lbl("lbl_screenoff", 12, 26, 86, 22, cardPower, false, false);
        numScreenOff = Spn(104, 22, 66, 1, 60, 1, _s.ScreenOffMin, cardPower);

        cardWindow = Card("sec_window", 14, 296, 428, 118);
        chkTopMost = Chk("chk_topmost", 12, 26, 200, cardWindow, _s.TopMost);
        lblTheme = Lbl("lbl_theme", 12, 58, 52, 22, cardWindow, false, false);
        cbTheme = Sel(68, 54, 150, ThemeItems(), cardWindow);
        chkAutoStart = Chk("chk_autostart", 238, 26, 180, cardWindow, _s.AutoStart);
        chkTray = Chk("chk_tray", 238, 52, 180, cardWindow, _s.ShowTray);
        lblLang = Lbl("lbl_lang", 238, 84, 52, 22, cardWindow, false, false);
        cbLang = Sel(292, 80, 124, new string[] { "Русский", "Українська", "English" }, cardWindow);

        btnSave = Btn("btn_save", 14, 424, 286, 34);
        btnReset = Btn("btn_reset", 310, 424, 132, 34);
        btnReset.Secondary = true;

        chkTopMost.CheckedChanged += delegate { if (!_loading) ApplyTopMost(chkTopMost.Checked, true); };
        cbLang.SelectedIndexChanged += LangChanged;
        cbTheme.SelectedIndexChanged += ThemeChanged;
        btnSave.Click += SaveClick;
        btnReset.Click += ResetClick;

        _loading = true;
        cbLang.SelectedIndex = _s.Lang == "en" ? 2 : (_s.Lang == "uk" ? 1 : 0);
        cbTheme.SelectedIndex = _s.Theme == "light" ? 1 : (_s.Theme == "dark" ? 2 : 0);
        _loading = false;

        Tip(chkScreen, "tip_chk_screen"); Tip(numIdle, "tip_idle"); Tip(numDim, "tip_dim"); Tip(numNormal, "tip_normal");
        Tip(chkTouch, "tip_chk_touch"); Tip(numBlock, "tip_block");
        Tip(chkKey, "tip_chk_key"); Tip(numUp, "tip_up"); Tip(numDown, "tip_down");
        Tip(numScreenOff, "tip_screenoff");
        Tip(chkTopMost, "tip_topmost"); Tip(cbTheme, "tip_theme");
        Tip(chkAutoStart, "tip_autostart"); Tip(chkTray, "tip_tray"); Tip(cbLang, "tip_lang");
        Tip(btnSave, "tip_save"); Tip(btnReset, "tip_reset");

        BuildTray();
    }

    string[] ThemeItems()
    {
        return new string[] { L10n.T("theme_system"), L10n.T("theme_light"), L10n.T("theme_dark") };
    }

    void BuildTray()
    {
        tray = new NotifyIcon();
        tray.Icon = this.Icon != null ? this.Icon : SystemIcons.Application;
        tray.Text = "MacBookToolkit";
        ContextMenu cm = new ContextMenu();
        miShow = new MenuItem(L10n.T("tray_show"));
        miShow.Click += delegate { ShowFromTray(); };
        miTopMost = new MenuItem(L10n.T("tray_topmost"));
        miTopMost.Click += delegate { ApplyTopMost(!TopMost, true); };
        miAutoStart = new MenuItem(L10n.T("tray_autostart"));
        miAutoStart.Click += delegate
        {
            _s.AutoStart = !_s.AutoStart;
            _s.Save();
            SetAutoStart(_s.AutoStart);
            _loading = true; chkAutoStart.Checked = _s.AutoStart; _loading = false;
            SyncTrayMenu();
        };
        miExit = new MenuItem(L10n.T("tray_exit"));
        miExit.Click += ExitClick;
        cm.MenuItems.Add(miShow);
        cm.MenuItems.Add(miTopMost);
        cm.MenuItems.Add(miAutoStart);
        cm.MenuItems.Add(new MenuItem("-"));
        cm.MenuItems.Add(miExit);
        tray.ContextMenu = cm;
        tray.MouseClick += delegate(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) ShowFromTray();
        };
        tray.DoubleClick += delegate { ShowFromTray(); };
        tray.Visible = _s.ShowTray;
        SyncTrayMenu();
    }

    void SyncTrayMenu()
    {
        if (miTopMost != null) miTopMost.Checked = TopMost;
        if (miAutoStart != null) miAutoStart.Checked = _s.AutoStart;
    }

    void ShowFromTray()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
        BringToFront();
    }

    // ---------- language / theme ----------
    void ApplyLanguage()
    {
        _loading = true;
        L10n.Lang = _s.Lang;
        Text = "MacBookToolkit";
        foreach (KeyValuePair<Control, string> kv in _textKeys)
        {
            ThemedCard card = kv.Key as ThemedCard;
            if (card != null) card.Title = L10n.T(kv.Value);
            else kv.Key.Text = L10n.T(kv.Value);
        }
        if (caption != null)
        {
            caption.VersionText = "v" + (string.IsNullOrEmpty(Application.ProductVersion) ? "1.2" : Application.ProductVersion);
            caption.Invalidate();
        }
        if (cbTheme != null)
        {
            int idx = cbTheme.SelectedIndex;
            cbTheme.Items = ThemeItems();
            cbTheme.SelectedIndex = idx < 0 ? 0 : idx;
        }
        if (miShow != null) miShow.Text = L10n.T("tray_show");
        if (miTopMost != null) miTopMost.Text = L10n.T("tray_topmost");
        if (miAutoStart != null) miAutoStart.Text = L10n.T("tray_autostart");
        if (miExit != null) miExit.Text = L10n.T("tray_exit");
        if (tip != null)
        {
            foreach (KeyValuePair<Control, string> kv in _tipKeys) tip.SetToolTip(kv.Key, L10n.T(kv.Value));
        }
        _loading = false;
        Invalidate(true);
    }

    void ThemeChanged(object sender, EventArgs e)
    {
        if (_loading) return;
        _s.Theme = cbTheme.SelectedIndex == 1 ? "light" : (cbTheme.SelectedIndex == 2 ? "dark" : "system");
        _s.Save();
        ApplyThemeNow();
    }

    void ApplyThemeNow()
    {
        _theme = ThemeManager.Resolve(_s.Theme);
        ThemeManager.ApplyTheme(this, _theme);
        ApplyTitleBarTheme();
        Invalidate(true);
    }

    void ApplyTitleBarTheme()
    {
        if (!IsHandleCreated) return;
        try
        {
            int dark = (_theme != null && _theme.Dark) ? 1 : 0;
            DwmSetWindowAttribute(Handle, 20, ref dark, 4);
            DwmSetWindowAttribute(Handle, 19, ref dark, 4);
            int round = 2;
            DwmSetWindowAttribute(Handle, 33, ref round, 4);
            SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0004 | 0x0010 | 0x0020);
        }
        catch { }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyTitleBarTheme();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ClassStyle |= 0x20000;
            return cp;
        }
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg == 0x001A || m.Msg == 0x031A || m.Msg == 0x0320)
        {
            if (_s != null && _s.Theme == "system" && IsHandleCreated)
            {
                try { BeginInvoke(new Action(ApplyThemeNow)); } catch { }
            }
        }
    }

    // ---------- settings actions ----------
    void ApplyTopMost(bool on, bool persist)
    {
        TopMost = on;
        _s.TopMost = on;
        if (chkTopMost != null) { _loading = true; chkTopMost.Checked = on; _loading = false; }
        SyncTrayMenu();
        if (persist) _s.Save();
    }

    void LangChanged(object sender, EventArgs e)
    {
        if (_loading) return;
        _s.Lang = cbLang.SelectedIndex == 2 ? "en" : (cbLang.SelectedIndex == 1 ? "uk" : "ru");
        _s.Save();
        ApplyLanguage();
        ApplyThemeNow();
    }

    void SaveClick(object sender, EventArgs e)
    {
        _s.ScreenEnabled = chkScreen.Checked;
        _s.IdleSec = numIdle.Value;
        _s.DimPct = numDim.Value;
        _s.NormalPct = numNormal.Value;
        _s.TouchEnabled = chkTouch.Checked;
        _s.BlockMs = numBlock.Value;
        _s.KeyEnabled = chkKey.Checked;
        _s.DebounceUpMs = numUp.Value;
        _s.DebounceDownMs = numDown.Value;
        _s.ScreenOffMin = numScreenOff.Value;
        _s.AutoStart = chkAutoStart.Checked;
        _s.ShowTray = chkTray.Checked;
        _s.TopMost = chkTopMost.Checked;
        if (_s.DimPct > _s.NormalPct) { _s.DimPct = _s.NormalPct; numDim.Value = _s.DimPct; }
        _s.Save();
        SetAutoStart(_s.AutoStart);
        tray.Visible = _s.ShowTray;
        ApplyTopMost(_s.TopMost, false);
        ApplyThemeNow();
        _brightnessOk = SetBrightness(_s.NormalPct);
        _dimmed = !_brightnessOk;
        ThreadPool.QueueUserWorkItem(delegate(object state) { ApplyPower(); });
        Program.LogNote("save", "settings saved and applied");
    }

    void ResetClick(object sender, EventArgs e)
    {
        if (MessageBox.Show(L10n.T("reset_confirm"), L10n.T("reset_title"),
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        Settings d = new Settings();
        d.Lang = _s.Lang;
        d.Theme = _s.Theme;
        _s = d;
        _loading = true;
        chkScreen.Checked = d.ScreenEnabled;
        numIdle.Value = d.IdleSec;
        numDim.Value = d.DimPct;
        numNormal.Value = d.NormalPct;
        chkTouch.Checked = d.TouchEnabled;
        numBlock.Value = d.BlockMs;
        chkKey.Checked = d.KeyEnabled;
        numUp.Value = d.DebounceUpMs;
        numDown.Value = d.DebounceDownMs;
        numScreenOff.Value = d.ScreenOffMin;
        chkAutoStart.Checked = d.AutoStart;
        chkTray.Checked = d.ShowTray;
        _loading = false;
        _s.Save();
        SetAutoStart(_s.AutoStart);
        tray.Visible = _s.ShowTray;
        ApplyTopMost(_s.TopMost, false);
        ApplyThemeNow();
        _brightnessOk = SetBrightness(d.NormalPct);
        _dimmed = !_brightnessOk;
        ThreadPool.QueueUserWorkItem(delegate(object state) { ApplyPower(); });
        SyncTrayMenu();
        MessageBox.Show(L10n.T("reset_done"), "MacBookToolkit");
    }

    // ---------- lifecycle ----------
    void ExitClick(object sender, EventArgs e)
    {
        tray.Visible = false;
        idleTimer.Stop();
        RestoreBrightnessIfDimmed();
        RemoveHooks();
        Application.Exit();
    }

    void FormResize(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized && _s.ShowTray)
        {
            Hide();
            tray.ShowBalloonTip(1500, "MacBookToolkit", L10n.T("tray_min"), ToolTipIcon.Info);
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (_systemApplied || _s == null) return;
        _systemApplied = true;
        Calibrate();
        ApplyTitleBarTheme();
        Resize += FormResize;
    }

    void Calibrate()
    {
        _dimmed = false;
        _brightnessTries = 0;
        _brightnessOk = SetBrightness(_s.NormalPct);
        if (_brightnessOk) _brightnessTries = 99;
        SetAutoStart(_s.AutoStart);
        tray.Visible = _s.ShowTray;
        ApplyTopMost(_s.TopMost, false);
        ThreadPool.QueueUserWorkItem(delegate(object state) { ApplyPower(); });
        Program.LogNote("calibrate", "brightness=" + _s.NormalPct + "%; video idle=" +
            _s.ScreenOffMin + " min; autostart=" + _s.AutoStart + "; topmost=" + _s.TopMost +
            "; theme=" + _s.Theme + "; lang=" + _s.Lang);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing && _s.ShowTray)
        {
            e.Cancel = true;
            Hide();
            tray.ShowBalloonTip(1500, "MacBookToolkit", L10n.T("tray_min"), ToolTipIcon.Info);
            return;
        }
        RestoreBrightnessIfDimmed();
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        try
        {
            if (idleTimer != null) { idleTimer.Stop(); idleTimer.Dispose(); idleTimer = null; }
            RemoveHooks();
            if (tray != null) { tray.Visible = false; tray.Dispose(); tray = null; }
            if (tip != null) { tip.Dispose(); tip = null; }
            if (Font != null) Font.Dispose();
            if (Icon != null) Icon.Dispose();
        }
        catch { }
        base.OnFormClosed(e);
    }

    // ---------- system integration ----------
    void ApplyPower()
    {
        try
        {
            int sec = _s.ScreenOffMin * 60;
            Run("powercfg", "/setacvalueindex SCHEME_CURRENT SUB_VIDEO VIDEOIDLE " + sec.ToString());
            Run("powercfg", "/setdcvalueindex SCHEME_CURRENT SUB_VIDEO VIDEOIDLE " + sec.ToString());
            Run("powercfg", "/setactive SCHEME_CURRENT");
        }
        catch (Exception ex) { Program.LogError("apply power", ex); }
    }

    static void Run(string exe, string args)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo(exe, args);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            using (Process p = Process.Start(psi))
            {
                if (p == null) { Program.LogNote("run", exe + ": not started"); return; }
                if (!p.WaitForExit(5000)) { Program.LogNote("run", exe + " " + args + ": timeout"); return; }
                if (p.ExitCode != 0) Program.LogNote("run", exe + " " + args + ": exit " + p.ExitCode);
            }
        }
        catch (Exception ex) { Program.LogError("run " + exe, ex); }
    }

    void SetAutoStart(bool on)
    {
        try
        {
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (k == null) return;
                if (on) k.SetValue("MacBookToolkit", "\"" + Application.ExecutablePath + "\"");
                else k.DeleteValue("MacBookToolkit", false);
            }
        }
        catch (Exception ex) { Program.LogError("autostart", ex); }
    }

    static bool SetBrightness(int pct)
    {
        if (pct < 0) return false;
        if (pct > 100) pct = 100;
        try
        {
            int ok = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\wmi", "SELECT * FROM WmiMonitorBrightnessMethods"))
            using (ManagementObjectCollection monitors = searcher.Get())
            {
                foreach (ManagementObject m in monitors)
                {
                    using (m)
                    {
                        try { m.InvokeMethod("WmiSetBrightness", new object[] { (uint)0, (byte)pct }); ok++; }
                        catch (Exception ex) { Program.LogNote("brightness", "one monitor failed: " + ex.Message); }
                    }
                }
            }
            if (ok > 0) { _brightnessErrorLogged = false; return true; }
            if (!_brightnessErrorLogged) { _brightnessErrorLogged = true; Program.LogNote("brightness", "no WMI brightness control available"); }
            return false;
        }
        catch (Exception ex)
        {
            if (!_brightnessErrorLogged) { _brightnessErrorLogged = true; Program.LogError("brightness", ex); }
            return false;
        }
    }

    static uint IdleMs()
    {
        LASTINPUTINFO lii = new LASTINPUTINFO();
        lii.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
        if (!GetLastInputInfo(ref lii)) return 0;
        return (uint)(Environment.TickCount - (int)lii.dwTime);
    }

    static bool WantsDim(uint idleMs, Settings s)
    {
        return s.ScreenEnabled && idleMs >= (uint)(s.IdleSec * 1000);
    }

    void IdleTick(object sender, EventArgs e)
    {
        if (!_brightnessOk && _brightnessTries < 5)
        {
            _brightnessTries++;
            if (SetBrightness(_s.NormalPct)) { _brightnessOk = true; _dimmed = false; }
        }
        bool wantDim = WantsDim(IdleMs(), _s) && _brightnessOk;
        if (wantDim && !_dimmed)
        {
            if (SetBrightness(_s.DimPct)) _dimmed = true;
        }
        else if (!wantDim && _dimmed)
        {
            if (SetBrightness(_s.NormalPct)) _dimmed = false;
        }
    }

    static bool Excluded(int vk)
    {
        switch (vk)
        {
            case 0x08: case 0x09: case 0x0D: case 0x1B: case 0x2E:
            case 0x25: case 0x26: case 0x27: case 0x28:
            case 0x21: case 0x22: case 0x23: case 0x24:
            case 0x10: case 0x11: case 0x12:
            case 0xA0: case 0xA1: case 0xA2: case 0xA3: case 0xA4: case 0xA5:
            case 0x5B: case 0x5C: case 0x5D: case 0x2C:
                return true;
            default:
                return false;
        }
    }

    static IntPtr KbCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0 && _s != null)
            {
                KBDLLHOOKSTRUCT k = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                int vk = k.vkCode & 0xFF;
                bool injected = (k.flags & LLKHF_INJECTED) != 0;
                uint now = (uint)Environment.TickCount;
                bool isDown = (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN);
                bool isUp = (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP);

                if (!injected)
                {
                    if (isDown) { _lastKeyTick = now; _haveKeyTick = true; }
                    if (_s.KeyEnabled && !Excluded(vk))
                    {
                        if (isUp) _debounce.NoteKeyUp(vk, now);
                        else if (isDown && _debounce.ShouldDropKeyDown(vk, now, _s.DebounceUpMs, _s.DebounceDownMs))
                            return (IntPtr)1;
                    }
                }
            }
        }
        catch (Exception ex) { Program.LogError("keyboard hook", ex); }
        return CallNextHookEx(_kbHook, nCode, wParam, lParam);
    }

    static IntPtr MsCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0 && _s != null && _s.TouchEnabled && _s.BlockMs > 0 && _haveKeyTick)
            {
                int msg = wParam.ToInt32();
                bool blockable = msg == WM_MOUSEMOVE || msg == WM_LBUTTONDOWN || msg == WM_RBUTTONDOWN ||
                                 msg == WM_MBUTTONDOWN || msg == WM_XBUTTONDOWN;
                if (blockable)
                {
                    MSLLHOOKSTRUCT m = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    bool injected = (m.flags & LLMHF_INJECTED) != 0;
                    if (!injected)
                    {
                        uint dt = unchecked((uint)Environment.TickCount - _lastKeyTick);
                        if (dt < (uint)_s.BlockMs) return (IntPtr)1;
                    }
                }
            }
        }
        catch (Exception ex) { Program.LogError("mouse hook", ex); }
        return CallNextHookEx(_msHook, nCode, wParam, lParam);
    }

    void InstallHooks()
    {
        try
        {
            _kbProc = new LowLevelProc(KbCallback);
            _msProc = new LowLevelProc(MsCallback);
            IntPtr hMod = GetModuleHandle(null);
            _kbHook = SetWindowsHookEx(WH_KEYBOARD_LL, _kbProc, hMod, 0);
            _msHook = SetWindowsHookEx(WH_MOUSE_LL, _msProc, hMod, 0);
        }
        catch (Exception ex) { Program.LogError("install hooks", ex); }
        if (_kbHook == IntPtr.Zero)
        {
            Program.LogNote("install hooks", "keyboard hook failed, win32 error " + Marshal.GetLastWin32Error());
            _s.KeyEnabled = false;
            chkKey.Checked = false;
            chkKey.Enabled = false;
        }
        if (_msHook == IntPtr.Zero)
        {
            Program.LogNote("install hooks", "mouse hook failed, win32 error " + Marshal.GetLastWin32Error());
            _s.TouchEnabled = false;
            chkTouch.Checked = false;
            chkTouch.Enabled = false;
        }
    }

    void RemoveHooks()
    {
        try
        {
            if (_kbHook != IntPtr.Zero) { UnhookWindowsHookEx(_kbHook); _kbHook = IntPtr.Zero; }
            if (_msHook != IntPtr.Zero) { UnhookWindowsHookEx(_msHook); _msHook = IntPtr.Zero; }
        }
        catch { }
    }

    public static void RestoreBrightnessIfDimmed()
    {
        try
        {
            if (_dimmed && _s != null) { SetBrightness(_s.NormalPct); _dimmed = false; }
        }
        catch { }
    }
}

public class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr FindWindow(string cls, string title);
    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr h, int cmd);
    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")]
    static extern IntPtr SendMessageTimeout(IntPtr h, uint msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);
    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += delegate(object s, ThreadExceptionEventArgs e)
        {
            MainForm.RestoreBrightnessIfDimmed();
            LogError("UI thread", e.Exception);
        };
        AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e)
        {
            MainForm.RestoreBrightnessIfDimmed();
            LogError("AppDomain", e.ExceptionObject as Exception);
        };
        Ui.InitDpi();

        bool createdNew;
        using (Mutex mutex = new Mutex(true, "MacBookToolkit_SingleInstance", out createdNew))
        {
            if (!createdNew)
            {
                IntPtr existing = WaitForWindow(4000);
                if (existing != IntPtr.Zero)
                {
                    if (IsResponsive(existing))
                    {
                        ShowWindow(existing, 9);
                        SetForegroundWindow(existing);
                        return;
                    }
                    DialogResult r = MessageBox.Show(L10n.T("hung_body"), L10n.T("hung_title"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (r != DialogResult.Yes) return;
                    KillOwner(existing);
                    Thread.Sleep(700);
                }
            }
            Application.Run(new MainForm());
        }
    }

    static IntPtr WaitForWindow(int totalMs)
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < totalMs)
        {
            IntPtr h = FindWindow(null, "MacBookToolkit");
            if (h != IntPtr.Zero) return h;
            Thread.Sleep(150);
        }
        return IntPtr.Zero;
    }

    static bool IsResponsive(IntPtr hwnd)
    {
        try
        {
            IntPtr res;
            return SendMessageTimeout(hwnd, 0, IntPtr.Zero, IntPtr.Zero, 0x0002, 1200, out res) != IntPtr.Zero;
        }
        catch { return false; }
    }

    static void KillOwner(IntPtr hwnd)
    {
        try
        {
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            using (Process p = Process.GetProcessById((int)pid))
            {
                p.Kill();
                p.WaitForExit(3000);
            }
            LogNote("single instance", "terminated hung instance pid=" + pid);
        }
        catch (Exception ex) { LogError("terminate hung", ex); }
    }

    public static void LogNote(string source, string note)
    {
        WriteLog("[" + source + "] " + note);
    }

    public static void LogError(string source, Exception ex)
    {
        WriteLog("[" + source + "] " + (ex == null ? "error" : ex.ToString()));
    }

    static void WriteLog(string text)
    {
        try
        {
            string dir = Settings.Dir();
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, "errors.log"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + text + Environment.NewLine);
        }
        catch { }
    }
}
